using System;
using System.Collections;
using Liv.Lck.Collections;
using Liv.Lck.Encoding;
using Liv.Lck.ErrorHandling;
using Liv.Lck.Telemetry;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckEncodeLooper : ILckEncodeLooper, IDisposable, ILckEarlyUpdate
{
	private readonly ILckEncoder _encoder;

	private readonly ILckOutputConfigurer _outputConfigurer;

	private readonly ILckAudioMixer _audioMixer;

	private readonly ILckVideoCapturer _videoCapturer;

	private readonly ILckEventBus _eventBus;

	private readonly ILckTelemetryClient _telemetryClient;

	private float _pausedForTime;

	private float _videoTime;

	private float _prevVideoTime;

	private bool _disposed;

	private const float MinVideoTimeIncrement = 0.001f;

	private const float TrackTimestampDifferenceTolerance = 0.3f;

	private const int EncodingWarmupFrames = 3;

	private const string EncodingWarmupCoroutineName = "LckEncodeLooper:StartEncodingAfterWarmupFrames";

	[Preserve]
	public LckEncodeLooper(ILckEncoder encoder, ILckOutputConfigurer outputConfigurer, ILckAudioMixer audioMixer, ILckVideoCapturer videoCapturer, ILckEventBus eventBus, ILckTelemetryClient telemetryClient)
	{
		_encoder = encoder;
		_outputConfigurer = outputConfigurer;
		_audioMixer = audioMixer;
		_videoCapturer = videoCapturer;
		_eventBus = eventBus;
		_telemetryClient = telemetryClient;
		_eventBus.AddListener<LckEvents.EncoderStartedEvent>(OnEncoderStarted);
	}

	public void EarlyUpdate()
	{
		ILckEncoder encoder = _encoder;
		if (encoder == null || !encoder.IsActive())
		{
			UnregisterEncodeFrameEarlyUpdate();
			return;
		}
		float num = Time.unscaledDeltaTime;
		AudioBuffer mixedAudio = _audioMixer.GetMixedAudio(_videoTime + _pausedForTime);
		if (num > 1f)
		{
			LckLog.LogWarning("LCK detected lag spike during capture - adjusting capture time accordingly", "EarlyUpdate", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckEncodeLooper.cs", 70);
			uint num2 = _outputConfigurer.GetAudioSampleRate().Result * _outputConfigurer.GetNumberOfAudioChannels().Result;
			num = (float)mixedAudio.Count / (float)num2;
		}
		if (_encoder.IsPaused())
		{
			_pausedForTime += num;
		}
		else
		{
			if (!IsAudioDataValid(mixedAudio))
			{
				return;
			}
			EncoderSessionData currentSessionData = _encoder.GetCurrentSessionData();
			if (currentSessionData.EncodedAudioSamplesPerChannel == 0L && mixedAudio.Count == 0)
			{
				for (int i = 0; i < 1024; i++)
				{
					mixedAudio.TryAdd(0f);
				}
			}
			EnsureTrackTimeAlignment(ref _videoTime, CalculateAudioTime(), _prevVideoTime);
			if (!_encoder.EncodeFrame(_videoTime, mixedAudio, _videoCapturer.HasCurrentFrameBeenCaptured()))
			{
				HandleEncodeFrameError($"LCK EncodeFrame returned false. This indicates a critical error. (recordingTime: {currentSessionData.CaptureTimeSeconds}, audioTimestampSamples: {currentSessionData.EncodedAudioSamplesPerChannel})");
			}
			_videoTime += num;
		}
	}

	private float CalculateAudioTime()
	{
		EncoderSessionData currentSessionData = _encoder.GetCurrentSessionData();
		uint result = _outputConfigurer.GetAudioSampleRate().Result;
		return (float)currentSessionData.EncodedAudioSamplesPerChannel / (float)result;
	}

	private bool IsAudioDataValid(AudioBuffer audioData)
	{
		if (audioData != null)
		{
			return true;
		}
		EncoderSessionData currentSessionData = _encoder.GetCurrentSessionData();
		HandleEncodeFrameError($"LCK Audio data is null (captureTime: {currentSessionData.CaptureTimeSeconds}, audioTimestampSamples: {currentSessionData.EncodedAudioSamplesPerChannel})");
		return false;
	}

	private void StartEncodingFrames()
	{
		_videoTime = (_prevVideoTime = (_pausedForTime = 0f));
		LckUpdateManager.RegisterSingleEarlyUpdate(this);
	}

	private void UnregisterEncodeFrameEarlyUpdate()
	{
		LckUpdateManager.UnregisterSingleEarlyUpdate(this);
	}

	private void HandleEncodeFrameError(string errorMessage)
	{
		LckLog.LogError(errorMessage, "HandleEncodeFrameError", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckEncodeLooper.cs", 138);
		_eventBus.Trigger(new LckEvents.CaptureErrorEvent(new LckCaptureError(CaptureErrorType.EncoderError, errorMessage)));
	}

	private IEnumerator StartEncodingAfterWarmupFrames(int warmupFrameCount)
	{
		_videoCapturer.ForceCaptureAllFrames = true;
		while (warmupFrameCount > 0)
		{
			yield return null;
			warmupFrameCount--;
		}
		_videoCapturer.ForceCaptureAllFrames = false;
		StartEncodingFrames();
	}

	private void OnEncoderStarted(LckEvents.EncoderStartedEvent encoderStartedEvent)
	{
		if (encoderStartedEvent.Result.Success)
		{
			LckMonoBehaviourMediator.StartCoroutine("LckEncodeLooper:StartEncodingAfterWarmupFrames", StartEncodingAfterWarmupFrames(3));
		}
	}

	private static void EnsureTrackTimeAlignment(ref float videoTime, float audioTime, float prevVideoTime)
	{
		float num = videoTime - audioTime;
		float num2 = Math.Abs(num);
		if (!(num2 <= 0.3f))
		{
			LckLog.LogError($"Video track is {Mathf.FloorToInt(1000f * num2)}ms " + ((num > 0f) ? "ahead of" : "behind") + " audio track - adjusting video time to re-sync", "EnsureTrackTimeAlignment", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\LckEncodeLooper.cs", 180);
			videoTime = Math.Max(audioTime, prevVideoTime + 0.001f);
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			LckMonoBehaviourMediator.StopCoroutineByName("LckEncodeLooper:StartEncodingAfterWarmupFrames");
			UnregisterEncodeFrameEarlyUpdate();
			_disposed = true;
		}
	}
}
