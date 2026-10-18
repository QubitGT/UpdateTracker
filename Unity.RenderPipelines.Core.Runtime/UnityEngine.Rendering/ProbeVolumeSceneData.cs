using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering;

[Serializable]
[Obsolete("This class is no longer necessary for APV implementation.")]
public class ProbeVolumeSceneData
{
	internal Object parentAsset;

	[SerializeField]
	[FormerlySerializedAs("sceneBounds")]
	[Obsolete("This data is now serialized directly in the baking set asset")]
	internal SerializedDictionary<string, Bounds> obsoleteSceneBounds;

	[SerializeField]
	[FormerlySerializedAs("hasProbeVolumes")]
	[Obsolete("This data is now serialized directly in the baking set asset")]
	internal SerializedDictionary<string, bool> obsoleteHasProbeVolumes;

	public ProbeVolumeSceneData(Object parentAsset)
	{
		SetParentObject(parentAsset);
	}

	[Obsolete]
	public void SetParentObject(Object parent)
	{
		parentAsset = parent;
	}
}
