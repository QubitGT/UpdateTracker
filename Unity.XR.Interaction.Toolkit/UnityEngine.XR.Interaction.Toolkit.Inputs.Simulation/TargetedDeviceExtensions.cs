namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

internal static class TargetedDeviceExtensions
{
	public static TargetedDevices WithDevice(this TargetedDevices devices, TargetedDevices device)
	{
		return devices | device;
	}

	public static TargetedDevices WithoutDevice(this TargetedDevices devices, TargetedDevices device)
	{
		return devices & ~device;
	}

	public static bool HasDevice(this TargetedDevices devices, TargetedDevices device)
	{
		return (devices & device) == device;
	}
}
