using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Rendering;

[BurstCompile]
internal static class LODGroupDataPoolBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int FreeLODGroupData_000002F1_0024PostfixBurstDelegate(in NativeArray<int> destroyedLODGroupsID, ref NativeList<LODGroupData> lodGroupsData, ref NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash, ref NativeList<GPUInstanceIndex> freeLODGroupDataHandles);

	internal static class FreeLODGroupData_000002F1_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FreeLODGroupData_000002F1_0024PostfixBurstDelegate>(FreeLODGroupData).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(in NativeArray<int> destroyedLODGroupsID, ref NativeList<LODGroupData> lodGroupsData, ref NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash, ref NativeList<GPUInstanceIndex> freeLODGroupDataHandles)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeList<LODGroupData>, ref NativeParallelHashMap<int, GPUInstanceIndex>, ref NativeList<GPUInstanceIndex>, int>)functionPointer)(ref destroyedLODGroupsID, ref lodGroupsData, ref lodGroupDataHash, ref freeLODGroupDataHandles);
				}
			}
			return FreeLODGroupData_0024BurstManaged(in destroyedLODGroupsID, ref lodGroupsData, ref lodGroupDataHash, ref freeLODGroupDataHandles);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int AllocateOrGetLODGroupDataInstances_000002F2_0024PostfixBurstDelegate(in NativeArray<int> lodGroupsID, ref NativeList<LODGroupData> lodGroupsData, ref NativeList<LODGroupCullingData> lodGroupCullingData, ref NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash, ref NativeList<GPUInstanceIndex> freeLODGroupDataHandles, ref NativeArray<GPUInstanceIndex> lodGroupInstances);

	internal static class AllocateOrGetLODGroupDataInstances_000002F2_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<AllocateOrGetLODGroupDataInstances_000002F2_0024PostfixBurstDelegate>(AllocateOrGetLODGroupDataInstances).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(in NativeArray<int> lodGroupsID, ref NativeList<LODGroupData> lodGroupsData, ref NativeList<LODGroupCullingData> lodGroupCullingData, ref NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash, ref NativeList<GPUInstanceIndex> freeLODGroupDataHandles, ref NativeArray<GPUInstanceIndex> lodGroupInstances)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeList<LODGroupData>, ref NativeList<LODGroupCullingData>, ref NativeParallelHashMap<int, GPUInstanceIndex>, ref NativeList<GPUInstanceIndex>, ref NativeArray<GPUInstanceIndex>, int>)functionPointer)(ref lodGroupsID, ref lodGroupsData, ref lodGroupCullingData, ref lodGroupDataHash, ref freeLODGroupDataHandles, ref lodGroupInstances);
				}
			}
			return AllocateOrGetLODGroupDataInstances_0024BurstManaged(in lodGroupsID, ref lodGroupsData, ref lodGroupCullingData, ref lodGroupDataHash, ref freeLODGroupDataHandles, ref lodGroupInstances);
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EFreeLODGroupData_000002F1_0024PostfixBurstDelegate))]
	public static int FreeLODGroupData(in NativeArray<int> destroyedLODGroupsID, ref NativeList<LODGroupData> lodGroupsData, ref NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash, ref NativeList<GPUInstanceIndex> freeLODGroupDataHandles)
	{
		return FreeLODGroupData_000002F1_0024BurstDirectCall.Invoke(in destroyedLODGroupsID, ref lodGroupsData, ref lodGroupDataHash, ref freeLODGroupDataHandles);
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EAllocateOrGetLODGroupDataInstances_000002F2_0024PostfixBurstDelegate))]
	public static int AllocateOrGetLODGroupDataInstances(in NativeArray<int> lodGroupsID, ref NativeList<LODGroupData> lodGroupsData, ref NativeList<LODGroupCullingData> lodGroupCullingData, ref NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash, ref NativeList<GPUInstanceIndex> freeLODGroupDataHandles, ref NativeArray<GPUInstanceIndex> lodGroupInstances)
	{
		return AllocateOrGetLODGroupDataInstances_000002F2_0024BurstDirectCall.Invoke(in lodGroupsID, ref lodGroupsData, ref lodGroupCullingData, ref lodGroupDataHash, ref freeLODGroupDataHandles, ref lodGroupInstances);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static int FreeLODGroupData_0024BurstManaged(in NativeArray<int> destroyedLODGroupsID, ref NativeList<LODGroupData> lodGroupsData, ref NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash, ref NativeList<GPUInstanceIndex> freeLODGroupDataHandles)
	{
		int num = 0;
		foreach (int item2 in destroyedLODGroupsID)
		{
			if (lodGroupDataHash.TryGetValue(item2, out var item))
			{
				lodGroupDataHash.Remove(item2);
				freeLODGroupDataHandles.Add(in item);
				ref LODGroupData reference = ref lodGroupsData.ElementAt(item.index);
				num += reference.rendererCount;
				reference.valid = false;
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static int AllocateOrGetLODGroupDataInstances_0024BurstManaged(in NativeArray<int> lodGroupsID, ref NativeList<LODGroupData> lodGroupsData, ref NativeList<LODGroupCullingData> lodGroupCullingData, ref NativeParallelHashMap<int, GPUInstanceIndex> lodGroupDataHash, ref NativeList<GPUInstanceIndex> freeLODGroupDataHandles, ref NativeArray<GPUInstanceIndex> lodGroupInstances)
	{
		int num = freeLODGroupDataHandles.Length;
		int length = lodGroupsData.Length;
		int num2 = 0;
		for (int i = 0; i < lodGroupsID.Length; i++)
		{
			int key = lodGroupsID[i];
			if (!lodGroupDataHash.TryGetValue(key, out var item))
			{
				if (num == 0)
				{
					GPUInstanceIndex gPUInstanceIndex = default(GPUInstanceIndex);
					gPUInstanceIndex.index = length++;
					item = gPUInstanceIndex;
				}
				else
				{
					item = freeLODGroupDataHandles[--num];
				}
				lodGroupDataHash.TryAdd(key, item);
			}
			else
			{
				num2 += lodGroupsData.ElementAt(item.index).rendererCount;
			}
			lodGroupInstances[i] = item;
		}
		freeLODGroupDataHandles.ResizeUninitialized(num);
		lodGroupsData.ResizeUninitialized(length);
		lodGroupCullingData.ResizeUninitialized(length);
		return num2;
	}
}
