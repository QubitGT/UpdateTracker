using System.Collections.Generic;

namespace g3;

public static class MeshIOUtil
{
	public static List<GenericMaterial> FindUniqueMaterialList(List<WriteMesh> meshes)
	{
		List<GenericMaterial> list = new List<GenericMaterial>();
		foreach (WriteMesh mesh in meshes)
		{
			if (mesh.Materials == null)
			{
				continue;
			}
			foreach (GenericMaterial material in mesh.Materials)
			{
				if (!list.Contains(material))
				{
					list.Add(material);
				}
			}
		}
		return list;
	}
}
