using System;
using System.Runtime.InteropServices;
using Liv.Lck.Collections;
using Liv.Lck.Utilities;
using UnityEngine;

namespace Liv.Lck;

internal class LckAudioCaptureFMODAndUnity : MonoBehaviour, ILckAudioSource
{
	private GCHandle _mObjHandle;

	private readonly AudioBuffer _tmpRemixBuffer = new AudioBuffer(98000);

	private readonly AudioBuffer _tmpAudio = new AudioBuffer(98000);

	private readonly AudioBuffer _fmodBuffer = new AudioBuffer(98000);

	private readonly AudioBuffer _unityBuffer = new AudioBuffer(98000);

	private readonly AudioBuffer _mixBuffer = new AudioBuffer(98000);

	private int _fmodSampleRate;

	private int _unitySampleRate;

	private bool _isCapturing;

	private readonly object _audioThreadLock = new object();

	public bool IsCapturing()
	{
		return _isCapturing;
	}

	private static void TryAppendToBuffer(float[] srcDataBuffer, int srcStartIdx, int srcDataLength, AudioBuffer destBuffer)
	{
		if (!destBuffer.TryExtendFrom(srcDataBuffer, srcStartIdx, srcDataLength))
		{
			LckLog.LogWarning("LCK Audio Capture (FMOD + Unity) losing data. Expecting this to be a lag spike.", "TryAppendToBuffer", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureFMODAndUnity.cs", 44);
		}
	}

	private static void TryAppendToBuffer(AudioBuffer srcBuffer, AudioBuffer destBuffer)
	{
		if (!destBuffer.TryExtendFrom(srcBuffer))
		{
			LckLog.LogWarning("LCK Audio Capture (FMOD + Unity) losing data. Expecting this to be a lag spike.", "TryAppendToBuffer", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureFMODAndUnity.cs", 52);
		}
	}

	private static void AppendToBufferAsStereo(float[] sourceAudioBuffer, int sourceAudioStartIdx, int sourceAudioLength, int sourceChannels, AudioBuffer destBuffer, AudioBuffer remixBuffer)
	{
		switch (sourceChannels)
		{
		case 2:
			LckLog.LogWarning("LCK Audio Capture (FMOD + Unity): Got stereo input. No remixing necessary.", "AppendToBufferAsStereo", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureFMODAndUnity.cs", 62);
			TryAppendToBuffer(sourceAudioBuffer, sourceAudioStartIdx, sourceAudioLength, destBuffer);
			break;
		case 1:
			LckLog.Log("LCK Audio Capture (FMOD + Unity): Got mono input. Remixing to stereo.", "AppendToBufferAsStereo", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureFMODAndUnity.cs", 66);
			ChannelMixingUtils.ConvertMonoToStereo(sourceAudioBuffer, sourceAudioStartIdx, sourceAudioLength, remixBuffer);
			TryAppendToBuffer(remixBuffer, destBuffer);
			break;
		case 6:
			LckLog.Log("LCK Audio Capture (FMOD + Unity): Got 5.1 input. Remixing to stereo.", "AppendToBufferAsStereo", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureFMODAndUnity.cs", 71);
			ChannelMixingUtils.ConvertFiveOneToStereo(sourceAudioBuffer, sourceAudioStartIdx, sourceAudioLength, remixBuffer);
			TryAppendToBuffer(remixBuffer, destBuffer);
			break;
		default:
			LckLog.LogError("LCK Audio Capture (FMOD + Unity): LCK only supports Mono, Stereo or 5.1 input at this time. " + $"Got: {sourceChannels} channels", "AppendToBufferAsStereo", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureFMODAndUnity.cs", 78);
			break;
		}
	}

	protected virtual void OnAudioFilterRead(float[] data, int channels)
	{
		if (!_isCapturing)
		{
			return;
		}
		lock (_audioThreadLock)
		{
			AppendToBufferAsStereo(data, 0, data.Length, channels, _unityBuffer, _tmpRemixBuffer);
		}
	}

	private void Start()
	{
		_unitySampleRate = AudioSettings.outputSampleRate;
		if (_unitySampleRate != _fmodSampleRate)
		{
			LckLog.LogError($"LCK Audio Capture (FMOD + Unity): Unity sample rate ({_unitySampleRate}) and FMOD " + $"sample rate ({_fmodSampleRate}) do not match - this is not currently supported, so " + "audio pitch may be incorrect in captures", "Start", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckAudioCaptureFMODAndUnity.cs", 192);
		}
	}

	private void OnDestroy()
	{
	}

	public void GetAudioData(ILckAudioSource.AudioDataCallbackDelegate callback)
	{
		lock (_audioThreadLock)
		{
			_mixBuffer.Clear();
			int count = _fmodBuffer.Count;
			int count2 = _unityBuffer.Count;
			int num = Math.Min(count, count2);
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					float value = _fmodBuffer[i] + _unityBuffer[i];
					_mixBuffer.TryAdd(value);
				}
			}
			callback(_mixBuffer);
			if (num > 0)
			{
				_fmodBuffer.SkipAudioSamples(num);
				_unityBuffer.SkipAudioSamples(num);
			}
			_mixBuffer.Clear();
		}
	}

	public void EnableCapture()
	{
		_isCapturing = true;
		_fmodBuffer.Clear();
		_unityBuffer.Clear();
		_mixBuffer.Clear();
	}

	public void DisableCapture()
	{
		_isCapturing = false;
		_fmodBuffer.Clear();
		_unityBuffer.Clear();
		_mixBuffer.Clear();
	}
}
