using System;

namespace Meta.WitAi.TTS.Data;

[Serializable]
public class TTSDiskCacheSettings
{
	public TTSDiskCacheLocation DiskCacheLocation;

	public bool StreamFromDisk;

	public float StreamBufferLength = 5f;
}
