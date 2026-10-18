using System;
using System.Diagnostics;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public struct UnsafeScratchAllocator
{
	private unsafe void* m_Pointer;

	private int m_LengthInBytes;

	private readonly int m_CapacityInBytes;

	public unsafe UnsafeScratchAllocator(void* ptr, int capacityInBytes)
	{
		m_Pointer = ptr;
		m_LengthInBytes = 0;
		m_CapacityInBytes = capacityInBytes;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckAllocationDoesNotExceedCapacity(ulong requestedSize)
	{
		if (requestedSize > (ulong)m_CapacityInBytes)
		{
			throw new ArgumentException($"Cannot allocate more than provided size in UnsafeScratchAllocator. Requested: {requestedSize} Size: {m_LengthInBytes} Capacity: {m_CapacityInBytes}");
		}
	}

	public unsafe void* Allocate(int sizeInBytes, int alignmentInBytes)
	{
		if (sizeInBytes == 0)
		{
			return null;
		}
		ulong num = (ulong)(alignmentInBytes - 1);
		long num2 = ((long)(IntPtr)m_Pointer + m_LengthInBytes + (long)num) & (long)(~num);
		long num3 = (byte*)(void*)(IntPtr)num2 - (byte*)m_Pointer;
		num3 += sizeInBytes;
		m_LengthInBytes = (int)num3;
		return (void*)(IntPtr)num2;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe void* Allocate<T>(int count = 1) where T : unmanaged
	{
		return Allocate(UnsafeUtility.SizeOf<T>() * count, UnsafeUtility.AlignOf<T>());
	}
}
