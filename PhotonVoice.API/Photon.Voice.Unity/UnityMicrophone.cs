using UnityEngine;

namespace Photon.Voice.Unity;

public static class UnityMicrophone
{
	public static string[] devices => Microphone.devices;

	public static void End(string deviceName)
	{
		Microphone.End(deviceName);
	}

	public static void GetDeviceCaps(string deviceName, out int minFreq, out int maxFreq)
	{
		Microphone.GetDeviceCaps(deviceName, out minFreq, out maxFreq);
	}

	public static int GetPosition(string deviceName)
	{
		return Microphone.GetPosition(deviceName);
	}

	public static bool IsRecording(string deviceName)
	{
		return Microphone.IsRecording(deviceName);
	}

	public static AudioClip Start(string deviceName, bool loop, int lengthSec, int frequency)
	{
		return Microphone.Start(deviceName, loop, lengthSec, frequency);
	}
}
