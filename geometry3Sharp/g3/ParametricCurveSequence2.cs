using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace g3;

public class ParametricCurveSequence2 : IParametricCurve2d, IMultiCurve2d
{
	private List<IParametricCurve2d> curves;

	private bool closed;

	public int Count => curves.Count;

	public ReadOnlyCollection<IParametricCurve2d> Curves => curves.AsReadOnly();

	public bool IsClosed
	{
		get
		{
			return closed;
		}
		set
		{
			closed = value;
		}
	}

	public double ParamLength
	{
		get
		{
			double num = 0.0;
			foreach (IParametricCurve2d curf in Curves)
			{
				num += curf.ParamLength;
			}
			return num;
		}
	}

	public bool HasArcLength
	{
		get
		{
			foreach (IParametricCurve2d curf in Curves)
			{
				if (!curf.HasArcLength)
				{
					return false;
				}
			}
			return true;
		}
	}

	public double ArcLength
	{
		get
		{
			double num = 0.0;
			foreach (IParametricCurve2d curf in Curves)
			{
				num += curf.ArcLength;
			}
			return num;
		}
	}

	public bool IsTransformable => true;

	public ParametricCurveSequence2()
	{
		curves = new List<IParametricCurve2d>();
	}

	public ParametricCurveSequence2(IEnumerable<IParametricCurve2d> curvesIn, bool isClosed = false)
	{
		curves = new List<IParametricCurve2d>(curvesIn);
		closed = true;
	}

	public void Append(IParametricCurve2d c)
	{
		curves.Add(c);
	}

	public void Prepend(IParametricCurve2d c)
	{
		curves.Insert(0, c);
	}

	public Vector2d SampleT(double t)
	{
		double num = 0.0;
		for (int i = 0; i < Curves.Count; i++)
		{
			double paramLength = curves[i].ParamLength;
			if (t <= num + paramLength)
			{
				double t2 = t - num;
				return curves[i].SampleT(t2);
			}
			num += paramLength;
		}
		throw new ArgumentException("ParametricCurveSequence2.SampleT: argument out of range");
	}

	public Vector2d TangentT(double t)
	{
		double num = 0.0;
		for (int i = 0; i < Curves.Count; i++)
		{
			double paramLength = curves[i].ParamLength;
			if (t <= num + paramLength)
			{
				double t2 = t - num;
				return curves[i].TangentT(t2);
			}
			num += paramLength;
		}
		throw new ArgumentException("ParametricCurveSequence2.SampleT: argument out of range");
	}

	public Vector2d SampleArcLength(double a)
	{
		double num = 0.0;
		for (int i = 0; i < Curves.Count; i++)
		{
			double arcLength = curves[i].ArcLength;
			if (a <= num + arcLength)
			{
				double a2 = a - num;
				return curves[i].SampleArcLength(a2);
			}
			num += arcLength;
		}
		throw new ArgumentException("ParametricCurveSequence2.SampleArcLength: argument out of range");
	}

	public void Reverse()
	{
		foreach (IParametricCurve2d curf in curves)
		{
			curf.Reverse();
		}
		curves.Reverse();
	}

	public IParametricCurve2d Clone()
	{
		ParametricCurveSequence2 parametricCurveSequence = new ParametricCurveSequence2();
		parametricCurveSequence.closed = closed;
		parametricCurveSequence.curves = new List<IParametricCurve2d>();
		foreach (IParametricCurve2d curf in curves)
		{
			parametricCurveSequence.curves.Add(curf.Clone());
		}
		return parametricCurveSequence;
	}

	public void Transform(ITransform2 xform)
	{
		foreach (IParametricCurve2d curf in curves)
		{
			curf.Transform(xform);
		}
	}
}
