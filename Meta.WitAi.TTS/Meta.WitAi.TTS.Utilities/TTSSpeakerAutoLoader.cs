using System.Collections.Generic;
using Meta.WitAi.TTS.Data;
using UnityEngine;

namespace Meta.WitAi.TTS.Utilities;

[RequireComponent(typeof(TTSSpeaker))]
public class TTSSpeakerAutoLoader : MonoBehaviour, ITTSPhraseProvider
{
	public TTSSpeaker Speaker;

	public TextAsset PhraseFile;

	[SerializeField]
	private string[] _phrases;

	public bool LoadManually;

	private TTSClipData[] _clips;

	private int _clipsLoading;

	public string[] Phrases => _phrases;

	public TTSClipData[] Clips => _clips;

	public bool IsLoaded => _clipsLoading == 0;

	protected virtual void Start()
	{
		if (!LoadManually)
		{
			LoadClips();
		}
	}

	public virtual void LoadClips()
	{
		if (_clips != null)
		{
			VLog.W("Cannot autoload clips twice.");
			return;
		}
		_phrases = GetAllPhrases().ToArray();
		List<TTSClipData> list = new List<TTSClipData>();
		string[] phrases = _phrases;
		foreach (string textToSpeak in phrases)
		{
			_clipsLoading++;
			TTSClipData item = TTSService.Instance.Load(textToSpeak, Speaker.VoiceID, null, null, OnClipReady);
			list.Add(item);
		}
		_clips = list.ToArray();
	}

	public virtual List<string> GetAllPhrases()
	{
		SetupSpeaker();
		List<string> list = new List<string>();
		AddUniquePhrases(list, PhraseFile?.text.Split('\n'));
		AddUniquePhrases(list, Phrases);
		List<string> list2 = new List<string>();
		for (int i = 0; i < list.Count; i++)
		{
			List<string> finalText = Speaker.GetFinalText(list[i]);
			if (finalText != null && finalText.Count > 0)
			{
				list2.AddRange(finalText);
			}
		}
		return list2;
	}

	private void AddUniquePhrases(List<string> list, string[] newPhrases)
	{
		if (newPhrases == null)
		{
			return;
		}
		foreach (string text in newPhrases)
		{
			if (!string.IsNullOrEmpty(text) && !list.Contains(text))
			{
				list.Add(text);
			}
		}
	}

	protected virtual void SetupSpeaker()
	{
		if (!Speaker)
		{
			Speaker = base.gameObject.GetComponent<TTSSpeaker>();
			if (!Speaker)
			{
				Speaker = base.gameObject.AddComponent<TTSSpeaker>();
			}
		}
	}

	protected virtual void OnClipReady(TTSClipData clipData, string error)
	{
		_clipsLoading--;
	}

	protected virtual void OnDestroy()
	{
		UnloadClips();
	}

	protected virtual void UnloadClips()
	{
		if (_clips != null)
		{
			TTSClipData[] clips = _clips;
			foreach (TTSClipData clipData in clips)
			{
				TTSService.Instance?.Unload(clipData);
			}
			_clips = null;
			_phrases = null;
		}
	}

	public virtual List<string> GetVoiceIds()
	{
		SetupSpeaker();
		string text = Speaker?.VoiceSettings.SettingsId;
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		return new List<string> { text };
	}

	public virtual List<string> GetVoicePhrases(string voiceId)
	{
		return GetAllPhrases();
	}
}
