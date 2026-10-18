using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder.Poly2Tri;

namespace UnityEngine.ProBuilder.MeshOperations;

internal static class Triangulation
{
	private static TriangulationContext s_TriangulationContext;

	private static TriangulationContext triangulationContext
	{
		get
		{
			if (s_TriangulationContext == null)
			{
				s_TriangulationContext = new DTSweepContext();
			}
			return s_TriangulationContext;
		}
	}

	public static bool SortAndTriangulate(IList<Vector2> points, out List<int> indexes, bool convex = false)
	{
		IList<Vector2> list = Projection.Sort(points);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		for (int i = 0; i < list.Count; i++)
		{
			dictionary.Add(i, points.IndexOf(list[i]));
		}
		if (!Triangulate(list, out indexes, convex))
		{
			return false;
		}
		for (int j = 0; j < indexes.Count; j++)
		{
			indexes[j] = dictionary[indexes[j]];
		}
		return true;
	}

	public static bool TriangulateVertices(IList<Vertex> vertices, out List<int> triangles, bool unordered = true, bool convex = false)
	{
		Vector3[] array = new Vector3[vertices.Count];
		for (int i = 0; i < vertices.Count; i++)
		{
			array[i] = vertices[i].position;
		}
		return TriangulateVertices(array, out triangles, unordered, convex);
	}

	public static bool TriangulateVertices(Vector3[] vertices, out List<int> triangles, Vector3[][] holes = null)
	{
		triangles = null;
		if (((vertices != null) ? vertices.Length : 0) < 3)
		{
			return false;
		}
		Vector3 normal = Projection.FindBestPlane(vertices).normal;
		Vector2[] points = Projection.PlanarProject(vertices, null, normal);
		Vector2[][] array = null;
		if (holes != null)
		{
			array = new Vector2[holes.Length][];
			for (int i = 0; i < holes.Length; i++)
			{
				if (holes[i].Length < 3)
				{
					return false;
				}
				array[i] = Projection.PlanarProject(holes[i], null, normal);
			}
		}
		return Triangulate(points, array, out triangles);
	}

	public static bool TriangulateVertices(Vector3[] vertices, out List<int> triangles, bool unordered = true, bool convex = false)
	{
		triangles = null;
		int num = ((vertices != null) ? vertices.Length : 0);
		if (num < 3)
		{
			return false;
		}
		if (num == 3)
		{
			triangles = new List<int> { 0, 1, 2 };
			return true;
		}
		Vector2[] points = Projection.PlanarProject(vertices);
		if (unordered)
		{
			return SortAndTriangulate(points, out triangles, convex);
		}
		return Triangulate(points, out triangles, convex);
	}

	public static bool Triangulate(IList<Vector2> points, out List<int> indexes, bool convex = false)
	{
		indexes = new List<int>();
		int index = 0;
		Triangulatable triangulatable2;
		if (!convex)
		{
			Triangulatable triangulatable = new Polygon(points.Select((Vector2 x) => new PolygonPoint(x.x, x.y, index++)));
			triangulatable2 = triangulatable;
		}
		else
		{
			Triangulatable triangulatable = new PointSet(points.Select((Vector2 x) => new TriangulationPoint(x.x, x.y, index++)).ToList());
			triangulatable2 = triangulatable;
		}
		Triangulatable triangulatable3 = triangulatable2;
		try
		{
			triangulationContext.Clear();
			triangulationContext.PrepareTriangulation(triangulatable3);
			DTSweep.Triangulate((DTSweepContext)triangulationContext);
		}
		catch (Exception ex)
		{
			Log.Info("Triangulation failed: " + ex.ToString());
			return false;
		}
		foreach (DelaunayTriangle triangle in triangulatable3.Triangles)
		{
			if (triangle.Points[0].Index < 0 || triangle.Points[1].Index < 0 || triangle.Points[2].Index < 0)
			{
				Log.Info("Triangulation failed: Additional vertices were inserted.");
				return false;
			}
			indexes.Add(triangle.Points[0].Index);
			indexes.Add(triangle.Points[1].Index);
			indexes.Add(triangle.Points[2].Index);
		}
		WindingOrder windingOrder = SurfaceTopology.GetWindingOrder(points);
		if (SurfaceTopology.GetWindingOrder(new Vector2[3]
		{
			points[indexes[0]],
			points[indexes[1]],
			points[indexes[2]]
		}) != windingOrder)
		{
			indexes.Reverse();
		}
		return true;
	}

	public static bool Triangulate(IList<Vector2> points, IList<IList<Vector2>> holes, out List<int> indexes)
	{
		indexes = new List<int>();
		int index = 0;
		List<Vector2> list = new List<Vector2>(points);
		Polygon polygon = new Polygon(points.Select((Vector2 x) => new PolygonPoint(x.x, x.y, index++)));
		if (holes != null)
		{
			for (int i = 0; i < holes.Count; i++)
			{
				list.AddRange(holes[i]);
				Polygon poly = new Polygon(holes[i].Select((Vector2 x) => new PolygonPoint(x.x, x.y, index++)));
				polygon.AddHole(poly);
			}
		}
		try
		{
			triangulationContext.Clear();
			triangulationContext.PrepareTriangulation(polygon);
			DTSweep.Triangulate((DTSweepContext)triangulationContext);
		}
		catch (Exception ex)
		{
			Log.Info("Triangulation failed: " + ex.ToString());
			return false;
		}
		foreach (DelaunayTriangle triangle in polygon.Triangles)
		{
			if (triangle.Points[0].Index < 0 || triangle.Points[1].Index < 0 || triangle.Points[2].Index < 0)
			{
				Log.Info("Triangulation failed: Additional vertices were inserted.");
				return false;
			}
			indexes.Add(triangle.Points[0].Index);
			indexes.Add(triangle.Points[1].Index);
			indexes.Add(triangle.Points[2].Index);
		}
		WindingOrder windingOrder = SurfaceTopology.GetWindingOrder(points);
		if (SurfaceTopology.GetWindingOrder(new Vector2[3]
		{
			list[indexes[0]],
			list[indexes[1]],
			list[indexes[2]]
		}) != windingOrder)
		{
			indexes.Reverse();
		}
		return true;
	}
}
