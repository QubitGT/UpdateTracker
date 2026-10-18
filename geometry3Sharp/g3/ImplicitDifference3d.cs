using System;

namespace g3;

public class ImplicitDifference3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d A;

	public BoundedImplicitFunction3d B;

	public double Value(ref Vector3d pt)
	{
		return Math.Max(A.Value(ref pt), 0.0 - B.Value(ref pt));
	}

	public AxisAlignedBox3d Bounds()
	{
		return A.Bounds();
	}
}
