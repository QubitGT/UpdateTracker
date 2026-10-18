using System;
using System.Collections.Generic;
using Liv.Lck.Collections;
using Liv.Lck.NativeMicrophone;
using Liv.Lck.Settings;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

namespace Liv.Lck;

internal class LckAudioMixer : ILckAudioMixer, IDisposable, ILckLateUpdate
{
	private ILckAudioSource _gameAudioSource;

	private bool _isGameAudioMuted;

	private float _gameAudioGain = 1f;

	private Queue<float> _gameAudioQueue = new Queue<float>();

	private ILckAudioSource _nativeMicrophoneCapture;

	private bool _isMicrophoneMuted;

	private float _microphoneGain = 1f;

	private Queue<float> _microphoneQueue = new Queue<float>();

	private AudioBuffer _micAudioBuffer = new AudioBuffer(96000);

	private float _lastMicrophoneLevel;

	private AudioBuffer _gameAudioBuffer = new AudioBuffer(96000);

	private float _lastGameAudioLevel;

	private AudioBuffer _mixedAudioBuffer = new AudioBuffer(96000);

	private int _remainingGameAudioValuesToAdjust;

	private int _gameAudioValueCountOffset;

	private readonly int _sampleRate;

	private Component _audioCaptureMarker;

	private const int _targetAudioBufferLength = 1024;

	private ILckAudioLimiter _lckAudioLimiterHard;

	private ILckAudioLimiter _lckAudioLimiterSoft;

	private ILckAudioLimiter _lckAudioLimiterCurve;

	private const int TrackTimeDifferenceToleranceMilli = 100;

	private const int NumberOfChannels = 2;

	private float? _micCaptureStartRecordingTime;

	private int _totalGameSamples;

	private int _totalMicSamples;

	private static readonly ProfilerMarker _lateUpdateProfileMarker = new ProfilerMarker("LckAudioMixer.LateUpdate");

	[Preserve]
	public LckAudioMixer(ILckEventBus eventBus, ILckOutputConfigurer outputConfigurer)
	{
		_sampleRate = (int)outputConfigurer.GetAudioSampleRate().Result;
		VerifyAudioCaptureComponent();
		_nativeMicrophoneCapture = new LckNativeMicrophone(_sampleRate);
		_lckAudioLimiterHard = new LckAudioHardLimiter(0.65f, 6f);
		_lckAudioLimiterSoft = new LckAudioSoftLimiter(0.6f, 0.8f, 12f);
		eventBus.AddListener<LckEvents.EncoderStartedEvent>(OnEncoderStarted);
		LckUpdateManager.RegisterSingleLateUpdate(this);
	}

	public AudioBuffer GetMixedAudio(float recordingTime)
	{
		return MixAudioArrays(recordingTime);
	}

	public void ReadAvailableAudioData()
	{
		if (VerifyAudioCaptureComponent())
		{
			if (_nativeMicrophoneCapture.IsCapturing())
			{
				_nativeMicrophoneCapture.GetAudioData(MicrophoneAudioDataCallback);
			}
			_gameAudioSource.GetAudioData(GameAudioDataCallback);
		}
	}

	public void EnableCapture()
	{
		VerifyAudioCaptureComponent();
		if (_gameAudioSource == null)
		{
			LckLog.LogError("Unable to enable audio capture - game audio source is null", "EnableCapture", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 98);
			return;
		}
		_gameAudioSource.EnableCapture();
		_micCaptureStartRecordingTime = null;
		_microphoneQueue.Clear();
		_gameAudioQueue.Clear();
		_totalGameSamples = 0;
		_totalMicSamples = 0;
		PrepareGameAudioSyncOffset();
	}

	public void DisableCapture()
	{
		_gameAudioSource?.DisableCapture();
		_micCaptureStartRecordingTime = null;
		_microphoneQueue.Clear();
		_gameAudioQueue.Clear();
	}

