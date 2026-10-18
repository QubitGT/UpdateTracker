using System.Collections.Generic;

namespace g3;

public class MeshEdgeMidpoints : IPointSet
{
	public DMesh3 Mesh;

	public int VertexCount => Mesh.EdgeCount;

	public int MaxVertexID => Mesh.MaxEdgeID;

	public bool HasVertexNormals => false;

	public bool HasVertexColors => false;

	public int Timestamp => Mesh.Timestamp;

	public MeshEdgeMidpoints(DMesh3 mesh)
	{
		Mesh = mesh;
	}

	public Vector3d GetVertex(int i)
	{
		return Mesh.GetEdgePoint(i, 0.5);
	}

	public Vector3f GetVertexNormal(int i)
	{
		return Vector3f.AxisY;
	}

	public Vector3f GetVertexColor(int i)
	{
		return Vector3f.One;
	}

	public bool IsVertex(int vID)
	{
		return Mesh.IsEdge(vID);
	}

	public IEnumerable<int> VertexIndices()
	{
		return Mesh.EdgeIndices();
	}
}
