using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class GeometryUtils
{
	private const float k_TwoPi = MathF.PI * 2f;

	private static readonly Vector3 k_Up = Vector3.up;

	private static readonly Vector3 k_Forward = Vector3.forward;

	private static readonly Vector3 k_Zero = Vector3.zero;

	private static readonly Quaternion k_VerticalCorrection = Quaternion.AngleAxis(180f, k_Up);

	private const float k_MostlyVertical = 0.95f;

	private static readonly List<Vector3> k_HullEdgeDirections = new List<Vector3>();

	private static readonly HashSet<int> k_HullIndices = new HashSet<int>();

	public static bool FindClosestEdge(List<Vector3> vertices, Vector3 point, out Vector3 vertexA, out Vector3 vertexB)
	{
		int count = vertices.Count;
		if (count < 1)
		{
			vertexA = Vector3.zero;
			vertexB = Vector3.zero;
			return false;
		}
		float num = float.MaxValue;
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector3 = vertices[i];
			Vector3 vector4 = vertices[(i + 1) % vertices.Count];
			Vector3 vector5 = ClosestPointOnLineSegment(point, vector3, vector4);
			float num2 = Vector3.SqrMagnitude(point - vector5);
			if (num2 < num)
			{
				num = num2;
				vector = vector3;
				vector2 = vector4;
			}
		}
		vertexA = vector;
		vertexB = vector2;
		return true;
	}

	public static Vector3 PointOnOppositeSideOfPolygon(List<Vector3> vertices, Vector3 point)
	{
		int count = vertices.Count;
		if (count < 3)
		{
			return Vector3.zero;
		}
		Vector3 vector = vertices[0];
		Vector3 vector2 = vertices[1];
		Vector3 normalized = Vector3.Cross(rhs: vertices[2] - vector, lhs: vector2 - vector).normalized;
		Vector3 zero = Vector3.zero;
		foreach (Vector3 vertex in vertices)
		{
			zero += vertex;
		}
		zero *= 1f / (float)count;
		Vector3 vector3 = Vector3.ProjectOnPlane(point - zero, normalized);
		int num = count - 1;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector4 = vertices[i];
			Vector3 vector5 = ((i == num) ? vector : vertices[i + 1]) - vector4;
			ClosestTimesOnTwoLines(vector4, vector5, zero, -vector3 * 100f, out var s, out var t);
			if (t >= 0f && s >= 0f && s <= 1f)
			{
				return vector4 + vector5 * s;
			}
		}
		return Vector3.zero;
	}

	public static void TriangulatePolygon(List<int> indices, int vertCount, bool reverse = false)
	{
		vertCount -= 2;
		indices.EnsureCapacity(vertCount * 3);
		if (reverse)
		{
			for (int i = 0; i < vertCount; i++)
			{
				indices.Add(0);
				indices.Add(i + 2);
				indices.Add(i + 1);
			}
		}
		else
		{
			for (int j = 0; j < vertCount; j++)
			{
				indices.Add(0);
				indices.Add(j + 1);
				indices.Add(j + 2);
			}
		}
	}

	public static bool ClosestTimesOnTwoLines(Vector3 positionA, Vector3 velocityA, Vector3 positionB, Vector3 velocityB, out float s, out float t, double parallelTest = double.Epsilon)
	{
		double num = Vector3.Dot(velocityA, velocityA);
		double num2 = Vector3.Dot(velocityA, velocityB);
		double num3 = Vector3.Dot(velocityB, velocityB);
		double num4 = num * num3 - num2 * num2;
		if (Math.Abs(num4) < parallelTest)
		{
			s = 0f;
			t = 0f;
			return false;
		}
		Vector3 rhs = positionA - positionB;
		float num5 = Vector3.Dot(velocityA, rhs);
		float num6 = Vector3.Dot(velocityB, rhs);
		s = (float)((num2 * (double)num6 - (double)num5 * num3) / num4);
		t = (float)((num * (double)num6 - (double)num5 * num2) / num4);
		return true;
	}

	public static bool ClosestTimesOnTwoLinesXZ(Vector3 positionA, Vector3 velocityA, Vector3 positionB, Vector3 velocityB, out float s, out float t, double parallelTest = double.Epsilon)
	{
		double num = velocityA.x * velocityA.x + velocityA.z * velocityA.z;
		double num2 = velocityA.x * velocityB.x + velocityA.z * velocityB.z;
		double num3 = velocityB.x * velocityB.x + velocityB.z * velocityB.z;
		double num4 = num * num3 - num2 * num2;
		if (Math.Abs(num4) < parallelTest)
		{
			s = 0f;
			t = 0f;
			return false;
		}
		Vector3 vector = positionA - positionB;
		float num5 = velocityA.x * vector.x + velocityA.z * vector.z;
		float num6 = velocityB.x * vector.x + velocityB.z * vector.z;
		s = (float)((num2 * (double)num6 - (double)num5 * num3) / num4);
		t = (float)((num * (double)num6 - (double)num5 * num2) / num4);
		return true;
	}

	public static bool ClosestPointsOnTwoLineSegments(Vector3 a, Vector3 aLineVector, Vector3 b, Vector3 bLineVector, out Vector3 resultA, out Vector3 resultB, double parallelTest = double.Epsilon)
	{
		float s;
		float t;
		bool flag = !ClosestTimesOnTwoLines(a, aLineVector, b, bLineVector, out s, out t, parallelTest);
		if (s > 0f && s <= 1f && t > 0f && t <= 1f)
		{
			resultA = a + aLineVector * s;
			resultB = b + bLineVector * t;
		}
		else
		{
			Vector3 vector = b + bLineVector;
			Vector3 vector2 = a + aLineVector;
			Vector3 vector3 = ClosestPointOnLineSegment(a, b, vector);
			Vector3 vector4 = ClosestPointOnLineSegment(vector2, b, vector);
			float num = Vector3.Distance(a, vector3);
			resultA = a;
			resultB = vector3;
			float num2 = Vector3.Distance(vector2, vector4);
			if (num2 < num)
			{
				resultA = vector2;
				resultB = vector4;
				num = num2;
			}
			Vector3 vector5 = ClosestPointOnLineSegment(b, a, vector2);
			num2 = Vector3.Distance(b, vector5);
			if (num2 < num)
			{
				resultA = vector5;
				resultB = b;
				num = num2;
			}
			Vector3 vector6 = ClosestPointOnLineSegment(vector, a, vector2);
			num2 = Vector3.Distance(vector, vector6);
			if (num2 < num)
			{
				resultA = vector6;
				resultB = vector;
			}
			if (flag)
			{
				if (Vector3.Dot(aLineVector, bLineVector) > 0f)
				{
					t = Vector3.Dot(vector - a, aLineVector.normalized) * 0.5f;
					Vector3 vector7 = a + aLineVector.normalized * t;
					Vector3 vector8 = vector + bLineVector.normalized * (0f - t);
					if (t > 0f && t < aLineVector.magnitude)
					{
						resultA = vector7;
						resultB = vector8;
					}
				}
				else
				{
					t = Vector3.Dot(vector2 - vector, aLineVector.normalized) * 0.5f;
					Vector3 vector9 = vector2 + aLineVector.normalized * (0f - t);
					Vector3 vector10 = vector + bLineVector.normalized * (0f - t);
					if (t > 0f && t < aLineVector.magnitude)
					{
						resultA = vector9;
						resultB = vector10;
					}
				}
			}
		}
		return flag;
	}

	public static Vector3 ClosestPointOnLineSegment(Vector3 point, Vector3 a, Vector3 b)
	{
		Vector3 vector = b - a;
		Vector3 normalized = vector.normalized;
		float num = Vector3.Dot(point - a, normalized);
		if (num < 0f)
		{
			return a;
		}
		if (num * num > vector.sqrMagnitude)
		{
			return b;
		}
		return a + num * normalized;
	}

	public static void ClosestPolygonApproach(List<Vector3> verticesA, List<Vector3> verticesB, out Vector3 pointA, out Vector3 pointB, float parallelTest = 0f)
	{
		pointA = default(Vector3);
		pointB = default(Vector3);
		float num = float.MaxValue;
		int count = verticesA.Count;
		int count2 = verticesB.Count;
		int num2 = count - 1;
		int num3 = count2 - 1;
		Vector3 vector = verticesA[0];
		Vector3 vector2 = verticesB[0];
		for (int i = 0; i < count; i++)
		{
			Vector3 vector3 = verticesA[i];
			Vector3 aLineVector = ((i == num2) ? vector : verticesA[i + 1]) - vector3;
			for (int j = 0; j < count2; j++)
			{
				Vector3 vector4 = verticesB[j];
				Vector3 bLineVector = ((j == num3) ? vector2 : verticesB[j + 1]) - vector4;
				Vector3 resultA;
				Vector3 resultB;
				bool num4 = ClosestPointsOnTwoLineSegments(vector3, aLineVector, vector4, bLineVector, out resultA, out resultB, parallelTest);
				float num5 = Vector3.Distance(resultA, resultB);
				if (num4)
				{
					if (num5 - num < parallelTest)
					{
						num = num5 - parallelTest;
						pointA = resultA;
						pointB = resultB;
					}
				}
				else if (num5 < num)
				{
					num = num5;
					pointA = resultA;
					pointB = resultB;
				}
			}
		}
	}

	public static bool PointInPolygon(Vector3 testPoint, List<Vector3> vertices)
	{
		if (vertices.Count < 3)
		{
			return false;
		}
		int num = 0;
		int i = 0;
		Vector3 vector = vertices[vertices.Count - 1];
		vector.x -= testPoint.x;
		vector.z -= testPoint.z;
		bool flag = false;
		if (!MathUtility.ApproximatelyZero(vector.z))
		{
			flag = vector.z < 0f;
		}
		else
		{
			for (int num2 = vertices.Count - 2; num2 >= 0; num2--)
			{
				float z = vertices[num2].z;
				z -= testPoint.z;
				if (!MathUtility.ApproximatelyZero(z))
				{
					flag = z < 0f;
					break;
				}
			}
		}
		for (; i < vertices.Count; i++)
		{
			Vector3 vector2 = vertices[i];
			vector2.x -= testPoint.x;
			vector2.z -= testPoint.z;
			Vector3 vector3 = vector2 - vector;
			float sqrMagnitude = vector3.sqrMagnitude;
			if (MathUtility.ApproximatelyZero(vector3.x * vector2.z - vector3.z * vector2.x) && vector.sqrMagnitude <= sqrMagnitude && vector2.sqrMagnitude <= sqrMagnitude)
			{
				return true;
			}
			if (!MathUtility.ApproximatelyZero(vector2.z))
			{
				bool flag2 = vector2.z < 0f;
				if (flag2 != flag)
				{
					flag = flag2;
					if ((vector.x * vector2.z - vector.z * vector2.x) / (0f - (vector.z - vector2.z)) > 0f)
					{
						num++;
					}
				}
			}
			vector = vector2;
		}
		return num % 2 > 0;
	}

	public static bool PointInPolygon3D(Vector3 testPoint, List<Vector3> vertices)
	{
		if (vertices.Count < 3)
		{
			return false;
		}
		double num = 0.0;
		for (int i = 0; i < vertices.Count; i++)
		{
			Vector3 lhs = vertices[i] - testPoint;
			Vector3 rhs = vertices[(i + 1) % vertices.Count] - testPoint;
			float num2 = lhs.sqrMagnitude * rhs.sqrMagnitude;
			if (num2 <= MathUtility.EpsilonScaled)
			{
				return true;
			}
			double num3 = Math.Acos(Vector3.Dot(lhs, rhs) / Mathf.Sqrt(num2));
			num += num3;
		}
		return Mathf.Abs((float)num - MathF.PI * 2f) < 0.01f;
	}

	public static Vector3 ProjectPointOnPlane(Vector3 planeNormal, Vector3 planePoint, Vector3 point)
	{
		float num = 0f - Vector3.Dot(planeNormal.normalized, point - planePoint);
		return point + planeNormal.normalized * num;
	}

	public static bool ConvexHull2D(List<Vector3> points, List<Vector3> hull)
	{
		if (points.Count < 3)
		{
			return false;
		}
		k_HullIndices.Clear();
		int count = points.Count;
		int num = 0;
		for (int i = 1; i < count; i++)
		{
			Vector3 vector = points[i];
			float x = vector.x;
			float z = vector.z;
			Vector3 vector2 = points[num];
			float x2 = vector2.x;
			float z2 = vector2.z;
			if (x < x2 || (MathUtility.Approximately(x, x2) && z < z2))
			{
				num = i;
			}
		}
		int num2 = num;
		do
		{
			Vector3 vector3 = points[num2];
			hull.Add(vector3);
			k_HullIndices.Add(num2);
			int num3 = 0;
			Vector3 vector4 = points[num3];
			for (int j = 1; j < count; j++)
			{
				if (j == num2 || (k_HullIndices.Contains(j) && j != num))
				{
					continue;
				}
				Vector3 vector5 = points[j];
				Vector3 vector6 = vector4 - vector3;
				Vector3 vector7 = vector5 - vector3;
				float num4 = vector6.z * vector7.x - vector6.x * vector7.z;
				bool flag = num4 < 0f;
				if ((flag ? (0f - num4) : num4) < MathUtility.EpsilonScaled)
				{
					if (Vector3.SqrMagnitude(vector3 - vector4) < Vector3.SqrMagnitude(vector3 - vector5))
					{
						num3 = j;
						vector4 = points[num3];
					}
				}
				else if (flag)
				{
					num3 = j;
					vector4 = points[num3];
				}
			}
			num2 = num3;
		}
		while (num2 != num);
		return true;
	}

	public static Vector3 PolygonCentroid2D(List<Vector3> vertices)
	{
		int count = vertices.Count;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		int i;
		double num4;
		double num5;
		double num6;
		double num7;
		double num8;
		for (i = 0; i < count - 1; i++)
		{
			Vector3 vector = vertices[i];
			num4 = vector.x;
			num5 = vector.z;
			Vector3 vector2 = vertices[i + 1];
			num6 = vector2.x;
			num7 = vector2.z;
			num8 = num4 * num7 - num6 * num5;
			num += num8;
			num2 += (num4 + num6) * num8;
			num3 += (num5 + num7) * num8;
		}
		Vector3 vector3 = vertices[i];
		num4 = vector3.x;
		num5 = vector3.z;
		Vector3 vector4 = vertices[0];
		num6 = vector4.x;
		num7 = vector4.z;
		num8 = num4 * num7 - num6 * num5;
		num += num8;
		num2 += (num4 + num6) * num8;
		num3 += (num5 + num7) * num8;
		num *= 0.5;
		double num9 = 6.0 * num;
		num2 /= num9;
		num3 /= num9;
		return new Vector3((float)num2, 0f, (float)num3);
	}

	public static Vector2 OrientedMinimumBoundingBox2D(List<Vector3> convexHull, Vector3[] boundingBox)
	{
		Vector3 caliperA = new Vector3(0f, 0f, 1f);
		Vector3 caliperC = new Vector3(0f, 0f, -1f);
		Vector3 caliperB = new Vector3(1f, 0f, 0f);
		Vector3 caliperD = new Vector3(-1f, 0f, 0f);
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		int indexA = 0;
		int indexC = 0;
		int indexB = 0;
		int indexD = 0;
		int count = convexHull.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = convexHull[i];
			float x = vector.x;
			if (x < num)
			{
				num = x;
				indexA = i;
			}
			if (x > num3)
			{
				num3 = x;
				indexC = i;
			}
			float z = vector.z;
			if (z < num2)
			{
				num2 = z;
				indexD = i;
			}
			if (z > num4)
			{
				num4 = z;
				indexB = i;
			}
		}
		k_HullEdgeDirections.Clear();
		int num5 = count - 1;
		for (int j = 0; j < num5; j++)
		{
			Vector3 item = convexHull[j + 1] - convexHull[j];
			item.Normalize();
			k_HullEdgeDirections.Add(item);
		}
		Vector3 item2 = convexHull[0] - convexHull[num5];
		item2.Normalize();
		k_HullEdgeDirections.Add(item2);
		double num6 = double.MaxValue;
		for (int k = 0; k < count; k++)
		{
			Vector3 alignEdge = k_HullEdgeDirections[indexA];
			Vector3 alignEdge2 = k_HullEdgeDirections[indexC];
			Vector3 alignEdge3 = k_HullEdgeDirections[indexB];
			Vector3 alignEdge4 = k_HullEdgeDirections[indexD];
			double num7 = Math.Acos(caliperA.x * alignEdge.x + caliperA.z * alignEdge.z);
			double num8 = Math.Acos(caliperC.x * alignEdge2.x + caliperC.z * alignEdge2.z);
			double num9 = Math.Acos(caliperB.x * alignEdge3.x + caliperB.z * alignEdge3.z);
			double num10 = Math.Acos(caliperD.x * alignEdge4.x + caliperD.z * alignEdge4.z);
			int num11 = 0;
			double num12 = num7;
			if (num8 < num12)
			{
				num12 = num8;
				num11 = 1;
			}
			if (num9 < num12)
			{
				num12 = num9;
				num11 = 2;
			}
			if (num10 < num12)
			{
				num11 = 3;
			}
			Vector3 caliperAEndCorner;
			Vector3 caliperBEndCorner;
			Vector3 caliperCEndCorner;
			Vector3 caliperDEndCorner;
			switch (num11)
			{
			case 0:
				RotateCalipers(alignEdge, convexHull, ref indexA, out indexB, out indexC, out indexD, out caliperA, out caliperB, out caliperC, out caliperD, out caliperAEndCorner, out caliperBEndCorner, out caliperCEndCorner, out caliperDEndCorner);
				break;
			case 1:
				RotateCalipers(alignEdge2, convexHull, ref indexC, out indexD, out indexA, out indexB, out caliperC, out caliperD, out caliperA, out caliperB, out caliperCEndCorner, out caliperDEndCorner, out caliperAEndCorner, out caliperBEndCorner);
				break;
			case 2:
				RotateCalipers(alignEdge3, convexHull, ref indexB, out indexC, out indexD, out indexA, out caliperB, out caliperC, out caliperD, out caliperA, out caliperBEndCorner, out caliperCEndCorner, out caliperDEndCorner, out caliperAEndCorner);
				break;
			default:
				RotateCalipers(alignEdge4, convexHull, ref indexD, out indexA, out indexB, out indexC, out caliperD, out caliperA, out caliperB, out caliperC, out caliperDEndCorner, out caliperAEndCorner, out caliperBEndCorner, out caliperCEndCorner);
				break;
			}
			float sqrMagnitude = (caliperAEndCorner - caliperBEndCorner).sqrMagnitude;
			float sqrMagnitude2 = (caliperAEndCorner - caliperDEndCorner).sqrMagnitude;
			float num13 = sqrMagnitude * sqrMagnitude2;
			if ((double)num13 < num6)
			{
				num6 = num13;
				boundingBox[0] = caliperDEndCorner;
				boundingBox[1] = caliperCEndCorner;
				boundingBox[2] = caliperBEndCorner;
				boundingBox[3] = caliperAEndCorner;
			}
		}
		Vector3 a = boundingBox[0];
		float x2 = Vector3.Distance(a, boundingBox[3]);
		float y = Vector3.Distance(a, boundingBox[1]);
		return new Vector2(x2, y);
	}

	private static void RotateCalipers(Vector3 alignEdge, List<Vector3> vertices, ref int indexA, out int indexB, out int indexC, out int indexD, out Vector3 caliperA, out Vector3 caliperB, out Vector3 caliperC, out Vector3 caliperD, out Vector3 caliperAEndCorner, out Vector3 caliperBEndCorner, out Vector3 caliperCEndCorner, out Vector3 caliperDEndCorner)
	{
		int count = vertices.Count;
		caliperA = alignEdge;
		caliperB = new Vector3(caliperA.z, 0f, 0f - caliperA.x);
		caliperC = -caliperA;
		caliperD = -caliperB;
		indexA = (indexA + 1) % count;
		Vector3 vector = vertices[indexA];
		indexB = indexA;
		float num = 0f;
		float t;
		while (true)
		{
			int num2 = (indexB + 1) % count;
			ClosestTimesOnTwoLinesXZ(vector, caliperA, vertices[num2], caliperD, out var s, out t);
			if (s <= num)
			{
				break;
			}
			num = s;
			indexB = num2;
		}
		caliperAEndCorner = vector + caliperA * num;
		Vector3 vector2 = vertices[indexB];
		indexC = indexB;
		num = 0f;
		while (true)
		{
			int num3 = (indexC + 1) % count;
			ClosestTimesOnTwoLinesXZ(vector2, caliperB, vertices[num3], caliperA, out var s2, out t);
			if (s2 <= num)
			{
				break;
			}
			num = s2;
			indexC = num3;
		}
		caliperBEndCorner = vector2 + caliperB * num;
		Vector3 vector3 = vertices[indexC];
		indexD = indexC;
		num = 0f;
		while (true)
		{
			int num4 = (indexD + 1) % count;
			ClosestTimesOnTwoLinesXZ(vector3, caliperC, vertices[num4], caliperB, out var s3, out t);
			if (s3 <= num)
			{
				break;
			}
			num = s3;
			indexD = num4;
		}
		caliperCEndCorner = vector3 + caliperC * num;
		caliperDEndCorner = caliperCEndCorner + caliperAEndCorner - caliperBEndCorner;
	}

	public static Quaternion RotationForBox(Vector3[] vertices)
	{
		Vector3 vector = vertices[0];
		Vector3 toDirection = vertices[3] - vector;
		return Quaternion.FromToRotation(Vector3.right, toDirection);
	}

	public static float ConvexPolygonArea(List<Vector3> vertices)
	{
		int count = vertices.Count;
		if (count < 3)
		{
			return 0f;
		}
		Vector3 vector = vertices[0];
		int num = count - 1;
		Vector3 vector2 = vertices[num];
		float num2 = vector2.x * vector.z - vector.x * vector2.z;
		for (int i = 0; i < num; i++)
		{
			Vector3 vector3 = vertices[i];
			Vector3 vector4 = vertices[i + 1];
			num2 += vector3.x * vector4.z - vector4.x * vector3.z;
		}
		return Math.Abs(num2 * 0.5f);
	}

	public static bool PolygonInPolygon(List<Vector3> polygonA, List<Vector3> polygonB)
	{
		if (polygonA.Count < 1)
		{
			return false;
		}
		foreach (Vector3 item in polygonA)
		{
			if (!PointInPolygon3D(item, polygonB))
			{
				return false;
			}
		}
		return true;
	}

	public static bool PolygonsWithinRange(List<Vector3> polygonA, List<Vector3> polygonB, float maxDistance)
	{
		return PolygonsWithinSqRange(polygonA, polygonB, maxDistance * maxDistance);
	}

	public static bool PolygonsWithinSqRange(List<Vector3> polygonA, List<Vector3> polygonB, float maxSqDistance)
	{
		ClosestPolygonApproach(polygonA, polygonB, out var pointA, out var pointB);
		if (!(Vector3.SqrMagnitude(pointB - pointA) <= maxSqDistance) && !PolygonInPolygon(polygonA, polygonB))
		{
			return PolygonInPolygon(polygonB, polygonA);
		}
		return true;
	}

	public static bool PointOnPolygonBoundsXZ(Vector3 testPoint, List<Vector3> vertices, float epsilon = float.Epsilon)
	{
		int count = vertices.Count;
		if (count < 2)
		{
			return false;
		}
		Vector3 lineStart = vertices[count - 1];
		foreach (Vector3 vertex in vertices)
		{
			if (PointOnLineSegmentXZ(testPoint, lineStart, vertex, epsilon))
			{
				return true;
			}
			lineStart = vertex;
		}
		return false;
	}

	public static bool PointOnLineSegmentXZ(Vector3 testPoint, Vector3 lineStart, Vector3 lineEnd, float epsilon = float.Epsilon)
	{
		Vector3 vector = lineEnd - lineStart;
		Vector3 vector2 = testPoint - lineStart;
		float num = vector.z * vector2.x - vector.x * vector2.z;
		if (((num >= 0f) ? num : (0f - num)) >= epsilon)
		{
			return false;
		}
		float num2 = vector.x * vector2.x + vector.z * vector2.z;
		float num3 = vector.x * vector.x + vector.z * vector.z;
		if (num2 >= 0f - epsilon)
		{
			return num2 <= num3 + epsilon;
		}
		return false;
	}

	private static Quaternion NormalizeRotationKeepingUp(Quaternion rot)
	{
		Vector3 normalized = (rot * k_Up).normalized;
		Vector3 forward;
		if (Mathf.Abs(normalized.y) > 0.95f)
		{
			forward = Vector3.Cross(k_Forward, normalized);
		}
		else
		{
			Vector3 rhs = Vector3.Cross(normalized, k_Up);
			forward = Vector3.Cross(normalized, rhs);
		}
		return Quaternion.LookRotation(forward, normalized);
	}

	public static Pose PolygonUVPoseFromPlanePose(Pose pose)
	{
		return new Pose(k_Zero, NormalizeRotationKeepingUp(pose.rotation));
	}

	public static Vector2 PolygonVertexToUV(Vector3 vertexPos, Pose planePose, Pose uvPose)
	{
		Vector3 vector = planePose.position + planePose.rotation * vertexPos;
		Vector3 vector2 = Quaternion.Inverse(uvPose.rotation) * (vector - uvPose.position);
		vector2 = k_VerticalCorrection * vector2;
		return new Vector2(vector2.x, vector2.z);
	}
}
