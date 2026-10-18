using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Unity.Cinemachine;

internal class PolyPath64 : PolyPathBase
{
	public List<Point64>? Polygon { get; private set; }

	[IndexerName("Child")]
	public PolyPath64 this[int index]
	{
		get
		{
			if (index < 0 || index >= _childs.Count)
			{
				throw new InvalidOperationException();
			}
			return (PolyPath64)_childs[index];
		}
	}

	public PolyPath64(PolyPathBase? parent = null)
		: base(parent)
	{
	}

	internal override PolyPathBase AddChild(List<Point64> p)
	{
		PolyPathBase polyPathBase = new PolyPath64(this);
		(polyPathBase as PolyPath64).Polygon = p;
		_childs.Add(polyPathBase);
		return polyPathBase;
	}

	public double Area()
	{
		double num = ((Polygon == null) ? 0.0 : Clipper.Area(Polygon));
		foreach (PolyPath64 child in _childs)
		{
			num += child.Area();
		}
		return num;
	}
}
