using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

[Serializable]
public class SerializableSourceBlendShape2Combined
{
	public GameObject[] srcGameObject;

	public int[] srcBlendShapeIdx;

	public GameObject[] combinedMeshTargetGameObject;

	public int[] blendShapeIdx;

	public void SetBuffers(GameObject[] srcGameObjs, int[] srcBlendShapeIdxs, GameObject[] targGameObjs, int[] targBlendShapeIdx)
	{
		srcGameObject = srcGameObjs;
		srcBlendShapeIdx = srcBlendShapeIdxs;
		combinedMeshTargetGameObject = targGameObjs;
		blendShapeIdx = targBlendShapeIdx;
	}

	public void DebugPrint()
	{
		if (srcGameObject == null)
		{
			Debug.LogError("Empty");
			return;
		}
		for (int i = 0; i < srcGameObject.Length; i++)
		{
			Debug.LogFormat("{0} {1} {2} {3}", srcGameObject[i], srcBlendShapeIdx[i], combinedMeshTargetGameObject[i], blendShapeIdx[i]);
		}
	}

	public Dictionary<MB3_MeshCombiner.MBBlendShapeKey, MB3_MeshCombiner.MBBlendShapeValue> GenerateMapFromSerializedData()
	{
		if (srcGameObject == null || srcBlendShapeIdx == null || combinedMeshTargetGameObject == null || blendShapeIdx == null || srcGameObject.Length != srcBlendShapeIdx.Length || srcGameObject.Length != combinedMeshTargetGameObject.Length || srcGameObject.Length != blendShapeIdx.Length)
		{
			Debug.LogError("Error GenerateMapFromSerializedData. Serialized data was malformed or missing.");
			return null;
		}
		Dictionary<MB3_MeshCombiner.MBBlendShapeKey, MB3_MeshCombiner.MBBlendShapeValue> dictionary = new Dictionary<MB3_MeshCombiner.MBBlendShapeKey, MB3_MeshCombiner.MBBlendShapeValue>();
		for (int i = 0; i < srcGameObject.Length; i++)
		{
			GameObject gameObject = srcGameObject[i];
			GameObject gameObject2 = combinedMeshTargetGameObject[i];
			if (gameObject == null || gameObject2 == null)
			{
				Debug.LogError("Error GenerateMapFromSerializedData. There were null references in the serialized data to source or target game objects. This can happen if the SerializableSourceBlendShape2Combined was serialized in a prefab but the source and target SkinnedMeshRenderer GameObjects  were not.");
				return null;
			}
			dictionary.Add(new MB3_MeshCombiner.MBBlendShapeKey(gameObject, srcBlendShapeIdx[i]), new MB3_MeshCombiner.MBBlendShapeValue
			{
				combinedMeshGameObject = gameObject2,
				blendShapeIndex = blendShapeIdx[i]
			});
		}
		return dictionary;
	}
}
