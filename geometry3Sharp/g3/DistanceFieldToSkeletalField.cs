namespace g3;

public class DistanceFieldToSkeletalField : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d DistanceField;

	public double FalloffDistance;

	public const double ZeroIsocontour = 27.0 / 64.0;

	public AxisAlignedBox3d Bounds()
	{
		AxisAlignedBox3d result = DistanceField.Bounds();
		result.Expand(FalloffDistance);
		return result;
	}

	public double Value(ref Vector3d pt)
	{
		double num = DistanceField.Value(ref pt);
		if (num > FalloffDistance)
		{
			return 0.0;
		}
		if (num < 0.0 - FalloffDistance)
		{
			return 1.0;
		}
		double num2 = (num + FalloffDistance) / (2.0 * FalloffDistance);
		double num3 = 1.0 - num2 * num2;
		return num3 * num3 * num3;
	}
}
