using System;
using System.Collections.Generic;

namespace g3;

public static class MeshMeasurements
{
	public struct GenusResult
	{
		public bool Valid;

		public int Genus;

		public bool HasBoundary;

		public bool MultipleConnectedComponents;

		public bool HasBowtieVertices;
	}

	public static void MassProperties(IEnumerable<Index3i> triangle_indices, Func<int, Vector3d> getVertexF, out double mass, out Vector3d center, out double[,] inertia3x3, bool bodyCoords = false)
	{
		double[] array = new double[10];
		foreach (Index3i triangle_index in triangle_indices)
		{
			Vector3d vector3d = getVertexF(triangle_index.a);
			Vector3d vector3d2 = getVertexF(triangle_index.b);
			Vector3d vector3d3 = getVertexF(triangle_index.c);
			Vector3d vector3d4 = vector3d2 - vector3d;
			Vector3d v = vector3d3 - vector3d;
			Vector3d vector3d5 = vector3d4.Cross(v);
			double num = vector3d.x + vector3d2.x;
			double num2 = num + vector3d3.x;
			double num3 = vector3d.x * vector3d.x;
			double num4 = num3 + vector3d2.x * num;
			double num5 = num4 + vector3d3.x * num2;
			double num6 = vector3d.x * num3 + vector3d2.x * num4 + vector3d3.x * num5;
			double num7 = num5 + vector3d.x * (num2 + vector3d.x);
			double num8 = num5 + vector3d2.x * (num2 + vector3d2.x);
			double num9 = num5 + vector3d3.x * (num2 + vector3d3.x);
			num = vector3d.y + vector3d2.y;
			double num10 = num + vector3d3.y;
			num3 = vector3d.y * vector3d.y;
			num4 = num3 + vector3d2.y * num;
			double num11 = num4 + vector3d3.y * num10;
			double num12 = vector3d.y * num3 + vector3d2.y * num4 + vector3d3.y * num11;
			double num13 = num11 + vector3d.y * (num10 + vector3d.y);
			double num14 = num11 + vector3d2.y * (num10 + vector3d2.y);
			double num15 = num11 + vector3d3.y * (num10 + vector3d3.y);
			num = vector3d.z + vector3d2.z;
			double num16 = num + vector3d3.z;
			num3 = vector3d.z * vector3d.z;
			num4 = num3 + vector3d2.z * num;
			double num17 = num4 + vector3d3.z * num16;
			double num18 = vector3d.z * num3 + vector3d2.z * num4 + vector3d3.z * num17;
			double num19 = num17 + vector3d.z * (num16 + vector3d.z);
			double num20 = num17 + vector3d2.z * (num16 + vector3d2.z);
			double num21 = num17 + vector3d3.z * (num16 + vector3d3.z);
			array[0] += vector3d5.x * num2;
			array[1] += vector3d5.x * num5;
			array[2] += vector3d5.y * num11;
			array[3] += vector3d5.z * num17;
			array[4] += vector3d5.x * num6;
			array[5] += vector3d5.y * num12;
			array[6] += vector3d5.z * num18;
			array[7] += vector3d5.x * (vector3d.y * num7 + vector3d2.y * num8 + vector3d3.y * num9);
			array[8] += vector3d5.y * (vector3d.z * num13 + vector3d2.z * num14 + vector3d3.z * num15);
			array[9] += vector3d5.z * (vector3d.x * num19 + vector3d2.x * num20 + vector3d3.x * num21);
		}
		array[0] *= 1.0 / 6.0;
		array[1] *= 1.0 / 24.0;
		array[2] *= 1.0 / 24.0;
		array[3] *= 1.0 / 24.0;
		array[4] *= 1.0 / 60.0;
		array[5] *= 1.0 / 60.0;
		array[6] *= 1.0 / 60.0;
		array[7] *= 1.0 / 120.0;
		array[8] *= 1.0 / 120.0;
		array[9] *= 1.0 / 120.0;
		mass = array[0];
		center = new Vector3d(array[1], array[2], array[3]) / mass;
		inertia3x3 = new double[3, 3];
		inertia3x3[0, 0] = array[5] + array[6];
		inertia3x3[0, 1] = 0.0 - array[7];
		inertia3x3[0, 2] = 0.0 - array[9];
		inertia3x3[1, 0] = inertia3x3[0, 1];
		inertia3x3[1, 1] = array[4] + array[6];
		inertia3x3[1, 2] = 0.0 - array[8];
		inertia3x3[2, 0] = inertia3x3[0, 2];
		inertia3x3[2, 1] = inertia3x3[1, 2];
		inertia3x3[2, 2] = array[4] + array[5];
		if (bodyCoords)
		{
			inertia3x3[0, 0] -= mass * (center.y * center.y + center.z * center.z);
			inertia3x3[0, 1] += mass * center.x * center.y;
			inertia3x3[0, 2] += mass * center.z * center.x;
			inertia3x3[1, 0] = inertia3x3[0, 1];
			inertia3x3[1, 1] -= mass * (center.z * center.z + center.x * center.x);
			inertia3x3[1, 2] += mass * center.y * center.z;
			inertia3x3[2, 0] = inertia3x3[0, 2];
			inertia3x3[2, 1] = inertia3x3[1, 2];
			inertia3x3[2, 2] -= mass * (center.x * center.x + center.y * center.y);
		}
	}

