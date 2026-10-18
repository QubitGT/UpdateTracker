using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using OVR.OpenVR;
using UnityEngine;
using UnityEngine.XR;

[HelpURL("https://developer.oculus.com/documentation/unity/unity-ovrinput/")]
public static class OVRInput
{
	[Flags]
	public enum Button
	{
		None = 0,
		One = 1,
		Two = 2,
		Three = 4,
		Four = 8,
		Start = 0x100,
		Back = 0x200,
		PrimaryShoulder = 0x1000,
		PrimaryIndexTrigger = 0x2000,
		PrimaryHandTrigger = 0x4000,
		PrimaryThumbstick = 0x8000,
		PrimaryThumbstickUp = 0x10000,
		PrimaryThumbstickDown = 0x20000,
		PrimaryThumbstickLeft = 0x40000,
		PrimaryThumbstickRight = 0x80000,
		PrimaryTouchpad = 0x400,
		SecondaryShoulder = 0x100000,
		SecondaryIndexTrigger = 0x200000,
		SecondaryHandTrigger = 0x400000,
		SecondaryThumbstick = 0x800000,
		SecondaryThumbstickUp = 0x1000000,
		SecondaryThumbstickDown = 0x2000000,
		SecondaryThumbstickLeft = 0x4000000,
		SecondaryThumbstickRight = 0x8000000,
		SecondaryTouchpad = 0x800,
		DpadUp = 0x10,
		DpadDown = 0x20,
		DpadLeft = 0x40,
		DpadRight = 0x80,
		Up = 0x10000000,
		Down = 0x20000000,
		Left = 0x40000000,
		Right = int.MinValue,
		Any = -1
	}

	[Flags]
	public enum RawButton
	{
		None = 0,
		A = 1,
		B = 2,
		X = 0x100,
		Y = 0x200,
		Start = 0x100000,
		Back = 0x200000,
		LShoulder = 0x800,
		LIndexTrigger = 0x10000000,
		LHandTrigger = 0x20000000,
		LThumbstick = 0x400,
		LThumbstickUp = 0x10,
		LThumbstickDown = 0x20,
		LThumbstickLeft = 0x40,
		LThumbstickRight = 0x80,
		LTouchpad = 0x40000000,
		RShoulder = 8,
		RIndexTrigger = 0x4000000,
		RHandTrigger = 0x8000000,
		RThumbstick = 4,
		RThumbstickUp = 0x1000,
		RThumbstickDown = 0x2000,
		RThumbstickLeft = 0x4000,
		RThumbstickRight = 0x8000,
		RTouchpad = int.MinValue,
		DpadUp = 0x10000,
		DpadDown = 0x20000,
		DpadLeft = 0x40000,
		DpadRight = 0x80000,
		Any = -1
	}

	[Flags]
	public enum Touch
	{
		None = 0,
		One = 1,
		Two = 2,
		Three = 4,
		Four = 8,
		PrimaryIndexTrigger = 0x2000,
		PrimaryThumbstick = 0x8000,
		PrimaryThumbRest = 0x1000,
		PrimaryTouchpad = 0x400,
		SecondaryIndexTrigger = 0x200000,
		SecondaryThumbstick = 0x800000,
		SecondaryThumbRest = 0x100000,
		SecondaryTouchpad = 0x800,
		Any = -1
	}

	[Flags]
	public enum RawTouch
	{
		None = 0,
		A = 1,
		B = 2,
		X = 0x100,
		Y = 0x200,
		LIndexTrigger = 0x1000,
		LThumbstick = 0x400,
		LThumbRest = 0x800,
		LTouchpad = 0x40000000,
		RIndexTrigger = 0x10,
		RThumbstick = 4,
		RThumbRest = 8,
		RTouchpad = int.MinValue,
		Any = -1
	}

	[Flags]
	public enum NearTouch
	{
		None = 0,
		PrimaryIndexTrigger = 1,
		PrimaryThumbButtons = 2,
		SecondaryIndexTrigger = 4,
		SecondaryThumbButtons = 8,
		Any = -1
	}

	[Flags]
	public enum RawNearTouch
	{
		None = 0,
		LIndexTrigger = 1,
		LThumbButtons = 2,
		RIndexTrigger = 4,
		RThumbButtons = 8,
		Any = -1
	}

	[Flags]
	public enum Axis1D
	{
		None = 0,
		PrimaryIndexTrigger = 1,
		PrimaryHandTrigger = 4,
		SecondaryIndexTrigger = 2,
		SecondaryHandTrigger = 8,
		PrimaryIndexTriggerCurl = 0x10,
		PrimaryIndexTriggerSlide = 0x20,
		PrimaryThumbRestForce = 0x40,
		PrimaryStylusForce = 0x80,
		SecondaryIndexTriggerCurl = 0x100,
		SecondaryIndexTriggerSlide = 0x200,
		SecondaryThumbRestForce = 0x400,
		SecondaryStylusForce = 0x800,
		PrimaryIndexTriggerForce = 0x1000,
		SecondaryIndexTriggerForce = 0x2000,
		Any = -1
	}

	[Flags]
	public enum RawAxis1D
	{
		None = 0,
		LIndexTrigger = 1,
		LHandTrigger = 4,
		RIndexTrigger = 2,
		RHandTrigger = 8,
		LIndexTriggerCurl = 0x10,
		LIndexTriggerSlide = 0x20,
		LThumbRestForce = 0x40,
		LStylusForce = 0x80,
		RIndexTriggerCurl = 0x100,
		RIndexTriggerSlide = 0x200,
		RThumbRestForce = 0x400,
		RStylusForce = 0x800,
		LIndexTriggerForce = 0x1000,
		RIndexTriggerForce = 0x2000,
		Any = -1
	}

	[Flags]
	public enum Axis2D
	{
		None = 0,
		PrimaryThumbstick = 1,
		PrimaryTouchpad = 4,
		SecondaryThumbstick = 2,
		SecondaryTouchpad = 8,
		Any = -1
	}

	[Flags]
	public enum RawAxis2D
	{
		None = 0,
		LThumbstick = 1,
		LTouchpad = 4,
		RThumbstick = 2,
		RTouchpad = 8,
		Any = -1
	}

	[Flags]
	public enum OpenVRButton : ulong
	{
		None = 0uL,
		Two = 2uL,
		Thumbstick = 0x100000000uL,
		Grip = 4uL
	}

	[Flags]
	public enum Controller
	{
		None = 0,
		LTouch = 1,
		RTouch = 2,
		Touch = 3,
		Remote = 4,
		Gamepad = 0x10,
		Hands = 0x60,
		LHand = 0x20,
		RHand = 0x40,
		Active = int.MinValue,
		All = -1
	}

	public enum Handedness
	{
		Unsupported,
		LeftHanded,
		RightHanded
	}

	public enum HapticsLocation
	{
		None = 0,
		Hand = 1,
		Thumb = 2,
		Index = 4
	}

	public enum InteractionProfile
	{
		None = 0,
		Touch = 1,
		TouchPro = 2,
		TouchPlus = 4
	}

	public enum Hand
	{
		None = -1,
		HandLeft,
		HandRight
	}

	public enum InputDeviceShowState
	{
		Always,
		ControllerInHandOrNoHand,
		ControllerInHand,
		ControllerNotInHand,
		NoHand
	}

	public enum ControllerInHandState
	{
		NoHand,
		ControllerInHand,
		ControllerNotInHand
	}

	public struct HapticsAmplitudeEnvelopeVibration
	{
		public int SamplesCount;

		public float[] Samples;

		public float Duration;
	}

	public struct HapticsPcmVibration
	{
		public int SamplesCount;

		public float[] Samples;

		public float SampleRateHz;

		public bool Append;
	}

	[Flags]
	public enum OpenVRController : ulong
	{
		Unknown = 0uL,
		OculusTouch = 1uL,
		ViveController = 2uL,
		WindowsMRController = 3uL
	}

	public struct OpenVRControllerDetails
	{
		public VRControllerState_t state;

		public OpenVRController controllerType;

		public uint deviceID;

		public Vector3 localPosition;

		public Quaternion localOrientation;
	}

	private class HapticInfo
	{
		public bool playingHaptics;

		public float hapticsDurationPlayed;

		public float hapticsDuration;

		public float hapticAmplitude;

		public XRNode node;
	}

	public abstract class OVRControllerBase
	{
		public class VirtualButtonMap
		{
			public RawButton None;

			public RawButton One;

			public RawButton Two;

			public RawButton Three;

			public RawButton Four;

			public RawButton Start;

			public RawButton Back;

			public RawButton PrimaryShoulder;

			public RawButton PrimaryIndexTrigger;

			public RawButton PrimaryHandTrigger;

			public RawButton PrimaryThumbstick;

			public RawButton PrimaryThumbstickUp;

			public RawButton PrimaryThumbstickDown;

			public RawButton PrimaryThumbstickLeft;

			public RawButton PrimaryThumbstickRight;

			public RawButton PrimaryTouchpad;

			public RawButton SecondaryShoulder;

			public RawButton SecondaryIndexTrigger;

			public RawButton SecondaryHandTrigger;

			public RawButton SecondaryThumbstick;

			public RawButton SecondaryThumbstickUp;

			public RawButton SecondaryThumbstickDown;

			public RawButton SecondaryThumbstickLeft;

			public RawButton SecondaryThumbstickRight;

			public RawButton SecondaryTouchpad;

			public RawButton DpadUp;

			public RawButton DpadDown;

			public RawButton DpadLeft;

			public RawButton DpadRight;

			public RawButton Up;

			public RawButton Down;

			public RawButton Left;

			public RawButton Right;

			public RawButton ToRawMask(Button virtualMask)
			{
				RawButton rawButton = RawButton.None;
				if (virtualMask == Button.None)
				{
					return RawButton.None;
				}
				if ((virtualMask & Button.One) != 0)
				{
					rawButton |= One;
				}
				if ((virtualMask & Button.Two) != 0)
				{
					rawButton |= Two;
				}
				if ((virtualMask & Button.Three) != 0)
				{
					rawButton |= Three;
				}
				if ((virtualMask & Button.Four) != 0)
				{
					rawButton |= Four;
				}
				if ((virtualMask & Button.Start) != 0)
				{
					rawButton |= Start;
				}
				if ((virtualMask & Button.Back) != 0)
				{
					rawButton |= Back;
				}
				if ((virtualMask & Button.PrimaryShoulder) != 0)
				{
					rawButton |= PrimaryShoulder;
				}
				if ((virtualMask & Button.PrimaryIndexTrigger) != 0)
				{
					rawButton |= PrimaryIndexTrigger;
				}
				if ((virtualMask & Button.PrimaryHandTrigger) != 0)
				{
					rawButton |= PrimaryHandTrigger;
				}
				if ((virtualMask & Button.PrimaryThumbstick) != 0)
				{
					rawButton |= PrimaryThumbstick;
				}
				if ((virtualMask & Button.PrimaryThumbstickUp) != 0)
				{
					rawButton |= PrimaryThumbstickUp;
				}
				if ((virtualMask & Button.PrimaryThumbstickDown) != 0)
				{
					rawButton |= PrimaryThumbstickDown;
				}
				if ((virtualMask & Button.PrimaryThumbstickLeft) != 0)
				{
					rawButton |= PrimaryThumbstickLeft;
				}
				if ((virtualMask & Button.PrimaryThumbstickRight) != 0)
				{
					rawButton |= PrimaryThumbstickRight;
				}
				if ((virtualMask & Button.PrimaryTouchpad) != 0)
				{
					rawButton |= PrimaryTouchpad;
				}
				if ((virtualMask & Button.SecondaryShoulder) != 0)
				{
					rawButton |= SecondaryShoulder;
				}
				if ((virtualMask & Button.SecondaryIndexTrigger) != 0)
				{
					rawButton |= SecondaryIndexTrigger;
				}
				if ((virtualMask & Button.SecondaryHandTrigger) != 0)
				{
					rawButton |= SecondaryHandTrigger;
				}
				if ((virtualMask & Button.SecondaryThumbstick) != 0)
				{
					rawButton |= SecondaryThumbstick;
				}
				if ((virtualMask & Button.SecondaryThumbstickUp) != 0)
				{
					rawButton |= SecondaryThumbstickUp;
				}
				if ((virtualMask & Button.SecondaryThumbstickDown) != 0)
				{
					rawButton |= SecondaryThumbstickDown;
				}
				if ((virtualMask & Button.SecondaryThumbstickLeft) != 0)
				{
					rawButton |= SecondaryThumbstickLeft;
				}
				if ((virtualMask & Button.SecondaryThumbstickRight) != 0)
				{
					rawButton |= SecondaryThumbstickRight;
				}
				if ((virtualMask & Button.SecondaryTouchpad) != 0)
				{
					rawButton |= SecondaryTouchpad;
				}
				if ((virtualMask & Button.DpadUp) != 0)
				{
					rawButton |= DpadUp;
				}
				if ((virtualMask & Button.DpadDown) != 0)
				{
					rawButton |= DpadDown;
				}
				if ((virtualMask & Button.DpadLeft) != 0)
				{
					rawButton |= DpadLeft;
				}
				if ((virtualMask & Button.DpadRight) != 0)
				{
					rawButton |= DpadRight;
				}
				if ((virtualMask & Button.Up) != 0)
				{
					rawButton |= Up;
				}
				if ((virtualMask & Button.Down) != 0)
				{
					rawButton |= Down;
				}
				if ((virtualMask & Button.Left) != 0)
				{
					rawButton |= Left;
				}
				if (((uint)virtualMask & 0x80000000u) != 0)
				{
					rawButton |= Right;
				}
				return rawButton;
			}
		}

