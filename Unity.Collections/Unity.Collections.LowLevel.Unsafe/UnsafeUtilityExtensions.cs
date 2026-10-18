using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public static class UnsafeUtilityExtensions
{
	internal unsafe static void MemSwap(void* ptr, void* otherPtr, long size)
	{
		byte* ptr2 = (byte*)ptr;
		byte* ptr3 = (byte*)otherPtr;
		byte* ptr4 = stackalloc byte[1024];
		while (size > 0)
		{
			long num = math.min(size, 1024L);
			UnsafeUtility.MemCpy(ptr4, ptr2, num);
			UnsafeUtility.MemCpy(ptr2, ptr3, num);
			UnsafeUtility.MemCpy(ptr3, ptr4, num);
			size -= num;
			ptr3 += num;
			ptr2 += num;
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static T ReadArrayElementBoundsChecked<T>(void* source, int index, int capacity) where T : unmanaged
	{
		return UnsafeUtility.ReadArrayElement<T>(source, index);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static void WriteArrayElementBoundsChecked<T>(void* destination, int index, T value, int capacity) where T : unmanaged
	{
		UnsafeUtility.WriteArrayElement(destination, index, value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static void* AddressOf<T>(in T value) where T : unmanaged
	{
		return ILSupport.AddressOf(in value);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static ref T AsRef<T>(in T value) where T : unmanaged
	{
		return ref ILSupport.AsRef(in value);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private unsafe static void CheckMemSwapOverlap(byte* dst, byte* src, long size)
	{
		if (dst + size > src && src + size > dst)
		{
			throw new InvalidOperationException("MemSwap memory blocks are overlapped.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckIndexRange(int index, int capacity)
	{
		if (index > capacity - 1 || index < 0)
		{
			throw new IndexOutOfRangeException($"Attempt to read or write from array index {index}, which is out of bounds. Array capacity is {capacity}. " + "This may lead to a crash, data corruption, or reading invalid data.");
		}
	}
}
