using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator.Skinned;

public class SkinnedColliderEditorData : ScriptableObject, IEditorData
{
	public const int INVALID_INDEX = -1;

	public SkinnedColliderRuntimeData runtimeData;

	public float defaultMass = 1f;

	public float defaultLinearDrag;

	public float defaultAngularDrag = 0.05f;

	public float defaultLinearDamping;

	public float defaultAngularDamping;

	public PhysicsMaterial defaultMaterial;

	public ColliderType defaultColliderType;

	public List<BoneData> boneData = new List<BoneData>();

	public List<BoneHullData> boneHullData = new List<BoneHullData>();

	private int selectedBoneIndex = -1;

	private int selectedHullIndex = -1;

	private int lastModifiedFrame;

	public Mesh sourceMesh;

	public Hash160 sourceMeshHash;

	public bool suppressMeshModificationWarning;

	public Hash160 CachedHash
	{
		get
		{
			return sourceMeshHash;
		}
		set
		{
			sourceMeshHash = value;
		}
	}

	public bool HasCachedData
	{
		get
		{
			if (sourceMeshHash != null)
			{
				return sourceMeshHash.IsValid();
			}
			return false;
		}
	}

	public Mesh SourceMesh => sourceMesh;

	public IHull[] Hulls => boneHullData.ToArray();

	public bool HasSuppressMeshModificationWarning => suppressMeshModificationWarning;

	public void SetSelection(BoneData bone)
	{
		for (int i = 0; i < boneData.Count; i++)
		{
			if (boneData[i] == bone)
			{
				selectedBoneIndex = i;
				selectedHullIndex = -1;
				break;
			}
		}
		MarkDirty();
	}

	public void SetSelection(BoneHullData hull)
	{
		for (int i = 0; i < boneHullData.Count; i++)
		{
			if (boneHullData[i] == hull)
			{
				selectedHullIndex = i;
				selectedBoneIndex = -1;
				break;
			}
		}
		MarkDirty();
	}

	public void ClearSelection()
	{
		selectedBoneIndex = -1;
		selectedHullIndex = -1;
		MarkDirty();
	}

	public BoneData GetSelectedBone()
	{
		if (selectedBoneIndex >= 0 && selectedBoneIndex < boneData.Count)
		{
			return boneData[selectedBoneIndex];
		}
		return null;
	}

	public BoneHullData GetSelectedHull()
	{
		if (selectedHullIndex >= 0 && selectedHullIndex < boneHullData.Count)
		{
			return boneHullData[selectedHullIndex];
		}
		return null;
	}

	public BoneData GetBoneData(Transform bone)
	{
		if (bone == null)
		{
			return null;
		}
		return GetBoneData(bone.name);
	}

	public BoneData GetBoneData(string boneName)
	{
		foreach (BoneData boneDatum in boneData)
		{
			if (boneDatum.targetBoneName == boneName)
			{
				return boneDatum;
			}
		}
		return null;
	}

	public BoneHullData[] GetBoneHullData(Transform bone)
	{
		if (bone == null)
		{
			return new BoneHullData[0];
		}
		return GetBoneHullData(bone.name);
	}

	public BoneHullData[] GetBoneHullData(string boneName)
	{
		List<BoneHullData> list = new List<BoneHullData>();
		foreach (BoneHullData boneHullDatum in boneHullData)
		{
			if (boneHullDatum.targetBoneName == boneName)
			{
				list.Add(boneHullDatum);
			}
		}
		return list.ToArray();
	}

	public void SetAssetDirty()
	{
		MarkDirty();
	}

	public void MarkDirty()
	{
	}

	public int GetLastModifiedFrame()
	{
		return lastModifiedFrame;
	}

	public void Add(BoneData data)
	{
		boneData.Add(data);
		MarkDirty();
	}

	public void Remove(BoneData data)
	{
		boneData.Remove(data);
		MarkDirty();
	}

	public void Add(BoneHullData data)
	{
		boneHullData.Add(data);
		MarkDirty();
	}

	public void Remove(BoneHullData data)
	{
		boneHullData.Remove(data);
		MarkDirty();
	}
}
