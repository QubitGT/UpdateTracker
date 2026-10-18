using System.IO;
using Meta.Voice.Logging;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Interfaces;
using Meta.WitAi.Utilities;
using UnityEngine;

namespace Meta.WitAi.TTS.Integrations;

[LogCategory(LogCategory.TextToSpeech)]
public class TTSDiskCache : MonoBehaviour, ITTSDiskCacheHandler
{
	[Header("Disk Cache Settings")]
	[SerializeField]
	private string _diskPath = "TTS/";

	[SerializeField]
	private TTSDiskCacheSettings _defaultSettings = new TTSDiskCacheSettings();

	public string DiskPath => _diskPath;

	public TTSDiskCacheSettings DiskCacheDefaultSettings => _defaultSettings;

	public string GetDiskCachePath(TTSClipData clipData)
	{
		if (!ShouldCacheToDisk(clipData))
		{
			return string.Empty;
		}
		TTSDiskCacheLocation diskCacheLocation = clipData.diskCacheSettings.DiskCacheLocation;
		string text = string.Empty;
		switch (diskCacheLocation)
		{
		case TTSDiskCacheLocation.Persistent:
			text = Application.persistentDataPath;
			break;
		case TTSDiskCacheLocation.Temporary:
			text = Application.temporaryCachePath;
			break;
		case TTSDiskCacheLocation.Preload:
			text = Application.streamingAssetsPath;
			break;
		}
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		text = Path.Combine(text, DiskPath);
		if ((diskCacheLocation != TTSDiskCacheLocation.Preload || !Application.isPlaying) && !IOUtility.CreateDirectory(text))
		{
			VLog.E($"Failed to create tts directory\nPath: {text}\nLocation: {diskCacheLocation}");
			return string.Empty;
		}
		return Path.Combine(text, clipData.clipID + clipData.extension);
	}

	public bool ShouldCacheToDisk(TTSClipData clipData)
	{
		if (clipData != null && clipData.diskCacheSettings.DiskCacheLocation != 0)
		{
			return !string.IsNullOrEmpty(clipData.clipID);
		}
		return false;
	}
}
