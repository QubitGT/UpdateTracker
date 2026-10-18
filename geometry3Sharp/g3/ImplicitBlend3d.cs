using System;

namespace g3;

public class ImplicitBlend3d : BoundedImplicitFunction3d, ImplicitFunction3d
{
	public BoundedImplicitFunction3d A;

	public BoundedImplicitFunction3d B;

	private double weightA = 0.01;

	private double weightB = 0.01;

	private double blend = 2.0;

	public double ExpandBounds = 0.25;

	public double WeightA
	{
		get
		{
			return weightA;
		}
		set
		{
			weightA = MathUtil.Clamp(value, 1E-05, 100000.0);
		}
	}

	public double WeightB
	{
		get
		{
			return weightB;
		}
		set
		{
			weightB = MathUtil.Clamp(value, 1E-05, 100000.0);
		}
	}

	public double Blend
	{
		get
		{
			return blend;
		}
		set
		{
			blend = MathUtil.Clamp(value, 0.0, 100000.0);
		}
	}

	public double Value(ref Vector3d pt)
	{
		double num = A.Value(ref pt);
		double num2 = B.Value(ref pt);
		double num3 = num * num + num2 * num2;
		if (num3 > 1000000000000.0)
		{
			return Math.Min(num, num2);
		}
		double num4 = num / weightA;
		double num5 = num2 / weightB;
		double num6 = blend / (1.0 + num4 * num4 + num5 * num5);
		return 0.666666 * (num + num2 - Math.Sqrt(num3 - num * num2)) - num6;
	}

	public AxisAlignedBox3d Bounds()
	{
		AxisAlignedBox3d result = A.Bounds();
		result.Contain(B.Bounds());
		result.Expand(ExpandBounds * result.MaxDim);
		return result;
	}
}
