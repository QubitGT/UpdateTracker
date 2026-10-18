using System;

namespace g3;

public class PolyLine2DCurve : IParametricCurve2d
{
	public PolyLine2d Polyline;

	public bool IsClosed => false;

	public double ParamLength => Polyline.VertexCount;

	public bool HasArcLength => true;

	public double ArcLength => Polyline.ArcLength;

	public bool IsTransformable => true;

	public Vector2d SampleT(double t)
	{
		int num = (int)t;
		if (num >= Polyline.VertexCount - 1)
		{
			return Polyline[Polyline.VertexCount - 1];
		}
		Vector2d vector2d = Polyline[num];
		Vector2d vector2d2 = Polyline[num + 1];
		double num2 = t - (double)num;
		return (1.0 - num2) * vector2d + num2 * vector2d2;
	}

	public Vector2d TangentT(double t)
	{
		throw new NotImplementedException("Polygon2dCurve.TangentT");
	}

	public Vector2d SampleArcLength(double a)
	{
		throw new NotImplementedException("Polygon2dCurve.SampleArcLength");
	}

	public void Reverse()
	{
		Polyline.Reverse();
	}

	public IParametricCurve2d Clone()
	{
		return new PolyLine2DCurve
		{
			Polyline = new PolyLine2d(Polyline)
		};
	}

	public void Transform(ITransform2 xform)
	{
		Polyline.Transform(xform);
	}
}
