using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public static class CollectionHelper
{
	[StructLayout(LayoutKind.Explicit)]
	internal struct LongDoubleUnion
	{
		[FieldOffset(0)]
		internal long longValue;

		[FieldOffset(0)]
		internal double doubleValue;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[BurstCompile]
	public struct DummyJob : IJob
	{
		public void Execute()
		{
		}
	}

	public const int CacheLineSize = 64;

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckAllocator(AllocatorManager.AllocatorHandle allocator)
	{
		if (!ShouldDeallocate(allocator))
		{
			throw new ArgumentException($"Allocator {allocator} must not be None or Invalid");
		}
	}

	public static int Log2Floor(int value)
	{
		return 31 - math.lzcnt((uint)value);
	}

	public static int Log2Ceil(int value)
	{
		return 32 - math.lzcnt((uint)(value - 1));
	}

	public static int Align(int size, int alignmentPowerOfTwo)
	{
		if (alignmentPowerOfTwo == 0)
		{
			return size;
		}
		return (size + alignmentPowerOfTwo - 1) & ~(alignmentPowerOfTwo - 1);
	}

	public static ulong Align(ulong size, ulong alignmentPowerOfTwo)
	{
		if (alignmentPowerOfTwo == 0L)
		{
			return size;
		}
		return (size + alignmentPowerOfTwo - 1) & ~(alignmentPowerOfTwo - 1);
	}

	internal unsafe static void* AlignPointer(void* ptr, int alignmentPowerOfTwo)
	{
		if (alignmentPowerOfTwo == 0)
		{
			return ptr;
		}
		nuint num = (nuint)alignmentPowerOfTwo;
		return (void*)((nuint)((byte*)ptr + num - 1) & ~(num - 1));
	}

	public unsafe static bool IsAligned(void* p, int alignmentPowerOfTwo)
	{
		return ((ulong)p & (ulong)((long)alignmentPowerOfTwo - 1L)) == 0;
	}

	public static bool IsAligned(ulong offset, int alignmentPowerOfTwo)
	{
		return (offset & (ulong)((long)alignmentPowerOfTwo - 1L)) == 0;
	}

	public static bool IsPowerOfTwo(int value)
	{
		return (value & (value - 1)) == 0;
	}

	public unsafe static uint Hash(void* ptr, int bytes)
	{
		ulong num = 5381uL;
		while (bytes > 0)
		{
			ulong num2 = ((byte*)ptr)[--bytes];
			num = (num << 5) + num + num2;
		}
		return (uint)num;
	}

	[ExcludeFromBurstCompatTesting("Used only for debugging, and uses managed strings")]
	internal static void WriteLayout(Type type)
	{
		Console.WriteLine($"   Offset | Bytes  | Name     Layout: {0}", type.Name);
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			Console.WriteLine("   {0, 6} | {1, 6} | {2}", Marshal.OffsetOf(type, fieldInfo.Name), Marshal.SizeOf(fieldInfo.FieldType), fieldInfo.Name);
		}
	}

	internal static bool ShouldDeallocate(AllocatorManager.AllocatorHandle allocator)
	{
		return allocator.ToAllocator > Allocator.None;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[return: AssumeRange(0L, 2147483647L)]
	internal static int AssumePositive(int value)
	{
		return value;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "ENABLE_UNITY_COLLECTIONS_CHECKS", GenericTypeArguments = new Type[] { typeof(NativeArray<int>) })]
	internal static void CheckIsUnmanaged<T>()
	{
		if (!UnsafeUtility.IsUnmanaged<T>())
		{
			throw new ArgumentException($"{typeof(T)} used in native collection is not blittable or not primitive");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckIntPositivePowerOfTwo(int value)
	{
		if (value <= 0 || (value & (value - 1)) != 0)
		{
			throw new ArgumentException($"Alignment requested: {value} is not a non-zero, positive power of two.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckUlongPositivePowerOfTwo(ulong value)
	{
		if (value == 0 || (value & (value - 1)) != 0)
		{
			throw new ArgumentException($"Alignment requested: {value} is not a non-zero, positive power of two.");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckIndexInRange(int index, int length)
	{
		if ((uint)index >= (uint)length)
		{
			throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckCapacityInRange(int capacity, int length)
	{
		if (capacity < 0)
		{
			throw new ArgumentOutOfRangeException($"Capacity {capacity} must be positive.");
		}
		if (capacity < length)
		{
			throw new ArgumentOutOfRangeException($"Capacity {capacity} is out of range in container of '{length}' Length.");
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(AllocatorManager.AllocatorHandle)
	})]
	public static NativeArray<T> CreateNativeArray<T, U>(int length, ref U allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : unmanaged where U : unmanaged, AllocatorManager.IAllocator
	{
		NativeArray<T> array;
		if (!allocator.IsCustomAllocator)
		{
			array = new NativeArray<T>(length, allocator.ToAllocator, options);
		}
		else
		{
			array = default(NativeArray<T>);
			NativeArrayExtensions.Initialize(ref array, length, ref allocator, options);
		}
		return array;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static NativeArray<T> CreateNativeArray<T>(int length, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : unmanaged
	{
		NativeArray<T> array;
		if (!AllocatorManager.IsCustomAllocator(allocator))
		{
			array = new NativeArray<T>(length, allocator.ToAllocator, options);
		}
		else
		{
			array = default(NativeArray<T>);
			NativeArrayExtensions.Initialize(ref array, length, allocator, options);
		}
		return array;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static NativeArray<T> CreateNativeArray<T>(NativeArray<T> array, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		NativeArray<T> array2;
		if (!AllocatorManager.IsCustomAllocator(allocator))
		{
			array2 = new NativeArray<T>(array, allocator.ToAllocator);
		}
		else
		{
			array2 = default(NativeArray<T>);
			NativeArrayExtensions.Initialize(ref array2, array.Length, allocator);
			array2.CopyFrom(array);
		}
		return array2;
	}

	[ExcludeFromBurstCompatTesting("Managed array")]
	public static NativeArray<T> CreateNativeArray<T>(T[] array, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		NativeArray<T> array2;
		if (!AllocatorManager.IsCustomAllocator(allocator))
		{
			array2 = new NativeArray<T>(array, allocator.ToAllocator);
		}
		else
		{
			array2 = default(NativeArray<T>);
			NativeArrayExtensions.Initialize(ref array2, array.Length, allocator);
			array2.CopyFrom(array);
		}
		return array2;
	}

	[ExcludeFromBurstCompatTesting("Managed array")]
	public static NativeArray<T> CreateNativeArray<T, U>(T[] array, ref U allocator) where T : unmanaged where U : unmanaged, AllocatorManager.IAllocator
	{
		NativeArray<T> array2;
		if (!allocator.IsCustomAllocator)
		{
			array2 = new NativeArray<T>(array, allocator.ToAllocator);
		}
		else
		{
			array2 = default(NativeArray<T>);
			NativeArrayExtensions.Initialize(ref array2, array.Length, ref allocator);
			array2.CopyFrom(array);
		}
		return array2;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static void DisposeNativeArray<T>(NativeArray<T> nativeArray, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		NativeArrayExtensions.DisposeCheckAllocator(ref nativeArray);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static void Dispose<T>(NativeArray<T> nativeArray) where T : unmanaged
	{
		NativeArrayExtensions.DisposeCheckAllocator(ref nativeArray);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckConvertArguments<T>(int length) where T : unmanaged
	{
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length must be >= 0");
		}
		if (!UnsafeUtility.IsUnmanaged<T>())
		{
			throw new InvalidOperationException($"{typeof(T)} used in NativeArray<{typeof(T)}> must be unmanaged (contain no managed types).");
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static NativeArray<T> ConvertExistingDataToNativeArray<T>(void* dataPointer, int length, AllocatorManager.AllocatorHandle allocator, bool setTempMemoryHandle = false) where T : unmanaged
	{
		NativeArray<T> result = default(NativeArray<T>);
		result.m_Buffer = dataPointer;
		result.m_Length = length;
		if (!allocator.IsCustomAllocator)
		{
			result.m_AllocatorLabel = allocator.ToAllocator;
		}
		else
		{
			result.m_AllocatorLabel = Allocator.None;
		}
		return result;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static NativeArray<T> ConvertExistingNativeListToNativeArray<T>(ref NativeList<T> nativeList, int length, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		return ConvertExistingDataToNativeArray<T>(nativeList.GetUnsafePtr(), length, allocator);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int),
		typeof(AllocatorManager.AllocatorHandle)
	})]
	public static NativeParallelMultiHashMap<TKey, TValue> CreateNativeParallelMultiHashMap<TKey, TValue, U>(int length, ref U allocator) where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged where U : unmanaged, AllocatorManager.IAllocator
	{
		NativeParallelMultiHashMap<TKey, TValue> result = default(NativeParallelMultiHashMap<TKey, TValue>);
		result.Initialize(length, ref allocator);
		return result;
	}

	[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "ENABLE_UNITY_COLLECTIONS_CHECKS", GenericTypeArguments = new Type[] { typeof(DummyJob) })]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	public static void CheckReflectionDataCorrect<T>(IntPtr reflectionData)
	{
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	[BurstDiscard]
	private static void CheckReflectionDataCorrectInternal<T>(IntPtr reflectionData, ref bool burstCompiled)
	{
		if (reflectionData == IntPtr.Zero)
		{
			throw new InvalidOperationException($"Reflection data was not set up by an Initialize() call. For generic job types, please include [assembly: RegisterGenericJobType(typeof({typeof(T)}))] in your source file.");
		}
		burstCompiled = false;
	}
}
