using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Liv.Lck.Collections;
using Liv.Lck.Settings;
using UnityEngine;

namespace Liv.Lck.NativeMicrophone;

public class LckNativeMicrophone : IDisposable, ILckAudioSource
{
	public enum ReturnCode : uint
	{
		Ok,
		Error,
		InvalidKey,
		DefaultInputDeviceError,
		BuildStreamError,
		NoAudioData,
		LoggerAlreadySet,
		CaptureNotStarted
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void AudioDataCallbackDelegate(IntPtr dataPtr, int length, ulong audioCaptureKey);

	private static Dictionary<ulong, LckNativeMicrophone> _instances = new Dictionary<ulong, LckNativeMicrophone>();

	private const string __DllName = "native_microphone";

	private ulong _nativeInstance;

	private AudioDataCallbackDelegate _callback;

	private AudioBuffer _audioBuffer = new AudioBuffer(96000);

	private IntPtr _callbackPtr;

	private bool _isCapturing;

	private bool _shouldDisableCapture;

	private bool _shouldEnableCapture;

	private Task _setMicStateTask;

	[DllImport("native_microphone", CallingConvention = CallingConvention.Cdecl)]
	private static extern ulong microphone_capture_new(uint sampleRate);

	[DllImport("native_microphone", CallingConvention = CallingConvention.Cdecl)]
	private static extern ReturnCode microphone_capture_free(ulong audioCaptureKey);

	[DllImport("native_microphone", CallingConvention = CallingConvention.Cdecl)]
	private static extern ReturnCode microphone_capture_start(ulong audioCaptureKey);

	[DllImport("native_microphone", CallingConvention = CallingConvention.Cdecl)]
	private static extern ReturnCode microphone_capture_stop(ulong audioCaptureKey);

	[DllImport("native_microphone", CallingConvention = CallingConvention.Cdecl)]
	private static extern ReturnCode microphone_capture_get_audio(ulong audioCaptureKey, IntPtr callback);

	[DllImport("native_microphone", CallingConvention = CallingConvention.Cdecl)]
	private static extern void set_max_log_level(LogLevel levelFilter);

	public LckNativeMicrophone(int sampleRate)
	{
		SetMaxLogLevel(LckSettings.Instance.MicrophoneLogLevel);
		_callback = AudioDataCallback;
		_callbackPtr = Marshal.GetFunctionPointerForDelegate(_callback);
		_nativeInstance = microphone_capture_new((uint)sampleRate);
		_instances.Add(_nativeInstance, this);
	}

	[MonoPInvokeCallback(typeof(AudioDataCallbackDelegate))]
	private static void AudioDataCallback(IntPtr dataPtr, int length, ulong audioCaptureKey)
	{
		if (_instances.TryGetValue(audioCaptureKey, out var value))
		{
			try
			{
				if (value._audioBuffer.Capacity < length)
				{
					LckLog.LogWarning($"LCK Native Microphone dropping audio: {value._audioBuffer.Capacity} < {length}", "AudioDataCallback", ".\\Packages\\tv.liv.lck\\Runtime\\Plugins\\NativeMicrophone\\NativeMicrophone.cs", 103);
				}
				int count = Mathf.Min(length, value._audioBuffer.Capacity);
				if (!value._audioBuffer.TryCopyFrom(dataPtr, count))
				{
					LckLog.LogError("LCK Mic Audio data copy failed", "AudioDataCallback", ".\\Packages\\tv.liv.lck\\Runtime\\Plugins\\NativeMicrophone\\NativeMicrophone.cs", 108);
				}
				return;
			}
			catch (Exception ex)
			{
				Debug.LogError("LCK Exception during mic audio copy: " + ex.Message);
				return;
			}
		}
		LckLog.LogError("LCK NativeMicrophone: Could not find instance for key: " + audioCaptureKey, "AudioDataCallback", ".\\Packages\\tv.liv.lck\\Runtime\\Plugins\\NativeMicrophone\\NativeMicrophone.cs", 119);
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void InitMicrophone()
	{
	}

	public void Dispose()
	{
		if (_nativeInstance != 0L)
		{
			microphone_capture_free(_nativeInstance);
			_instances.Remove(_nativeInstance);
			_nativeInstance = 0uL;
		}
	}

	public bool IsCapturing()
	{
		return _isCapturing;
	}

	public void GetAudioData(ILckAudioSource.AudioDataCallbackDelegate callback)
	{
		_audioBuffer.Clear();
		if (_isCapturing)
		{
			microphone_capture_get_audio(_nativeInstance, _callbackPtr);
		}
		callback(_audioBuffer);
	}

	public void EnableCapture()
	{
		_shouldEnableCapture = true;
		_shouldDisableCapture = false;
		if (_setMicStateTask == null)
		{
			_setMicStateTask = Task.Run(() => SetMicrophoneCaptureActive(active: true));
		}
	}

	public void DisableCapture()
	{
		_shouldDisableCapture = true;
		_shouldEnableCapture = false;
		if (_setMicStateTask == null)
		{
			_setMicStateTask = Task.Run(() => SetMicrophoneCaptureActive(active: false));
		}
	}

	private async Task SetMicrophoneCaptureActive(bool active)
	{
		if (_isCapturing == active)
		{
			_setMicStateTask = null;
			return;
		}
		if (active)
		{
			_isCapturing = microphone_capture_start(_nativeInstance) == ReturnCode.Ok;
			_shouldEnableCapture = false;
			_setMicStateTask = null;
			if (_shouldDisableCapture)
			{
				await SetMicrophoneCaptureActive(active: false);
			}
			return;
		}
		_isCapturing = false;
		microphone_capture_stop(_nativeInstance);
		_shouldDisableCapture = false;
		_setMicStateTask = null;
		if (_shouldEnableCapture)
		{
			await SetMicrophoneCaptureActive(active: true);
		}
	}

	public static void SetMaxLogLevel(LogLevel logLevel)
	{
		set_max_log_level(logLevel);
	}
}
