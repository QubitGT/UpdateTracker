using System;

namespace g3;

public class TriangulatedPolygonGenerator : MeshGenerator
{
	public GeneralPolygon2d Polygon;

	public Vector3f FixedNormal = Vector3f.AxisZ;

	public TrivialRectGenerator.UVModes UVMode;

	public int Subdivisions = 1;

	public override MeshGenerator Generate()
	{
		MeshInsertPolygon insertion;
		DMesh3 dMesh = new DMesh3(ComputeResult(out insertion), bCompact: true);
		int vertexCount = dMesh.VertexCount;
		vertices = new VectorArray3d(vertexCount);
		uv = new VectorArray2f(vertexCount);
		normals = new VectorArray3f(vertexCount);
		for (int i = 0; i < vertexCount; i++)
		{
			vertices[i] = dMesh.GetVertex(i);
			uv[i] = dMesh.GetVertexUV(i);
			normals[i] = FixedNormal;
		}
		int triangleCount = dMesh.TriangleCount;
		triangles = new IndexArray3i(triangleCount);
		for (int j = 0; j < triangleCount; j++)
		{
			triangles[j] = dMesh.GetTriangle(j);
		}
		return this;
	}

	public DMesh3 ComputeResult(out MeshInsertPolygon insertion)
	{
		AxisAlignedBox2d bounds = Polygon.Bounds;
		double fRadius = 0.1 * bounds.DiagonalLength;
		bounds.Expand(fRadius);
		TrivialRectGenerator obj = ((Subdivisions == 1) ? new TrivialRectGenerator() : new GriddedRectGenerator
		{
			EdgeVertices = Subdivisions
		});
		obj.Width = (float)bounds.Width;
		obj.Height = (float)bounds.Height;
		obj.IndicesMap = new Index2i(1, 2);
		obj.UVMode = UVMode;
		obj.Clockwise = true;
		obj.Generate();
		DMesh3 dMesh = new DMesh3();
		obj.MakeMesh(dMesh);
		GeneralPolygon2d generalPolygon2d = new GeneralPolygon2d(Polygon);
		Vector2d center = bounds.Center;
		generalPolygon2d.Translate(-center);
		MeshInsertPolygon meshInsertPolygon = new MeshInsertPolygon
		{
			Mesh = dMesh,
			Polygon = generalPolygon2d
		};
		if (!meshInsertPolygon.Insert())
		{
			throw new Exception("TriangulatedPolygonGenerator: failed to Insert()");
		}
		MeshFaceSelection selected = meshInsertPolygon.InteriorTriangles;
		new MeshEditor(dMesh).RemoveTriangles((int tid) => !selected.IsSelected(tid), bRemoveIsolatedVerts: true);
		Vector3d v = new Vector3d(center.x, center.y, 0.0);
		MeshTransforms.Translate(dMesh, v);
		insertion = meshInsertPolygon;
		return dMesh;
	}
}
