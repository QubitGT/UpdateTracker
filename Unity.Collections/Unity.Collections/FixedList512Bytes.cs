using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Properties;
using UnityEngine;

namespace Unity.Collections;

[Serializable]
[DebuggerTypeProxy(typeof(FixedList512BytesDebugView<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct FixedList512Bytes<T> : INativeList<T>, IIndexable<T>, IEnumerable<T>, IEnumerable, IEquatable<FixedList32Bytes<T>>, IComparable<FixedList32Bytes<T>>, IEquatable<FixedList64Bytes<T>>, IComparable<FixedList64Bytes<T>>, IEquatable<FixedList128Bytes<T>>, IComparable<FixedList128Bytes<T>>, IEquatable<FixedList512Bytes<T>>, IComparable<FixedList512Bytes<T>>, IEquatable<FixedList4096Bytes<T>>, IComparable<FixedList4096Bytes<T>> where T : unmanaged
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private FixedList512Bytes<T> m_List;

		private int m_Index;

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_List[m_Index];
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(ref FixedList512Bytes<T> list)
		{
			m_List = list;
			m_Index = -1;
		}

		public void Dispose()
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			m_Index++;
			return m_Index < m_List.Length;
		}

		public void Reset()
		{
			m_Index = -1;
		}
	}

	[SerializeField]
	internal FixedBytes512Align8 data;

	internal unsafe ushort length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			fixed (FixedBytes512Align8* ptr = &data)
			{
				void* ptr2 = ptr;
				return *(ushort*)ptr2;
			}
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			fixed (FixedBytes512Align8* ptr = &data)
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
			fixed (FixedBytes512Align8* ptr = &data)
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

	internal int LengthInBytes => Length * UnsafeUtility.SizeOf<T>();

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
			return FixedList.Capacity<FixedBytes512Align8, T>();
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

	public unsafe static bool operator ==(in FixedList512Bytes<T> a, in FixedList32Bytes<T> b)
	{
		if (a.length != b.length)
		{
			return false;
		}
		return UnsafeUtility.MemCmp(a.Buffer, b.Buffer, a.LengthInBytes) == 0;
	}

	public static bool operator !=(in FixedList512Bytes<T> a, in FixedList32Bytes<T> b)
	{
		return !(a == b);
	}

	public unsafe int CompareTo(FixedList32Bytes<T> other)
	{
		byte* num = buffer;
		byte* ptr = other.buffer;
		byte* ptr2 = num + FixedList.PaddingBytes<T>();
		byte* ptr3 = ptr + FixedList.PaddingBytes<T>();
		int num2 = math.min(Length, other.Length);
		for (int i = 0; i < num2; i++)
		{
			int num3 = UnsafeUtility.MemCmp(ptr2 + sizeof(T) * i, ptr3 + sizeof(T) * i, sizeof(T));
			if (num3 != 0)
			{
				return num3;
			}
		}
		return Length.CompareTo(other.Length);
	}

	public bool Equals(FixedList32Bytes<T> other)
	{
		return CompareTo(other) == 0;
	}

	public FixedList512Bytes(in FixedList32Bytes<T> other)
	{
		this = default(FixedList512Bytes<T>);
		Initialize(in other);
	}

	internal unsafe int Initialize(in FixedList32Bytes<T> other)
	{
		if (other.Length > Capacity)
		{
			return 1;
		}
		length = other.length;
		UnsafeUtility.MemCpy(Buffer, other.Buffer, LengthInBytes);
		return 0;
	}

	public static implicit operator FixedList512Bytes<T>(in FixedList32Bytes<T> other)
	{
		return new FixedList512Bytes<T>(in other);
	}

	public unsafe static bool operator ==(in FixedList512Bytes<T> a, in FixedList64Bytes<T> b)
	{
		if (a.length != b.length)
		{
			return false;
		}
		return UnsafeUtility.MemCmp(a.Buffer, b.Buffer, a.LengthInBytes) == 0;
	}

	public static bool operator !=(in FixedList512Bytes<T> a, in FixedList64Bytes<T> b)
	{
		return !(a == b);
	}

	public unsafe int CompareTo(FixedList64Bytes<T> other)
	{
		byte* num = buffer;
		byte* ptr = other.buffer;
		byte* ptr2 = num + FixedList.PaddingBytes<T>();
		byte* ptr3 = ptr + FixedList.PaddingBytes<T>();
		int num2 = math.min(Length, other.Length);
		for (int i = 0; i < num2; i++)
		{
			int num3 = UnsafeUtility.MemCmp(ptr2 + sizeof(T) * i, ptr3 + sizeof(T) * i, sizeof(T));
			if (num3 != 0)
			{
				return num3;
			}
		}
		return Length.CompareTo(other.Length);
	}

	public bool Equals(FixedList64Bytes<T> other)
	{
		return CompareTo(other) == 0;
	}

	public FixedList512Bytes(in FixedList64Bytes<T> other)
	{
		this = default(FixedList512Bytes<T>);
		Initialize(in other);
	}

	internal unsafe int Initialize(in FixedList64Bytes<T> other)
	{
		if (other.Length > Capacity)
		{
			return 1;
		}
		length = other.length;
		UnsafeUtility.MemCpy(Buffer, other.Buffer, LengthInBytes);
		return 0;
	}

	public static implicit operator FixedList512Bytes<T>(in FixedList64Bytes<T> other)
	{
		return new FixedList512Bytes<T>(in other);
	}

	public unsafe static bool operator ==(in FixedList512Bytes<T> a, in FixedList128Bytes<T> b)
	{
		if (a.length != b.length)
		{
			return false;
		}
		return UnsafeUtility.MemCmp(a.Buffer, b.Buffer, a.LengthInBytes) == 0;
	}

	public static bool operator !=(in FixedList512Bytes<T> a, in FixedList128Bytes<T> b)
	{
		return !(a == b);
	}

	public unsafe int CompareTo(FixedList128Bytes<T> other)
	{
		byte* num = buffer;
		byte* ptr = other.buffer;
		byte* ptr2 = num + FixedList.PaddingBytes<T>();
		byte* ptr3 = ptr + FixedList.PaddingBytes<T>();
		int num2 = math.min(Length, other.Length);
		for (int i = 0; i < num2; i++)
		{
			int num3 = UnsafeUtility.MemCmp(ptr2 + sizeof(T) * i, ptr3 + sizeof(T) * i, sizeof(T));
			if (num3 != 0)
			{
				return num3;
			}
		}
		return Length.CompareTo(other.Length);
	}

	public bool Equals(FixedList128Bytes<T> other)
	{
		return CompareTo(other) == 0;
	}

	public FixedList512Bytes(in FixedList128Bytes<T> other)
	{
		this = default(FixedList512Bytes<T>);
		Initialize(in other);
	}

	internal unsafe int Initialize(in FixedList128Bytes<T> other)
	{
		if (other.Length > Capacity)
		{
			return 1;
		}
		length = other.length;
		UnsafeUtility.MemCpy(Buffer, other.Buffer, LengthInBytes);
		return 0;
	}

	public static implicit operator FixedList512Bytes<T>(in FixedList128Bytes<T> other)
	{
		return new FixedList512Bytes<T>(in other);
	}

	public unsafe static bool operator ==(in FixedList512Bytes<T> a, in FixedList512Bytes<T> b)
	{
		if (a.length != b.length)
		{
			return false;
		}
		return UnsafeUtility.MemCmp(a.Buffer, b.Buffer, a.LengthInBytes) == 0;
	}

	public static bool operator !=(in FixedList512Bytes<T> a, in FixedList512Bytes<T> b)
	{
		return !(a == b);
	}

	public unsafe int CompareTo(FixedList512Bytes<T> other)
	{
		byte* num = buffer;
		byte* ptr = other.buffer;
		byte* ptr2 = num + FixedList.PaddingBytes<T>();
		byte* ptr3 = ptr + FixedList.PaddingBytes<T>();
		int num2 = math.min(Length, other.Length);
		for (int i = 0; i < num2; i++)
		{
			int num3 = UnsafeUtility.MemCmp(ptr2 + sizeof(T) * i, ptr3 + sizeof(T) * i, sizeof(T));
			if (num3 != 0)
			{
				return num3;
			}
		}
		return Length.CompareTo(other.Length);
	}

	public bool Equals(FixedList512Bytes<T> other)
	{
		return CompareTo(other) == 0;
	}

	public unsafe static bool operator ==(in FixedList512Bytes<T> a, in FixedList4096Bytes<T> b)
	{
		if (a.length != b.length)
		{
			return false;
		}
		return UnsafeUtility.MemCmp(a.Buffer, b.Buffer, a.LengthInBytes) == 0;
	}

	public static bool operator !=(in FixedList512Bytes<T> a, in FixedList4096Bytes<T> b)
	{
		return !(a == b);
	}

	public unsafe int CompareTo(FixedList4096Bytes<T> other)
	{
		byte* num = buffer;
		byte* ptr = other.buffer;
		byte* ptr2 = num + FixedList.PaddingBytes<T>();
		byte* ptr3 = ptr + FixedList.PaddingBytes<T>();
		int num2 = math.min(Length, other.Length);
		for (int i = 0; i < num2; i++)
		{
			int num3 = UnsafeUtility.MemCmp(ptr2 + sizeof(T) * i, ptr3 + sizeof(T) * i, sizeof(T));
			if (num3 != 0)
			{
				return num3;
			}
		}
		return Length.CompareTo(other.Length);
	}

	public bool Equals(FixedList4096Bytes<T> other)
	{
		return CompareTo(other) == 0;
	}

	public FixedList512Bytes(in FixedList4096Bytes<T> other)
	{
		this = default(FixedList512Bytes<T>);
		Initialize(in other);
	}

	internal unsafe int Initialize(in FixedList4096Bytes<T> other)
	{
		if (other.Length > Capacity)
		{
			return 1;
		}
		length = other.length;
		UnsafeUtility.MemCpy(Buffer, other.Buffer, LengthInBytes);
		return 0;
	}

	public static implicit operator FixedList512Bytes<T>(in FixedList4096Bytes<T> other)
	{
		return new FixedList512Bytes<T>(in other);
	}

	[ExcludeFromBurstCompatTesting("Takes managed object")]
	public override bool Equals(object obj)
	{
		if (obj is FixedList32Bytes<T> other)
		{
			return Equals(other);
		}
		if (obj is FixedList64Bytes<T> other2)
		{
			return Equals(other2);
		}
		if (obj is FixedList128Bytes<T> other3)
		{
			return Equals(other3);
		}
		if (obj is FixedList512Bytes<T> other4)
		{
			return Equals(other4);
		}
		if (obj is FixedList4096Bytes<T> other5)
		{
			return Equals(other5);
		}
		return false;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(ref this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		throw new NotImplementedException();
	}
}
