using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder.Stl;

internal static class pb_Stl_Exporter
{
	public static bool Export(string path, GameObject[] gameObjects, FileType type)
	{
		Mesh[] array = CreateWorldSpaceMeshesWithTransforms(gameObjects.Select((GameObject x) => x.transform).ToArray());
		bool result = false;
		if (array != null && array.Length != 0 && !string.IsNullOrEmpty(path))
		{
			result = pb_Stl.WriteFile(path, array, type);
		}
		int num = 0;
		while (array != null && num < array.Length)
		{
			Object.DestroyImmediate(array[num]);
			num++;
		}
		return result;
	}

	private static Mesh[] CreateWorldSpaceMeshesWithTransforms(IList<Transform> transforms)
	{
		if (transforms == null || transforms.Count < 1)
		{
			return null;
		}
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < transforms.Count; i++)
		{
			zero += transforms[i].position;
		}
		Vector3 position = zero / transforms.Count;
		GameObject gameObject = new GameObject();
		gameObject.name = "ROOT";
		gameObject.transform.position = position;
		foreach (Transform transform2 in transforms)
		{
			GameObject gameObject2 = Object.Instantiate(transform2.gameObject);
			gameObject2.transform.SetParent(transform2.parent, worldPositionStays: false);
			gameObject2.transform.SetParent(gameObject.transform, worldPositionStays: true);
		}
		gameObject.transform.position = Vector3.zero;
		List<MeshFilter> list = (from x in gameObject.GetComponentsInChildren<MeshFilter>()
			where x.sharedMesh != null
			select x).ToList();
		int count = list.Count;
		Mesh[] array = new Mesh[count];
		for (int j = 0; j < count; j++)
		{
			Transform transform = list[j].transform;
			Vector3[] vertices = list[j].sharedMesh.vertices;
			Vector3[] normals = list[j].sharedMesh.normals;
			for (int k = 0; k < vertices.Length; k++)
			{
				vertices[k] = transform.TransformPoint(vertices[k]);
				normals[k] = transform.TransformDirection(normals[k]);
			}
			Mesh mesh = new Mesh();
			mesh.name = list[j].name;
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.triangles = list[j].sharedMesh.triangles;
			array[j] = mesh;
		}
		Object.DestroyImmediate(gameObject);
		return array;
	}
}
