using System;
using UnityEngine;

namespace Fusion.Photon.Realtime;

[Serializable]
[HelpURL("https://doc.photonengine.com/en-us/pun/v2/getting-started/initial-setup")]
[CreateAssetMenu(menuName = "Fusion/Photon Application Settings", fileName = "PhotonAppSettings")]
[FusionGlobalScriptableObject("Assets/Photon/Fusion/Resources/PhotonAppSettings.asset")]
public class PhotonAppSettings : FusionGlobalScriptableObject<PhotonAppSettings>
{
	[InlineHelp]
	public FusionAppSettings AppSettings;

	public static PhotonAppSettings Global => FusionGlobalScriptableObject<PhotonAppSettings>.GlobalInternal;

	public static bool IsGlobalLoaded => FusionGlobalScriptableObject<PhotonAppSettings>.IsGlobalLoadedInternal;

	public static bool TryGetGlobal(out PhotonAppSettings settings)
	{
		return FusionGlobalScriptableObject<PhotonAppSettings>.TryGetGlobalInternal(out settings);
	}
}
