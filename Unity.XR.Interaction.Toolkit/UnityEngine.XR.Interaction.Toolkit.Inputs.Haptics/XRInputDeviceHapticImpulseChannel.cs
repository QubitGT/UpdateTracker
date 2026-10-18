namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class XRInputDeviceHapticImpulseChannel : IXRHapticImpulseChannel
{
	public int motorChannel { get; set; }

	public InputDevice device { get; set; }

	public bool SendHapticImpulse(float amplitude, float duration, float frequency)
	{
		return device.SendHapticImpulse((uint)motorChannel, amplitude, duration);
	}
}