	public static void MassProperties(DMesh3 mesh, out double mass, out Vector3d center, out double[,] inertia3x3, bool bodyCoords = false)
	{
		MassProperties(mesh.Triangles(), (int vID) => mesh.GetVertex(vID), out mass, out center, out inertia3x3);
	}

	public static Vector2d VolumeArea(DMesh3 mesh, IEnumerable<int> triangles, Func<int, Vector3d> getVertexF)
	{
		double num = 0.0;
		double num2 = 0.0;
		foreach (int triangle2 in triangles)
		{
			Index3i triangle = mesh.GetTriangle(triangle2);
			Vector3d vector3d = getVertexF(triangle.a);
			Vector3d vector3d2 = getVertexF(triangle.b);
			Vector3d vector3d3 = getVertexF(triangle.c);
			Vector3d vector3d4 = vector3d2 - vector3d;
			Vector3d v = vector3d3 - vector3d;
			Vector3d vector3d5 = vector3d4.Cross(v);
			num2 += 0.5 * vector3d5.Length;
			double num3 = vector3d.x + vector3d2.x + vector3d3.x;
			num += vector3d5.x * num3;
		}
		return new Vector2d(num * (1.0 / 6.0), num2);
	}

	public static double VertexOneRingArea(DMesh3 mesh, int vid, bool bDisjoint = true)
	{
		double num = 0.0;
		double num2 = (bDisjoint ? (1.0 / 3.0) : 1.0);
		foreach (int item in mesh.VtxTrianglesItr(vid))
		{
			num += mesh.GetTriArea(item) * num2;
		}
		return num;
	}

	public static Vector3d Centroid(IEnumerable<Vector3d> vertices)
	{
		Vector3d zero = Vector3d.Zero;
		int num = 0;
		foreach (Vector3d vertex in vertices)
		{
			zero += vertex;
			num++;
		}
		return zero / num;
	}

	public static Vector3d Centroid<T>(IEnumerable<T> values, Func<T, Vector3d> PositionF)
	{
		Vector3d zero = Vector3d.Zero;
		int num = 0;
		foreach (T value in values)
		{
			zero += PositionF(value);
			num++;
		}
		return zero / num;
	}

	public static Vector3d Centroid(DMesh3 mesh, bool bOnlyTriVertices = true)
	{
		if (bOnlyTriVertices)
		{
			Vector3d zero = Vector3d.Zero;
			int num = 0;
			foreach (int item in mesh.VertexIndices())
			{
				if (mesh.GetVtxEdgeCount(item) > 0)
				{
					zero += mesh.GetVertex(item);
					num++;
				}
			}
			return zero / num;
		}
		return Centroid(mesh.Vertices());
	}

	public static AxisAlignedBox3d Bounds(DMesh3 mesh, Func<Vector3d, Vector3d> TransformF)
	{
		AxisAlignedBox3d empty = AxisAlignedBox3d.Empty;
		if (TransformF == null)
		{
			foreach (Vector3d item in mesh.Vertices())
			{
				empty.Contain(item);
			}
		}
		else
		{
			foreach (Vector3d item2 in mesh.Vertices())
			{
				Vector3d v = TransformF(item2);
				empty.Contain(ref v);
			}
		}
		return empty;
	}

	public static AxisAlignedBox3d Bounds(IMesh mesh, Func<Vector3d, Vector3d> TransformF)
	{
		AxisAlignedBox3d empty = AxisAlignedBox3d.Empty;
		if (TransformF == null)
		{
			foreach (int item in mesh.VertexIndices())
			{
				empty.Contain(mesh.GetVertex(item));
			}
		}
		else
		{
			foreach (int item2 in mesh.VertexIndices())
			{
				Vector3d v = TransformF(mesh.GetVertex(item2));
				empty.Contain(ref v);
			}
		}
		return empty;
	}

	public static AxisAlignedBox3d BoundsV(IMesh mesh, IEnumerable<int> vertexIndices, Func<Vector3d, Vector3d> TransformF = null)
	{
		AxisAlignedBox3d empty = AxisAlignedBox3d.Empty;
		if (TransformF == null)
		{
			foreach (int vertexIndex in vertexIndices)
			{
				empty.Contain(mesh.GetVertex(vertexIndex));
			}
		}
		else
		{
			foreach (int vertexIndex2 in vertexIndices)
			{
				empty.Contain(TransformF(mesh.GetVertex(vertexIndex2)));
			}
		}
		return empty;
	}

