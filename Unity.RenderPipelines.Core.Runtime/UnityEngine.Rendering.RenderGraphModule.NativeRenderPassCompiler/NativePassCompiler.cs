using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal class NativePassCompiler : IDisposable
{
	internal struct RenderGraphInputInfo
	{
		public RenderGraphResourceRegistry m_ResourcesForDebugOnly;

		public List<RenderGraphPass> m_RenderPasses;

		public string debugName;

		public bool disablePassCulling;

		public bool disablePassMerging;
	}

	internal enum NativeCompilerProfileId
	{
		NRPRGComp_PrepareNativePass,
		NRPRGComp_SetupContextData,
		NRPRGComp_BuildGraph,
		NRPRGComp_CullNodes,
		NRPRGComp_TryMergeNativePasses,
		NRPRGComp_FindResourceUsageRanges,
		NRPRGComp_DetectMemorylessResources,
		NRPRGComp_ExecuteInitializeResources,
		NRPRGComp_ExecuteBeginRenderpassCommand,
		NRPRGComp_ExecuteDestroyResources
	}

	internal RenderGraphInputInfo graph;

	internal CompilerContextData contextData;

	internal CompilerContextData defaultContextData;

	internal CommandBuffer previousCommandBuffer;

	private Stack<int> toVisitPassIds;

	private RenderGraphCompilationCache m_CompilationCache;

	internal const int k_EstimatedPassCount = 100;

	internal const int k_MaxSubpass = 8;

	private NativeList<AttachmentDescriptor> m_BeginRenderPassAttachments;

	private const int ArbitraryMaxNbMergedPasses = 16;

	private DynamicArray<Name> graphPassNamesForDebug = new DynamicArray<Name>(16);

	public NativePassCompiler(RenderGraphCompilationCache cache)
	{
		m_CompilationCache = cache;
		defaultContextData = new CompilerContextData();
		toVisitPassIds = new Stack<int>(100);
	}

	~NativePassCompiler()
	{
		Cleanup();
	}

	public void Dispose()
	{
		Cleanup();
		GC.SuppressFinalize(this);
	}

	public void Cleanup()
	{
		contextData?.Dispose();
		defaultContextData?.Dispose();
		if (m_BeginRenderPassAttachments.IsCreated)
		{
			m_BeginRenderPassAttachments.Dispose();
		}
	}

	public bool Initialize(RenderGraphResourceRegistry resources, List<RenderGraphPass> renderPasses, RenderGraphDebugParams debugParams, string debugName, bool useCompilationCaching, int graphHash, int frameIndex)
	{
		bool result = false;
		if (!useCompilationCaching)
		{
			contextData = defaultContextData;
		}
		else
		{
			result = m_CompilationCache.GetCompilationCache(graphHash, frameIndex, out contextData);
		}
		graph.m_ResourcesForDebugOnly = resources;
		graph.m_RenderPasses = renderPasses;
		graph.disablePassCulling = debugParams.disablePassCulling;
		graph.disablePassMerging = debugParams.disablePassMerging;
		graph.debugName = debugName;
		Clear(!useCompilationCaching);
		return result;
	}

	public void Compile(RenderGraphResourceRegistry resources)
	{
		SetupContextData(resources);
		BuildGraph();
		CullUnusedRenderPasses();
		TryMergeNativePasses();
		FindResourceUsageRanges();
		DetectMemoryLessResources();
		PrepareNativeRenderPasses();
	}

	public void Clear(bool clearContextData)
	{
		if (clearContextData)
		{
			contextData.Clear();
		}
		toVisitPassIds.Clear();
	}

	private void SetPassStatesForNativePass(int nativePassId)
	{
		NativePassData.SetPassStatesForNativePass(contextData, nativePassId);
	}

	private void SetupContextData(RenderGraphResourceRegistry resources)
	{
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_SetupContextData)))
		{
			contextData.Initialize(resources, 100);
		}
	}

	private void BuildGraph()
	{
		CompilerContextData compilerContextData = contextData;
		List<RenderGraphPass> renderPasses = graph.m_RenderPasses;
		compilerContextData.passData.ResizeUninitialized(renderPasses.Count);
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_BuildGraph)))
		{
			for (int i = 0; i < renderPasses.Count; i++)
			{
				RenderGraphPass pass = renderPasses[i];
				ref PassData reference = ref compilerContextData.passData.ElementAt(i);
				reference.ResetAndInitialize(in pass, i);
				DynamicArray<Name> passNames = compilerContextData.passNames;
				Name value = new Name(pass.name, computeUTF8ByteCount: true);
				passNames.Add(in value);
				if (reference.hasSideEffects)
				{
					toVisitPassIds.Push(i);
				}
				if (reference.type == RenderGraphPassType.Raster)
				{
					reference.firstFragment = compilerContextData.fragmentData.Length;
					if (pass.depthAccess.textureHandle.handle.IsValid())
					{
						reference.fragmentInfoHasDepth = true;
						if (compilerContextData.AddToFragmentList(pass.depthAccess, reference.firstFragment, reference.numFragments))
						{
							reference.AddFragment(pass.depthAccess.textureHandle.handle, compilerContextData);
						}
					}
					for (int j = 0; j < pass.colorBufferMaxIndex + 1; j++)
					{
						if (pass.colorBufferAccess[j].textureHandle.handle.IsValid() && compilerContextData.AddToFragmentList(pass.colorBufferAccess[j], reference.firstFragment, reference.numFragments))
						{
							reference.AddFragment(pass.colorBufferAccess[j].textureHandle.handle, compilerContextData);
						}
					}
					if (pass.hasShadingRateImage && pass.shadingRateAccess.textureHandle.handle.IsValid())
					{
						reference.shadingRateImageIndex = compilerContextData.fragmentData.Length;
						compilerContextData.AddToFragmentList(pass.shadingRateAccess, reference.shadingRateImageIndex, 0);
					}
					reference.firstFragmentInput = compilerContextData.fragmentData.Length;
					for (int k = 0; k < pass.fragmentInputMaxIndex + 1; k++)
					{
						if (pass.fragmentInputAccess[k].textureHandle.IsValid())
						{
							_ = ref pass.fragmentInputAccess[k];
							if (compilerContextData.AddToFragmentList(pass.fragmentInputAccess[k], reference.firstFragmentInput, reference.numFragmentInputs))
							{
								reference.AddFragmentInput(pass.fragmentInputAccess[k].textureHandle.handle, compilerContextData);
							}
						}
					}
					reference.firstRandomAccessResource = compilerContextData.randomAccessResourceData.Length;
					for (int l = 0; l < renderPasses[i].randomAccessResourceMaxIndex + 1; l++)
					{
						ref RenderGraphPass.RandomWriteResourceInfo reference2 = ref renderPasses[i].randomAccessResource[l];
						if (reference2.h.IsValid() && compilerContextData.AddToRandomAccessResourceList(reference2.h, l, reference2.preserveCounterValue, reference.firstRandomAccessResource, reference.numRandomAccessResources))
						{
							reference.AddRandomAccessResource();
						}
					}
					_ = reference.numFragments;
				}
				reference.firstInput = compilerContextData.inputData.Length;
				reference.firstOutput = compilerContextData.outputData.Length;
				for (int m = 0; m < 3; m++)
				{
					List<ResourceHandle> list = pass.resourceWriteLists[m];
					int count = list.Count;
					for (int n = 0; n < count; n++)
					{
						ResourceHandle resourceHandle = list[n];
						if (compilerContextData.UnversionedResourceData(resourceHandle).isImported && !reference.hasSideEffects)
						{
							reference.hasSideEffects = true;
							toVisitPassIds.Push(i);
						}
						compilerContextData.resources[resourceHandle].SetWritingPass(compilerContextData, resourceHandle, i);
						ref NativeList<PassOutputData> outputData = ref compilerContextData.outputData;
						PassOutputData value2 = new PassOutputData(resourceHandle);
						outputData.Add(in value2);
						reference.numOutputs++;
					}
					List<ResourceHandle> list2 = pass.resourceReadLists[m];
					int count2 = list2.Count;
					for (int num = 0; num < count2; num++)
					{
						ResourceHandle resourceHandle2 = list2[num];
						compilerContextData.resources[resourceHandle2].RegisterReadingPass(compilerContextData, resourceHandle2, i, reference.numInputs);
						ref NativeList<PassInputData> inputData = ref compilerContextData.inputData;
						PassInputData value3 = new PassInputData(resourceHandle2);
						inputData.Add(in value3);
						reference.numInputs++;
					}
					List<ResourceHandle> list3 = pass.transientResourceList[m];
					int count3 = list3.Count;
					for (int num2 = 0; num2 < count3; num2++)
					{
						ResourceHandle resourceHandle3 = list3[num2];
						compilerContextData.resources[resourceHandle3].RegisterReadingPass(compilerContextData, resourceHandle3, i, reference.numInputs);
						ref NativeList<PassInputData> inputData2 = ref compilerContextData.inputData;
						PassInputData value3 = new PassInputData(resourceHandle3);
						inputData2.Add(in value3);
						reference.numInputs++;
						compilerContextData.resources[resourceHandle3].SetWritingPass(compilerContextData, resourceHandle3, i);
						ref NativeList<PassOutputData> outputData2 = ref compilerContextData.outputData;
						PassOutputData value2 = new PassOutputData(resourceHandle3);
						outputData2.Add(in value2);
						reference.numOutputs++;
					}
				}
			}
		}
	}

	private void CullUnusedRenderPasses()
	{
		CompilerContextData compilerContextData = contextData;
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_CullNodes)))
		{
			if (graph.disablePassCulling)
			{
				return;
			}
			compilerContextData.CullAllPasses(isCulled: true);
			ReadOnlySpan<PassInputData> readOnlySpan;
			while (toVisitPassIds.Count != 0)
			{
				int index = toVisitPassIds.Pop();
				ref PassData reference = ref compilerContextData.passData.ElementAt(index);
				if (reference.culled)
				{
					readOnlySpan = reference.Inputs(compilerContextData);
					for (int i = 0; i < readOnlySpan.Length; i++)
					{
						ref readonly PassInputData reference2 = ref readOnlySpan[i];
						int writePassId = compilerContextData.resources[reference2.resource].writePassId;
						toVisitPassIds.Push(writePassId);
					}
					reference.culled = false;
				}
			}
			int length = compilerContextData.passData.Length;
			for (int j = 0; j < length; j++)
			{
				ref PassData reference3 = ref compilerContextData.passData.ElementAt(j);
				if (!reference3.culled)
				{
					continue;
				}
				ReadOnlySpan<PassOutputData> readOnlySpan2 = reference3.Outputs(compilerContextData);
				for (int i = 0; i < readOnlySpan2.Length; i++)
				{
					ResourceHandle resource = readOnlySpan2[i].resource;
					if (resource.version == compilerContextData.UnversionedResourceData(resource).latestVersionNumber)
					{
						compilerContextData.UnversionedResourceData(resource).latestVersionNumber--;
					}
				}
				readOnlySpan = reference3.Inputs(compilerContextData);
				for (int i = 0; i < readOnlySpan.Length; i++)
				{
					ResourceHandle resource2 = readOnlySpan[i].resource;
					compilerContextData.resources[resource2].RemoveReadingPass(compilerContextData, resource2, reference3.passId);
				}
			}
		}
	}

	private void TryMergeNativePasses()
	{
		CompilerContextData compilerContextData = contextData;
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_TryMergeNativePasses)))
		{
			int num = -1;
			for (int i = 0; i < compilerContextData.passData.Length; i++)
			{
				ref PassData reference = ref compilerContextData.passData.ElementAt(i);
				if (reference.culled)
				{
					continue;
				}
				if (num == -1)
				{
					if (reference.type == RenderGraphPassType.Raster)
					{
						ref NativeList<NativePassData> nativePassData = ref compilerContextData.nativePassData;
						NativePassData value = new NativePassData(ref reference, compilerContextData);
						nativePassData.Add(in value);
						reference.nativePassIndex = NativeListExtensions.LastIndex(ref compilerContextData.nativePassData);
						num = reference.nativePassIndex;
					}
					continue;
				}
				PassBreakAudit passBreakAudit = (graph.disablePassMerging ? new PassBreakAudit(PassBreakReason.PassMergingDisabled, i) : NativePassData.TryMerge(contextData, num, i));
				if (passBreakAudit.reason != PassBreakReason.Merged)
				{
					SetPassStatesForNativePass(num);
					if (passBreakAudit.reason == PassBreakReason.NonRasterPass)
					{
						num = -1;
						continue;
					}
					ref NativeList<NativePassData> nativePassData2 = ref compilerContextData.nativePassData;
					NativePassData value = new NativePassData(ref reference, compilerContextData);
					nativePassData2.Add(in value);
					reference.nativePassIndex = NativeListExtensions.LastIndex(ref compilerContextData.nativePassData);
					num = reference.nativePassIndex;
				}
			}
			if (num >= 0)
			{
				SetPassStatesForNativePass(num);
			}
		}
	}

	private void FindResourceUsageRanges()
	{
		CompilerContextData compilerContextData = contextData;
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_FindResourceUsageRanges)))
		{
			for (int i = 0; i < compilerContextData.passData.Length; i++)
			{
				ref PassData reference = ref compilerContextData.passData.ElementAt(i);
				if (reference.culled)
				{
					continue;
				}
				ReadOnlySpan<PassInputData> readOnlySpan = reference.Inputs(compilerContextData);
				for (int j = 0; j < readOnlySpan.Length; j++)
				{
					ResourceHandle resource = readOnlySpan[j].resource;
					ref ResourceUnversionedData reference2 = ref compilerContextData.UnversionedResourceData(resource);
					reference2.lastUsePassID = -1;
					if (resource.version == 0 && reference2.firstUsePassID < 0)
					{
						reference2.firstUsePassID = reference.passId;
						reference.AddFirstUse(resource, compilerContextData);
					}
					if (reference2.latestVersionNumber == resource.version)
					{
						reference2.tag++;
					}
				}
				ReadOnlySpan<PassOutputData> readOnlySpan2 = reference.Outputs(compilerContextData);
				for (int j = 0; j < readOnlySpan2.Length; j++)
				{
					ResourceHandle resource2 = readOnlySpan2[j].resource;
					ref ResourceUnversionedData reference3 = ref compilerContextData.UnversionedResourceData(resource2);
					if (resource2.version == 1 && reference3.firstUsePassID < 0)
					{
						reference3.firstUsePassID = reference.passId;
						reference.AddFirstUse(resource2, compilerContextData);
					}
					if (reference3.latestVersionNumber == resource2.version)
					{
						reference3.lastWritePassID = reference.passId;
					}
				}
			}
			for (int k = 0; k < compilerContextData.passData.Length; k++)
			{
				ref PassData reference4 = ref compilerContextData.passData.ElementAt(k);
				if (reference4.culled)
				{
					continue;
				}
				reference4.waitOnGraphicsFencePassId = -1;
				reference4.insertGraphicsFence = false;
				ReadOnlySpan<PassInputData> readOnlySpan = reference4.Inputs(compilerContextData);
				for (int j = 0; j < readOnlySpan.Length; j++)
				{
					ResourceHandle resource3 = readOnlySpan[j].resource;
					ref ResourceUnversionedData reference5 = ref compilerContextData.UnversionedResourceData(resource3);
					if (reference5.latestVersionNumber == resource3.version)
					{
						int num = reference5.tag - 1;
						if (num == 0)
						{
							reference5.lastUsePassID = reference4.passId;
							reference4.AddLastUse(resource3, compilerContextData);
						}
						reference5.tag = num;
					}
					if (reference4.waitOnGraphicsFencePassId != -1)
					{
						continue;
					}
					ref ResourceVersionedData reference6 = ref compilerContextData.VersionedResourceData(resource3);
					if (reference6.written)
					{
						ref PassData reference7 = ref compilerContextData.passData.ElementAt(reference6.writePassId);
						if (reference7.asyncCompute != reference4.asyncCompute)
						{
							reference4.waitOnGraphicsFencePassId = reference7.passId;
						}
					}
				}
				ReadOnlySpan<PassOutputData> readOnlySpan2 = reference4.Outputs(compilerContextData);
				for (int j = 0; j < readOnlySpan2.Length; j++)
				{
					ResourceHandle resource4 = readOnlySpan2[j].resource;
					ref ResourceUnversionedData reference8 = ref compilerContextData.UnversionedResourceData(resource4);
					ref ResourceVersionedData reference9 = ref compilerContextData.VersionedResourceData(resource4);
					if (reference8.latestVersionNumber == resource4.version && reference9.numReaders == 0)
					{
						reference8.lastUsePassID = reference4.passId;
						reference4.AddLastUse(resource4, compilerContextData);
					}
					int numReaders = reference9.numReaders;
					for (int l = 0; l < numReaders; l++)
					{
						int index = compilerContextData.resources.IndexReader(resource4, l);
						ref ResourceReaderData reference10 = ref compilerContextData.resources.readerData[resource4.iType].ElementAt(index);
						ref PassData reference11 = ref compilerContextData.passData.ElementAt(reference10.passId);
						if (reference4.asyncCompute != reference11.asyncCompute)
						{
							reference4.insertGraphicsFence = true;
							break;
						}
					}
				}
			}
		}
	}

	private void PrepareNativeRenderPasses()
	{
		for (int i = 0; i < contextData.nativePassData.Length; i++)
		{
			DetermineLoadStoreActions(ref contextData.nativePassData.ElementAt(i));
		}
	}

	private static bool IsGlobalTextureInPass(RenderGraphPass pass, ResourceHandle handle)
	{
		foreach (var setGlobals in pass.setGlobalsList)
		{
			if (setGlobals.Item1.handle.index == handle.index)
			{
				return true;
			}
		}
		return false;
	}

	private void DetectMemoryLessResources()
	{
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_DetectMemorylessResources)))
		{
			CompilerContextData.NativePassIterator enumerator = contextData.NativePasses.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ref readonly NativePassData current = ref enumerator.Current;
				ReadOnlySpan<PassData> readOnlySpan = current.GraphPasses(contextData);
				ReadOnlySpan<PassData> readOnlySpan2 = readOnlySpan;
				for (int i = 0; i < readOnlySpan2.Length; i++)
				{
					ref readonly PassData reference = ref readOnlySpan2[i];
					ReadOnlySpan<ResourceHandle> readOnlySpan3 = reference.FirstUsedResources(contextData);
					for (int j = 0; j < readOnlySpan3.Length; j++)
					{
						ref readonly ResourceHandle reference2 = ref readOnlySpan3[j];
						ref ResourceUnversionedData reference3 = ref contextData.UnversionedResourceData(reference2);
						if (reference2.type != 0 || reference3.isImported)
						{
							continue;
						}
						bool flag = IsGlobalTextureInPass(graph.m_RenderPasses[reference.passId], reference2);
						ReadOnlySpan<PassData> readOnlySpan4 = readOnlySpan;
						for (int k = 0; k < readOnlySpan4.Length; k++)
						{
							ref readonly PassData reference4 = ref readOnlySpan4[k];
							ReadOnlySpan<ResourceHandle> readOnlySpan5 = reference4.LastUsedResources(contextData);
							for (int l = 0; l < readOnlySpan5.Length; l++)
							{
								ref readonly ResourceHandle reference5 = ref readOnlySpan5[l];
								ref ResourceUnversionedData reference6 = ref contextData.UnversionedResourceData(reference5);
								if (reference5.type == RenderGraphResourceType.Texture && !reference6.isImported && reference2.index == reference5.index && !flag && (current.numNativeSubPasses > 1 || reference4.IsUsedAsFragment(reference2, contextData)))
								{
									reference3.memoryLess = true;
									reference6.memoryLess = true;
								}
							}
						}
					}
				}
			}
		}
	}

	internal static bool IsSameNativeSubPass(ref SubPassDescriptor a, ref SubPassDescriptor b)
	{
		if (a.flags != b.flags || a.colorOutputs.Length != b.colorOutputs.Length || a.inputs.Length != b.inputs.Length)
		{
			return false;
		}
		for (int i = 0; i < a.colorOutputs.Length; i++)
		{
			if (a.colorOutputs[i] != b.colorOutputs[i])
			{
				return false;
			}
		}
		for (int j = 0; j < a.inputs.Length; j++)
		{
			if (a.inputs[j] != b.inputs[j])
			{
				return false;
			}
		}
		return true;
	}

	private void ExecuteInitializeResource(InternalRenderGraphContext rgContext, RenderGraphResourceRegistry resources, in PassData pass)
	{
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_ExecuteInitializeResources)))
		{
			resources.forceManualClearOfResource = true;
			ReadOnlySpan<ResourceHandle> readOnlySpan2;
			if (pass.type == RenderGraphPassType.Raster && pass.nativePassIndex >= 0)
			{
				if (pass.mergeState == PassMergeState.Begin || pass.mergeState == PassMergeState.None)
				{
					ReadOnlySpan<PassData> readOnlySpan = contextData.nativePassData.ElementAt(pass.nativePassIndex).GraphPasses(contextData);
					for (int i = 0; i < readOnlySpan.Length; i++)
					{
						ref readonly PassData reference = ref readOnlySpan[i];
						readOnlySpan2 = reference.FirstUsedResources(contextData);
						for (int j = 0; j < readOnlySpan2.Length; j++)
						{
							ref readonly ResourceHandle reference2 = ref readOnlySpan2[j];
							ref ResourceUnversionedData reference3 = ref contextData.UnversionedResourceData(reference2);
							bool flag = reference.IsUsedAsFragment(reference2, contextData);
							resources.forceManualClearOfResource = !flag;
							if (!reference3.memoryLess)
							{
								if (!reference3.isImported)
								{
									resources.CreatePooledResource(rgContext, reference2.iType, reference2.index);
								}
								else if (reference3.clear && resources.forceManualClearOfResource)
								{
									resources.ClearResource(rgContext, reference2.iType, reference2.index);
								}
							}
						}
					}
				}
			}
			else
			{
				readOnlySpan2 = pass.FirstUsedResources(contextData);
				for (int i = 0; i < readOnlySpan2.Length; i++)
				{
					ref readonly ResourceHandle reference4 = ref readOnlySpan2[i];
					ref ResourceUnversionedData reference5 = ref contextData.UnversionedResourceData(reference4);
					if (!reference5.isImported)
					{
						resources.CreatePooledResource(rgContext, reference4.iType, reference4.index);
					}
					else if (reference5.clear)
					{
						resources.ClearResource(rgContext, reference4.iType, reference4.index);
					}
				}
			}
			resources.forceManualClearOfResource = true;
		}
	}

	private void DetermineLoadStoreActions(ref NativePassData nativePass)
	{
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_PrepareNativePass)))
		{
			_ = ref contextData.passData.ElementAt(nativePass.firstGraphPass);
			_ = ref contextData.passData.ElementAt(nativePass.lastGraphPass);
			if (nativePass.fragments.size <= 0)
			{
				return;
			}
			ref FixedAttachmentArray<PassFragmentData> fragments = ref nativePass.fragments;
			int num = 0;
			while (true)
			{
				int num2 = num;
				FixedAttachmentArray<PassFragmentData> fixedAttachmentArray = fragments;
				if (num2 >= fixedAttachmentArray.size)
				{
					break;
				}
				fixedAttachmentArray = fragments;
				ref PassFragmentData reference = ref fixedAttachmentArray[num];
				ResourceHandle resource = reference.resource;
				bool memoryless = false;
				int mipLevel = reference.mipLevel;
				int depthSlice = reference.depthSlice;
				RenderBufferLoadAction loadAction = RenderBufferLoadAction.DontCare;
				RenderBufferStoreAction storeAction = RenderBufferStoreAction.DontCare;
				bool flag = reference.accessFlags.HasFlag(AccessFlags.Write) && !reference.accessFlags.HasFlag(AccessFlags.Discard);
				ref ResourceUnversionedData reference2 = ref contextData.UnversionedResourceData(reference.resource);
				bool isImported = reference2.isImported;
				int lastUsePassID = reference2.lastUsePassID;
				bool flag2 = lastUsePassID >= nativePass.lastGraphPass + 1;
				if (reference.accessFlags.HasFlag(AccessFlags.Read) || flag)
				{
					if (reference2.firstUsePassID >= nativePass.firstGraphPass)
					{
						loadAction = ((!isImported) ? RenderBufferLoadAction.Clear : (reference2.clear ? RenderBufferLoadAction.Clear : RenderBufferLoadAction.Load));
					}
					else
					{
						loadAction = RenderBufferLoadAction.Load;
						if (flag2)
						{
							storeAction = RenderBufferStoreAction.Store;
						}
					}
				}
				if (reference.accessFlags.HasFlag(AccessFlags.Write))
				{
					if (nativePass.samples <= 1)
					{
						storeAction = ((!flag2) ? ((!isImported) ? RenderBufferStoreAction.DontCare : (reference2.discard ? RenderBufferStoreAction.DontCare : RenderBufferStoreAction.Store)) : RenderBufferStoreAction.Store);
					}
					else
					{
						storeAction = RenderBufferStoreAction.DontCare;
						bool flag3 = reference2.latestVersionNumber == reference.resource.version;
						bool flag4 = isImported && flag3;
						if (lastUsePassID >= nativePass.firstGraphPass + nativePass.numGraphPasses)
						{
							bool flag5 = flag4 && !reference2.discard;
							bool flag6 = flag4 && !reference2.bindMS;
							ReadOnlySpan<ResourceReaderData> readOnlySpan = contextData.Readers(reference.resource);
							for (int i = 0; i < readOnlySpan.Length; i++)
							{
								ref readonly ResourceReaderData reference3 = ref readOnlySpan[i];
								ref PassData reference4 = ref contextData.passData.ElementAt(reference3.passId);
								bool flag7 = reference4.IsUsedAsFragment(reference.resource, contextData);
								if (reference4.type == RenderGraphPassType.Unsafe)
								{
									flag5 = true;
									flag6 = !reference2.bindMS;
									break;
								}
								if (flag7)
								{
									flag5 = true;
								}
								else if (reference2.bindMS)
								{
									flag5 = true;
								}
								else
								{
									flag6 = true;
								}
							}
							if (flag5 && flag6)
							{
								storeAction = RenderBufferStoreAction.StoreAndResolve;
							}
							else if (flag6)
							{
								storeAction = RenderBufferStoreAction.Resolve;
							}
							else if (flag5)
							{
								storeAction = RenderBufferStoreAction.Store;
							}
						}
						else if (flag4)
						{
							storeAction = (reference2.bindMS ? (reference2.discard ? RenderBufferStoreAction.DontCare : RenderBufferStoreAction.Store) : ((!reference2.discard) ? RenderBufferStoreAction.StoreAndResolve : ((!nativePass.hasDepth || nativePass.attachments.size != 0) ? RenderBufferStoreAction.Resolve : RenderBufferStoreAction.DontCare)));
						}
					}
				}
				if (reference2.memoryLess)
				{
					memoryless = true;
				}
				NativePassAttachment data = new NativePassAttachment(resource, loadAction, storeAction, memoryless, mipLevel, depthSlice);
				nativePass.attachments.Add(in data);
				num++;
			}
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void ValidateNativePass(in NativePassData nativePass, int width, int height, int depth, int samples, int attachmentCount)
	{
		if (RenderGraph.enableValidityChecks)
		{
			if (nativePass.attachments.size == 0 || nativePass.numNativeSubPasses == 0)
			{
				throw new Exception("Empty render pass");
			}
			if (width == 0 || height == 0 || depth == 0 || samples == 0 || nativePass.numNativeSubPasses == 0 || attachmentCount == 0)
			{
				throw new Exception("Invalid render pass properties. One or more properties are zero.");
			}
		}
	}

	[Conditional("DEVELOPMENT_BUILD")]
	[Conditional("UNITY_EDITOR")]
	private void ValidateAttachment(in RenderTargetInfo attRenderTargetInfo, RenderGraphResourceRegistry resources, int nativePassWidth, int nativePassHeight, int nativePassMSAASamples, bool isVrs)
	{
		if (!RenderGraph.enableValidityChecks)
		{
			return;
		}
		if (isVrs)
		{
			Vector2Int allocTileSize = ShadingRateImage.GetAllocTileSize(nativePassWidth, nativePassHeight);
			if (attRenderTargetInfo.width != allocTileSize.x || attRenderTargetInfo.height != allocTileSize.y || attRenderTargetInfo.msaaSamples != 1)
			{
				throw new Exception("Low level rendergraph error: Shading rate image attachment in renderpass does not match!");
			}
		}
		else if (attRenderTargetInfo.width != nativePassWidth || attRenderTargetInfo.height != nativePassHeight || attRenderTargetInfo.msaaSamples != nativePassMSAASamples)
		{
			throw new Exception("Low level rendergraph error: Attachments in renderpass do not match!");
		}
	}

	internal unsafe void ExecuteBeginRenderPass(InternalRenderGraphContext rgContext, RenderGraphResourceRegistry resources, ref NativePassData nativePass)
	{
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_ExecuteBeginRenderpassCommand)))
		{
			ref FixedAttachmentArray<NativePassAttachment> attachments = ref nativePass.attachments;
			int size = attachments.size;
			int width = nativePass.width;
			int height = nativePass.height;
			int volumeDepth = nativePass.volumeDepth;
			int samples = nativePass.samples;
			NativeArray<SubPassDescriptor> subPasses = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<SubPassDescriptor>(contextData.nativeSubPassData.GetUnsafeReadOnlyPtr() + nativePass.firstNativeSubPass, nativePass.numNativeSubPasses, Allocator.None);
			if (nativePass.hasFoveatedRasterization)
			{
				rgContext.cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Enabled);
			}
			if (nativePass.hasShadingRateStates)
			{
				rgContext.cmd.SetShadingRateFragmentSize(nativePass.shadingRateFragmentSize);
				rgContext.cmd.SetShadingRateCombiner(ShadingRateCombinerStage.Primitive, nativePass.primitiveShadingRateCombiner);
				rgContext.cmd.SetShadingRateCombiner(ShadingRateCombinerStage.Fragment, nativePass.fragmentShadingRateCombiner);
			}
			if (!m_BeginRenderPassAttachments.IsCreated)
			{
				m_BeginRenderPassAttachments = new NativeList<AttachmentDescriptor>(8, Allocator.Persistent);
			}
			m_BeginRenderPassAttachments.Resize(size, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < size; i++)
			{
				ref readonly ResourceHandle handle = ref attachments[i].handle;
				resources.GetRenderTargetInfo(in handle, out var outInfo);
				ref AttachmentDescriptor reference = ref m_BeginRenderPassAttachments.ElementAt(i);
				reference = new AttachmentDescriptor(outInfo.format);
				if (!attachments[i].memoryless)
				{
					RTHandle texture = resources.GetTexture(handle.index);
					RenderTargetIdentifier renderTargetIdentifier = texture;
					reference.loadStoreTarget = new RenderTargetIdentifier(renderTargetIdentifier, attachments[i].mipLevel, CubemapFace.Unknown, attachments[i].depthSlice);
					if (attachments[i].storeAction == RenderBufferStoreAction.Resolve || attachments[i].storeAction == RenderBufferStoreAction.StoreAndResolve)
					{
						reference.resolveTarget = texture;
					}
				}
				reference.loadAction = attachments[i].loadAction;
				reference.storeAction = attachments[i].storeAction;
				if (attachments[i].loadAction == RenderBufferLoadAction.Clear)
				{
					reference.clearColor = Color.red;
					reference.clearDepth = 1f;
					reference.clearStencil = 0u;
					TextureDesc textureResourceDesc = resources.GetTextureResourceDesc(in handle, noThrowOnInvalidDesc: true);
					if (i == 0 && nativePass.hasDepth)
					{
						reference.clearDepth = 1f;
					}
					else
					{
						reference.clearColor = textureResourceDesc.clearColor;
					}
				}
			}
			NativeArray<AttachmentDescriptor> attachments2 = m_BeginRenderPassAttachments.AsArray();
			int depthAttachmentIndex = ((!nativePass.hasDepth) ? (-1) : 0);
			ReadOnlySpan<byte> empty = ReadOnlySpan<byte>.Empty;
			rgContext.cmd.BeginRenderPass(width, height, volumeDepth, samples, attachments2, depthAttachmentIndex, nativePass.shadingRateImageIndex, subPasses, empty);
			CommandBuffer.ThrowOnSetRenderTarget = true;
		}
	}

	private void ExecuteDestroyResource(InternalRenderGraphContext rgContext, RenderGraphResourceRegistry resources, ref PassData pass)
	{
		using (new ProfilingScope(ProfilingSampler.Get(NativeCompilerProfileId.NRPRGComp_ExecuteDestroyResources)))
		{
			rgContext.renderGraphPool.ReleaseAllTempAlloc();
			ReadOnlySpan<ResourceHandle> readOnlySpan2;
			if (pass.type == RenderGraphPassType.Raster && pass.nativePassIndex >= 0)
			{
				if (pass.mergeState != PassMergeState.End && pass.mergeState != PassMergeState.None)
				{
					return;
				}
				ReadOnlySpan<PassData> readOnlySpan = contextData.nativePassData.ElementAt(pass.nativePassIndex).GraphPasses(contextData);
				for (int i = 0; i < readOnlySpan.Length; i++)
				{
					readOnlySpan2 = readOnlySpan[i].LastUsedResources(contextData);
					for (int j = 0; j < readOnlySpan2.Length; j++)
					{
						ref readonly ResourceHandle reference = ref readOnlySpan2[j];
						ref ResourceUnversionedData reference2 = ref contextData.UnversionedResourceData(reference);
						if (!reference2.isImported && !reference2.memoryLess)
						{
							resources.ReleasePooledResource(rgContext, reference.iType, reference.index);
						}
					}
				}
				return;
			}
			readOnlySpan2 = pass.LastUsedResources(contextData);
			for (int i = 0; i < readOnlySpan2.Length; i++)
			{
				ref readonly ResourceHandle reference3 = ref readOnlySpan2[i];
				if (!contextData.UnversionedResourceData(reference3).isImported)
				{
					resources.ReleasePooledResource(rgContext, reference3.iType, reference3.index);
				}
			}
		}
	}

	internal void ExecuteSetRandomWriteTarget(in CommandBuffer cmd, RenderGraphResourceRegistry resources, int index, ResourceHandle resource, bool preserveCounterValue = true)
	{
		if (resource.type == RenderGraphResourceType.Texture)
		{
			RTHandle texture = resources.GetTexture(resource.index);
			cmd.SetRandomWriteTarget(index, texture);
			return;
		}
		if (resource.type == RenderGraphResourceType.Buffer)
		{
			GraphicsBuffer buffer = resources.GetBuffer(resource.index);
			if (preserveCounterValue)
			{
				cmd.SetRandomWriteTarget(index, buffer);
			}
			else
			{
				cmd.SetRandomWriteTarget(index, buffer, preserveCounterValue: false);
			}
			return;
		}
		throw new Exception($"Invalid resource type {resource.type}, expected texture or buffer");
	}

	internal void ExecuteGraphNode(ref InternalRenderGraphContext rgContext, RenderGraphResourceRegistry resources, RenderGraphPass pass)
	{
		rgContext.executingPass = pass;
		if (!pass.HasRenderFunc())
		{
			throw new InvalidOperationException($"RenderPass {pass.name} was not provided with an execute function.");
		}
		using (new ProfilingScope(rgContext.cmd, pass.customSampler))
		{
			pass.Execute(rgContext);
			foreach (var setGlobals in pass.setGlobalsList)
			{
				rgContext.cmd.SetGlobalTexture(setGlobals.Item2, setGlobals.Item1);
			}
		}
	}

	public void ExecuteGraph(InternalRenderGraphContext rgContext, RenderGraphResourceRegistry resources, in List<RenderGraphPass> passes)
	{
		bool flag = false;
		previousCommandBuffer = rgContext.cmd;
		rgContext.cmd.ClearRandomWriteTargets();
		for (int i = 0; i < contextData.passData.Length; i++)
		{
			ref PassData reference = ref contextData.passData.ElementAt(i);
			if (reference.culled)
			{
				continue;
			}
			bool flag2 = reference.type == RenderGraphPassType.Raster;
			ExecuteInitializeResource(rgContext, resources, in reference);
			bool flag3 = reference.type == RenderGraphPassType.Compute && reference.asyncCompute;
			if (flag3)
			{
				if (!rgContext.contextlessTesting)
				{
					rgContext.renderContext.ExecuteCommandBuffer(rgContext.cmd);
				}
				rgContext.cmd.Clear();
				CommandBuffer commandBuffer = CommandBufferPool.Get("async cmd");
				commandBuffer.SetExecutionFlags(CommandBufferExecutionFlags.AsyncCompute);
				rgContext.cmd = commandBuffer;
			}
			if (reference.waitOnGraphicsFencePassId != -1)
			{
				GraphicsFence fence = contextData.fences[reference.waitOnGraphicsFencePassId];
				rgContext.cmd.WaitOnAsyncGraphicsFence(fence);
			}
			bool flag4 = false;
			if (flag2 && reference.mergeState <= PassMergeState.Begin && reference.nativePassIndex >= 0)
			{
				ref NativePassData reference2 = ref contextData.nativePassData.ElementAt(reference.nativePassIndex);
				if (reference2.fragments.size > 0)
				{
					ExecuteBeginRenderPass(rgContext, resources, ref reference2);
					flag4 = true;
					flag = true;
				}
			}
			if (reference.mergeState >= PassMergeState.SubPass && reference.beginNativeSubpass)
			{
				if (!flag)
				{
					throw new Exception("Compiler error: Pass is marked as beginning a native sub pass but no pass is currently active.");
				}
				rgContext.cmd.NextSubPass();
			}
			if (reference.numRandomAccessResources > 0)
			{
				ReadOnlySpan<PassRandomWriteData> readOnlySpan = reference.RandomWriteTextures(contextData);
				for (int j = 0; j < readOnlySpan.Length; j++)
				{
					PassRandomWriteData passRandomWriteData = readOnlySpan[j];
					ExecuteSetRandomWriteTarget(in rgContext.cmd, resources, passRandomWriteData.index, passRandomWriteData.resource);
				}
			}
			ExecuteGraphNode(ref rgContext, resources, passes[reference.passId]);
			if (reference.numRandomAccessResources > 0)
			{
				rgContext.cmd.ClearRandomWriteTargets();
			}
			if (reference.insertGraphicsFence)
			{
				GraphicsFence value = rgContext.cmd.CreateAsyncGraphicsFence();
				contextData.fences[reference.passId] = value;
			}
			if (flag2)
			{
				if (((reference.mergeState == PassMergeState.None && flag4) || reference.mergeState == PassMergeState.End) && reference.nativePassIndex >= 0)
				{
					ref NativePassData reference3 = ref contextData.nativePassData.ElementAt(reference.nativePassIndex);
					if (reference3.fragments.size > 0)
					{
						if (!flag)
						{
							throw new Exception("Compiler error: Generated a subpass pass but no pass is currently active.");
						}
						if (reference3.hasFoveatedRasterization)
						{
							rgContext.cmd.SetFoveatedRenderingMode(FoveatedRenderingMode.Disabled);
						}
						rgContext.cmd.EndRenderPass();
						CommandBuffer.ThrowOnSetRenderTarget = false;
						flag = false;
						if (reference3.hasShadingRateStates || reference3.hasShadingRateImage)
						{
							rgContext.cmd.ResetShadingRate();
						}
					}
				}
			}
			else if (flag3)
			{
				rgContext.renderContext.ExecuteCommandBufferAsync(rgContext.cmd, ComputeQueueType.Background);
				CommandBufferPool.Release(rgContext.cmd);
				rgContext.cmd = previousCommandBuffer;
			}
			ExecuteDestroyResource(rgContext, resources, ref reference);
		}
	}

	private static RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo.AttachmentInfo MakeAttachmentInfo(CompilerContextData ctx, in NativePassData nativePass, int attachmentIndex)
	{
		NativePassAttachment attachment = nativePass.attachments[attachmentIndex];
		ResourceUnversionedData resourceUnversionedData = ctx.UnversionedResourceData(attachment.handle);
		LoadAudit loadAudit = nativePass.loadAudit[attachmentIndex];
		string text = LoadAudit.LoadReasonMessages[(int)loadAudit.reason];
		if (loadAudit.passId >= 0)
		{
			text = text.Replace("{pass}", "<b>" + ctx.passNames[loadAudit.passId].name + "</b>");
		}
		StoreAudit storeAudit = nativePass.storeAudit[attachmentIndex];
		string text2 = StoreAudit.StoreReasonMessages[(int)storeAudit.reason];
		if (storeAudit.passId >= 0)
		{
			text2 = text2.Replace("{pass}", "<b>" + ctx.passNames[storeAudit.passId].name + "</b>");
		}
		string text3 = string.Empty;
		if (storeAudit.msaaReason != 0 && storeAudit.msaaReason != StoreReason.NoMSAABuffer)
		{
			text3 = StoreAudit.StoreReasonMessages[(int)storeAudit.msaaReason];
			if (storeAudit.msaaPassId >= 0)
			{
				text3 = text3.Replace("{pass}", "<b>" + ctx.passNames[storeAudit.msaaPassId].name + "</b>");
			}
		}
		return new RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo.AttachmentInfo
		{
			resourceName = resourceUnversionedData.GetName(ctx, attachment.handle),
			attachmentIndex = attachmentIndex,
			loadReason = text,
			storeReason = text2,
			storeMsaaReason = text3,
			attachment = attachment
		};
	}

	internal static string MakePassBreakInfoMessage(CompilerContextData ctx, in NativePassData nativePass)
	{
		string text = "";
		if (nativePass.breakAudit.breakPass >= 0)
		{
			text = text + "Failed to merge " + ctx.passNames[nativePass.breakAudit.breakPass].name + " into this native pass.\n";
		}
		return text + PassBreakAudit.BreakReasonMessages[(int)nativePass.breakAudit.reason];
	}

	internal static string MakePassMergeMessage(CompilerContextData ctx, in PassData pass, in PassData prevPass, PassBreakAudit mergeResult)
	{
		string text = ((mergeResult.reason == PassBreakReason.Merged) ? "The passes are <b>compatible</b> to be merged.\n\n" : "The passes are <b>incompatible</b> to be merged.\n\n");
		string text2 = InjectSpaces(pass.GetName(ctx).name);
		string text3 = InjectSpaces(prevPass.GetName(ctx).name);
		switch (mergeResult.reason)
		{
		case PassBreakReason.Merged:
			if (pass.nativePassIndex == prevPass.nativePassIndex && pass.mergeState != PassMergeState.None)
			{
				return text + "Passes are merged.";
			}
			return text + "Passes can be merged but are not recorded consecutively.";
		case PassBreakReason.TargetSizeMismatch:
			return text + "The fragment attachments of the passes have different sizes or sample counts.\n" + $"- {text3}: {prevPass.fragmentInfoWidth}x{prevPass.fragmentInfoHeight}, {prevPass.fragmentInfoSamples} sample(s).\n" + $"- {text2}: {pass.fragmentInfoWidth}x{pass.fragmentInfoHeight}, {pass.fragmentInfoSamples} sample(s).";
		case PassBreakReason.NextPassReadsTexture:
			return text + text3 + " output is sampled by " + text2 + " as a regular texture, the pass needs to break.";
		case PassBreakReason.NextPassTargetsTexture:
			return text + text3 + " reads a texture that " + text2 + " targets to, the pass needs to break.";
		case PassBreakReason.NonRasterPass:
			return text + $"{text3} is type {prevPass.type}. Only Raster passes can be merged.";
		case PassBreakReason.DifferentDepthTextures:
			return text + text3 + " uses a different depth buffer than " + text2 + ".";
		case PassBreakReason.AttachmentLimitReached:
			return text + $"Merging the passes would use more than {8} attachments.";
		case PassBreakReason.SubPassLimitReached:
			return text + $"Merging the passes would use more than {8} native subpasses.";
		case PassBreakReason.EndOfGraph:
			return text + "The pass is the last pass in the graph.";
		case PassBreakReason.DifferentShadingRateImages:
			return text + text3 + " uses a different shading rate image than " + text2 + ".";
		case PassBreakReason.DifferentShadingRateStates:
			return text + text3 + " uses different shading rate states than " + text2 + ".";
		case PassBreakReason.PassMergingDisabled:
			return text + "The pass merging is disabled.";
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static string InjectSpaces(string camelCaseString)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < camelCaseString.Length; i++)
		{
			if (char.IsUpper(camelCaseString[i]) && i != 0 && char.IsLower(camelCaseString[i - 1]))
			{
				stringBuilder.Append(" ");
			}
			stringBuilder.Append(camelCaseString[i]);
		}
		return stringBuilder.ToString();
	}

	internal void GenerateNativeCompilerDebugData(ref RenderGraph.DebugData debugData)
	{
		ref CompilerContextData reference = ref contextData;
		debugData.isNRPCompiler = true;
		Dictionary<(RenderGraphResourceType, int), List<int>> dictionary = new Dictionary<(RenderGraphResourceType, int), List<int>>();
		Dictionary<(RenderGraphResourceType, int), List<int>> dictionary2 = new Dictionary<(RenderGraphResourceType, int), List<int>>();
		foreach (RenderGraphPass renderPass in graph.m_RenderPasses)
		{
			for (int i = 0; i < 3; i++)
			{
				int length = reference.resources.unversionedData[i].Length;
				for (int j = 0; j < length; j++)
				{
					foreach (ResourceHandle item3 in renderPass.resourceReadLists[i])
					{
						if (!renderPass.implicitReadsList.Contains(item3) && item3.type == (RenderGraphResourceType)i && item3.index == j)
						{
							(RenderGraphResourceType, int) key = ((RenderGraphResourceType)i, j);
							if (!dictionary.ContainsKey(key))
							{
								dictionary[key] = new List<int>();
							}
							dictionary[key].Add(renderPass.index);
						}
					}
					foreach (ResourceHandle item4 in renderPass.resourceWriteLists[i])
					{
						if (item4.type == (RenderGraphResourceType)i && item4.index == j)
						{
							(RenderGraphResourceType, int) key2 = ((RenderGraphResourceType)i, j);
							if (!dictionary2.ContainsKey(key2))
							{
								dictionary2[key2] = new List<int>();
							}
							dictionary2[key2].Add(renderPass.index);
						}
					}
					foreach (ResourceHandle item5 in renderPass.transientResourceList[i])
					{
						if (item5.type == (RenderGraphResourceType)i && item5.index == j)
						{
							(RenderGraphResourceType, int) key3 = ((RenderGraphResourceType)i, j);
							if (!dictionary.ContainsKey(key3))
							{
								dictionary[key3] = new List<int>();
							}
							dictionary[key3].Add(renderPass.index);
							if (!dictionary2.ContainsKey(key3))
							{
								dictionary2[key3] = new List<int>();
							}
							dictionary2[key3].Add(renderPass.index);
						}
					}
				}
			}
		}
		for (int k = 0; k < 3; k++)
		{
			int length2 = reference.resources.unversionedData[k].Length;
			for (int l = 0; l < length2; l++)
			{
				ref ResourceUnversionedData reference2 = ref reference.resources.unversionedData[k].ElementAt(l);
				RenderGraph.DebugData.ResourceData item = default(RenderGraph.DebugData.ResourceData);
				RenderGraphResourceType renderGraphResourceType = (RenderGraphResourceType)k;
				bool flag = l == 0;
				if (!flag)
				{
					string name = reference.resources.resourceNames[k][l].name;
					item.name = ((!string.IsNullOrEmpty(name)) ? name : "(unnamed)");
					item.imported = reference2.isImported;
				}
				else
				{
					item.name = "<null>";
					item.imported = true;
				}
				RenderTargetInfo outInfo = default(RenderTargetInfo);
				if (renderGraphResourceType == RenderGraphResourceType.Texture && !flag)
				{
					ResourceHandle res = new ResourceHandle(l, renderGraphResourceType, shared: false);
					try
					{
						graph.m_ResourcesForDebugOnly.GetRenderTargetInfo(in res, out outInfo);
					}
					catch (Exception)
					{
					}
				}
				item.creationPassIndex = reference2.firstUsePassID;
				item.releasePassIndex = reference2.lastUsePassID;
				item.textureData = new RenderGraph.DebugData.TextureResourceData();
				item.textureData.width = reference2.width;
				item.textureData.height = reference2.height;
				item.textureData.depth = reference2.volumeDepth;
				item.textureData.samples = reference2.msaaSamples;
				item.textureData.format = outInfo.format;
				item.textureData.bindMS = reference2.bindMS;
				item.textureData.clearBuffer = reference2.clear;
				item.memoryless = reference2.memoryLess;
				item.consumerList = new List<int>();
				item.producerList = new List<int>();
				if (dictionary.ContainsKey(((RenderGraphResourceType)k, l)))
				{
					item.consumerList = dictionary[((RenderGraphResourceType)k, l)];
				}
				if (dictionary2.ContainsKey(((RenderGraphResourceType)k, l)))
				{
					item.producerList = dictionary2[((RenderGraphResourceType)k, l)];
				}
				debugData.resourceLists[k].Add(item);
			}
		}
		for (int m = 0; m < reference.passData.Length; m++)
		{
			RenderGraphPass renderGraphPass = graph.m_RenderPasses[m];
			ref PassData reference3 = ref reference.passData.ElementAt(m);
			string name2 = InjectSpaces(reference3.GetName(reference).name);
			RenderGraph.DebugData.PassData item2 = default(RenderGraph.DebugData.PassData);
			item2.name = name2;
			item2.type = reference3.type;
			item2.culled = reference3.culled;
			item2.async = reference3.asyncCompute;
			item2.nativeSubPassIndex = reference3.nativeSubPassIndex;
			item2.generateDebugData = renderGraphPass.generateDebugData;
			item2.resourceReadLists = new List<int>[3];
			item2.resourceWriteLists = new List<int>[3];
			RenderGraph.DebugData.s_PassScriptMetadata.TryGetValue(renderGraphPass, out item2.scriptInfo);
			item2.syncFromPassIndex = -1;
			item2.syncToPassIndex = -1;
			item2.nrpInfo = new RenderGraph.DebugData.PassData.NRPInfo();
			item2.nrpInfo.width = reference3.fragmentInfoWidth;
			item2.nrpInfo.height = reference3.fragmentInfoHeight;
			item2.nrpInfo.volumeDepth = reference3.fragmentInfoVolumeDepth;
			item2.nrpInfo.samples = reference3.fragmentInfoSamples;
			item2.nrpInfo.hasDepth = reference3.fragmentInfoHasDepth;
			foreach (var setGlobals in renderGraphPass.setGlobalsList)
			{
				item2.nrpInfo.setGlobals.Add(setGlobals.Item1.handle.index);
			}
			for (int n = 0; n < 3; n++)
			{
				item2.resourceReadLists[n] = new List<int>();
				item2.resourceWriteLists[n] = new List<int>();
				foreach (ResourceHandle item6 in renderGraphPass.resourceReadLists[n])
				{
					if (!renderGraphPass.implicitReadsList.Contains(item6))
					{
						item2.resourceReadLists[n].Add(item6.index);
					}
				}
				foreach (ResourceHandle item7 in renderGraphPass.resourceWriteLists[n])
				{
					item2.resourceWriteLists[n].Add(item7.index);
				}
			}
			ReadOnlySpan<PassFragmentData> readOnlySpan = reference3.FragmentInputs(reference);
			for (int num = 0; num < readOnlySpan.Length; num++)
			{
				PassFragmentData passFragmentData = readOnlySpan[num];
				item2.nrpInfo.textureFBFetchList.Add(passFragmentData.resource.index);
			}
			debugData.passList.Add(item2);
		}
		CompilerContextData.NativePassIterator enumerator4 = reference.NativePasses.GetEnumerator();
		while (enumerator4.MoveNext())
		{
			ref readonly NativePassData current8 = ref enumerator4.Current;
			List<int> list = new List<int>();
			for (int num2 = current8.firstGraphPass; num2 < current8.lastGraphPass + 1; num2++)
			{
				list.Add(num2);
			}
			if (current8.numGraphPasses > 0)
			{
				RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo nativeRenderPassInfo = new RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo();
				nativeRenderPassInfo.passBreakReasoning = MakePassBreakInfoMessage(reference, in current8);
				nativeRenderPassInfo.attachmentInfos = new List<RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo.AttachmentInfo>();
				for (int num3 = 0; num3 < current8.attachments.size; num3++)
				{
					nativeRenderPassInfo.attachmentInfos.Add(MakeAttachmentInfo(reference, in current8, num3));
				}
				nativeRenderPassInfo.passCompatibility = new Dictionary<int, RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo.PassCompatibilityInfo>();
				nativeRenderPassInfo.mergedPassIds = list;
				for (int num4 = 0; num4 < list.Count; num4++)
				{
					int index = list[num4];
					RenderGraph.DebugData.PassData value = debugData.passList[index];
					value.nrpInfo.nativePassInfo = nativeRenderPassInfo;
					debugData.passList[index] = value;
				}
			}
		}
		for (int num5 = 0; num5 < reference.passData.Length; num5++)
		{
			ref PassData reference4 = ref reference.passData.ElementAt(num5);
			RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo nativePassInfo = debugData.passList[reference4.passId].nrpInfo.nativePassInfo;
			if (nativePassInfo == null)
			{
				continue;
			}
			ReadOnlySpan<PassInputData> readOnlySpan2 = reference4.Inputs(reference);
			for (int num = 0; num < readOnlySpan2.Length; num++)
			{
				ref readonly PassInputData reference5 = ref readOnlySpan2[num];
				ref ResourceVersionedData reference6 = ref reference.VersionedResourceData(reference5.resource);
				if (reference6.written)
				{
					PassData prevPass = reference.passData[reference6.writePassId];
					PassBreakAudit mergeResult = ((prevPass.nativePassIndex >= 0) ? NativePassData.CanMerge(reference, prevPass.nativePassIndex, reference4.passId) : new PassBreakAudit(PassBreakReason.NonRasterPass, reference4.passId));
					string message = "This pass writes to a resource that is read by the currently selected pass.\n\n" + MakePassMergeMessage(reference, in reference4, in prevPass, mergeResult);
					nativePassInfo.passCompatibility.TryAdd(prevPass.passId, new RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo.PassCompatibilityInfo
					{
						message = message,
						isCompatible = (mergeResult.reason == PassBreakReason.Merged)
					});
				}
			}
			if (reference4.nativePassIndex < 0)
			{
				continue;
			}
			ReadOnlySpan<PassOutputData> readOnlySpan3 = reference4.Outputs(reference);
			for (int num = 0; num < readOnlySpan3.Length; num++)
			{
				ref readonly PassOutputData reference7 = ref readOnlySpan3[num];
				if (reference.UnversionedResourceData(reference7.resource).lastUsePassID != reference4.passId)
				{
					int numReaders = reference.VersionedResourceData(reference7.resource).numReaders;
					for (int num6 = 0; num6 < numReaders; num6++)
					{
						int index2 = reference.resources.IndexReader(reference7.resource, num6);
						ref ResourceReaderData reference8 = ref reference.resources.readerData[reference7.resource.iType].ElementAt(index2);
						PassData pass = reference.passData[reference8.passId];
						PassBreakAudit mergeResult2 = NativePassData.CanMerge(reference, reference4.nativePassIndex, pass.passId);
						string message2 = "This pass reads a resource that is written to by the currently selected pass.\n\n" + MakePassMergeMessage(reference, in pass, in reference4, mergeResult2);
						nativePassInfo.passCompatibility.TryAdd(pass.passId, new RenderGraph.DebugData.PassData.NRPInfo.NativeRenderPassInfo.PassCompatibilityInfo
						{
							message = message2,
							isCompatible = (mergeResult2.reason == PassBreakReason.Merged)
						});
					}
				}
			}
		}
	}
}
