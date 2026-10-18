using System;

namespace g3;

public class ImplicitSmoothUnion3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d A;

	public BoundedImplicitFunction3d B;

	private const double mul = 2.0 / 3.0;

	public double Value(ref Vector3d pt)
	{
		double num = A.Value(ref pt);
		double num2 = B.Value(ref pt);
		return 2.0 / 3.0 * (num + num2 - Math.Sqrt(num * num + num2 * num2 - num * num2));
	}

	public AxisAlignedBox3d Bounds()
	{
		AxisAlignedBox3d result = A.Bounds();
		result.Contain(B.Bounds());
		return result;
	}
}