		public class VirtualTouchMap
		{
			public RawTouch None;

			public RawTouch One;

			public RawTouch Two;

			public RawTouch Three;

			public RawTouch Four;

			public RawTouch PrimaryIndexTrigger;

			public RawTouch PrimaryThumbstick;

			public RawTouch PrimaryThumbRest;

			public RawTouch PrimaryTouchpad;

			public RawTouch SecondaryIndexTrigger;

			public RawTouch SecondaryThumbstick;

			public RawTouch SecondaryThumbRest;

			public RawTouch SecondaryTouchpad;

			public RawTouch ToRawMask(Touch virtualMask)
			{
				RawTouch rawTouch = RawTouch.None;
				if (virtualMask == Touch.None)
				{
					return RawTouch.None;
				}
				if ((virtualMask & Touch.One) != 0)
				{
					rawTouch |= One;
				}
				if ((virtualMask & Touch.Two) != 0)
				{
					rawTouch |= Two;
				}
				if ((virtualMask & Touch.Three) != 0)
				{
					rawTouch |= Three;
				}
				if ((virtualMask & Touch.Four) != 0)
				{
					rawTouch |= Four;
				}
				if ((virtualMask & Touch.PrimaryIndexTrigger) != 0)
				{
					rawTouch |= PrimaryIndexTrigger;
				}
				if ((virtualMask & Touch.PrimaryThumbstick) != 0)
				{
					rawTouch |= PrimaryThumbstick;
				}
				if ((virtualMask & Touch.PrimaryThumbRest) != 0)
				{
					rawTouch |= PrimaryThumbRest;
				}
				if ((virtualMask & Touch.PrimaryTouchpad) != 0)
				{
					rawTouch |= PrimaryTouchpad;
				}
				if ((virtualMask & Touch.SecondaryIndexTrigger) != 0)
				{
					rawTouch |= SecondaryIndexTrigger;
				}
				if ((virtualMask & Touch.SecondaryThumbstick) != 0)
				{
					rawTouch |= SecondaryThumbstick;
				}
				if ((virtualMask & Touch.SecondaryThumbRest) != 0)
				{
					rawTouch |= SecondaryThumbRest;
				}
				if ((virtualMask & Touch.SecondaryTouchpad) != 0)
				{
					rawTouch |= SecondaryTouchpad;
				}
				return rawTouch;
			}
		}

		public class VirtualNearTouchMap
		{
			public RawNearTouch None;

			public RawNearTouch PrimaryIndexTrigger;

			public RawNearTouch PrimaryThumbButtons;

			public RawNearTouch SecondaryIndexTrigger;

			public RawNearTouch SecondaryThumbButtons;

			public RawNearTouch ToRawMask(NearTouch virtualMask)
			{
				RawNearTouch rawNearTouch = RawNearTouch.None;
				if (virtualMask == NearTouch.None)
				{
					return RawNearTouch.None;
				}
				if ((virtualMask & NearTouch.PrimaryIndexTrigger) != 0)
				{
					rawNearTouch |= PrimaryIndexTrigger;
				}
				if ((virtualMask & NearTouch.PrimaryThumbButtons) != 0)
				{
					rawNearTouch |= PrimaryThumbButtons;
				}
				if ((virtualMask & NearTouch.SecondaryIndexTrigger) != 0)
				{
					rawNearTouch |= SecondaryIndexTrigger;
				}
				if ((virtualMask & NearTouch.SecondaryThumbButtons) != 0)
				{
					rawNearTouch |= SecondaryThumbButtons;
				}
				return rawNearTouch;
			}
		}

		public class VirtualAxis1DMap
		{
			public RawAxis1D None;

			public RawAxis1D PrimaryIndexTrigger;

			public RawAxis1D PrimaryHandTrigger;

			public RawAxis1D SecondaryIndexTrigger;

			public RawAxis1D SecondaryHandTrigger;

			public RawAxis1D PrimaryIndexTriggerCurl;

			public RawAxis1D PrimaryIndexTriggerSlide;

			public RawAxis1D PrimaryThumbRestForce;

			public RawAxis1D PrimaryStylusForce;

			public RawAxis1D SecondaryIndexTriggerCurl;

			public RawAxis1D SecondaryIndexTriggerSlide;

			public RawAxis1D SecondaryThumbRestForce;

			public RawAxis1D SecondaryStylusForce;

			public RawAxis1D PrimaryIndexTriggerForce;

			public RawAxis1D SecondaryIndexTriggerForce;

			public RawAxis1D ToRawMask(Axis1D virtualMask)
			{
				RawAxis1D rawAxis1D = RawAxis1D.None;
				if (virtualMask == Axis1D.None)
				{
					return RawAxis1D.None;
				}
				if ((virtualMask & Axis1D.PrimaryIndexTrigger) != 0)
				{
					rawAxis1D |= PrimaryIndexTrigger;
				}
				if ((virtualMask & Axis1D.PrimaryHandTrigger) != 0)
				{
					rawAxis1D |= PrimaryHandTrigger;
				}
				if ((virtualMask & Axis1D.SecondaryIndexTrigger) != 0)
				{
					rawAxis1D |= SecondaryIndexTrigger;
				}
				if ((virtualMask & Axis1D.SecondaryHandTrigger) != 0)
				{
					rawAxis1D |= SecondaryHandTrigger;
				}
				if ((virtualMask & Axis1D.PrimaryIndexTriggerCurl) != 0)
				{
					rawAxis1D |= PrimaryIndexTriggerCurl;
				}
				if ((virtualMask & Axis1D.PrimaryIndexTriggerSlide) != 0)
				{
					rawAxis1D |= PrimaryIndexTriggerSlide;
				}
				if ((virtualMask & Axis1D.PrimaryThumbRestForce) != 0)
				{
					rawAxis1D |= PrimaryThumbRestForce;
				}
				if ((virtualMask & Axis1D.PrimaryStylusForce) != 0)
				{
					rawAxis1D |= PrimaryStylusForce;
				}
				if ((virtualMask & Axis1D.SecondaryIndexTriggerCurl) != 0)
				{
					rawAxis1D |= SecondaryIndexTriggerCurl;
				}
				if ((virtualMask & Axis1D.SecondaryIndexTriggerSlide) != 0)
				{
					rawAxis1D |= SecondaryIndexTriggerSlide;
				}
				if ((virtualMask & Axis1D.SecondaryThumbRestForce) != 0)
				{
					rawAxis1D |= SecondaryThumbRestForce;
				}
				if ((virtualMask & Axis1D.SecondaryStylusForce) != 0)
				{
					rawAxis1D |= SecondaryStylusForce;
				}
				if ((virtualMask & Axis1D.SecondaryIndexTriggerForce) != 0)
				{
					rawAxis1D |= SecondaryIndexTriggerForce;
				}
				if ((virtualMask & Axis1D.PrimaryIndexTriggerForce) != 0)
				{
					rawAxis1D |= PrimaryIndexTriggerForce;
				}
				return rawAxis1D;
			}
		}

		public class VirtualAxis2DMap
		{
			public RawAxis2D None;

			public RawAxis2D PrimaryThumbstick;

			public RawAxis2D PrimaryTouchpad;

			public RawAxis2D SecondaryThumbstick;

			public RawAxis2D SecondaryTouchpad;

			public RawAxis2D ToRawMask(Axis2D virtualMask)
			{
				RawAxis2D rawAxis2D = RawAxis2D.None;
				if (virtualMask == Axis2D.None)
				{
					return RawAxis2D.None;
				}
				if ((virtualMask & Axis2D.PrimaryThumbstick) != 0)
				{
					rawAxis2D |= PrimaryThumbstick;
				}
				if ((virtualMask & Axis2D.PrimaryTouchpad) != 0)
				{
					rawAxis2D |= PrimaryTouchpad;
				}
				if ((virtualMask & Axis2D.SecondaryThumbstick) != 0)
				{
					rawAxis2D |= SecondaryThumbstick;
				}
				if ((virtualMask & Axis2D.SecondaryTouchpad) != 0)
				{
					rawAxis2D |= SecondaryTouchpad;
				}
				return rawAxis2D;
			}
		}

		public Controller controllerType;

		public VirtualButtonMap buttonMap = new VirtualButtonMap();

		public VirtualTouchMap touchMap = new VirtualTouchMap();

		public VirtualNearTouchMap nearTouchMap = new VirtualNearTouchMap();

		public VirtualAxis1DMap axis1DMap = new VirtualAxis1DMap();

		public VirtualAxis2DMap axis2DMap = new VirtualAxis2DMap();

		public OVRPlugin.ControllerState6 previousState;

		public OVRPlugin.ControllerState6 currentState;

		public bool shouldApplyDeadzone = true;

		private uint[] HapticsPcmSamplesConsumedCache = new uint[1];

		public OVRControllerBase()
		{
			ConfigureButtonMap();
			ConfigureTouchMap();
			ConfigureNearTouchMap();
			ConfigureAxis1DMap();
			ConfigureAxis2DMap();
		}

		public virtual Controller Update()
		{
			OVRPlugin.ControllerState6 controllerState = ((OVRManager.loadedXRDevice != OVRManager.XRDevice.OpenVR || (controllerType & Controller.Touch) == 0) ? OVRPlugin.GetControllerState6((uint)controllerType) : GetOpenVRControllerState(controllerType));
			if (controllerState.LIndexTrigger >= AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 268435456u;
			}
			if (controllerState.LHandTrigger >= AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 536870912u;
			}
			if (controllerState.LThumbstick.y >= AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 16u;
			}
			if (controllerState.LThumbstick.y <= 0f - AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 32u;
			}
			if (controllerState.LThumbstick.x <= 0f - AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 64u;
			}
			if (controllerState.LThumbstick.x >= AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 128u;
			}
			if (controllerState.RIndexTrigger >= AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 67108864u;
			}
			if (controllerState.RHandTrigger >= AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 134217728u;
			}
			if (controllerState.RThumbstick.y >= AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 4096u;
			}
			if (controllerState.RThumbstick.y <= 0f - AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 8192u;
			}
			if (controllerState.RThumbstick.x <= 0f - AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 16384u;
			}
			if (controllerState.RThumbstick.x >= AXIS_AS_BUTTON_THRESHOLD)
			{
				controllerState.Buttons |= 32768u;
			}
			previousState = currentState;
			currentState = controllerState;
			return (Controller)((int)currentState.ConnectedControllers & (int)controllerType);
		}

