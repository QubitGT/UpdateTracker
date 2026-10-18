using UnityEngine;

namespace Technie.PhysicsCreator.Skinned;

[ExecuteInEditMode]
[DisallowMultipleComponent]
public class SkinnedColliderCreator : MonoBehaviour, ICreatorComponent
{
	public SkinnedMeshRenderer targetSkinnedRenderer;

	public SkinnedColliderEditorData editorData;

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
		targetSkinnedRenderer = base.gameObject.GetComponent<SkinnedMeshRenderer>();
	}

	public GameObject GetGameObject()
	{
		return base.gameObject;
	}

	public bool HasEditorData()
	{
		return editorData != null;
	}

	public IEditorData GetEditorData()
	{
		return editorData;
	}

	public Transform FindBone(BoneData boneData)
	{
		if (boneData == null)
		{
			return null;
		}
		return FindBone(targetSkinnedRenderer, boneData.targetBoneName);
	}

	public Transform FindBone(BoneHullData hullData)
	{
		if (hullData == null)
		{
			return null;
		}
		return FindBone(targetSkinnedRenderer, hullData.targetBoneName);
	}

	public static Transform FindBone(SkinnedMeshRenderer skinnedRenderer, string nameToFind)
	{
		if (skinnedRenderer == null)
		{
			return null;
		}
		if (nameToFind == null)
		{
			return null;
		}
		Transform[] bones = skinnedRenderer.bones;
		foreach (Transform transform in bones)
		{
			if (transform.name == nameToFind)
			{
				return transform;
			}
		}
		return null;
	}
}
