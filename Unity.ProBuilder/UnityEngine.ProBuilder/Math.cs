using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

public static class Math
{
	public const float phi = 1.618034f;

	private const float k_FltEpsilon = float.Epsilon;

	private const float k_FltCompareEpsilon = 0.0001f;

	internal const float handleEpsilon = 0.0001f;

	private static Vector3 tv1;

	private static Vector3 tv2;

	private static Vector3 tv3;

	private static Vector3 tv4;

	internal static Vector2 PointInCircumference(float radius, float angleInDegrees, Vector2 origin)
	{
		float x = radius * Mathf.Cos(MathF.PI / 180f * angleInDegrees) + origin.x;
		float y = radius * Mathf.Sin(MathF.PI / 180f * angleInDegrees) + origin.y;
		return new Vector2(x, y);
	}

	internal static Vector2 PointInEllipseCircumference(float xRadius, float yRadius, float angleInDegrees, Vector2 origin, out Vector2 tangent)
	{
		float num = Mathf.Cos(MathF.PI / 180f * angleInDegrees);
		float num2 = Mathf.Sin(MathF.PI / 180f * angleInDegrees);
		float num3 = xRadius * num + origin.x;
		float num4 = yRadius * num2 + origin.y;
		tangent = new Vector2((0f - num4) / (yRadius * yRadius), num3 / (xRadius * xRadius));
		tangent.Normalize();
		return new Vector2(num3, num4);
	}

	internal static Vector2 PointInEllipseCircumferenceWithConstantAngle(float xRadius, float yRadius, float angleInDegrees, Vector2 origin, out Vector2 tangent)
	{
		float num = Mathf.Cos(MathF.PI / 180f * angleInDegrees);
		float num2 = Mathf.Sin(MathF.PI / 180f * angleInDegrees);
		float num3 = Mathf.Tan(MathF.PI / 180f * angleInDegrees);
		float num4 = num3 * num3;
		float num5 = xRadius * yRadius / Mathf.Sqrt(yRadius * yRadius + xRadius * xRadius * num4);
		if (num < 0f)
		{
			num5 = 0f - num5;
		}
		float num6 = xRadius * yRadius / Mathf.Sqrt(xRadius * xRadius + yRadius * yRadius / num4);
		if (num2 < 0f)
		{
			num6 = 0f - num6;
		}
		tangent = new Vector2((0f - num6) / (yRadius * yRadius), num5 / (xRadius * xRadius));
		tangent.Normalize();
		return new Vector2(num5, num6);
	}

	internal static Vector3 PointInSphere(float radius, float latitudeAngle, float longitudeAngle)
	{
		float x = radius * Mathf.Cos(MathF.PI / 180f * latitudeAngle) * Mathf.Sin(MathF.PI / 180f * longitudeAngle);
		float y = radius * Mathf.Sin(MathF.PI / 180f * latitudeAngle) * Mathf.Sin(MathF.PI / 180f * longitudeAngle);
		float z = radius * Mathf.Cos(MathF.PI / 180f * longitudeAngle);
		return new Vector3(x, y, z);
	}

	internal static float SignedAngle(Vector2 a, Vector2 b)
	{
		float num = Vector2.Angle(a, b);
		if (b.x - a.x < 0f)
		{
			num = 360f - num;
		}
		return num;
	}

	public static float SqrDistance(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.y - a.y;
		float num3 = b.z - a.z;
		return num * num + num2 * num2 + num3 * num3;
	}

	public static float TriangleArea(Vector3 x, Vector3 y, Vector3 z)
	{
		float num = SqrDistance(x, y);
		float num2 = SqrDistance(y, z);
		float num3 = SqrDistance(z, x);
		return Mathf.Sqrt((2f * num * num2 + 2f * num2 * num3 + 2f * num3 * num - num * num - num2 * num2 - num3 * num3) / 16f);
	}

	internal static float PolygonArea(Vector3[] vertices, int[] indexes)
	{
		float num = 0f;
		for (int i = 0; i < indexes.Length; i += 3)
		{
			num += TriangleArea(vertices[indexes[i]], vertices[indexes[i + 1]], vertices[indexes[i + 2]]);
		}
		return num;
	}

