using System;

namespace g3;

public abstract class SculptCurveDeformation
{
	public struct DeformInfo
	{
		public bool bNoChange;

		public double maxEdgeLenSqr;

		public double minEdgeLenSqr;
	}

	protected DCurve3 _curve;

	protected Func<double, double, double> _weightfunc;

	protected double radius = 0.10000000149011612;

	protected Frame3f vPreviousPos;

	public DCurve3 Curve
	{
		get
		{
			return _curve;
		}
		set
		{
			if (_curve != value)
			{
				_curve = value;
			}
		}
	}

	public Func<double, double, double> WeightFunc
	{
		get
		{
			return _weightfunc;
		}
		set
		{
			if (_weightfunc != value)
			{
				_weightfunc = value;
			}
		}
	}

	public double Radius
	{
		get
		{
			return radius;
		}
		set
		{
			radius = value;
		}
	}

	public SculptCurveDeformation()
	{
		WeightFunc = (double d, double r) => MathUtil.WyvillFalloff01(MathUtil.Clamp(d / r, 0.0, 1.0));
	}

	public virtual void BeginDeformation(Frame3f vStartPos)
	{
		vPreviousPos = vStartPos;
	}

	public virtual DeformInfo UpdateDeformation(Frame3f vNextPos)
	{
		DeformInfo result = Apply(vNextPos);
		vPreviousPos = vNextPos;
		return result;
	}

	public abstract DeformInfo Apply(Frame3f vNextPos);
}
