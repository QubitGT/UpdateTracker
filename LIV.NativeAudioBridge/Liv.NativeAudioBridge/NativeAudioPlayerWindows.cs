using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Liv.NativeAudioBridge;

public class NativeAudioPlayerWindows : INativeAudioPlayer, IDisposable
{
	private delegate void WaveOutProc(IntPtr hwo, int uMsg, int dwInstance, int dwParam1, int dwParam2);

	private struct PreloadedAudio
	{
		public GCHandle DataHandle;

		public WaveFormat Format;

		public int BufferLength;
	}

	private struct WaveFormat
	{
		public short wFormatTag;

		public short nChannels;

		public int nSamplesPerSec;

		public int nAvgBytesPerSec;

		public short nBlockAlign;

		public short wBitsPerSample;

		public short cbSize;
	}

	private struct WaveHdr
	{
		public IntPtr lpData;

		public int dwBufferLength;

		public int dwBytesRecorded;

		public IntPtr dwUser;

		public int dwFlags;

		public int dwLoops;

		public IntPtr lpNext;

		public int reserved;
	}

	private static byte[] _audioByteDataArray;

	private const int BitsPerSample = 16;

	private const string Lib = "winmm.dll";

	private bool _disposed;

	private Dictionary<int, PreloadedAudio> _audioClips = new Dictionary<int, PreloadedAudio>();

	[DllImport("winmm.dll")]
	private static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, WaveFormat lpFormat, WaveOutProc dwCallback, int dwInstance, int dwFlags);

	[DllImport("winmm.dll")]
	private static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

	[DllImport("winmm.dll")]
	private static extern int waveOutWrite(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

	[DllImport("winmm.dll")]
	private static extern int waveOutUnprepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);

	[DllImport("winmm.dll")]
	private static extern int waveOutClose(IntPtr hWaveOut);

	public void PreloadAudioClip(AudioClip audioClip, float volume, bool forceReload = false)
	{
		ValidateAudioClipForPreloading(audioClip);
		PreloadAudioClip(audioClip.GetHashCode(), PrepareAudioData(audioClip, volume), audioClip.frequency, audioClip.channels, 16, forceReload);
	}

	public void PlayAudioClip(AudioClip audioClip, float volume = 1f)
	{
		if (!audioClip)
		{
			throw new InvalidOperationException("LCK: Native Audio can not play AudioClip, audio clip is null.");
		}
		int audioClipId = audioClip.GetHashCode();
		if (!_audioClips.ContainsKey(audioClipId))
		{
			PreloadAudioClip(audioClipId, PrepareAudioData(audioClip, volume), audioClip.frequency, audioClip.channels, 16, forceReload: false);
		}
		Task.Run(async delegate
		{
			await PlayAudio(audioClipId);
		});
	}

	public void StopAllAudio()
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		foreach (KeyValuePair<int, PreloadedAudio> audioClip in _audioClips)
		{
			if (audioClip.Value.DataHandle.IsAllocated)
			{
				audioClip.Value.DataHandle.Free();
			}
		}
		_audioClips.Clear();
		_disposed = true;
	}

	~NativeAudioPlayerWindows()
	{
		Dispose(disposing: false);
	}

	private void ValidateAudioClipForPreloading(AudioClip audioClip)
	{
		if (!audioClip)
		{
			throw new InvalidOperationException("Native Audio can not preload AudioClip, audio clip is null.");
		}
	}

	private void PreloadAudioClip(int key, byte[] audioData, int sampleRate, int channels, int bitsPerSample, bool forceReload)
	{
		if (forceReload || !_audioClips.ContainsKey(key))
		{
			if (forceReload && _audioClips.ContainsKey(key))
			{
				UnloadAudioClip(key);
			}
			WaveFormat waveFormat = default(WaveFormat);
			waveFormat.wFormatTag = 1;
			waveFormat.nChannels = (short)channels;
			waveFormat.nSamplesPerSec = sampleRate;
			waveFormat.wBitsPerSample = (short)bitsPerSample;
			waveFormat.nBlockAlign = (short)(channels * bitsPerSample / 8);
			waveFormat.nAvgBytesPerSec = sampleRate * channels * bitsPerSample / 8;
			waveFormat.cbSize = 0;
			WaveFormat format = waveFormat;
			GCHandle dataHandle = GCHandle.Alloc(audioData, GCHandleType.Pinned);
			_audioClips[key] = new PreloadedAudio
			{
				DataHandle = dataHandle,
				BufferLength = audioData.Length,
				Format = format
			};
		}
	}

	private void UnloadAudioClip(int audioClipKey)
	{
		int hashCode = audioClipKey.GetHashCode();
		if (!_audioClips.ContainsKey(hashCode))
		{
			throw new InvalidOperationException($"LCK: Native Audio cannot unload AudioClip ({audioClipKey}), it is not preloaded.");
		}
		PreloadedAudio preloadedAudio = _audioClips[hashCode];
		if (preloadedAudio.DataHandle.IsAllocated)
		{
			preloadedAudio.DataHandle.Free();
		}
		_audioClips.Remove(hashCode);
	}

	private static byte[] PrepareAudioData(AudioClip clip, float volume)
	{
		return ConvertAudioClipToByteArray(clip, volume);
	}

	private static byte[] ConvertAudioClipToByteArray(AudioClip clip, float volume)
	{
		float[] array = new float[clip.samples * clip.channels];
		clip.GetData(array, 0);
		byte[] array2 = new byte[array.Length * 2];
		int num = 32767;
		for (int i = 0; i < array.Length; i++)
		{
			short num2 = (short)(Mathf.Clamp(array[i] * volume, -1f, 1f) * (float)num);
			array2[i * 2] = (byte)((uint)num2 & 0xFFu);
			array2[i * 2 + 1] = (byte)((num2 & 0xFF00) >> 8);
		}
		return array2;
	}

	private Task PlayAudio(int audioClipId)
	{
		PreloadedAudio preloadedAudio = _audioClips[audioClipId];
		if (waveOutOpen(out var hWaveOut, -1, preloadedAudio.Format, null, 0, 0) != 0)
		{
			throw new InvalidOperationException("Failed to open waveform audio device.");
		}
		WaveHdr waveHdr = default(WaveHdr);
		waveHdr.lpData = preloadedAudio.DataHandle.AddrOfPinnedObject();
		waveHdr.dwBufferLength = preloadedAudio.BufferLength;
		waveHdr.dwFlags = 0;
		waveHdr.dwLoops = 0;
		waveHdr.dwUser = GCHandle.ToIntPtr(preloadedAudio.DataHandle);
		WaveHdr lpWaveOutHdr = waveHdr;
		waveOutPrepareHeader(hWaveOut, ref lpWaveOutHdr, Marshal.SizeOf(lpWaveOutHdr));
		waveOutWrite(hWaveOut, ref lpWaveOutHdr, Marshal.SizeOf(lpWaveOutHdr));
		while ((lpWaveOutHdr.dwFlags & 1) != 1)
		{
			Thread.Sleep(100);
		}
		waveOutUnprepareHeader(hWaveOut, ref lpWaveOutHdr, Marshal.SizeOf(lpWaveOutHdr));
		waveOutClose(hWaveOut);
		return Task.CompletedTask;
	}
}
