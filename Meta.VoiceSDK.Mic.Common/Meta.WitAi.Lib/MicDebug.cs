using System;
using System.IO;
using Meta.WitAi.Interfaces;
using UnityEngine;

namespace Meta.WitAi.Lib;

public class MicDebug : MonoBehaviour
{
	[SerializeField]
	private IAudioInputSource _micSource;

	[SerializeField]
	private string fileDirectory = "MicClips";

	[SerializeField]
	private string fileName = "mic_debug_";

	private FileStream _fileStream;

	private byte[] _buffer;

	private const int FLOAT_TO_SHORT = 32767;

	private const int BYTES_PER_SHORT = 2;

	private void OnEnable()
	{
		if (_micSource == null)
		{
			_micSource = base.gameObject.GetComponentInChildren<IAudioInputSource>();
		}
		if (_micSource != null)
		{
			_micSource.OnStartRecording += OnStartRecording;
			_micSource.OnSampleReady += OnSampleReady;
			_micSource.OnStopRecording += OnStopRecording;
		}
	}

	private void OnDisable()
	{
		if (_micSource != null)
		{
			_micSource.OnStartRecording -= OnStartRecording;
			_micSource.OnSampleReady -= OnSampleReady;
			_micSource.OnStopRecording -= OnStopRecording;
		}
	}

	private void OnDestroy()
	{
		UnloadStream();
	}

	private void OnStartRecording()
	{
		string temporaryCachePath = Application.temporaryCachePath;
		temporaryCachePath = temporaryCachePath + "/" + fileDirectory;
		if (temporaryCachePath.EndsWith("/"))
		{
			temporaryCachePath = temporaryCachePath.Substring(0, temporaryCachePath.Length - 1);
		}
		if (!Directory.Exists(temporaryCachePath))
		{
			Directory.CreateDirectory(temporaryCachePath);
		}
		DateTime now = DateTime.Now;
		temporaryCachePath = $"{temporaryCachePath}/{fileName}{now.Year:0000}{now.Month:00}{now.Day:00}_{now.Hour:00}{now.Minute:00}{now.Second:00}.pcm";
		VLog.D("MicDebug - Writing recording to file: " + temporaryCachePath);
		_fileStream = File.Open(temporaryCachePath, FileMode.Create);
	}

	private void OnSampleReady(int sampleCount, float[] sample, float levelMax)
	{
		if (_fileStream != null && sample != null)
		{
			if (_buffer == null || _buffer.Length != sample.Length * 2)
			{
				_buffer = new byte[sample.Length * 2];
			}
			for (int i = 0; i < sample.Length; i++)
			{
				short num = (short)(sample[i] * 32767f);
				_buffer[i * 2] = (byte)num;
				_buffer[i * 2 + 1] = (byte)(num >> 8);
			}
			_fileStream.Write(_buffer, 0, _buffer.Length);
		}
	}

	private void OnStopRecording()
	{
		UnloadStream();
	}

	private void UnloadStream()
	{
		if (_fileStream != null)
		{
			_fileStream.Close();
			_fileStream.Dispose();
			_fileStream = null;
		}
	}
}
