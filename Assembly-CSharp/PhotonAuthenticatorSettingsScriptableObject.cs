using UnityEngine;

[CreateAssetMenu(fileName = "PhotonAuthenticatorSettings", menuName = "ScriptableObjects/Networking/PhotonAuthenticatorSettings")]
public class PhotonAuthenticatorSettingsScriptableObject : ScriptableObject
{
	public string PunAppId;

	public string VoiceAppId;
}
