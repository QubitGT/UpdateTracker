using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering;

[ExecuteAlways]
[AddComponentMenu("")]
public class ProbeVolumePerSceneData : MonoBehaviour
{
	[Serializable]
	internal struct ObsoletePerScenarioData
	{
		public int sceneHash;

		public TextAsset cellDataAsset;

		public TextAsset cellOptionalDataAsset;
	}

	[Serializable]
	private struct ObsoleteSerializablePerScenarioDataItem
	{
		public string scenario;

		public ObsoletePerScenarioData data;
	}

	[SerializeField]
	[FormerlySerializedAs("bakingSet")]
	internal ProbeVolumeBakingSet serializedBakingSet;

	[SerializeField]
	internal string sceneGUID = "";

	[FormerlySerializedAs("asset")]
	[SerializeField]
	internal ObsoleteProbeVolumeAsset obsoleteAsset;

	[FormerlySerializedAs("cellSharedDataAsset")]
	[SerializeField]
	internal TextAsset obsoleteCellSharedDataAsset;

	[FormerlySerializedAs("cellSupportDataAsset")]
	[SerializeField]
	internal TextAsset obsoleteCellSupportDataAsset;

	[FormerlySerializedAs("serializedScenarios")]
	[SerializeField]
	private List<ObsoleteSerializablePerScenarioDataItem> obsoleteSerializedScenarios = new List<ObsoleteSerializablePerScenarioDataItem>();

	public ProbeVolumeBakingSet bakingSet => serializedBakingSet;

	internal void Clear()
	{
		QueueSceneRemoval();
		serializedBakingSet = null;
	}

	internal void QueueSceneLoading()
	{
		if (!(serializedBakingSet == null))
		{
			ProbeReferenceVolume.instance.AddPendingSceneLoading(sceneGUID, serializedBakingSet);
		}
	}

	internal void QueueSceneRemoval()
	{
		if (serializedBakingSet != null)
		{
			ProbeReferenceVolume.instance.AddPendingSceneRemoval(sceneGUID);
		}
	}

	private void OnEnable()
	{
		ProbeReferenceVolume.instance.RegisterPerSceneData(this);
	}

	private void OnDisable()
	{
		QueueSceneRemoval();
		ProbeReferenceVolume.instance.UnregisterPerSceneData(this);
	}

	private void OnValidate()
	{
	}

	internal void Initialize()
	{
		ProbeReferenceVolume.instance.RegisterBakingSet(this);
		QueueSceneRemoval();
		QueueSceneLoading();
	}

	internal bool ResolveCellData()
	{
		if (serializedBakingSet != null)
		{
			return serializedBakingSet.ResolveCellData(serializedBakingSet.GetSceneCellIndexList(sceneGUID));
		}
		return false;
	}
}
