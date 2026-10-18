namespace Meta.Voice.Audio;

public interface IAudioClipStream
{
	bool IsReady { get; }

	bool IsComplete { get; }

	int Channels { get; }

	int SampleRate { get; }

	int AddedSamples { get; }

	int ExpectedSamples { get; }

	int TotalSamples { get; }

	float Length { get; }

	float StreamReadyLength { get; }

	AudioClipStreamSampleDelegate OnAddSamples { get; set; }

	AudioClipStreamDelegate OnStreamReady { get; set; }

	AudioClipStreamDelegate OnStreamUpdated { get; set; }

	AudioClipStreamDelegate OnStreamComplete { get; set; }

	AudioClipStreamDelegate OnStreamUnloaded { get; set; }

	void AddSamples(float[] samples, int offset, int length);

	void SetExpectedSamples(int expectedSamples);

	void Unload();
}
