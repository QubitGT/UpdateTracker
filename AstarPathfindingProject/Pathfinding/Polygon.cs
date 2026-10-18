using System;
using System.Collections.Generic;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

public static class Polygon
{
	private static readonly Dictionary<Int3, int> cached_Int3_int_dict = new Dictionary<Int3, int>();

	public static bool ContainsPointXZ(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
	{
		if (VectorMath.IsClockwiseMarginXZ(a, b, p) && VectorMath.IsClockwiseMarginXZ(b, c, p))
		{
			return VectorMath.IsClockwiseMarginXZ(c, a, p);
		}
		return false;
	}

	public static bool ContainsPointXZ(Int3 a, Int3 b, Int3 c, Int3 p)
	{
		if (VectorMath.IsClockwiseOrColinearXZ(a, b, p) && VectorMath.IsClockwiseOrColinearXZ(b, c, p))
		{
			return VectorMath.IsClockwiseOrColinearXZ(c, a, p);
		}
		return false;
	}

	public static bool ContainsPoint(Int2 a, Int2 b, Int2 c, Int2 p)
	{
		if (VectorMath.IsClockwiseOrColinear(a, b, p) && VectorMath.IsClockwiseOrColinear(b, c, p))
		{
			return VectorMath.IsClockwiseOrColinear(c, a, p);
		}
		return false;
	}

	public static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
	{
		int num = polyPoints.Length - 1;
		bool flag = false;
		int num2 = 0;
		while (num2 < polyPoints.Length)
		{
			if (((polyPoints[num2].y <= p.y && p.y < polyPoints[num].y) || (polyPoints[num].y <= p.y && p.y < polyPoints[num2].y)) && p.x < (polyPoints[num].x - polyPoints[num2].x) * (p.y - polyPoints[num2].y) / (polyPoints[num].y - polyPoints[num2].y) + polyPoints[num2].x)
			{
				flag = !flag;
			}
			num = num2++;
		}
		return flag;
	}

	public static bool ContainsPointXZ(Vector3[] polyPoints, Vector3 p)
	{
		int num = polyPoints.Length - 1;
		bool flag = false;
		int num2 = 0;
		while (num2 < polyPoints.Length)
		{
			if (((polyPoints[num2].z <= p.z && p.z < polyPoints[num].z) || (polyPoints[num].z <= p.z && p.z < polyPoints[num2].z)) && p.x < (polyPoints[num].x - polyPoints[num2].x) * (p.z - polyPoints[num2].z) / (polyPoints[num].z - polyPoints[num2].z) + polyPoints[num2].x)
			{
				flag = !flag;
			}
			num = num2++;
		}
		return flag;
	}

	public static int SampleYCoordinateInTriangle(Int3 p1, Int3 p2, Int3 p3, Int3 p)
	{
		double num = (double)(p2.z - p3.z) * (double)(p1.x - p3.x) + (double)(p3.x - p2.x) * (double)(p1.z - p3.z);
		double num2 = ((double)(p2.z - p3.z) * (double)(p.x - p3.x) + (double)(p3.x - p2.x) * (double)(p.z - p3.z)) / num;
		double num3 = ((double)(p3.z - p1.z) * (double)(p.x - p3.x) + (double)(p1.x - p3.x) * (double)(p.z - p3.z)) / num;
		return (int)Math.Round(num2 * (double)p1.y + num3 * (double)p2.y + (1.0 - num2 - num3) * (double)p3.y);
	}

	public static Vector3[] ConvexHullXZ(Vector3[] points)
	{
		if (points.Length == 0)
		{
			return new Vector3[0];
		}
		List<Vector3> list = ListPool<Vector3>.Claim();
		int num = 0;
		for (int i = 1; i < points.Length; i++)
		{
			if (points[i].x < points[num].x)
			{
				num = i;
			}
		}
		int num2 = num;
		int num3 = 0;
		do
		{
			list.Add(points[num]);
			int num4 = 0;
			for (int j = 0; j < points.Length; j++)
			{
				if (num4 == num || !VectorMath.RightOrColinearXZ(points[num], points[num4], points[j]))
				{
					num4 = j;
				}
			}
			num = num4;
			num3++;
			if (num3 > 10000)
			{
				Debug.LogWarning("Infinite Loop in Convex Hull Calculation");
				break;
			}
		}
		while (num != num2);
		Vector3[] result = list.ToArray();
		ListPool<Vector3>.Release(list);
		return result;
	}

	public static Vector2 ClosestPointOnTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
	{
		Vector2 vector = b - a;
		Vector2 vector2 = c - a;
		Vector2 rhs = p - a;
		float num = Vector2.Dot(vector, rhs);
		float num2 = Vector2.Dot(vector2, rhs);
		if (num <= 0f && num2 <= 0f)
		{
			return a;
		}
		Vector2 rhs2 = p - b;
		float num3 = Vector2.Dot(vector, rhs2);
		float num4 = Vector2.Dot(vector2, rhs2);
		if (num3 >= 0f && num4 <= num3)
		{
			return b;
		}
		if (num >= 0f && num3 <= 0f && num * num4 - num3 * num2 <= 0f)
		{
			float num5 = num / (num - num3);
			return a + vector * num5;
		}
		Vector2 rhs3 = p - c;
		float num6 = Vector2.Dot(vector, rhs3);
		float num7 = Vector2.Dot(vector2, rhs3);
		if (num7 >= 0f && num6 <= num7)
		{
			return c;
		}
		if (num2 >= 0f && num7 <= 0f && num6 * num2 - num * num7 <= 0f)
		{
			float num8 = num2 / (num2 - num7);
			return a + vector2 * num8;
		}
		if (num4 - num3 >= 0f && num6 - num7 >= 0f && num3 * num7 - num6 * num4 <= 0f)
		{
			float num9 = (num4 - num3) / (num4 - num3 + (num6 - num7));
			return b + (c - b) * num9;
		}
		return p;
	}

	public static Vector3 ClosestPointOnTriangleXZ(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
	{
		Vector2 lhs = new Vector2(b.x - a.x, b.z - a.z);
		Vector2 lhs2 = new Vector2(c.x - a.x, c.z - a.z);
		Vector2 rhs = new Vector2(p.x - a.x, p.z - a.z);
		float num = Vector2.Dot(lhs, rhs);
		float num2 = Vector2.Dot(lhs2, rhs);
		if (num <= 0f && num2 <= 0f)
		{
			return a;
		}
		Vector2 rhs2 = new Vector2(p.x - b.x, p.z - b.z);
		float num3 = Vector2.Dot(lhs, rhs2);
		float num4 = Vector2.Dot(lhs2, rhs2);
		if (num3 >= 0f && num4 <= num3)
		{
			return b;
		}
		float num5 = num * num4 - num3 * num2;
		if (num >= 0f && num3 <= 0f && num5 <= 0f)
		{
			float num6 = num / (num - num3);
			return (1f - num6) * a + num6 * b;
		}
		Vector2 rhs3 = new Vector2(p.x - c.x, p.z - c.z);
		float num7 = Vector2.Dot(lhs, rhs3);
		float num8 = Vector2.Dot(lhs2, rhs3);
		if (num8 >= 0f && num7 <= num8)
		{
			return c;
		}
		float num9 = num7 * num2 - num * num8;
		if (num2 >= 0f && num8 <= 0f && num9 <= 0f)
		{
			float num10 = num2 / (num2 - num8);
			return (1f - num10) * a + num10 * c;
		}
		float num11 = num3 * num8 - num7 * num4;
		if (num4 - num3 >= 0f && num7 - num8 >= 0f && num11 <= 0f)
		{
			float num12 = (num4 - num3) / (num4 - num3 + (num7 - num8));
			return b + (c - b) * num12;
		}
		float num13 = 1f / (num11 + num9 + num5);
		float num14 = num9 * num13;
		float num15 = num5 * num13;
		return new Vector3(p.x, (1f - num14 - num15) * a.y + num14 * b.y + num15 * c.y, p.z);
	}

	public static Vector3 ClosestPointOnTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
	{
		Vector3 vector = b - a;
		Vector3 vector2 = c - a;
		Vector3 rhs = p - a;
		float num = Vector3.Dot(vector, rhs);
		float num2 = Vector3.Dot(vector2, rhs);
		if (num <= 0f && num2 <= 0f)
		{
			return a;
		}
		Vector3 rhs2 = p - b;
		float num3 = Vector3.Dot(vector, rhs2);
		float num4 = Vector3.Dot(vector2, rhs2);
		if (num3 >= 0f && num4 <= num3)
		{
			return b;
		}
		float num5 = num * num4 - num3 * num2;
		if (num >= 0f && num3 <= 0f && num5 <= 0f)
		{
			float num6 = num / (num - num3);
			return a + vector * num6;
		}
		Vector3 rhs3 = p - c;
		float num7 = Vector3.Dot(vector, rhs3);
		float num8 = Vector3.Dot(vector2, rhs3);
		if (num8 >= 0f && num7 <= num8)
		{
			return c;
		}
		float num9 = num7 * num2 - num * num8;
		if (num2 >= 0f && num8 <= 0f && num9 <= 0f)
		{
			float num10 = num2 / (num2 - num8);
			return a + vector2 * num10;
		}
		float num11 = num3 * num8 - num7 * num4;
		if (num4 - num3 >= 0f && num7 - num8 >= 0f && num11 <= 0f)
		{
			float num12 = (num4 - num3) / (num4 - num3 + (num7 - num8));
			return b + (c - b) * num12;
		}
		float num13 = 1f / (num11 + num9 + num5);
		float num14 = num9 * num13;
		float num15 = num5 * num13;
		return a + vector * num14 + vector2 * num15;
	}

	public static void CompressMesh(List<Int3> vertices, List<int> triangles, out Int3[] outVertices, out int[] outTriangles)
	{
		Dictionary<Int3, int> dictionary = cached_Int3_int_dict;
		dictionary.Clear();
		int[] array = ArrayPool<int>.Claim(vertices.Count);
		int num = 0;
		for (int i = 0; i < vertices.Count; i++)
		{
			if (!dictionary.TryGetValue(vertices[i], out var value) && !dictionary.TryGetValue(vertices[i] + new Int3(0, 1, 0), out value) && !dictionary.TryGetValue(vertices[i] + new Int3(0, -1, 0), out value))
			{
				dictionary.Add(vertices[i], num);
				array[i] = num;
				vertices[num] = vertices[i];
				num++;
			}
			else
			{
				array[i] = value;
			}
		}
		outTriangles = new int[triangles.Count];
		for (int j = 0; j < outTriangles.Length; j++)
		{
			outTriangles[j] = array[triangles[j]];
		}
		outVertices = new Int3[num];
		for (int k = 0; k < num; k++)
		{
			outVertices[k] = vertices[k];
		}
		ArrayPool<int>.Release(ref array);
	}

	public static void TraceContours(Dictionary<int, int> outline, HashSet<int> hasInEdge, Action<List<int>, bool> results)
	{
		List<int> list = ListPool<int>.Claim();
		List<int> list2 = ListPool<int>.Claim();
		list2.AddRange(outline.Keys);
		for (int i = 0; i <= 1; i++)
		{
			bool flag = i == 1;
			for (int j = 0; j < list2.Count; j++)
			{
				int num = list2[j];
				if (!flag && hasInEdge.Contains(num))
				{
					continue;
				}
				int num2 = num;
				list.Clear();
				list.Add(num2);
				while (outline.ContainsKey(num2))
				{
					int num3 = outline[num2];
					outline.Remove(num2);
					list.Add(num3);
					if (num3 == num)
					{
						break;
					}
					num2 = num3;
				}
				if (list.Count > 1)
				{
					results(list, flag);
				}
			}
		}
		ListPool<int>.Release(ref list2);
		ListPool<int>.Release(ref list);
	}

	public static void Subdivide(List<Vector3> points, List<Vector3> result, int subSegments)
	{
		for (int i = 0; i < points.Count - 1; i++)
		{
			for (int j = 0; j < subSegments; j++)
			{
				result.Add(Vector3.Lerp(points[i], points[i + 1], (float)j / (float)subSegments));
			}
		}
		result.Add(points[points.Count - 1]);
	}
}
