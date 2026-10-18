namespace g3;

public class ImplicitOffset3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d A;

	public double Offset;

	public double Value(ref Vector3d pt)
	{
		return A.Value(ref pt) - Offset;
	}

	public AxisAlignedBox3d Bounds()
	{
		AxisAlignedBox3d result = A.Bounds();
		result.Expand(Offset);
		return result;
	}
}
