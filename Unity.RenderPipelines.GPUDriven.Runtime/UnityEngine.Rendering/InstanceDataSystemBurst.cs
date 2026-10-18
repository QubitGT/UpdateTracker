using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Rendering;

[BurstCompile]
internal static class InstanceDataSystemBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ReallocateInstances_000002A0_0024PostfixBurstDelegate(bool implicitInstanceIndices, in NativeArray<int> rendererGroupIDs, in NativeArray<GPUDrivenPackedRendererData> packedRendererData, in NativeArray<int> instanceOffsets, in NativeArray<int> instanceCounts, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeArray<InstanceHandle> instances, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash);

	internal static class ReallocateInstances_000002A0_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ReallocateInstances_000002A0_0024PostfixBurstDelegate>(ReallocateInstances).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(bool implicitInstanceIndices, in NativeArray<int> rendererGroupIDs, in NativeArray<GPUDrivenPackedRendererData> packedRendererData, in NativeArray<int> instanceOffsets, in NativeArray<int> instanceCounts, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeArray<InstanceHandle> instances, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<bool, ref NativeArray<int>, ref NativeArray<GPUDrivenPackedRendererData>, ref NativeArray<int>, ref NativeArray<int>, ref InstanceAllocators, ref CPUInstanceData, ref CPUPerCameraInstanceData, ref CPUSharedInstanceData, ref NativeArray<InstanceHandle>, ref NativeParallelMultiHashMap<int, InstanceHandle>, void>)functionPointer)(implicitInstanceIndices, ref rendererGroupIDs, ref packedRendererData, ref instanceOffsets, ref instanceCounts, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref instances, ref rendererGroupInstanceMultiHash);
					return;
				}
			}
			ReallocateInstances_0024BurstManaged(implicitInstanceIndices, in rendererGroupIDs, in packedRendererData, in instanceOffsets, in instanceCounts, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref instances, ref rendererGroupInstanceMultiHash);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void FreeRendererGroupInstances_000002A1_0024PostfixBurstDelegate(in NativeArray<int>.ReadOnly rendererGroupsID, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash);

	internal static class FreeRendererGroupInstances_000002A1_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FreeRendererGroupInstances_000002A1_0024PostfixBurstDelegate>(FreeRendererGroupInstances).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<int>.ReadOnly rendererGroupsID, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<int>.ReadOnly, ref InstanceAllocators, ref CPUInstanceData, ref CPUPerCameraInstanceData, ref CPUSharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle>, void>)functionPointer)(ref rendererGroupsID, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref rendererGroupInstanceMultiHash);
					return;
				}
			}
			FreeRendererGroupInstances_0024BurstManaged(in rendererGroupsID, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref rendererGroupInstanceMultiHash);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void FreeInstances_000002A2_0024PostfixBurstDelegate(in NativeArray<InstanceHandle>.ReadOnly instances, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash);

	internal static class FreeInstances_000002A2_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FreeInstances_000002A2_0024PostfixBurstDelegate>(FreeInstances).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<InstanceHandle>.ReadOnly instances, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<InstanceHandle>.ReadOnly, ref InstanceAllocators, ref CPUInstanceData, ref CPUPerCameraInstanceData, ref CPUSharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle>, void>)functionPointer)(ref instances, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref rendererGroupInstanceMultiHash);
					return;
				}
			}
			FreeInstances_0024BurstManaged(in instances, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref rendererGroupInstanceMultiHash);
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EReallocateInstances_000002A0_0024PostfixBurstDelegate))]
	public static void ReallocateInstances(bool implicitInstanceIndices, in NativeArray<int> rendererGroupIDs, in NativeArray<GPUDrivenPackedRendererData> packedRendererData, in NativeArray<int> instanceOffsets, in NativeArray<int> instanceCounts, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeArray<InstanceHandle> instances, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
	{
		ReallocateInstances_000002A0_0024BurstDirectCall.Invoke(implicitInstanceIndices, in rendererGroupIDs, in packedRendererData, in instanceOffsets, in instanceCounts, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref instances, ref rendererGroupInstanceMultiHash);
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EFreeRendererGroupInstances_000002A1_0024PostfixBurstDelegate))]
	public static void FreeRendererGroupInstances(in NativeArray<int>.ReadOnly rendererGroupsID, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
	{
		FreeRendererGroupInstances_000002A1_0024BurstDirectCall.Invoke(in rendererGroupsID, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref rendererGroupInstanceMultiHash);
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EFreeInstances_000002A2_0024PostfixBurstDelegate))]
	public static void FreeInstances(in NativeArray<InstanceHandle>.ReadOnly instances, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
	{
		FreeInstances_000002A2_0024BurstDirectCall.Invoke(in instances, ref instanceAllocators, ref instanceData, ref perCameraInstanceData, ref sharedInstanceData, ref rendererGroupInstanceMultiHash);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static void ReallocateInstances_0024BurstManaged(bool implicitInstanceIndices, in NativeArray<int> rendererGroupIDs, in NativeArray<GPUDrivenPackedRendererData> packedRendererData, in NativeArray<int> instanceOffsets, in NativeArray<int> instanceCounts, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeArray<InstanceHandle> instances, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
	{
		for (int i = 0; i < rendererGroupIDs.Length; i++)
		{
			int key = rendererGroupIDs[i];
			bool hasTree = packedRendererData[i].hasTree;
			int num;
			int num2;
			if (implicitInstanceIndices)
			{
				num = 1;
				num2 = i;
			}
			else
			{
				num = instanceCounts[i];
				num2 = instanceOffsets[i];
			}
			SharedInstanceHandle sharedInstanceHandle;
			if (rendererGroupInstanceMultiHash.TryGetFirstValue(key, out var item, out var it))
			{
				sharedInstanceHandle = instanceData.Get_SharedInstance(item);
				if (sharedInstanceData.Get_RefCount(sharedInstanceHandle) - num > 0)
				{
					bool flag = true;
					int num3 = 0;
					for (int j = 0; j < num; j++)
					{
						flag = rendererGroupInstanceMultiHash.TryGetNextValue(out item, ref it);
					}
					while (flag)
					{
						int index = instanceData.InstanceToIndex(item);
						instanceData.Remove(item);
						perCameraInstanceData.Remove(index);
						instanceAllocators.FreeInstance(item);
						rendererGroupInstanceMultiHash.Remove(it);
						num3++;
						flag = rendererGroupInstanceMultiHash.TryGetNextValue(out item, ref it);
					}
				}
			}
			else
			{
				sharedInstanceHandle = instanceAllocators.AllocateSharedInstance();
				sharedInstanceData.AddNoGrow(sharedInstanceHandle);
			}
			if (num > 0)
			{
				sharedInstanceData.Set_RefCount(sharedInstanceHandle, num);
				for (int k = 0; k < num; k++)
				{
					int index2 = num2 + k;
					if (!instances[index2].valid)
					{
						InstanceHandle instanceHandle = (hasTree ? instanceAllocators.AllocateInstance(InstanceType.SpeedTree) : instanceAllocators.AllocateInstance(InstanceType.MeshRenderer));
						instanceData.AddNoGrow(instanceHandle);
						perCameraInstanceData.IncreaseInstanceCount();
						int index3 = instanceData.InstanceToIndex(instanceHandle);
						instanceData.sharedInstances[index3] = sharedInstanceHandle;
						instanceData.movedInCurrentFrameBits.Set(index3, value: false);
						instanceData.movedInPreviousFrameBits.Set(index3, value: false);
						instanceData.visibleInPreviousFrameBits.Set(index3, value: false);
						rendererGroupInstanceMultiHash.Add(key, instanceHandle);
						instances[index2] = instanceHandle;
					}
				}
			}
			else
			{
				sharedInstanceData.Remove(sharedInstanceHandle);
				instanceAllocators.FreeSharedInstance(sharedInstanceHandle);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static void FreeRendererGroupInstances_0024BurstManaged(in NativeArray<int>.ReadOnly rendererGroupsID, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
	{
		foreach (int item2 in rendererGroupsID)
		{
			InstanceHandle item;
			NativeParallelMultiHashMapIterator<int> it;
			bool flag = rendererGroupInstanceMultiHash.TryGetFirstValue(item2, out item, out it);
			while (flag)
			{
				SharedInstanceHandle instance = instanceData.Get_SharedInstance(item);
				int index = sharedInstanceData.SharedInstanceToIndex(instance);
				int num = sharedInstanceData.refCounts[index];
				if (num > 1)
				{
					sharedInstanceData.refCounts[index] = num - 1;
				}
				else
				{
					sharedInstanceData.Remove(instance);
					instanceAllocators.FreeSharedInstance(instance);
				}
				int index2 = instanceData.InstanceToIndex(item);
				instanceData.Remove(item);
				perCameraInstanceData.Remove(index2);
				instanceAllocators.FreeInstance(item);
				flag = rendererGroupInstanceMultiHash.TryGetNextValue(out item, ref it);
			}
			rendererGroupInstanceMultiHash.Remove(item2);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static void FreeInstances_0024BurstManaged(in NativeArray<InstanceHandle>.ReadOnly instances, ref InstanceAllocators instanceAllocators, ref CPUInstanceData instanceData, ref CPUPerCameraInstanceData perCameraInstanceData, ref CPUSharedInstanceData sharedInstanceData, ref NativeParallelMultiHashMap<int, InstanceHandle> rendererGroupInstanceMultiHash)
	{
		foreach (InstanceHandle instance2 in instances)
		{
			if (!instanceData.IsValidInstance(instance2))
			{
				continue;
			}
			int index = instanceData.InstanceToIndex(instance2);
			SharedInstanceHandle instance = instanceData.sharedInstances[index];
			int index2 = sharedInstanceData.SharedInstanceToIndex(instance);
			int num = sharedInstanceData.refCounts[index2];
			int key = sharedInstanceData.rendererGroupIDs[index2];
			if (num > 1)
			{
				sharedInstanceData.refCounts[index2] = num - 1;
			}
			else
			{
				sharedInstanceData.Remove(instance);
				instanceAllocators.FreeSharedInstance(instance);
			}
			int index3 = instanceData.InstanceToIndex(instance2);
			instanceData.Remove(instance2);
			perCameraInstanceData.Remove(index3);
			instanceAllocators.FreeInstance(instance2);
			InstanceHandle item;
			NativeParallelMultiHashMapIterator<int> it;
			bool flag = rendererGroupInstanceMultiHash.TryGetFirstValue(key, out item, out it);
			while (flag)
			{
				if (instance2.Equals(item))
				{
					rendererGroupInstanceMultiHash.Remove(it);
					break;
				}
				flag = rendererGroupInstanceMultiHash.TryGetNextValue(out item, ref it);
			}
		}
	}
}
