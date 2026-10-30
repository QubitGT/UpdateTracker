using UnityEngine;

public class PhotonAuthenticatorSettings
{
	public static string PunAppId;

	public static string VoiceAppId;

	static PhotonAuthenticatorSettings()
	{
		Load("PhotonAuthenticatorSettings");
	}

	public static void Load(string path)
	{
		PhotonAuthenticatorSettingsScriptableObject photonAuthenticatorSettingsScriptableObject = Resources.Load<PhotonAuthenticatorSettingsScriptableObject>(path);
		PunAppId = photonAuthenticatorSettingsScriptableObject.PunAppId;
		VoiceAppId = photonAuthenticatorSettingsScriptableObject.VoiceAppId;
	}
}