		private OVRPlugin.ControllerState6 GetOpenVRControllerState(Controller controllerType)
		{
			OVRPlugin.ControllerState6 result = default(OVRPlugin.ControllerState6);
			if ((controllerType & Controller.LTouch) == Controller.LTouch && IsValidOpenVRDevice(openVRControllerDetails[0].deviceID))
			{
				VRControllerState_t state = openVRControllerDetails[0].state;
				if ((state.ulButtonPressed & 2) == 2)
				{
					result.Buttons |= 512u;
				}
				if ((state.ulButtonPressed & 0x100000000L) == 4294967296L)
				{
					result.Buttons |= 1024u;
				}
				result.LIndexTrigger = state.rAxis1.x;
				if (openVRControllerDetails[0].controllerType == OpenVRController.OculusTouch || openVRControllerDetails[0].controllerType == OpenVRController.ViveController)
				{
					result.LThumbstick.x = state.rAxis0.x;
					result.LThumbstick.y = state.rAxis0.y;
				}
				else if (openVRControllerDetails[0].controllerType == OpenVRController.WindowsMRController)
				{
					result.LThumbstick.x = state.rAxis2.x;
					result.LThumbstick.y = state.rAxis2.y;
				}
				if (openVRControllerDetails[0].controllerType == OpenVRController.OculusTouch)
				{
					result.LHandTrigger = state.rAxis2.x;
				}
				else if (openVRControllerDetails[0].controllerType == OpenVRController.ViveController || openVRControllerDetails[0].controllerType == OpenVRController.WindowsMRController)
				{
					result.LHandTrigger = (((state.ulButtonPressed & 4) == 4) ? 1 : 0);
				}
			}
			if ((controllerType & Controller.RTouch) == Controller.RTouch && IsValidOpenVRDevice(openVRControllerDetails[1].deviceID))
			{
				VRControllerState_t state2 = openVRControllerDetails[1].state;
				if ((state2.ulButtonPressed & 2) == 2)
				{
					result.Buttons |= 2u;
				}
				if ((state2.ulButtonPressed & 0x100000000L) == 4294967296L)
				{
					result.Buttons |= 4u;
				}
				result.RIndexTrigger = state2.rAxis1.x;
				if (openVRControllerDetails[1].controllerType == OpenVRController.OculusTouch || openVRControllerDetails[1].controllerType == OpenVRController.ViveController)
				{
					result.RThumbstick.x = state2.rAxis0.x;
					result.RThumbstick.y = state2.rAxis0.y;
				}
				else if (openVRControllerDetails[1].controllerType == OpenVRController.WindowsMRController)
				{
					result.RThumbstick.x = state2.rAxis2.x;
					result.RThumbstick.y = state2.rAxis2.y;
				}
				if (openVRControllerDetails[1].controllerType == OpenVRController.OculusTouch)
				{
					result.RHandTrigger = state2.rAxis2.x;
				}
				else if (openVRControllerDetails[1].controllerType == OpenVRController.ViveController || openVRControllerDetails[1].controllerType == OpenVRController.WindowsMRController)
				{
					result.RHandTrigger = (((state2.ulButtonPressed & 4) == 4) ? 1 : 0);
				}
			}
			return result;
		}

		public virtual void SetControllerVibration(float frequency, float amplitude)
		{
			OVRPlugin.SetControllerVibration((uint)controllerType, frequency, amplitude);
		}

		public virtual void SetControllerLocalizedVibration(HapticsLocation hapticsLocationMask, float frequency, float amplitude)
		{
			OVRPlugin.SetControllerLocalizedVibration((OVRPlugin.Controller)controllerType, (OVRPlugin.HapticsLocation)hapticsLocationMask, frequency, amplitude);
		}

		public virtual void SetControllerHapticsAmplitudeEnvelope(HapticsAmplitudeEnvelopeVibration hapticsVibration)
		{
			GCHandle gCHandle = GCHandle.Alloc(hapticsVibration.Samples, GCHandleType.Pinned);
			try
			{
				OVRPlugin.HapticsAmplitudeEnvelopeVibration hapticsVibration2 = default(OVRPlugin.HapticsAmplitudeEnvelopeVibration);
				hapticsVibration2.AmplitudeCount = (uint)hapticsVibration.SamplesCount;
				hapticsVibration2.Amplitudes = gCHandle.AddrOfPinnedObject();
				hapticsVibration2.Duration = hapticsVibration.Duration;
				OVRPlugin.SetControllerHapticsAmplitudeEnvelope((OVRPlugin.Controller)controllerType, hapticsVibration2);
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
			}
		}

		public virtual int SetControllerHapticsPcm(HapticsPcmVibration hapticsVibration)
		{
			GCHandle gCHandle = GCHandle.Alloc(hapticsVibration.Samples, GCHandleType.Pinned);
			GCHandle gCHandle2 = GCHandle.Alloc(HapticsPcmSamplesConsumedCache, GCHandleType.Pinned);
			int result = 0;
			try
			{
				OVRPlugin.HapticsPcmVibration hapticsVibration2 = default(OVRPlugin.HapticsPcmVibration);
				hapticsVibration2.BufferSize = (uint)hapticsVibration.SamplesCount;
				hapticsVibration2.Buffer = gCHandle.AddrOfPinnedObject();
				hapticsVibration2.SampleRateHz = hapticsVibration.SampleRateHz;
				hapticsVibration2.Append = (hapticsVibration.Append ? OVRPlugin.Bool.True : OVRPlugin.Bool.False);
				hapticsVibration2.SamplesConsumed = gCHandle2.AddrOfPinnedObject();
				if (OVRPlugin.SetControllerHapticsPcm((OVRPlugin.Controller)controllerType, hapticsVibration2))
				{
					result = Marshal.ReadInt32(hapticsVibration2.SamplesConsumed);
				}
			}
			finally
			{
				if (gCHandle.IsAllocated)
				{
					gCHandle.Free();
				}
				if (gCHandle2.IsAllocated)
				{
					gCHandle2.Free();
				}
			}
			return result;
		}

		public virtual float GetControllerSampleRateHz()
		{
			OVRPlugin.GetControllerSampleRateHz((OVRPlugin.Controller)controllerType, out var sampleRateHz);
			return sampleRateHz;
		}

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public virtual byte GetBatteryPercentRemaining()
		{
			return 0;
		}

		public abstract void ConfigureButtonMap();

		public abstract void ConfigureTouchMap();

		public abstract void ConfigureNearTouchMap();

		public abstract void ConfigureAxis1DMap();

		public abstract void ConfigureAxis2DMap();

		public RawButton ResolveToRawMask(Button virtualMask)
		{
			return buttonMap.ToRawMask(virtualMask);
		}

		public RawTouch ResolveToRawMask(Touch virtualMask)
		{
			return touchMap.ToRawMask(virtualMask);
		}

		public RawNearTouch ResolveToRawMask(NearTouch virtualMask)
		{
			return nearTouchMap.ToRawMask(virtualMask);
		}

		public RawAxis1D ResolveToRawMask(Axis1D virtualMask)
		{
			return axis1DMap.ToRawMask(virtualMask);
		}

		public RawAxis2D ResolveToRawMask(Axis2D virtualMask)
		{
			return axis2DMap.ToRawMask(virtualMask);
		}
	}

	public class OVRControllerTouch : OVRControllerBase
	{
		public OVRControllerTouch()
		{
			controllerType = Controller.Touch;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.A;
			buttonMap.Two = RawButton.B;
			buttonMap.Three = RawButton.X;
			buttonMap.Four = RawButton.Y;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.None;
			buttonMap.PrimaryShoulder = RawButton.None;
			buttonMap.PrimaryIndexTrigger = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger = RawButton.LHandTrigger;
			buttonMap.PrimaryThumbstick = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.None;
			buttonMap.SecondaryIndexTrigger = RawButton.RIndexTrigger;
			buttonMap.SecondaryHandTrigger = RawButton.RHandTrigger;
			buttonMap.SecondaryThumbstick = RawButton.RThumbstick;
			buttonMap.SecondaryThumbstickUp = RawButton.RThumbstickUp;
			buttonMap.SecondaryThumbstickDown = RawButton.RThumbstickDown;
			buttonMap.SecondaryThumbstickLeft = RawButton.RThumbstickLeft;
			buttonMap.SecondaryThumbstickRight = RawButton.RThumbstickRight;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.None;
			buttonMap.DpadDown = RawButton.None;
			buttonMap.DpadLeft = RawButton.None;
			buttonMap.DpadRight = RawButton.None;
			buttonMap.Up = RawButton.LThumbstickUp;
			buttonMap.Down = RawButton.LThumbstickDown;
			buttonMap.Left = RawButton.LThumbstickLeft;
			buttonMap.Right = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.A;
			touchMap.Two = RawTouch.B;
			touchMap.Three = RawTouch.X;
			touchMap.Four = RawTouch.Y;
			touchMap.PrimaryIndexTrigger = RawTouch.LIndexTrigger;
			touchMap.PrimaryThumbstick = RawTouch.LThumbstick;
			touchMap.PrimaryThumbRest = RawTouch.LThumbRest;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.RIndexTrigger;
			touchMap.SecondaryThumbstick = RawTouch.RThumbstick;
			touchMap.SecondaryThumbRest = RawTouch.RThumbRest;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.LIndexTrigger;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.LThumbButtons;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.RIndexTrigger;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.RThumbButtons;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.LHandTrigger;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.RIndexTrigger;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.RHandTrigger;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.LIndexTriggerCurl;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.LIndexTriggerSlide;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.LThumbRestForce;
			axis1DMap.PrimaryStylusForce = RawAxis1D.LStylusForce;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.RIndexTriggerCurl;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.RIndexTriggerSlide;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.RThumbRestForce;
			axis1DMap.SecondaryStylusForce = RawAxis1D.RStylusForce;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.LIndexTriggerForce;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.RIndexTriggerForce;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad = RawAxis2D.LTouchpad;
			axis2DMap.SecondaryThumbstick = RawAxis2D.RThumbstick;
			axis2DMap.SecondaryTouchpad = RawAxis2D.RTouchpad;
		}

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public override byte GetBatteryPercentRemaining()
		{
			byte lBatteryPercentRemaining = currentState.LBatteryPercentRemaining;
			byte rBatteryPercentRemaining = currentState.RBatteryPercentRemaining;
			if (lBatteryPercentRemaining > rBatteryPercentRemaining)
			{
				return rBatteryPercentRemaining;
			}
			return lBatteryPercentRemaining;
		}
	}

	public class OVRControllerLTouch : OVRControllerBase
	{
		public OVRControllerLTouch()
		{
			controllerType = Controller.LTouch;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.X;
			buttonMap.Two = RawButton.Y;
			buttonMap.Three = RawButton.None;
			buttonMap.Four = RawButton.None;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.None;
			buttonMap.PrimaryShoulder = RawButton.None;
			buttonMap.PrimaryIndexTrigger = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger = RawButton.LHandTrigger;
			buttonMap.PrimaryThumbstick = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.None;
			buttonMap.SecondaryIndexTrigger = RawButton.None;
			buttonMap.SecondaryHandTrigger = RawButton.None;
			buttonMap.SecondaryThumbstick = RawButton.None;
			buttonMap.SecondaryThumbstickUp = RawButton.None;
			buttonMap.SecondaryThumbstickDown = RawButton.None;
			buttonMap.SecondaryThumbstickLeft = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.None;
			buttonMap.DpadDown = RawButton.None;
			buttonMap.DpadLeft = RawButton.None;
			buttonMap.DpadRight = RawButton.None;
			buttonMap.Up = RawButton.LThumbstickUp;
			buttonMap.Down = RawButton.LThumbstickDown;
			buttonMap.Left = RawButton.LThumbstickLeft;
			buttonMap.Right = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.X;
			touchMap.Two = RawTouch.Y;
			touchMap.Three = RawTouch.None;
			touchMap.Four = RawTouch.None;
			touchMap.PrimaryIndexTrigger = RawTouch.LIndexTrigger;
			touchMap.PrimaryThumbstick = RawTouch.LThumbstick;
			touchMap.PrimaryThumbRest = RawTouch.LThumbRest;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.None;
			touchMap.SecondaryThumbstick = RawTouch.None;
			touchMap.SecondaryThumbRest = RawTouch.None;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.LIndexTrigger;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.LThumbButtons;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.LHandTrigger;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.LIndexTriggerCurl;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.LIndexTriggerSlide;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.LThumbRestForce;
			axis1DMap.PrimaryStylusForce = RawAxis1D.LStylusForce;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.None;
			axis1DMap.SecondaryStylusForce = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.LIndexTriggerForce;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad = RawAxis2D.LTouchpad;
			axis2DMap.SecondaryThumbstick = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad = RawAxis2D.None;
		}

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public override byte GetBatteryPercentRemaining()
		{
			return currentState.LBatteryPercentRemaining;
		}
	}

