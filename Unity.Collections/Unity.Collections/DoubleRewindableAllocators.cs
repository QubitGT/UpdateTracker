using System;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

public struct DoubleRewindableAllocators : IDisposable
{
	private unsafe RewindableAllocator* Pointer;

	private AllocatorHelper<RewindableAllocator> UpdateAllocatorHelper0;

	private AllocatorHelper<RewindableAllocator> UpdateAllocatorHelper1;

	public unsafe ref RewindableAllocator Allocator => ref UnsafeUtility.AsRef<RewindableAllocator>(Pointer);

	public unsafe bool IsCreated => Pointer != null;

	internal bool EnableBlockFree
	{
		get
		{
			return UpdateAllocatorHelper0.Allocator.EnableBlockFree;
		}
		set
		{
			UpdateAllocatorHelper0.Allocator.EnableBlockFree = value;
			UpdateAllocatorHelper1.Allocator.EnableBlockFree = value;
		}
	}

	public unsafe void Update()
	{
		RewindableAllocator* ptr = (RewindableAllocator*)UnsafeUtility.AddressOf(ref UpdateAllocatorHelper0.Allocator);
		RewindableAllocator* ptr2 = (RewindableAllocator*)UnsafeUtility.AddressOf(ref UpdateAllocatorHelper1.Allocator);
		Pointer = ((Pointer == ptr) ? ptr2 : ptr);
		Allocator.Rewind();
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckIsCreated()
	{
		if (!IsCreated)
		{
			throw new InvalidOperationException("DoubleRewindableAllocators is not created.");
		}
	}

	public DoubleRewindableAllocators(AllocatorManager.AllocatorHandle backingAllocator, int initialSizeInBytes)
	{
		this = default(DoubleRewindableAllocators);
		Initialize(backingAllocator, initialSizeInBytes);
	}

	public unsafe void Initialize(AllocatorManager.AllocatorHandle backingAllocator, int initialSizeInBytes)
	{
		UpdateAllocatorHelper0 = new AllocatorHelper<RewindableAllocator>(backingAllocator);
		UpdateAllocatorHelper1 = new AllocatorHelper<RewindableAllocator>(backingAllocator);
		UpdateAllocatorHelper0.Allocator.Initialize(initialSizeInBytes);
		UpdateAllocatorHelper1.Allocator.Initialize(initialSizeInBytes);
		Pointer = null;
		Update();
	}

	public void Dispose()
	{
		if (IsCreated)
		{
			UpdateAllocatorHelper0.Allocator.Dispose();
			UpdateAllocatorHelper1.Allocator.Dispose();
			UpdateAllocatorHelper0.Dispose();
			UpdateAllocatorHelper1.Dispose();
		}
	}
}
