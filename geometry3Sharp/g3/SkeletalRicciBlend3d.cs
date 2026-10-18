using System;

namespace g3;

public class SkeletalRicciBlend3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d A;

	public BoundedImplicitFunction3d B;

	public double BlendPower = 2.0;

	public double Value(ref Vector3d pt)
	{
		double num = A.Value(ref pt);
		double num2 = B.Value(ref pt);
		if (BlendPower == 1.0)
		{
			return num + num2;
		}
		if (BlendPower == 2.0)
		{
			return Math.Sqrt(num * num + num2 * num2);
		}
		return Math.Pow(Math.Pow(num, BlendPower) + Math.Pow(num2, BlendPower), 1.0 / BlendPower);
	}

	public AxisAlignedBox3d Bounds()
	{
		AxisAlignedBox3d result = A.Bounds();
		result.Contain(B.Bounds());
		result.Expand(0.25 * result.MaxDim);
		return result;
	}
}
