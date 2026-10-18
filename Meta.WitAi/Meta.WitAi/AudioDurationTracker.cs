using System;
using Meta.WitAi.Data;

namespace Meta.WitAi;

public class AudioDurationTracker
{
	private readonly string _requestId;

	private double _bytesCaptured;

	private readonly int _bytesPerSample;

	private readonly AudioEncoding _audioEncoding;

	private long _finalizeTimeStamp;

	private double _audioDurationMs;

	public AudioDurationTracker(string requestId, AudioEncoding audioEncoding)
	{
		_requestId = requestId;
		_audioEncoding = audioEncoding;
		_bytesPerSample = _audioEncoding.bits / 8;
	}

	public void AddBytes(long bytes)
	{
		_bytesCaptured += bytes;
	}

	public void FinalizeAudio()
	{
		_finalizeTimeStamp = DateTime.UtcNow.Ticks / 10000;
		_audioDurationMs = _bytesCaptured / (double)(_audioEncoding.samplerate * _audioEncoding.numChannels * _bytesPerSample) * 1000.0;
	}

	public long GetFinalizeTimeStamp()
	{
		return _finalizeTimeStamp;
	}

	public double GetAudioDuration()
	{
		return _audioDurationMs;
	}

	public string GetRequestId()
	{
		return _requestId;
	}
}
