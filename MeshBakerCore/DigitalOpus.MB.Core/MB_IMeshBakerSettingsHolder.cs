using UnityEngine;

namespace DigitalOpus.MB.Core;

public interface MB_IMeshBakerSettingsHolder
{
	MB_IMeshBakerSettings GetMeshBakerSettings();

	void GetMeshBakerSettingsAsSerializedProperty(out string propertyName, out Object targetObj);
}