	private class OVRControllerRTouch : OVRControllerBase
	{
		public OVRControllerRTouch()
		{
			controllerType = Controller.RTouch;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.A;
			buttonMap.Two = RawButton.B;
			buttonMap.Three = RawButton.None;
			buttonMap.Four = RawButton.None;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.None;
			buttonMap.PrimaryShoulder = RawButton.None;
			buttonMap.PrimaryIndexTrigger = RawButton.RIndexTrigger;
			buttonMap.PrimaryHandTrigger = RawButton.RHandTrigger;
			buttonMap.PrimaryThumbstick = RawButton.RThumbstick;
			buttonMap.PrimaryThumbstickUp = RawButton.RThumbstickUp;
			buttonMap.PrimaryThumbstickDown = RawButton.RThumbstickDown;
			buttonMap.PrimaryThumbstickLeft = RawButton.RThumbstickLeft;
			buttonMap.PrimaryThumbstickRight = RawButton.RThumbstickRight;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.None;
			buttonMap.SecondaryIndexTrigger = RawButton.None;
			buttonMap.SecondaryHandTrigger = RawButton.None;
			buttonMap.SecondaryThumbstick = RawButton.None;
			buttonMap.SecondaryThumbstickUp = RawButton.None;
			buttonMap.SecondaryThumbstickDown = RawButton.None;
			buttonMap.SecondaryThumbstickLeft = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.None;
			buttonMap.DpadDown = RawButton.None;
			buttonMap.DpadLeft = RawButton.None;
			buttonMap.DpadRight = RawButton.None;
			buttonMap.Up = RawButton.RThumbstickUp;
			buttonMap.Down = RawButton.RThumbstickDown;
			buttonMap.Left = RawButton.RThumbstickLeft;
			buttonMap.Right = RawButton.RThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.A;
			touchMap.Two = RawTouch.B;
			touchMap.Three = RawTouch.None;
			touchMap.Four = RawTouch.None;
			touchMap.PrimaryIndexTrigger = RawTouch.RIndexTrigger;
			touchMap.PrimaryThumbstick = RawTouch.RThumbstick;
			touchMap.PrimaryThumbRest = RawTouch.RThumbRest;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.None;
			touchMap.SecondaryThumbstick = RawTouch.None;
			touchMap.SecondaryThumbRest = RawTouch.None;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.RIndexTrigger;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.RThumbButtons;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.RIndexTrigger;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.RHandTrigger;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.RIndexTriggerCurl;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.RIndexTriggerSlide;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.RThumbRestForce;
			axis1DMap.PrimaryStylusForce = RawAxis1D.RStylusForce;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.None;
			axis1DMap.SecondaryStylusForce = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.RIndexTriggerForce;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.RThumbstick;
			axis2DMap.PrimaryTouchpad = RawAxis2D.RTouchpad;
			axis2DMap.SecondaryThumbstick = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad = RawAxis2D.None;
		}

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public override byte GetBatteryPercentRemaining()
		{
			return currentState.RBatteryPercentRemaining;
		}
	}

	public class OVRControllerHands : OVRControllerBase
	{
		public OVRControllerHands()
		{
			controllerType = Controller.Hands;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.A;
			buttonMap.Two = RawButton.None;
			buttonMap.Three = RawButton.X;
			buttonMap.Four = RawButton.None;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.None;
			buttonMap.PrimaryShoulder = RawButton.None;
			buttonMap.PrimaryIndexTrigger = RawButton.None;
			buttonMap.PrimaryHandTrigger = RawButton.None;
			buttonMap.PrimaryThumbstick = RawButton.None;
			buttonMap.PrimaryThumbstickUp = RawButton.None;
			buttonMap.PrimaryThumbstickDown = RawButton.None;
			buttonMap.PrimaryThumbstickLeft = RawButton.None;
			buttonMap.PrimaryThumbstickRight = RawButton.None;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.None;
			buttonMap.SecondaryIndexTrigger = RawButton.None;
			buttonMap.SecondaryHandTrigger = RawButton.None;
			buttonMap.SecondaryThumbstick = RawButton.None;
			buttonMap.SecondaryThumbstickUp = RawButton.None;
			buttonMap.SecondaryThumbstickDown = RawButton.None;
			buttonMap.SecondaryThumbstickLeft = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.None;
			buttonMap.DpadDown = RawButton.None;
			buttonMap.DpadLeft = RawButton.None;
			buttonMap.DpadRight = RawButton.None;
			buttonMap.Up = RawButton.None;
			buttonMap.Down = RawButton.None;
			buttonMap.Left = RawButton.None;
			buttonMap.Right = RawButton.None;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.None;
			touchMap.Two = RawTouch.None;
			touchMap.Three = RawTouch.None;
			touchMap.Four = RawTouch.None;
			touchMap.PrimaryIndexTrigger = RawTouch.None;
			touchMap.PrimaryThumbstick = RawTouch.None;
			touchMap.PrimaryThumbRest = RawTouch.None;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.None;
			touchMap.SecondaryThumbstick = RawTouch.None;
			touchMap.SecondaryThumbRest = RawTouch.None;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.None;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.None;
			axis1DMap.PrimaryStylusForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.None;
			axis1DMap.SecondaryStylusForce = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.None;
			axis2DMap.PrimaryTouchpad = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad = RawAxis2D.None;
		}

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public override byte GetBatteryPercentRemaining()
		{
			byte lBatteryPercentRemaining = currentState.LBatteryPercentRemaining;
			byte rBatteryPercentRemaining = currentState.RBatteryPercentRemaining;
			if (lBatteryPercentRemaining > rBatteryPercentRemaining)
			{
				return rBatteryPercentRemaining;
			}
			return lBatteryPercentRemaining;
		}
	}

	public class OVRControllerLHand : OVRControllerBase
	{
		public OVRControllerLHand()
		{
			controllerType = Controller.LHand;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.X;
			buttonMap.Two = RawButton.None;
			buttonMap.Three = RawButton.None;
			buttonMap.Four = RawButton.None;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.None;
			buttonMap.PrimaryShoulder = RawButton.None;
			buttonMap.PrimaryIndexTrigger = RawButton.None;
			buttonMap.PrimaryHandTrigger = RawButton.None;
			buttonMap.PrimaryThumbstick = RawButton.None;
			buttonMap.PrimaryThumbstickUp = RawButton.None;
			buttonMap.PrimaryThumbstickDown = RawButton.None;
			buttonMap.PrimaryThumbstickLeft = RawButton.None;
			buttonMap.PrimaryThumbstickRight = RawButton.None;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.None;
			buttonMap.SecondaryIndexTrigger = RawButton.None;
			buttonMap.SecondaryHandTrigger = RawButton.None;
			buttonMap.SecondaryThumbstick = RawButton.None;
			buttonMap.SecondaryThumbstickUp = RawButton.None;
			buttonMap.SecondaryThumbstickDown = RawButton.None;
			buttonMap.SecondaryThumbstickLeft = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.None;
			buttonMap.DpadDown = RawButton.None;
			buttonMap.DpadLeft = RawButton.None;
			buttonMap.DpadRight = RawButton.None;
			buttonMap.Up = RawButton.None;
			buttonMap.Down = RawButton.None;
			buttonMap.Left = RawButton.None;
			buttonMap.Right = RawButton.None;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.None;
			touchMap.Two = RawTouch.None;
			touchMap.Three = RawTouch.None;
			touchMap.Four = RawTouch.None;
			touchMap.PrimaryIndexTrigger = RawTouch.None;
			touchMap.PrimaryThumbstick = RawTouch.None;
			touchMap.PrimaryThumbRest = RawTouch.None;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.None;
			touchMap.SecondaryThumbstick = RawTouch.None;
			touchMap.SecondaryThumbRest = RawTouch.None;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.None;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.None;
			axis1DMap.PrimaryStylusForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.None;
			axis1DMap.SecondaryStylusForce = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.None;
			axis2DMap.PrimaryTouchpad = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad = RawAxis2D.None;
		}

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public override byte GetBatteryPercentRemaining()
		{
			return currentState.LBatteryPercentRemaining;
		}
	}

	public class OVRControllerRHand : OVRControllerBase
	{
		public OVRControllerRHand()
		{
			controllerType = Controller.RHand;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.A;
			buttonMap.Two = RawButton.None;
			buttonMap.Three = RawButton.None;
			buttonMap.Four = RawButton.None;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.None;
			buttonMap.PrimaryShoulder = RawButton.None;
			buttonMap.PrimaryIndexTrigger = RawButton.None;
			buttonMap.PrimaryHandTrigger = RawButton.None;
			buttonMap.PrimaryThumbstick = RawButton.None;
			buttonMap.PrimaryThumbstickUp = RawButton.None;
			buttonMap.PrimaryThumbstickDown = RawButton.None;
			buttonMap.PrimaryThumbstickLeft = RawButton.None;
			buttonMap.PrimaryThumbstickRight = RawButton.None;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.None;
			buttonMap.SecondaryIndexTrigger = RawButton.None;
			buttonMap.SecondaryHandTrigger = RawButton.None;
			buttonMap.SecondaryThumbstick = RawButton.None;
			buttonMap.SecondaryThumbstickUp = RawButton.None;
			buttonMap.SecondaryThumbstickDown = RawButton.None;
			buttonMap.SecondaryThumbstickLeft = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.None;
			buttonMap.DpadDown = RawButton.None;
			buttonMap.DpadLeft = RawButton.None;
			buttonMap.DpadRight = RawButton.None;
			buttonMap.Up = RawButton.None;
			buttonMap.Down = RawButton.None;
			buttonMap.Left = RawButton.None;
			buttonMap.Right = RawButton.None;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.None;
			touchMap.Two = RawTouch.None;
			touchMap.Three = RawTouch.None;
			touchMap.Four = RawTouch.None;
			touchMap.PrimaryIndexTrigger = RawTouch.None;
			touchMap.PrimaryThumbstick = RawTouch.None;
			touchMap.PrimaryThumbRest = RawTouch.None;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.None;
			touchMap.SecondaryThumbstick = RawTouch.None;
			touchMap.SecondaryThumbRest = RawTouch.None;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.None;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.None;
			axis1DMap.PrimaryStylusForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.None;
			axis1DMap.SecondaryStylusForce = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.None;
			axis2DMap.PrimaryTouchpad = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad = RawAxis2D.None;
		}

		[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
		public override byte GetBatteryPercentRemaining()
		{
			return currentState.RBatteryPercentRemaining;
		}
	}

	public class OVRControllerRemote : OVRControllerBase
	{
		public OVRControllerRemote()
		{
			controllerType = Controller.Remote;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.Start;
			buttonMap.Two = RawButton.Back;
			buttonMap.Three = RawButton.None;
			buttonMap.Four = RawButton.None;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.Back;
			buttonMap.PrimaryShoulder = RawButton.None;
			buttonMap.PrimaryIndexTrigger = RawButton.None;
			buttonMap.PrimaryHandTrigger = RawButton.None;
			buttonMap.PrimaryThumbstick = RawButton.None;
			buttonMap.PrimaryThumbstickUp = RawButton.None;
			buttonMap.PrimaryThumbstickDown = RawButton.None;
			buttonMap.PrimaryThumbstickLeft = RawButton.None;
			buttonMap.PrimaryThumbstickRight = RawButton.None;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.None;
			buttonMap.SecondaryIndexTrigger = RawButton.None;
			buttonMap.SecondaryHandTrigger = RawButton.None;
			buttonMap.SecondaryThumbstick = RawButton.None;
			buttonMap.SecondaryThumbstickUp = RawButton.None;
			buttonMap.SecondaryThumbstickDown = RawButton.None;
			buttonMap.SecondaryThumbstickLeft = RawButton.None;
			buttonMap.SecondaryThumbstickRight = RawButton.None;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.DpadUp;
			buttonMap.DpadDown = RawButton.DpadDown;
			buttonMap.DpadLeft = RawButton.DpadLeft;
			buttonMap.DpadRight = RawButton.DpadRight;
			buttonMap.Up = RawButton.DpadUp;
			buttonMap.Down = RawButton.DpadDown;
			buttonMap.Left = RawButton.DpadLeft;
			buttonMap.Right = RawButton.DpadRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.None;
			touchMap.Two = RawTouch.None;
			touchMap.Three = RawTouch.None;
			touchMap.Four = RawTouch.None;
			touchMap.PrimaryIndexTrigger = RawTouch.None;
			touchMap.PrimaryThumbstick = RawTouch.None;
			touchMap.PrimaryThumbRest = RawTouch.None;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.None;
			touchMap.SecondaryThumbstick = RawTouch.None;
			touchMap.SecondaryThumbRest = RawTouch.None;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.None;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.None;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.None;
			axis1DMap.PrimaryStylusForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.None;
			axis1DMap.SecondaryStylusForce = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.None;
			axis2DMap.PrimaryTouchpad = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick = RawAxis2D.None;
			axis2DMap.SecondaryTouchpad = RawAxis2D.None;
		}
	}

