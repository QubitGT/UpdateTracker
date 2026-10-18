using System.Collections;
using System.Text;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.WitAi.TTS.UX;

public class TTSSpeakerStatusLabel : TTSSpeakerObserver
{
	[SerializeField]
	private Text _label;

	private bool _needsRefresh = true;

	private Coroutine _refreshUpdater;

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshLabel();
		_refreshUpdater = StartCoroutine(RefreshUpdater());
	}

	protected override void OnDisable()
	{
		if (_refreshUpdater != null)
		{
			StopCoroutine(_refreshUpdater);
			_refreshUpdater = null;
		}
	}

	protected override void OnLoadBegin(TTSSpeaker speaker, TTSClipData clipData)
	{
		_needsRefresh = true;
	}

	protected override void OnLoadAbort(TTSSpeaker speaker, TTSClipData clipData)
	{
		_needsRefresh = true;
	}

	protected override void OnLoadFailed(TTSSpeaker speaker, TTSClipData clipData, string error)
	{
		_needsRefresh = true;
	}

	protected override void OnLoadSuccess(TTSSpeaker speaker, TTSClipData clipData)
	{
		_needsRefresh = true;
	}

	protected override void OnPlaybackReady(TTSSpeaker speaker, TTSClipData clipData)
	{
		_needsRefresh = true;
	}

	protected override void OnPlaybackStart(TTSSpeaker speaker, TTSClipData clipData)
	{
		_needsRefresh = true;
	}

	protected override void OnPlaybackCancelled(TTSSpeaker speaker, TTSClipData clipData, string reason)
	{
		_needsRefresh = true;
	}

	protected override void OnPlaybackComplete(TTSSpeaker speaker, TTSClipData clipData)
	{
		_needsRefresh = true;
	}

	private IEnumerator RefreshUpdater()
	{
		while (true)
		{
			if (_needsRefresh)
			{
				RefreshLabel();
			}
			yield return null;
		}
	}

	private void RefreshLabel()
	{
		_needsRefresh = false;
		StringBuilder stringBuilder = new StringBuilder();
		if (base.Speaker.IsSpeaking)
		{
			AppendClipText(stringBuilder, base.Speaker.SpeakingClip, "Speaking");
		}
		int num = 1;
		foreach (TTSClipData queuedClip in base.Speaker.QueuedClips)
		{
			AppendClipText(stringBuilder, queuedClip, $"Queue[{num}]");
			num++;
		}
		_label.text = stringBuilder.ToString();
		_label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _label.preferredHeight);
	}

	private void AppendClipText(StringBuilder status, TTSClipData clipData, string clipKey)
	{
		status.AppendLine(clipKey);
		status.AppendLine("\tText: '" + clipData.textToSpeak + "'");
		status.AppendLine("\tVoice: " + ((clipData.voiceSettings == null) ? "" : clipData.voiceSettings.SettingsId));
		status.AppendLine("\tType: " + clipData.extension);
		status.AppendLine($"\tStatus: {clipData.loadState}");
		if (clipData.loadState == TTSClipLoadState.Loaded)
		{
			status.AppendLine($"\tLoad Time: {clipData.readyDuration:0.000} seconds");
		}
		status.Append("\n");
	}
}
