using System;

namespace g3;

public class Polygon2DCurve : IParametricCurve2d
{
	public Polygon2d Polygon;

	public bool IsClosed => true;

	public double ParamLength => Polygon.VertexCount;

	public bool HasArcLength => true;

	public double ArcLength => Polygon.ArcLength;

	public bool IsTransformable => true;

	public Vector2d SampleT(double t)
	{
		int num = (int)t;
		if (num >= Polygon.VertexCount - 1)
		{
			return Polygon[Polygon.VertexCount - 1];
		}
		Vector2d vector2d = Polygon[num];
		Vector2d vector2d2 = Polygon[num + 1];
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
		Polygon.Reverse();
	}

	public IParametricCurve2d Clone()
	{
		return new Polygon2DCurve
		{
			Polygon = new Polygon2d(Polygon)
		};
	}

	public void Transform(ITransform2 xform)
	{
		Polygon.Transform(xform);
	}
}
