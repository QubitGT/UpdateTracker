using System.Collections.Concurrent;
using System.Linq;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Interfaces;
using UnityEngine;

namespace Meta.WitAi.TTS.Integrations;

public class BaseTTSRuntimeCache : MonoBehaviour, ITTSRuntimeCacheHandler
{
	protected ConcurrentDictionary<string, TTSClipData> _clips = new ConcurrentDictionary<string, TTSClipData>();

	public event TTSClipCallback OnClipAdded;

	public event TTSClipCallback OnClipRemoved;

	public virtual TTSClipData[] GetClips()
	{
		return _clips.Values.ToArray();
	}

	protected virtual void OnDestroy()
	{
		_clips.Clear();
	}

	public virtual TTSClipData GetClip(string clipId)
	{
		_clips.TryGetValue(clipId, out var value);
		return value;
	}

	public virtual bool AddClip(TTSClipData clipData)
	{
		if (clipData == null || string.IsNullOrEmpty(clipData.clipID))
		{
			return false;
		}
		if (_clips.TryGetValue(clipData.clipID, out var value) && value != null && value.Equals(clipData))
		{
			return true;
		}
		_clips[clipData.clipID] = clipData;
		SetupClip(clipData);
		return true;
	}

	protected virtual void SetupClip(TTSClipData clipData)
	{
		this.OnClipAdded?.Invoke(clipData);
	}

	public virtual void RemoveClip(string clipID)
	{
		if ((!_clips.TryGetValue(clipID, out var value) || !string.IsNullOrEmpty(value.textToSpeak)) && _clips.TryRemove(clipID, out value))
		{
			BreakdownClip(value);
		}
	}

	protected virtual void BreakdownClip(TTSClipData clipData)
	{
		clipData.clipStream?.Unload();
		clipData.clipStream = null;
		this.OnClipRemoved?.Invoke(clipData);
	}
}