	internal static Vector2 RotateAroundPoint(this Vector2 v, Vector2 origin, float theta)
	{
		float x = origin.x;
		float y = origin.y;
		float x2 = v.x;
		float y2 = v.y;
		float num = Mathf.Sin(theta * (MathF.PI / 180f));
		float num2 = Mathf.Cos(theta * (MathF.PI / 180f));
		float num3 = x2 - x;
		y2 -= y;
		float num4 = num3 * num2 + y2 * num;
		float num5 = (0f - num3) * num + y2 * num2;
		float x3 = num4 + x;
		y2 = num5 + y;
		return new Vector2(x3, y2);
	}

	public static Vector2 ScaleAroundPoint(this Vector2 v, Vector2 origin, Vector2 scale)
	{
		return Vector2.Scale(v - origin, scale) + origin;
	}

	internal static Vector2 Perpendicular(Vector2 value)
	{
		return new Vector2(0f - value.y, value.x);
	}

	public static Vector2 ReflectPoint(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
	{
		Vector2 from = lineEnd - lineStart;
		Vector2 vector = new Vector2(0f - from.y, from.x);
		float num = Mathf.Sin(Vector2.Angle(from, point - lineStart) * (MathF.PI / 180f)) * Vector2.Distance(point, lineStart);
		return point + num * 2f * ((Vector2.Dot(point - lineStart, vector) > 0f) ? (-1f) : 1f) * vector;
	}

	internal static float SqrDistanceRayPoint(Ray ray, Vector3 point)
	{
		return Vector3.Cross(ray.direction, point - ray.origin).sqrMagnitude;
	}

	public static float DistancePointLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
	{
		float num = (lineStart.x - lineEnd.x) * (lineStart.x - lineEnd.x) + (lineStart.y - lineEnd.y) * (lineStart.y - lineEnd.y);
		if (num == 0f)
		{
			return Vector2.Distance(point, lineStart);
		}
		float num2 = Vector2.Dot(point - lineStart, lineEnd - lineStart) / num;
		if ((double)num2 < 0.0)
		{
			return Vector2.Distance(point, lineStart);
		}
		if ((double)num2 > 1.0)
		{
			return Vector2.Distance(point, lineEnd);
		}
		Vector2 b = lineStart + num2 * (lineEnd - lineStart);
		return Vector2.Distance(point, b);
	}

	public static float DistancePointLineSegment(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
	{
		float num = (lineStart.x - lineEnd.x) * (lineStart.x - lineEnd.x) + (lineStart.y - lineEnd.y) * (lineStart.y - lineEnd.y) + (lineStart.z - lineEnd.z) * (lineStart.z - lineEnd.z);
		if (num == 0f)
		{
			return Vector3.Distance(point, lineStart);
		}
		float num2 = Vector3.Dot(point - lineStart, lineEnd - lineStart) / num;
		if ((double)num2 < 0.0)
		{
			return Vector3.Distance(point, lineStart);
		}
		if ((double)num2 > 1.0)
		{
			return Vector3.Distance(point, lineEnd);
		}
		Vector3 b = lineStart + num2 * (lineEnd - lineStart);
		return Vector3.Distance(point, b);
	}

	public static Vector3 GetNearestPointRayRay(Ray a, Ray b)
	{
		return GetNearestPointRayRay(a.origin, a.direction, b.origin, b.direction);
	}

	internal static Vector3 GetNearestPointRayRay(Vector3 ao, Vector3 ad, Vector3 bo, Vector3 bd)
	{
		float num = Vector3.Dot(ad, bd);
		float num2 = Mathf.Abs(num);
		if (num2 - 1f > Mathf.Epsilon || num2 < Mathf.Epsilon)
		{
			return ao;
		}
		Vector3 rhs = bo - ao;
		float num3 = (0f - num) * Vector3.Dot(bd, rhs) + Vector3.Dot(ad, rhs) * Vector3.Dot(bd, bd);
		float num4 = Vector3.Dot(ad, ad) * Vector3.Dot(bd, bd) - num * num;
		return ao + ad * (num3 / num4);
	}

	internal static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, ref Vector2 intersect)
	{
		intersect = Vector2.zero;
		Vector2 vector = default(Vector2);
		vector.x = p1.x - p0.x;
		vector.y = p1.y - p0.y;
		Vector2 vector2 = default(Vector2);
		vector2.x = p3.x - p2.x;
		vector2.y = p3.y - p2.y;
		float num = ((0f - vector.y) * (p0.x - p2.x) + vector.x * (p0.y - p2.y)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
		float num2 = (vector2.x * (p0.y - p2.y) - vector2.y * (p0.x - p2.x)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
		if (num >= 0f && num <= 1f && num2 >= 0f && num2 <= 1f)
		{
			intersect.x = p0.x + num2 * vector.x;
			intersect.y = p0.y + num2 * vector.y;
			return true;
		}
		return false;
	}

	internal static bool GetLineSegmentIntersect(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		Vector2 vector = default(Vector2);
		vector.x = p1.x - p0.x;
		vector.y = p1.y - p0.y;
		Vector2 vector2 = default(Vector2);
		vector2.x = p3.x - p2.x;
		vector2.y = p3.y - p2.y;
		float num = ((0f - vector.y) * (p0.x - p2.x) + vector.x * (p0.y - p2.y)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
		float num2 = (vector2.x * (p0.y - p2.y) - vector2.y * (p0.x - p2.x)) / ((0f - vector2.x) * vector.y + vector.x * vector2.y);
		if (num >= 0f && num <= 1f && num2 >= 0f)
		{
			return num2 <= 1f;
		}
		return false;
	}

	internal static bool PointInPolygon(Vector2[] polygon, Vector2 point, int[] indexes = null)
	{
		int num = ((indexes != null) ? indexes.Length : polygon.Length);
		if (num % 2 != 0)
		{
			Debug.LogError("PointInPolygon requires polygon indexes be divisible by 2!");
			return false;
		}
		Bounds2D bounds2D = new Bounds2D(polygon, indexes);
		if (bounds2D.ContainsPoint(point))
		{
			Vector2 vector = polygon[(indexes != null) ? indexes[0] : 0];
			Vector2 vector2 = polygon[(indexes == null) ? 1 : indexes[1]];
			Vector2 vector3 = vector + (vector2 - vector) * 0.5f - bounds2D.center;
			Vector2 p = bounds2D.center + vector3 * (bounds2D.size.y + bounds2D.size.x + 2f);
			int num2 = 0;
			for (int i = 0; i < num; i += 2)
			{
				int num3 = ((indexes != null) ? indexes[i] : i);
				int num4 = ((indexes != null) ? indexes[i + 1] : (i + 1));
				if (GetLineSegmentIntersect(p, point, polygon[num3], polygon[num4]))
				{
					num2++;
				}
			}
			return num2 % 2 != 0;
		}
		return false;
	}

	internal static bool PointInPolygon(Vector2[] positions, Bounds2D polyBounds, Edge[] edges, Vector2 point)
	{
		int num = edges.Length * 2;
		Vector2 p = polyBounds.center + Vector2.up * (polyBounds.size.y + 2f);
		int num2 = 0;
		for (int i = 0; i < num; i += 2)
		{
			if (GetLineSegmentIntersect(p, point, positions[i], positions[i + 1]))
			{
				num2++;
			}
		}
		return num2 % 2 != 0;
	}

	internal static bool PointInPolygon(Vector3[] positions, Bounds2D polyBounds, Edge[] edges, Vector2 point)
	{
		int num = edges.Length * 2;
		Vector2 p = polyBounds.center + Vector2.up * (polyBounds.size.y + 2f);
		int num2 = 0;
		for (int i = 0; i < num; i += 2)
		{
			if (GetLineSegmentIntersect(p, point, positions[i], positions[i + 1]))
			{
				num2++;
			}
		}
		return num2 % 2 != 0;
	}

	internal static bool RectIntersectsLineSegment(Rect rect, Vector2 a, Vector2 b)
	{
		return Clipping.RectContainsLineSegment(rect, a.x, a.y, b.x, b.y);
	}

	internal static bool RectIntersectsLineSegment(Rect rect, Vector3 a, Vector3 b)
	{
		return Clipping.RectContainsLineSegment(rect, a.x, a.y, b.x, b.y);
	}

	public static bool RayIntersectsTriangle(Ray InRay, Vector3 InTriangleA, Vector3 InTriangleB, Vector3 InTriangleC, out float OutDistance, out Vector3 OutPoint)
	{
		OutDistance = 0f;
		OutPoint = Vector3.zero;
		Vector3 vector = InTriangleB - InTriangleA;
		Vector3 vector2 = InTriangleC - InTriangleA;
		Vector3 rhs = Vector3.Cross(InRay.direction, vector2);
		float num = Vector3.Dot(vector, rhs);
		if (num > 0f - Mathf.Epsilon && num < Mathf.Epsilon)
		{
			return false;
		}
		float num2 = 1f / num;
		Vector3 lhs = InRay.origin - InTriangleA;
		float num3 = Vector3.Dot(lhs, rhs) * num2;
		if (num3 < 0f || num3 > 1f)
		{
			return false;
		}
		Vector3 rhs2 = Vector3.Cross(lhs, vector);
		float num4 = Vector3.Dot(InRay.direction, rhs2) * num2;
		if (num4 < 0f || num3 + num4 > 1f)
		{
			return false;
		}
		float num5 = Vector3.Dot(vector2, rhs2) * num2;
		if (num5 > Mathf.Epsilon)
		{
			OutDistance = num5;
			OutPoint.x = num3 * InTriangleB.x + num4 * InTriangleC.x + (1f - (num3 + num4)) * InTriangleA.x;
			OutPoint.y = num3 * InTriangleB.y + num4 * InTriangleC.y + (1f - (num3 + num4)) * InTriangleA.y;
			OutPoint.z = num3 * InTriangleB.z + num4 * InTriangleC.z + (1f - (num3 + num4)) * InTriangleA.z;
			return true;
		}
		return false;
	}

	internal static bool RayIntersectsTriangle2(Vector3 origin, Vector3 dir, Vector3 vert0, Vector3 vert1, Vector3 vert2, ref float distance, ref Vector3 normal)
	{
		Subtract(vert0, vert1, ref tv1);
		Subtract(vert0, vert2, ref tv2);
		Cross(dir, tv2, ref tv4);
		float num = Vector3.Dot(tv1, tv4);
		if (num < Mathf.Epsilon)
		{
			return false;
		}
		Subtract(vert0, origin, ref tv3);
		float num2 = Vector3.Dot(tv3, tv4);
		if (num2 < 0f || num2 > num)
		{
			return false;
		}
		Cross(tv3, tv1, ref tv4);
		float num3 = Vector3.Dot(dir, tv4);
		if (num3 < 0f || num2 + num3 > num)
		{
			return false;
		}
		distance = Vector3.Dot(tv2, tv4) * (1f / num);
		Cross(tv1, tv2, ref normal);
		return true;
	}

	public static float Secant(float x)
	{
		return 1f / Mathf.Cos(x);
	}

	public static Vector3 Normal(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		float num = p1.x - p0.x;
		float num2 = p1.y - p0.y;
		float num3 = p1.z - p0.z;
		float num4 = p2.x - p0.x;
		float num5 = p2.y - p0.y;
		float num6 = p2.z - p0.z;
		Vector3 result = new Vector3(num2 * num6 - num3 * num5, num3 * num4 - num * num6, num * num5 - num2 * num4);
		if (result.magnitude < Mathf.Epsilon)
		{
			return new Vector3(0f, 0f, 0f);
		}
		result.Normalize();
		return result;
	}

	internal static Vector3 Normal(IList<Vertex> vertices, IList<int> indexes = null)
	{
		if (indexes == null || indexes.Count % 3 != 0)
		{
			Vector3 result = Vector3.Cross(vertices[1].position - vertices[0].position, vertices[2].position - vertices[0].position);
			result.Normalize();
			return result;
		}
		int count = indexes.Count;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < count; i += 3)
		{
			zero += Normal(vertices[indexes[i]].position, vertices[indexes[i + 1]].position, vertices[indexes[i + 2]].position);
		}
		zero /= (float)count / 3f;
		zero.Normalize();
		return zero;
	}

	public static Vector3 Normal(ProBuilderMesh mesh, Face face)
	{
		if (mesh == null || face == null)
		{
			throw new ArgumentNullException("mesh");
		}
		Vector3[] positionsInternal = mesh.positionsInternal;
		Vector3 vector = Normal(positionsInternal[face.indexesInternal[0]], positionsInternal[face.indexesInternal[1]], positionsInternal[face.indexesInternal[2]]);
		if (face.indexesInternal.Length > 6)
		{
			Vector3 normal = Projection.FindBestPlane(positionsInternal, face.distinctIndexesInternal).normal;
			if (Vector3.Dot(vector, normal) < 0f)
			{
				vector.x = 0f - normal.x;
				vector.y = 0f - normal.y;
				vector.z = 0f - normal.z;
			}
			else
			{
				vector.x = normal.x;
				vector.y = normal.y;
				vector.z = normal.z;
			}
		}
		return vector;
	}

	public static Normal NormalTangentBitangent(ProBuilderMesh mesh, Face face)
	{
		if (mesh == null || face == null || face.indexesInternal.Length < 3)
		{
			throw new ArgumentNullException("mesh", "Cannot find normal, tangent, and bitangent for null object, or faces with < 3 indexes.");
		}
		if (mesh.texturesInternal == null || mesh.texturesInternal.Length != mesh.vertexCount)
		{
			throw new ArgumentException("Mesh textures[0] channel is not present, cannot calculate tangents.");
		}
		Vector3 vector = Normal(mesh, face);
		Vector3 tangent = Vector3.zero;
		Vector3 zero = Vector3.zero;
		Vector4 vector2 = new Vector4(0f, 0f, 0f, 1f);
		long num = face.indexesInternal[0];
		long num2 = face.indexesInternal[1];
		long num3 = face.indexesInternal[2];
		Vector3 vector3 = mesh.positionsInternal[num];
		Vector3 vector4 = mesh.positionsInternal[num2];
		Vector3 vector5 = mesh.positionsInternal[num3];
		Vector2 vector6 = mesh.texturesInternal[num];
		Vector2 vector7 = mesh.texturesInternal[num2];
		Vector2 vector8 = mesh.texturesInternal[num3];
		float num4 = vector4.x - vector3.x;
		float num5 = vector5.x - vector3.x;
		float num6 = vector4.y - vector3.y;
		float num7 = vector5.y - vector3.y;
		float num8 = vector4.z - vector3.z;
		float num9 = vector5.z - vector3.z;
		float num10 = vector7.x - vector6.x;
		float num11 = vector8.x - vector6.x;
		float num12 = vector7.y - vector6.y;
		float num13 = vector8.y - vector6.y;
		float num14 = 1f / (num10 * num13 - num11 * num12);
		Vector3 vector9 = new Vector3((num13 * num4 - num12 * num5) * num14, (num13 * num6 - num12 * num7) * num14, (num13 * num8 - num12 * num9) * num14);
		Vector3 vector10 = new Vector3((num10 * num5 - num11 * num4) * num14, (num10 * num7 - num11 * num6) * num14, (num10 * num9 - num11 * num8) * num14);
		tangent += vector9;
		zero += vector10;
		Vector3 normal = vector;
		Vector3.OrthoNormalize(ref normal, ref tangent);
		vector2.x = tangent.x;
		vector2.y = tangent.y;
		vector2.z = tangent.z;
		vector2.w = ((Vector3.Dot(Vector3.Cross(normal, tangent), zero) < 0f) ? (-1f) : 1f);
		Normal result = default(Normal);
		result.normal = vector;
		result.tangent = vector2;
		result.bitangent = Vector3.Cross(vector, (Vector3)vector2 * vector2.w);
		return result;
	}

	internal static bool IsCardinalAxis(Vector3 v, float epsilon = float.Epsilon)
	{
		if (v == Vector3.zero)
		{
			return false;
		}
		v.Normalize();
		if (!(1f - Mathf.Abs(Vector3.Dot(Vector3.up, v)) < epsilon) && !(1f - Mathf.Abs(Vector3.Dot(Vector3.forward, v)) < epsilon))
		{
			return 1f - Mathf.Abs(Vector3.Dot(Vector3.right, v)) < epsilon;
		}
		return true;
	}

	internal static Vector2 DivideBy(this Vector2 v, Vector2 o)
	{
		return new Vector2(v.x / o.x, v.y / o.y);
	}

	internal static Vector3 DivideBy(this Vector3 v, Vector3 o)
	{
		return new Vector3(v.x / o.x, v.y / o.y, v.z / o.z);
	}

	internal static T Max<T>(T[] array) where T : IComparable<T>
	{
		if (array == null || array.Length < 1)
		{
			return default(T);
		}
		T val = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].CompareTo(val) >= 0)
			{
				val = array[i];
			}
		}
		return val;
	}

