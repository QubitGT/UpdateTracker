using UnityEngine.InputSystem.Layouts;
using UnityEngine.XR;

namespace UnityEngine.InputSystem.XR;

[InputControlLayout(commonUsages = new string[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = true, displayName = "XR Controller")]
public class XRController : TrackedDevice
{
	public static XRController leftHand => InputSystem.GetDevice<XRController>(CommonUsages.LeftHand);

	public static XRController rightHand => InputSystem.GetDevice<XRController>(CommonUsages.RightHand);

	protected override void FinishSetup()
	{
		base.FinishSetup();
		XRDeviceDescriptor xRDeviceDescriptor = XRDeviceDescriptor.FromJson(base.description.capabilities);
		if (xRDeviceDescriptor != null)
		{
			if ((xRDeviceDescriptor.characteristics & InputDeviceCharacteristics.Left) != 0)
			{
				InputSystem.SetDeviceUsage(this, CommonUsages.LeftHand);
			}
			else if ((xRDeviceDescriptor.characteristics & InputDeviceCharacteristics.Right) != 0)
			{
				InputSystem.SetDeviceUsage(this, CommonUsages.RightHand);
			}
		}
	}
}
