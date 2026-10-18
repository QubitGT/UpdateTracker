using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[StructLayout(LayoutKind.Explicit, Size = 63)]
public struct XRSimulatedControllerState : IInputStateTypeInfo
{
	[FieldOffset(0)]
	[InputControl(usage = "Primary2DAxis", aliases = new string[] { "thumbstick", "joystick" }, offset = 0u)]
	public Vector2 primary2DAxis;

	[FieldOffset(8)]
	[InputControl(usage = "Trigger", layout = "Axis", offset = 8u)]
	public float trigger;

	[FieldOffset(12)]
	[InputControl(usage = "Grip", layout = "Axis", offset = 12u)]
	public float grip;

	[FieldOffset(16)]
	[InputControl(usage = "Secondary2DAxis", offset = 16u)]
	public Vector2 secondary2DAxis;

	[FieldOffset(24)]
	[InputControl(name = "primaryButton", usage = "PrimaryButton", layout = "Button", bit = 0u, offset = 24u)]
	[InputControl(name = "primaryTouch", usage = "PrimaryTouch", layout = "Button", bit = 1u, offset = 24u)]
	[InputControl(name = "secondaryButton", usage = "SecondaryButton", layout = "Button", bit = 2u, offset = 24u)]
	[InputControl(name = "secondaryTouch", usage = "SecondaryTouch", layout = "Button", bit = 3u, offset = 24u)]
	[InputControl(name = "gripButton", usage = "GripButton", layout = "Button", bit = 4u, offset = 24u, alias = "gripPressed")]
	[InputControl(name = "triggerButton", usage = "TriggerButton", layout = "Button", bit = 5u, offset = 24u, alias = "triggerPressed")]
	[InputControl(name = "menuButton", usage = "MenuButton", layout = "Button", bit = 6u, offset = 24u)]
	[InputControl(name = "primary2DAxisClick", usage = "Primary2DAxisClick", layout = "Button", bit = 7u, offset = 24u)]
	[InputControl(name = "primary2DAxisTouch", usage = "Primary2DAxisTouch", layout = "Button", bit = 8u, offset = 24u)]
	[InputControl(name = "secondary2DAxisClick", usage = "Secondary2DAxisClick", layout = "Button", bit = 9u, offset = 24u)]
	[InputControl(name = "secondary2DAxisTouch", usage = "Secondary2DAxisTouch", layout = "Button", bit = 10u, offset = 24u)]
	[InputControl(name = "userPresence", usage = "UserPresence", layout = "Button", bit = 11u, offset = 24u)]
	public ushort buttons;

	[FieldOffset(26)]
	[InputControl(usage = "BatteryLevel", layout = "Axis", offset = 26u)]
	public float batteryLevel;

	[FieldOffset(30)]
	[InputControl(usage = "TrackingState", layout = "Integer", offset = 30u)]
	public int trackingState;

	[FieldOffset(34)]
	[InputControl(usage = "IsTracked", layout = "Button", offset = 34u)]
	public bool isTracked;

	[FieldOffset(35)]
	[InputControl(usage = "DevicePosition", offset = 35u)]
	public Vector3 devicePosition;

	[FieldOffset(47)]
	[InputControl(usage = "DeviceRotation", offset = 47u)]
	public Quaternion deviceRotation;

	public static FourCC formatId => new FourCC('X', 'R', 'S', 'C');

	public FourCC format => formatId;

	public XRSimulatedControllerState WithButton(ControllerButton button, bool state = true)
	{
		int num = 1 << (int)button;
		if (state)
		{
			buttons |= (ushort)num;
		}
		else
		{
			buttons &= (ushort)(~num);
		}
		return this;
	}

	public XRSimulatedControllerState ToggleButton(ControllerButton button)
	{
		int num = 1 << (int)button;
		buttons ^= (ushort)num;
		return this;
	}

	public bool HasButton(ControllerButton button)
	{
		int num = 1 << (int)button;
		return (buttons & num) != 0;
	}

	public void Reset()
	{
		primary2DAxis = default(Vector2);
		trigger = 0f;
		grip = 0f;
		secondary2DAxis = default(Vector2);
		buttons = 0;
		batteryLevel = 0f;
		trackingState = 0;
		isTracked = false;
		devicePosition = default(Vector3);
		deviceRotation = Quaternion.identity;
	}
}
