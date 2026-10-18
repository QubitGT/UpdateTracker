using System;
using Meta.WitAi.Data;

namespace Meta.WitAi.Interfaces;

public interface IAudioInputSource
{
	bool IsRecording { get; }

	AudioEncoding AudioEncoding { get; }

	bool IsMuted { get; }

	event Action OnStartRecording;

	event Action OnStartRecordingFailed;

	event Action<int, float[], float> OnSampleReady;

	event Action OnStopRecording;

	event Action OnMicMuted;

	event Action OnMicUnmuted;

	void StartRecording(int sampleLen);

	void StopRecording();
}
