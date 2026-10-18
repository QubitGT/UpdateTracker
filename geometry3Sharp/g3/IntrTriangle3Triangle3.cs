using System;

namespace g3;

public class IntrTriangle3Triangle3
{
	private Triangle3d triangle0;

	private Triangle3d triangle1;

	public bool ReportCoplanarIntersection;

	public int Quantity;

	public IntersectionResult Result;

	public IntersectionType Type;

	public Vector3dTuple3 Points;

	public Vector3d[] PolygonPoints;

	public Triangle3d Triangle0
	{
		get
		{
			return triangle0;
		}
		set
		{
			triangle0 = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public Triangle3d Triangle1
	{
		get
		{
			return triangle1;
		}
		set
		{
			triangle1 = value;
			Result = IntersectionResult.NotComputed;
		}
	}

	public IntrTriangle3Triangle3(Triangle3d t0, Triangle3d t1)
	{
		triangle0 = t0;
		triangle1 = t1;
	}

	public IntrTriangle3Triangle3 Compute()
	{
		Find();
		return this;
	}

	public bool Find()
	{
		if (Result != 0)
		{
			return Result != IntersectionResult.NoIntersection;
		}
		Result = IntersectionResult.NoIntersection;
		Plane3d plane = new Plane3d(triangle0.V0, triangle0.V1, triangle0.V2);
		TrianglePlaneRelations(ref triangle1, ref plane, out var distance, out var sign, out var positive, out var negative, out var zero);
		if (positive == 3 || negative == 3)
		{
			return false;
		}
		if (zero == 3)
		{
			if (ReportCoplanarIntersection)
			{
				return GetCoplanarIntersection(ref plane, ref triangle0, ref triangle1);
			}
			return false;
		}
		if (positive == 0 || negative == 0)
		{
			if (zero == 2)
			{
				for (int i = 0; i < 3; i++)
				{
					if (sign[i] != 0)
					{
						int key = (i + 2) % 3;
						int key2 = (i + 1) % 3;
						return IntersectsSegment(ref plane, ref triangle0, triangle1[key], triangle1[key2]);
					}
				}
			}
			else
			{
				for (int i = 0; i < 3; i++)
				{
					if (sign[i] == 0)
					{
						return ContainsPoint(ref triangle0, ref plane, triangle1[i]);
					}
				}
			}
		}
		if (zero == 0)
		{
			int num = ((positive == 1) ? 1 : (-1));
			for (int i = 0; i < 3; i++)
			{
				if (sign[i] == num)
				{
					int key = (i + 2) % 3;
					int key2 = (i + 1) % 3;
					double num2 = distance[i] / (distance[i] - distance[key]);
					Vector3d end = triangle1[i] + num2 * (triangle1[key] - triangle1[i]);
					num2 = distance[i] / (distance[i] - distance[key2]);
					Vector3d end2 = triangle1[i] + num2 * (triangle1[key2] - triangle1[i]);
					return IntersectsSegment(ref plane, ref triangle0, end, end2);
				}
			}
		}
		for (int i = 0; i < 3; i++)
		{
			if (sign[i] == 0)
			{
				int key = (i + 2) % 3;
				int key2 = (i + 1) % 3;
				double num2 = distance[key] / (distance[key] - distance[key2]);
				Vector3d end = triangle1[key] + num2 * (triangle1[key2] - triangle1[key]);
				return IntersectsSegment(ref plane, ref triangle0, triangle1[i], end);
			}
		}
		return false;
	}

	public bool Test()
	{
		return Intersects(ref triangle0, ref triangle1, ref Type);
	}

	public static bool Intersects(ref Triangle3d triangle0, ref Triangle3d triangle1)
	{
		IntersectionType type = IntersectionType.Empty;
		return Intersects(ref triangle0, ref triangle1, ref type);
	}

	private static bool Intersects(ref Triangle3d triangle0, ref Triangle3d triangle1, ref IntersectionType type)
	{
		Vector3dTuple3 vector3dTuple = default(Vector3dTuple3);
		vector3dTuple.V0 = triangle0.V1 - triangle0.V0;
		vector3dTuple.V1 = triangle0.V2 - triangle0.V1;
		vector3dTuple.V2 = triangle0.V0 - triangle0.V2;
		Vector3d axis = vector3dTuple.V0.UnitCross(ref vector3dTuple.V1);
		double num = axis.Dot(ref triangle0.V0);
		ProjectOntoAxis(ref triangle1, ref axis, out var fmin, out var fmax);
		if (num < fmin || num > fmax)
		{
			return false;
		}
		Vector3dTuple3 vector3dTuple2 = default(Vector3dTuple3);
		vector3dTuple2.V0 = triangle1.V1 - triangle1.V0;
		vector3dTuple2.V1 = triangle1.V2 - triangle1.V1;
		vector3dTuple2.V2 = triangle1.V0 - triangle1.V2;
		Vector3d v = vector3dTuple2.V0.UnitCross(ref vector3dTuple2.V1);
		Vector3d v2 = axis.UnitCross(ref v);
		double fmin2;
		double fmax2;
		if (v2.Dot(ref v2) >= 1E-08)
		{
			double num2 = v.Dot(ref triangle1.V0);
			ProjectOntoAxis(ref triangle0, ref v, out fmin2, out fmax2);
			if (num2 < fmin2 || num2 > fmax2)
			{
				return false;
			}
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					Vector3d axis2 = vector3dTuple[j].UnitCross(vector3dTuple2[i]);
					ProjectOntoAxis(ref triangle0, ref axis2, out fmin2, out fmax2);
					ProjectOntoAxis(ref triangle1, ref axis2, out fmin, out fmax);
					if (fmax2 < fmin || fmax < fmin2)
					{
						return false;
					}
				}
			}
			type = IntersectionType.Unknown;
		}
		else
		{
			for (int j = 0; j < 3; j++)
			{
				Vector3d axis2 = axis.UnitCross(vector3dTuple[j]);
				ProjectOntoAxis(ref triangle0, ref axis2, out fmin2, out fmax2);
				ProjectOntoAxis(ref triangle1, ref axis2, out fmin, out fmax);
				if (fmax2 < fmin || fmax < fmin2)
				{
					return false;
				}
			}
			for (int i = 0; i < 3; i++)
			{
				Vector3d axis2 = v.UnitCross(vector3dTuple2[i]);
				ProjectOntoAxis(ref triangle0, ref axis2, out fmin2, out fmax2);
				ProjectOntoAxis(ref triangle1, ref axis2, out fmin, out fmax);
				if (fmax2 < fmin || fmax < fmin2)
				{
					return false;
				}
			}
			type = IntersectionType.Plane;
		}
		return true;
	}

