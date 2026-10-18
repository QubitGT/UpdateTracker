namespace g3;

public class SimpleHoleFiller
{
	public DMesh3 Mesh;

	public EdgeLoop Loop;

	public int NewVertex;

	public int[] NewTriangles;

	public SimpleHoleFiller(DMesh3 mesh, EdgeLoop loop)
	{
		Mesh = mesh;
		Loop = loop;
		NewVertex = -1;
		NewTriangles = null;
	}

	public virtual ValidationStatus Validate()
	{
		return MeshValidation.IsBoundaryLoop(Mesh, Loop);
	}

	public virtual bool Fill(int group_id = -1)
	{
		if (Loop.Vertices.Length < 3)
		{
			return false;
		}
		if (Loop.Vertices.Length == 3)
		{
			Index3i tv = new Index3i(Loop.Vertices[0], Loop.Vertices[2], Loop.Vertices[1]);
			int num = Mesh.AppendTriangle(tv, group_id);
			if (num < 0)
			{
				return false;
			}
			NewTriangles = new int[1] { num };
			NewVertex = -1;
			return true;
		}
		Vector3d zero = Vector3d.Zero;
		for (int i = 0; i < Loop.Vertices.Length; i++)
		{
			zero += Mesh.GetVertex(Loop.Vertices[i]);
		}
		zero *= 1.0 / (double)Loop.Vertices.Length;
		NewVertex = Mesh.AppendVertex(zero);
		MeshEditor meshEditor = new MeshEditor(Mesh);
		try
		{
			NewTriangles = meshEditor.AddTriangleFan_OrderedVertexLoop(NewVertex, Loop.Vertices, group_id);
		}
		catch
		{
			NewTriangles = null;
		}
		if (NewTriangles == null)
		{
			Mesh.RemoveVertex(NewVertex);
			NewVertex = -1;
			return false;
		}
		return true;
	}
}
