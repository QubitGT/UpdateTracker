using System;
using System.Collections.Generic;
using g3;

namespace gs;

public class RemoveDuplicateTriangles
{
	private class TriCentroids : IPointSet
	{
		public DMesh3 Mesh;

		public int VertexCount => Mesh.TriangleCount;

		public int MaxVertexID => Mesh.MaxTriangleID;

		public bool HasVertexNormals => false;

		public bool HasVertexColors => false;

		public int Timestamp => Mesh.Timestamp;

		public Vector3d GetVertex(int i)
		{
			return Mesh.GetTriCentroid(i);
		}

		public Vector3f GetVertexNormal(int i)
		{
			return Vector3f.AxisY;
		}

		public Vector3f GetVertexColor(int i)
		{
			return Vector3f.One;
		}

		public bool IsVertex(int tID)
		{
			return Mesh.IsTriangle(tID);
		}

		public IEnumerable<int> VertexIndices()
		{
			return Mesh.TriangleIndices();
		}
	}

	public DMesh3 Mesh;

	public double VertexTolerance = 9.999999974752427E-07;

	public bool CheckOrientation = true;

	public int Removed;

	public RemoveDuplicateTriangles(DMesh3 mesh)
	{
		Mesh = mesh;
	}

	public virtual bool Apply()
	{
		Removed = 0;
		double tolSqr = VertexTolerance * VertexTolerance;
		PointSetHashtable pointSetHashtable = new PointSetHashtable(new TriCentroids
		{
			Mesh = Mesh
		});
		int maxAxisSubdivs = ((Mesh.TriangleCount > 100000) ? 128 : 64);
		pointSetHashtable.Build(maxAxisSubdivs);
		Vector3d v = Vector3d.Zero;
		Vector3d v2 = Vector3d.Zero;
		Vector3d v3 = Vector3d.Zero;
		Vector3d v4 = Vector3d.Zero;
		Vector3d v5 = Vector3d.Zero;
		Vector3d v6 = Vector3d.Zero;
		int maxTriangleID = Mesh.MaxTriangleID;
		int[] array = new int[1024];
		for (int i = 0; i < maxTriangleID; i++)
		{
			if (!Mesh.IsTriangle(i))
			{
				continue;
			}
			Vector3d triCentroid = Mesh.GetTriCentroid(i);
			int buffer_count;
			while (!pointSetHashtable.FindInBall(triCentroid, VertexTolerance, array, out buffer_count))
			{
				array = new int[array.Length];
			}
			if (buffer_count == 1 && array[0] != i)
			{
				throw new Exception("RemoveDuplicateTriangles.Apply: how could this happen?!");
			}
			if (buffer_count <= 1)
			{
				continue;
			}
			Mesh.GetTriVertices(i, ref v, ref v2, ref v3);
			Vector3d vector3d = MathUtil.Normal(v, v2, v3);
			for (int j = 0; j < buffer_count; j++)
			{
				if (array[j] == i)
				{
					continue;
				}
				Mesh.GetTriVertices(array[j], ref v4, ref v5, ref v6);
				if (!is_same_triangle(ref v, ref v2, ref v3, ref v4, ref v5, ref v6, tolSqr))
				{
					continue;
				}
				if (CheckOrientation)
				{
					Vector3d v7 = MathUtil.Normal(v4, v5, v6);
					if (vector3d.Dot(v7) < 0.99)
					{
						continue;
					}
				}
				if (Mesh.RemoveTriangle(array[j]) == MeshResult.Ok)
				{
					Removed++;
				}
			}
		}
		return true;
	}

	private bool is_same_triangle(ref Vector3d a, ref Vector3d b, ref Vector3d c, ref Vector3d x, ref Vector3d y, ref Vector3d z, double tolSqr)
	{
		if (a.DistanceSquared(x) < tolSqr)
		{
			if (b.DistanceSquared(y) < tolSqr && c.DistanceSquared(z) < tolSqr)
			{
				return true;
			}
			if (b.DistanceSquared(z) < tolSqr && c.DistanceSquared(y) < tolSqr)
			{
				return true;
			}
		}
		else if (a.DistanceSquared(y) < tolSqr)
		{
			if (b.DistanceSquared(x) < tolSqr && c.DistanceSquared(z) < tolSqr)
			{
				return true;
			}
			if (b.DistanceSquared(z) < tolSqr && c.DistanceSquared(x) < tolSqr)
			{
				return true;
			}
		}
		else if (a.DistanceSquared(z) < tolSqr)
		{
			if (b.DistanceSquared(x) < tolSqr && c.DistanceSquared(y) < tolSqr)
			{
				return true;
			}
			if (b.DistanceSquared(y) < tolSqr && c.DistanceSquared(x) < tolSqr)
			{
				return true;
			}
		}
		return false;
	}
}
