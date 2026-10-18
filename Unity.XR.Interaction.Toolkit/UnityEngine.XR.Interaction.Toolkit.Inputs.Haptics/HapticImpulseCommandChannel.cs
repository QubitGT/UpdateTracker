using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticImpulseCommandChannel : IXRHapticImpulseChannel
{
	public int motorChannel { get; set; }

	public UnityEngine.InputSystem.InputDevice device { get; set; }

	public bool SendHapticImpulse(float amplitude, float duration, float frequency)
	{
		if (device == null)
		{
			return false;
		}
		SendHapticImpulseCommand command = SendHapticImpulseCommand.Create(motorChannel, amplitude, duration);
		return device.ExecuteCommand(ref command) >= 0;
	}
}
