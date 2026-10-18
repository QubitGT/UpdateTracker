using UnityEngine.InputSystem.Layouts;

namespace UnityEngine.InputSystem.Switch;

internal static class SwitchSupportHID
{
	public static void Initialize()
	{
		InputSystem.RegisterLayout<SwitchProControllerHID>(null, default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 1406).WithCapability("productId", 8201));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3853).WithCapability("productId", 146));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3853).WithCapability("productId", 170));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3853).WithCapability("productId", 193));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3853).WithCapability("productId", 220));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3853).WithCapability("productId", 246));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3695).WithCapability("productId", 384));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3695).WithCapability("productId", 385));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3695).WithCapability("productId", 389));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3695).WithCapability("productId", 390));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3695).WithCapability("productId", 391));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 8406).WithCapability("productId", 42770));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 8406).WithCapability("productId", 42774));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3695).WithCapability("productId", 388));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 3695).WithCapability("productId", 392));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 8406).WithCapability("productId", 42772));
		InputSystem.RegisterLayoutMatcher<SwitchProControllerHID>(default(InputDeviceMatcher).WithInterface("HID").WithCapability("vendorId", 8406).WithCapability("productId", 42773));
	}
}
