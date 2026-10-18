using System;
using System.Collections;
using System.Collections.Generic;

namespace g3;

public class RemapItr<T, T2> : IEnumerable<T>, IEnumerable
{
	public IEnumerable<T2> OtherItr;

	public Func<T2, T> ValueF;

	public RemapItr(IEnumerable<T2> otherIterator, Func<T2, T> valueFunction)
	{
		OtherItr = otherIterator;
		ValueF = valueFunction;
	}

	public IEnumerator<T> GetEnumerator()
	{
		foreach (T2 item in OtherItr)
		{
			yield return ValueF(item);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