	public static void ProjectOntoAxis(ref Triangle3d triangle, ref Vector3d axis, out double fmin, out double fmax)
	{
		double num = axis.Dot(triangle.V0);
		double num2 = axis.Dot(triangle.V1);
		double num3 = axis.Dot(triangle.V2);
		fmin = num;
		fmax = fmin;
		if (num2 < fmin)
		{
			fmin = num2;
		}
		else if (num2 > fmax)
		{
			fmax = num2;
		}
		if (num3 < fmin)
		{
			fmin = num3;
		}
		else if (num3 > fmax)
		{
			fmax = num3;
		}
	}

	public static void TrianglePlaneRelations(ref Triangle3d triangle, ref Plane3d plane, out Vector3d distance, out Index3i sign, out int positive, out int negative, out int zero)
	{
		positive = 0;
		negative = 0;
		zero = 0;
		distance = Vector3d.Zero;
		sign = Index3i.Zero;
		for (int i = 0; i < 3; i++)
		{
			distance[i] = plane.DistanceTo(triangle[i]);
			if (distance[i] > 1E-08)
			{
				sign[i] = 1;
				positive++;
			}
			else if (distance[i] < -1E-08)
			{
				sign[i] = -1;
				negative++;
			}
			else
			{
				distance[i] = 0.0;
				sign[i] = 0;
				zero++;
			}
		}
	}

