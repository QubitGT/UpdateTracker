using System;

namespace UnityEngine.XR.Interaction.Toolkit;

[Obsolete("InputHelpers has been deprecated in version 3.0.0. Use XRInputDeviceButtonReader or XRInputDeviceValueReader instead.")]
public static class InputHelpers
{
	[Obsolete("Button has been deprecated in version 3.0.0. Use XRInputDeviceButtonReader or XRInputDeviceValueReader instead.")]
	public enum Button
	{
		None = 0,
		MenuButton = 1,
		Trigger = 2,
		Grip = 3,
		TriggerButton = 4,
		GripButton = 5,
		PrimaryButton = 6,
		PrimaryTouch = 7,
		SecondaryButton = 8,
		SecondaryTouch = 9,
		Primary2DAxisTouch = 10,
		Primary2DAxisClick = 11,
		Secondary2DAxisTouch = 12,
		Secondary2DAxisClick = 13,
		PrimaryAxis2DUp = 14,
		PrimaryAxis2DDown = 15,
		PrimaryAxis2DLeft = 16,
		PrimaryAxis2DRight = 17,
		SecondaryAxis2DUp = 18,
		SecondaryAxis2DDown = 19,
		SecondaryAxis2DLeft = 20,
		SecondaryAxis2DRight = 21,
		[Obsolete("TriggerPressed has been deprecated. Use TriggerButton instead. (UnityUpgradable) -> TriggerButton", true)]
		TriggerPressed = 4,
		[Obsolete("GripPressed has been deprecated. Use GripButton instead. (UnityUpgradable) -> GripButton", true)]
		GripPressed = 5
	}

	[Obsolete("Axis2D has been deprecated in version 3.0.0. Use XRInputDeviceButtonReader or XRInputDeviceValueReader instead.")]
	public enum Axis2D
	{
		None,
		PrimaryAxis2D,
		SecondaryAxis2D
	}

	private enum ButtonReadType
	{
		None,
		Binary,
		Axis1D,
		Axis2DUp,
		Axis2DDown,
		Axis2DLeft,
		Axis2DRight
	}

	private struct ButtonInfo
	{
		public string name;

		public ButtonReadType type;

		public ButtonInfo(string name, ButtonReadType type)
		{
			this.name = name;
			this.type = type;
		}
	}

	private static readonly ButtonInfo[] s_ButtonData = new ButtonInfo[22]
	{
		new ButtonInfo("", ButtonReadType.None),
		new ButtonInfo("MenuButton", ButtonReadType.Binary),
		new ButtonInfo("Trigger", ButtonReadType.Axis1D),
		new ButtonInfo("Grip", ButtonReadType.Axis1D),
		new ButtonInfo("TriggerButton", ButtonReadType.Binary),
		new ButtonInfo("GripButton", ButtonReadType.Binary),
		new ButtonInfo("PrimaryButton", ButtonReadType.Binary),
		new ButtonInfo("PrimaryTouch", ButtonReadType.Binary),
		new ButtonInfo("SecondaryButton", ButtonReadType.Binary),
		new ButtonInfo("SecondaryTouch", ButtonReadType.Binary),
		new ButtonInfo("Primary2DAxisTouch", ButtonReadType.Binary),
		new ButtonInfo("Primary2DAxisClick", ButtonReadType.Binary),
		new ButtonInfo("Secondary2DAxisTouch", ButtonReadType.Binary),
		new ButtonInfo("Secondary2DAxisClick", ButtonReadType.Binary),
		new ButtonInfo("Primary2DAxis", ButtonReadType.Axis2DUp),
		new ButtonInfo("Primary2DAxis", ButtonReadType.Axis2DDown),
		new ButtonInfo("Primary2DAxis", ButtonReadType.Axis2DLeft),
		new ButtonInfo("Primary2DAxis", ButtonReadType.Axis2DRight),
		new ButtonInfo("Secondary2DAxis", ButtonReadType.Axis2DUp),
		new ButtonInfo("Secondary2DAxis", ButtonReadType.Axis2DDown),
		new ButtonInfo("Secondary2DAxis", ButtonReadType.Axis2DLeft),
		new ButtonInfo("Secondary2DAxis", ButtonReadType.Axis2DRight)
	};

