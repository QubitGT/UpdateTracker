using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Input;

[Preserve]
[InputControlLayout(displayName = "OpenXR Action Map")]
public abstract class OpenXRDevice : UnityEngine.InputSystem.InputDevice
{
	protected override void FinishSetup()
	{
		base.FinishSetup();
		XRDeviceDescriptor xRDeviceDescriptor = XRDeviceDescriptor.FromJson(base.description.capabilities);
		if (xRDeviceDescriptor != null)
		{
			if ((xRDeviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
			{
				UnityEngine.InputSystem.InputSystem.SetDeviceUsage(this, UnityEngine.InputSystem.CommonUsages.LeftHand);
			}
			else if ((xRDeviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
			{
				UnityEngine.InputSystem.InputSystem.SetDeviceUsage(this, UnityEngine.InputSystem.CommonUsages.RightHand);
			}
		}
	}
}
