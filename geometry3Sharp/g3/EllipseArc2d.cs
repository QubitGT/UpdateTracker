using System;

namespace g3;

public class EllipseArc2d : IParametricCurve2d
{
	public Vector2d Center;

	public Vector2d Axis0;

	public Vector2d Axis1;

	public Vector2d Extent;

	public double AngleStartDeg;

	public double AngleEndDeg;

	public bool IsReversed;

	public bool IsClosed => false;

	public double ParamLength => 1.0;

	public bool HasArcLength => false;

	public double ArcLength
	{
		get
		{
			throw new NotImplementedException("Ellipse2.ArcLength");
		}
	}

	public bool IsTransformable => true;

	public EllipseArc2d(Vector2d center, double rotationAngleDeg, double extent0, double extent1, double startDeg, double endDeg)
	{
		Center = center;
		Matrix2d matrix2d = new Matrix2d(rotationAngleDeg * (Math.PI / 180.0));
		Axis0 = matrix2d * Vector2d.AxisX;
		Axis1 = matrix2d * Vector2d.AxisY;
		Extent = new Vector2d(extent0, extent1);
		IsReversed = false;
		AngleStartDeg = startDeg;
		AngleEndDeg = endDeg;
		if (AngleEndDeg < AngleStartDeg)
		{
			AngleEndDeg += 360.0;
		}
	}

	public EllipseArc2d(Vector2d center, Vector2d axis0, Vector2d axis1, Vector2d extent, double startDeg, double endDeg)
	{
		Center = center;
		Axis0 = axis0;
		Axis1 = axis1;
		Extent = extent;
		IsReversed = false;
		AngleStartDeg = startDeg;
		AngleEndDeg = endDeg;
		if (AngleEndDeg < AngleStartDeg)
		{
			AngleEndDeg += 360.0;
		}
	}

	public Vector2d SampleT(double t)
	{
		double num = (IsReversed ? ((1.0 - t) * AngleEndDeg + t * AngleStartDeg) : ((1.0 - t) * AngleStartDeg + t * AngleEndDeg)) * (Math.PI / 180.0);
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		double x = Extent.x;
		double y = Extent.y;
		double num4 = x * num3;
		double num5 = y * num2;
		double num6 = x * y / Math.Sqrt(num5 * num5 + num4 * num4);
		Vector2d vector2d = new Vector2d(num6 * num2, num6 * num3);
		return Center + vector2d.x * Axis0 + vector2d.y * Axis1;
	}

	public Vector2d TangentT(double t)
	{
		double num = (IsReversed ? ((1.0 - t) * AngleEndDeg + t * AngleStartDeg) : ((1.0 - t) * AngleStartDeg + t * AngleEndDeg)) * (Math.PI / 180.0);
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		double x = Extent.x;
		double y = Extent.y;
		double num4 = x * num3;
		double num5 = y * num2;
		double num6 = num4 * num4 + num5 * num5;
		double num7 = Math.Sqrt(num6);
		double num8 = 0.5 * (1.0 / num7) * (2.0 * x * x * num3 * num2 - 2.0 * y * y * num2 * num3);
		double num9 = ((0.0 - x) * y * num3 * num7 - num8 * (x * y * num2)) / num6;
		double num10 = (x * y * num2 * num7 - num8 * (x * y * num3)) / num6;
		Vector2d vector2d = num9 * Axis0 + num10 * Axis1;
		if (IsReversed)
		{
			vector2d = -vector2d;
		}
		vector2d.Normalize();
		return vector2d;
	}

	public Vector2d SampleArcLength(double a)
	{
		throw new NotImplementedException("Ellipse2.SampleArcLength");
	}

	public void Reverse()
	{
		IsReversed = !IsReversed;
	}

	public IParametricCurve2d Clone()
	{
		return new EllipseArc2d(Center, Axis0, Axis1, Extent, AngleStartDeg, AngleEndDeg)
		{
			IsReversed = IsReversed
		};
	}

	public void Transform(ITransform2 xform)
	{
		Center = xform.TransformP(Center);
		Axis0 = xform.TransformN(Axis0);
		Axis1 = xform.TransformN(Axis1);
		Extent.x = xform.TransformScalar(Extent.x);
		Extent.y = xform.TransformScalar(Extent.y);
	}
}