	public class OVRControllerGamepadPC : OVRControllerBase
	{
		public OVRControllerGamepadPC()
		{
			controllerType = Controller.Gamepad;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.A;
			buttonMap.Two = RawButton.B;
			buttonMap.Three = RawButton.X;
			buttonMap.Four = RawButton.Y;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.Back;
			buttonMap.PrimaryShoulder = RawButton.LShoulder;
			buttonMap.PrimaryIndexTrigger = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger = RawButton.None;
			buttonMap.PrimaryThumbstick = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.RShoulder;
			buttonMap.SecondaryIndexTrigger = RawButton.RIndexTrigger;
			buttonMap.SecondaryHandTrigger = RawButton.None;
			buttonMap.SecondaryThumbstick = RawButton.RThumbstick;
			buttonMap.SecondaryThumbstickUp = RawButton.RThumbstickUp;
			buttonMap.SecondaryThumbstickDown = RawButton.RThumbstickDown;
			buttonMap.SecondaryThumbstickLeft = RawButton.RThumbstickLeft;
			buttonMap.SecondaryThumbstickRight = RawButton.RThumbstickRight;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.DpadUp;
			buttonMap.DpadDown = RawButton.DpadDown;
			buttonMap.DpadLeft = RawButton.DpadLeft;
			buttonMap.DpadRight = RawButton.DpadRight;
			buttonMap.Up = RawButton.LThumbstickUp;
			buttonMap.Down = RawButton.LThumbstickDown;
			buttonMap.Left = RawButton.LThumbstickLeft;
			buttonMap.Right = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.None;
			touchMap.Two = RawTouch.None;
			touchMap.Three = RawTouch.None;
			touchMap.Four = RawTouch.None;
			touchMap.PrimaryIndexTrigger = RawTouch.None;
			touchMap.PrimaryThumbstick = RawTouch.None;
			touchMap.PrimaryThumbRest = RawTouch.None;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.None;
			touchMap.SecondaryThumbstick = RawTouch.None;
			touchMap.SecondaryThumbRest = RawTouch.None;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.RIndexTrigger;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.None;
			axis1DMap.PrimaryStylusForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.None;
			axis1DMap.SecondaryStylusForce = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick = RawAxis2D.RThumbstick;
			axis2DMap.SecondaryTouchpad = RawAxis2D.None;
		}
	}

	private class OVRControllerGamepadAndroid : OVRControllerBase
	{
		public OVRControllerGamepadAndroid()
		{
			controllerType = Controller.Gamepad;
		}

		public override void ConfigureButtonMap()
		{
			buttonMap.None = RawButton.None;
			buttonMap.One = RawButton.A;
			buttonMap.Two = RawButton.B;
			buttonMap.Three = RawButton.X;
			buttonMap.Four = RawButton.Y;
			buttonMap.Start = RawButton.Start;
			buttonMap.Back = RawButton.Back;
			buttonMap.PrimaryShoulder = RawButton.LShoulder;
			buttonMap.PrimaryIndexTrigger = RawButton.LIndexTrigger;
			buttonMap.PrimaryHandTrigger = RawButton.None;
			buttonMap.PrimaryThumbstick = RawButton.LThumbstick;
			buttonMap.PrimaryThumbstickUp = RawButton.LThumbstickUp;
			buttonMap.PrimaryThumbstickDown = RawButton.LThumbstickDown;
			buttonMap.PrimaryThumbstickLeft = RawButton.LThumbstickLeft;
			buttonMap.PrimaryThumbstickRight = RawButton.LThumbstickRight;
			buttonMap.PrimaryTouchpad = RawButton.None;
			buttonMap.SecondaryShoulder = RawButton.RShoulder;
			buttonMap.SecondaryIndexTrigger = RawButton.RIndexTrigger;
			buttonMap.SecondaryHandTrigger = RawButton.None;
			buttonMap.SecondaryThumbstick = RawButton.RThumbstick;
			buttonMap.SecondaryThumbstickUp = RawButton.RThumbstickUp;
			buttonMap.SecondaryThumbstickDown = RawButton.RThumbstickDown;
			buttonMap.SecondaryThumbstickLeft = RawButton.RThumbstickLeft;
			buttonMap.SecondaryThumbstickRight = RawButton.RThumbstickRight;
			buttonMap.SecondaryTouchpad = RawButton.None;
			buttonMap.DpadUp = RawButton.DpadUp;
			buttonMap.DpadDown = RawButton.DpadDown;
			buttonMap.DpadLeft = RawButton.DpadLeft;
			buttonMap.DpadRight = RawButton.DpadRight;
			buttonMap.Up = RawButton.LThumbstickUp;
			buttonMap.Down = RawButton.LThumbstickDown;
			buttonMap.Left = RawButton.LThumbstickLeft;
			buttonMap.Right = RawButton.LThumbstickRight;
		}

		public override void ConfigureTouchMap()
		{
			touchMap.None = RawTouch.None;
			touchMap.One = RawTouch.None;
			touchMap.Two = RawTouch.None;
			touchMap.Three = RawTouch.None;
			touchMap.Four = RawTouch.None;
			touchMap.PrimaryIndexTrigger = RawTouch.None;
			touchMap.PrimaryThumbstick = RawTouch.None;
			touchMap.PrimaryThumbRest = RawTouch.None;
			touchMap.PrimaryTouchpad = RawTouch.None;
			touchMap.SecondaryIndexTrigger = RawTouch.None;
			touchMap.SecondaryThumbstick = RawTouch.None;
			touchMap.SecondaryThumbRest = RawTouch.None;
			touchMap.SecondaryTouchpad = RawTouch.None;
		}

		public override void ConfigureNearTouchMap()
		{
			nearTouchMap.None = RawNearTouch.None;
			nearTouchMap.PrimaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.PrimaryThumbButtons = RawNearTouch.None;
			nearTouchMap.SecondaryIndexTrigger = RawNearTouch.None;
			nearTouchMap.SecondaryThumbButtons = RawNearTouch.None;
		}

		public override void ConfigureAxis1DMap()
		{
			axis1DMap.None = RawAxis1D.None;
			axis1DMap.PrimaryIndexTrigger = RawAxis1D.LIndexTrigger;
			axis1DMap.PrimaryHandTrigger = RawAxis1D.None;
			axis1DMap.SecondaryIndexTrigger = RawAxis1D.RIndexTrigger;
			axis1DMap.SecondaryHandTrigger = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.PrimaryThumbRestForce = RawAxis1D.None;
			axis1DMap.PrimaryStylusForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerCurl = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerSlide = RawAxis1D.None;
			axis1DMap.SecondaryThumbRestForce = RawAxis1D.None;
			axis1DMap.SecondaryStylusForce = RawAxis1D.None;
			axis1DMap.PrimaryIndexTriggerForce = RawAxis1D.None;
			axis1DMap.SecondaryIndexTriggerForce = RawAxis1D.None;
		}

		public override void ConfigureAxis2DMap()
		{
			axis2DMap.None = RawAxis2D.None;
			axis2DMap.PrimaryThumbstick = RawAxis2D.LThumbstick;
			axis2DMap.PrimaryTouchpad = RawAxis2D.None;
			axis2DMap.SecondaryThumbstick = RawAxis2D.RThumbstick;
			axis2DMap.SecondaryTouchpad = RawAxis2D.None;
		}
	}

	public static readonly float AXIS_AS_BUTTON_THRESHOLD;

	public static readonly float AXIS_DEADZONE_THRESHOLD;

	public static List<OVRControllerBase> controllers;

	public static Controller activeControllerType;

	public static Controller connectedControllerTypes;

	public static OVRPlugin.Step stepType;

	public static int fixedUpdateCount;

	private static bool _pluginSupportsActiveController;

	private static bool _pluginSupportsActiveControllerCached;

	private static Version _pluginSupportsActiveControllerMinVersion;

	private static int NUM_HAPTIC_CHANNELS;

	private static HapticInfo[] hapticInfos;

	private static float OPENVR_MAX_HAPTIC_AMPLITUDE;

	private static float HAPTIC_VIBRATION_DURATION_SECONDS;

	private static string OPENVR_TOUCH_NAME;

	private static string OPENVR_VIVE_CONTROLLER_NAME;

	private static string OPENVR_WINDOWSMR_CONTROLLER_NAME;

	public static OpenVRControllerDetails[] openVRControllerDetails;

	public static bool pluginSupportsActiveController
	{
		get
		{
			if (!_pluginSupportsActiveControllerCached)
			{
				_pluginSupportsActiveController = true && OVRPlugin.version >= _pluginSupportsActiveControllerMinVersion;
				_pluginSupportsActiveControllerCached = true;
			}
			return _pluginSupportsActiveController;
		}
	}

	static OVRInput()
	{
		AXIS_AS_BUTTON_THRESHOLD = 0.5f;
		AXIS_DEADZONE_THRESHOLD = 0.2f;
		activeControllerType = Controller.None;
		connectedControllerTypes = Controller.None;
		stepType = OVRPlugin.Step.Render;
		fixedUpdateCount = 0;
		_pluginSupportsActiveController = false;
		_pluginSupportsActiveControllerCached = false;
		_pluginSupportsActiveControllerMinVersion = new Version(1, 9, 0);
		NUM_HAPTIC_CHANNELS = 2;
		OPENVR_MAX_HAPTIC_AMPLITUDE = 4000f;
		HAPTIC_VIBRATION_DURATION_SECONDS = 2f;
		OPENVR_TOUCH_NAME = "oculus_touch";
		OPENVR_VIVE_CONTROLLER_NAME = "vive_controller";
		OPENVR_WINDOWSMR_CONTROLLER_NAME = "holographic_controller";
		openVRControllerDetails = new OpenVRControllerDetails[2];
		controllers = new List<OVRControllerBase>
		{
			new OVRControllerGamepadPC(),
			new OVRControllerTouch(),
			new OVRControllerLTouch(),
			new OVRControllerRTouch(),
			new OVRControllerHands(),
			new OVRControllerLHand(),
			new OVRControllerRHand(),
			new OVRControllerRemote()
		};
		InitHapticInfo();
	}

	public static void Update()
	{
		connectedControllerTypes = Controller.None;
		stepType = OVRPlugin.Step.Render;
		fixedUpdateCount = 0;
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
		{
			UpdateXRControllerNodeIds();
			UpdateXRControllerHaptics();
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			connectedControllerTypes |= oVRControllerBase.Update();
			if ((connectedControllerTypes & oVRControllerBase.controllerType) != 0)
			{
				RawTouch rawMask = RawTouch.Any;
				if (Get(RawButton.Any, oVRControllerBase.controllerType) || Get(rawMask, oVRControllerBase.controllerType))
				{
					activeControllerType = oVRControllerBase.controllerType;
				}
			}
		}
		if ((activeControllerType == Controller.LTouch || activeControllerType == Controller.RTouch) && (connectedControllerTypes & Controller.Touch) == Controller.Touch)
		{
			activeControllerType = Controller.Touch;
		}
		if ((activeControllerType == Controller.LHand || activeControllerType == Controller.RHand) && (connectedControllerTypes & Controller.Hands) == Controller.Hands)
		{
			activeControllerType = Controller.Hands;
		}
		if ((connectedControllerTypes & activeControllerType) == 0)
		{
			activeControllerType = Controller.None;
		}
		if (activeControllerType == Controller.None && (connectedControllerTypes & Controller.Hands) != 0)
		{
			activeControllerType = connectedControllerTypes & Controller.Hands;
		}
		bool flag = OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus && pluginSupportsActiveController;
		if (OVRManager.instance != null && OVRManager.instance.IsSimultaneousHandsAndControllersSupported)
		{
			flag = OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus;
		}
		if (flag)
		{
			Controller controller = activeControllerType;
			connectedControllerTypes = (Controller)OVRPlugin.GetConnectedControllers();
			activeControllerType = (Controller)OVRPlugin.GetActiveController();
			if (activeControllerType == Controller.None && (controller & Controller.Hands) != 0)
			{
				activeControllerType = controller;
			}
		}
		else if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
		{
			activeControllerType = connectedControllerTypes;
		}
	}