	internal static T Min<T>(T[] array) where T : IComparable<T>
	{
		if (array == null || array.Length < 1)
		{
			return default(T);
		}
		T val = array[0];
		for (int i = 1; i < array.Length; i++)
		{
			if (array[i].CompareTo(val) < 0)
			{
				val = array[i];
			}
		}
		return val;
	}

	internal static float LargestValue(Vector3 v)
	{
		if (v.x > v.y && v.x > v.z)
		{
			return v.x;
		}
		if (v.y > v.x && v.y > v.z)
		{
			return v.y;
		}
		return v.z;
	}

	internal static float LargestValue(Vector2 v)
	{
		if (!(v.x > v.y))
		{
			return v.y;
		}
		return v.x;
	}

	internal static Vector2 SmallestVector2(Vector2[] v)
	{
		int num = v.Length;
		Vector2 result = v[0];
		for (int i = 0; i < num; i++)
		{
			if (v[i].x < result.x)
			{
				result.x = v[i].x;
			}
			if (v[i].y < result.y)
			{
				result.y = v[i].y;
			}
		}
		return result;
	}

	internal static Vector2 SmallestVector2(Vector2[] v, IList<int> indexes)
	{
		int count = indexes.Count;
		Vector2 result = v[indexes[0]];
		for (int i = 0; i < count; i++)
		{
			if (v[indexes[i]].x < result.x)
			{
				result.x = v[indexes[i]].x;
			}
			if (v[indexes[i]].y < result.y)
			{
				result.y = v[indexes[i]].y;
			}
		}
		return result;
	}

