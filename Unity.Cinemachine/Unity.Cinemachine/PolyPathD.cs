using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Unity.Cinemachine;

internal class PolyPathD : PolyPathBase
{
	internal double Scale { get; set; }

	public List<PointD>? Polygon { get; private set; }

	[IndexerName("Child")]
	public PolyPathD this[int index]
	{
		get
		{
			if (index < 0 || index >= _childs.Count)
			{
				throw new InvalidOperationException();
			}
			return (PolyPathD)_childs[index];
		}
	}

	public PolyPathD(PolyPathBase? parent = null)
		: base(parent)
	{
	}

	internal override PolyPathBase AddChild(List<Point64> p)
	{
		PolyPathBase polyPathBase = new PolyPathD(this);
		(polyPathBase as PolyPathD).Scale = Scale;
		(polyPathBase as PolyPathD).Polygon = Clipper.ScalePathD(p, 1.0 / Scale);
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
