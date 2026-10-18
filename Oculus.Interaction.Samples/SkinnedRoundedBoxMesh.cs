using System;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedRoundedBoxMesh : MonoBehaviour
{
	[SerializeField]
	private Transform _topLeft;

	[SerializeField]
	private Transform _topRight;

	[SerializeField]
	private Transform _bottomLeft;

	[SerializeField]
	private Transform _bottomRight;

	[SerializeField]
	private int _cornerSegmentCount;

	[SerializeField]
	private int _cylinderFaceCount;

	[SerializeField]
	private float _borderRadius;

	[SerializeField]
	private float _cylinderRadius;

	[SerializeField]
	private SkinnedMeshRenderer _skinnedMeshRenderer;

	public bool generateOnStart;

	private void Start()
	{
		if (generateOnStart)
		{
			GenerateMesh(_cornerSegmentCount, _borderRadius, _cylinderFaceCount, _cylinderRadius, _skinnedMeshRenderer, _topLeft, _topRight, _bottomLeft, _bottomRight);
		}
	}

	public static Vector2[] GenerateArcPath(float startAngle, float endAngle, int steps, float radius, bool closed)
	{
		float num = (endAngle - startAngle) / (float)steps;
		List<Vector2> list = new List<Vector2>();
		int num2 = (closed ? (steps + 1) : steps);
		for (int i = 0; i < num2; i++)
		{
			Vector2 vector = new Vector2(Mathf.Cos(startAngle + num * (float)i), Mathf.Sin(startAngle + num * (float)i));
			list.Add(vector * radius);
		}
		return list.ToArray();
	}

	public static Vector3[] GenerateCylinderAroundPath(List<Vector2> path, int cylinderFaceCount, float cylinderRadius)
	{
		Vector2[] array = GenerateArcPath(0f, MathF.PI * 2f, cylinderFaceCount, cylinderRadius, closed: false);
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i < path.Count; i++)
		{
			Vector2 vector = path[i];
			Vector3 vector2 = Vector3.Normalize(Vector3.Cross(Vector3.Normalize(path[(i + 1) % path.Count] - vector), new Vector3(0f, 0f, 1f)));
			for (int j = 0; j < array.Length; j++)
			{
				Vector3 item = (Vector3)vector + array[j].x * vector2 + array[j].y * Vector3.forward;
				list.Add(item);
			}
		}
		return list.ToArray();
	}

	public static int[] GenerateCylinderIndices(int cornerSegmentCount, int cylinderFaceCount)
	{
		List<int> list = new List<int>();
		int num = cornerSegmentCount * 4;
		for (int i = 0; i < num; i++)
		{
			int num2 = cylinderFaceCount * i;
			int num3 = cylinderFaceCount * ((i + 1) % num);
			for (int j = 0; j < cylinderFaceCount; j++)
			{
				int num4 = j;
				int num5 = (j + 1) % cylinderFaceCount;
				list.Add(num2 + num4);
				list.Add(num3 + num4);
				list.Add(num3 + num5);
				list.Add(num2 + num4);
				list.Add(num3 + num5);
				list.Add(num2 + num5);
			}
		}
		return list.ToArray();
	}

	private static void PushBoneWeigth(int boneIndex, List<BoneWeight> weights, int cornerSegmentCount, int cylinderFaceCount)
	{
		int num = cornerSegmentCount * cylinderFaceCount;
		for (int i = 0; i < num; i++)
		{
			BoneWeight item = default(BoneWeight);
			item.boneIndex0 = boneIndex;
			item.weight0 = 1f;
			item.boneIndex1 = 0;
			item.boneIndex2 = 0;
			item.boneIndex3 = 0;
			weights.Add(item);
		}
	}

	public static void GenerateMesh(int cornerSegmentCount, float borderRadius, int cylinderFaceCount, float cylinderRadius, SkinnedMeshRenderer skinnedMeshRenderer, Transform topLeft, Transform topRight, Transform bottomLeft, Transform bottomRight)
	{
		Mesh mesh = new Mesh();
		List<Vector2> list = new List<Vector2>();
		list.AddRange(GenerateArcPath(MathF.PI, MathF.PI / 2f, cornerSegmentCount, borderRadius, closed: false));
		list.AddRange(GenerateArcPath(MathF.PI / 2f, 0f, cornerSegmentCount, borderRadius, closed: false));
		list.AddRange(GenerateArcPath(0f, -MathF.PI / 2f, cornerSegmentCount, borderRadius, closed: false));
		list.AddRange(GenerateArcPath(-MathF.PI / 2f, -MathF.PI, cornerSegmentCount, borderRadius, closed: false));
		Vector3[] vertices = GenerateCylinderAroundPath(list, cylinderFaceCount, cylinderRadius);
		int[] indices = GenerateCylinderIndices(cornerSegmentCount, cylinderFaceCount);
		List<BoneWeight> list2 = new List<BoneWeight>();
		PushBoneWeigth(0, list2, cornerSegmentCount, cylinderFaceCount);
		PushBoneWeigth(1, list2, cornerSegmentCount, cylinderFaceCount);
		PushBoneWeigth(3, list2, cornerSegmentCount, cylinderFaceCount);
		PushBoneWeigth(2, list2, cornerSegmentCount, cylinderFaceCount);
		mesh.vertices = vertices;
		mesh.SetIndices(indices, MeshTopology.Triangles, 0);
		mesh.boneWeights = list2.ToArray();
		mesh.bindposes = new Matrix4x4[4]
		{
			Matrix4x4.identity,
			Matrix4x4.identity,
			Matrix4x4.identity,
			Matrix4x4.identity
		};
		mesh.name = "SkinnedRoundedBoxMesh";
		skinnedMeshRenderer.bones = new Transform[4] { topLeft, topRight, bottomLeft, bottomRight };
		skinnedMeshRenderer.rootBone = topLeft;
		skinnedMeshRenderer.sharedMesh = mesh;
		mesh.RecalculateBounds();
	}

	[ContextMenu("Generate Mesh")]
	public void GenerateMeshFromMenu()
	{
		GenerateMesh(_cornerSegmentCount, _borderRadius, _cylinderFaceCount, _cylinderRadius, _skinnedMeshRenderer, _topLeft, _topRight, _bottomLeft, _bottomRight);
	}
}