	internal static Vector2 LargestVector2(Vector2[] v)
	{
		int num = v.Length;
		Vector2 result = v[0];
		for (int i = 0; i < num; i++)
		{
			if (v[i].x > result.x)
			{
				result.x = v[i].x;
			}
			if (v[i].y > result.y)
			{
				result.y = v[i].y;
			}
		}
		return result;
	}

	internal static Vector2 LargestVector2(Vector2[] v, IList<int> indexes)
	{
		int count = indexes.Count;
		Vector2 result = v[indexes[0]];
		for (int i = 0; i < count; i++)
		{
			if (v[indexes[i]].x > result.x)
			{
				result.x = v[indexes[i]].x;
			}
			if (v[indexes[i]].y > result.y)
			{
				result.y = v[indexes[i]].y;
			}
		}
		return result;
	}

	internal static Bounds GetBounds(Vector3[] positions, IList<int> indices = null)
	{
		bool flag = indices != null;
		if ((flag && indices.Count < 1) || positions.Length < 1)
		{
			return default(Bounds);
		}
		Vector3 vector = positions[flag ? indices[0] : 0];
		Vector3 vector2 = vector;
		if (flag)
		{
			int i = 1;
			for (int count = indices.Count; i < count; i++)
			{
				vector.x = Mathf.Min(positions[indices[i]].x, vector.x);
				vector2.x = Mathf.Max(positions[indices[i]].x, vector2.x);
				vector.y = Mathf.Min(positions[indices[i]].y, vector.y);
				vector2.y = Mathf.Max(positions[indices[i]].y, vector2.y);
				vector.z = Mathf.Min(positions[indices[i]].z, vector.z);
				vector2.z = Mathf.Max(positions[indices[i]].z, vector2.z);
			}
		}
		else
		{
			int j = 1;
			for (int num = positions.Length; j < num; j++)
			{
				vector.x = Mathf.Min(positions[j].x, vector.x);
				vector2.x = Mathf.Max(positions[j].x, vector2.x);
				vector.y = Mathf.Min(positions[j].y, vector.y);
				vector2.y = Mathf.Max(positions[j].y, vector2.y);
				vector.z = Mathf.Min(positions[j].z, vector.z);
				vector2.z = Mathf.Max(positions[j].z, vector2.z);
			}
		}
		return new Bounds((vector + vector2) * 0.5f, vector2 - vector);
	}

