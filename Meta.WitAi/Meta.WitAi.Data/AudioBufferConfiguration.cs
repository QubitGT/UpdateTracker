using System;
using UnityEngine;

namespace Meta.WitAi.Data;

[Serializable]
public class AudioBufferConfiguration
{
	[Tooltip("The length of the individual samples read from the audio source")]
	[Range(10f, 500f)]
	[SerializeField]
	public int sampleLengthInMs = 10;

	[Tooltip("The total audio data that should be buffered for lookback purposes on sound based activations.")]
	[SerializeField]
	public float micBufferLengthInSeconds = 1f;

	[Tooltip("The audio encoding to be used for transmission of audio data, should keep as default in almost all scenarios.  Adjust encoding directly on IAudioInput script such as Mic to capture at different rates.")]
	[SerializeField]
	public AudioEncoding encoding = new AudioEncoding();
}
