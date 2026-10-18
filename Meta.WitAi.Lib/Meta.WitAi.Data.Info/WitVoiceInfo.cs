using System;

namespace Meta.WitAi.Data.Info;

[Serializable]
public struct WitVoiceInfo
{
	public string name;

	public string locale;

	public string gender;

	public string[] styles;

	public string[] supported_features;
}
