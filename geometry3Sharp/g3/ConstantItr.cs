using System.Collections;
using System.Collections.Generic;

namespace g3;

public class ConstantItr<T> : IEnumerable<T>, IEnumerable
{
	public T ConstantValue;

	public int N;

	public ConstantItr(int count, T constant)
	{
		N = count;
		ConstantValue = constant;
	}

	public IEnumerator<T> GetEnumerator()
	{
		int i = 0;
		while (i < N)
		{
			yield return ConstantValue;
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
