using System;

namespace g3;

public class Arc2d : IParametricCurve2d
{
	public Vector2d Center;

	public double Radius;

	public double AngleStartDeg;

	public double AngleEndDeg;

	public bool IsReversed;

	private static readonly Vector2d[] bounds_dirs = new Vector2d[4]
	{
		Vector2d.AxisX,
		Vector2d.AxisY,
		-Vector2d.AxisX,
		-Vector2d.AxisY
	};

	public Vector2d P0 => SampleT(0.0);

	public Vector2d P1 => SampleT(1.0);

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

	public bool IsClosed => false;

	public double ParamLength => 1.0;

	public bool HasArcLength => true;

	public double ArcLength => (AngleEndDeg - AngleStartDeg) * (Math.PI / 180.0) * Radius;

	public bool IsTransformable => true;

	public AxisAlignedBox2d Bounds
	{
		get
		{
			int num = (int)(AngleStartDeg / 90.0);
			if ((double)(num * 90) < AngleStartDeg)
			{
				num++;
			}
			int num2 = (int)(AngleEndDeg / 90.0);
			if ((double)(num2 * 90) > AngleEndDeg)
			{
				num2--;
			}
			AxisAlignedBox2d empty = AxisAlignedBox2d.Empty;
			while (num <= num2)
			{
				int num3 = num++ % 4;
				empty.Contain(bounds_dirs[num3]);
			}
			empty.Scale(Radius);
			empty.Translate(Center);
			empty.Contain(P0);
			empty.Contain(P1);
			return empty;
		}
	}

	public Arc2d(Vector2d center, double radius, double startDeg, double endDeg)
	{
		IsReversed = false;
		Center = center;
		Radius = radius;
		AngleStartDeg = startDeg;
		AngleEndDeg = endDeg;
		if (AngleEndDeg < AngleStartDeg)
		{
			AngleEndDeg += 360.0;
		}
	}

	public Arc2d(Vector2d vCenter, Vector2d vStart, Vector2d vEnd)
	{
		IsReversed = false;
		SetFromCenterAndPoints(vCenter, vStart, vEnd);
	}

	public void SetFromCenterAndPoints(Vector2d vCenter, Vector2d vStart, Vector2d vEnd)
	{
		Vector2d vector2d = vStart - vCenter;
		Vector2d vector2d2 = vEnd - vCenter;
		AngleStartDeg = Math.Atan2(vector2d.y, vector2d.x) * (180.0 / Math.PI);
		AngleEndDeg = Math.Atan2(vector2d2.y, vector2d2.x) * (180.0 / Math.PI);
		if (AngleEndDeg < AngleStartDeg)
		{
			AngleEndDeg += 360.0;
		}
		Center = vCenter;
		Radius = vector2d.Length;
	}

	public Vector2d SampleT(double t)
	{
		double num = (IsReversed ? ((1.0 - t) * AngleEndDeg + t * AngleStartDeg) : ((1.0 - t) * AngleStartDeg + t * AngleEndDeg)) * (Math.PI / 180.0);
		double num2 = Math.Cos(num);
		double num3 = Math.Sin(num);
		return new Vector2d(Center.x + Radius * num2, Center.y + Radius * num3);
	}

	public Vector2d TangentT(double t)
	{
		double num = (IsReversed ? ((1.0 - t) * AngleEndDeg + t * AngleStartDeg) : ((1.0 - t) * AngleStartDeg + t * AngleEndDeg));
		num *= Math.PI / 180.0;
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
		if (ArcLength < 2.220446049250313E-16)
		{
			if (!(a < 0.5))
			{
				return SampleT(1.0);
			}
			return SampleT(0.0);
		}
		double num = a / ArcLength;
		double num2 = (IsReversed ? ((1.0 - num) * AngleEndDeg + num * AngleStartDeg) : ((1.0 - num) * AngleStartDeg + num * AngleEndDeg)) * (Math.PI / 180.0);
		double num3 = Math.Cos(num2);
		double num4 = Math.Sin(num2);
		return new Vector2d(Center.x + Radius * num3, Center.y + Radius * num4);
	}

	public void Reverse()
	{
		IsReversed = !IsReversed;
	}

	public IParametricCurve2d Clone()
	{
		return new Arc2d(Center, Radius, AngleStartDeg, AngleEndDeg)
		{
			IsReversed = IsReversed
		};
	}

	public void Transform(ITransform2 xform)
	{
		Vector2d vCenter = xform.TransformP(Center);
		Vector2d vStart = xform.TransformP(IsReversed ? P1 : P0);
		Vector2d vEnd = xform.TransformP(IsReversed ? P0 : P1);
		SetFromCenterAndPoints(vCenter, vStart, vEnd);
	}

	public double Distance(Vector2d point)
	{
		Vector2d vector2d = point - Center;
		double length = vector2d.Length;
		if (length > 2.220446049250313E-16)
		{
			Vector2d vector2d2 = vector2d / length;
			double num = Math.Atan2(vector2d2.y, vector2d2.x) * (180.0 / Math.PI);
			if (!(num >= AngleStartDeg) || !(num <= AngleEndDeg))
			{
				double num2 = MathUtil.ClampAngleDeg(num, AngleStartDeg, AngleEndDeg) * (Math.PI / 180.0);
				double num3 = Math.Cos(num2);
				double num4 = Math.Sin(num2);
				return new Vector2d(Center.x + Radius * num3, Center.y + Radius * num4).Distance(point);
			}
			return Math.Abs(length - Radius);
		}
		return Radius;
	}

	public Vector2d NearestPoint(Vector2d point)
	{
		Vector2d vector2d = point - Center;
		double length = vector2d.Length;
		if (length > 2.220446049250313E-16)
		{
			Vector2d vector2d2 = vector2d / length;
			double num = Math.Atan2(vector2d2.y, vector2d2.x);
			num *= 180.0 / Math.PI;
			num = MathUtil.ClampAngleDeg(num, AngleStartDeg, AngleEndDeg);
			num = Math.PI / 180.0 * num;
			double num2 = Math.Cos(num);
			double num3 = Math.Sin(num);
			return new Vector2d(Center.x + Radius * num2, Center.y + Radius * num3);
		}
		return SampleT(0.5);
	}
}
