using UnityEngine;

namespace Technie.PhysicsCreator;

public class UnpackedMesh
{
	private MeshRenderer rigidRenderer;

	private SkinnedMeshRenderer skinnedRenderer;

	private Mesh srcMesh;

	private Vector3[] vertices;

	private Vector3[] normals;

	private BoneWeight[] weights;

	private int[] indices;

	private Vector3[] modelSpaceVertices;

	public SkinnedMeshRenderer SkinnedRenderer => skinnedRenderer;

	public Mesh Mesh => srcMesh;

	public Transform ModelSpaceTransform
	{
		get
		{
			if (skinnedRenderer != null)
			{
				return skinnedRenderer.rootBone.parent;
			}
			return rigidRenderer.transform;
		}
	}

	public Vector3[] RawVertices => vertices;

	public Vector3[] ModelSpaceVertices => modelSpaceVertices;

	public BoneWeight[] BoneWeights => weights;

	public int NumVertices => vertices.Length;

	public int[] Indices => indices;

	public static UnpackedMesh Create(Renderer renderer)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
		MeshRenderer meshRenderer = renderer as MeshRenderer;
		if (skinnedMeshRenderer != null)
		{
			return new UnpackedMesh(skinnedMeshRenderer);
		}
		if (meshRenderer != null)
		{
			return new UnpackedMesh(meshRenderer);
		}
		return null;
	}

	public UnpackedMesh(MeshRenderer rigidRenderer)
	{
		this.rigidRenderer = rigidRenderer;
		MeshFilter component = rigidRenderer.GetComponent<MeshFilter>();
		srcMesh = ((component != null) ? component.sharedMesh : null);
		if (srcMesh != null)
		{
			vertices = srcMesh.vertices;
			normals = srcMesh.normals;
			indices = srcMesh.triangles;
			weights = null;
			modelSpaceVertices = srcMesh.vertices;
		}
	}

	public UnpackedMesh(SkinnedMeshRenderer skinnedRenderer)
	{
		this.skinnedRenderer = skinnedRenderer;
		srcMesh = skinnedRenderer.sharedMesh;
		vertices = srcMesh.vertices;
		normals = srcMesh.normals;
		weights = srcMesh.boneWeights;
		indices = srcMesh.triangles;
		Transform[] bones = skinnedRenderer.bones;
		Transform parent = skinnedRenderer.rootBone.parent;
		Matrix4x4[] bindposes = srcMesh.bindposes;
		modelSpaceVertices = new Vector3[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			modelSpaceVertices[i] = ApplyBindPoseWeighted(vertices[i], weights[i], bindposes, bones, parent);
		}
	}

	private static Vector3 ApplyBindPoseWeighted(Vector3 inputVertex, BoneWeight weight, Matrix4x4[] bindPoses, Transform[] bones, Transform outputLocalSpace)
	{
		Vector3 position = bindPoses[weight.boneIndex0].MultiplyPoint(inputVertex);
		Vector3 position2 = bindPoses[weight.boneIndex1].MultiplyPoint(inputVertex);
		Vector3 position3 = bindPoses[weight.boneIndex2].MultiplyPoint(inputVertex);
		Vector3 position4 = bindPoses[weight.boneIndex3].MultiplyPoint(inputVertex);
		Vector3 vector = bones[weight.boneIndex0].TransformPoint(position);
		Vector3 vector2 = bones[weight.boneIndex1].TransformPoint(position2);
		Vector3 vector3 = bones[weight.boneIndex2].TransformPoint(position3);
		Vector3 vector4 = bones[weight.boneIndex3].TransformPoint(position4);
		Vector3 position5 = vector * weight.weight0 + vector2 * weight.weight1 + vector3 * weight.weight2 + vector4 * weight.weight3;
		return outputLocalSpace.InverseTransformPoint(position5);
	}
}
