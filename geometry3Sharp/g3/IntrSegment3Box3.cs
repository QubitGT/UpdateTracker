using System;

namespace g3;

public class IntrSegment3Box3
{
	private Segment3d segment;

	private Box3d box;

	private bool solid;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public double SegmentParam0;

	public double SegmentParam1;

	public Vector3d Point0 = Vector3d.Zero;

	public Vector3d Point1 = Vector3d.Zero;

	public Segment3d Segment
	{
		get
		{
			return segment;
		}
		set
		{
			segment = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public Box3d Box
	{
		get
		{
			return box;
		}
		set
		{
			box = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public bool Solid
	{
		get
		{
			return solid;
		}
		set
		{
			solid = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public bool IsSimpleIntersection
	{
		get
		{
			if (Result == IntersectionResult.Intersects)
			{
				return Type == IntersectionType.Point;
			}
			return false;
		}
	}

	public IntrSegment3Box3(Segment3d s, Box3d b, bool solidBox)
	{
		segment = s;
		box = b;
		solid = solidBox;
	}

	public IntrSegment3Box3 Compute()
	{
		Find();
		return this;
	}

	public bool Find()
	{
		if (Result != 0)
		{
			return Result == IntersectionResult.Intersects;
		}
		if (!segment.Direction.IsNormalized)
		{
			Type = IntersectionType.Empty;
			Result = IntersectionResult.InvalidQuery;
			return false;
		}
		SegmentParam0 = 0.0 - segment.Extent;
		SegmentParam1 = segment.Extent;
		DoClipping(ref SegmentParam0, ref SegmentParam1, segment.Center, segment.Direction, box, solid, ref Quantity, ref Point0, ref Point1, ref Type);
		Result = ((Type != 0) ? IntersectionResult.Intersects : IntersectionResult.NoIntersection);
		return Result == IntersectionResult.Intersects;
	}

	public bool Test()
	{
		Vector3d zero = Vector3d.Zero;
		Vector3d zero2 = Vector3d.Zero;
		Vector3d zero3 = Vector3d.Zero;
		Vector3d v = segment.Center - box.Center;
		zero[0] = Math.Abs(segment.Direction.Dot(box.AxisX));
		zero2[0] = Math.Abs(v.Dot(box.AxisX));
		double num = box.Extent.x + segment.Extent * zero[0];
		if (zero2[0] > num)
		{
			return false;
		}
		zero[1] = Math.Abs(segment.Direction.Dot(box.AxisY));
		zero2[1] = Math.Abs(v.Dot(box.AxisY));
		num = box.Extent.y + segment.Extent * zero[1];
		if (zero2[1] > num)
		{
			return false;
		}
		zero[2] = Math.Abs(segment.Direction.Dot(box.AxisZ));
		zero2[2] = Math.Abs(v.Dot(box.AxisZ));
		num = box.Extent.z + segment.Extent * zero[2];
		if (zero2[2] > num)
		{
			return false;
		}
		Vector3d vector3d = segment.Direction.Cross(v);
		zero3[0] = Math.Abs(vector3d.Dot(box.AxisX));
		num = box.Extent.y * zero[2] + box.Extent.z * zero[1];
		if (zero3[0] > num)
		{
			return false;
		}
		zero3[1] = Math.Abs(vector3d.Dot(box.AxisY));
		num = box.Extent.x * zero[2] + box.Extent.z * zero[0];
		if (zero3[1] > num)
		{
			return false;
		}
		zero3[2] = Math.Abs(vector3d.Dot(box.AxisZ));
		num = box.Extent.x * zero[1] + box.Extent.y * zero[0];
		if (zero3[2] > num)
		{
			return false;
		}
		return true;
	}

	public static bool DoClipping(ref double t0, ref double t1, Vector3d origin, Vector3d direction, Box3d box, bool solid, ref int quantity, ref Vector3d point0, ref Vector3d point1, ref IntersectionType intrType)
	{
		Vector3d vector3d = origin - box.Center;
		Vector3d vector3d2 = new Vector3d(vector3d.Dot(box.AxisX), vector3d.Dot(box.AxisY), vector3d.Dot(box.AxisZ));
		Vector3d vector3d3 = new Vector3d(direction.Dot(box.AxisX), direction.Dot(box.AxisY), direction.Dot(box.AxisZ));
		double num = t0;
		double num2 = t1;
		if (Clip(vector3d3.x, 0.0 - vector3d2.x - box.Extent.x, ref t0, ref t1) && Clip(0.0 - vector3d3.x, vector3d2.x - box.Extent.x, ref t0, ref t1) && Clip(vector3d3.y, 0.0 - vector3d2.y - box.Extent.y, ref t0, ref t1) && Clip(0.0 - vector3d3.y, vector3d2.y - box.Extent.y, ref t0, ref t1) && Clip(vector3d3.z, 0.0 - vector3d2.z - box.Extent.z, ref t0, ref t1) && Clip(0.0 - vector3d3.z, vector3d2.z - box.Extent.z, ref t0, ref t1) && (solid || t0 != num || t1 != num2))
		{
			if (t1 > t0)
			{
				intrType = IntersectionType.Segment;
				quantity = 2;
				point0 = origin + t0 * direction;
				point1 = origin + t1 * direction;
			}
			else
			{
				intrType = IntersectionType.Point;
				quantity = 1;
				point0 = origin + t0 * direction;
			}
		}
		else
		{
			quantity = 0;
			intrType = IntersectionType.Empty;
		}
		return intrType != IntersectionType.Empty;
	}

	public static bool Clip(double denom, double numer, ref double t0, ref double t1)
	{
		if (denom > 0.0)
		{
			if (numer > denom * t1)
			{
				return false;
			}
			if (numer > denom * t0)
			{
				t0 = numer / denom;
			}
			return true;
		}
		if (denom < 0.0)
		{
			if (numer > denom * t0)
			{
				return false;
			}
			if (numer > denom * t1)
			{
				t1 = numer / denom;
			}
			return true;
		}
		return numer <= 0.0;
	}
}
