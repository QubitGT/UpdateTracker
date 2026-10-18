using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

internal struct OVRNativeList<T> : IDisposable, IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T> where T : unmanaged
{
	private NativeArray<T> _array;

	private readonly Allocator _allocator;

	public int Count { get; private set; }

	public int Capacity => _array.Length;

	public bool IsCreated => _array.IsCreated;

	public unsafe T* Data
	{
		get
		{
			if (!IsCreated)
			{
				return null;
			}
			return (T*)_array.GetUnsafePtr();
		}
	}

	public T this[int index]
	{
		get
		{
			if (index >= Count)
			{
				throw new IndexOutOfRangeException(string.Format("{0} ({1}) must be less than {2} ({3}).", "index", index, "Count", Count));
			}
			return _array[index];
		}
		set
		{
			if (index >= Count)
			{
				throw new IndexOutOfRangeException(string.Format("{0} ({1}) must be less than {2} ({3}).", "index", index, "Count", Count));
			}
			_array[index] = value;
		}
	}

	public OVRNativeList(int? initialCapacity, Allocator allocator)
	{
		_array = (initialCapacity.HasValue ? new NativeArray<T>(initialCapacity.Value, allocator) : default(NativeArray<T>));
		_allocator = allocator;
		Count = 0;
	}

	public OVRNativeList(Allocator allocator)
	{
		_array = default(NativeArray<T>);
		_allocator = allocator;
		Count = 0;
	}

	public unsafe T* PtrToElementAt(int index)
	{
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index", index, "index must be greater than or equal to zero.");
		}
		if (index >= Count)
		{
			throw new ArgumentOutOfRangeException("index", index, string.Format("{0} must be less than {1} ({2}).", "index", "Count", Count));
		}
		return Data + index;
	}

	public NativeArray<T> AsNativeArray()
	{
		return _array.GetSubArray(0, Count);
	}

	public unsafe Span<T> AsSpan()
	{
		return new Span<T>(Data, Count);
	}

	public unsafe ReadOnlySpan<T> AsReadOnlySpan()
	{
		return new ReadOnlySpan<T>(Data, Count);
	}

	public NativeArray<T>.Enumerator GetEnumerator()
	{
		return AsNativeArray().GetEnumerator();
	}

	public void Add(T item)
	{
		EnsureCapacity(Count + 1);
		_array[Count++] = item;
	}

	public unsafe void AddRange(IEnumerable<T> collection)
	{
		OVREnumerable<T> oVREnumerable = collection.ToNonAlloc();
		if (oVREnumerable.TryGetCount(out var count))
		{
			EnsureCapacity(Count + count);
			if (collection is T[] array)
			{
				fixed (T* source = array)
				{
					UnsafeUtility.MemCpy(Data + Count, source, sizeof(T) * array.Length);
				}
				Count += count;
				return;
			}
		}
		foreach (T item in oVREnumerable)
		{
			Add(item);
		}
	}

	public void Clear()
	{
		Count = 0;
	}

	public void Dispose()
	{
		if (_array.IsCreated)
		{
			_array.Dispose();
			_array = default(NativeArray<T>);
		}
		Count = 0;
	}

	public JobHandle Dispose(JobHandle dependency)
	{
		Count = 0;
		return _array.Dispose(dependency);
	}

	public unsafe static implicit operator T*(OVRNativeList<T> list)
	{
		return list.Data;
	}

	public static implicit operator Span<T>(OVRNativeList<T> list)
	{
		return list.AsSpan();
	}

	public static implicit operator ReadOnlySpan<T>(OVRNativeList<T> list)
	{
		return list.AsReadOnlySpan();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private unsafe void EnsureCapacity(int capacity)
	{
		if (Capacity < capacity)
		{
			capacity = Math.Max(capacity, Math.Max(4, Capacity * 3 / 2));
			NativeArray<T> nativeArray = new NativeArray<T>(capacity, _allocator);
			if (_array.IsCreated)
			{
				UnsafeUtility.MemCpy(nativeArray.GetUnsafePtr(), _array.GetUnsafeReadOnlyPtr(), sizeof(T) * Count);
				_array.Dispose();
			}
			_array = nativeArray;
		}
	}
}
internal static class OVRNativeList
{
	public readonly struct CapacityHelper
	{
		private readonly int? _count;

		public CapacityHelper(int? count)
		{
			_count = count;
		}

		public OVRNativeList<T> AllocateEmpty<T>(Allocator allocator) where T : unmanaged
		{
			return new OVRNativeList<T>(_count, allocator);
		}
	}

	public static CapacityHelper WithSuggestedCapacityFrom<T>([NoEnumeration] IEnumerable<T> collection)
	{
		return new CapacityHelper(collection.ToNonAlloc().Count);
	}

	public static CapacityHelper WithSuggestedCapacityFrom<T>([NoEnumeration] IEnumerable<T> collection, out OVREnumerable<T> nonAllocatingEnumerable)
	{
		nonAllocatingEnumerable = collection.ToNonAlloc();
		return new CapacityHelper(nonAllocatingEnumerable.Count);
	}

	public static OVRNativeList<T> ToNativeList<T>(this IEnumerable<T> collection, Allocator allocator) where T : unmanaged
	{
		OVRNativeList<T> result = new OVRNativeList<T>(allocator);
		result.AddRange(collection);
		return result;
	}
}