	private AudioBuffer MixAudioArrays(float recordingTime)
	{
		if (_gameAudioSource == null)
		{
			LckLog.LogError("LCK No game audio source found", "MixAudioArrays", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 126);
			return null;
		}
		bool flag = _nativeMicrophoneCapture.IsCapturing();
		if (!_micCaptureStartRecordingTime.HasValue && flag)
		{
			_micCaptureStartRecordingTime = recordingTime;
		}
		EnqueueGameBufferSamples();
		if (flag)
		{
			EnqueueMicBufferSamples();
		}
		else
		{
			_microphoneQueue.Clear();
		}
		EnsureAudioSourceSamplesWithinTolerance("_gameAudioSource", recordingTime, _gameAudioQueue, ref _totalGameSamples);
		if (flag && _micCaptureStartRecordingTime.HasValue)
		{
			float captureTime = recordingTime - _micCaptureStartRecordingTime.Value;
			EnsureAudioSourceSamplesWithinTolerance("_nativeMicrophoneCapture", captureTime, _microphoneQueue, ref _totalMicSamples);
		}
		int blocks = DetermineAvailableBlockCount(flag);
		return MixBlocksIntoMixedAudioBuffer(flag, blocks);
	}

	private void EnqueueGameBufferSamples()
	{
		if (_remainingGameAudioValuesToAdjust > 0)
		{
			int num = Math.Max(_remainingGameAudioValuesToAdjust - _gameAudioQueue.Count, 0);
			for (int i = 0; i < num; i++)
			{
				_gameAudioQueue.Enqueue(0f);
			}
			_remainingGameAudioValuesToAdjust -= num;
		}
		_totalGameSamples += _gameAudioBuffer.Count / 2;
		for (int j = 0; j < _gameAudioBuffer.Count; j++)
		{
			_gameAudioQueue.Enqueue(_gameAudioBuffer[j] * _gameAudioGain * (_isGameAudioMuted ? 0f : 1f));
		}
		if (_remainingGameAudioValuesToAdjust < 0)
		{
			int num2 = Mathf.Min(Mathf.Abs(_remainingGameAudioValuesToAdjust), _gameAudioQueue.Count);
			for (int k = 0; k < num2; k++)
			{
				_gameAudioQueue.Dequeue();
			}
			_remainingGameAudioValuesToAdjust += num2;
		}
	}

	private void EnqueueMicBufferSamples()
	{
		if (_micAudioBuffer != null)
		{
			_totalMicSamples += _micAudioBuffer.Count / 2;
			for (int i = 0; i < _micAudioBuffer.Count; i++)
			{
				_microphoneQueue.Enqueue(_micAudioBuffer[i] * _microphoneGain * (_isMicrophoneMuted ? 0f : 1f));
			}
		}
	}

	private int DetermineAvailableBlockCount(bool shouldIncludeMicAudio)
	{
		int num = CountAvailableGameBlocks();
		if (!shouldIncludeMicAudio)
		{
			return num;
		}
		int b = CountAvailableMicrophoneBlocks();
		return Mathf.Min(num, b);
	}

	private int CountAvailableGameBlocks()
	{
		int num = _gameAudioQueue.Count;
		if (_gameAudioValueCountOffset > 0)
		{
			num -= _gameAudioValueCountOffset;
		}
		return Math.Max(0, num / 1024);
	}

	private int CountAvailableMicrophoneBlocks()
	{
		int num = _microphoneQueue.Count;
		if (_gameAudioValueCountOffset < 0)
		{
			num += _gameAudioValueCountOffset;
		}
		return Math.Max(0, num / 1024);
	}

	private AudioBuffer MixBlocksIntoMixedAudioBuffer(bool shouldIncludeMicAudio, int blocks)
	{
		int num = blocks * 1024;
		_mixedAudioBuffer.Clear();
		for (int i = 0; i < num; i++)
		{
			float num2 = _gameAudioQueue.Dequeue();
			if (shouldIncludeMicAudio)
			{
				num2 += _microphoneQueue.Dequeue();
			}
			float value = ApplyLimiter(num2);
			if (!_mixedAudioBuffer.TryAdd(value))
			{
				LckLog.LogWarning("LCK Mixed audio buffer overflow", "MixBlocksIntoMixedAudioBuffer", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 268);
				break;
			}
		}
		return _mixedAudioBuffer;
	}

