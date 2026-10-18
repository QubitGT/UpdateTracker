using System;

namespace g3;

public class MeshExtrudeLoop
{
	public DMesh3 Mesh;

	public EdgeLoop Loop;

	public Func<Vector3d, Vector3f, int, Vector3d> PositionF;

	public int[] NewTriangles;

	public EdgeLoop NewLoop;

	public MeshExtrudeLoop(DMesh3 mesh, EdgeLoop loop)
	{
		Mesh = mesh;
		Loop = loop;
		PositionF = (Vector3d pos, Vector3f normal, int idx) => pos + Vector3d.AxisY;
	}

	public virtual ValidationStatus Validate()
	{
		return MeshValidation.IsBoundaryLoop(Mesh, Loop);
	}

	public virtual bool Extrude(int group_id = -1)
	{
		int num = Loop.Vertices.Length;
		NewLoop = new EdgeLoop(Mesh);
		NewLoop.Vertices = new int[num];
		for (int i = 0; i < num; i++)
		{
			int fromVID = Loop.Vertices[i];
			NewLoop.Vertices[i] = Mesh.AppendVertex(Mesh, fromVID);
		}
		for (int j = 0; j < num; j++)
		{
			Vector3d vertex = Mesh.GetVertex(Loop.Vertices[j]);
			Vector3f vertexNormal = Mesh.GetVertexNormal(Loop.Vertices[j]);
			Vector3d vNewPos = PositionF(vertex, vertexNormal, Loop.Vertices[j]);
			Mesh.SetVertex(NewLoop.Vertices[j], vNewPos);
		}
		MeshEditor meshEditor = new MeshEditor(Mesh);
		NewTriangles = meshEditor.StitchLoop(Loop.Vertices, NewLoop.Vertices, group_id);
		return true;
	}
}
