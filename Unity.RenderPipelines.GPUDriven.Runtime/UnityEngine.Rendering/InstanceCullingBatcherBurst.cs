using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

[BurstCompile]
internal static class InstanceCullingBatcherBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void RemoveDrawInstanceIndices_00000188_0024PostfixBurstDelegate(in NativeArray<int> drawInstanceIndices, ref NativeList<DrawInstance> drawInstances, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawRange> drawRanges, ref NativeList<DrawBatch> drawBatches);

	internal static class RemoveDrawInstanceIndices_00000188_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<RemoveDrawInstanceIndices_00000188_0024PostfixBurstDelegate>(RemoveDrawInstanceIndices).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<int> drawInstanceIndices, ref NativeList<DrawInstance> drawInstances, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawRange> drawRanges, ref NativeList<DrawBatch> drawBatches)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeList<DrawInstance>, ref NativeParallelHashMap<RangeKey, int>, ref NativeParallelHashMap<DrawKey, int>, ref NativeList<DrawRange>, ref NativeList<DrawBatch>, void>)functionPointer)(ref drawInstanceIndices, ref drawInstances, ref rangeHash, ref batchHash, ref drawRanges, ref drawBatches);
					return;
				}
			}
			RemoveDrawInstanceIndices_0024BurstManaged(in drawInstanceIndices, ref drawInstances, ref rangeHash, ref batchHash, ref drawRanges, ref drawBatches);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void CreateDrawBatches_0000018C_0024PostfixBurstDelegate(bool implicitInstanceIndices, in NativeArray<InstanceHandle> instances, in GPUDrivenRendererGroupData rendererData, in NativeParallelHashMap<int, BatchMeshID> batchMeshHash, in NativeParallelHashMap<int, BatchMaterialID> batchMaterialHash, in NativeParallelHashMap<int, GPUDrivenPackedMaterialData> packedMaterialDataHash, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeList<DrawRange> drawRanges, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawBatch> drawBatches, ref NativeList<DrawInstance> drawInstances);

	internal static class CreateDrawBatches_0000018C_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<CreateDrawBatches_0000018C_0024PostfixBurstDelegate>(CreateDrawBatches).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(bool implicitInstanceIndices, in NativeArray<InstanceHandle> instances, in GPUDrivenRendererGroupData rendererData, in NativeParallelHashMap<int, BatchMeshID> batchMeshHash, in NativeParallelHashMap<int, BatchMaterialID> batchMaterialHash, in NativeParallelHashMap<int, GPUDrivenPackedMaterialData> packedMaterialDataHash, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeList<DrawRange> drawRanges, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawBatch> drawBatches, ref NativeList<DrawInstance> drawInstances)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<bool, ref NativeArray<InstanceHandle>, ref GPUDrivenRendererGroupData, ref NativeParallelHashMap<int, BatchMeshID>, ref NativeParallelHashMap<int, BatchMaterialID>, ref NativeParallelHashMap<int, GPUDrivenPackedMaterialData>, ref NativeParallelHashMap<RangeKey, int>, ref NativeList<DrawRange>, ref NativeParallelHashMap<DrawKey, int>, ref NativeList<DrawBatch>, ref NativeList<DrawInstance>, void>)functionPointer)(implicitInstanceIndices, ref instances, ref rendererData, ref batchMeshHash, ref batchMaterialHash, ref packedMaterialDataHash, ref rangeHash, ref drawRanges, ref batchHash, ref drawBatches, ref drawInstances);
					return;
				}
			}
			CreateDrawBatches_0024BurstManaged(implicitInstanceIndices, in instances, in rendererData, in batchMeshHash, in batchMaterialHash, in packedMaterialDataHash, ref rangeHash, ref drawRanges, ref batchHash, ref drawBatches, ref drawInstances);
		}
	}

	private static void RemoveDrawRange(in RangeKey key, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeList<DrawRange> drawRanges)
	{
		int num = rangeHash[key];
		rangeHash[drawRanges.ElementAt(drawRanges.Length - 1).key] = num;
		rangeHash.Remove(key);
		drawRanges.RemoveAtSwapBack(num);
	}

	private static void RemoveDrawBatch(in DrawKey key, ref NativeList<DrawRange> drawRanges, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawBatch> drawBatches)
	{
		int num = batchHash[key];
		int index = rangeHash[key.range];
		ref DrawRange reference = ref drawRanges.ElementAt(index);
		if (--reference.drawCount == 0)
		{
			RemoveDrawRange(in reference.key, ref rangeHash, ref drawRanges);
		}
		batchHash[drawBatches.ElementAt(drawBatches.Length - 1).key] = num;
		batchHash.Remove(key);
		drawBatches.RemoveAtSwapBack(num);
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002ERemoveDrawInstanceIndices_00000188_0024PostfixBurstDelegate))]
	public static void RemoveDrawInstanceIndices(in NativeArray<int> drawInstanceIndices, ref NativeList<DrawInstance> drawInstances, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawRange> drawRanges, ref NativeList<DrawBatch> drawBatches)
	{
		RemoveDrawInstanceIndices_00000188_0024BurstDirectCall.Invoke(in drawInstanceIndices, ref drawInstances, ref rangeHash, ref batchHash, ref drawRanges, ref drawBatches);
	}

	private static ref DrawRange EditDrawRange(in RangeKey key, NativeParallelHashMap<RangeKey, int> rangeHash, NativeList<DrawRange> drawRanges)
	{
		if (!rangeHash.TryGetValue(key, out var item))
		{
			DrawRange drawRange = default(DrawRange);
			drawRange.key = key;
			drawRange.drawCount = 0;
			drawRange.drawOffset = 0;
			DrawRange value = drawRange;
			item = drawRanges.Length;
			rangeHash.Add(key, item);
			drawRanges.Add(in value);
		}
		return ref drawRanges.ElementAt(item);
	}

	private static ref DrawBatch EditDrawBatch(in DrawKey key, in SubMeshDescriptor subMeshDescriptor, NativeParallelHashMap<DrawKey, int> batchHash, NativeList<DrawBatch> drawBatches)
	{
		MeshProceduralInfo procInfo = default(MeshProceduralInfo);
		procInfo.topology = subMeshDescriptor.topology;
		procInfo.baseVertex = (uint)subMeshDescriptor.baseVertex;
		procInfo.firstIndex = (uint)subMeshDescriptor.indexStart;
		procInfo.indexCount = (uint)subMeshDescriptor.indexCount;
		if (!batchHash.TryGetValue(key, out var item))
		{
			DrawBatch drawBatch = default(DrawBatch);
			drawBatch.key = key;
			drawBatch.instanceCount = 0;
			drawBatch.instanceOffset = 0;
			drawBatch.procInfo = procInfo;
			DrawBatch value = drawBatch;
			item = drawBatches.Length;
			batchHash.Add(key, item);
			drawBatches.Add(in value);
		}
		return ref drawBatches.ElementAt(item);
	}

	private static void ProcessRenderer(int i, bool implicitInstanceIndices, in GPUDrivenRendererGroupData rendererData, NativeParallelHashMap<int, BatchMeshID> batchMeshHash, NativeParallelHashMap<int, GPUDrivenPackedMaterialData> packedMaterialDataHash, NativeParallelHashMap<int, BatchMaterialID> batchMaterialHash, NativeArray<InstanceHandle> instances, NativeList<DrawInstance> drawInstances, NativeParallelHashMap<RangeKey, int> rangeHash, NativeList<DrawRange> drawRanges, NativeParallelHashMap<DrawKey, int> batchHash, NativeList<DrawBatch> drawBatches)
	{
		int index = rendererData.meshIndex[i];
		int key = rendererData.meshID[index];
		GPUDrivenMeshLodInfo gPUDrivenMeshLodInfo = rendererData.meshLodInfo[index];
		short num = rendererData.subMeshCount[index];
		int num2 = rendererData.subMeshDescOffset[index];
		BatchMeshID meshID = batchMeshHash[key];
		int num3 = rendererData.rendererGroupID[i];
		short num4 = rendererData.subMeshStartIndex[i];
		int num5 = rendererData.gameObjectLayer[i];
		uint renderingLayerMask = rendererData.renderingLayerMask[i];
		int num6 = rendererData.materialsOffset[i];
		short num7 = rendererData.materialsCount[i];
		int num8 = rendererData.lightmapIndex[i];
		GPUDrivenPackedRendererData gPUDrivenPackedRendererData = rendererData.packedRendererData[i];
		int rendererPriority = rendererData.rendererPriority[i];
		int num9;
		int num10;
		if (implicitInstanceIndices)
		{
			num9 = 1;
			num10 = i;
		}
		else
		{
			num9 = rendererData.instancesCount[i];
			num10 = rendererData.instancesOffset[i];
		}
		if (num9 == 0)
		{
			return;
		}
		InstanceComponentGroup instanceComponentGroup = InstanceComponentGroup.Default;
		if (gPUDrivenPackedRendererData.hasTree)
		{
			instanceComponentGroup |= InstanceComponentGroup.Wind;
		}
		if ((num8 & 0xFFFF) >= 65534)
		{
			if (gPUDrivenPackedRendererData.lightProbeUsage == LightProbeUsage.BlendProbes)
			{
				instanceComponentGroup |= InstanceComponentGroup.LightProbe;
			}
		}
		else
		{
			instanceComponentGroup |= InstanceComponentGroup.Lightmap;
		}
		Span<GPUDrivenPackedMaterialData> span = stackalloc GPUDrivenPackedMaterialData[(int)num7];
		bool flag = true;
		for (int j = 0; j < num7; j++)
		{
			if (j >= num)
			{
				Debug.LogWarning("Material count in the shared material list is higher than sub mesh count for the mesh. Object may be corrupted.");
				continue;
			}
			int index2 = rendererData.materialIndex[num6 + j];
			GPUDrivenPackedMaterialData item;
			if (rendererData.packedMaterialData.Length > 0)
			{
				item = rendererData.packedMaterialData[index2];
			}
			else
			{
				int key2 = rendererData.materialID[index2];
				packedMaterialDataHash.TryGetValue(key2, out item);
			}
			flag &= item.isIndirectSupported;
			span[j] = item;
		}
		RangeKey rangeKey = default(RangeKey);
		rangeKey.layer = (byte)num5;
		rangeKey.renderingLayerMask = renderingLayerMask;
		rangeKey.motionMode = gPUDrivenPackedRendererData.motionVecGenMode;
		rangeKey.shadowCastingMode = gPUDrivenPackedRendererData.shadowCastingMode;
		rangeKey.staticShadowCaster = gPUDrivenPackedRendererData.staticShadowCaster;
		rangeKey.rendererPriority = rendererPriority;
		rangeKey.supportsIndirect = flag;
		RangeKey key3 = rangeKey;
		ref DrawRange reference = ref EditDrawRange(in key3, rangeHash, drawRanges);
		for (int k = 0; k < num7; k++)
		{
			if (k >= num)
			{
				Debug.LogWarning("Material count in the shared material list is higher than sub mesh count for the mesh. Object may be corrupted.");
				continue;
			}
			int index3 = rendererData.materialIndex[num6 + k];
			int num11 = rendererData.materialID[index3];
			GPUDrivenPackedMaterialData gPUDrivenPackedMaterialData = span[k];
			if (num11 == 0)
			{
				Debug.LogWarning("Material in the shared materials list is null. Object will be partially rendered.");
				continue;
			}
			batchMaterialHash.TryGetValue(num11, out var item2);
			BatchDrawCommandFlags batchDrawCommandFlags = BatchDrawCommandFlags.LODCrossFadeValuePacked;
			batchDrawCommandFlags |= BatchDrawCommandFlags.UseLegacyLightmapsKeyword;
			if (gPUDrivenPackedMaterialData.isMotionVectorsPassEnabled)
			{
				batchDrawCommandFlags |= BatchDrawCommandFlags.HasMotion;
			}
			if (gPUDrivenPackedMaterialData.isTransparent)
			{
				batchDrawCommandFlags |= BatchDrawCommandFlags.HasSortingPosition;
			}
			if (gPUDrivenPackedMaterialData.supportsCrossFade)
			{
				batchDrawCommandFlags |= BatchDrawCommandFlags.LODCrossFadeKeyword;
			}
			int num12 = math.max(gPUDrivenMeshLodInfo.levelCount, 1);
			for (int l = 0; l < num12; l++)
			{
				int num13 = num4 + k;
				SubMeshDescriptor subMeshDescriptor = rendererData.subMeshDesc[num2 + num13 * num12 + l];
				DrawKey drawKey = default(DrawKey);
				drawKey.materialID = item2;
				drawKey.meshID = meshID;
				drawKey.submeshIndex = num13;
				drawKey.activeMeshLod = (gPUDrivenMeshLodInfo.lodSelectionActive ? l : (-1));
				drawKey.flags = batchDrawCommandFlags;
				drawKey.transparentInstanceId = (gPUDrivenPackedMaterialData.isTransparent ? num3 : 0);
				drawKey.range = key3;
				drawKey.overridenComponents = (uint)instanceComponentGroup;
				drawKey.lightmapIndex = num8;
				DrawKey key4 = drawKey;
				ref DrawBatch reference2 = ref EditDrawBatch(in key4, in subMeshDescriptor, batchHash, drawBatches);
				if (reference2.instanceCount == 0)
				{
					reference.drawCount++;
				}
				reference2.instanceCount += num9;
				for (int m = 0; m < num9; m++)
				{
					int index4 = num10 + m;
					InstanceHandle instanceHandle = instances[index4];
					DrawInstance value = new DrawInstance
					{
						key = key4,
						instanceIndex = instanceHandle.index
					};
					drawInstances.Add(in value);
				}
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002ECreateDrawBatches_0000018C_0024PostfixBurstDelegate))]
	public static void CreateDrawBatches(bool implicitInstanceIndices, in NativeArray<InstanceHandle> instances, in GPUDrivenRendererGroupData rendererData, in NativeParallelHashMap<int, BatchMeshID> batchMeshHash, in NativeParallelHashMap<int, BatchMaterialID> batchMaterialHash, in NativeParallelHashMap<int, GPUDrivenPackedMaterialData> packedMaterialDataHash, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeList<DrawRange> drawRanges, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawBatch> drawBatches, ref NativeList<DrawInstance> drawInstances)
	{
		CreateDrawBatches_0000018C_0024BurstDirectCall.Invoke(implicitInstanceIndices, in instances, in rendererData, in batchMeshHash, in batchMaterialHash, in packedMaterialDataHash, ref rangeHash, ref drawRanges, ref batchHash, ref drawBatches, ref drawInstances);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal unsafe static void RemoveDrawInstanceIndices_0024BurstManaged(in NativeArray<int> drawInstanceIndices, ref NativeList<DrawInstance> drawInstances, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawRange> drawRanges, ref NativeList<DrawBatch> drawBatches)
	{
		DrawInstance* unsafePtr = drawInstances.GetUnsafePtr();
		int num = drawInstances.Length - 1;
		for (int num2 = drawInstanceIndices.Length - 1; num2 >= 0; num2--)
		{
			int num3 = drawInstanceIndices[num2];
			DrawInstance* ptr = unsafePtr + num3;
			int index = batchHash[ptr->key];
			ref DrawBatch reference = ref drawBatches.ElementAt(index);
			if (--reference.instanceCount == 0)
			{
				RemoveDrawBatch(in reference.key, ref drawRanges, ref rangeHash, ref batchHash, ref drawBatches);
			}
			UnsafeUtility.MemCpy(ptr, unsafePtr + num--, sizeof(DrawInstance));
		}
		drawInstances.ResizeUninitialized(num + 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static void CreateDrawBatches_0024BurstManaged(bool implicitInstanceIndices, in NativeArray<InstanceHandle> instances, in GPUDrivenRendererGroupData rendererData, in NativeParallelHashMap<int, BatchMeshID> batchMeshHash, in NativeParallelHashMap<int, BatchMaterialID> batchMaterialHash, in NativeParallelHashMap<int, GPUDrivenPackedMaterialData> packedMaterialDataHash, ref NativeParallelHashMap<RangeKey, int> rangeHash, ref NativeList<DrawRange> drawRanges, ref NativeParallelHashMap<DrawKey, int> batchHash, ref NativeList<DrawBatch> drawBatches, ref NativeList<DrawInstance> drawInstances)
	{
		for (int i = 0; i < rendererData.rendererGroupID.Length; i++)
		{
			ProcessRenderer(i, implicitInstanceIndices, in rendererData, batchMeshHash, packedMaterialDataHash, batchMaterialHash, instances, drawInstances, rangeHash, drawRanges, batchHash, drawBatches);
		}
	}
}
