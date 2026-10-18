using System.Collections;
using System.Collections.Generic;

namespace Cysharp.Text;

internal readonly struct ReadOnlyListAdaptor<T> : IReadOnlyList<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
{
	private readonly IList<T> _list;

	public T this[int index] => _list[index];

	public int Count => _list.Count;

	public ReadOnlyListAdaptor(IList<T> list)
	{
		_list = list;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
