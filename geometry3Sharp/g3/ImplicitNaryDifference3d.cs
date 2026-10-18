using System;
using System.Collections.Generic;

namespace g3;

public class ImplicitNaryDifference3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d A;

	public List<BoundedImplicitFunction3d> BSet;

	public double Value(ref Vector3d pt)
	{
		double num = A.Value(ref pt);
		int count = BSet.Count;
		if (count == 0)
		{
			return num;
		}
		double num2 = BSet[0].Value(ref pt);
		for (int i = 1; i < count; i++)
		{
			num2 = Math.Min(num2, BSet[i].Value(ref pt));
		}
		return Math.Max(num, 0.0 - num2);
	}

	public AxisAlignedBox3d Bounds()
	{
		return A.Bounds();
	}
}
