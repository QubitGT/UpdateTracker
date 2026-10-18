using System;
using System.Collections.Generic;

namespace g3;

public class ImplicitNaryUnion3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public List<BoundedImplicitFunction3d> Children;

	public double Value(ref Vector3d pt)
	{
		double num = Children[0].Value(ref pt);
		int count = Children.Count;
		for (int i = 1; i < count; i++)
		{
			num = Math.Min(num, Children[i].Value(ref pt));
		}
		return num;
	}

	public AxisAlignedBox3d Bounds()
	{
		AxisAlignedBox3d result = Children[0].Bounds();
		int count = Children.Count;
		for (int i = 1; i < count; i++)
		{
			result.Contain(Children[i].Bounds());
		}
		return result;
	}
}
