using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[DebuggerDisplay("Length = {m_ListData == null ? default : m_ListData->Length}, Capacity = {m_ListData == null ? default : m_ListData->Capacity}")]
[DebuggerTypeProxy(typeof(NativeListDebugView<>))]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
public struct NativeList<T> : INativeDisposable, IDisposable, INativeList<T>, IIndexable<T>, IEnumerable<T>, IEnumerable where T : unmanaged
{
	[NativeContainer]
	[NativeContainerIsAtomicWriteOnly]
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

		public unsafe void AddNoResize(T value)
		{
			int index = Interlocked.Increment(ref ListData->m_length) - 1;
			UnsafeUtility.WriteArrayElement(ListData->Ptr, index, value);
		}

		public unsafe void AddRangeNoResize(void* ptr, int count)
		{
			int num = Interlocked.Add(ref ListData->m_length, count) - count;
			int num2 = sizeof(T);
			void* destination = (byte*)ListData->Ptr + num * num2;
			UnsafeUtility.MemCpy(destination, ptr, count * num2);
		}

		public unsafe void AddRangeNoResize(UnsafeList<T> list)
		{
			AddRangeNoResize(list.Ptr, list.Length);
		}

		public unsafe void AddRangeNoResize(NativeList<T> list)
		{
			AddRangeNoResize(*list.m_ListData);
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeList<T>* m_ListData;

	public unsafe T this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (*m_ListData)[index];
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			(*m_ListData)[index] = value;
		}
	}

	public unsafe int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return CollectionHelper.AssumePositive(m_ListData->Length);
		}
		set
		{
			m_ListData->Resize(value, NativeArrayOptions.ClearMemory);
		}
	}

	public unsafe int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		readonly get
		{
			return m_ListData->Capacity;
		}
		set
		{
			m_ListData->Capacity = value;
		}
	}

	public unsafe readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (m_ListData != null)
			{
				return m_ListData->Length == 0;
			}
			return true;
		}
	}

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_ListData != null;
		}
	}

	public NativeList(AllocatorManager.AllocatorHandle allocator)
		: this(1, allocator)
	{
	}

	public NativeList(int initialCapacity, AllocatorManager.AllocatorHandle allocator)
	{
		this = default(NativeList<T>);
		AllocatorManager.AllocatorHandle allocator2 = allocator;
		Initialize(initialCapacity, ref allocator2);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(AllocatorManager.AllocatorHandle) })]
	internal unsafe void Initialize<U>(int initialCapacity, ref U allocator) where U : unmanaged, AllocatorManager.IAllocator
	{
		m_ListData = UnsafeList<T>.Create(initialCapacity, ref allocator, NativeArrayOptions.UninitializedMemory);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(AllocatorManager.AllocatorHandle) })]
	internal static NativeList<T> New<U>(int initialCapacity, ref U allocator) where U : unmanaged, AllocatorManager.IAllocator
	{
		NativeList<T> result = default(NativeList<T>);
		result.Initialize(initialCapacity, ref allocator);
		return result;
	}

	public unsafe ref T ElementAt(int index)
	{
		return ref m_ListData->ElementAt(index);
	}

	public unsafe UnsafeList<T>* GetUnsafeList()
	{
		return m_ListData;
	}

	public unsafe void AddNoResize(T value)
	{
		m_ListData->AddNoResize(value);
	}

	public unsafe void AddRangeNoResize(void* ptr, int count)
	{
		m_ListData->AddRangeNoResize(ptr, count);
	}

	public unsafe void AddRangeNoResize(NativeList<T> list)
	{
		m_ListData->AddRangeNoResize(*list.m_ListData);
	}

	public unsafe void Add(in T value)
	{
		m_ListData->Add(in value);
	}

	public unsafe void AddRange(NativeArray<T> array)
	{
		AddRange(array.GetUnsafeReadOnlyPtr(), array.Length);
	}

	public unsafe void AddRange(void* ptr, int count)
	{
		m_ListData->AddRange(ptr, CollectionHelper.AssumePositive(count));
	}

	public unsafe void AddReplicate(in T value, int count)
	{
		m_ListData->AddReplicate(in value, CollectionHelper.AssumePositive(count));
	}

	public unsafe void InsertRangeWithBeginEnd(int begin, int end)
	{
		m_ListData->InsertRangeWithBeginEnd(begin, end);
	}

	public void InsertRange(int index, int count)
	{
		InsertRangeWithBeginEnd(index, index + count);
	}

	public unsafe void RemoveAtSwapBack(int index)
	{
		m_ListData->RemoveAtSwapBack(index);
	}

	public unsafe void RemoveRangeSwapBack(int index, int count)
	{
		m_ListData->RemoveRangeSwapBack(index, count);
	}

	public unsafe void RemoveAt(int index)
	{
		m_ListData->RemoveAt(index);
	}

	public unsafe void RemoveRange(int index, int count)
	{
		m_ListData->RemoveRange(index, count);
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			UnsafeList<T>.Destroy(m_ListData);
			m_ListData = null;
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(AllocatorManager.AllocatorHandle) })]
	internal unsafe void Dispose<U>(ref U allocator) where U : unmanaged, AllocatorManager.IAllocator
	{
		if (IsCreated)
		{
			UnsafeList<T>.Destroy(m_ListData, ref allocator);
			m_ListData = null;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		NativeListDisposeJob jobData = default(NativeListDisposeJob);
		jobData.Data = new NativeListDispose
		{
			m_ListData = (UntypedUnsafeList*)m_ListData
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_ListData = null;
		return result;
	}

	public unsafe void Clear()
	{
		m_ListData->Clear();
	}

	[Obsolete("Implicit cast from `NativeList<T>` to `NativeArray<T>` has been deprecated; Use '.AsArray()' method to do explicit cast instead.", false)]
	public static implicit operator NativeArray<T>(NativeList<T> nativeList)
	{
		return nativeList.AsArray();
	}

	public unsafe NativeArray<T> AsArray()
	{
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(m_ListData->Ptr, m_ListData->Length, Allocator.None);
	}

	public unsafe NativeArray<T> AsDeferredJobArray()
	{
		byte* listData = (byte*)m_ListData;
		listData++;
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(listData, 0, Allocator.Invalid);
	}

	public unsafe NativeArray<T> ToArray(AllocatorManager.AllocatorHandle allocator)
	{
		NativeArray<T> result = CollectionHelper.CreateNativeArray<T>(Length, allocator, NativeArrayOptions.UninitializedMemory);
		UnsafeUtility.MemCpy(result.m_Buffer, m_ListData->Ptr, Length * UnsafeUtility.SizeOf<T>());
		return result;
	}

	public unsafe void CopyFrom(in NativeArray<T> other)
	{
		m_ListData->CopyFrom(in other);
	}

	public unsafe void CopyFrom(in UnsafeList<T> other)
	{
		m_ListData->CopyFrom(in other);
	}

	public unsafe void CopyFrom(in NativeList<T> other)
	{
		CopyFrom(in *other.m_ListData);
	}

	public NativeArray<T>.Enumerator GetEnumerator()
	{
		NativeArray<T> array = AsArray();
		return new NativeArray<T>.Enumerator(ref array);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		throw new NotImplementedException();
	}

	public unsafe void Resize(int length, NativeArrayOptions options)
	{
		m_ListData->Resize(length, options);
	}

	public void ResizeUninitialized(int length)
	{
		Resize(length, NativeArrayOptions.UninitializedMemory);
	}

	public unsafe void SetCapacity(int capacity)
	{
		m_ListData->SetCapacity(capacity);
	}

	public unsafe void TrimExcess()
	{
		m_ListData->TrimExcess();
	}

	public unsafe NativeArray<T>.ReadOnly AsReadOnly()
	{
		return new NativeArray<T>.ReadOnly(m_ListData->Ptr, m_ListData->Length);
	}

	public unsafe NativeArray<T>.ReadOnly AsParallelReader()
	{
		return new NativeArray<T>.ReadOnly(m_ListData->Ptr, m_ListData->Length);
	}

	public unsafe ParallelWriter AsParallelWriter()
	{
		return new ParallelWriter(m_ListData);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckInitialCapacity(int initialCapacity)
	{
		if (initialCapacity < 0)
		{
			throw new ArgumentOutOfRangeException("initialCapacity", "Capacity must be >= 0");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckTotalSize(int initialCapacity, long totalSize)
	{
		if (totalSize > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("initialCapacity", $"Capacity * sizeof(T) cannot exceed {int.MaxValue} bytes");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckSufficientCapacity(int capacity, int length)
	{
		if (capacity < length)
		{
			throw new InvalidOperationException($"Length {length} exceeds Capacity {capacity}");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckIndexInRange(int value, int length)
	{
		if (value < 0)
		{
			throw new IndexOutOfRangeException($"Value {value} must be positive.");
		}
		if ((uint)value >= (uint)length)
		{
			throw new IndexOutOfRangeException($"Value {value} is out of range in NativeList of '{length}' Length.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckArgPositive(int value)
	{
		if (value < 0)
		{
			throw new ArgumentOutOfRangeException($"Value {value} must be positive.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private unsafe void CheckHandleMatches(AllocatorManager.AllocatorHandle handle)
	{
		if (m_ListData == null)
		{
			throw new ArgumentOutOfRangeException($"Allocator handle {handle} can't match because container is not initialized.");
		}
		if (m_ListData->Allocator.Index != handle.Index)
		{
			throw new ArgumentOutOfRangeException($"Allocator handle {handle} can't match because container handle index doesn't match.");
		}
		if (m_ListData->Allocator.Version != handle.Version)
		{
			throw new ArgumentOutOfRangeException($"Allocator handle {handle} matches container handle index, but has different version.");
		}
	}
}