	public static void FixedUpdate()
	{
		if (OVRPlugin.nativeXrApi != OVRPlugin.XrApi.OpenXR)
		{
			stepType = OVRPlugin.Step.Physics;
			double predictionSeconds = (double)fixedUpdateCount * (double)Time.fixedDeltaTime / (double)Mathf.Max(Time.timeScale, 1E-06f);
			fixedUpdateCount++;
			OVRPlugin.UpdateNodePhysicsPoses(0, predictionSeconds);
		}
	}

	public static InteractionProfile GetCurrentInteractionProfile(Hand hand)
	{
		return (InteractionProfile)OVRPlugin.GetCurrentInteractionProfile((OVRPlugin.Hand)hand);
	}

	public static bool GetControllerOrientationTracked(Controller controllerType)
	{
		return controllerType switch
		{
			Controller.LTouch => OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.ControllerLeft), 
			Controller.LHand => OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.HandLeft), 
			Controller.RTouch => OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.ControllerRight), 
			Controller.RHand => OVRPlugin.GetNodeOrientationTracked(OVRPlugin.Node.HandRight), 
			_ => false, 
		};
	}

	public static bool GetControllerOrientationValid(Controller controllerType)
	{
		return controllerType switch
		{
			Controller.LTouch => OVRPlugin.GetNodeOrientationValid(OVRPlugin.Node.ControllerLeft), 
			Controller.LHand => OVRPlugin.GetNodeOrientationValid(OVRPlugin.Node.HandLeft), 
			Controller.RTouch => OVRPlugin.GetNodeOrientationValid(OVRPlugin.Node.ControllerRight), 
			Controller.RHand => OVRPlugin.GetNodeOrientationValid(OVRPlugin.Node.HandRight), 
			_ => false, 
		};
	}

	public static bool GetControllerPositionTracked(Controller controllerType)
	{
		return controllerType switch
		{
			Controller.LTouch => OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.ControllerLeft), 
			Controller.LHand => OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandLeft), 
			Controller.RTouch => OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.ControllerRight), 
			Controller.RHand => OVRPlugin.GetNodePositionTracked(OVRPlugin.Node.HandRight), 
			_ => false, 
		};
	}

	public static bool GetControllerPositionValid(Controller controllerType)
	{
		return controllerType switch
		{
			Controller.LTouch => OVRPlugin.GetNodePositionValid(OVRPlugin.Node.ControllerLeft), 
			Controller.LHand => OVRPlugin.GetNodePositionValid(OVRPlugin.Node.HandLeft), 
			Controller.RTouch => OVRPlugin.GetNodePositionValid(OVRPlugin.Node.ControllerRight), 
			Controller.RHand => OVRPlugin.GetNodePositionValid(OVRPlugin.Node.HandRight), 
			_ => false, 
		};
	}

	public static bool AreHandPosesGeneratedByControllerData(OVRPlugin.Step stepId, Hand hand)
	{
		return hand switch
		{
			Hand.HandLeft => OVRPlugin.AreHandPosesGeneratedByControllerData(stepId, OVRPlugin.Node.HandLeft), 
			Hand.HandRight => OVRPlugin.AreHandPosesGeneratedByControllerData(stepId, OVRPlugin.Node.HandRight), 
			_ => false, 
		};
	}

	public static bool EnableSimultaneousHandsAndControllers()
	{
		return OVRPlugin.SetSimultaneousHandsAndControllersEnabled(enabled: true);
	}

	public static bool DisableSimultaneousHandsAndControllers()
	{
		return OVRPlugin.SetSimultaneousHandsAndControllersEnabled(enabled: false);
	}

	public static ControllerInHandState GetControllerIsInHandState(Hand hand)
	{
		switch (hand)
		{
		case Hand.HandLeft:
			if ((connectedControllerTypes & Controller.LHand) != 0)
			{
				if (OVRPlugin.GetControllerIsInHand(OVRPlugin.Step.Render, OVRPlugin.Node.ControllerLeft))
				{
					return ControllerInHandState.ControllerInHand;
				}
				return ControllerInHandState.ControllerNotInHand;
			}
			return ControllerInHandState.NoHand;
		case Hand.HandRight:
			if ((connectedControllerTypes & Controller.RHand) != 0)
			{
				if (OVRPlugin.GetControllerIsInHand(OVRPlugin.Step.Render, OVRPlugin.Node.ControllerRight))
				{
					return ControllerInHandState.ControllerInHand;
				}
				return ControllerInHandState.ControllerNotInHand;
			}
			return ControllerInHandState.NoHand;
		default:
			return ControllerInHandState.NoHand;
		}
	}

	public static Controller GetActiveControllerForHand(Handedness handedness)
	{
		switch (handedness)
		{
		case Handedness.LeftHanded:
			if ((activeControllerType & Controller.LTouch) != 0)
			{
				return Controller.LTouch;
			}
			if ((activeControllerType & Controller.LHand) != 0)
			{
				return Controller.LHand;
			}
			break;
		case Handedness.RightHanded:
			if ((activeControllerType & Controller.RTouch) != 0)
			{
				return Controller.RTouch;
			}
			if ((activeControllerType & Controller.RHand) != 0)
			{
				return Controller.RHand;
			}
			break;
		default:
			return Controller.None;
		}
		return Controller.None;
	}

	public static Vector3 GetLocalControllerPosition(Controller controllerType)
	{
		switch (controllerType)
		{
		case Controller.LTouch:
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				return OVRPlugin.GetNodePose(OVRPlugin.Node.ControllerLeft, stepType).ToOVRPose().position;
			}
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				return openVRControllerDetails[0].localPosition;
			}
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.Position, OVRPlugin.Node.ControllerLeft, stepType, out var retVec4))
			{
				return retVec4;
			}
			return Vector3.zero;
		}
		case Controller.LHand:
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				return OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, stepType).ToOVRPose().position;
			}
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				return openVRControllerDetails[0].localPosition;
			}
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandLeft, stepType, out var retVec3))
			{
				return retVec3;
			}
			return Vector3.zero;
		}
		case Controller.RTouch:
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				return OVRPlugin.GetNodePose(OVRPlugin.Node.ControllerRight, stepType).ToOVRPose().position;
			}
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				return openVRControllerDetails[1].localPosition;
			}
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.Position, OVRPlugin.Node.ControllerRight, stepType, out var retVec2))
			{
				return retVec2;
			}
			return Vector3.zero;
		}
		case Controller.RHand:
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				return OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, stepType).ToOVRPose().position;
			}
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				return openVRControllerDetails[1].localPosition;
			}
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.Position, OVRPlugin.Node.HandRight, stepType, out var retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		}
		default:
			return Vector3.zero;
		}
	}

	public static Vector3 GetLocalControllerVelocity(Controller controllerType)
	{
		Vector3 retVec = Vector3.zero;
		switch (controllerType)
		{
		case Controller.LTouch:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.Velocity, OVRPlugin.Node.ControllerLeft, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.LHand:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.Velocity, OVRPlugin.Node.HandLeft, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.RTouch:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.Velocity, OVRPlugin.Node.ControllerRight, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.RHand:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.Velocity, OVRPlugin.Node.HandRight, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		default:
			return Vector3.zero;
		}
	}

	[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
	public static Vector3 GetLocalControllerAcceleration(Controller controllerType)
	{
		Vector3 retVec = Vector3.zero;
		switch (controllerType)
		{
		case Controller.LTouch:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.Acceleration, OVRPlugin.Node.ControllerLeft, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.LHand:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.Acceleration, OVRPlugin.Node.HandLeft, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.RTouch:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.Acceleration, OVRPlugin.Node.ControllerRight, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.RHand:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.Acceleration, OVRPlugin.Node.HandRight, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		default:
			return Vector3.zero;
		}
	}

	public static Quaternion GetLocalControllerRotation(Controller controllerType)
	{
		switch (controllerType)
		{
		case Controller.LTouch:
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				return OVRPlugin.GetNodePose(OVRPlugin.Node.ControllerLeft, stepType).ToOVRPose().orientation;
			}
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				return openVRControllerDetails[0].localOrientation;
			}
			if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.LeftHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.ControllerLeft, stepType, out var retQuat4))
			{
				return retQuat4;
			}
			return Quaternion.identity;
		}
		case Controller.LHand:
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				return OVRPlugin.GetNodePose(OVRPlugin.Node.HandLeft, stepType).ToOVRPose().orientation;
			}
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				return openVRControllerDetails[0].localOrientation;
			}
			if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.LeftHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.HandLeft, stepType, out var retQuat3))
			{
				return retQuat3;
			}
			return Quaternion.identity;
		}
		case Controller.RTouch:
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				return OVRPlugin.GetNodePose(OVRPlugin.Node.ControllerRight, stepType).ToOVRPose().orientation;
			}
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				return openVRControllerDetails[1].localOrientation;
			}
			if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.RightHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.ControllerRight, stepType, out var retQuat2))
			{
				return retQuat2;
			}
			return Quaternion.identity;
		}
		case Controller.RHand:
		{
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
			{
				return OVRPlugin.GetNodePose(OVRPlugin.Node.HandRight, stepType).ToOVRPose().orientation;
			}
			if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR)
			{
				return openVRControllerDetails[1].localOrientation;
			}
			if (OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.RightHand, NodeStatePropertyType.Orientation, OVRPlugin.Node.HandRight, stepType, out var retQuat))
			{
				return retQuat;
			}
			return Quaternion.identity;
		}
		default:
			return Quaternion.identity;
		}
	}

	public static Vector3 GetLocalControllerAngularVelocity(Controller controllerType)
	{
		Vector3 retVec = Vector3.zero;
		switch (controllerType)
		{
		case Controller.LTouch:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.AngularVelocity, OVRPlugin.Node.ControllerLeft, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.LHand:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.AngularVelocity, OVRPlugin.Node.HandLeft, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.RTouch:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.AngularVelocity, OVRPlugin.Node.ControllerRight, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.RHand:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.AngularVelocity, OVRPlugin.Node.HandRight, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		default:
			return Vector3.zero;
		}
	}

	[Obsolete("Deprecated. Acceleration is not supported in OpenXR", false)]
	public static Vector3 GetLocalControllerAngularAcceleration(Controller controllerType)
	{
		Vector3 retVec = Vector3.zero;
		switch (controllerType)
		{
		case Controller.LTouch:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.AngularAcceleration, OVRPlugin.Node.ControllerLeft, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.LHand:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.LeftHand, NodeStatePropertyType.AngularAcceleration, OVRPlugin.Node.HandLeft, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.RTouch:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.AngularAcceleration, OVRPlugin.Node.ControllerRight, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		case Controller.RHand:
			if (OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.RightHand, NodeStatePropertyType.AngularAcceleration, OVRPlugin.Node.HandRight, stepType, out retVec))
			{
				return retVec;
			}
			return Vector3.zero;
		default:
			return Vector3.zero;
		}
	}

	public static bool GetLocalControllerStatesWithoutPrediction(Controller controllerType, out Vector3 position, out Quaternion rotation, out Vector3 velocity, out Vector3 angularVelocity)
	{
		position = Vector3.zero;
		rotation = Quaternion.identity;
		velocity = Vector3.zero;
		angularVelocity = Vector3.zero;
		if (OVRManager.loadedXRDevice != OVRManager.XRDevice.Oculus)
		{
			return false;
		}
		if (OVRPlugin.nativeXrApi != OVRPlugin.XrApi.OpenXR)
		{
			return false;
		}
		OVRPlugin.PoseStatef nodePoseStateImmediate;
		switch (controllerType)
		{
		case Controller.LTouch:
			nodePoseStateImmediate = OVRPlugin.GetNodePoseStateImmediate(OVRPlugin.Node.ControllerLeft);
			break;
		case Controller.LHand:
			nodePoseStateImmediate = OVRPlugin.GetNodePoseStateImmediate(OVRPlugin.Node.HandLeft);
			break;
		case Controller.RTouch:
			nodePoseStateImmediate = OVRPlugin.GetNodePoseStateImmediate(OVRPlugin.Node.ControllerRight);
			break;
		case Controller.RHand:
			nodePoseStateImmediate = OVRPlugin.GetNodePoseStateImmediate(OVRPlugin.Node.HandRight);
			break;
		default:
			return false;
		}
		if (GetControllerPositionValid(controllerType))
		{
			position = nodePoseStateImmediate.Pose.ToOVRPose().position;
			velocity = nodePoseStateImmediate.Velocity.FromFlippedZVector3f();
		}
		if (GetControllerOrientationValid(controllerType))
		{
			rotation = nodePoseStateImmediate.Pose.ToOVRPose().orientation;
			angularVelocity = nodePoseStateImmediate.AngularVelocity.FromFlippedZVector3f();
		}
		return true;
	}

	public static Handedness GetDominantHand()
	{
		return (Handedness)OVRPlugin.GetDominantHand();
	}

	public static bool Get(Button virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButton(virtualMask, RawButton.None, controllerMask);
	}

	public static bool Get(RawButton rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButton(Button.None, rawMask, controllerMask);
	}

	private static bool GetResolvedButton(Button virtualMask, RawButton rawMask, Controller controllerMask)
	{
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawButton rawButton = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.currentState.Buttons & (uint)rawButton) != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool GetDown(Button virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButtonDown(virtualMask, RawButton.None, controllerMask);
	}

	public static bool GetDown(RawButton rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButtonDown(Button.None, rawMask, controllerMask);
	}

	private static bool GetResolvedButtonDown(Button virtualMask, RawButton rawMask, Controller controllerMask)
	{
		bool result = false;
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawButton rawButton = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.previousState.Buttons & (uint)rawButton) != 0)
				{
					return false;
				}
				if ((oVRControllerBase.currentState.Buttons & (uint)rawButton) != 0 && (oVRControllerBase.previousState.Buttons & (uint)rawButton) == 0)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public static bool GetUp(Button virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButtonUp(virtualMask, RawButton.None, controllerMask);
	}

	public static bool GetUp(RawButton rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedButtonUp(Button.None, rawMask, controllerMask);
	}

	private static bool GetResolvedButtonUp(Button virtualMask, RawButton rawMask, Controller controllerMask)
	{
		bool result = false;
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawButton rawButton = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.currentState.Buttons & (uint)rawButton) != 0)
				{
					return false;
				}
				if ((oVRControllerBase.currentState.Buttons & (uint)rawButton) == 0 && (oVRControllerBase.previousState.Buttons & (uint)rawButton) != 0)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public static bool Get(Touch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouch(virtualMask, RawTouch.None, controllerMask);
	}

	public static bool Get(RawTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouch(Touch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedTouch(Touch virtualMask, RawTouch rawMask, Controller controllerMask)
	{
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawTouch rawTouch = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.currentState.Touches & (uint)rawTouch) != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool GetDown(Touch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouchDown(virtualMask, RawTouch.None, controllerMask);
	}

	public static bool GetDown(RawTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouchDown(Touch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedTouchDown(Touch virtualMask, RawTouch rawMask, Controller controllerMask)
	{
		bool result = false;
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawTouch rawTouch = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.previousState.Touches & (uint)rawTouch) != 0)
				{
					return false;
				}
				if ((oVRControllerBase.currentState.Touches & (uint)rawTouch) != 0 && (oVRControllerBase.previousState.Touches & (uint)rawTouch) == 0)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public static bool GetUp(Touch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouchUp(virtualMask, RawTouch.None, controllerMask);
	}

	public static bool GetUp(RawTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedTouchUp(Touch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedTouchUp(Touch virtualMask, RawTouch rawMask, Controller controllerMask)
	{
		bool result = false;
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawTouch rawTouch = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.currentState.Touches & (uint)rawTouch) != 0)
				{
					return false;
				}
				if ((oVRControllerBase.currentState.Touches & (uint)rawTouch) == 0 && (oVRControllerBase.previousState.Touches & (uint)rawTouch) != 0)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public static bool Get(NearTouch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouch(virtualMask, RawNearTouch.None, controllerMask);
	}

	public static bool Get(RawNearTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouch(NearTouch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedNearTouch(NearTouch virtualMask, RawNearTouch rawMask, Controller controllerMask)
	{
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawNearTouch rawNearTouch = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.currentState.NearTouches & (uint)rawNearTouch) != 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool GetDown(NearTouch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouchDown(virtualMask, RawNearTouch.None, controllerMask);
	}

	public static bool GetDown(RawNearTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouchDown(NearTouch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedNearTouchDown(NearTouch virtualMask, RawNearTouch rawMask, Controller controllerMask)
	{
		bool result = false;
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawNearTouch rawNearTouch = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.previousState.NearTouches & (uint)rawNearTouch) != 0)
				{
					return false;
				}
				if ((oVRControllerBase.currentState.NearTouches & (uint)rawNearTouch) != 0 && (oVRControllerBase.previousState.NearTouches & (uint)rawNearTouch) == 0)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public static bool GetUp(NearTouch virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouchUp(virtualMask, RawNearTouch.None, controllerMask);
	}

	public static bool GetUp(RawNearTouch rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedNearTouchUp(NearTouch.None, rawMask, controllerMask);
	}

	private static bool GetResolvedNearTouchUp(NearTouch virtualMask, RawNearTouch rawMask, Controller controllerMask)
	{
		bool result = false;
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				RawNearTouch rawNearTouch = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
				if ((oVRControllerBase.currentState.NearTouches & (uint)rawNearTouch) != 0)
				{
					return false;
				}
				if ((oVRControllerBase.currentState.NearTouches & (uint)rawNearTouch) == 0 && (oVRControllerBase.previousState.NearTouches & (uint)rawNearTouch) != 0)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public static float Get(Axis1D virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedAxis1D(virtualMask, RawAxis1D.None, controllerMask);
	}

	public static float Get(RawAxis1D rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedAxis1D(Axis1D.None, rawMask, controllerMask);
	}

	private static float GetResolvedAxis1D(Axis1D virtualMask, RawAxis1D rawMask, Controller controllerMask)
	{
		float num = 0f;
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (OVRManager.loadedXRDevice != OVRManager.XRDevice.Oculus)
			{
				oVRControllerBase.shouldApplyDeadzone = false;
			}
			if (!ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				continue;
			}
			RawAxis1D rawAxis1D = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
			if ((RawAxis1D.LIndexTrigger & rawAxis1D) != 0)
			{
				float num2 = oVRControllerBase.currentState.LIndexTrigger;
				if (oVRControllerBase.shouldApplyDeadzone)
				{
					num2 = CalculateDeadzone(num2, AXIS_DEADZONE_THRESHOLD);
				}
				num = CalculateAbsMax(num, num2);
			}
			if ((RawAxis1D.RIndexTrigger & rawAxis1D) != 0)
			{
				float num3 = oVRControllerBase.currentState.RIndexTrigger;
				if (oVRControllerBase.shouldApplyDeadzone)
				{
					num3 = CalculateDeadzone(num3, AXIS_DEADZONE_THRESHOLD);
				}
				num = CalculateAbsMax(num, num3);
			}
			if ((RawAxis1D.LHandTrigger & rawAxis1D) != 0)
			{
				float num4 = oVRControllerBase.currentState.LHandTrigger;
				if (oVRControllerBase.shouldApplyDeadzone)
				{
					num4 = CalculateDeadzone(num4, AXIS_DEADZONE_THRESHOLD);
				}
				num = CalculateAbsMax(num, num4);
			}
			if ((RawAxis1D.RHandTrigger & rawAxis1D) != 0)
			{
				float num5 = oVRControllerBase.currentState.RHandTrigger;
				if (oVRControllerBase.shouldApplyDeadzone)
				{
					num5 = CalculateDeadzone(num5, AXIS_DEADZONE_THRESHOLD);
				}
				num = CalculateAbsMax(num, num5);
			}
			if ((RawAxis1D.LIndexTriggerCurl & rawAxis1D) != 0)
			{
				float lIndexTriggerCurl = oVRControllerBase.currentState.LIndexTriggerCurl;
				num = CalculateAbsMax(num, lIndexTriggerCurl);
			}
			if ((RawAxis1D.RIndexTriggerCurl & rawAxis1D) != 0)
			{
				float rIndexTriggerCurl = oVRControllerBase.currentState.RIndexTriggerCurl;
				num = CalculateAbsMax(num, rIndexTriggerCurl);
			}
			if ((RawAxis1D.LIndexTriggerSlide & rawAxis1D) != 0)
			{
				float lIndexTriggerSlide = oVRControllerBase.currentState.LIndexTriggerSlide;
				num = CalculateAbsMax(num, lIndexTriggerSlide);
			}
			if ((RawAxis1D.RIndexTriggerSlide & rawAxis1D) != 0)
			{
				float rIndexTriggerSlide = oVRControllerBase.currentState.RIndexTriggerSlide;
				num = CalculateAbsMax(num, rIndexTriggerSlide);
			}
			if ((RawAxis1D.LThumbRestForce & rawAxis1D) != 0)
			{
				float lThumbRestForce = oVRControllerBase.currentState.LThumbRestForce;
				num = CalculateAbsMax(num, lThumbRestForce);
			}
			if ((RawAxis1D.RThumbRestForce & rawAxis1D) != 0)
			{
				float rThumbRestForce = oVRControllerBase.currentState.RThumbRestForce;
				num = CalculateAbsMax(num, rThumbRestForce);
			}
			if ((RawAxis1D.LStylusForce & rawAxis1D) != 0)
			{
				float lStylusForce = oVRControllerBase.currentState.LStylusForce;
				num = CalculateAbsMax(num, lStylusForce);
			}
			if ((RawAxis1D.RStylusForce & rawAxis1D) != 0)
			{
				float rStylusForce = oVRControllerBase.currentState.RStylusForce;
				num = CalculateAbsMax(num, rStylusForce);
			}
			if ((RawAxis1D.LIndexTriggerForce & rawAxis1D) != 0)
			{
				float lIndexTriggerForce = oVRControllerBase.currentState.LIndexTriggerForce;
				num = CalculateAbsMax(num, lIndexTriggerForce);
			}
			if ((RawAxis1D.RIndexTriggerForce & rawAxis1D) != 0)
			{
				float rIndexTriggerForce = oVRControllerBase.currentState.RIndexTriggerForce;
				num = CalculateAbsMax(num, rIndexTriggerForce);
			}
		}
		return num;
	}

	public static Vector2 Get(Axis2D virtualMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedAxis2D(virtualMask, RawAxis2D.None, controllerMask);
	}

	public static Vector2 Get(RawAxis2D rawMask, Controller controllerMask = Controller.Active)
	{
		return GetResolvedAxis2D(Axis2D.None, rawMask, controllerMask);
	}

	private static Vector2 GetResolvedAxis2D(Axis2D virtualMask, RawAxis2D rawMask, Controller controllerMask)
	{
		Vector2 vector = Vector2.zero;
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (OVRManager.loadedXRDevice != OVRManager.XRDevice.Oculus)
			{
				oVRControllerBase.shouldApplyDeadzone = false;
			}
			if (!ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				continue;
			}
			RawAxis2D rawAxis2D = rawMask | oVRControllerBase.ResolveToRawMask(virtualMask);
			if ((RawAxis2D.LThumbstick & rawAxis2D) != 0)
			{
				Vector2 vector2 = new Vector2(oVRControllerBase.currentState.LThumbstick.x, oVRControllerBase.currentState.LThumbstick.y);
				if (oVRControllerBase.shouldApplyDeadzone)
				{
					vector2 = CalculateDeadzone(vector2, AXIS_DEADZONE_THRESHOLD);
				}
				vector = CalculateAbsMax(vector, vector2);
			}
			if ((RawAxis2D.LTouchpad & rawAxis2D) != 0)
			{
				Vector2 b = new Vector2(oVRControllerBase.currentState.LTouchpad.x, oVRControllerBase.currentState.LTouchpad.y);
				vector = CalculateAbsMax(vector, b);
			}
			if ((RawAxis2D.RThumbstick & rawAxis2D) != 0)
			{
				Vector2 vector3 = new Vector2(oVRControllerBase.currentState.RThumbstick.x, oVRControllerBase.currentState.RThumbstick.y);
				if (oVRControllerBase.shouldApplyDeadzone)
				{
					vector3 = CalculateDeadzone(vector3, AXIS_DEADZONE_THRESHOLD);
				}
				vector = CalculateAbsMax(vector, vector3);
			}
			if ((RawAxis2D.RTouchpad & rawAxis2D) != 0)
			{
				Vector2 b2 = new Vector2(oVRControllerBase.currentState.RTouchpad.x, oVRControllerBase.currentState.RTouchpad.y);
				vector = CalculateAbsMax(vector, b2);
			}
		}
		return vector;
	}

	public static Controller GetConnectedControllers()
	{
		return connectedControllerTypes;
	}

	public static bool IsControllerConnected(Controller controller)
	{
		return (connectedControllerTypes & controller) == controller;
	}

	public static Controller GetActiveController()
	{
		return activeControllerType;
	}

	private static void StartVibration(float amplitude, float duration, XRNode controllerNode)
	{
		int num = ((controllerNode != XRNode.LeftHand) ? 1 : 0);
		hapticInfos[num].hapticsDurationPlayed = 0f;
		hapticInfos[num].hapticAmplitude = amplitude;
		hapticInfos[num].hapticsDuration = duration;
		hapticInfos[num].playingHaptics = amplitude != 0f;
		hapticInfos[num].node = controllerNode;
		if (amplitude <= 0f || duration <= 0f)
		{
			hapticInfos[num].playingHaptics = false;
		}
	}

	public static void SetOpenVRLocalPose(Vector3 leftPos, Vector3 rightPos, Quaternion leftRot, Quaternion rightRot)
	{
		openVRControllerDetails[0].localPosition = leftPos;
		openVRControllerDetails[0].localOrientation = leftRot;
		openVRControllerDetails[1].localPosition = rightPos;
		openVRControllerDetails[1].localOrientation = rightRot;
	}

	public static string GetOpenVRStringProperty(ETrackedDeviceProperty prop, uint deviceId = 0u)
	{
		ETrackedPropertyError pError = ETrackedPropertyError.TrackedProp_Success;
		CVRSystem system = OpenVR.System;
		if (system != null)
		{
			uint stringTrackedDeviceProperty = system.GetStringTrackedDeviceProperty(deviceId, prop, null, 0u, ref pError);
			if (stringTrackedDeviceProperty > 1)
			{
				StringBuilder stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
				system.GetStringTrackedDeviceProperty(deviceId, prop, stringBuilder, stringTrackedDeviceProperty, ref pError);
				return stringBuilder.ToString();
			}
			if (pError == ETrackedPropertyError.TrackedProp_Success)
			{
				return "<unknown>";
			}
			return pError.ToString();
		}
		return "";
	}

	private static void UpdateXRControllerNodeIds()
	{
		if (OVRManager.loadedXRDevice != OVRManager.XRDevice.OpenVR)
		{
			return;
		}
		openVRControllerDetails[0].deviceID = 64u;
		openVRControllerDetails[1].deviceID = 64u;
		CVRSystem system = OpenVR.System;
		if (system == null)
		{
			return;
		}
		for (uint num = 0u; num < 64; num++)
		{
			if (system.GetTrackedDeviceClass(num) == ETrackedDeviceClass.Controller && system.IsTrackedDeviceConnected(num))
			{
				string openVRStringProperty = GetOpenVRStringProperty(ETrackedDeviceProperty.Prop_ControllerType_String, num);
				OpenVRController controllerType = (OpenVRController)((openVRStringProperty == OPENVR_TOUCH_NAME) ? 1 : ((openVRStringProperty == OPENVR_VIVE_CONTROLLER_NAME) ? 2 : ((!(openVRStringProperty == OPENVR_WINDOWSMR_CONTROLLER_NAME)) ? 0 : 3)));
				switch (system.GetControllerRoleForTrackedDeviceIndex(num))
				{
				case ETrackedControllerRole.LeftHand:
					system.GetControllerState(num, ref openVRControllerDetails[0].state, (uint)Marshal.SizeOf(typeof(VRControllerState_t)));
					openVRControllerDetails[0].deviceID = num;
					openVRControllerDetails[0].controllerType = controllerType;
					connectedControllerTypes |= Controller.LTouch;
					break;
				case ETrackedControllerRole.RightHand:
					system.GetControllerState(num, ref openVRControllerDetails[1].state, (uint)Marshal.SizeOf(typeof(VRControllerState_t)));
					openVRControllerDetails[1].deviceID = num;
					openVRControllerDetails[1].controllerType = controllerType;
					connectedControllerTypes |= Controller.RTouch;
					break;
				}
			}
		}
	}

	private static void UpdateXRControllerHaptics()
	{
		if (OVRManager.loadedXRDevice != OVRManager.XRDevice.OpenVR)
		{
			return;
		}
		for (int i = 0; i < NUM_HAPTIC_CHANNELS; i++)
		{
			if (hapticInfos[i].playingHaptics)
			{
				hapticInfos[i].hapticsDurationPlayed += Time.deltaTime;
				PlayHapticImpulse(hapticInfos[i].hapticAmplitude, hapticInfos[i].node);
				if (hapticInfos[i].hapticsDurationPlayed >= hapticInfos[i].hapticsDuration)
				{
					hapticInfos[i].playingHaptics = false;
				}
			}
		}
	}

	private static void InitHapticInfo()
	{
		hapticInfos = new HapticInfo[NUM_HAPTIC_CHANNELS];
		for (int i = 0; i < NUM_HAPTIC_CHANNELS; i++)
		{
			hapticInfos[i] = new HapticInfo();
		}
	}

	private static void PlayHapticImpulse(float amplitude, XRNode deviceNode)
	{
		CVRSystem system = OpenVR.System;
		if (system != null && amplitude != 0f)
		{
			uint num = ((deviceNode == XRNode.LeftHand) ? openVRControllerDetails[0].deviceID : openVRControllerDetails[1].deviceID);
			if (IsValidOpenVRDevice(num))
			{
				system.TriggerHapticPulse(num, 0u, (ushort)(OPENVR_MAX_HAPTIC_AMPLITUDE * amplitude));
			}
		}
	}

	private static bool IsValidOpenVRDevice(uint deviceId)
	{
		if (deviceId >= 0)
		{
			return deviceId < 64;
		}
		return false;
	}

	public static void SetControllerVibration(float frequency, float amplitude, Controller controllerMask = Controller.Active)
	{
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
		{
			if (((uint)controllerMask & 0x80000000u) != 0)
			{
				controllerMask |= activeControllerType;
			}
			for (int i = 0; i < controllers.Count; i++)
			{
				OVRControllerBase oVRControllerBase = controllers[i];
				if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
				{
					oVRControllerBase.SetControllerVibration(frequency, amplitude);
				}
			}
		}
		else if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR && (controllerMask == Controller.LTouch || controllerMask == Controller.RTouch))
		{
			XRNode controllerNode = ((controllerMask == Controller.LTouch) ? XRNode.LeftHand : XRNode.RightHand);
			StartVibration(amplitude, HAPTIC_VIBRATION_DURATION_SECONDS, controllerNode);
		}
	}

	public static void SetControllerLocalizedVibration(HapticsLocation hapticsLocationMask, float frequency, float amplitude, Controller controllerMask = Controller.Active)
	{
		if (OVRManager.loadedXRDevice == OVRManager.XRDevice.Oculus)
		{
			if (((uint)controllerMask & 0x80000000u) != 0)
			{
				controllerMask |= activeControllerType;
			}
			for (int i = 0; i < controllers.Count; i++)
			{
				OVRControllerBase oVRControllerBase = controllers[i];
				if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
				{
					oVRControllerBase.SetControllerLocalizedVibration(hapticsLocationMask, frequency, amplitude);
				}
			}
		}
		else if (OVRManager.loadedXRDevice == OVRManager.XRDevice.OpenVR && (hapticsLocationMask & HapticsLocation.Hand) != 0)
		{
			SetControllerVibration(frequency, amplitude, controllerMask);
		}
	}

	public static void SetControllerHapticsAmplitudeEnvelope(HapticsAmplitudeEnvelopeVibration hapticsVibration, Controller controllerMask = Controller.Active)
	{
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				oVRControllerBase.SetControllerHapticsAmplitudeEnvelope(hapticsVibration);
			}
		}
	}

	public static int SetControllerHapticsPcm(HapticsPcmVibration hapticsVibration, Controller controllerMask = Controller.Active)
	{
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		int result = 0;
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				result = oVRControllerBase.SetControllerHapticsPcm(hapticsVibration);
			}
		}
		return result;
	}

	public static float GetControllerSampleRateHz(Controller controllerMask = Controller.Active)
	{
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				return oVRControllerBase.GetControllerSampleRateHz();
			}
		}
		return 0f;
	}

	[Obsolete("Deprecated. The controller battery percentage data is no longer supported in OpenXR", false)]
	public static byte GetControllerBatteryPercentRemaining(Controller controllerMask = Controller.Active)
	{
		if (((uint)controllerMask & 0x80000000u) != 0)
		{
			controllerMask |= activeControllerType;
		}
		byte result = 0;
		for (int i = 0; i < controllers.Count; i++)
		{
			OVRControllerBase oVRControllerBase = controllers[i];
			if (ShouldResolveController(oVRControllerBase.controllerType, controllerMask))
			{
				result = oVRControllerBase.GetBatteryPercentRemaining();
				break;
			}
		}
		return result;
	}

	private static Vector2 CalculateAbsMax(Vector2 a, Vector2 b)
	{
		float sqrMagnitude = a.sqrMagnitude;
		float sqrMagnitude2 = b.sqrMagnitude;
		if (sqrMagnitude >= sqrMagnitude2)
		{
			return a;
		}
		return b;
	}

	private static float CalculateAbsMax(float a, float b)
	{
		float num = ((a >= 0f) ? a : (0f - a));
		float num2 = ((b >= 0f) ? b : (0f - b));
		if (num >= num2)
		{
			return a;
		}
		return b;
	}

	private static Vector2 CalculateDeadzone(Vector2 a, float deadzone)
	{
		if (a.sqrMagnitude <= deadzone * deadzone)
		{
			return Vector2.zero;
		}
		a *= (a.magnitude - deadzone) / (1f - deadzone);
		if (a.sqrMagnitude > 1f)
		{
			return a.normalized;
		}
		return a;
	}

	private static float CalculateDeadzone(float a, float deadzone)
	{
		float num = ((a >= 0f) ? a : (0f - a));
		if (num <= deadzone)
		{
			return 0f;
		}
		a *= (num - deadzone) / (1f - deadzone);
		if (a * a > 1f)
		{
			if (!(a >= 0f))
			{
				return -1f;
			}
			return 1f;
		}
		return a;
	}

	private static bool ShouldResolveController(Controller controllerType, Controller controllerMask)
	{
		bool result = false;
		if ((controllerType & controllerMask) == controllerType)
		{
			result = true;
		}
		if ((controllerMask & Controller.Touch) == Controller.Touch && (controllerType & Controller.Touch) != 0 && (controllerType & Controller.Touch) != Controller.Touch)
		{
			result = false;
		}
		if ((controllerMask & Controller.Hands) == Controller.Hands && (controllerType & Controller.Hands) != 0 && (controllerType & Controller.Hands) != Controller.Hands)
		{
			result = false;
		}
		return result;
	}
}
