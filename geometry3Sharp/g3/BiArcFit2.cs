using System;
using System.Collections.Generic;

namespace g3;

public class BiArcFit2
{
	private struct Arc
	{
		public Vector2d Center;

		public double Radius;

		public double AngleStartR;

		public double AngleEndR;

		public bool PositiveRotation;

		public bool IsSegment;

		public Vector2d P0;

		public Vector2d P1;

		public Arc(Vector2d c, double r, double startR, double endR, bool posRotation)
		{
			Center = c;
			Radius = r;
			AngleStartR = startR;
			AngleEndR = endR;
			PositiveRotation = posRotation;
			IsSegment = false;
			P0 = (P1 = Vector2d.Zero);
		}

		public Arc(Vector2d p0, Vector2d p1)
		{
			Center = Vector2d.Zero;
			Radius = (AngleStartR = (AngleEndR = 0.0));
			PositiveRotation = false;
			IsSegment = true;
			P0 = p0;
			P1 = p1;
		}
	}

	public Vector2d Point1;

	public Vector2d Point2;

	public Vector2d Tangent1;

	public Vector2d Tangent2;

	public double Epsilon = 1E-08;

	public Arc2d Arc1;

	public Arc2d Arc2;

	public bool Arc1IsSegment;

	public bool Arc2IsSegment;

	public Segment2d Segment1;

	public Segment2d Segment2;

	public double FitD1;

	public double FitD2;

	private Arc arc1;

	private Arc arc2;

	public List<IParametricCurve2d> Curves
	{
		get
		{
			IParametricCurve2d parametricCurve2d2;
			if (!Arc1IsSegment)
			{
				IParametricCurve2d parametricCurve2d = Arc1;
				parametricCurve2d2 = parametricCurve2d;
			}
			else
			{
				IParametricCurve2d parametricCurve2d = Segment1;
				parametricCurve2d2 = parametricCurve2d;
			}
			IParametricCurve2d item = parametricCurve2d2;
			IParametricCurve2d parametricCurve2d3;
			if (!Arc2IsSegment)
			{
				IParametricCurve2d parametricCurve2d = Arc2;
				parametricCurve2d3 = parametricCurve2d;
			}
			else
			{
				IParametricCurve2d parametricCurve2d = Segment2;
				parametricCurve2d3 = parametricCurve2d;
			}
			IParametricCurve2d item2 = parametricCurve2d3;
			return new List<IParametricCurve2d> { item, item2 };
		}
	}

	public IParametricCurve2d Curve1
	{
		get
		{
			if (!Arc1IsSegment)
			{
				return Arc1;
			}
			return Segment1;
		}
	}

	public IParametricCurve2d Curve2
	{
		get
		{
			if (!Arc2IsSegment)
			{
				return Arc2;
			}
			return Segment2;
		}
	}

	public BiArcFit2(Vector2d point1, Vector2d tangent1, Vector2d point2, Vector2d tangent2)
	{
		Point1 = point1;
		Tangent1 = tangent1;
		Point2 = point2;
		Tangent2 = tangent2;
		Fit();
		set_output();
	}

	public BiArcFit2(Vector2d point1, Vector2d tangent1, Vector2d point2, Vector2d tangent2, double d1)
	{
		Point1 = point1;
		Tangent1 = tangent1;
		Point2 = point2;
		Tangent2 = tangent2;
		Fit(d1);
		set_output();
	}

	private void set_output()
	{
		if (arc1.IsSegment)
		{
			Arc1IsSegment = true;
			Segment1 = new Segment2d(arc1.P0, arc1.P1);
		}
		else
		{
			Arc1IsSegment = false;
			Arc1 = get_arc(0);
		}
		if (arc2.IsSegment)
		{
			Arc2IsSegment = true;
			Segment2 = new Segment2d(arc2.P1, arc2.P0);
		}
		else
		{
			Arc2IsSegment = false;
			Arc2 = get_arc(1);
		}
	}

	public double Distance(Vector2d point)
	{
		double val = (Arc1IsSegment ? Math.Sqrt(Segment1.DistanceSquared(point)) : Arc1.Distance(point));
		double val2 = (Arc2IsSegment ? Math.Sqrt(Segment2.DistanceSquared(point)) : Arc2.Distance(point));
		return Math.Min(val, val2);
	}

	public Vector2d NearestPoint(Vector2d point)
	{
		Vector2d result = (Arc1IsSegment ? Segment1.NearestPoint(point) : Arc1.NearestPoint(point));
		Vector2d result2 = (Arc2IsSegment ? Segment2.NearestPoint(point) : Arc2.NearestPoint(point));
		if (!(result.DistanceSquared(point) < result2.DistanceSquared(point)))
		{
			return result2;
		}
		return result;
	}

	private void set_arc(int i, Arc a)
	{
		if (i == 0)
		{
			arc1 = a;
		}
		else
		{
			arc2 = a;
		}
	}

	private Arc2d get_arc(int i)
	{
		Arc arc = ((i == 0) ? arc1 : arc2);
		double num = arc.AngleStartR * (180.0 / Math.PI);
		double num2 = arc.AngleEndR * (180.0 / Math.PI);
		if (arc.PositiveRotation)
		{
			double num3 = num;
			num = num2;
			num2 = num3;
		}
		Arc2d arc2d = new Arc2d(arc.Center, arc.Radius, num, num2);
		if (i == 0 && arc2d.SampleT(0.0).DistanceSquared(Point1) > 1E-08)
		{
			arc2d.Reverse();
		}
		if (i == 1 && arc2d.SampleT(1.0).DistanceSquared(Point2) > 1E-08)
		{
			arc2d.Reverse();
		}
		return arc2d;
	}

