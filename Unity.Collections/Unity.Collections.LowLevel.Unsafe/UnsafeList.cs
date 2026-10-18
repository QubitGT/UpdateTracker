using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Jobs;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}, IsEmpty = {IsEmpty}")]
[DebuggerTypeProxy(typeof(UnsafeListTDebugView<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct UnsafeList<T> : INativeDisposable, IDisposable, INativeList<T>, IIndexable<T>, IEnumerable<T>, IEnumerable where T : unmanaged
{
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ReadOnly : IEnumerable<T>, IEnumerable
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe readonly T* Ptr;

		public readonly int Length;

		public unsafe readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return Ptr != null;
			}
		}

		public readonly bool IsEmpty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				if (IsCreated)
				{
					return Length == 0;
				}
				return true;
			}
		}

		internal unsafe ReadOnly(T* ptr, int length)
		{
			Ptr = ptr;
			Length = length;
		}

		public unsafe Enumerator GetEnumerator()
		{
			Enumerator result = default(Enumerator);
			result.m_Ptr = Ptr;
			result.m_Length = Length;
			result.m_Index = -1;
			return result;
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

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ParallelReader
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe readonly T* Ptr;

		public readonly int Length;

		internal unsafe ParallelReader(T* ptr, int length)
		{
			Ptr = ptr;
			Length = length;
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ParallelWriter
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe UnsafeList<T>* ListData;

		public unsafe readonly void* Ptr
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return ListData->Ptr;
			}
		}

		internal unsafe ParallelWriter(UnsafeList<T>* listData)
		{
			ListData = listData;
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe void AddNoResize(T value)
		{
			int index = Interlocked.Increment(ref ListData->m_length) - 1;
			UnsafeUtility.WriteArrayElement(ListData->Ptr, index, value);
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe void AddRangeNoResize(void* ptr, int count)
		{
			int num = Interlocked.Add(ref ListData->m_length, count) - count;
			void* destination = (byte*)ListData->Ptr + num * sizeof(T);
			UnsafeUtility.MemCpy(destination, ptr, count * sizeof(T));
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe void AddRangeNoResize(UnsafeList<T> list)
		{
			AddRangeNoResize(list.Ptr, list.Length);
		}
	}

	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		internal unsafe T* m_Ptr;

		internal int m_Length;

		internal int m_Index;

		public unsafe T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_Ptr[m_Index];
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			return ++m_Index < m_Length;
		}

		public void Reset()
		{
			m_Index = -1;
		}
	}

	[NativeDisableUnsafePtrRestriction]
	public unsafe T* Ptr;

	public int m_length;

	public int m_capacity;

	public AllocatorManager.AllocatorHandle Allocator;

	private readonly int padding;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return CollectionHelper.AssumePositive(m_length);
		}
		set
		{
			if (value > Capacity)
			{
				Resize(value);
			}
			else
			{
				m_length = value;
			}
		}
	}

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return CollectionHelper.AssumePositive(m_capacity);
		}
		set
		{
			SetCapacity(value);
		}
	}

	public unsafe T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Ptr[CollectionHelper.AssumePositive(index)];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			Ptr[CollectionHelper.AssumePositive(index)] = value;
		}
	}

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (IsCreated)
			{
				return m_length == 0;
			}
			return true;
		}
	}

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Ptr != null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe ref T ElementAt(int index)
	{
		return ref Ptr[CollectionHelper.AssumePositive(index)];
	}

	public unsafe UnsafeList(T* ptr, int length)
	{
		this = default(UnsafeList<T>);
		Ptr = ptr;
		m_length = length;
		m_capacity = length;
		Allocator = AllocatorManager.None;
	}

	public unsafe UnsafeList(int initialCapacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
	{
		Ptr = null;
		m_length = 0;
		m_capacity = 0;
		Allocator = allocator;
		padding = 0;
		SetCapacity(math.max(initialCapacity, 1));
		if (options == NativeArrayOptions.ClearMemory && Ptr != null)
		{
			int num = sizeof(T);
			UnsafeUtility.MemClear(Ptr, Capacity * num);
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(AllocatorManager.AllocatorHandle) })]
	internal unsafe static UnsafeList<T>* Create<U>(int initialCapacity, ref U allocator, NativeArrayOptions options) where U : unmanaged, AllocatorManager.IAllocator
	{
		UnsafeList<T>* intPtr = AllocatorManager.Allocate(ref allocator, default(UnsafeList<T>), 1);
		*intPtr = new UnsafeList<T>(initialCapacity, allocator.Handle, options);
		return intPtr;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(AllocatorManager.AllocatorHandle) })]
	internal unsafe static void Destroy<U>(UnsafeList<T>* listData, ref U allocator) where U : unmanaged, AllocatorManager.IAllocator
	{
		listData->Dispose(ref allocator);
		AllocatorManager.Free(ref allocator, listData, sizeof(UnsafeList<T>), UnsafeUtility.AlignOf<UnsafeList<T>>(), 1);
	}

	public unsafe static UnsafeList<T>* Create(int initialCapacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
	{
		UnsafeList<T>* intPtr = AllocatorManager.Allocate<UnsafeList<T>>(allocator);
		*intPtr = new UnsafeList<T>(initialCapacity, allocator, options);
		return intPtr;
	}

	public unsafe static void Destroy(UnsafeList<T>* listData)
	{
		AllocatorManager.AllocatorHandle allocator = listData->Allocator;
		listData->Dispose();
		AllocatorManager.Free(allocator, listData);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(AllocatorManager.AllocatorHandle) })]
	internal unsafe void Dispose<U>(ref U allocator) where U : unmanaged, AllocatorManager.IAllocator
	{
		AllocatorManager.Free(ref allocator, Ptr, m_capacity);
		Ptr = null;
		m_length = 0;
		m_capacity = 0;
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			if (CollectionHelper.ShouldDeallocate(Allocator))
			{
				AllocatorManager.Free(Allocator, Ptr, m_capacity);
				Allocator = AllocatorManager.Invalid;
			}
			Ptr = null;
			m_length = 0;
			m_capacity = 0;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		if (CollectionHelper.ShouldDeallocate(Allocator))
		{
			UnsafeDisposeJob jobData = default(UnsafeDisposeJob);
			jobData.Ptr = Ptr;
			jobData.Allocator = Allocator;
			JobHandle result = jobData.Schedule(inputDeps);
			Ptr = null;
			Allocator = AllocatorManager.Invalid;
			return result;
		}
		Ptr = null;
		return inputDeps;
	}

	public void Clear()
	{
		m_length = 0;
	}

	public unsafe void Resize(int length, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
	{
		int length2 = m_length;
		if (length > Capacity)
		{
			SetCapacity(length);
		}
		m_length = length;
		if (options == NativeArrayOptions.ClearMemory && length2 < length)
		{
			int num = length - length2;
			byte* ptr = (byte*)Ptr;
			int num2 = sizeof(T);
			UnsafeUtility.MemClear(ptr + length2 * num2, num * num2);
		}
	}

	private unsafe void ResizeExact<U>(ref U allocator, int newCapacity) where U : unmanaged, AllocatorManager.IAllocator
	{
		newCapacity = math.max(0, newCapacity);
		T* ptr = null;
		int alignOf = UnsafeUtility.AlignOf<T>();
		int num = sizeof(T);
		if (newCapacity > 0)
		{
			ptr = (T*)AllocatorManager.Allocate(ref allocator, num, alignOf, newCapacity);
			if (Ptr != null && m_capacity > 0)
			{
				int num2 = math.min(newCapacity, Capacity) * num;
				UnsafeUtility.MemCpy(ptr, Ptr, num2);
			}
		}
		AllocatorManager.Free(ref allocator, Ptr, Capacity);
		Ptr = ptr;
		m_capacity = newCapacity;
		m_length = math.min(m_length, newCapacity);
	}

	private void ResizeExact(int capacity)
	{
		ResizeExact(ref Allocator, capacity);
	}

	private unsafe void SetCapacity<U>(ref U allocator, int capacity) where U : unmanaged, AllocatorManager.IAllocator
	{
		int num = sizeof(T);
		int x = math.max(capacity, 64 / num);
		x = math.ceilpow2(x);
		if (x != Capacity)
		{
			ResizeExact(ref allocator, x);
		}
	}

	public void SetCapacity(int capacity)
	{
		SetCapacity(ref Allocator, capacity);
	}

	public void TrimExcess()
	{
		if (Capacity != m_length)
		{
			ResizeExact(m_length);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void AddNoResize(T value)
	{
		UnsafeUtility.WriteArrayElement(Ptr, m_length, value);
		m_length++;
	}

	public unsafe void AddRangeNoResize(void* ptr, int count)
	{
		int num = sizeof(T);
		void* destination = (byte*)Ptr + m_length * num;
		UnsafeUtility.MemCpy(destination, ptr, count * num);
		m_length += count;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe void AddRangeNoResize(UnsafeList<T> list)
	{
		AddRangeNoResize(list.Ptr, CollectionHelper.AssumePositive(list.Length));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Add(in T value)
	{
		int length = m_length;
		if (m_length < m_capacity)
		{
			Ptr[length] = value;
			m_length++;
		}
		else
		{
			Resize(length + 1);
			Ptr[length] = value;
		}
	}

	public unsafe void AddRange(void* ptr, int count)
	{
		int length = m_length;
		if (m_length + count > Capacity)
		{
			Resize(m_length + count);
		}
		else
		{
			m_length += count;
		}
		int num = sizeof(T);
		void* destination = (byte*)Ptr + length * num;
		UnsafeUtility.MemCpy(destination, ptr, count * num);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe void AddRange(UnsafeList<T> list)
	{
		AddRange(list.Ptr, list.Length);
	}

	public unsafe void AddReplicate(in T value, int count)
	{
		int length = m_length;
		if (m_length + count > Capacity)
		{
			Resize(m_length + count);
		}
		else
		{
			m_length += count;
		}
		fixed (T* ptr = &value)
		{
			void* source = ptr;
			UnsafeUtility.MemCpyReplicate(Ptr + length, source, UnsafeUtility.SizeOf<T>(), count);
		}
	}

	public unsafe void InsertRangeWithBeginEnd(int begin, int end)
	{
		begin = CollectionHelper.AssumePositive(begin);
		end = CollectionHelper.AssumePositive(end);
		int num = end - begin;
		if (num >= 1)
		{
			int length = m_length;
			if (m_length + num > Capacity)
			{
				Resize(m_length + num);
			}
			else
			{
				m_length += num;
			}
			int num2 = length - begin;
			if (num2 >= 1)
			{
				int num3 = sizeof(T);
				int num4 = num2 * num3;
				byte* ptr = (byte*)Ptr;
				byte* destination = ptr + end * num3;
				byte* source = ptr + begin * num3;
				UnsafeUtility.MemMove(destination, source, num4);
			}
		}
	}

	public void InsertRange(int index, int count)
	{
		InsertRangeWithBeginEnd(index, index + count);
	}

	public unsafe void RemoveAtSwapBack(int index)
	{
		index = CollectionHelper.AssumePositive(index);
		int num = m_length - 1;
		T* num2 = Ptr + index;
		T* ptr = Ptr + num;
		*num2 = *ptr;
		m_length--;
	}

	public unsafe void RemoveRangeSwapBack(int index, int count)
	{
		index = CollectionHelper.AssumePositive(index);
		count = CollectionHelper.AssumePositive(count);
		if (count > 0)
		{
			int num = math.max(m_length - count, index + count);
			int num2 = sizeof(T);
			void* destination = (byte*)Ptr + index * num2;
			void* source = (byte*)Ptr + num * num2;
			UnsafeUtility.MemCpy(destination, source, (m_length - num) * num2);
			m_length -= count;
		}
	}

	public unsafe void RemoveAt(int index)
	{
		index = CollectionHelper.AssumePositive(index);
		T* ptr = Ptr + index;
		T* ptr2 = ptr + 1;
		m_length--;
		for (int i = index; i < m_length; i++)
		{
			*(ptr++) = *(ptr2++);
		}
	}

	public unsafe void RemoveRange(int index, int count)
	{
		index = CollectionHelper.AssumePositive(index);
		count = CollectionHelper.AssumePositive(count);
		if (count > 0)
		{
			int num = math.min(index + count, m_length);
			int num2 = sizeof(T);
			void* destination = (byte*)Ptr + index * num2;
			void* source = (byte*)Ptr + num * num2;
			UnsafeUtility.MemCpy(destination, source, (m_length - num) * num2);
			m_length -= count;
		}
	}

	public unsafe ReadOnly AsReadOnly()
	{
		return new ReadOnly(Ptr, Length);
	}

	public unsafe ParallelReader AsParallelReader()
	{
		return new ParallelReader(Ptr, Length);
	}

	public unsafe ParallelWriter AsParallelWriter()
	{
		return new ParallelWriter((UnsafeList<T>*)UnsafeUtility.AddressOf(ref this));
	}

	public unsafe void CopyFrom(in NativeArray<T> other)
	{
		Resize(other.Length);
		UnsafeUtility.MemCpy(Ptr, other.GetUnsafeReadOnlyPtr(), UnsafeUtility.SizeOf<T>() * other.Length);
	}

	public unsafe void CopyFrom(in UnsafeList<T> other)
	{
		Resize(other.Length);
		UnsafeUtility.MemCpy(Ptr, other.Ptr, UnsafeUtility.SizeOf<T>() * other.Length);
	}

	public unsafe Enumerator GetEnumerator()
	{
		Enumerator result = default(Enumerator);
		result.m_Ptr = Ptr;
		result.m_Length = Length;
		result.m_Index = -1;
		return result;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal unsafe static void CheckNull(void* listData)
	{
		if (listData == null)
		{
			throw new InvalidOperationException("UnsafeList has yet to be created or has been destroyed!");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckIndexCount(int index, int count)
	{
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException($"Value for count {count} must be positive.");
		}
		if (index < 0)
		{
			throw new IndexOutOfRangeException($"Value for index {index} must be positive.");
		}
		if (index > Length)
		{
			throw new IndexOutOfRangeException($"Value for index {index} is out of bounds.");
		}
		if (index + count > Length)
		{
			throw new ArgumentOutOfRangeException($"Value for count {count} is out of bounds.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckBeginEndNoLength(int begin, int end)
	{
		if (begin > end)
		{
			throw new ArgumentException($"Value for begin {begin} index must less or equal to end {end}.");
		}
		if (begin < 0)
		{
			throw new ArgumentOutOfRangeException($"Value for begin {begin} must be positive.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckBeginEnd(int begin, int end)
	{
		if (begin > Length)
		{
			throw new ArgumentOutOfRangeException($"Value for begin {begin} is out of bounds.");
		}
		if (end > Length)
		{
			throw new ArgumentOutOfRangeException($"Value for end {end} is out of bounds.");
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckNoResizeHasEnoughCapacity(int length)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private void CheckNoResizeHasEnoughCapacity(int length, int index)
	{
		if (Capacity < index + length)
		{
			throw new InvalidOperationException($"AddNoResize assumes that list capacity is sufficient (Capacity {Capacity}, Length {Length}), requested length {length}!");
		}
	}
}
