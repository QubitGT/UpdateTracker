namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

internal static class TargetedDevicesExtensions
{
	public static XRDeviceSimulator.TargetedDevices WithDevice(this XRDeviceSimulator.TargetedDevices devices, XRDeviceSimulator.TargetedDevices device)
	{
		return devices | device;
	}

	public static XRDeviceSimulator.TargetedDevices WithoutDevice(this XRDeviceSimulator.TargetedDevices devices, XRDeviceSimulator.TargetedDevices device)
	{
		return devices & ~device;
	}

	public static bool HasDevice(this XRDeviceSimulator.TargetedDevices devices, XRDeviceSimulator.TargetedDevices device)
	{
		return (devices & device) == device;
	}
}
