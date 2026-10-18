using g3;

namespace gs;

public class LinearFalloff : IFalloffFunction
{
	public double ConstantRange;

	public double FalloffT(double t)
	{
		t = MathUtil.Clamp(t, 0.0, 1.0);
		if (ConstantRange <= 0.0)
		{
			return 1.0 - t;
		}
		if (!(t < ConstantRange))
		{
			return 1.0 - (t - ConstantRange) / (1.0 - ConstantRange);
		}
		return 1.0;
	}

	public IFalloffFunction Duplicate()
	{
		return new WyvillFalloff
		{
			ConstantRange = ConstantRange
		};
	}
}
