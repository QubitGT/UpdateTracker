namespace g3;

public class ImplicitHalfSpace3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public Vector3d Origin;

	public Vector3d Normal;

	public double Value(ref Vector3d pt)
	{
		return (pt - Origin).Dot(Normal);
	}

	public AxisAlignedBox3d Bounds()
	{
		return new AxisAlignedBox3d(Origin, 2.220446049250313E-16);
	}
}
