using System;
using Meta.WitAi.Data.Configuration;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.TTS.Integrations;

[Serializable]
public struct TTSWitRequestSettings
{
	[FormerlySerializedAs("configuration")]
	[Tooltip("The configuration used for audio requests.")]
	[SerializeField]
	internal WitConfiguration _configuration;

	[Tooltip("The desired audio type to be requested from wit.")]
	public TTSWitAudioType audioType;

	[Tooltip("Whether or not audio should be streamed from wit if possible.")]
	public bool audioStream;

	[Tooltip("Whether or not events should be requested along with audio data.")]
	public bool useEvents;

	[Tooltip("Number of audio clip streams to pool immediately on first enable.")]
	public int audioStreamPreloadCount;

	[Tooltip("The total number of seconds to be buffered in order to consider ready.")]
	public float audioReadyDuration;

	[Tooltip("Maximum length of audio clip stream in seconds.")]
	public float audioMaxDuration;
}
