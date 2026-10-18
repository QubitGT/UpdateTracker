using UnityEngine.InputSystem;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.OpenXR;

public class OpenXRHapticImpulseChannel : IXRHapticImpulseChannel
{
	public InputAction hapticAction { get; set; }

	public UnityEngine.InputSystem.InputDevice device { get; set; }

	public bool SendHapticImpulse(float amplitude, float duration, float frequency)
	{
		if (OpenXRInput.GetActionHandle(hapticAction) == 0L)
		{
			return false;
		}
		OpenXRInput.SendHapticImpulse(hapticAction, amplitude, frequency, duration, device);
		return true;
	}
}
