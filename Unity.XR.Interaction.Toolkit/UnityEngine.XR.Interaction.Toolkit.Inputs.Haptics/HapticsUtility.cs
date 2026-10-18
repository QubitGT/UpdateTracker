using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public static class HapticsUtility
{
	public enum Controller
	{
		Left,
		Right,
		Both
	}

	private static HapticImpulseCommandChannelGroup s_LeftChannelGroup;

	private static HapticImpulseCommandChannelGroup s_RightChannelGroup;

	private static XRInputDeviceHapticImpulseChannelGroup s_LegacyLeftChannelGroup;

	private static XRInputDeviceHapticImpulseChannelGroup s_LegacyRightChannelGroup;

	private static InputDevice s_LegacyLeftDevice;

	private static InputDevice s_LegacyRightDevice;

	private static HapticControlActionManager s_HapticControlManager;

	private static InputAction s_LeftHapticAction;

	private static InputAction s_RightHapticAction;

	public static bool SendHapticImpulse(float amplitude, float duration, Controller controller, float frequency = 0f, int channel = 0)
	{
		bool flag = false;
		bool flag2 = false;
		if (controller == Controller.Left || controller == Controller.Both)
		{
			UnityEngine.InputSystem.XR.XRController leftHand = UnityEngine.InputSystem.XR.XRController.leftHand;
			if (leftHand != null)
			{
				if (frequency > 0f && channel == 0)
				{
					flag = SendHapticImpulseOpenXR(GetLeftHapticAction(), amplitude, duration, frequency);
				}
				if (!flag)
				{
					flag = SendHapticImpulse(ref s_LeftChannelGroup, leftHand, channel, amplitude, duration, frequency);
				}
			}
			else
			{
				flag = SendHapticImpulseLegacy(ref s_LegacyLeftChannelGroup, ref s_LegacyLeftDevice, XRInputTrackingAggregator.Characteristics.leftController, channel, amplitude, duration, frequency);
			}
		}
		if (controller == Controller.Right || controller == Controller.Both)
		{
			UnityEngine.InputSystem.XR.XRController rightHand = UnityEngine.InputSystem.XR.XRController.rightHand;
			if (rightHand != null)
			{
				if (frequency > 0f && channel == 0)
				{
					flag2 = SendHapticImpulseOpenXR(GetRightHapticAction(), amplitude, duration, frequency);
				}
				if (!flag2)
				{
					flag2 = SendHapticImpulse(ref s_RightChannelGroup, rightHand, channel, amplitude, duration, frequency);
				}
			}
			else
			{
				flag2 = SendHapticImpulseLegacy(ref s_LegacyRightChannelGroup, ref s_LegacyRightDevice, XRInputTrackingAggregator.Characteristics.rightController, channel, amplitude, duration, frequency);
			}
		}
		return controller switch
		{
			Controller.Both => flag && flag2, 
			Controller.Left => flag, 
			Controller.Right => flag2, 
			_ => false, 
		};
	}

	private static bool SendHapticImpulseOpenXR(InputAction hapticAction, float amplitude, float duration, float frequency)
	{
		if (s_HapticControlManager == null)
		{
			s_HapticControlManager = new HapticControlActionManager();
		}
		return s_HapticControlManager.GetChannelGroup(hapticAction)?.GetChannel().SendHapticImpulse(amplitude, duration, frequency) ?? false;
	}

	private static bool SendHapticImpulse(ref HapticImpulseCommandChannelGroup channelGroup, UnityEngine.InputSystem.InputDevice device, int channel, float amplitude, float duration, float frequency)
	{
		if (channelGroup == null)
		{
			channelGroup = new HapticImpulseCommandChannelGroup();
		}
		channelGroup.Initialize(device);
		return channelGroup.GetChannel(channel)?.SendHapticImpulse(amplitude, duration, frequency) ?? false;
	}

	private static bool SendHapticImpulseLegacy(ref XRInputDeviceHapticImpulseChannelGroup channelGroup, ref InputDevice device, InputDeviceCharacteristics characteristics, int channel, float amplitude, float duration, float frequency)
	{
		if (channelGroup == null)
		{
			channelGroup = new XRInputDeviceHapticImpulseChannelGroup();
		}
		if (device.isValid || XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(characteristics, out device))
		{
			channelGroup.Initialize(device);
			return channelGroup.GetChannel(channel)?.SendHapticImpulse(amplitude, duration, frequency) ?? false;
		}
		return false;
	}

	private static InputAction GetLeftHapticAction()
	{
		if (s_LeftHapticAction == null)
		{
			s_LeftHapticAction = new InputAction("Left Haptic", InputActionType.PassThrough, "<XRController>{LeftHand}/{Haptic}");
			s_LeftHapticAction.Enable();
		}
		return s_LeftHapticAction;
	}

	private static InputAction GetRightHapticAction()
	{
		if (s_RightHapticAction == null)
		{
			s_RightHapticAction = new InputAction("Right Haptic", InputActionType.PassThrough, "<XRController>{RightHand}/{Haptic}");
			s_RightHapticAction.Enable();
		}
		return s_RightHapticAction;
	}
}
