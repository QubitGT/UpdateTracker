using UnityEngine;

[AddComponentMenu("GorillaTag/Environment/Bake Blend Shape")]
public class BakeBlendShape : MonoBehaviour
{
	private void Update()
	{
		Mesh mesh = new Mesh();
		MeshCollider component = GetComponent<MeshCollider>();
		GetComponent<SkinnedMeshRenderer>().BakeMesh(mesh);
		component.sharedMesh = null;
		component.sharedMesh = mesh;
	}
}
