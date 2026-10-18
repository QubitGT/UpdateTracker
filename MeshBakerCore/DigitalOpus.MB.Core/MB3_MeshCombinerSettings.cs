using UnityEngine;

namespace DigitalOpus.MB.Core;

[CreateAssetMenu(fileName = "MeshBakerSettings", menuName = "Mesh Baker/Mesh Baker Settings")]
public class MB3_MeshCombinerSettings : ScriptableObject, MB_IMeshBakerSettingsHolder
{
	public MB3_MeshCombinerSettingsData data;

	public MB_IMeshBakerSettings GetMeshBakerSettings()
	{
		return data;
	}

	public void GetMeshBakerSettingsAsSerializedProperty(out string propertyName, out Object targetObj)
	{
		targetObj = this;
		propertyName = "data";
	}
}
