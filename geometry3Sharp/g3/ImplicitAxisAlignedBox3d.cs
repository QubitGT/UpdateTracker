namespace g3;

public class ImplicitAxisAlignedBox3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public AxisAlignedBox3d AABox;

	public double Value(ref Vector3d pt)
	{
		return AABox.SignedDistance(pt);
	}

	public AxisAlignedBox3d Bounds()
	{
		return AABox;
	}
}
