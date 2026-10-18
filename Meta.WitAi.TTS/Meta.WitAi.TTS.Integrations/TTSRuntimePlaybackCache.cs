using System;
using System.Collections.Concurrent;
using Meta.WitAi.TTS.Data;
using UnityEngine;

namespace Meta.WitAi.TTS.Integrations;

public class TTSRuntimePlaybackCache : BaseTTSRuntimeCache
{
	private ConcurrentDictionary<string, int> _requests = new ConcurrentDictionary<string, int>();

	protected override void SetupClip(TTSClipData clipData)
	{
		_requests[clipData.clipID] = 0;
		clipData.onRequestBegin = (Action<TTSClipData>)Delegate.Combine(clipData.onRequestBegin, new Action<TTSClipData>(OnRequestBegin));
		clipData.onRequestComplete = (Action<TTSClipData>)Delegate.Combine(clipData.onRequestComplete, new Action<TTSClipData>(OnRequestComplete));
		base.SetupClip(clipData);
	}

	private void OnRequestBegin(TTSClipData clipData)
	{
		string clipID = clipData.clipID;
		if (!_requests.TryGetValue(clipID, out var value))
		{
			value = 0;
		}
		_requests[clipID] = value + 1;
	}

	private void OnRequestComplete(TTSClipData clipData)
	{
		string clipID = clipData.clipID;
		if (_requests.TryGetValue(clipID, out var value))
		{
			value = Mathf.Max(0, value - 1);
			_requests[clipID] = value;
			if (value == 0)
			{
				RemoveClip(clipData.clipID);
			}
		}
	}

	protected override void BreakdownClip(TTSClipData clipData)
	{
		clipData.onRequestBegin = (Action<TTSClipData>)Delegate.Remove(clipData.onRequestBegin, new Action<TTSClipData>(OnRequestBegin));
		clipData.onRequestComplete = (Action<TTSClipData>)Delegate.Remove(clipData.onRequestComplete, new Action<TTSClipData>(OnRequestComplete));
		_requests.TryRemove(clipData.clipID, out var _);
		base.BreakdownClip(clipData);
	}
}
