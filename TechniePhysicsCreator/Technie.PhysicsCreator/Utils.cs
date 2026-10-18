using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator;

public static class Utils
{
	public static Matrix4x4 CreateSkewableTRS(Transform target)
	{
		if (target.parent == null)
		{
			return Matrix4x4.TRS(target.localPosition, target.localRotation, target.localScale);
		}
		Matrix4x4 matrix4x = CreateSkewableTRS(target.parent);
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(target.localPosition, target.localRotation, target.localScale);
		return matrix4x * matrix4x2;
	}

	public static void Inflate(Vector3 point, ref Vector3 min, ref Vector3 max)
	{
		min.x = Mathf.Min(min.x, point.x);
		min.y = Mathf.Min(min.y, point.y);
		min.z = Mathf.Min(min.z, point.z);
		max.x = Mathf.Max(max.x, point.x);
		max.y = Mathf.Max(max.y, point.y);
		max.z = Mathf.Max(max.z, point.z);
	}

	public static Plane[] ConvertToPlanes(Mesh convexMesh, bool show)
	{
		List<Plane> list = new List<Plane>();
		Vector3[] vertices = convexMesh.vertices;
		int[] triangles = convexMesh.triangles;
		for (int i = 0; i < triangles.Length; i += 3)
		{
			Vector3 vector = vertices[triangles[i]];
			Vector3 vector2 = vertices[triangles[i + 1]];
			Vector3 vector3 = vertices[triangles[i + 2]];
			Vector3 normalized = (vector2 - vector).normalized;
			Vector3 normalized2 = (vector3 - vector).normalized;
			Vector3 normalized3 = Vector3.Cross(normalized, normalized2).normalized;
			if (!(normalized3.magnitude > 0.01f))
			{
				continue;
			}
			Plane plane = new Plane(normalized3, vector);
			if (!Contains(plane, list))
			{
				list.Add(plane);
				if (show)
				{
					GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
					gameObject.name = $"{i} : {triangles[i]} / {triangles[i + 1]} / {triangles[i + 2]}";
					gameObject.transform.SetPositionAndRotation(vector, Quaternion.LookRotation(normalized3));
				}
			}
		}
		return list.ToArray();
	}

	public static bool Contains(Plane toTest, List<Plane> planes)
	{
		foreach (Plane plane in planes)
		{
			if (Mathf.Abs(toTest.distance - plane.distance) < 0.01f && Vector3.Angle(toTest.normal, plane.normal) < 0.01f)
			{
				return true;
			}
		}
		return false;
	}

	public static Mesh Clip(Mesh boundingMesh, Mesh inputMesh)
	{
		if (boundingMesh == null || boundingMesh.triangles.Length == 0)
		{
			return null;
		}
		if (inputMesh == null || inputMesh.triangles.Length == 0)
		{
			return null;
		}
		CuttableMesh cuttableMesh = new CuttableMesh(inputMesh);
		MeshCutter meshCutter = new MeshCutter();
		Plane[] array = ConvertToPlanes(boundingMesh, show: false);
		foreach (Plane worldCutPlane in array)
		{
			meshCutter.Cut(cuttableMesh, worldCutPlane);
			Mesh inputMesh2 = meshCutter.GetBackOutput().CreateMesh();
			cuttableMesh = new CuttableMesh(QHullUtil.FindConvexHull("", inputMesh2, showErrorInLog: false));
		}
		Mesh mesh = cuttableMesh.CreateMesh();
		if (mesh.triangles.Length != 0)
		{
			return mesh;
		}
		return null;
	}

	public static float CalcTriangleArea(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		Vector3 lhs = p1 - p0;
		Vector3 rhs = p2 - p0;
		return 0.5f * Vector3.Cross(lhs, rhs).magnitude;
	}

	public static float TimeProgression(float elapsedTime, float maxTime)
	{
		float num = elapsedTime / maxTime;
		return 0f - (0f - num) / (num + 0.5f);
	}

	public static float AsymtopicProgression(float inputProgress, float maxProgression, float rate)
	{
		return 0f - maxProgression * (0f - inputProgress) / (inputProgress + rate);
	}

	public static int FindBoneIndex(SkinnedMeshRenderer skinnedRenderer, Transform bone)
	{
		Transform[] bones = skinnedRenderer.bones;
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] == bone)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool IsWeightAboveThreshold(BoneWeight weights, int ownBoneIndex, float minThreshold, float maxThreshold)
	{
		if (!IsWeightAboveThreshold(weights.boneIndex0, weights.weight0, ownBoneIndex, minThreshold, maxThreshold) && !IsWeightAboveThreshold(weights.boneIndex1, weights.weight1, ownBoneIndex, minThreshold, maxThreshold) && !IsWeightAboveThreshold(weights.boneIndex2, weights.weight2, ownBoneIndex, minThreshold, maxThreshold))
		{
			return IsWeightAboveThreshold(weights.boneIndex3, weights.weight3, ownBoneIndex, minThreshold, maxThreshold);
		}
		return true;
	}

	public static bool IsWeightAboveThreshold(int boneIndex, float boneWeight, int ourIndex, float minThreshold, float maxThreshold)
	{
		if (boneIndex == ourIndex && boneWeight >= minThreshold)
		{
			return boneWeight <= maxThreshold;
		}
		return false;
	}

	public static int NumVerticesForBone(UnpackedMesh mesh, Transform bone, float minThreshold, float maxThreshold)
	{
		int num = 0;
		int ownBoneIndex = FindBoneIndex(mesh.SkinnedRenderer, bone);
		for (int i = 0; i < mesh.NumVertices; i++)
		{
			if (IsWeightAboveThreshold(mesh.BoneWeights[i], ownBoneIndex, minThreshold, maxThreshold))
			{
				num++;
			}
		}
		return num;
	}

	public static void UpdateCachedVertices(IHull hull, Mesh srcMesh)
	{
		Vector3[] vertices = srcMesh.vertices;
		int[] triangles = srcMesh.triangles;
		List<Vector3> list = new List<Vector3>();
		int[] selectedFaces = hull.GetSelectedFaces();
		for (int i = 0; i < hull.NumSelectedTriangles; i++)
		{
			int num = selectedFaces[i];
			int num2 = num * 3;
			int num3 = num * 3 + 1;
			int num4 = num * 3 + 2;
			Vector3 item = vertices[triangles[num2]];
			Vector3 item2 = vertices[triangles[num3]];
			Vector3 item3 = vertices[triangles[num4]];
			list.Add(item);
			list.Add(item2);
			list.Add(item3);
		}
		hull.CachedTriangleVertices = list.ToArray();
	}
}
