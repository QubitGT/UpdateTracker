using System;

namespace Fusion;

public class DynamicHeapInstance
{
	private unsafe DynamicHeap* _heap;

	public unsafe DynamicHeapInstance(params Type[] types)
	{
		_heap = DynamicHeap.Create(DynamicHeap.Config.Default, types);
	}

	unsafe ~DynamicHeapInstance()
	{
		if (_heap != null)
		{
			DynamicHeap.Destroy(_heap);
		}
	}

	public unsafe void Free(void* ptr)
	{
		DynamicHeap.Free(_heap, ptr);
	}

	public unsafe void* Allocate(int size)
	{
		return DynamicHeap.Allocate(_heap, size);
	}

	public unsafe void* AllocateArray<T>(int length) where T : unmanaged
	{
		VerifyArrayLength(length);
		return DynamicHeap.Allocate(_heap, sizeof(T) * length);
	}

	public unsafe void* AllocateArrayPointers<T>(int length) where T : unmanaged
	{
		VerifyArrayLength(length);
		return DynamicHeap.Allocate(_heap, sizeof(T*) * length);
	}

	public unsafe void* AllocateTracked<T>(bool root = false) where T : unmanaged
	{
		return DynamicHeap.AllocateTracked<T>(_heap, 1, root);
	}

	public unsafe void* AllocateTrackedArray<T>(int length, bool root = false) where T : unmanaged
	{
		VerifyArrayLength(length);
		return DynamicHeap.AllocateTracked<T>(_heap, (ushort)length, root);
	}

	public unsafe void* AllocateTrackedArrayPointers<T>(int length, bool root = false) where T : unmanaged
	{
		VerifyArrayLength(length);
		return DynamicHeap.AllocateTrackedPointerArray<T>(_heap, (ushort)length, root);
	}

	private void VerifyArrayLength(int length)
	{
		Assert.Always(length > 0, "length > 0");
		Assert.Always(length <= 65535, "length <= ushort.MaxValue");
	}
}
