using System.Collections.Generic;

namespace g3;

public struct WriteMesh
{
	public IMesh Mesh;

	public string Name;

	public List<GenericMaterial> Materials;

	public IIndexMap TriToMaterialMap;

	public DenseUVMesh UVs;

	public WriteMesh(IMesh mesh, string name = "")
	{
		Mesh = mesh;
		Name = name;
		UVs = null;
		Materials = null;
		TriToMaterialMap = null;
	}
}