	public static Vector2 Average(IList<Vector2> array, IList<int> indexes = null)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Vector2 zero = Vector2.zero;
		float num = indexes?.Count ?? array.Count;
		if (indexes == null)
		{
			for (int i = 0; (float)i < num; i++)
			{
				zero += array[i];
			}
		}
		else
		{
			for (int j = 0; (float)j < num; j++)
			{
				zero += array[indexes[j]];
			}
		}
		return zero / num;
	}

	public static Vector3 Average(IList<Vector3> array, IList<int> indexes = null)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Vector3 zero = Vector3.zero;
		float num = indexes?.Count ?? array.Count;
		if (indexes == null)
		{
			for (int i = 0; (float)i < num; i++)
			{
				zero.x += array[i].x;
				zero.y += array[i].y;
				zero.z += array[i].z;
			}
		}
		else
		{
			for (int j = 0; (float)j < num; j++)
			{
				zero.x += array[indexes[j]].x;
				zero.y += array[indexes[j]].y;
				zero.z += array[indexes[j]].z;
			}
		}
		return zero / num;
	}

	public static Vector4 Average(IList<Vector4> array, IList<int> indexes = null)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		Vector4 vector = Vector3.zero;
		float num = indexes?.Count ?? array.Count;
		if (indexes == null)
		{
			for (int i = 0; (float)i < num; i++)
			{
				vector.x += array[i].x;
				vector.y += array[i].y;
				vector.z += array[i].z;
			}
		}
		else
		{
			for (int j = 0; (float)j < num; j++)
			{
				vector.x += array[indexes[j]].x;
				vector.y += array[indexes[j]].y;
				vector.z += array[indexes[j]].z;
			}
		}
		return vector / num;
	}

	internal static Vector3 InvertScaleVector(Vector3 scaleVector)
	{
		for (int i = 0; i < 3; i++)
		{
			scaleVector[i] = ((scaleVector[i] == 0f) ? 0f : (1f / scaleVector[i]));
		}
		return scaleVector;
	}

	internal static bool Approx2(this Vector2 a, Vector2 b, float delta = 0.0001f)
	{
		if (Mathf.Abs(a.x - b.x) < delta)
		{
			return Mathf.Abs(a.y - b.y) < delta;
		}
		return false;
	}

	internal static bool Approx3(this Vector3 a, Vector3 b, float delta = 0.0001f)
	{
		if (Mathf.Abs(a.x - b.x) < delta && Mathf.Abs(a.y - b.y) < delta)
		{
			return Mathf.Abs(a.z - b.z) < delta;
		}
		return false;
	}

	internal static bool Approx4(this Vector4 a, Vector4 b, float delta = 0.0001f)
	{
		if (Mathf.Abs(a.x - b.x) < delta && Mathf.Abs(a.y - b.y) < delta && Mathf.Abs(a.z - b.z) < delta)
		{
			return Mathf.Abs(a.w - b.w) < delta;
		}
		return false;
	}

	internal static bool ApproxC(this Color a, Color b, float delta = 0.0001f)
	{
		if (Mathf.Abs(a.r - b.r) < delta && Mathf.Abs(a.g - b.g) < delta && Mathf.Abs(a.b - b.b) < delta)
		{
			return Mathf.Abs(a.a - b.a) < delta;
		}
		return false;
	}

	internal static bool Approx(this float a, float b, float delta = 0.0001f)
	{
		return Mathf.Abs(b - a) < Mathf.Abs(delta);
	}

	public static int Clamp(int value, int lowerBound, int upperBound)
	{
		if (value >= lowerBound)
		{
			if (value <= upperBound)
			{
				return value;
			}
			return upperBound;
		}
		return lowerBound;
	}

	internal static Vector3 Abs(this Vector3 v)
	{
		return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
	}

	internal static Vector3 Sign(this Vector3 v)
	{
		return new Vector3(Mathf.Sign(v.x), Mathf.Sign(v.y), Mathf.Sign(v.z));
	}

	internal static float Sum(this Vector3 v)
	{
		return Mathf.Abs(v.x) + Mathf.Abs(v.y) + Mathf.Abs(v.z);
	}

	internal static void Cross(Vector3 a, Vector3 b, ref Vector3 res)
	{
		res.x = a.y * b.z - a.z * b.y;
		res.y = a.z * b.x - a.x * b.z;
		res.z = a.x * b.y - a.y * b.x;
	}

	internal static void Subtract(Vector3 a, Vector3 b, ref Vector3 res)
	{
		res.x = b.x - a.x;
		res.y = b.y - a.y;
		res.z = b.z - a.z;
	}

	internal static bool IsNumber(float value)
	{
		if (!float.IsInfinity(value))
		{
			return !float.IsNaN(value);
		}
		return false;
	}

	internal static bool IsNumber(Vector2 value)
	{
		if (IsNumber(value.x))
		{
			return IsNumber(value.y);
		}
		return false;
	}

	internal static bool IsNumber(Vector3 value)
	{
		if (IsNumber(value.x) && IsNumber(value.y))
		{
			return IsNumber(value.z);
		}
		return false;
	}

	internal static bool IsNumber(Vector4 value)
	{
		if (IsNumber(value.x) && IsNumber(value.y) && IsNumber(value.z))
		{
			return IsNumber(value.w);
		}
		return false;
	}

	internal static float MakeNonZero(float value, float min = 0.0001f)
	{
		if (float.IsNaN(value) || float.IsInfinity(value) || Mathf.Abs(value) < min)
		{
			return min * Mathf.Sign(value);
		}
		return value;
	}

	internal static Vector4 FixNaN(Vector4 value)
	{
		value.x = (IsNumber(value.x) ? value.x : 0f);
		value.y = (IsNumber(value.y) ? value.y : 0f);
		value.z = (IsNumber(value.z) ? value.z : 0f);
		value.w = (IsNumber(value.w) ? value.w : 0f);
		return value;
	}

	internal static Vector2 EnsureUnitVector(Vector2 value)
	{
		if (!(Mathf.Abs(value.sqrMagnitude) < float.Epsilon))
		{
			return value.normalized;
		}
		return Vector2.right;
	}

	internal static Vector3 EnsureUnitVector(Vector3 value)
	{
		if (!(Mathf.Abs(value.sqrMagnitude) < float.Epsilon))
		{
			return value.normalized;
		}
		return Vector3.up;
	}

	internal static Vector4 EnsureUnitVector(Vector4 value)
	{
		Vector3 vector = EnsureUnitVector((Vector3)value);
		return new Vector4(vector.x, vector.y, vector.z, MakeNonZero(value.w, 1f));
	}
}
