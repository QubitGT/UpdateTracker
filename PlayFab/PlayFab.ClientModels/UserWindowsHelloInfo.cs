using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class UserWindowsHelloInfo : PlayFabBaseModel
{
	public string WindowsHelloDeviceName;

	public string WindowsHelloPublicKeyHash;
}
