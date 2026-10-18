using g3;

namespace gs;

public class WyvillFalloff : IFalloffFunction
{
	public double ConstantRange;

	public double FalloffT(double t)
	{
		t = MathUtil.Clamp(t, 0.0, 1.0);
		if (ConstantRange <= 0.0)
		{
			return MathUtil.WyvillFalloff01(t);
		}
		return MathUtil.WyvillFalloff(t, ConstantRange, 1.0);
	}

	public IFalloffFunction Duplicate()
	{
		return new WyvillFalloff
		{
			ConstantRange = ConstantRange
		};
	}
}