	private bool ContainsPoint(ref Triangle3d triangle, ref Plane3d plane, Vector3d point)
	{
		Vector3d u = Vector3d.Zero;
		Vector3d v = Vector3d.Zero;
		Vector3d.GenerateComplementBasis(ref u, ref v, plane.Normal);
		Vector3d v2 = point - triangle[0];
		Vector3d v3 = triangle[1] - triangle[0];
		Vector3d v4 = triangle[2] - triangle[0];
		Vector2d test = new Vector2d(u.Dot(v2), v.Dot(v2));
		if (new QueryTuple2d(new Vector2dTuple3(Vector2d.Zero, new Vector2d(u.Dot(v3), v.Dot(v3)), new Vector2d(u.Dot(v4), v.Dot(v4)))).ToTriangle(test, 0, 1, 2) <= 0)
		{
			Result = IntersectionResult.Intersects;
			Type = IntersectionType.Point;
			Quantity = 1;
			Points[0] = point;
			return true;
		}
		return false;
	}

	private bool IntersectsSegment(ref Plane3d plane, ref Triangle3d triangle, Vector3d end0, Vector3d end1)
	{
		int num = 0;
		double num2 = Math.Abs(plane.Normal.x);
		double num3 = Math.Abs(plane.Normal.y);
		if (num3 > num2)
		{
			num = 1;
			num2 = num3;
		}
		num3 = Math.Abs(plane.Normal.z);
		if (num3 > num2)
		{
			num = 2;
		}
		Triangle2d t = default(Triangle2d);
		Vector2d zero = Vector2d.Zero;
		Vector2d zero2 = Vector2d.Zero;
		switch (num)
		{
		case 0:
		{
			for (int i = 0; i < 3; i++)
			{
				t[i] = triangle[i].yz;
				zero.x = end0.y;
				zero.y = end0.z;
				zero2.x = end1.y;
				zero2.y = end1.z;
			}
			break;
		}
		case 1:
		{
			for (int i = 0; i < 3; i++)
			{
				t[i] = triangle[i].xz;
				zero.x = end0.x;
				zero.y = end0.z;
				zero2.x = end1.x;
				zero2.y = end1.z;
			}
			break;
		}
		default:
		{
			for (int i = 0; i < 3; i++)
			{
				t[i] = triangle[i].xy;
				zero.x = end0.x;
				zero.y = end0.y;
				zero2.x = end1.x;
				zero2.y = end1.y;
			}
			break;
		}
		}
		IntrSegment2Triangle2 intrSegment2Triangle = new IntrSegment2Triangle2(new Segment2d(zero, zero2), t);
		if (!intrSegment2Triangle.Find())
		{
			return false;
		}
		Vector2dTuple2 vector2dTuple = default(Vector2dTuple2);
		if (intrSegment2Triangle.Type == IntersectionType.Segment)
		{
			Result = IntersectionResult.Intersects;
			Type = IntersectionType.Segment;
			Quantity = 2;
			vector2dTuple.V0 = intrSegment2Triangle.Point0;
			vector2dTuple.V1 = intrSegment2Triangle.Point1;
		}
		else
		{
			Result = IntersectionResult.Intersects;
			Type = IntersectionType.Point;
			Quantity = 1;
			vector2dTuple.V0 = intrSegment2Triangle.Point0;
		}
		switch (num)
		{
		case 0:
		{
			double num5 = 1.0 / plane.Normal.x;
			for (int i = 0; i < Quantity; i++)
			{
				double x2 = vector2dTuple[i].x;
				double y2 = vector2dTuple[i].y;
				double x3 = num5 * (plane.Constant - plane.Normal.y * x2 - plane.Normal.z * y2);
				Points[i] = new Vector3d(x3, x2, y2);
			}
			break;
		}
		case 1:
		{
			double num6 = 1.0 / plane.Normal.y;
			for (int i = 0; i < Quantity; i++)
			{
				double x4 = vector2dTuple[i].x;
				double y3 = vector2dTuple[i].y;
				double y4 = num6 * (plane.Constant - plane.Normal.x * x4 - plane.Normal.z * y3);
				Points[i] = new Vector3d(x4, y4, y3);
			}
			break;
		}
		default:
		{
			double num4 = 1.0 / plane.Normal.z;
			for (int i = 0; i < Quantity; i++)
			{
				double x = vector2dTuple[i].x;
				double y = vector2dTuple[i].y;
				double z = num4 * (plane.Constant - plane.Normal.x * x - plane.Normal.y * y);
				Points[i] = new Vector3d(x, y, z);
			}
			break;
		}
		}
		return true;
	}

