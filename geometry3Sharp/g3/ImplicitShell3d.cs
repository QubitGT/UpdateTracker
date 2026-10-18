using System;

namespace g3;

public class ImplicitShell3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d A;

	public Interval1d Inside;

	public double Value(ref Vector3d pt)
	{
		double num = A.Value(ref pt);
		if (num < Inside.a)
		{
			return Inside.a - num;
		}
		if (num > Inside.b)
		{
			return num - Inside.b;
		}
		return 0.0 - Math.Min(Math.Abs(num - Inside.a), Math.Abs(num - Inside.b));
	}

	public AxisAlignedBox3d Bounds()
	{
		AxisAlignedBox3d result = A.Bounds();
		result.Expand(Math.Max(0.0, Inside.b));
		return result;
	}
}
