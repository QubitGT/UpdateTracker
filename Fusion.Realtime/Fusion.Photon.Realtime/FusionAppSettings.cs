using System;

namespace Fusion.Photon.Realtime;

[Serializable]
public class FusionAppSettings : AppSettings
{
	[InlineHelp]
	public EncryptionMode encryptionMode;

	[InlineHelp]
	public int emptyRoomTtl;

	public new FusionAppSettings GetCopy()
	{
		FusionAppSettings fusionAppSettings = new FusionAppSettings();
		CopyTo(fusionAppSettings);
		fusionAppSettings.encryptionMode = encryptionMode;
		fusionAppSettings.emptyRoomTtl = emptyRoomTtl;
		return fusionAppSettings;
	}

	public override string ToString()
	{
		return $"encryptionMode {encryptionMode}, emptyRoomTtl {emptyRoomTtl}, {ToStringFull()}";
	}
}