	private bool GetCoplanarIntersection(ref Plane3d plane, ref Triangle3d tri0, ref Triangle3d tri1)
	{
		int num = 0;
		double num2 = Math.Abs(plane.Normal.x);
		double num3 = Math.Abs(plane.Normal.y);
		if (num3 > num2)
		{
			num = 1;
			num2 = num3;
		}
		num3 = Math.Abs(plane.Normal.z);
		if (num3 > num2)
		{
			num = 2;
		}
		Triangle2d t = default(Triangle2d);
		Triangle2d t2 = default(Triangle2d);
		switch (num)
		{
		case 0:
		{
			for (int i = 0; i < 3; i++)
			{
				t[i] = tri0[i].yz;
				t2[i] = tri1[i].yz;
			}
			break;
		}
		case 1:
		{
			for (int i = 0; i < 3; i++)
			{
				t[i] = tri0[i].xz;
				t2[i] = tri1[i].xz;
			}
			break;
		}
		default:
		{
			for (int i = 0; i < 3; i++)
			{
				t[i] = tri0[i].xy;
				t2[i] = tri1[i].xy;
			}
			break;
		}
		}
		Vector2d vector2d = t[1] - t[0];
		Vector2d v = t[2] - t[0];
		if (vector2d.DotPerp(v) < 0.0)
		{
			Vector2d value = t[1];
			t[1] = t[2];
			t[2] = value;
		}
		vector2d = t2[1] - t2[0];
		v = t2[2] - t2[0];
		if (vector2d.DotPerp(v) < 0.0)
		{
			Vector2d value = t2[1];
			t2[1] = t2[2];
			t2[2] = value;
		}
		IntrTriangle2Triangle2 intrTriangle2Triangle = new IntrTriangle2Triangle2(t, t2);
		if (!intrTriangle2Triangle.Find())
		{
			return false;
		}
		PolygonPoints = new Vector3d[intrTriangle2Triangle.Quantity];
		Quantity = intrTriangle2Triangle.Quantity;
		switch (num)
		{
		case 0:
		{
			double num5 = 1.0 / plane.Normal.x;
			for (int i = 0; i < Quantity; i++)
			{
				double x2 = intrTriangle2Triangle.Points[i].x;
				double y2 = intrTriangle2Triangle.Points[i].y;
				double x3 = num5 * (plane.Constant - plane.Normal.y * x2 - plane.Normal.z * y2);
				PolygonPoints[i] = new Vector3d(x3, x2, y2);
			}
			break;
		}
		case 1:
		{
			double num6 = 1.0 / plane.Normal.y;
			for (int i = 0; i < Quantity; i++)
			{
				double x4 = intrTriangle2Triangle.Points[i].x;
				double y3 = intrTriangle2Triangle.Points[i].y;
				double y4 = num6 * (plane.Constant - plane.Normal.x * x4 - plane.Normal.z * y3);
				PolygonPoints[i] = new Vector3d(x4, y4, y3);
			}
			break;
		}
		default:
		{
			double num4 = 1.0 / plane.Normal.z;
			for (int i = 0; i < Quantity; i++)
			{
				double x = intrTriangle2Triangle.Points[i].x;
				double y = intrTriangle2Triangle.Points[i].y;
				double z = num4 * (plane.Constant - plane.Normal.x * x - plane.Normal.y * y);
				PolygonPoints[i] = new Vector3d(x, y, z);
			}
			break;
		}
		}
		Result = IntersectionResult.Intersects;
		Type = IntersectionType.Polygon;
		return true;
	}
}