	private float ApplyLimiter(float mixedAudioRaw)
	{
		float result = mixedAudioRaw;
		switch (LckSettings.Instance.AudioLimiter)
		{
		case LckSettings.LimiterType.SoftClip:
			result = LckAudioLimiterUtils.ApplySoftClip(mixedAudioRaw);
			break;
		case LckSettings.LimiterType.None:
			result = mixedAudioRaw;
			break;
		}
		return result;
	}

	private void MicrophoneAudioDataCallback(AudioBuffer audioBuffer)
	{
		_micAudioBuffer.Clear();
		if (audioBuffer.Count > 0)
		{
			if (!_micAudioBuffer.TryCopyFrom(audioBuffer))
			{
				LckLog.LogError("LCK Mic audio data copy failed", "MicrophoneAudioDataCallback", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 301);
			}
			else
			{
				_lastMicrophoneLevel = (_lastMicrophoneLevel + CalculateRootMeanSquare(_micAudioBuffer)) / 2f;
			}
		}
	}

	private void GameAudioDataCallback(AudioBuffer audioBuffer)
	{
		_gameAudioBuffer.Clear();
		if (audioBuffer.Count <= 0)
		{
			return;
		}
		if (!_gameAudioBuffer.TryCopyFrom(audioBuffer))
		{
			LckLog.LogError("LCK Game audio data copy failed", "GameAudioDataCallback", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 317);
			return;
		}
		_lastGameAudioLevel = (_lastGameAudioLevel + CalculateRootMeanSquare(audioBuffer)) / 2f;
		if (float.IsNaN(_lastGameAudioLevel))
		{
			_lastGameAudioLevel = 0f;
		}
	}

	private bool VerifyAudioCaptureComponent()
	{
		if (_audioCaptureMarker == null)
		{
			AudioListener[] array = UnityEngine.Object.FindObjectsOfType<AudioListener>(includeInactive: false);
			List<AudioListener> list = new List<AudioListener>();
			foreach (AudioListener audioListener in array)
			{
				if (audioListener.enabled)
				{
					list.Add(audioListener);
				}
			}
			if (list.Count == 0)
			{
				LckLog.Log("LCK Found no audio listener in the scene, looking for AudioCaptureMarker", "VerifyAudioCaptureComponent", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 346);
				LckAudioMarker[] array2 = UnityEngine.Object.FindObjectsOfType<LckAudioMarker>(includeInactive: false);
				if (array2.Length != 0)
				{
					_audioCaptureMarker = array2[0];
				}
				if (array2.Length > 1)
				{
					LckLog.LogError("LCK found more than one AudioCaptureMarker in the scene. This is not valid", "VerifyAudioCaptureComponent", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 357);
				}
			}
			else
			{
				if (list.Count > 0)
				{
					_audioCaptureMarker = list[0];
				}
				if (list.Count > 1)
				{
					LckLog.LogError("LCK found more than one active AudioListener in the scene. This is not valid", "VerifyAudioCaptureComponent", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 369);
				}
			}
		}
		if (_gameAudioSource == null)
		{
			_gameAudioSource = _audioCaptureMarker.gameObject.GetComponent<ILckAudioSource>();
			if (_gameAudioSource == null)
			{
				_gameAudioSource = _audioCaptureMarker.gameObject.AddComponent<LckAudioCapture>();
			}
		}
		return true;
	}

	private bool CheckMicAudioPermissions()
	{
		return true;
	}

