using System;
using System.Collections;
using System.Collections.Generic;

namespace Oculus.Platform.Models;

public class DeserializableList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	protected List<T> _Data;

	protected string _NextUrl;

	protected string _PreviousUrl;

	public int Count => _Data.Count;

	bool ICollection<T>.IsReadOnly => ((ICollection<T>)_Data).IsReadOnly;

	public T this[int index]
	{
		get
		{
			return _Data[index];
		}
		set
		{
			_Data[index] = value;
		}
	}

	[Obsolete("Use IList interface on the DeserializableList object instead.", false)]
	public List<T> Data => _Data;

	public bool HasNextPage => !string.IsNullOrEmpty(NextUrl);

	public bool HasPreviousPage => !string.IsNullOrEmpty(PreviousUrl);

	public string NextUrl => _NextUrl;

	public string PreviousUrl => _PreviousUrl;

	public int IndexOf(T obj)
	{
		return _Data.IndexOf(obj);
	}

	public void Add(T item)
	{
		_Data.Add(item);
	}

	public void Clear()
	{
		_Data.Clear();
	}

	public bool Contains(T item)
	{
		return _Data.Contains(item);
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		_Data.CopyTo(array, arrayIndex);
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _Data.GetEnumerator();
	}

	public void Insert(int index, T item)
	{
		_Data.Insert(index, item);
	}

	public bool Remove(T item)
	{
		return _Data.Remove(item);
	}

	public void RemoveAt(int index)
	{
		_Data.RemoveAt(index);
	}

	private IEnumerator GetEnumerator1()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator1();
	}
}
