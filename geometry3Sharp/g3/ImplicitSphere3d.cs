namespace g3;

public class ImplicitSphere3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public Vector3d Origin;

	public double Radius;

	public double Value(ref Vector3d pt)
	{
		return pt.Distance(ref Origin) - Radius;
	}

	public AxisAlignedBox3d Bounds()
	{
		return new AxisAlignedBox3d(Origin, Radius);
	}
}
