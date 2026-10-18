using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Collections;

namespace UnityEngine.Rendering;

[BurstCompile]
internal static class GPUResidentDrawerBurst
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void ClassifyMaterials_000000EA_0024PostfixBurstDelegate(in NativeArray<int> materialIDs, in NativeParallelHashMap<int, BatchMaterialID>.ReadOnly batchMaterialHash, ref NativeList<int> supportedMaterialIDs, ref NativeList<int> unsupportedMaterialIDs, ref NativeList<GPUDrivenPackedMaterialData> supportedPackedMaterialDatas);

	internal static class ClassifyMaterials_000000EA_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<ClassifyMaterials_000000EA_0024PostfixBurstDelegate>(ClassifyMaterials).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<int> materialIDs, in NativeParallelHashMap<int, BatchMaterialID>.ReadOnly batchMaterialHash, ref NativeList<int> supportedMaterialIDs, ref NativeList<int> unsupportedMaterialIDs, ref NativeList<GPUDrivenPackedMaterialData> supportedPackedMaterialDatas)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeParallelHashMap<int, BatchMaterialID>.ReadOnly, ref NativeList<int>, ref NativeList<int>, ref NativeList<GPUDrivenPackedMaterialData>, void>)functionPointer)(ref materialIDs, ref batchMaterialHash, ref supportedMaterialIDs, ref unsupportedMaterialIDs, ref supportedPackedMaterialDatas);
					return;
				}
			}
			ClassifyMaterials_0024BurstManaged(in materialIDs, in batchMaterialHash, ref supportedMaterialIDs, ref unsupportedMaterialIDs, ref supportedPackedMaterialDatas);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void FindUnsupportedRenderers_000000EB_0024PostfixBurstDelegate(in NativeArray<int> unsupportedMaterials, in NativeArray<SmallIntegerArray>.ReadOnly materialIDArrays, in NativeArray<int>.ReadOnly rendererGroups, ref NativeList<int> unsupportedRenderers);

	internal static class FindUnsupportedRenderers_000000EB_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<FindUnsupportedRenderers_000000EB_0024PostfixBurstDelegate>(FindUnsupportedRenderers).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<int> unsupportedMaterials, in NativeArray<SmallIntegerArray>.ReadOnly materialIDArrays, in NativeArray<int>.ReadOnly rendererGroups, ref NativeList<int> unsupportedRenderers)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeArray<SmallIntegerArray>.ReadOnly, ref NativeArray<int>.ReadOnly, ref NativeList<int>, void>)functionPointer)(ref unsupportedMaterials, ref materialIDArrays, ref rendererGroups, ref unsupportedRenderers);
					return;
				}
			}
			FindUnsupportedRenderers_0024BurstManaged(in unsupportedMaterials, in materialIDArrays, in rendererGroups, ref unsupportedRenderers);
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetMaterialsWithChangedPackedMaterial_000000EC_0024PostfixBurstDelegate(in NativeArray<int> materialIDs, in NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas, in NativeParallelHashMap<int, GPUDrivenPackedMaterialData>.ReadOnly packedMaterialHash, ref NativeHashSet<int> filteredMaterials);

	internal static class GetMaterialsWithChangedPackedMaterial_000000EC_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetMaterialsWithChangedPackedMaterial_000000EC_0024PostfixBurstDelegate>(GetMaterialsWithChangedPackedMaterial).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in NativeArray<int> materialIDs, in NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas, in NativeParallelHashMap<int, GPUDrivenPackedMaterialData>.ReadOnly packedMaterialHash, ref NativeHashSet<int> filteredMaterials)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref NativeArray<int>, ref NativeArray<GPUDrivenPackedMaterialData>, ref NativeParallelHashMap<int, GPUDrivenPackedMaterialData>.ReadOnly, ref NativeHashSet<int>, void>)functionPointer)(ref materialIDs, ref packedMaterialDatas, ref packedMaterialHash, ref filteredMaterials);
					return;
				}
			}
			GetMaterialsWithChangedPackedMaterial_0024BurstManaged(in materialIDs, in packedMaterialDatas, in packedMaterialHash, ref filteredMaterials);
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EClassifyMaterials_000000EA_0024PostfixBurstDelegate))]
	public static void ClassifyMaterials(in NativeArray<int> materialIDs, in NativeParallelHashMap<int, BatchMaterialID>.ReadOnly batchMaterialHash, ref NativeList<int> supportedMaterialIDs, ref NativeList<int> unsupportedMaterialIDs, ref NativeList<GPUDrivenPackedMaterialData> supportedPackedMaterialDatas)
	{
		ClassifyMaterials_000000EA_0024BurstDirectCall.Invoke(in materialIDs, in batchMaterialHash, ref supportedMaterialIDs, ref unsupportedMaterialIDs, ref supportedPackedMaterialDatas);
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EFindUnsupportedRenderers_000000EB_0024PostfixBurstDelegate))]
	public static void FindUnsupportedRenderers(in NativeArray<int> unsupportedMaterials, in NativeArray<SmallIntegerArray>.ReadOnly materialIDArrays, in NativeArray<int>.ReadOnly rendererGroups, ref NativeList<int> unsupportedRenderers)
	{
		FindUnsupportedRenderers_000000EB_0024BurstDirectCall.Invoke(in unsupportedMaterials, in materialIDArrays, in rendererGroups, ref unsupportedRenderers);
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	[MonoPInvokeCallback(typeof(UnityEngine_002ERendering_002EGetMaterialsWithChangedPackedMaterial_000000EC_0024PostfixBurstDelegate))]
	public static void GetMaterialsWithChangedPackedMaterial(in NativeArray<int> materialIDs, in NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas, in NativeParallelHashMap<int, GPUDrivenPackedMaterialData>.ReadOnly packedMaterialHash, ref NativeHashSet<int> filteredMaterials)
	{
		GetMaterialsWithChangedPackedMaterial_000000EC_0024BurstDirectCall.Invoke(in materialIDs, in packedMaterialDatas, in packedMaterialHash, ref filteredMaterials);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static void ClassifyMaterials_0024BurstManaged(in NativeArray<int> materialIDs, in NativeParallelHashMap<int, BatchMaterialID>.ReadOnly batchMaterialHash, ref NativeList<int> supportedMaterialIDs, ref NativeList<int> unsupportedMaterialIDs, ref NativeList<GPUDrivenPackedMaterialData> supportedPackedMaterialDatas)
	{
		NativeList<int> nativeList = new NativeList<int>(4, Allocator.Temp);
		foreach (int materialID in materialIDs)
		{
			int value = materialID;
			if (batchMaterialHash.ContainsKey(value))
			{
				nativeList.Add(in value);
			}
		}
		if (nativeList.IsEmpty)
		{
			nativeList.Dispose();
			return;
		}
		unsupportedMaterialIDs.Resize(nativeList.Length, NativeArrayOptions.UninitializedMemory);
		supportedMaterialIDs.Resize(nativeList.Length, NativeArrayOptions.UninitializedMemory);
		supportedPackedMaterialDatas.Resize(nativeList.Length, NativeArrayOptions.UninitializedMemory);
		int num = GPUDrivenProcessor.ClassifyMaterials(nativeList.AsArray(), unsupportedMaterialIDs.AsArray(), supportedMaterialIDs.AsArray(), supportedPackedMaterialDatas.AsArray());
		unsupportedMaterialIDs.Resize(num, NativeArrayOptions.ClearMemory);
		supportedMaterialIDs.Resize(nativeList.Length - num, NativeArrayOptions.ClearMemory);
		supportedPackedMaterialDatas.Resize(supportedMaterialIDs.Length, NativeArrayOptions.ClearMemory);
		nativeList.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static void FindUnsupportedRenderers_0024BurstManaged(in NativeArray<int> unsupportedMaterials, in NativeArray<SmallIntegerArray>.ReadOnly materialIDArrays, in NativeArray<int>.ReadOnly rendererGroups, ref NativeList<int> unsupportedRenderers)
	{
		for (int i = 0; i < materialIDArrays.Length; i++)
		{
			SmallIntegerArray smallIntegerArray = materialIDArrays[i];
			int value = rendererGroups[i];
			for (int j = 0; j < smallIntegerArray.Length; j++)
			{
				int value2 = smallIntegerArray[j];
				if (unsupportedMaterials.Contains(value2))
				{
					unsupportedRenderers.Add(in value);
					break;
				}
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal static void GetMaterialsWithChangedPackedMaterial_0024BurstManaged(in NativeArray<int> materialIDs, in NativeArray<GPUDrivenPackedMaterialData> packedMaterialDatas, in NativeParallelHashMap<int, GPUDrivenPackedMaterialData>.ReadOnly packedMaterialHash, ref NativeHashSet<int> filteredMaterials)
	{
		for (int i = 0; i < materialIDs.Length; i++)
		{
			int num = materialIDs[i];
			GPUDrivenPackedMaterialData other = packedMaterialDatas[i];
			if (!packedMaterialHash.TryGetValue(num, out var item) || !item.Equals(other))
			{
				filteredMaterials.Add(num);
			}
		}
	}
}
