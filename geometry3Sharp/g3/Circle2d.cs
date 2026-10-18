using System;

namespace g3;

public class Circle2d : IParametricCurve2d
{
	public Vector2d Center;

	public double Radius;

	public bool IsReversed;

	public double Curvature => 1.0 / Radius;

	public double SignedCurvature
	{
		get
		{
			if (!IsReversed)
			{
				return 1.0 / Radius;
			}
			return -1.0 / Radius;
		}
	}

	public bool IsClosed => true;

	public bool IsTransformable => true;

	public double ParamLength => 1.0;

	public bool HasArcLength => true;

	public double ArcLength => Math.PI * 2.0 * Radius;

	public double Circumference
	{
		get
		{
			return Math.PI * 2.0 * Radius;
		}
		set
		{
			Radius = value / (Math.PI * 2.0);
		}
	}

	public double Diameter
	{
		get
		{
			return 2.0 * Radius;
		}
		set
		{
			Radius = value / 2.0;
		}
	}

	public double Area
	{
		get
		{
			return Math.PI * Radius * Radius;
		}
		set
		{
			Radius = Math.Sqrt(value / Math.PI);
		}
	}

	public AxisAlignedBox2d Bounds => new AxisAlignedBox2d(Center, Radius, Radius);

	public Circle2d(double radius)
	{
		IsReversed = false;
		Center = Vector2d.Zero;
		Radius = radius;
	}

	public Circle2d(Vector2d center, double radius)
	{
		IsReversed = false;
		Center = center;
		Radius = radius;
	}

	public void Reverse()
	{
		IsReversed = !IsReversed;
	}

	public IParametricCurve2d Clone()
	{
		return new Circle2d(Center, Radius)
		{
			IsReversed = IsReversed
		};
	}

	public void Transform(ITransform2 xform)
	{
		Center = xform.TransformP(Center);
		Radius = xform.TransformScalar(Radius);
	}

	public Vector2d SampleDeg(double degrees)
	{
		double num = degrees * (Math.PI / 180.0);
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		return new Vector2d(Center.x + Radius * num2, Center.y + Radius * num3);
	}

	public Vector2d SampleRad(double radians)
	{
		double num = Math.Cos(radians);
		double num2 = Math.Sin(radians);
		return new Vector2d(Center.x + Radius * num, Center.y + Radius * num2);
	}

	public Vector2d SampleT(double t)
	{
		double num = (IsReversed ? ((0.0 - t) * (Math.PI * 2.0)) : (t * (Math.PI * 2.0)));
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		return new Vector2d(Center.x + Radius * num2, Center.y + Radius * num3);
	}

	public Vector2d TangentT(double t)
	{
		double num = (IsReversed ? ((0.0 - t) * (Math.PI * 2.0)) : (t * (Math.PI * 2.0)));
		Vector2d vector2d = new Vector2d(0.0 - Math.Sin(num), Math.Cos(num));
		if (IsReversed)
		{
			vector2d = -vector2d;
		}
		vector2d.Normalize();
		return vector2d;
	}

	public Vector2d SampleArcLength(double a)
	{
		double num = a / ArcLength;
		double num2 = (IsReversed ? ((0.0 - num) * (Math.PI * 2.0)) : (num * (Math.PI * 2.0)));
		double num3 = Math.Cos(num2);
		double num4 = Math.Sin(num2);
		return new Vector2d(Center.x + Radius * num3, Center.y + Radius * num4);
	}

	public bool Contains(Vector2d p)
	{
		return Center.DistanceSquared(p) <= Radius * Radius;
	}

	public double SignedDistance(Vector2d pt)
	{
		return Center.Distance(pt) - Radius;
	}

	public double Distance(Vector2d pt)
	{
		return Math.Abs(Center.Distance(pt) - Radius);
	}

	public static double RadiusArea(double r)
	{
		return Math.PI * r * r;
	}

	public static double RadiusCircumference(double r)
	{
		return Math.PI * 2.0 * r;
	}

	public static double BoundingPolygonRadius(double r, int n)
	{
		double d = Math.PI * 2.0 / (double)n / 2.0;
		return r / Math.Cos(d);
	}
}