	public static AxisAlignedBox3d BoundsT(IMesh mesh, IEnumerable<int> triangleIndices, Func<Vector3d, Vector3d> TransformF = null)
	{
		AxisAlignedBox3d empty = AxisAlignedBox3d.Empty;
		if (TransformF == null)
		{
			foreach (int triangleIndex in triangleIndices)
			{
				Index3i triangle = mesh.GetTriangle(triangleIndex);
				for (int i = 0; i < 3; i++)
				{
					empty.Contain(mesh.GetVertex(triangle[i]));
				}
			}
		}
		else
		{
			foreach (int triangleIndex2 in triangleIndices)
			{
				Index3i triangle2 = mesh.GetTriangle(triangleIndex2);
				for (int j = 0; j < 3; j++)
				{
					empty.Contain(TransformF(mesh.GetVertex(triangle2[j])));
				}
			}
		}
		return empty;
	}

	public static double AreaT(DMesh3 mesh, IEnumerable<int> triangleIndices)
	{
		double num = 0.0;
		foreach (int triangleIndex in triangleIndices)
		{
			num += mesh.GetTriArea(triangleIndex);
		}
		return num;
	}

	public static AxisAlignedBox3d BoundsInFrame(DMesh3 mesh, Frame3f frame, Func<Vector3d, Vector3d> TransformF = null)
	{
		AxisAlignedBox3d empty = AxisAlignedBox3d.Empty;
		if (TransformF == null)
		{
			foreach (Vector3d item in mesh.Vertices())
			{
				Vector3d v = frame.ToFrameP(item);
				empty.Contain(ref v);
			}
		}
		else
		{
			foreach (Vector3d item2 in mesh.Vertices())
			{
				Vector3d v2 = TransformF(item2);
				Vector3d v3 = frame.ToFrameP(ref v2);
				empty.Contain(ref v3);
			}
		}
		return empty;
	}

	public static Interval1d ExtentsOnAxis(DMesh3 mesh, Vector3d axis, Func<Vector3d, Vector3d> TransformF = null)
	{
		Interval1d empty = Interval1d.Empty;
		if (TransformF == null)
		{
			foreach (Vector3d item in mesh.Vertices())
			{
				empty.Contain(item.Dot(ref axis));
			}
		}
		else
		{
			foreach (Vector3d item2 in mesh.Vertices())
			{
				empty.Contain(TransformF(item2).Dot(ref axis));
			}
		}
		return empty;
	}

	public static Interval1d ExtentsOnAxis(IMesh mesh, Vector3d axis, Func<Vector3d, Vector3d> TransformF = null)
	{
		Interval1d empty = Interval1d.Empty;
		if (TransformF == null)
		{
			foreach (int item in mesh.VertexIndices())
			{
				empty.Contain(mesh.GetVertex(item).Dot(ref axis));
			}
		}
		else
		{
			foreach (int item2 in mesh.VertexIndices())
			{
				empty.Contain(TransformF(mesh.GetVertex(item2)).Dot(ref axis));
			}
		}
		return empty;
	}

	public static Interval1i ExtremeVertices(DMesh3 mesh, Vector3d axis, Func<Vector3d, Vector3d> TransformF = null)
	{
		Interval1d empty = Interval1d.Empty;
		Interval1i result = new Interval1i(-1, -1);
		if (TransformF == null)
		{
			foreach (int item in mesh.VertexIndices())
			{
				double num = mesh.GetVertex(item).Dot(ref axis);
				if (num < empty.a)
				{
					empty.a = num;
					result.a = item;
				}
				else if (num > empty.b)
				{
					empty.b = num;
					result.b = item;
				}
			}
		}
		else
		{
			foreach (int item2 in mesh.VertexIndices())
			{
				double num2 = TransformF(mesh.GetVertex(item2)).Dot(ref axis);
				if (num2 < empty.a)
				{
					empty.a = num2;
					result.a = item2;
				}
				else if (num2 > empty.b)
				{
					empty.b = num2;
					result.b = item2;
				}
			}
		}
		return result;
	}

	public static GenusResult Genus(DMesh3 mesh)
	{
		GenusResult genusResult = default(GenusResult);
		genusResult.Valid = false;
		genusResult.Genus = -1;
		GenusResult result = genusResult;
		if (!mesh.CachedIsClosed)
		{
			result.HasBoundary = true;
			return result;
		}
		MeshConnectedComponents meshConnectedComponents = new MeshConnectedComponents(mesh);
		meshConnectedComponents.FindConnectedT();
		if (meshConnectedComponents.Count > 1)
		{
			result.MultipleConnectedComponents = true;
			return result;
		}
		int num = 0;
		foreach (int item in mesh.VertexIndices())
		{
			if (mesh.IsBowtieVertex(item))
			{
				result.HasBowtieVertices = true;
				return result;
			}
			if (mesh.GetVtxTriangleCount(item) == 0)
			{
				num++;
			}
		}
		int num2 = mesh.VertexCount - num;
		int triangleCount = mesh.TriangleCount;
		int edgeCount = mesh.EdgeCount;
		result.Genus = (2 - (num2 + triangleCount - edgeCount)) / 2;
		result.Valid = true;
		return result;
	}
}
