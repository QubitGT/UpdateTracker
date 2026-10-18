using System.Collections;
using System.Collections.Generic;

namespace Unity.Cinemachine;

internal abstract class PolyPathBase : IEnumerable
{
	internal PolyPathBase? _parent;

	internal List<PolyPathBase> _childs = new List<PolyPathBase>();

	public bool IsHole => GetIsHole();

	public int Count => _childs.Count;

	public PolyPathEnum GetEnumerator()
	{
		return new PolyPathEnum(_childs);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public PolyPathBase(PolyPathBase? parent = null)
	{
		_parent = parent;
	}

	private bool GetIsHole()
	{
		bool flag = true;
		for (PolyPathBase parent = _parent; parent != null; parent = parent._parent)
		{
			flag = !flag;
		}
		return flag;
	}

	internal abstract PolyPathBase AddChild(List<Point64> p);

	public void Clear()
	{
		_childs.Clear();
	}
}
