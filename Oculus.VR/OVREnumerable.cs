using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

internal readonly struct OVREnumerable<T> : IEnumerable<T>, IEnumerable
{
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private enum CollectionType
		{
			None,
			ReadOnlyList,
			List,
			Set,
			Queue,
			Enumerable
		}

		private int _listIndex;

		private readonly CollectionType _type;

		private readonly int _listCount;

		private readonly IEnumerator<T> _enumerator;

		private readonly IReadOnlyList<T> _readOnlyList;

		private HashSet<T>.Enumerator _setEnumerator;

		private Queue<T>.Enumerator _queueEnumerator;

		private List<T>.Enumerator _listEnumerator;

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return _type switch
				{
					CollectionType.List => _listEnumerator.Current, 
					CollectionType.ReadOnlyList => _readOnlyList[_listIndex], 
					CollectionType.Set => _setEnumerator.Current, 
					CollectionType.Queue => _queueEnumerator.Current, 
					CollectionType.Enumerable => _enumerator.Current, 
					_ => throw new InvalidOperationException($"Unsupported collection type {_type}."), 
				};
			}
		}

		object IEnumerator.Current => Current;

		public Enumerator(IEnumerable<T> enumerable)
		{
			_setEnumerator = default(HashSet<T>.Enumerator);
			_queueEnumerator = default(Queue<T>.Enumerator);
			_listEnumerator = default(List<T>.Enumerator);
			_enumerator = null;
			_readOnlyList = null;
			_listIndex = -1;
			_listCount = 0;
			if (enumerable != null)
			{
				if (!(enumerable is List<T> list))
				{
					if (!(enumerable is IReadOnlyList<T> readOnlyList))
					{
						if (!(enumerable is HashSet<T> hashSet))
						{
							if (enumerable is Queue<T> queue)
							{
								_queueEnumerator = queue.GetEnumerator();
								_type = CollectionType.Queue;
							}
							else
							{
								_enumerator = enumerable.GetEnumerator();
								_type = CollectionType.Enumerable;
							}
						}
						else
						{
							_setEnumerator = hashSet.GetEnumerator();
							_type = CollectionType.Set;
						}
					}
					else
					{
						_readOnlyList = readOnlyList;
						_listCount = readOnlyList.Count;
						_type = CollectionType.ReadOnlyList;
					}
				}
				else
				{
					_listEnumerator = list.GetEnumerator();
					_type = CollectionType.List;
				}
			}
			else
			{
				_type = CollectionType.None;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			return _type switch
			{
				CollectionType.None => false, 
				CollectionType.List => _listEnumerator.MoveNext(), 
				CollectionType.ReadOnlyList => MoveNextReadOnlyList(), 
				CollectionType.Set => _setEnumerator.MoveNext(), 
				CollectionType.Queue => _queueEnumerator.MoveNext(), 
				CollectionType.Enumerable => _enumerator.MoveNext(), 
				_ => throw new InvalidOperationException($"Unsupported collection type {_type}."), 
			};
		}

		private bool MoveNextReadOnlyList()
		{
			ValidateAndThrow();
			return ++_listIndex < _listCount;
		}

		public void Reset()
		{
			switch (_type)
			{
			case CollectionType.ReadOnlyList:
				ValidateAndThrow();
				_listIndex = -1;
				break;
			case CollectionType.Enumerable:
				_enumerator.Reset();
				break;
			case CollectionType.List:
			case CollectionType.Set:
			case CollectionType.Queue:
				break;
			}
		}

		public void Dispose()
		{
			switch (_type)
			{
			case CollectionType.List:
				_listEnumerator.Dispose();
				break;
			case CollectionType.Set:
				_setEnumerator.Dispose();
				break;
			case CollectionType.Queue:
				_queueEnumerator.Dispose();
				break;
			case CollectionType.Enumerable:
				_enumerator.Dispose();
				break;
			case CollectionType.ReadOnlyList:
				break;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ValidateAndThrow()
		{
			if (_listCount != _readOnlyList.Count)
			{
				throw new InvalidOperationException("The list changed length during enumeration.");
			}
		}
	}

	private readonly IEnumerable<T> _enumerable;

	public int? Count
	{
		get
		{
			IEnumerable<T> enumerable = _enumerable;
			if (enumerable != null)
			{
				if (!(enumerable is ICollection collection))
				{
					if (!(enumerable is ICollection<T> collection2))
					{
						if (enumerable is IReadOnlyCollection<T> readOnlyCollection)
						{
							return readOnlyCollection.Count;
						}
						return null;
					}
					return collection2.Count;
				}
				return collection.Count;
			}
			return 0;
		}
	}

	public OVREnumerable(IEnumerable<T> enumerable)
	{
		_enumerable = enumerable;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(_enumerable);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool TryGetCount(out int count)
	{
		int? count2 = Count;
		count = count2.GetValueOrDefault();
		return count2.HasValue;
	}

	[Obsolete("This method may enumerate the collection. Consider Count or TryGetCount instead.")]
	public int GetCount()
	{
		if (!TryGetCount(out var count))
		{
			count = 0;
			foreach (T item in _enumerable)
			{
				_ = item;
				count++;
			}
		}
		return count;
	}
}
internal static class OVREnumerable
{
	public unsafe static int CopyTo<T>(this OVREnumerable<T> enumerable, T* memory) where T : unmanaged
	{
		int result = 0;
		foreach (T item in enumerable)
		{
			memory[result++] = item;
		}
		return result;
	}
}
