using System;
using System.Collections.Generic;

namespace g3;

public static class CurveUtils2
{
	public static IParametricCurve2d Convert(Polygon2d poly)
	{
		ParametricCurveSequence2 parametricCurveSequence = new ParametricCurveSequence2();
		int vertexCount = poly.VertexCount;
		for (int i = 0; i < vertexCount; i++)
		{
			parametricCurveSequence.Append(new Segment2d(poly[i], poly[(i + 1) % vertexCount]));
		}
		parametricCurveSequence.IsClosed = true;
		return parametricCurveSequence;
	}

	public static double SampledDistance(IParametricCurve2d c, Vector2d point, int N = 100)
	{
		double paramLength = c.ParamLength;
		double num = double.MaxValue;
		for (int i = 0; i <= N; i++)
		{
			double num2 = (double)i / (double)N;
			num2 *= paramLength;
			double num3 = c.SampleT(num2).DistanceSquared(point);
			if (num3 < num)
			{
				num = num3;
			}
		}
		return Math.Sqrt(num);
	}

	public static IEnumerable<IParametricCurve2d> LeafCurvesIteration(IParametricCurve2d c)
	{
		if (c is IMultiCurve2d)
		{
			IMultiCurve2d multiCurve2d = c as IMultiCurve2d;
			foreach (IParametricCurve2d curf in multiCurve2d.Curves)
			{
				foreach (IParametricCurve2d item in LeafCurvesIteration(curf))
				{
					yield return item;
				}
			}
		}
		else
		{
			yield return c;
		}
	}

