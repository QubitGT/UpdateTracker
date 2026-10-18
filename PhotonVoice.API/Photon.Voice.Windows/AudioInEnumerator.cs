using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Photon.Voice.Windows;

public class AudioInEnumerator : DeviceEnumeratorBase
{
	private const string lib_name = "AudioIn";

	private IntPtr handle;

	[DllImport("AudioIn")]
	private static extern IntPtr Photon_Audio_In_CreateMicEnumerator();

	[DllImport("AudioIn")]
	private static extern void Photon_Audio_In_DestroyMicEnumerator(IntPtr handle);

	[DllImport("AudioIn")]
	private static extern int Photon_Audio_In_MicEnumerator_Count(IntPtr handle);

	[DllImport("AudioIn")]
	private static extern IntPtr Photon_Audio_In_MicEnumerator_NameAtIndex(IntPtr handle, int idx);

	[DllImport("AudioIn")]
	private static extern int Photon_Audio_In_MicEnumerator_IDAtIndex(IntPtr handle, int idx);

	public AudioInEnumerator(ILogger logger)
		: base(logger)
	{
		Refresh();
	}

	public override void Refresh()
	{
		Dispose();
		try
		{
			handle = Photon_Audio_In_CreateMicEnumerator();
			int num = Photon_Audio_In_MicEnumerator_Count(handle);
			devices = new List<DeviceInfo>();
			for (int i = 0; i < num; i++)
			{
				devices.Add(new DeviceInfo(Photon_Audio_In_MicEnumerator_IDAtIndex(handle, i), Marshal.PtrToStringAuto(Photon_Audio_In_MicEnumerator_NameAtIndex(handle, i))));
			}
			Error = null;
		}
		catch (Exception ex)
		{
			Error = ex.ToString();
			if (Error == null)
			{
				Error = "Exception in AudioInEnumerator.Refresh()";
			}
		}
	}

	public override void Dispose()
	{
		if (handle != IntPtr.Zero && Error == null)
		{
			Photon_Audio_In_DestroyMicEnumerator(handle);
			handle = IntPtr.Zero;
		}
	}
}
