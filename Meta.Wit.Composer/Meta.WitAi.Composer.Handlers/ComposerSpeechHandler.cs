using System;
using Meta.WitAi.Composer.Data;
using Meta.WitAi.Composer.Interfaces;
using Meta.WitAi.TTS.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Composer.Handlers;

public class ComposerSpeechHandler : MonoBehaviour, IComposerSpeechHandler
{
	[Tooltip("If true, queues tts phrases and plays them back in order.  If false, stops previous phrase to play new phrases")]
	public bool queuePhrases = true;

	[SerializeField]
	[FormerlySerializedAs("speakerNameContextMapKey")]
	public string SpeakerNameContextMapKey = "wit_composer_speaker";

	[SerializeField]
	[FormerlySerializedAs("_speakers")]
	public ComposerSpeakerData[] Speakers;

	public void SpeakPhrase(ComposerSessionData sessionData)
	{
		string responsePhrase = sessionData.responseData.responsePhrase;
		TTSSpeaker speaker = GetSpeaker(sessionData);
		if (speaker == null)
		{
			VLog.E($"Composer Speech Handler - No Speaker Found\nPhrase: {responsePhrase}\nPartial: {sessionData.responseData.responseIsFinal}");
			return;
		}
		if (sessionData.responseData.responseTtsSettings != null)
		{
			speaker.SetVoiceOverride(sessionData.responseData.responseTtsSettings);
		}
		if (queuePhrases)
		{
			speaker.SpeakQueued(responsePhrase);
		}
		else
		{
			speaker.Speak(responsePhrase);
		}
	}

	public bool IsSpeaking(ComposerSessionData sessionData)
	{
		TTSSpeaker speaker = GetSpeaker(sessionData);
		if (speaker != null)
		{
			if (!speaker.IsLoading)
			{
				return speaker.IsSpeaking;
			}
			return true;
		}
		return false;
	}

	private TTSSpeaker GetSpeaker(ComposerSessionData sessionData)
	{
		if (Speakers == null || Speakers.Length == 0)
		{
			return null;
		}
		int num = 0;
		string speakerName = GetSpeakerName(sessionData.contextMap);
		if (!string.IsNullOrEmpty(speakerName))
		{
			num = Array.FindIndex(Speakers, (ComposerSpeakerData s) => string.Equals(s.SpeakerName, speakerName, StringComparison.CurrentCultureIgnoreCase));
			if (num == -1)
			{
				return null;
			}
		}
		return Speakers[num].Speaker;
	}

	public string GetSpeakerName(ComposerContextMap contextMap)
	{
		if (contextMap != null && !(contextMap.Data == null))
		{
			return contextMap.Data[SpeakerNameContextMapKey].Value;
		}
		return null;
	}
}
