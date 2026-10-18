#define DEBUG
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Fusion;

public static class Native
{
	public const int ALIGNMENT = 8;

	public const int CACHE_LINE_SIZE = 64;

	private const int SentinelLeadingSize = 0;

	private const int SentinelTrailingSize = 0;

	private const ulong SentinelLeadingPattern = 15705636252112664309uL;

	private const ulong SentinelTrailingPattern = 12580085127939517179uL;

	public unsafe static void MemMove(void* destination, void* source, int size)
	{
		if (destination != null && source != null)
		{
			UnsafeUtility.MemMove(destination, source, size);
		}
	}

	public unsafe static void MemCpy(void* destination, void* source, int size)
	{
		if (destination != null && source != null)
		{
			UnsafeUtility.MemCpy(destination, source, size);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static void MemCpy(Span<int> d, Span<int> s)
	{
		Assert.Always(s.Length <= d.Length, s.Length, d.Length);
		UnsafeUtility.MemCpy(d.AsPointer<byte>(), s.AsPointer<byte>(), 4 * d.Length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static void MemCpy(Span<byte> d, Span<byte> s)
	{
		Assert.Always(s.Length <= d.Length, s.Length, d.Length);
		UnsafeUtility.MemCpy(d.AsPointer<byte>(), s.AsPointer<byte>(), d.Length);
	}

	public unsafe static void MemClear(void* ptr, int size)
	{
		if (ptr != null)
		{
			UnsafeUtility.MemClear(ptr, size);
		}
	}

	public unsafe static int MemCmp(void* ptr1, void* ptr2, int size)
	{
		if (ptr1 == null || ptr2 == null)
		{
			return 0;
		}
		return UnsafeUtility.MemCmp(ptr1, ptr2, size);
	}

	public unsafe static void* Malloc(int size)
	{
		if (size <= 0)
		{
			throw new Exception($"Trying to allocate <= bytes: {size}");
		}
		if (size > 1073741824)
		{
			throw new Exception($"Trying to allocate very large block: {size} bytes");
		}
		return UnsafeUtility.Malloc(size, 8, Allocator.Persistent);
	}

	private unsafe static void Free(void* memory)
	{
		if (memory != null)
		{
			UnsafeUtility.Free(memory, Allocator.Persistent);
		}
	}

	public static int SizeOf(Type t)
	{
		return UnsafeUtility.SizeOf(t);
	}

	public static int GetFieldOffset(FieldInfo fi)
	{
		return UnsafeUtility.GetFieldOffset(fi);
	}

	public unsafe static void Free(ref void* memory)
	{
		void* memory2 = memory;
		memory = null;
		Free(memory2);
	}

	public unsafe static void Free<T>(ref T* memory) where T : unmanaged
	{
		T* memory2 = memory;
		memory = null;
		Free(memory2);
	}

	public unsafe static void Free<T>(ref T** memory) where T : unmanaged
	{
		T** memory2 = memory;
		memory = null;
		Free(memory2);
	}

	public unsafe static void* MallocAndClear(int size)
	{
		void* ptr = Malloc(size);
		MemClear(ptr, size);
		return ptr;
	}

	public unsafe static T* MallocAndClear<T>() where T : unmanaged
	{
		T* ptr = Malloc<T>();
		MemClear(ptr, sizeof(T));
		return ptr;
	}

	public unsafe static T* Malloc<T>() where T : unmanaged
	{
		return (T*)Malloc(sizeof(T));
	}

	public unsafe static void* MallocAndClearArray(int stride, int length)
	{
		void* ptr = Malloc(stride * length);
		MemClear(ptr, stride * length);
		return ptr;
	}

	public unsafe static T* MallocAndClearArray<T>(int length) where T : unmanaged
	{
		return (T*)MallocAndClearArray(sizeof(T), length);
	}

	public unsafe static T* MallocAndClearArrayMin1<T>(int length) where T : unmanaged
	{
		return MallocAndClearArray<T>(Math.Max(1, length));
	}

	public unsafe static T** MallocAndClearPtrArray<T>(int length) where T : unmanaged
	{
		return (T**)MallocAndClearArray(sizeof(T*), length);
	}

	public unsafe static T** MallocAndClearPtrArrayMin1<T>(int length) where T : unmanaged
	{
		return MallocAndClearPtrArray<T>(Math.Max(1, length));
	}

	public unsafe static void ArrayCopy(void* source, int sourceIndex, void* destination, int destinationIndex, int count, int elementStride)
	{
		MemCpy((byte*)destination + destinationIndex * elementStride, (byte*)source + sourceIndex * elementStride, count * elementStride);
	}

	public unsafe static void ArrayClear<T>(T* ptr, int size) where T : unmanaged
	{
		MemClear(ptr, sizeof(T) * size);
	}

	public unsafe static int ArrayCompare<T>(T* ptr1, T* ptr2, int size) where T : unmanaged
	{
		return MemCmp(ptr1, ptr2, sizeof(T) * size);
	}

	public unsafe static T* DoubleArray<T>(T* array, int currentLength) where T : unmanaged
	{
		Assert.Check(currentLength > 0);
		return ExpandArray(array, currentLength, currentLength * 2);
	}

	public unsafe static T* ExpandArray<T>(T* array, int currentLength, int newLength) where T : unmanaged
	{
		Assert.Check(newLength > currentLength);
		T* ptr = MallocAndClearArray<T>(newLength);
		MemCpy(ptr, array, sizeof(T) * currentLength);
		Free(array);
		return ptr;
	}

	public unsafe static T** DoublePtrArray<T>(T** array, int currentLength) where T : unmanaged
	{
		return ExpandPtrArray(array, currentLength, currentLength * 2);
	}

	public unsafe static T** ExpandPtrArray<T>(T** array, int currentLength, int newLength) where T : unmanaged
	{
		Assert.Check(newLength > currentLength);
		T** ptr = MallocAndClearPtrArray<T>(newLength);
		MemCpy(ptr, array, sizeof(T*) * currentLength);
		Free(array);
		return ptr;
	}

	public unsafe static void* Expand(void* buffer, int currentSize, int newSize)
	{
		Assert.Check(newSize > currentSize);
		void* ptr = MallocAndClear(newSize);
		MemCpy(ptr, buffer, currentSize);
		Free(buffer);
		return ptr;
	}

	public unsafe static void MemCpyFast(void* d, void* s, int size)
	{
		switch (size)
		{
		case 4:
			*(int*)d = *(int*)s;
			break;
		case 8:
			*(long*)d = *(long*)s;
			break;
		case 12:
			*(long*)d = *(long*)s;
			*(int*)((byte*)d + (nint)2 * (nint)4) = *(int*)((byte*)s + (nint)2 * (nint)4);
			break;
		case 16:
			*(long*)d = *(long*)s;
			*(long*)((byte*)d + 8) = *(long*)((byte*)s + 8);
			break;
		default:
			MemCpy(d, s, size);
			break;
		}
	}

	public unsafe static int CopyFromArray<T>(void* destination, T[] source) where T : unmanaged
	{
		fixed (T* source2 = source)
		{
			MemCpy(destination, source2, source.Length * sizeof(T));
			return source.Length * sizeof(T);
		}
	}

	public unsafe static int CopyToArray<T>(T[] destination, void* source) where T : unmanaged
	{
		fixed (T* destination2 = destination)
		{
			MemCpy(destination2, source, destination.Length * sizeof(T));
			return destination.Length * sizeof(T);
		}
	}

	public static int GetLengthPrefixedUTF8ByteCount(string str)
	{
		return 4 + Encoding.UTF8.GetByteCount(str);
	}

	public unsafe static int WriteLengthPrefixedUTF8(void* destination, string str)
	{
		fixed (char* chars = str)
		{
			int byteCount = Encoding.UTF8.GetByteCount(str);
			int bytes = Encoding.UTF8.GetBytes(chars, str.Length, (byte*)destination + 4, byteCount);
			Assert.Check(byteCount == bytes, "Expected byte count mismatch {0} {1}", byteCount, bytes);
			*(int*)destination = bytes;
			return 4 + bytes;
		}
	}

	public unsafe static int ReadLengthPrefixedUTF8(void* source, out string result)
	{
		int num = *(int*)source;
		result = Encoding.UTF8.GetString((byte*)source + 4, num);
		return num + 4;
	}

	public unsafe static bool IsPointerAligned(void* pointer, int alignment)
	{
		return (long)pointer % (long)alignment == 0;
	}

	public unsafe static void* AlignPointer(void* pointer, int alignment)
	{
		long num = (long)pointer;
		if (num % alignment != 0)
		{
			return (byte*)pointer + (alignment - num % alignment);
		}
		return pointer;
	}

	public static int RoundToMaxAlignment(int stride)
	{
		return RoundToAlignment(stride, 8);
	}

	public static int WordCount(int stride, int wordSize)
	{
		return RoundToAlignment(stride, wordSize) / wordSize;
	}

	public static bool IsAligned(int stride, int alignment)
	{
		return RoundToAlignment(stride, alignment) == stride;
	}

	public static int RoundToAlignment(int stride, int alignment)
	{
		return alignment switch
		{
			1 => stride, 
			2 => (stride + 1 >> 1) * 2, 
			4 => (stride + 3 >> 2) * 4, 
			8 => (stride + 7 >> 3) * 8, 
			16 => (stride + 15 >> 4) * 16, 
			32 => (stride + 31 >> 5) * 32, 
			_ => throw new InvalidOperationException($"Invalid Alignment: {alignment}"), 
		};
	}

	public static long RoundToAlignment(long stride, int alignment)
	{
		return alignment switch
		{
			1 => stride, 
			2 => (stride + 1 >> 1) * 2, 
			4 => (stride + 3 >> 2) * 4, 
			8 => (stride + 7 >> 3) * 8, 
			16 => (stride + 15 >> 4) * 16, 
			32 => (stride + 31 >> 5) * 32, 
			_ => throw new InvalidOperationException($"Invalid Alignment: {alignment}"), 
		};
	}

	public static T Empty<T>() where T : unmanaged
	{
		return new T();
	}

	public static int RoundBitsUpTo64(int bits)
	{
		return (bits + 63 >> 6) * 64;
	}

	public static int RoundBitsUpTo32(int bits)
	{
		return (bits + 31 >> 5) * 32;
	}

	public unsafe static int GetAlignment<T>() where T : unmanaged
	{
		return GetAlignment(sizeof(T));
	}

	public static int GetAlignment(int stride)
	{
		if (stride % 16 == 0)
		{
			return 16;
		}
		if (stride % 8 == 0)
		{
			return 8;
		}
		if (stride % 4 == 0)
		{
			return 4;
		}
		return (stride % 2 != 0) ? 1 : 2;
	}

	public static int GetMaxAlignment(int a, int b)
	{
		return Math.Max(GetAlignment(a), GetAlignment(b));
	}

	public static int GetMaxAlignment(int a, int b, int c)
	{
		return Math.Max(GetMaxAlignment(a, b), GetAlignment(c));
	}

	public static int GetMaxAlignment(int a, int b, int c, int d)
	{
		return Math.Max(GetMaxAlignment(a, b, c), GetAlignment(d));
	}

	public static int GetMaxAlignment(int a, int b, int c, int d, int e)
	{
		return Math.Max(GetMaxAlignment(a, b, c, e), GetAlignment(e));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static byte* ReferenceToPointer<T>(ref T obj) where T : unmanaged
	{
		fixed (T* result = &obj)
		{
			return (byte*)result;
		}
	}

	[Conditional("ENABLE_NATIVE_ALLOC_SENTINELS")]
	private unsafe static void InitBlockSentinels(IntPtr memory, int size)
	{
		ulong num = 15705636252112664309uL;
		ulong num2 = 12580085127939517179uL;
		Span<byte> dst = new Span<byte>((void*)memory, 0);
		Span<byte> dst2 = new Span<byte>((byte*)(void*)memory + size, 0);
		new ReadOnlySpan<byte>(&num, 8).RepeatingCopyTo(dst);
		new ReadOnlySpan<byte>(&num2, 8).RepeatingCopyTo(dst2);
	}

	[Conditional("ENABLE_NATIVE_ALLOC_SENTINELS")]
	public unsafe static void ValidateBlockSentinels(IntPtr memory, int size)
	{
		ulong num = 15705636252112664309uL;
		ulong num2 = 12580085127939517179uL;
		ReadOnlySpan<byte> readOnlySpan = new ReadOnlySpan<byte>((void*)memory, 0);
		ReadOnlySpan<byte> readOnlySpan2 = new ReadOnlySpan<byte>((byte*)(void*)memory + size, 0);
		if (!readOnlySpan.RepeatingSequenceEqualTo(new ReadOnlySpan<byte>(&num, 8)))
		{
			InternalLogStreams.LogError?.Log("MSG600 Leading sentinel mismatch: " + BinUtils.BytesToHex(readOnlySpan));
		}
		if (!readOnlySpan2.RepeatingSequenceEqualTo(new ReadOnlySpan<byte>(&num2, 8)))
		{
			InternalLogStreams.LogError?.Log("MSG601 Trailing sentinel mismatch: " + BinUtils.BytesToHex(readOnlySpan2));
		}
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, out void* ptr0, out void* ptr1, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		int num = size0 + size1;
		byte* ptr2 = (byte*)(ptr0 = MallocAndClear(num)) + size0;
		ptr1 = ptr2;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, out void* ptr0, out void* ptr1, out void* ptr2, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		int num = size0 + size1 + size2;
		byte* ptr3 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1;
		ptr2 = ptr3;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		int num = size0 + size1 + size2 + size3;
		byte* ptr4 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2;
		ptr3 = ptr4;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, int size4, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, out void* ptr4, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		size4 = RoundToAlignment(size4, alignment);
		int num = size0 + size1 + size2 + size3 + size4;
		byte* ptr5 = (byte*)(ptr3 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2) + size3;
		ptr4 = ptr5;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		Assert.Check(IsPointerAligned(ptr4, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, int size4, int size5, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, out void* ptr4, out void* ptr5, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		size4 = RoundToAlignment(size4, alignment);
		size5 = RoundToAlignment(size5, alignment);
		int num = size0 + size1 + size2 + size3 + size4 + size5;
		byte* ptr6 = (byte*)(ptr4 = (byte*)(ptr3 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2) + size3) + size4;
		ptr5 = ptr6;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		Assert.Check(IsPointerAligned(ptr4, alignment));
		Assert.Check(IsPointerAligned(ptr5, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, int size4, int size5, int size6, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, out void* ptr4, out void* ptr5, out void* ptr6, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		size4 = RoundToAlignment(size4, alignment);
		size5 = RoundToAlignment(size5, alignment);
		size6 = RoundToAlignment(size6, alignment);
		int num = size0 + size1 + size2 + size3 + size4 + size5 + size6;
		byte* ptr7 = (byte*)(ptr5 = (byte*)(ptr4 = (byte*)(ptr3 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2) + size3) + size4) + size5;
		ptr6 = ptr7;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		Assert.Check(IsPointerAligned(ptr4, alignment));
		Assert.Check(IsPointerAligned(ptr5, alignment));
		Assert.Check(IsPointerAligned(ptr6, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, int size4, int size5, int size6, int size7, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, out void* ptr4, out void* ptr5, out void* ptr6, out void* ptr7, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		size4 = RoundToAlignment(size4, alignment);
		size5 = RoundToAlignment(size5, alignment);
		size6 = RoundToAlignment(size6, alignment);
		size7 = RoundToAlignment(size7, alignment);
		int num = size0 + size1 + size2 + size3 + size4 + size5 + size6 + size7;
		byte* ptr8 = (byte*)(ptr6 = (byte*)(ptr5 = (byte*)(ptr4 = (byte*)(ptr3 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2) + size3) + size4) + size5) + size6;
		ptr7 = ptr8;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		Assert.Check(IsPointerAligned(ptr4, alignment));
		Assert.Check(IsPointerAligned(ptr5, alignment));
		Assert.Check(IsPointerAligned(ptr6, alignment));
		Assert.Check(IsPointerAligned(ptr7, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, int size4, int size5, int size6, int size7, int size8, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, out void* ptr4, out void* ptr5, out void* ptr6, out void* ptr7, out void* ptr8, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		size4 = RoundToAlignment(size4, alignment);
		size5 = RoundToAlignment(size5, alignment);
		size6 = RoundToAlignment(size6, alignment);
		size7 = RoundToAlignment(size7, alignment);
		size8 = RoundToAlignment(size8, alignment);
		int num = size0 + size1 + size2 + size3 + size4 + size5 + size6 + size7 + size8;
		byte* ptr9 = (byte*)(ptr7 = (byte*)(ptr6 = (byte*)(ptr5 = (byte*)(ptr4 = (byte*)(ptr3 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2) + size3) + size4) + size5) + size6) + size7;
		ptr8 = ptr9;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		Assert.Check(IsPointerAligned(ptr4, alignment));
		Assert.Check(IsPointerAligned(ptr5, alignment));
		Assert.Check(IsPointerAligned(ptr6, alignment));
		Assert.Check(IsPointerAligned(ptr7, alignment));
		Assert.Check(IsPointerAligned(ptr8, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, int size4, int size5, int size6, int size7, int size8, int size9, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, out void* ptr4, out void* ptr5, out void* ptr6, out void* ptr7, out void* ptr8, out void* ptr9, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		size4 = RoundToAlignment(size4, alignment);
		size5 = RoundToAlignment(size5, alignment);
		size6 = RoundToAlignment(size6, alignment);
		size7 = RoundToAlignment(size7, alignment);
		size8 = RoundToAlignment(size8, alignment);
		size9 = RoundToAlignment(size9, alignment);
		int num = size0 + size1 + size2 + size3 + size4 + size5 + size6 + size7 + size8 + size9;
		byte* ptr10 = (byte*)(ptr8 = (byte*)(ptr7 = (byte*)(ptr6 = (byte*)(ptr5 = (byte*)(ptr4 = (byte*)(ptr3 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2) + size3) + size4) + size5) + size6) + size7) + size8;
		ptr9 = ptr10;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		Assert.Check(IsPointerAligned(ptr4, alignment));
		Assert.Check(IsPointerAligned(ptr5, alignment));
		Assert.Check(IsPointerAligned(ptr6, alignment));
		Assert.Check(IsPointerAligned(ptr7, alignment));
		Assert.Check(IsPointerAligned(ptr8, alignment));
		Assert.Check(IsPointerAligned(ptr9, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, int size4, int size5, int size6, int size7, int size8, int size9, int size10, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, out void* ptr4, out void* ptr5, out void* ptr6, out void* ptr7, out void* ptr8, out void* ptr9, out void* ptr10, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		size4 = RoundToAlignment(size4, alignment);
		size5 = RoundToAlignment(size5, alignment);
		size6 = RoundToAlignment(size6, alignment);
		size7 = RoundToAlignment(size7, alignment);
		size8 = RoundToAlignment(size8, alignment);
		size9 = RoundToAlignment(size9, alignment);
		size10 = RoundToAlignment(size10, alignment);
		int num = size0 + size1 + size2 + size3 + size4 + size5 + size6 + size7 + size8 + size9 + size10;
		byte* ptr11 = (byte*)(ptr9 = (byte*)(ptr8 = (byte*)(ptr7 = (byte*)(ptr6 = (byte*)(ptr5 = (byte*)(ptr4 = (byte*)(ptr3 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2) + size3) + size4) + size5) + size6) + size7) + size8) + size9;
		ptr10 = ptr11;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		Assert.Check(IsPointerAligned(ptr4, alignment));
		Assert.Check(IsPointerAligned(ptr5, alignment));
		Assert.Check(IsPointerAligned(ptr6, alignment));
		Assert.Check(IsPointerAligned(ptr7, alignment));
		Assert.Check(IsPointerAligned(ptr8, alignment));
		Assert.Check(IsPointerAligned(ptr9, alignment));
		Assert.Check(IsPointerAligned(ptr10, alignment));
		return num;
	}

	public unsafe static int MallocAndClearBlock(int size0, int size1, int size2, int size3, int size4, int size5, int size6, int size7, int size8, int size9, int size10, int size11, out void* ptr0, out void* ptr1, out void* ptr2, out void* ptr3, out void* ptr4, out void* ptr5, out void* ptr6, out void* ptr7, out void* ptr8, out void* ptr9, out void* ptr10, out void* ptr11, int alignment = 8)
	{
		size0 = RoundToAlignment(size0, alignment);
		size1 = RoundToAlignment(size1, alignment);
		size2 = RoundToAlignment(size2, alignment);
		size3 = RoundToAlignment(size3, alignment);
		size4 = RoundToAlignment(size4, alignment);
		size5 = RoundToAlignment(size5, alignment);
		size6 = RoundToAlignment(size6, alignment);
		size7 = RoundToAlignment(size7, alignment);
		size8 = RoundToAlignment(size8, alignment);
		size9 = RoundToAlignment(size9, alignment);
		size10 = RoundToAlignment(size10, alignment);
		size11 = RoundToAlignment(size11, alignment);
		int num = size0 + size1 + size2 + size3 + size4 + size5 + size6 + size7 + size8 + size9 + size10 + size11;
		byte* ptr12 = (byte*)(ptr10 = (byte*)(ptr9 = (byte*)(ptr8 = (byte*)(ptr7 = (byte*)(ptr6 = (byte*)(ptr5 = (byte*)(ptr4 = (byte*)(ptr3 = (byte*)(ptr2 = (byte*)(ptr1 = (byte*)(ptr0 = MallocAndClear(num)) + size0) + size1) + size2) + size3) + size4) + size5) + size6) + size7) + size8) + size9) + size10;
		ptr11 = ptr12;
		Assert.Check(IsPointerAligned(ptr0, alignment));
		Assert.Check(IsPointerAligned(ptr1, alignment));
		Assert.Check(IsPointerAligned(ptr2, alignment));
		Assert.Check(IsPointerAligned(ptr3, alignment));
		Assert.Check(IsPointerAligned(ptr4, alignment));
		Assert.Check(IsPointerAligned(ptr5, alignment));
		Assert.Check(IsPointerAligned(ptr6, alignment));
		Assert.Check(IsPointerAligned(ptr7, alignment));
		Assert.Check(IsPointerAligned(ptr8, alignment));
		Assert.Check(IsPointerAligned(ptr9, alignment));
		Assert.Check(IsPointerAligned(ptr10, alignment));
		Assert.Check(IsPointerAligned(ptr11, alignment));
		return num;
	}
}
