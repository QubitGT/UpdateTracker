using System.Collections.Generic;
using Meta.WitAi.TTS.Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.TTS.Integrations;

public class TTSRuntimeLRUCache : BaseTTSRuntimeCache
{
	[Header("Runtime Cache Settings")]
	[Tooltip("Whether or not to unload clip data after the clip capacity is hit")]
	[FormerlySerializedAs("_clipLimit")]
	public bool ClipLimit;

	[Tooltip("The maximum clips allowed in the runtime cache")]
	[FormerlySerializedAs("_clipCapacity")]
	[Min(1f)]
	public int ClipCapacity = 5;

	[Tooltip("Whether or not to unload clip data after the ram capacity is hit")]
	[FormerlySerializedAs("_ramLimit")]
	public bool RamLimit = true;

	[Tooltip("The maximum amount of RAM allowed in the runtime cache in KBs.  For example, 24k samples per second * 2bits per sample * 10 minutes (600 seconds) = 3600KBs")]
	[FormerlySerializedAs("_ramCapacity")]
	[Min(1f)]
	public int RamCapacity = 3600;

	private List<string> _clipOrder = new List<string>();

	public override TTSClipData[] GetClips()
	{
		TTSClipData[] array = new TTSClipData[_clipOrder.Count];
		for (int i = 0; i < array.Length; i++)
		{
			_clips.TryGetValue(_clipOrder[i], out var value);
			array[i] = value;
		}
		return array;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_clipOrder.Clear();
	}

	public bool RefreshClipLRU(string clipId)
	{
		int num = _clipOrder.IndexOf(clipId);
		if (num != -1)
		{
			_clipOrder.RemoveAt(num);
			_clipOrder.Add(clipId);
			return true;
		}
		return false;
	}

	public override TTSClipData GetClip(string clipId)
	{
		RefreshClipLRU(clipId);
		return base.GetClip(clipId);
	}

	protected override void SetupClip(TTSClipData clipData)
	{
		_clipOrder.Add(clipData.clipID);
		base.SetupClip(clipData);
	}

	public override bool AddClip(TTSClipData clipData)
	{
		if (!base.AddClip(clipData))
		{
			return false;
		}
		RefreshClipLRU(clipData.clipID);
		while (IsCacheFull() && _clipOrder.Count > 0)
		{
			string clipID = _clipOrder[0];
			_clipOrder.RemoveAt(0);
			RemoveClip(clipID);
		}
		return _clipOrder.Count > 0;
	}

	protected override void BreakdownClip(TTSClipData clipData)
	{
		int num = _clipOrder.IndexOf(clipData.clipID);
		if (num != -1)
		{
			_clipOrder.RemoveAt(num);
		}
		base.BreakdownClip(clipData);
	}

	private bool IsCacheFull()
	{
		if (ClipLimit && _clipOrder.Count > ClipCapacity)
		{
			return true;
		}
		if (RamLimit && GetCacheDiskSize() > RamCapacity)
		{
			return true;
		}
		return false;
	}

	public int GetCacheDiskSize()
	{
		long num = 0L;
		foreach (string key in _clips.Keys)
		{
			if (_clips[key].clipStream != null)
			{
				num += GetClipBytes(_clips[key].clipStream.Channels, _clips[key].clipStream.TotalSamples);
			}
		}
		return (int)(num / 1024) + 1;
	}

	public static long GetClipBytes(AudioClip clip)
	{
		if (clip != null)
		{
			return GetClipBytes(clip.channels, clip.samples);
		}
		return 0L;
	}

	public static long GetClipBytes(int channels, int samples)
	{
		return channels * samples * 2;
	}
}
