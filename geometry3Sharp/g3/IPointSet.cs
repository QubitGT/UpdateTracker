using System.Collections.Generic;

namespace g3;

public interface IPointSet
{
	int VertexCount { get; }

	int MaxVertexID { get; }

	bool HasVertexNormals { get; }

	bool HasVertexColors { get; }

	int Timestamp { get; }

	Vector3d GetVertex(int i);

	Vector3f GetVertexNormal(int i);

	Vector3f GetVertexColor(int i);

	bool IsVertex(int vID);

	IEnumerable<int> VertexIndices();
}
