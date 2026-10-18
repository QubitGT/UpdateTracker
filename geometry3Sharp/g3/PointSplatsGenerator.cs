using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class PointSplatsGenerator : MeshGenerator
{
	public IEnumerable<int> PointIndices;

	public int PointIndicesCount = -1;

	public Func<int, Vector3d> PointF;

	public Func<int, Vector3d> NormalF;

	public double Radius = 1.0;

	public PointSplatsGenerator()
	{
		WantUVs = false;
	}

	public override MeshGenerator Generate()
	{
		int num = ((PointIndicesCount == -1) ? PointIndices.Count() : PointIndicesCount);
		vertices = new VectorArray3d(num * 3);
		uv = null;
		normals = new VectorArray3f(vertices.Count);
		triangles = new IndexArray3i(num);
		Matrix2f matrix2f = new Matrix2f(MathF.PI * 2f / 3f);
		Vector2f vector2f = new Vector2f(0.0, Radius);
		Vector2f vector2f2 = matrix2f * vector2f;
		Vector2f v = matrix2f * vector2f2;
		int num2 = 0;
		int num3 = 0;
		foreach (int pointIndex in PointIndices)
		{
			Vector3d origin = PointF(pointIndex);
			Vector3d setZ = NormalF(pointIndex);
			Frame3f frame3f = new Frame3f(origin, setZ);
			triangles.Set(num3++, num2, num2 + 1, num2 + 2, Clockwise);
			vertices[num2++] = frame3f.FromPlaneUV(vector2f, 2);
			vertices[num2++] = frame3f.FromPlaneUV(vector2f2, 2);
			vertices[num2++] = frame3f.FromPlaneUV(v, 2);
		}
		return this;
	}

	public static DMesh3 Generate(IList<int> indices, Func<int, Vector3d> PointF, Func<int, Vector3d> NormalF, double radius)
	{
		return new PointSplatsGenerator
		{
			PointIndices = indices,
			PointIndicesCount = indices.Count,
			PointF = PointF,
			NormalF = NormalF,
			Radius = radius
		}.Generate().MakeDMesh();
	}
}
