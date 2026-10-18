using System;
using System.Runtime.CompilerServices;
using Unity.Collections;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal struct NativePassData
{
	public FixedAttachmentArray<LoadAudit> loadAudit;

	public FixedAttachmentArray<StoreAudit> storeAudit;

	public PassBreakAudit breakAudit;

	public FixedAttachmentArray<PassFragmentData> fragments;

	public FixedAttachmentArray<NativePassAttachment> attachments;

	public int firstGraphPass;

	public int lastGraphPass;

	public int numGraphPasses;

	public int firstNativeSubPass;

	public int numNativeSubPasses;

	public int width;

	public int height;

	public int volumeDepth;

	public int samples;

	public int shadingRateImageIndex;

	public bool hasDepth;

	public bool hasFoveatedRasterization;

	public bool hasShadingRateStates;

	public ShadingRateFragmentSize shadingRateFragmentSize;

	public ShadingRateCombiner primitiveShadingRateCombiner;

	public ShadingRateCombiner fragmentShadingRateCombiner;

	public bool hasShadingRateImage => shadingRateImageIndex >= 0;

	public NativePassData(ref PassData pass, CompilerContextData ctx)
	{
		firstGraphPass = pass.passId;
		lastGraphPass = pass.passId;
		numGraphPasses = 1;
		firstNativeSubPass = -1;
		numNativeSubPasses = 0;
		fragments = default(FixedAttachmentArray<PassFragmentData>);
		attachments = default(FixedAttachmentArray<NativePassAttachment>);
		width = pass.fragmentInfoWidth;
		height = pass.fragmentInfoHeight;
		volumeDepth = pass.fragmentInfoVolumeDepth;
		samples = pass.fragmentInfoSamples;
		hasDepth = pass.fragmentInfoHasDepth;
		hasFoveatedRasterization = pass.hasFoveatedRasterization;
		loadAudit = default(FixedAttachmentArray<LoadAudit>);
		storeAudit = default(FixedAttachmentArray<StoreAudit>);
		breakAudit = new PassBreakAudit(PassBreakReason.NotOptimized, -1);
		ReadOnlySpan<PassFragmentData> readOnlySpan = pass.Fragments(ctx);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly PassFragmentData data = ref readOnlySpan[i];
			fragments.Add(in data);
		}
		readOnlySpan = pass.FragmentInputs(ctx);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly PassFragmentData data2 = ref readOnlySpan[i];
			fragments.Add(in data2);
		}
		if (pass.fragmentInfoHasShadingRateImage && !hasFoveatedRasterization)
		{
			shadingRateImageIndex = fragments.size;
			ref FixedAttachmentArray<PassFragmentData> reference = ref fragments;
			PassFragmentData data3 = pass.ShadingRateImage(ctx);
			reference.Add(in data3);
		}
		else
		{
			shadingRateImageIndex = -1;
		}
		hasShadingRateStates = pass.hasShadingRateStates && !hasFoveatedRasterization;
		shadingRateFragmentSize = pass.shadingRateFragmentSize;
		primitiveShadingRateCombiner = pass.primitiveShadingRateCombiner;
		fragmentShadingRateCombiner = pass.fragmentShadingRateCombiner;
		TryMergeNativeSubPass(ctx, ref this, ref pass);
	}

	public SubPassFlags GetSubPassFlagForMerging()
	{
		if (!hasDepth)
		{
			throw new Exception("SubPassFlag for merging can not be determined if native pass doesn't have a depth attachment");
		}
		return SubPassFlags.ReadOnlyDepth;
	}

	public void Clear()
	{
		firstGraphPass = 0;
		numGraphPasses = 0;
		attachments.Clear();
		fragments.Clear();
		loadAudit.Clear();
		storeAudit.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly bool IsValid()
	{
		return numGraphPasses > 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly ReadOnlySpan<PassData> GraphPasses(CompilerContextData ctx)
	{
		if (lastGraphPass - firstGraphPass + 1 == numGraphPasses)
		{
			return NativeListExtensions.MakeReadOnlySpan(ref ctx.passData, firstGraphPass, numGraphPasses);
		}
		NativeArray<PassData> source = new NativeArray<PassData>(numGraphPasses, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		int i = firstGraphPass;
		int num = 0;
		for (; i < lastGraphPass + 1; i++)
		{
			PassData value = ctx.passData[i];
			if (!value.culled)
			{
				source[num++] = value;
			}
		}
		return source;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public readonly void GetGraphPassNames(CompilerContextData ctx, DynamicArray<Name> dest)
	{
		ReadOnlySpan<PassData> readOnlySpan = GraphPasses(ctx);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			Name value = readOnlySpan[i].GetName(ctx);
			dest.Add(in value);
		}
	}

	public static PassBreakAudit CanMerge(CompilerContextData contextData, int activeNativePassId, int passIdToMerge)
	{
		ref PassData reference = ref contextData.passData.ElementAt(passIdToMerge);
		if (reference.type != RenderGraphPassType.Raster)
		{
			return new PassBreakAudit(PassBreakReason.NonRasterPass, passIdToMerge);
		}
		ref NativePassData reference2 = ref contextData.nativePassData.ElementAt(activeNativePassId);
		if (reference.numFragments > 0 || reference.numFragmentInputs > 0)
		{
			if (reference2.width != reference.fragmentInfoWidth || reference2.height != reference.fragmentInfoHeight || reference2.volumeDepth != reference.fragmentInfoVolumeDepth || reference2.samples != reference.fragmentInfoSamples)
			{
				return new PassBreakAudit(PassBreakReason.TargetSizeMismatch, passIdToMerge);
			}
			if (reference2.hasDepth && reference.fragmentInfoHasDepth)
			{
				ref PassFragmentData reference3 = ref contextData.fragmentData.ElementAt(reference.firstFragment);
				if (reference2.fragments[0].resource.index != reference3.resource.index)
				{
					return new PassBreakAudit(PassBreakReason.DifferentDepthTextures, passIdToMerge);
				}
			}
			if (reference2.hasFoveatedRasterization != reference.hasFoveatedRasterization)
			{
				return new PassBreakAudit(PassBreakReason.FRStateMismatch, passIdToMerge);
			}
			if (reference2.hasShadingRateImage != reference.fragmentInfoHasShadingRateImage)
			{
				return new PassBreakAudit(PassBreakReason.DifferentShadingRateImages, passIdToMerge);
			}
			if (reference2.hasShadingRateImage)
			{
				PassFragmentData passFragmentData = reference.ShadingRateImage(contextData);
				PassFragmentData passFragmentData2 = reference2.fragments[reference2.shadingRateImageIndex];
				if (passFragmentData2.resource.index != passFragmentData.resource.index)
				{
					return new PassBreakAudit(PassBreakReason.DifferentShadingRateImages, passIdToMerge);
				}
			}
			if (reference2.hasShadingRateStates != reference.hasShadingRateStates)
			{
				return new PassBreakAudit(PassBreakReason.DifferentShadingRateStates, passIdToMerge);
			}
			if (reference2.hasShadingRateStates && (reference2.shadingRateFragmentSize != reference.shadingRateFragmentSize || reference2.primitiveShadingRateCombiner != reference.primitiveShadingRateCombiner || reference2.fragmentShadingRateCombiner != reference.fragmentShadingRateCombiner))
			{
				return new PassBreakAudit(PassBreakReason.DifferentShadingRateStates, passIdToMerge);
			}
		}
		ReadOnlySpan<PassInputData> readOnlySpan = reference.Inputs(contextData);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ResourceHandle resource = readOnlySpan[i].resource;
			int writePassId = contextData.resources[resource].writePassId;
			if (writePassId >= reference2.firstGraphPass && writePassId < reference2.lastGraphPass + 1 && !reference.IsUsedAsFragment(resource, contextData))
			{
				return new PassBreakAudit(PassBreakReason.NextPassReadsTexture, passIdToMerge);
			}
		}
		FixedAttachmentArray<PassFragmentData> fixedAttachmentArray = default(FixedAttachmentArray<PassFragmentData>);
		int num = 8 - reference2.fragments.size;
		ReadOnlySpan<PassFragmentData> readOnlySpan2 = reference.Fragments(contextData);
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			ref readonly PassFragmentData reference4 = ref readOnlySpan2[i];
			bool flag = false;
			for (int j = 0; j < reference2.fragments.size; j++)
			{
				if (PassFragmentData.SameSubResource(in reference2.fragments[j], in reference4))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (num == 0)
				{
					return new PassBreakAudit(PassBreakReason.AttachmentLimitReached, passIdToMerge);
				}
				fixedAttachmentArray.Add(in reference4);
				num--;
			}
			for (int k = reference2.firstGraphPass; k <= reference2.lastGraphPass; k++)
			{
				ref PassData reference5 = ref contextData.passData.ElementAt(k);
				readOnlySpan = reference5.Inputs(contextData);
				for (int l = 0; l < readOnlySpan.Length; l++)
				{
					ref readonly PassInputData reference6 = ref readOnlySpan[l];
					if (reference6.resource.index == reference4.resource.index && !reference5.IsUsedAsFragment(reference6.resource, contextData))
					{
						return new PassBreakAudit(PassBreakReason.NextPassTargetsTexture, passIdToMerge);
					}
				}
			}
		}
		readOnlySpan2 = reference.FragmentInputs(contextData);
		for (int i = 0; i < readOnlySpan2.Length; i++)
		{
			ref readonly PassFragmentData reference7 = ref readOnlySpan2[i];
			bool flag2 = false;
			for (int m = 0; m < reference2.fragments.size; m++)
			{
				if (PassFragmentData.SameSubResource(in reference2.fragments[m], in reference7))
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				if (num == 0)
				{
					return new PassBreakAudit(PassBreakReason.AttachmentLimitReached, passIdToMerge);
				}
				fixedAttachmentArray.Add(in reference7);
				num--;
			}
		}
		if (reference2.numGraphPasses >= 8 && !CanMergeNativeSubPass(contextData, ref reference2, ref reference))
		{
			return new PassBreakAudit(PassBreakReason.SubPassLimitReached, passIdToMerge);
		}
		return new PassBreakAudit(PassBreakReason.Merged, passIdToMerge);
	}

	private static bool CanMergeNativeSubPass(CompilerContextData contextData, ref NativePassData nativePass, ref PassData passToMerge)
	{
		if (passToMerge.numFragments == 0 && passToMerge.numFragmentInputs == 0)
		{
			return true;
		}
		if (nativePass.numNativeSubPasses == 0)
		{
			return false;
		}
		ref SubPassDescriptor reference = ref contextData.nativeSubPassData.ElementAt(nativePass.firstNativeSubPass + nativePass.numNativeSubPasses - 1);
		bool fragmentInfoHasDepth = passToMerge.fragmentInfoHasDepth;
		int num = (fragmentInfoHasDepth ? (-1) : 0);
		if (passToMerge.numFragments + num != reference.colorOutputs.Length)
		{
			return false;
		}
		if (passToMerge.numFragmentInputs != reference.inputs.Length)
		{
			return false;
		}
		SubPassFlags subPassFlags = SubPassFlags.None;
		if (!fragmentInfoHasDepth && nativePass.hasDepth)
		{
			subPassFlags = nativePass.GetSubPassFlagForMerging();
		}
		ref FixedAttachmentArray<PassFragmentData> reference2 = ref nativePass.fragments;
		int num2 = 0;
		ReadOnlySpan<PassFragmentData> readOnlySpan = passToMerge.Fragments(contextData);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly PassFragmentData reference3 = ref readOnlySpan[i];
			if (fragmentInfoHasDepth && num2 == 0)
			{
				subPassFlags = ((!reference3.accessFlags.HasFlag(AccessFlags.Write)) ? SubPassFlags.ReadOnlyDepth : SubPassFlags.None);
			}
			else
			{
				int num3 = -1;
				int num4 = 0;
				while (true)
				{
					int num5 = num4;
					FixedAttachmentArray<PassFragmentData> fixedAttachmentArray = reference2;
					if (num5 >= fixedAttachmentArray.size)
					{
						break;
					}
					fixedAttachmentArray = reference2;
					if (PassFragmentData.SameSubResource(in fixedAttachmentArray[num4], in reference3))
					{
						num3 = num4;
						break;
					}
					num4++;
				}
				if (num3 < 0 || num3 != reference.colorOutputs[num2 + num])
				{
					return false;
				}
			}
			num2++;
		}
		int num6 = 0;
		readOnlySpan = passToMerge.FragmentInputs(contextData);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly PassFragmentData y = ref readOnlySpan[i];
			int num7 = -1;
			int num8 = 0;
			while (true)
			{
				int num9 = num8;
				FixedAttachmentArray<PassFragmentData> fixedAttachmentArray = reference2;
				if (num9 >= fixedAttachmentArray.size)
				{
					break;
				}
				fixedAttachmentArray = reference2;
				if (PassFragmentData.SameSubResource(in fixedAttachmentArray[num8], in y))
				{
					num7 = num8;
					break;
				}
				num8++;
			}
			if (num7 < 0 || num7 != reference.inputs[num6])
			{
				return false;
			}
			num6++;
		}
		return subPassFlags == reference.flags;
	}

	public static void TryMergeNativeSubPass(CompilerContextData contextData, ref NativePassData nativePass, ref PassData passToMerge)
	{
		ref FixedAttachmentArray<PassFragmentData> reference = ref nativePass.fragments;
		if (nativePass.numNativeSubPasses == 0 && nativePass.fragments.size > 0)
		{
			nativePass.firstNativeSubPass = contextData.nativeSubPassData.Length;
		}
		SubPassDescriptor value = default(SubPassDescriptor);
		if (passToMerge.numFragments == 0 && passToMerge.numFragmentInputs == 0)
		{
			passToMerge.nativeSubPassIndex = nativePass.numNativeSubPasses - 1;
			passToMerge.beginNativeSubpass = false;
			return;
		}
		if (!passToMerge.fragmentInfoHasDepth && nativePass.hasDepth)
		{
			value.flags = nativePass.GetSubPassFlagForMerging();
		}
		int num = 0;
		int num2 = (passToMerge.fragmentInfoHasDepth ? (-1) : 0);
		value.colorOutputs = new AttachmentIndexArray(passToMerge.numFragments + num2);
		ReadOnlySpan<PassFragmentData> readOnlySpan = passToMerge.Fragments(contextData);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly PassFragmentData reference2 = ref readOnlySpan[i];
			if (passToMerge.fragmentInfoHasDepth && num == 0)
			{
				value.flags = ((!reference2.accessFlags.HasFlag(AccessFlags.Write)) ? SubPassFlags.ReadOnlyDepth : SubPassFlags.None);
			}
			else
			{
				int value2 = -1;
				for (int j = 0; j < reference.size; j++)
				{
					if (PassFragmentData.SameSubResource(in reference[j], in reference2))
					{
						value2 = j;
						break;
					}
				}
				value.colorOutputs[num + num2] = value2;
			}
			num++;
		}
		int num3 = 0;
		value.inputs = new AttachmentIndexArray(passToMerge.numFragmentInputs);
		readOnlySpan = passToMerge.FragmentInputs(contextData);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly PassFragmentData y = ref readOnlySpan[i];
			int value3 = -1;
			for (int k = 0; k < reference.size; k++)
			{
				if (PassFragmentData.SameSubResource(in reference[k], in y))
				{
					value3 = k;
					break;
				}
			}
			value.inputs[num3] = value3;
			num3++;
		}
		if (passToMerge.fragmentInfoHasShadingRateImage)
		{
			value.flags |= SubPassFlags.UseShadingRateImage;
		}
		if (nativePass.numNativeSubPasses == 0 || !NativePassCompiler.IsSameNativeSubPass(ref value, ref contextData.nativeSubPassData.ElementAt(nativePass.firstNativeSubPass + nativePass.numNativeSubPasses - 1)))
		{
			contextData.nativeSubPassData.Add(in value);
			nativePass.numNativeSubPasses++;
			passToMerge.beginNativeSubpass = true;
		}
		else
		{
			passToMerge.beginNativeSubpass = false;
		}
		passToMerge.nativeSubPassIndex = nativePass.numNativeSubPasses - 1;
	}

	private void AddDepthAttachmentFirstDuringMerge(CompilerContextData contextData, in PassFragmentData depthAttachment)
	{
		fragments.Add(in depthAttachment);
		hasDepth = true;
		int size = fragments.size;
		if (size == 1)
		{
			return;
		}
		int num = size - 1;
		ref PassFragmentData reference = ref fragments[0];
		ref PassFragmentData reference2 = ref fragments[num];
		PassFragmentData passFragmentData = fragments[num];
		PassFragmentData passFragmentData2 = fragments[0];
		reference = passFragmentData;
		reference2 = passFragmentData2;
		SubPassFlags subPassFlagForMerging = GetSubPassFlagForMerging();
		for (int i = firstNativeSubPass; i < firstNativeSubPass + numNativeSubPasses; i++)
		{
			ref SubPassDescriptor reference3 = ref contextData.nativeSubPassData.ElementAt(i);
			reference3.flags = subPassFlagForMerging;
			for (int j = 0; j < reference3.colorOutputs.Length; j++)
			{
				if (reference3.colorOutputs[j] == 0)
				{
					reference3.colorOutputs[j] = num;
				}
			}
			for (int k = 0; k < reference3.inputs.Length; k++)
			{
				if (reference3.inputs[k] == 0)
				{
					reference3.inputs[k] = num;
				}
			}
		}
		if (hasShadingRateImage && shadingRateImageIndex == 0)
		{
			shadingRateImageIndex = num;
		}
	}

	public static PassBreakAudit TryMerge(CompilerContextData contextData, int activeNativePassId, int passIdToMerge)
	{
		PassBreakAudit result = CanMerge(contextData, activeNativePassId, passIdToMerge);
		if (result.reason != PassBreakReason.Merged)
		{
			return result;
		}
		ref PassData reference = ref contextData.passData.ElementAt(passIdToMerge);
		ref NativePassData reference2 = ref contextData.nativePassData.ElementAt(activeNativePassId);
		reference.mergeState = PassMergeState.SubPass;
		if (reference.nativePassIndex >= 0)
		{
			contextData.nativePassData.ElementAt(reference.nativePassIndex).Clear();
		}
		reference.nativePassIndex = activeNativePassId;
		reference2.numGraphPasses++;
		reference2.lastGraphPass = passIdToMerge;
		if (!reference2.hasDepth && reference.fragmentInfoHasDepth)
		{
			PassFragmentData depthAttachment = contextData.fragmentData[reference.firstFragment];
			reference2.AddDepthAttachmentFirstDuringMerge(contextData, in depthAttachment);
		}
		ReadOnlySpan<PassFragmentData> readOnlySpan = reference.Fragments(contextData);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly PassFragmentData reference3 = ref readOnlySpan[i];
			bool flag = false;
			for (int j = 0; j < reference2.fragments.size; j++)
			{
				ref PassFragmentData reference4 = ref reference2.fragments[j];
				if (PassFragmentData.SameSubResource(in reference4, in reference3))
				{
					AccessFlags accessFlags = reference3.accessFlags;
					if (reference4.accessFlags.HasFlag(AccessFlags.Discard))
					{
						accessFlags &= ~AccessFlags.Read;
					}
					reference4 = new PassFragmentData(new ResourceHandle(in reference4.resource, reference3.resource.version), reference4.accessFlags | accessFlags, reference4.mipLevel, reference4.depthSlice);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				reference2.fragments.Add(in reference3);
			}
		}
		readOnlySpan = reference.FragmentInputs(contextData);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			ref readonly PassFragmentData reference5 = ref readOnlySpan[i];
			bool flag2 = false;
			for (int k = 0; k < reference2.fragments.size; k++)
			{
				ref PassFragmentData reference6 = ref reference2.fragments[k];
				if (PassFragmentData.SameSubResource(in reference6, in reference5))
				{
					AccessFlags accessFlags2 = reference5.accessFlags;
					if (reference6.accessFlags.HasFlag(AccessFlags.Discard))
					{
						accessFlags2 &= ~AccessFlags.Read;
					}
					reference6 = new PassFragmentData(new ResourceHandle(in reference6.resource, reference5.resource.version), reference6.accessFlags | accessFlags2, reference6.mipLevel, reference6.depthSlice);
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				reference2.fragments.Add(in reference5);
			}
		}
		TryMergeNativeSubPass(contextData, ref reference2, ref reference);
		SetPassStatesForNativePass(contextData, activeNativePassId);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetPassStatesForNativePass(CompilerContextData contextData, int nativePassId)
	{
		ref NativePassData reference = ref contextData.nativePassData.ElementAt(nativePassId);
		if (reference.numGraphPasses > 1)
		{
			contextData.passData.ElementAt(reference.firstGraphPass).mergeState = PassMergeState.Begin;
			int num = reference.lastGraphPass - reference.firstGraphPass + 1;
			for (int i = 1; i < num; i++)
			{
				int index = reference.firstGraphPass + i;
				if (contextData.passData.ElementAt(index).culled)
				{
					contextData.passData.ElementAt(index).mergeState = PassMergeState.None;
				}
				else
				{
					contextData.passData.ElementAt(reference.firstGraphPass + i).mergeState = PassMergeState.SubPass;
				}
			}
			contextData.passData.ElementAt(reference.lastGraphPass).mergeState = PassMergeState.End;
		}
		else
		{
			contextData.passData.ElementAt(reference.firstGraphPass).mergeState = PassMergeState.None;
		}
	}
}
