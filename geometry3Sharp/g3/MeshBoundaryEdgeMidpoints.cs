using System.Collections.Generic;

namespace g3;

public class MeshBoundaryEdgeMidpoints : IPointSet
{
	private int num_boundary_edges;

	public DMesh3 Mesh;

	public int VertexCount => num_boundary_edges;

	public int MaxVertexID => Mesh.MaxEdgeID;

	public bool HasVertexNormals => false;

	public bool HasVertexColors => false;

	public int Timestamp => Mesh.Timestamp;

	public MeshBoundaryEdgeMidpoints(DMesh3 mesh)
	{
		Mesh = mesh;
		num_boundary_edges = 0;
		foreach (int item in mesh.BoundaryEdgeIndices())
		{
			_ = item;
			num_boundary_edges++;
		}
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
		if (Mesh.IsEdge(vID))
		{
			return Mesh.IsBoundaryEdge(vID);
		}
		return false;
	}

	public IEnumerable<int> VertexIndices()
	{
		return Mesh.BoundaryEdgeIndices();
	}
}
