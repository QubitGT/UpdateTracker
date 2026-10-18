using System;
using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator.Skinned;

[Serializable]
public class BoneHullData : IHull
{
	public string targetBoneName;

	public HullType type;

	public ColliderType colliderType;

	public Color previewColour;

	public Mesh hullMesh;

	public PhysicsMaterial material;

	public bool isTrigger;

	[SerializeField]
	private float minThreshold;

	[SerializeField]
	private float maxThreshold;

	[SerializeField]
	private List<int> selectedFaces = new List<int>();

	public List<Vector3> cachedTriangleVertices = new List<Vector3>();

	public string Name => targetBoneName;

	public float MinThreshold => minThreshold;

	public float MaxThreshold => maxThreshold;

	public int NumSelectedTriangles => selectedFaces.Count;

	public Vector3[] CachedTriangleVertices
	{
		get
		{
			return cachedTriangleVertices.ToArray();
		}
		set
		{
			cachedTriangleVertices.Clear();
			cachedTriangleVertices.AddRange(value);
		}
	}

	public bool IsTriangleSelected(int triIndex, Renderer renderer, Mesh targetMesh)
	{
		if (type == HullType.Manual)
		{
			return selectedFaces.Contains(triIndex);
		}
		if (type == HullType.Auto)
		{
			SkinnedMeshRenderer skinnedRenderer = renderer as SkinnedMeshRenderer;
			BoneWeight[] boneWeights = targetMesh.boneWeights;
			int[] triangles = targetMesh.triangles;
			int num = triangles[triIndex * 3];
			int num2 = triangles[triIndex * 3 + 1];
			int num3 = triangles[triIndex * 3 + 2];
			BoneWeight weights = boneWeights[num];
			BoneWeight weights2 = boneWeights[num2];
			BoneWeight weights3 = boneWeights[num3];
			Transform bone = SkinnedColliderCreator.FindBone(skinnedRenderer, targetBoneName);
			int ownBoneIndex = Utils.FindBoneIndex(skinnedRenderer, bone);
			if (Utils.IsWeightAboveThreshold(weights, ownBoneIndex, minThreshold, maxThreshold) && Utils.IsWeightAboveThreshold(weights2, ownBoneIndex, minThreshold, maxThreshold) && Utils.IsWeightAboveThreshold(weights3, ownBoneIndex, minThreshold, maxThreshold))
			{
				return true;
			}
		}
		return false;
	}

	public int[] GetSelectedFaces()
	{
		return selectedFaces.ToArray();
	}

	public void AddToSelection(int newTriangleIndex, Mesh srcMesh)
	{
		if (!selectedFaces.Contains(newTriangleIndex))
		{
			selectedFaces.Add(newTriangleIndex);
			Utils.UpdateCachedVertices(this, srcMesh);
		}
	}

	public void RemoveFromSelection(int existingTriangleIndex, Mesh srcMesh)
	{
		selectedFaces.Remove(existingTriangleIndex);
		Utils.UpdateCachedVertices(this, srcMesh);
	}

	public void SetMinThreshold(float newMinThreshold)
	{
		minThreshold = newMinThreshold;
	}

	public void SetMaxThreshold(float newMaxThreshold)
	{
		maxThreshold = newMaxThreshold;
	}

	public void SetThresholds(float newMinThreshold, float newMaxThreshold, SkinnedMeshRenderer renderer, Mesh targetMesh)
	{
		minThreshold = newMinThreshold;
		maxThreshold = newMaxThreshold;
	}

	public void ClearSelectedFaces()
	{
		if (type == HullType.Manual)
		{
			selectedFaces.Clear();
			cachedTriangleVertices.Clear();
		}
	}

	public void SetSelectedFaces(List<int> newSelectedFaceIndices, Mesh srcMesh)
	{
		if (type == HullType.Manual)
		{
			selectedFaces.Clear();
			selectedFaces.AddRange(newSelectedFaceIndices);
			Utils.UpdateCachedVertices(this, srcMesh);
		}
	}

	public Vector3[] GetCachedTriangleVertices()
	{
		return cachedTriangleVertices.ToArray();
	}
}
