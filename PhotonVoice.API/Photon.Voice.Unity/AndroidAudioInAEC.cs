using System;
using UnityEngine;

namespace Photon.Voice.Unity;

public class AndroidAudioInAEC : IAudioPusher<short>, IAudioDesc, IDisposable, IResettable
{
	private class DataCallback : AndroidJavaProxy
	{
		private Action<short[]> callback;

		private IntPtr javaBuf;

		private int cntFrame;

		private int cntShort;

		public DataCallback()
			: base("com.exitgames.photon.audioinaec.AudioInAEC$DataCallback")
		{
		}

		public void SetCallback(Action<short[]> callback, IntPtr javaBuf)
		{
			this.callback = callback;
			this.javaBuf = javaBuf;
		}

		public void OnData()
		{
			if (callback != null)
			{
				short[] array = AndroidJNI.FromShortArray(javaBuf);
				cntFrame++;
				cntShort += array.Length;
				callback(array);
			}
		}

		public void OnStop()
		{
			AndroidJNI.DeleteGlobalRef(javaBuf);
		}
	}

	private AndroidJavaObject audioIn;

	private IntPtr javaBuf;

	private ILogger logger;

	private int audioInSampleRate;

	private DataCallback callback;

	public int Channels => 1;

	public int SamplingRate => audioInSampleRate;

	public string Error { get; private set; }

	public AndroidAudioInAEC(ILogger logger, bool enableAEC = false, bool enableAGC = false, bool enableNS = false)
	{
		this.logger = logger;
		try
		{
			callback = new DataCallback();
			audioIn = new AndroidJavaObject("com.exitgames.photon.audioinaec.AudioInAEC");
			int num = audioIn.Call<int>("GetMinBufferSize", new object[2] { 44100, Channels });
			logger.LogInfo("[PV] AndroidAudioInAEC: AndroidJavaObject created: aec: {0}/{1}, agc: {2}/{3}, ns: {4}/{5} minBufSize: {6}", enableAEC, audioIn.Call<bool>("AECIsAvailable", Array.Empty<object>()), enableAGC, audioIn.Call<bool>("AGCIsAvailable", Array.Empty<object>()), enableNS, audioIn.Call<bool>("NSIsAvailable", Array.Empty<object>()), num);
			AndroidJavaObject @static = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
			bool flag = audioIn.Call<bool>("Start", new object[8]
			{
				@static,
				callback,
				44100,
				Channels,
				num * 4,
				enableAEC,
				enableAGC,
				enableNS
			});
			if (flag)
			{
				audioInSampleRate = audioIn.Call<int>("GetSampleRate", Array.Empty<object>());
				logger.LogInfo("[PV] AndroidAudioInAEC: AndroidJavaObject started: {0}, sampling rate: {1}, channels: {2}, record buffer size: {3}", flag, SamplingRate, Channels, num * 4);
			}
			else
			{
				Error = "[PV] AndroidAudioInAEC constructor: calling Start java method failure";
				logger.LogError("[PV] AndroidAudioInAEC: {0}", Error);
			}
		}
		catch (Exception ex)
		{
			Error = ex.ToString();
			if (Error == null)
			{
				Error = "Exception in AndroidAudioInAEC constructor";
			}
			logger.LogError("[PV] AndroidAudioInAEC: {0}", Error);
		}
	}

	public void SetCallback(Action<short[]> callback, ObjectFactory<short[], int> bufferFactory)
	{
		if (Error == null)
		{
			int info = bufferFactory.Info;
			javaBuf = AndroidJNI.NewGlobalRef(AndroidJNI.NewShortArray(info));
			this.callback.SetCallback(callback, javaBuf);
			IntPtr methodID = AndroidJNI.GetMethodID(audioIn.GetRawClass(), "SetBuffer", "([S)Z");
			if (!AndroidJNI.CallBooleanMethod(audioIn.GetRawObject(), methodID, new jvalue[1]
			{
				new jvalue
				{
					l = javaBuf
				}
			}))
			{
				Error = "AndroidAudioInAEC.SetCallback(): calling SetBuffer java method failure";
			}
		}
		if (Error != null)
		{
			logger.LogError("[PV] AndroidAudioInAEC: {0}", Error);
		}
	}

	public void Reset()
	{
		if (audioIn != null)
		{
			audioIn.Call("Reset");
		}
	}

	public void Dispose()
	{
		if (audioIn != null)
		{
			audioIn.Call<bool>("Stop", Array.Empty<object>());
		}
	}
}
