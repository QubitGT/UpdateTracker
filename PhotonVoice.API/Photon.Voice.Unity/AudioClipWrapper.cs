using System;
using UnityEngine;

namespace Photon.Voice.Unity;

public class AudioClipWrapper : IAudioReader<float>, IDataReader<float>, IDisposable, IAudioDesc
{
	private AudioClip audioClip;

	private int readPos;

	private float startTime;

	private bool playing = true;

	public bool Loop { get; set; }

	public int SamplingRate => audioClip.frequency;

	public int Channels => audioClip.channels;

	public string Error { get; private set; }

	public AudioClipWrapper(AudioClip audioClip)
	{
		this.audioClip = audioClip;
		startTime = Time.time;
	}

	public bool Read(float[] buffer)
	{
		if (!playing)
		{
			return false;
		}
		int num = (int)((Time.time - startTime) * (float)audioClip.frequency);
		int num2 = buffer.Length / audioClip.channels;
		if (num > readPos + num2)
		{
			audioClip.GetData(buffer, readPos);
			readPos += num2;
			if (readPos >= audioClip.samples)
			{
				if (Loop)
				{
					readPos = 0;
					startTime = Time.time;
				}
				else
				{
					playing = false;
				}
			}
			return true;
		}
		return false;
	}

	public void Dispose()
	{
	}
}
