using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;

namespace Unity.Collections;

[Serializable]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(FixedBytes32Align8)
})]
internal struct FixedList<T, U> : INativeList<T>, IIndexable<T> where T : unmanaged where U : unmanaged
{
	[SerializeField]
	internal U data;

	internal unsafe ushort length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			fixed (U* ptr = &data)
			{
				void* ptr2 = ptr;
				return *(ushort*)ptr2;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			fixed (U* ptr = &data)
			{
				void* ptr2 = ptr;
				*(ushort*)ptr2 = value;
			}
		}
	}

	internal unsafe readonly byte* buffer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			fixed (U* ptr = &data)
			{
				void* ptr2 = ptr;
				return (byte*)ptr2 + UnsafeUtility.SizeOf<ushort>();
			}
		}
	}

	[CreateProperty]
	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return length;
		}
		set
		{
			length = (ushort)value;
		}
	}

	[CreateProperty]
	private IEnumerable<T> Elements => ToArray();

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Length == 0;
		}
	}

	internal readonly int LengthInBytes => Length * UnsafeUtility.SizeOf<T>();

	internal unsafe readonly byte* Buffer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return buffer + FixedList.PaddingBytes<T>();
		}
	}

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return FixedList.Capacity<U, T>();
		}
		set
		{
		}
	}

	public unsafe T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return UnsafeUtility.ReadArrayElement<T>(Buffer, CollectionHelper.AssumePositive(index));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			UnsafeUtility.WriteArrayElement(Buffer, CollectionHelper.AssumePositive(index), value);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref T ElementAt(int index)
	{
		return ref UnsafeUtility.ArrayElementAsRef<T>(Buffer, index);
	}

	public unsafe override int GetHashCode()
	{
		return (int)CollectionHelper.Hash(Buffer, LengthInBytes);
	}

	public void Add(in T item)
	{
		AddNoResize(in item);
	}

	public unsafe void AddRange(void* ptr, int length)
	{
		AddRangeNoResize(ptr, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void AddNoResize(in T item)
	{
		this[Length++] = item;
	}

	public unsafe void AddRangeNoResize(void* ptr, int length)
	{
		int num = Length;
		Length += length;
		UnsafeUtility.MemCpy(Buffer + (nint)num * (nint)sizeof(T), ptr, UnsafeUtility.SizeOf<T>() * length);
	}

	public unsafe void AddReplicate(in T value, int count)
	{
		int num = Length;
		Length += count;
		fixed (T* source = &value)
		{
			UnsafeUtility.MemCpyReplicate(Buffer + (nint)num * (nint)sizeof(T), source, UnsafeUtility.SizeOf<T>(), count);
		}
	}

	public void Clear()
	{
		Length = 0;
	}

	public unsafe void InsertRangeWithBeginEnd(int begin, int end)
	{
		int num = end - begin;
		if (num >= 1)
		{
			int num2 = length - begin;
			Length += num;
			if (num2 >= 1)
			{
				int num3 = num2 * UnsafeUtility.SizeOf<T>();
				byte* num4 = Buffer;
				byte* destination = num4 + end * UnsafeUtility.SizeOf<T>();
				byte* source = num4 + begin * UnsafeUtility.SizeOf<T>();
				UnsafeUtility.MemMove(destination, source, num3);
			}
		}
	}

	public void InsertRange(int index, int count)
	{
		InsertRangeWithBeginEnd(index, index + count);
	}

	public void Insert(int index, in T item)
	{
		InsertRangeWithBeginEnd(index, index + 1);
		this[index] = item;
	}

	public void RemoveAtSwapBack(int index)
	{
		RemoveRangeSwapBack(index, 1);
	}

	public unsafe void RemoveRangeSwapBack(int index, int count)
	{
		if (count > 0)
		{
			int num = math.max(Length - count, index + count);
			int num2 = UnsafeUtility.SizeOf<T>();
			void* destination = Buffer + index * num2;
			void* source = Buffer + num * num2;
			UnsafeUtility.MemCpy(destination, source, (Length - num) * num2);
			Length -= count;
		}
	}

	public void RemoveAt(int index)
	{
		RemoveRange(index, 1);
	}

	public unsafe void RemoveRange(int index, int count)
	{
		if (count > 0)
		{
			int num = math.min(index + count, Length);
			int num2 = UnsafeUtility.SizeOf<T>();
			void* destination = Buffer + index * num2;
			void* source = Buffer + num * num2;
			UnsafeUtility.MemCpy(destination, source, (Length - num) * num2);
			Length -= count;
		}
	}

	[ExcludeFromBurstCompatTesting("Returns managed array")]
	public unsafe T[] ToArray()
	{
		T[] array = new T[Length];
		byte* source = Buffer;
		fixed (T* destination = array)
		{
			UnsafeUtility.MemCpy(destination, source, LengthInBytes);
		}
		return array;
	}

	public unsafe NativeArray<T> ToNativeArray(AllocatorManager.AllocatorHandle allocator)
	{
		NativeArray<T> nativeArray = CollectionHelper.CreateNativeArray<T>(Length, allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeUtility.MemCpy(nativeArray.GetUnsafePtr(), Buffer, LengthInBytes);
		return nativeArray;
	}
}
[StructLayout(LayoutKind.Sequential, Size = 1)]
[GenerateTestsForBurstCompatibility]
internal struct FixedList
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal static int PaddingBytes<T>() where T : unmanaged
	{
		return math.max(0, math.min(6, (1 << math.tzcnt(UnsafeUtility.SizeOf<T>())) - 2));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	internal static int StorageBytes<BUFFER, T>() where BUFFER : unmanaged where T : unmanaged
	{
		return UnsafeUtility.SizeOf<BUFFER>() - UnsafeUtility.SizeOf<ushort>() - PaddingBytes<T>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	internal static int Capacity<BUFFER, T>() where BUFFER : unmanaged where T : unmanaged
	{
		return StorageBytes<BUFFER, T>() / UnsafeUtility.SizeOf<T>();
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckResize<BUFFER, T>(int newLength) where BUFFER : unmanaged where T : unmanaged
	{
		int num = Capacity<BUFFER, T>();
		if (newLength < 0 || newLength > num)
		{
			throw new IndexOutOfRangeException($"NewLength {newLength} is out of range of '{num}' Capacity.");
		}
	}
}
