using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace K4os.Compression.LZ4.Internal;

public struct PinnedMemory
{
	private unsafe byte* _pointer;

	private GCHandle _handle;

	private int _size;

	public static int MaxPooledSize { get; set; } = 1048576;


	public unsafe readonly byte* Pointer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _pointer;
		}
	}

	public unsafe Span<byte> Span
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Span<byte>(Pointer, _size);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe readonly T* Reference<T>() where T : unmanaged
	{
		return (T*)_pointer;
	}

	public static PinnedMemory Alloc(int size, bool zero = true)
	{
		Alloc(out var memory, size, zero);
		return memory;
	}

	public static void Alloc(out PinnedMemory memory, int size, bool zero = true)
	{
		if (size <= 0)
		{
			throw new ArgumentOutOfRangeException("size");
		}
		if (size > MaxPooledSize)
		{
			AllocateNative(out memory, size, zero);
		}
		else
		{
			RentManagedFromPool(out memory, size, zero);
		}
	}

	public unsafe static void Alloc<T>(out PinnedMemory memory, bool zero = true) where T : unmanaged
	{
		Alloc(out memory, sizeof(T), zero);
	}

	private unsafe static void AllocateNative(out PinnedMemory memory, int size, bool zero)
	{
		void* pointer = (zero ? Mem.AllocZero(size) : Mem.Alloc(size));
		GC.AddMemoryPressure(size);
		memory._pointer = (byte*)pointer;
		memory._handle = default(GCHandle);
		memory._size = size;
	}

	private unsafe static void RentManagedFromPool(out PinnedMemory memory, int size, bool zero)
	{
		GCHandle handle = GCHandle.Alloc(BufferPool.Alloc(size, zero), GCHandleType.Pinned);
		byte* pointer = (byte*)(void*)handle.AddrOfPinnedObject();
		memory._pointer = pointer;
		memory._handle = handle;
		memory._size = size;
	}

	public unsafe void Clear()
	{
		if (_size > 0 && _pointer != null)
		{
			Mem.Zero(_pointer, _size);
		}
	}

	public unsafe void Free()
	{
		if (_handle.IsAllocated)
		{
			ReleaseManaged();
		}
		else if (_pointer != null)
		{
			ReleaseNative();
		}
		ClearFields();
	}

	private void ReleaseManaged()
	{
		byte[] buffer = (_handle.IsAllocated ? ((byte[])_handle.Target) : null);
		_handle.Free();
		BufferPool.Free(buffer);
	}

	private unsafe void ReleaseNative()
	{
		GC.RemoveMemoryPressure(_size);
		Mem.Free(_pointer);
	}

	private unsafe void ClearFields()
	{
		_pointer = null;
		_handle = default(GCHandle);
		_size = 0;
	}
}
