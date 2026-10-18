namespace g3;

public class SkeletalBlend3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d A;

	public BoundedImplicitFunction3d B;

	public double Value(ref Vector3d pt)
	{
		return A.Value(ref pt) + B.Value(ref pt);
	}

	public AxisAlignedBox3d Bounds()
	{
		AxisAlignedBox3d result = A.Bounds();
		result.Contain(B.Bounds());
		result.Expand(0.25 * result.MaxDim);
		return result;
	}
}
