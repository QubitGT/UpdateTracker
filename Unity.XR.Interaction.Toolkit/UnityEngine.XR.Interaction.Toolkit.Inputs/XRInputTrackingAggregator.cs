using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

public static class XRInputTrackingAggregator
{
	public static class Characteristics
	{
		public static InputDeviceCharacteristics hmd => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.TrackedDevice;

		public static InputDeviceCharacteristics eyeGaze => InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice;

		public static InputDeviceCharacteristics leftController => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

		public static InputDeviceCharacteristics rightController => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;

		public static InputDeviceCharacteristics leftTrackedHand => InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left;

		public static InputDeviceCharacteristics rightTrackedHand => InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right;

		internal static InputDeviceCharacteristics leftHandInteraction => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left;

		internal static InputDeviceCharacteristics rightHandInteraction => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right;

		internal static InputDeviceCharacteristics leftMicrosoftHandInteraction => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left;

		internal static InputDeviceCharacteristics rightMicrosoftHandInteraction => InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right;
	}

	private static List<InputDevice> s_XRInputDevices;

	public static TrackingStatus GetHMDStatus()
	{
		if (!Application.isPlaying)
		{
			return default(TrackingStatus);
		}
		XRHMD device = UnityEngine.InputSystem.InputSystem.GetDevice<XRHMD>();
		if (device != null)
		{
			return GetTrackingStatus(device);
		}
		if (TryGetDeviceWithExactCharacteristics(Characteristics.hmd, out var inputDevice))
		{
			return GetTrackingStatus(inputDevice);
		}
		return default(TrackingStatus);
	}

	public static TrackingStatus GetEyeGazeStatus()
	{
		if (!Application.isPlaying)
		{
			return default(TrackingStatus);
		}
		EyeGazeInteraction.EyeGazeDevice device = UnityEngine.InputSystem.InputSystem.GetDevice<EyeGazeInteraction.EyeGazeDevice>();
		if (device != null)
		{
			return GetTrackingStatus(device);
		}
		if (TryGetDeviceWithExactCharacteristics(Characteristics.eyeGaze, out var inputDevice))
		{
			return GetTrackingStatus(inputDevice);
		}
		return default(TrackingStatus);
	}

	public static TrackingStatus GetLeftControllerStatus()
	{
		if (!Application.isPlaying)
		{
			return default(TrackingStatus);
		}
		UnityEngine.InputSystem.XR.XRController device = UnityEngine.InputSystem.InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRController>(UnityEngine.InputSystem.CommonUsages.LeftHand);
		if (device != null)
		{
			return GetTrackingStatus(device);
		}
		if (TryGetDeviceWithExactCharacteristics(Characteristics.leftController, out var inputDevice))
		{
			return GetTrackingStatus(inputDevice);
		}
		return default(TrackingStatus);
	}

	public static TrackingStatus GetRightControllerStatus()
	{
		if (!Application.isPlaying)
		{
			return default(TrackingStatus);
		}
		UnityEngine.InputSystem.XR.XRController device = UnityEngine.InputSystem.InputSystem.GetDevice<UnityEngine.InputSystem.XR.XRController>(UnityEngine.InputSystem.CommonUsages.RightHand);
		if (device != null)
		{
			return GetTrackingStatus(device);
		}
		if (TryGetDeviceWithExactCharacteristics(Characteristics.rightController, out var inputDevice))
		{
			return GetTrackingStatus(inputDevice);
		}
		return default(TrackingStatus);
	}

	public static TrackingStatus GetLeftTrackedHandStatus()
	{
		if (!Application.isPlaying)
		{
			return default(TrackingStatus);
		}
		if (TryGetDeviceWithExactCharacteristics(Characteristics.leftTrackedHand, out var inputDevice))
		{
			return GetTrackingStatus(inputDevice);
		}
		return default(TrackingStatus);
	}

	public static TrackingStatus GetRightTrackedHandStatus()
	{
		if (!Application.isPlaying)
		{
			return default(TrackingStatus);
		}
		if (TryGetDeviceWithExactCharacteristics(Characteristics.rightTrackedHand, out var inputDevice))
		{
			return GetTrackingStatus(inputDevice);
		}
		return default(TrackingStatus);
	}

	public static TrackingStatus GetLeftMetaAimHandStatus()
	{
		_ = Application.isPlaying;
		return default(TrackingStatus);
	}

	public static TrackingStatus GetRightMetaAimHandStatus()
	{
		_ = Application.isPlaying;
		return default(TrackingStatus);
	}

	internal static bool TryGetDeviceWithExactCharacteristics(InputDeviceCharacteristics desiredCharacteristics, out InputDevice inputDevice)
	{
		if (s_XRInputDevices == null)
		{
			s_XRInputDevices = new List<InputDevice>();
		}
		InputDevices.GetDevices(s_XRInputDevices);
		for (int i = 0; i < s_XRInputDevices.Count; i++)
		{
			inputDevice = s_XRInputDevices[i];
			if (inputDevice.characteristics == desiredCharacteristics)
			{
				return true;
			}
		}
		inputDevice = default(InputDevice);
		return false;
	}

	private static TrackingStatus GetTrackingStatus(TrackedDevice device)
	{
		if (device == null)
		{
			return default(TrackingStatus);
		}
		TrackingStatus result = default(TrackingStatus);
		result.isConnected = device.added;
		result.isTracked = device.isTracked.isPressed;
		result.trackingState = (InputTrackingState)device.trackingState.value;
		return result;
	}

	private static TrackingStatus GetTrackingStatus(EyeGazeInteraction.EyeGazeDevice device)
	{
		if (device == null)
		{
			return default(TrackingStatus);
		}
		TrackingStatus result = default(TrackingStatus);
		result.isConnected = device.added;
		result.isTracked = device.pose.isTracked.isPressed;
		result.trackingState = (InputTrackingState)device.pose.trackingState.value;
		return result;
	}

	private static TrackingStatus GetTrackingStatus(InputDevice device)
	{
		TrackingStatus result = default(TrackingStatus);
		result.isConnected = device.isValid;
		result.isTracked = device.TryGetFeatureValue(CommonUsages.isTracked, out var value) && value;
		result.trackingState = (device.TryGetFeatureValue(CommonUsages.trackingState, out var value2) ? value2 : InputTrackingState.None);
		return result;
	}
}
