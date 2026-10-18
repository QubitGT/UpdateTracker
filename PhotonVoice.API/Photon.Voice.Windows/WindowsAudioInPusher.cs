using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Photon.Voice.Windows;

public class WindowsAudioInPusher : IAudioPusher<short>, IAudioDesc, IDisposable
{
	private enum SystemMode
	{
		SINGLE_CHANNEL_AEC = 0,
		OPTIBEAM_ARRAY_ONLY = 2,
		OPTIBEAM_ARRAY_AND_AEC = 4,
		SINGLE_CHANNEL_NSAGC = 5
	}

	private delegate void CallbackDelegate(int instanceID, IntPtr buf, int len);

	private IntPtr handle;

	private int instanceID;

	private Action<short[]> pushCallback;

	private ObjectFactory<short[], int> bufferFactory;

	private static int instanceCnt;

	private static Dictionary<int, WindowsAudioInPusher> instancePerHandle = new Dictionary<int, WindowsAudioInPusher>();

	public int Channels => 1;

	public int SamplingRate => 16000;

	public string Error { get; private set; }

	[DllImport("AudioIn")]
	private static extern IntPtr Photon_Audio_In_Create(int instanceID, SystemMode systemMode, int micDevIdx, int spkDevIdx, Action<int, IntPtr, int> callback, bool featrModeOn, bool noiseSup, bool agc, bool cntrClip);

	[DllImport("AudioIn")]
	private static extern void Photon_Audio_In_Destroy(IntPtr handler);

	public WindowsAudioInPusher(int deviceID, ILogger logger)
	{
		try
		{
			lock (instancePerHandle)
			{
				handle = Photon_Audio_In_Create(instanceCnt, SystemMode.SINGLE_CHANNEL_AEC, deviceID, -1, nativePushCallback, featrModeOn: true, noiseSup: true, agc: true, cntrClip: true);
				instanceID = instanceCnt;
				instancePerHandle.Add(instanceCnt++, this);
			}
		}
		catch (Exception ex)
		{
			Error = ex.ToString();
			if (Error == null)
			{
				Error = "Exception in WindowsAudioInPusher constructor";
			}
			logger.LogError("[PV] WindowsAudioInPusher: " + Error);
		}
	}

	[MonoPInvokeCallback(typeof(CallbackDelegate))]
	private static void nativePushCallback(int instanceID, IntPtr buf, int len)
	{
		bool flag;
		WindowsAudioInPusher value;
		lock (instancePerHandle)
		{
			flag = instancePerHandle.TryGetValue(instanceID, out value);
		}
		if (flag)
		{
			value.push(buf, len);
		}
	}

	public void SetCallback(Action<short[]> callback, ObjectFactory<short[], int> bufferFactory)
	{
		this.bufferFactory = bufferFactory;
		pushCallback = callback;
	}

	private void push(IntPtr buf, int lenBytes)
	{
		if (pushCallback != null)
		{
			int num = lenBytes / 2;
			short[] array = bufferFactory.New(num);
			Marshal.Copy(buf, array, 0, num);
			pushCallback(array);
		}
	}

	public void Dispose()
	{
		lock (instancePerHandle)
		{
			instancePerHandle.Remove(instanceID);
		}
		if (handle != IntPtr.Zero)
		{
			Photon_Audio_In_Destroy(handle);
			handle = IntPtr.Zero;
		}
	}
}