	private void Fit()
	{
		Vector2d point = Point1;
		Vector2d point2 = Point2;
		Vector2d tangent = Tangent1;
		Vector2d tangent2 = Tangent2;
		Vector2d vector2d = point2 - point;
		double lengthSquared = vector2d.LengthSquared;
		Vector2d v = tangent + tangent2;
		bool flag = MathUtil.EpsilonEqual(v.LengthSquared, 4.0, Epsilon);
		double num = vector2d.Dot(tangent);
		bool flag2 = MathUtil.EpsilonEqual(num, 0.0, Epsilon);
		if (flag && flag2)
		{
			FitD1 = (FitD2 = double.PositiveInfinity);
			double num2 = Math.Atan2(vector2d.y, vector2d.x);
			Vector2d c = point + 0.25 * vector2d;
			Vector2d c2 = point + 0.75 * vector2d;
			double r = Math.Sqrt(lengthSquared) * 0.25;
			double num3 = vector2d.x * tangent.y - vector2d.y * tangent.x;
			arc1 = new Arc(c, r, num2, num2 + Math.PI, num3 < 0.0);
			arc1 = new Arc(c2, r, num2, num2 + Math.PI, num3 > 0.0);
			return;
		}
		double num4 = vector2d.Dot(v);
		double num5 = 0.0;
		if (flag)
		{
			num5 = lengthSquared / (4.0 * num);
		}
		else
		{
			double num6 = 2.0 - 2.0 * tangent.Dot(tangent2);
			num5 = (Math.Sqrt(num4 * num4 + num6 * lengthSquared) - num4) / num6;
		}
		FitD1 = (FitD2 = num5);
		Vector2d p = point + point2 + num5 * (tangent - tangent2);
		p *= 0.5;
		SetArcFromEdge(0, point, tangent, p, fromP1: true);
		SetArcFromEdge(1, point2, tangent2, p, fromP1: false);
	}

	private void Fit(double d1)
	{
		Vector2d point = Point1;
		Vector2d point2 = Point2;
		Vector2d tangent = Tangent1;
		Vector2d tangent2 = Tangent2;
		Vector2d vector2d = point2 - point;
		double lengthSquared = vector2d.LengthSquared;
		_ = (tangent + tangent2).LengthSquared;
		double num = vector2d.Dot(tangent);
		double num2 = vector2d.Dot(tangent2);
		double num3 = tangent.Dot(tangent2);
		double num4 = num2 - d1 * (num3 - 1.0);
		if (MathUtil.EpsilonEqual(num4, 0.0, 9.999999974752427E-07))
		{
			FitD1 = d1;
			FitD2 = double.PositiveInfinity;
			Vector2d p = point + d1 * tangent;
			p += (num2 - d1 * num3) * tangent2;
			SetArcFromEdge(0, point, tangent, p, fromP1: true);
			SetArcFromEdge(1, point2, tangent2, p, fromP1: false);
		}
		else
		{
			double num5 = (0.5 * lengthSquared - d1 * num) / num4;
			double num6 = 1.0 / (d1 + num5);
			Vector2d p2 = d1 * num5 * (tangent - tangent2);
			p2 += d1 * point2;
			p2 += num5 * point;
			p2 *= num6;
			FitD1 = d1;
			FitD2 = num5;
			SetArcFromEdge(0, point, tangent, p2, fromP1: true);
			SetArcFromEdge(1, point2, tangent2, p2, fromP1: false);
		}
	}

	private void SetArcFromEdge(int i, Vector2d p1, Vector2d t1, Vector2d p2, bool fromP1)
	{
		Vector2d vector2d = p2 - p1;
		Vector2d vector2d2 = new Vector2d(0.0 - t1.y, t1.x);
		double num = vector2d.Dot(vector2d2);
		if (MathUtil.EpsilonEqual(num, 0.0, Epsilon))
		{
			set_arc(i, new Arc(p1, p2));
			return;
		}
		double num2 = vector2d.LengthSquared / (2.0 * num);
		Vector2d vector2d3 = p1 + num2 * vector2d2;
		Vector2d vector2d4 = p1 - vector2d3;
		Vector2d vector2d5 = p2 - vector2d3;
		double startR = Math.Atan2(vector2d4.y, vector2d4.x);
		double endR = Math.Atan2(vector2d5.y, vector2d5.x);
		if (vector2d4.x * t1.y - vector2d4.y * t1.x > 0.0)
		{
			set_arc(i, new Arc(vector2d3, Math.Abs(num2), startR, endR, !fromP1));
		}
		else
		{
			set_arc(i, new Arc(vector2d3, Math.Abs(num2), startR, endR, fromP1));
		}
	}

	public void DebugPrint()
	{
		Console.WriteLine("biarc fit Pt0 {0} Pt1 {1}  Tan0 {2} Tan1 {3}", Point1, Point2, Tangent1, Tangent2);
		Console.WriteLine("  First: Start {0} End {1}  {2}", Arc1IsSegment ? Segment1.P0 : Arc1.SampleT(0.0), Arc1IsSegment ? Segment1.P1 : Arc1.SampleT(1.0), Arc1IsSegment ? "segment" : "arc");
		Console.WriteLine("  Second: Start {0} End {1}  {2}", Arc2IsSegment ? Segment2.P0 : Arc2.SampleT(0.0), Arc2IsSegment ? Segment2.P1 : Arc2.SampleT(1.0), Arc2IsSegment ? "segment" : "arc");
	}
}
