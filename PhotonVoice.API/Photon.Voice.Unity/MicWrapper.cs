using System;
using System.Linq;
using UnityEngine;

namespace Photon.Voice.Unity;

public class MicWrapper : IAudioReader<float>, IDataReader<float>, IDisposable, IAudioDesc
{
	protected AudioClip mic;

	protected string device;

	protected ILogger logger;

	protected int micPrevPos;

	protected int micLoopCnt;

	protected int readAbsPos;

	public AudioClip Mic => mic;

	public int SamplingRate
	{
		get
		{
			if (Error != null)
			{
				return 0;
			}
			return mic.frequency;
		}
	}

	public int Channels
	{
		get
		{
			if (Error != null)
			{
				return 0;
			}
			return mic.channels;
		}
	}

	public string Error { get; protected set; }

	public MicWrapper(string device, int suggestedFrequency, ILogger logger)
	{
		try
		{
			this.device = device;
			this.logger = logger;
			if (UnityMicrophone.devices.Length < 1)
			{
				Error = "No microphones found (UnityMicrophone.devices is empty)";
				logger.LogError("[PV] MicWrapper: " + Error);
				return;
			}
			if (!string.IsNullOrEmpty(device) && !UnityMicrophone.devices.Contains(device))
			{
				logger.LogError($"[PV] MicWrapper: \"{device}\" is not a valid Unity microphone device, falling back to default one");
				device = null;
			}
			UnityMicrophone.GetDeviceCaps(device, out var minFreq, out var maxFreq);
			int frequency = suggestedFrequency;
			if (suggestedFrequency < minFreq || (maxFreq != 0 && suggestedFrequency > maxFreq))
			{
				frequency = maxFreq;
			}
			mic = UnityMicrophone.Start(device, loop: true, 1, frequency);
		}
		catch (Exception ex)
		{
			Error = ex.ToString();
			if (Error == null)
			{
				Error = "Exception in MicWrapper constructor";
			}
			logger.LogError("[PV] MicWrapper: " + Error);
		}
	}

	public void Dispose()
	{
		UnityMicrophone.End(device);
		UnityEngine.Object.Destroy(mic);
		mic = null;
	}

	public virtual bool Read(float[] buffer)
	{
		if (Error != null)
		{
			return false;
		}
		int position = UnityMicrophone.GetPosition(device);
		if (position < micPrevPos)
		{
			micLoopCnt++;
		}
		micPrevPos = position;
		int num = micLoopCnt * mic.samples + position;
		if (mic.channels == 0)
		{
			Error = "Number of channels is 0 in Read()";
			logger.LogError("[PV] MicWrapper: " + Error);
			return false;
		}
		int num2 = buffer.Length / mic.channels;
		int num3 = readAbsPos + num2;
		if (num3 < num)
		{
			mic.GetData(buffer, readAbsPos % mic.samples);
			readAbsPos = num3;
			return true;
		}
		return false;
	}
}
