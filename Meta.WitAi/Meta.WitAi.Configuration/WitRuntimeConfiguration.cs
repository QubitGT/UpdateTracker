using System;
using Meta.Voice.Net.PubSub;
using Meta.WitAi.Data.Configuration;
using Meta.WitAi.Interfaces;
using Meta.WitAi.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.Configuration;

[Serializable]
public class WitRuntimeConfiguration
{
	[Tooltip("Configuration for the application used in this instance of Wit.ai services")]
	[SerializeField]
	public WitConfiguration witConfiguration;

	[Header("Keepalive")]
	[Tooltip("The minimum volume from the mic needed to keep the activation alive")]
	[SerializeField]
	public float minKeepAliveVolume = 0.0005f;

	[FormerlySerializedAs("minKeepAliveTime")]
	[Tooltip("The amount of time in seconds an activation will be kept open after volume is under the keep alive threshold")]
	[SerializeField]
	[DynamicRange("RecordingTimeRange", float.MinValue, float.MaxValue)]
	public float minKeepAliveTimeInSeconds = 2f;

	[FormerlySerializedAs("minTranscriptionKeepAliveTime")]
	[Tooltip("The amount of time in seconds an activation will be kept open after words have been detected in the live transcription")]
	[SerializeField]
	[DynamicRange("RecordingTimeRange", float.MinValue, float.MaxValue)]
	public float minTranscriptionKeepAliveTimeInSeconds = 1f;

	[Tooltip("The maximum amount of time in seconds the mic will stay active")]
	[SerializeField]
	[DynamicRange("RecordingTimeRange", float.MinValue, float.MaxValue)]
	public float maxRecordingTime = 20f;

	[Tooltip("Overidde the current configuration timeout if greater than 0")]
	public int overrideTimeoutMs;

	[Header("Sound Activation")]
	[Tooltip("The minimum volume level needed to be heard to start collecting data from the audio source.")]
	[SerializeField]
	public float soundWakeThreshold = 0.0005f;

	[Tooltip("The length of the individual samples read from the audio source")]
	[Range(10f, 500f)]
	[SerializeField]
	public int sampleLengthInMs = 10;

	[Tooltip("The total audio data that should be buffered for lookback purposes on sound based activations.")]
	[SerializeField]
	public float micBufferLengthInSeconds = 1f;

	[Tooltip("The maximum amount of concurrent requests that can occur")]
	[Range(1f, 10f)]
	[SerializeField]
	public int maxConcurrentRequests = 5;

	[Header("Custom Transcription")]
	[Tooltip("If true, the audio recorded in the activation will be sent to Wit.ai for processing. If a custom transcription provider is set and this is false, only the transcription will be sent to Wit.ai for processing")]
	[SerializeField]
	public bool sendAudioToWit = true;

	[Tooltip("A custom provider that returns text to be used for nlu processing on activation instead of sending audio.")]
	[SerializeField]
	public CustomTranscriptionProvider customTranscriptionProvider;

	[Tooltip("If always record is set the mic will fill the mic data buffer as long as the component is enabled in the scene.")]
	public bool alwaysRecord;

	[Tooltip("The preferred number of seconds to offset from the time the activation happens. A negative value here could help to catch any words that may have been cut off at the beginning of an activation (assuming input is already being read into the buffer)")]
	public float preferredActivationOffset = -0.5f;

	[Header("Web Sockets")]
	[Tooltip("If enabled, only transcription requests will be made.")]
	public bool transcribeOnly;

	[Tooltip("Various publish and subscription options available for this specific service.")]
	public PubSubSettings pubSubSettings = new PubSubSettings(string.Empty);

	public Action OnConfigurationUpdated;

	protected virtual Vector2 RecordingTimeRange => new Vector2(0f, 20f);
}
