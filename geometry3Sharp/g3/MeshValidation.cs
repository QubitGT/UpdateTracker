namespace g3;

public static class MeshValidation
{
	public static ValidationStatus IsEdgeLoop(DMesh3 mesh, EdgeLoop loop)
	{
		int num = loop.Vertices.Length;
		for (int i = 0; i < num; i++)
		{
			if (!mesh.IsVertex(loop.Vertices[i]))
			{
				return ValidationStatus.NotAVertex;
			}
		}
		for (int j = 0; j < num; j++)
		{
			int vA = loop.Vertices[j];
			int vB = loop.Vertices[(j + 1) % num];
			if (mesh.FindEdge(vA, vB) == -1)
			{
				return ValidationStatus.VerticesNotConnectedByEdge;
			}
		}
		return ValidationStatus.Ok;
	}

	public static ValidationStatus IsBoundaryLoop(DMesh3 mesh, EdgeLoop loop)
	{
		int num = loop.Vertices.Length;
		for (int i = 0; i < num; i++)
		{
			if (!mesh.IsBoundaryVertex(loop.Vertices[i]))
			{
				return ValidationStatus.NotBoundaryVertex;
			}
		}
		for (int j = 0; j < num; j++)
		{
			int num2 = loop.Vertices[j];
			int num3 = loop.Vertices[(j + 1) % num];
			int num4 = mesh.FindEdge(num2, num3);
			if (num4 == -1)
			{
				return ValidationStatus.VerticesNotConnectedByEdge;
			}
			if (!mesh.IsBoundaryEdge(num4))
			{
				return ValidationStatus.NotBoundaryEdge;
			}
			Index2i orientedBoundaryEdgeV = mesh.GetOrientedBoundaryEdgeV(num4);
			if (orientedBoundaryEdgeV.a != num2 || orientedBoundaryEdgeV.b != num3)
			{
				return ValidationStatus.IncorrectLoopOrientation;
			}
		}
		return ValidationStatus.Ok;
	}

	public static ValidationStatus HasDuplicateTriangles(DMesh3 mesh)
	{
		foreach (int item in mesh.TriangleIndices())
		{
			Index3i triNeighbourTris = mesh.GetTriNeighbourTris(item);
			if (triNeighbourTris.a == triNeighbourTris.b && triNeighbourTris.b == triNeighbourTris.c && triNeighbourTris.a != -1)
			{
				return ValidationStatus.DuplicateTriangles;
			}
		}
		return ValidationStatus.Ok;
	}
}