	private static readonly string[] s_Axis2DNames = new string[3] { "", "Primary2DAxis", "Secondary2DAxis" };

	private const float k_DefaultPressThreshold = 0.1f;

	[Obsolete("IsPressed has been deprecated in version 3.0.0. Use XRInputDeviceButtonReader instead.")]
	public static bool IsPressed(this InputDevice device, Button button, out bool isPressed, float pressThreshold = -1f)
	{
		if ((int)button >= s_ButtonData.Length)
		{
			throw new ArgumentException("[InputHelpers.IsPressed] The value of <button> is out of the supported range.");
		}
		if (!device.isValid)
		{
			isPressed = false;
			return false;
		}
		ButtonInfo buttonInfo = s_ButtonData[(int)button];
		switch (buttonInfo.type)
		{
		case ButtonReadType.Binary:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<bool>(buttonInfo.name), out var value5))
			{
				isPressed = value5;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis1D:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<float>(buttonInfo.name), out var value3))
			{
				float num3 = ((pressThreshold >= 0f) ? pressThreshold : 0.1f);
				isPressed = value3 >= num3;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis2DUp:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonInfo.name), out var value4))
			{
				float num4 = ((pressThreshold >= 0f) ? pressThreshold : 0.1f);
				isPressed = value4.y >= num4;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis2DDown:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonInfo.name), out var value2))
			{
				float num2 = ((pressThreshold >= 0f) ? pressThreshold : 0.1f);
				isPressed = value2.y <= 0f - num2;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis2DLeft:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonInfo.name), out var value6))
			{
				float num5 = ((pressThreshold >= 0f) ? pressThreshold : 0.1f);
				isPressed = value6.x <= 0f - num5;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis2DRight:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonInfo.name), out var value))
			{
				float num = ((pressThreshold >= 0f) ? pressThreshold : 0.1f);
				isPressed = value.x >= num;
				return true;
			}
			break;
		}
		}
		isPressed = false;
		return false;
	}

	[Obsolete("TryReadSingleValue has been deprecated in version 3.0.0. Use XRInputDeviceValueReader instead.")]
	public static bool TryReadSingleValue(this InputDevice device, Button button, out float singleValue)
	{
		if ((int)button >= s_ButtonData.Length)
		{
			throw new ArgumentException("[InputHelpers.TryReadSingleValue] The value of <button> is out of the supported range.");
		}
		if (!device.isValid)
		{
			singleValue = 0f;
			return false;
		}
		ButtonInfo buttonInfo = s_ButtonData[(int)button];
		switch (buttonInfo.type)
		{
		case ButtonReadType.Binary:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<bool>(buttonInfo.name), out var value4))
			{
				singleValue = (value4 ? 1f : 0f);
				return true;
			}
			break;
		}
		case ButtonReadType.Axis1D:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<float>(buttonInfo.name), out var value6))
			{
				singleValue = value6;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis2DUp:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonInfo.name), out var value2))
			{
				singleValue = value2.y;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis2DDown:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonInfo.name), out var value5))
			{
				singleValue = 0f - value5.y;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis2DLeft:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonInfo.name), out var value3))
			{
				singleValue = 0f - value3.x;
				return true;
			}
			break;
		}
		case ButtonReadType.Axis2DRight:
		{
			if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(buttonInfo.name), out var value))
			{
				singleValue = value.x;
				return true;
			}
			break;
		}
		}
		singleValue = 0f;
		return false;
	}

	[Obsolete("TryReadAxis2DValue has been deprecated in version 3.0.0. Use XRInputDeviceValueReader instead.")]
	public static bool TryReadAxis2DValue(this InputDevice device, Axis2D axis2D, out Vector2 value)
	{
		if ((int)axis2D >= s_Axis2DNames.Length)
		{
			throw new ArgumentException("[InputHelpers.TryReadAxis2DValue] The value of <axis2D> is out of the supported range.");
		}
		if (!device.isValid)
		{
			value = default(Vector2);
			return false;
		}
		string usageName = s_Axis2DNames[(int)axis2D];
		if (device.TryGetFeatureValue(new InputFeatureUsage<Vector2>(usageName), out value))
		{
			return true;
		}
		value = default(Vector2);
		return false;
	}
}
