using System;
using Meta.Voice;
using Meta.WitAi.Data;
using UnityEngine;

namespace Meta.WitAi.Events;

[Serializable]
public class AudioBufferEvents
{
	public delegate void OnSampleReadyEvent(RingBuffer<byte>.Marker marker, float levelMax);

	public Action<VoiceAudioInputState> OnAudioStateChange;

	public OnSampleReadyEvent OnSampleReady;

	[Tooltip("Fired when a sample is received from an audio input source")]
	public WitSampleEvent OnSampleReceived = new WitSampleEvent();

	[Tooltip("Called when the volume level of the mic input has changed")]
	public WitMicLevelChangedEvent OnMicLevelChanged = new WitMicLevelChangedEvent();

	[Header("Data")]
	public WitByteDataEvent OnByteDataReady = new WitByteDataEvent();

	public WitByteDataEvent OnByteDataSent = new WitByteDataEvent();
}
