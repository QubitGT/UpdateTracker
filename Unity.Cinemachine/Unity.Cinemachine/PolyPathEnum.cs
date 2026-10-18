using System;
using System.Collections;
using System.Collections.Generic;

namespace Unity.Cinemachine;

internal class PolyPathEnum : IEnumerator
{
	public List<PolyPathBase> _ppbList;

	private int position = -1;

	public PolyPathBase Current
	{
		get
		{
			if (position < 0 || position >= _ppbList.Count)
			{
				throw new InvalidOperationException();
			}
			return _ppbList[position];
		}
	}

	object IEnumerator.Current => Current;

	public PolyPathEnum(List<PolyPathBase> childs)
	{
		_ppbList = childs;
	}

	public bool MoveNext()
	{
		position++;
		return position < _ppbList.Count;
	}

	public void Reset()
	{
		position = -1;
	}
}
