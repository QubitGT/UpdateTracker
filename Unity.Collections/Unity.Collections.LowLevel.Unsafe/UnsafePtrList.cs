using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Jobs;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerDisplay("Length = {Length}, Capacity = {Capacity}, IsCreated = {IsCreated}, IsEmpty = {IsEmpty}")]
[DebuggerTypeProxy(typeof(UnsafePtrListDebugView<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct UnsafePtrList<T> : INativeDisposable, IDisposable, IEnumerable<IntPtr>, IEnumerable where T : unmanaged
{
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ReadOnly
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe readonly T** Ptr;

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

		internal unsafe ReadOnly(T** ptr, int length)
		{
			Ptr = ptr;
			Length = length;
		}

		public unsafe int IndexOf(void* ptr)
		{
			for (int i = 0; i < Length; i++)
			{
				if (Ptr[i] == ptr)
				{
					return i;
				}
			}
			return -1;
		}

		public unsafe bool Contains(void* ptr)
		{
			return IndexOf(ptr) != -1;
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ParallelReader
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe readonly T** Ptr;

		public readonly int Length;

		internal unsafe ParallelReader(T** ptr, int length)
		{
			Ptr = ptr;
			Length = length;
		}

		public unsafe int IndexOf(void* ptr)
		{
			for (int i = 0; i < Length; i++)
			{
				if (Ptr[i] == ptr)
				{
					return i;
				}
			}
			return -1;
		}

		public unsafe bool Contains(void* ptr)
		{
			return IndexOf(ptr) != -1;
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public struct ParallelWriter
	{
		[NativeDisableUnsafePtrRestriction]
		public unsafe readonly T** Ptr;

		[NativeDisableUnsafePtrRestriction]
		public unsafe UnsafeList<IntPtr>* ListData;

		internal unsafe ParallelWriter(T** ptr, UnsafeList<IntPtr>* listData)
		{
			Ptr = ptr;
			ListData = listData;
		}

		public unsafe void AddNoResize(T* value)
		{
			ListData->AddNoResize((IntPtr)value);
		}

		public unsafe void AddRangeNoResize(T** ptr, int count)
		{
			ListData->AddRangeNoResize(ptr, count);
		}

		public unsafe void AddRangeNoResize(UnsafePtrList<T> list)
		{
			ListData->AddRangeNoResize(list.Ptr, list.Length);
		}
	}

	[NativeDisableUnsafePtrRestriction]
	public unsafe readonly T** Ptr;

	public readonly int m_length;

	public readonly int m_capacity;

	public readonly AllocatorManager.AllocatorHandle Allocator;

	private readonly int padding;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return this.ListDataRO().Length;
		}
		set
		{
			UnsafePtrListExtensions.ListData(ref this).Length = value;
		}
	}

	public int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return this.ListDataRO().Capacity;
		}
		set
		{
			UnsafePtrListExtensions.ListData(ref this).Capacity = value;
		}
	}

	public unsafe T* this[int index]
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
				return Length == 0;
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
	public unsafe ref T* ElementAt(int index)
	{
		return ref Ptr[CollectionHelper.AssumePositive(index)];
	}

	public unsafe UnsafePtrList(T** ptr, int length)
	{
		this = default(UnsafePtrList<T>);
		Ptr = ptr;
		m_length = length;
		m_capacity = length;
		Allocator = AllocatorManager.None;
	}

	public unsafe UnsafePtrList(int initialCapacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
	{
		Ptr = null;
		m_length = 0;
		m_capacity = 0;
		padding = 0;
		Allocator = AllocatorManager.None;
		UnsafePtrListExtensions.ListData(ref this) = new UnsafeList<IntPtr>(initialCapacity, allocator, options);
	}

	public unsafe static UnsafePtrList<T>* Create(T** ptr, int length)
	{
		UnsafePtrList<T>* intPtr = AllocatorManager.Allocate<UnsafePtrList<T>>(AllocatorManager.Persistent);
		*intPtr = new UnsafePtrList<T>(ptr, length);
		return intPtr;
	}

	public unsafe static UnsafePtrList<T>* Create(int initialCapacity, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
	{
		UnsafePtrList<T>* intPtr = AllocatorManager.Allocate<UnsafePtrList<T>>(allocator);
		*intPtr = new UnsafePtrList<T>(initialCapacity, allocator, options);
		return intPtr;
	}

	public unsafe static void Destroy(UnsafePtrList<T>* listData)
	{
		AllocatorManager.AllocatorHandle handle = ((UnsafePtrListExtensions.ListData(ref *listData).Allocator.Value == AllocatorManager.Invalid.Value) ? AllocatorManager.Persistent : UnsafePtrListExtensions.ListData(ref *listData).Allocator);
		listData->Dispose();
		AllocatorManager.Free(handle, listData);
	}

	public void Dispose()
	{
		UnsafePtrListExtensions.ListData(ref this).Dispose();
	}

	public JobHandle Dispose(JobHandle inputDeps)
	{
		return UnsafePtrListExtensions.ListData(ref this).Dispose(inputDeps);
	}

	public void Clear()
	{
		UnsafePtrListExtensions.ListData(ref this).Clear();
	}

	public void Resize(int length, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
	{
		UnsafePtrListExtensions.ListData(ref this).Resize(length, options);
	}

	public void SetCapacity(int capacity)
	{
		UnsafePtrListExtensions.ListData(ref this).SetCapacity(capacity);
	}

	public void TrimExcess()
	{
		UnsafePtrListExtensions.ListData(ref this).TrimExcess();
	}

	public unsafe int IndexOf(void* ptr)
	{
		for (int i = 0; i < Length; i++)
		{
			if (Ptr[i] == ptr)
			{
				return i;
			}
		}
		return -1;
	}

	public unsafe bool Contains(void* ptr)
	{
		return IndexOf(ptr) != -1;
	}

	public unsafe void AddNoResize(void* value)
	{
		UnsafePtrListExtensions.ListData(ref this).AddNoResize((IntPtr)value);
	}

	public unsafe void AddRangeNoResize(void** ptr, int count)
	{
		UnsafePtrListExtensions.ListData(ref this).AddRangeNoResize(ptr, count);
	}

	public unsafe void AddRangeNoResize(UnsafePtrList<T> list)
	{
		UnsafePtrListExtensions.ListData(ref this).AddRangeNoResize(list.Ptr, list.Length);
	}

	public void Add(in IntPtr value)
	{
		UnsafePtrListExtensions.ListData(ref this).Add(in value);
	}

	public unsafe void Add(void* value)
	{
		ref UnsafeList<IntPtr> reference = ref UnsafePtrListExtensions.ListData(ref this);
		IntPtr value2 = (IntPtr)value;
		reference.Add(in value2);
	}

	public unsafe void AddRange(void* ptr, int length)
	{
		UnsafePtrListExtensions.ListData(ref this).AddRange(ptr, length);
	}

	public void AddRange(UnsafePtrList<T> list)
	{
		UnsafePtrListExtensions.ListData(ref this).AddRange(UnsafePtrListExtensions.ListData(ref list));
	}

	public void InsertRangeWithBeginEnd(int begin, int end)
	{
		UnsafePtrListExtensions.ListData(ref this).InsertRangeWithBeginEnd(begin, end);
	}

	public void RemoveAtSwapBack(int index)
	{
		UnsafePtrListExtensions.ListData(ref this).RemoveAtSwapBack(index);
	}

	public void RemoveRangeSwapBack(int index, int count)
	{
		UnsafePtrListExtensions.ListData(ref this).RemoveRangeSwapBack(index, count);
	}

	public void RemoveAt(int index)
	{
		UnsafePtrListExtensions.ListData(ref this).RemoveAt(index);
	}

	public void RemoveRange(int index, int count)
	{
		UnsafePtrListExtensions.ListData(ref this).RemoveRange(index, count);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	IEnumerator<IntPtr> IEnumerable<IntPtr>.GetEnumerator()
	{
		throw new NotImplementedException();
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
		return new ParallelWriter(Ptr, (UnsafeList<IntPtr>*)UnsafeUtility.AddressOf(ref this));
	}
}