	public static List<IParametricCurve2d> Flatten(List<IParametricCurve2d> curves)
	{
		List<IParametricCurve2d> list = new List<IParametricCurve2d>();
		foreach (IParametricCurve2d curf in curves)
		{
			foreach (IParametricCurve2d item in LeafCurvesIteration(curf))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static List<IParametricCurve2d> Flatten(IParametricCurve2d curve)
	{
		return new List<IParametricCurve2d>(LeafCurvesIteration(curve));
	}

	public static Vector2d GetMaxOriginDistances(IEnumerable<Vector2d> vertices)
	{
		Vector2d zero = Vector2d.Zero;
		foreach (Vector2d vertex in vertices)
		{
			double num = Math.Abs(vertex.x);
			if (num > zero.x)
			{
				zero.x = num;
			}
			double num2 = Math.Abs(vertex.y);
			if (num2 > zero.y)
			{
				zero.y = num2;
			}
		}
		return zero;
	}

	public static int FindNearestVertex(Vector2d pt, IEnumerable<Vector2d> vertices)
	{
		int num = 0;
		int result = -1;
		double num2 = double.MaxValue;
		foreach (Vector2d vertex in vertices)
		{
			double num3 = vertex.DistanceSquared(pt);
			if (num3 < num2)
			{
				num2 = num3;
				result = num;
			}
			num++;
		}
		return result;
	}

	public static Vector2d CentroidVtx(IEnumerable<Vector2d> vertices)
	{
		Vector2d zero = Vector2d.Zero;
		int num = 0;
		foreach (Vector2d vertex in vertices)
		{
			zero += vertex;
			num++;
		}
		if (num > 1)
		{
			zero /= (double)num;
		}
		return zero;
	}

	public static void LaplacianSmooth(IList<Vector2d> vertices, double alpha, int iterations, bool is_loop, bool in_place = false)
	{
		int count = vertices.Count;
		Vector2d[] array = null;
		if (!in_place)
		{
			array = new Vector2d[count];
		}
		IList<Vector2d> list2;
		if (!in_place)
		{
			IList<Vector2d> list = array;
			list2 = list;
		}
		else
		{
			list2 = vertices;
		}
		IList<Vector2d> list3 = list2;
		double num = 1.0 - alpha;
		for (int i = 0; i < iterations; i++)
		{
			if (is_loop)
			{
				for (int j = 0; j < count; j++)
				{
					Vector2d vector2d = (vertices[(j + count - 1) % count] + vertices[(j + 1) % count]) * 0.5;
					list3[j] = num * vertices[j] + alpha * vector2d;
				}
			}
			else
			{
				list3[0] = vertices[0];
				list3[count - 1] = vertices[count - 1];
				for (int k = 1; k < count - 1; k++)
				{
					Vector2d vector2d2 = (vertices[k - 1] + vertices[k + 1]) * 0.5;
					list3[k] = num * vertices[k] + alpha * vector2d2;
				}
			}
			if (!in_place)
			{
				for (int l = 0; l < count; l++)
				{
					vertices[l] = list3[l];
				}
			}
		}
	}

	public static void LaplacianSmoothConstrained(Polygon2d poly, double alpha, int iterations, double max_dist, bool bAllowShrink, bool bAllowGrow, bool bPerVertexDistances = true)
	{
		Polygon2d polygon2d = new Polygon2d(poly);
		int vertexCount = poly.VertexCount;
		Vector2d[] array = new Vector2d[poly.VertexCount];
		double num = max_dist * max_dist;
		double num2 = 1.0 - alpha;
		for (int i = 0; i < iterations; i++)
		{
			for (int j = 0; j < vertexCount; j++)
			{
				Vector2d vector2d = poly[j];
				Vector2d vector2d2 = (poly[(j + vertexCount - 1) % vertexCount] + poly[(j + 1) % vertexCount]) * 0.5;
				bool flag = true;
				if (!bAllowShrink || !bAllowGrow)
				{
					flag = ((!polygon2d.Contains(vector2d2)) ? bAllowGrow : bAllowShrink);
				}
				if (flag)
				{
					Vector2d vector2d3 = num2 * vector2d + alpha * vector2d2;
					if (bPerVertexDistances)
					{
						while (polygon2d[j].DistanceSquared(vector2d3) > num)
						{
							vector2d3 = (vector2d3 + vector2d) * 0.5;
						}
					}
					else
					{
						while (polygon2d.DistanceSquared(vector2d3) > num)
						{
							vector2d3 = (vector2d3 + vector2d) * 0.5;
						}
					}
					array[j] = vector2d3;
				}
				else
				{
					array[j] = vector2d;
				}
			}
			for (int k = 0; k < vertexCount; k++)
			{
				poly[k] = array[k];
			}
		}
	}

	public static void LaplacianSmoothConstrained(GeneralPolygon2d solid, double alpha, int iterations, double max_dist, bool bAllowShrink, bool bAllowGrow)
	{
		LaplacianSmoothConstrained(solid.Outer, alpha, iterations, max_dist, bAllowShrink, bAllowGrow);
		foreach (Polygon2d hole in solid.Holes)
		{
			LaplacianSmoothConstrained(hole, alpha, iterations, max_dist, bAllowShrink, bAllowGrow);
		}
	}

	public static List<T> Filter<T>(List<T> objects, Func<T, bool> keepF)
	{
		List<T> list = new List<T>(objects.Count);
		foreach (T @object in objects)
		{
			if (keepF(@object))
			{
				list.Add(@object);
			}
		}
		return list;
	}

	public static void Split<T>(List<T> objects, out List<T> set1, out List<T> set2, Func<T, bool> splitF)
	{
		set1 = new List<T>();
		set2 = new List<T>();
		foreach (T @object in objects)
		{
			if (splitF(@object))
			{
				set1.Add(@object);
			}
			else
			{
				set2.Add(@object);
			}
		}
	}

	public static Polygon2d SplitToTargetLength(Polygon2d poly, double length)
	{
		Polygon2d polygon2d = new Polygon2d();
		polygon2d.AppendVertex(poly[0]);
		for (int i = 0; i < poly.VertexCount; i++)
		{
			int key = (i + 1) % poly.VertexCount;
			double num = poly[i].Distance(poly[key]);
			if (num < length)
			{
				polygon2d.AppendVertex(poly[key]);
				continue;
			}
			int num2 = (int)Math.Ceiling(num / length);
			for (int j = 1; j < num2; j++)
			{
				double num3 = (double)j / (double)num2;
				Vector2d v = (1.0 - num3) * poly[i] + num3 * poly[key];
				polygon2d.AppendVertex(v);
			}
			if (i < poly.VertexCount - 1)
			{
				polygon2d.AppendVertex(poly[key]);
			}
		}
		return polygon2d;
	}

	public static List<GeneralPolygon2d> FilterDegenerate(List<GeneralPolygon2d> polygons, double minArea)
	{
		List<GeneralPolygon2d> list = new List<GeneralPolygon2d>(polygons.Count);
		List<Polygon2d> list2 = new List<Polygon2d>();
		foreach (GeneralPolygon2d polygon in polygons)
		{
			if (polygon.Outer.Area < minArea)
			{
				continue;
			}
			if (polygon.Holes.Count == 0)
			{
				list.Add(polygon);
				continue;
			}
			list2.Clear();
			for (int i = 0; i < polygon.Holes.Count; i++)
			{
				Polygon2d polygon2d = polygon.Holes[i];
				if (polygon2d.Area > minArea)
				{
					list2.Add(polygon2d);
				}
			}
			if (list2.Count != polygon.Holes.Count)
			{
				polygon.ClearHoles();
				foreach (Polygon2d item in list2)
				{
					polygon.AddHole(item, bCheckContainment: false, bCheckOrientation: false);
				}
			}
			list.Add(polygon);
		}
		return list;
	}
}