	public LckResult SetMicrophoneCaptureActive(bool active)
	{
		_lastMicrophoneLevel = 0f;
		if (active)
		{
			if (!CheckMicAudioPermissions())
			{
				return LckResult.NewError(LckError.MicrophonePermissionDenied, "The app has not been granted microphone permissions.");
			}
			_nativeMicrophoneCapture.EnableCapture();
		}
		else
		{
			_nativeMicrophoneCapture.DisableCapture();
			_micCaptureStartRecordingTime = null;
		}
		_totalMicSamples = 0;
		return LckResult.NewSuccess();
	}

	public LckResult<bool> GetMicrophoneCaptureActive()
	{
		return LckResult<bool>.NewSuccess(_nativeMicrophoneCapture.IsCapturing());
	}

	public LckResult SetGameAudioMute(bool isMute)
	{
		_isGameAudioMuted = isMute;
		return LckResult.NewSuccess();
	}

	public LckResult<bool> IsGameAudioMute()
	{
		return LckResult<bool>.NewSuccess(_isGameAudioMuted);
	}

	public void SetMicrophoneGain(float gain)
	{
		_microphoneGain = gain;
	}

	public void SetGameAudioGain(float gain)
	{
		_gameAudioGain = gain;
	}

	public float GetMicrophoneOutputLevel()
	{
		return _lastMicrophoneLevel;
	}

	public float GetGameOutputLevel()
	{
		return _lastGameAudioLevel;
	}

	private static float CalculateRootMeanSquare(AudioBuffer audioBuffer)
	{
		if (audioBuffer == null || audioBuffer.Count == 0)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < audioBuffer.Count; i++)
		{
			num += audioBuffer[i] * audioBuffer[i];
		}
		return Mathf.Sqrt(num / (float)audioBuffer.Count);
	}

	private static void PadWithSilence(Queue<float> audioQueue, int samplesToAdd, ref int runningSampleCount)
	{
		for (int i = 0; i < samplesToAdd; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				audioQueue.Enqueue(0f);
			}
			runningSampleCount++;
		}
	}

	private void EnsureAudioSourceSamplesWithinTolerance(string audioSourceName, float captureTime, Queue<float> audioSourceQueue, ref int audioSourceRunningSampleCount)
	{
		int num = Mathf.FloorToInt(captureTime * (float)_sampleRate);
		int num2 = audioSourceRunningSampleCount - num;
		int num3 = Math.Abs(num2);
		int num4 = 100 * (_sampleRate / 1000);
		if (num3 > num4)
		{
			if (num2 < 0)
			{
				LckLog.LogWarning($"{audioSourceName} is behind expected sample count ({num}) by " + $"{num3} samples - Padding with silence for missing samples", "EnsureAudioSourceSamplesWithinTolerance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 509);
				PadWithSilence(audioSourceQueue, num3, ref audioSourceRunningSampleCount);
			}
			else
			{
				LckLog.LogWarning($"{audioSourceName} is ahead of expected sample count ({num}) by " + $"{num3} samples - Expecting this to be a result of a lag spike", "EnsureAudioSourceSamplesWithinTolerance", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioMixer.cs", 517);
			}
		}
	}

	private void PrepareGameAudioSyncOffset()
	{
		float num = LckSettings.Instance.GameAudioSyncTimeOffsetInMS / 1000f;
		_gameAudioValueCountOffset = Mathf.CeilToInt(num * (float)_sampleRate) * 2;
		_remainingGameAudioValuesToAdjust = _gameAudioValueCountOffset;
	}

	private void OnEncoderStarted(LckEvents.EncoderStartedEvent encoderStartedEvent)
	{
		if (encoderStartedEvent.Result.Success)
		{
			EnableCapture();
		}
	}

	public void LateUpdate()
	{
		using (_lateUpdateProfileMarker.Auto())
		{
			ReadAvailableAudioData();
		}
	}

	public void Dispose()
	{
		LckUpdateManager.UnregisterSingleLateUpdate(this);
		(_nativeMicrophoneCapture as LckNativeMicrophone)?.Dispose();
		_nativeMicrophoneCapture = null;
	}
}
