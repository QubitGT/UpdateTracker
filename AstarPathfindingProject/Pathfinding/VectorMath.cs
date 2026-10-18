using System;
using UnityEngine;

namespace Pathfinding;

public static class VectorMath
{
	public static Vector2 ComplexMultiply(Vector2 a, Vector2 b)
	{
		return new Vector2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
	}

	public static Vector2 ComplexMultiplyConjugate(Vector2 a, Vector2 b)
	{
		return new Vector2(a.x * b.x + a.y * b.y, a.y * b.x - a.x * b.y);
	}

	public static Vector3 ClosestPointOnLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		Vector3 vector = Vector3.Normalize(lineEnd - lineStart);
		float num = Vector3.Dot(point - lineStart, vector);
		return lineStart + num * vector;
	}

	public static float ClosestPointOnLineFactor(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		Vector3 rhs = lineEnd - lineStart;
		float sqrMagnitude = rhs.sqrMagnitude;
		if ((double)sqrMagnitude <= 1E-06)
		{
			return 0f;
		}
		return Vector3.Dot(point - lineStart, rhs) / sqrMagnitude;
	}

	public static float ClosestPointOnLineFactor(Int3 lineStart, Int3 lineEnd, Int3 point)
	{
		Int3 rhs = lineEnd - lineStart;
		float sqrMagnitude = rhs.sqrMagnitude;
		float num = Int3.DotLong(point - lineStart, rhs);
		if (sqrMagnitude != 0f)
		{
			num /= sqrMagnitude;
		}
		return num;
	}

	public static float ClosestPointOnLineFactor(Int2 lineStart, Int2 lineEnd, Int2 point)
	{
		Int2 b = lineEnd - lineStart;
		double num = b.sqrMagnitudeLong;
		double num2 = Int2.DotLong(point - lineStart, b);
		if (num != 0.0)
		{
			num2 /= num;
		}
		return (float)num2;
	}

	public static Vector3 ClosestPointOnSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		Vector3 vector = lineEnd - lineStart;
		float sqrMagnitude = vector.sqrMagnitude;
		if ((double)sqrMagnitude <= 1E-06)
		{
			return lineStart;
		}
		float value = Vector3.Dot(point - lineStart, vector) / sqrMagnitude;
		return lineStart + Mathf.Clamp01(value) * vector;
	}

	public static Vector3 ClosestPointOnSegmentXZ(Vector3 lineStart, Vector3 lineEnd, Vector3 point)
	{
		lineStart.y = point.y;
		lineEnd.y = point.y;
		Vector3 vector = lineEnd - lineStart;
		vector.y = 0f;
		float magnitude = vector.magnitude;
		Vector3 vector2 = ((magnitude > float.Epsilon) ? (vector / magnitude) : Vector3.zero);
		float value = Vector3.Dot(point - lineStart, vector2);
		return lineStart + Mathf.Clamp(value, 0f, vector.magnitude) * vector2;
	}

	public static float SqrDistancePointSegmentApproximate(int x, int z, int px, int pz, int qx, int qz)
	{
		float num = qx - px;
		float num2 = qz - pz;
		float num3 = x - px;
		float num4 = z - pz;
		float num5 = num * num + num2 * num2;
		float num6 = num * num3 + num2 * num4;
		if (num5 > 0f)
		{
			num6 /= num5;
		}
		if (num6 < 0f)
		{
			num6 = 0f;
		}
		else if (num6 > 1f)
		{
			num6 = 1f;
		}
		num3 = (float)px + num6 * num - (float)x;
		num4 = (float)pz + num6 * num2 - (float)z;
		return num3 * num3 + num4 * num4;
	}

	public static float SqrDistancePointSegmentApproximate(Int3 a, Int3 b, Int3 p)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		float num3 = p.x - a.x;
		float num4 = p.z - a.z;
		float num5 = num * num + num2 * num2;
		float num6 = num * num3 + num2 * num4;
		if (num5 > 0f)
		{
			num6 /= num5;
		}
		if (num6 < 0f)
		{
			num6 = 0f;
		}
		else if (num6 > 1f)
		{
			num6 = 1f;
		}
		num3 = (float)a.x + num6 * num - (float)p.x;
		num4 = (float)a.z + num6 * num2 - (float)p.z;
		return num3 * num3 + num4 * num4;
	}

	public static float SqrDistancePointSegment(Vector3 a, Vector3 b, Vector3 p)
	{
		return (ClosestPointOnSegment(a, b, p) - p).sqrMagnitude;
	}

	public static float SqrDistanceSegmentSegment(Vector3 s1, Vector3 e1, Vector3 s2, Vector3 e2)
	{
		Vector3 vector = e1 - s1;
		Vector3 vector2 = e2 - s2;
		Vector3 vector3 = s1 - s2;
		double num = Vector3.Dot(vector, vector);
		double num2 = Vector3.Dot(vector, vector2);
		double num3 = Vector3.Dot(vector2, vector2);
		double num4 = Vector3.Dot(vector, vector3);
		double num5 = Vector3.Dot(vector2, vector3);
		double num7;
		double num6;
		double num8;
		double num9;
		if ((num7 = (num6 = num * num3 - num2 * num2)) < 1E-05)
		{
			num8 = 0.0;
			num6 = 1.0;
			num9 = num5;
			num7 = num3;
		}
		else
		{
			num8 = num2 * num5 - num3 * num4;
			num9 = num * num5 - num2 * num4;
			if (num8 < 0.0)
			{
				num8 = 0.0;
				num9 = num5;
				num7 = num3;
			}
			else if (num8 > num6)
			{
				num8 = num6;
				num9 = num5 + num2;
				num7 = num3;
			}
		}
		if (num9 < 0.0)
		{
			num9 = 0.0;
			if (0.0 - num4 < 0.0)
			{
				num8 = 0.0;
			}
			else if (0.0 - num4 > num)
			{
				num8 = num6;
			}
			else
			{
				num8 = 0.0 - num4;
				num6 = num;
			}
		}
		else if (num9 > num7)
		{
			num9 = num7;
			if (0.0 - num4 + num2 < 0.0)
			{
				num8 = 0.0;
			}
			else if (0.0 - num4 + num2 > num)
			{
				num8 = num6;
			}
			else
			{
				num8 = 0.0 - num4 + num2;
				num6 = num;
			}
		}
		double num10 = ((Math.Abs(num8) < 9.999999747378752E-06) ? 0.0 : (num8 / num6));
		double num11 = ((Math.Abs(num9) < 9.999999747378752E-06) ? 0.0 : (num9 / num7));
		return (vector3 + (float)num10 * vector - (float)num11 * vector2).sqrMagnitude;
	}

	public static float SqrDistanceXZ(Vector3 a, Vector3 b)
	{
		Vector3 vector = a - b;
		return vector.x * vector.x + vector.z * vector.z;
	}

	public static long SignedTriangleAreaTimes2XZ(Int3 a, Int3 b, Int3 c)
	{
		return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);
	}

	public static float SignedTriangleAreaTimes2XZ(Vector3 a, Vector3 b, Vector3 c)
	{
		return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
	}

	public static bool RightXZ(Vector3 a, Vector3 b, Vector3 p)
	{
		return (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) < -1E-45f;
	}

	public static bool RightXZ(Int3 a, Int3 b, Int3 p)
	{
		return (long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) < 0;
	}

	public static Side SideXZ(Int3 a, Int3 b, Int3 p)
	{
		long num = (long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z);
		if (num <= 0)
		{
			if (num >= 0)
			{
				return Side.Colinear;
			}
			return Side.Right;
		}
		return Side.Left;
	}

	public static bool RightOrColinear(Vector2 a, Vector2 b, Vector2 p)
	{
		return (b.x - a.x) * (p.y - a.y) - (p.x - a.x) * (b.y - a.y) <= 0f;
	}

	public static bool RightOrColinear(Int2 a, Int2 b, Int2 p)
	{
		return (long)(b.x - a.x) * (long)(p.y - a.y) - (long)(p.x - a.x) * (long)(b.y - a.y) <= 0;
	}

	public static bool RightOrColinearXZ(Vector3 a, Vector3 b, Vector3 p)
	{
		return (b.x - a.x) * (p.z - a.z) - (p.x - a.x) * (b.z - a.z) <= 0f;
	}

	public static bool RightOrColinearXZ(Int3 a, Int3 b, Int3 p)
	{
		return (long)(b.x - a.x) * (long)(p.z - a.z) - (long)(p.x - a.x) * (long)(b.z - a.z) <= 0;
	}

	public static bool IsClockwiseMarginXZ(Vector3 a, Vector3 b, Vector3 c)
	{
		return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z) <= float.Epsilon;
	}

	public static bool IsClockwiseXZ(Vector3 a, Vector3 b, Vector3 c)
	{
		return (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z) < 0f;
	}

	public static bool IsClockwiseXZ(Int3 a, Int3 b, Int3 c)
	{
		return RightXZ(a, b, c);
	}

	public static bool IsClockwiseOrColinearXZ(Int3 a, Int3 b, Int3 c)
	{
		return RightOrColinearXZ(a, b, c);
	}

	public static bool IsClockwiseOrColinear(Int2 a, Int2 b, Int2 c)
	{
		return RightOrColinear(a, b, c);
	}

	public static bool IsColinear(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 vector = b - a;
		Vector3 vector2 = c - a;
		float num = vector.y * vector2.z - vector.z * vector2.y;
		float num2 = vector.z * vector2.x - vector.x * vector2.z;
		float num3 = vector.x * vector2.y - vector.y * vector2.x;
		return num * num + num2 * num2 + num3 * num3 <= 0.0001f;
	}

	public static bool IsColinear(Vector2 a, Vector2 b, Vector2 c)
	{
		float num = (b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y);
		if (num <= 0.0001f)
		{
			return num >= -0.0001f;
		}
		return false;
	}

	public static bool IsColinearXZ(Int3 a, Int3 b, Int3 c)
	{
		return (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z) == 0;
	}

	public static bool IsColinearXZ(Vector3 a, Vector3 b, Vector3 c)
	{
		float num = (b.x - a.x) * (c.z - a.z) - (c.x - a.x) * (b.z - a.z);
		if (num <= 1E-07f)
		{
			return num >= -1E-07f;
		}
		return false;
	}

	public static bool IsColinearAlmostXZ(Int3 a, Int3 b, Int3 c)
	{
		long num = (long)(b.x - a.x) * (long)(c.z - a.z) - (long)(c.x - a.x) * (long)(b.z - a.z);
		if (num > -1)
		{
			return num < 1;
		}
		return false;
	}

	public static bool SegmentsIntersect(Int2 start1, Int2 end1, Int2 start2, Int2 end2)
	{
		if (RightOrColinear(start1, end1, start2) != RightOrColinear(start1, end1, end2))
		{
			return RightOrColinear(start2, end2, start1) != RightOrColinear(start2, end2, end1);
		}
		return false;
	}

	public static bool SegmentsIntersectXZ(Int3 start1, Int3 end1, Int3 start2, Int3 end2)
	{
		if (RightOrColinearXZ(start1, end1, start2) != RightOrColinearXZ(start1, end1, end2))
		{
			return RightOrColinearXZ(start2, end2, start1) != RightOrColinearXZ(start2, end2, end1);
		}
		return false;
	}

	public static bool SegmentsIntersectXZ(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.z * vector.x - vector2.x * vector.z;
		if (num == 0f)
		{
			return false;
		}
		float num2 = vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x);
		float num3 = vector.x * (start1.z - start2.z) - vector.z * (start1.x - start2.x);
		float num4 = num2 / num;
		float num5 = num3 / num;
		if (num4 < 0f || num4 > 1f || num5 < 0f || num5 > 1f)
		{
			return false;
		}
		return true;
	}

	public static bool LineLineIntersectionFactor(Vector2 start1, Vector2 dir1, Vector2 start2, Vector2 dir2, out float t)
	{
		float num = dir2.y * dir1.x - dir2.x * dir1.y;
		if (Mathf.Abs(num) < 0.0001f)
		{
			t = 0f;
			return false;
		}
		float num2 = dir2.x * (start1.y - start2.y) - dir2.y * (start1.x - start2.x);
		t = num2 / num;
		return true;
	}

	public static Vector3 LineDirIntersectionPointXZ(Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2)
	{
		float num = dir2.z * dir1.x - dir2.x * dir1.z;
		if (num == 0f)
		{
			return start1;
		}
		float num2 = (dir2.x * (start1.z - start2.z) - dir2.z * (start1.x - start2.x)) / num;
		return start1 + dir1 * num2;
	}

	public static Vector3 LineDirIntersectionPointXZ(Vector3 start1, Vector3 dir1, Vector3 start2, Vector3 dir2, out bool intersects)
	{
		float num = dir2.z * dir1.x - dir2.x * dir1.z;
		if (num == 0f)
		{
			intersects = false;
			return start1;
		}
		float num2 = (dir2.x * (start1.z - start2.z) - dir2.z * (start1.x - start2.x)) / num;
		intersects = true;
		return start1 + dir1 * num2;
	}

	public static bool RaySegmentIntersectXZ(Int3 start1, Int3 end1, Int3 start2, Int3 end2)
	{
		Int3 @int = end1 - start1;
		Int3 int2 = end2 - start2;
		long num = int2.z * @int.x - int2.x * @int.z;
		if (num == 0L)
		{
			return false;
		}
		long num2 = int2.x * (start1.z - start2.z) - int2.z * (start1.x - start2.x);
		long num3 = @int.x * (start1.z - start2.z) - @int.z * (start1.x - start2.x);
		if (!((num2 < 0) ^ (num < 0)))
		{
			return false;
		}
		if (!((num3 < 0) ^ (num < 0)))
		{
			return false;
		}
		if ((num >= 0 && num3 > num) || (num < 0 && num3 <= num))
		{
			return false;
		}
		return true;
	}

	public static bool LineIntersectionFactorXZ(Int3 start1, Int3 end1, Int3 start2, Int3 end2, out float factor1, out float factor2)
	{
		Int3 @int = end1 - start1;
		Int3 int2 = end2 - start2;
		long num = int2.z * @int.x - int2.x * @int.z;
		if (num == 0L)
		{
			factor1 = 0f;
			factor2 = 0f;
			return false;
		}
		long num2 = int2.x * (start1.z - start2.z) - int2.z * (start1.x - start2.x);
		long num3 = @int.x * (start1.z - start2.z) - @int.z * (start1.x - start2.x);
		factor1 = (float)num2 / (float)num;
		factor2 = (float)num3 / (float)num;
		return true;
	}

	public static bool LineIntersectionFactorXZ(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out float factor1, out float factor2)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.z * vector.x - vector2.x * vector.z;
		if (num <= 1E-05f && num >= -1E-05f)
		{
			factor1 = 0f;
			factor2 = 0f;
			return false;
		}
		float num2 = vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x);
		float num3 = vector.x * (start1.z - start2.z) - vector.z * (start1.x - start2.x);
		float num4 = num2 / num;
		float num5 = num3 / num;
		factor1 = num4;
		factor2 = num5;
		return true;
	}

	public static float LineRayIntersectionFactorXZ(Int3 start1, Int3 end1, Int3 start2, Int3 end2)
	{
		Int3 @int = end1 - start1;
		Int3 int2 = end2 - start2;
		int num = int2.z * @int.x - int2.x * @int.z;
		if (num == 0)
		{
			return float.NaN;
		}
		int num2 = int2.x * (start1.z - start2.z) - int2.z * (start1.x - start2.x);
		if ((float)(@int.x * (start1.z - start2.z) - @int.z * (start1.x - start2.x)) / (float)num < 0f)
		{
			return float.NaN;
		}
		return (float)num2 / (float)num;
	}

	public static float LineIntersectionFactorXZ(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.z * vector.x - vector2.x * vector.z;
		if (num == 0f)
		{
			return -1f;
		}
		return (vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x)) / num;
	}

	public static Vector3 LineIntersectionPointXZ(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2)
	{
		bool intersects;
		return LineIntersectionPointXZ(start1, end1, start2, end2, out intersects);
	}

	public static Vector3 LineIntersectionPointXZ(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.z * vector.x - vector2.x * vector.z;
		if (num == 0f)
		{
			intersects = false;
			return start1;
		}
		float num2 = (vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x)) / num;
		intersects = true;
		return start1 + vector * num2;
	}

	public static Vector2 LineIntersectionPoint(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2)
	{
		bool intersects;
		return LineIntersectionPoint(start1, end1, start2, end2, out intersects);
	}

	public static Vector2 LineIntersectionPoint(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out bool intersects)
	{
		Vector2 vector = end1 - start1;
		Vector2 vector2 = end2 - start2;
		float num = vector2.y * vector.x - vector2.x * vector.y;
		if (num == 0f)
		{
			intersects = false;
			return start1;
		}
		float num2 = (vector2.x * (start1.y - start2.y) - vector2.y * (start1.x - start2.x)) / num;
		intersects = true;
		return start1 + vector * num2;
	}

	public static Vector3 SegmentIntersectionPointXZ(Vector3 start1, Vector3 end1, Vector3 start2, Vector3 end2, out bool intersects)
	{
		Vector3 vector = end1 - start1;
		Vector3 vector2 = end2 - start2;
		float num = vector2.z * vector.x - vector2.x * vector.z;
		if (num == 0f)
		{
			intersects = false;
			return start1;
		}
		float num2 = vector2.x * (start1.z - start2.z) - vector2.z * (start1.x - start2.x);
		float num3 = vector.x * (start1.z - start2.z) - vector.z * (start1.x - start2.x);
		float num4 = num2 / num;
		float num5 = num3 / num;
		if (num4 < 0f || num4 > 1f || num5 < 0f || num5 > 1f)
		{
			intersects = false;
			return start1;
		}
		intersects = true;
		return start1 + vector * num4;
	}

	public static bool SegmentIntersectsBounds(Bounds bounds, Vector3 a, Vector3 b)
	{
		a -= bounds.center;
		b -= bounds.center;
		Vector3 vector = (a + b) * 0.5f;
		Vector3 vector2 = a - vector;
		Vector3 vector3 = new Vector3(Math.Abs(vector2.x), Math.Abs(vector2.y), Math.Abs(vector2.z));
		Vector3 extents = bounds.extents;
		if (Math.Abs(vector.x) > extents.x + vector3.x)
		{
			return false;
		}
		if (Math.Abs(vector.y) > extents.y + vector3.y)
		{
			return false;
		}
		if (Math.Abs(vector.z) > extents.z + vector3.z)
		{
			return false;
		}
		if (Math.Abs(vector.y * vector2.z - vector.z * vector2.y) > extents.y * vector3.z + extents.z * vector3.y)
		{
			return false;
		}
		if (Math.Abs(vector.x * vector2.z - vector.z * vector2.x) > extents.x * vector3.z + extents.z * vector3.x)
		{
			return false;
		}
		if (Math.Abs(vector.x * vector2.y - vector.y * vector2.x) > extents.x * vector3.y + extents.y * vector3.x)
		{
			return false;
		}
		return true;
	}

	public static float LineCircleIntersectionFactor(Vector3 circleCenter, Vector3 linePoint1, Vector3 linePoint2, float radius)
	{
		float magnitude;
		Vector3 rhs = Normalize(linePoint2 - linePoint1, out magnitude);
		Vector3 lhs = linePoint1 - circleCenter;
		float num = Vector3.Dot(lhs, rhs);
		float num2 = num * num - (lhs.sqrMagnitude - radius * radius);
		if (num2 < 0f)
		{
			num2 = 0f;
		}
		float num3 = 0f - num + Mathf.Sqrt(num2);
		if (!(magnitude > 1E-05f))
		{
			return 1f;
		}
		return num3 / magnitude;
	}

	public static bool ReversesFaceOrientations(Matrix4x4 matrix)
	{
		Vector3 lhs = matrix.MultiplyVector(new Vector3(1f, 0f, 0f));
		Vector3 rhs = matrix.MultiplyVector(new Vector3(0f, 1f, 0f));
		return Vector3.Dot(rhs: matrix.MultiplyVector(new Vector3(0f, 0f, 1f)), lhs: Vector3.Cross(lhs, rhs)) < 0f;
	}

	public static bool ReversesFaceOrientationsXZ(Matrix4x4 matrix)
	{
		Vector3 vector = matrix.MultiplyVector(new Vector3(1f, 0f, 0f));
		Vector3 vector2 = matrix.MultiplyVector(new Vector3(0f, 0f, 1f));
		return vector.x * vector2.z - vector2.x * vector.z < 0f;
	}

	public static Vector3 Normalize(Vector3 v, out float magnitude)
	{
		magnitude = v.magnitude;
		if (magnitude > 1E-05f)
		{
			return v / magnitude;
		}
		return Vector3.zero;
	}

	public static Vector2 Normalize(Vector2 v, out float magnitude)
	{
		magnitude = v.magnitude;
		if (magnitude > 1E-05f)
		{
			return v / magnitude;
		}
		return Vector2.zero;
	}

	public static Vector3 ClampMagnitudeXZ(Vector3 v, float maxMagnitude)
	{
		float num = v.x * v.x + v.z * v.z;
		if (num > maxMagnitude * maxMagnitude && maxMagnitude > 0f)
		{
			float num2 = maxMagnitude / Mathf.Sqrt(num);
			v.x *= num2;
			v.z *= num2;
		}
		return v;
	}

	public static float MagnitudeXZ(Vector3 v)
	{
		return Mathf.Sqrt(v.x * v.x + v.z * v.z);
	}
}
