namespace g3;

public interface IMeshBuilder
{
	bool SupportsMetaData { get; }

	int AppendNewMesh(bool bHaveVtxNormals, bool bHaveVtxColors, bool bHaveVtxUVs, bool bHaveFaceGroups);

	int AppendNewMesh(DMesh3 existingMesh);

	void SetActiveMesh(int id);

	int AppendVertex(double x, double y, double z);

	int AppendVertex(NewVertexInfo info);

	int AppendTriangle(int i, int j, int k);

	int AppendTriangle(int i, int j, int k, int g);

	void SetVertexUV(int vID, Vector2f Uvs);

	int BuildMaterial(GenericMaterial m);

	void AssignMaterial(int materialID, int meshID);

	void AppendMetaData(string identifier, object data);
}
