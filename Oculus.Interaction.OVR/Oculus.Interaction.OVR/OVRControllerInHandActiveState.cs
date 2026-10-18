using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.OVR;

[Feature(Feature.Interaction)]
public class OVRControllerInHandActiveState : MonoBehaviour, IActiveState
{
	[SerializeField]
	private OVRInput.Hand _handType;

	[SerializeField]
	[Tooltip("Determines if the ActiveState should be enabled or disabled when the hand is grabbing a controller")]
	[HelpBox("Ensure you have enabled ConformingHandsToControllers or/and Concurrent Hands/Controller Support in the OVRCameraRig.ControllerDrivenHandPosesType and that the OVRHand component ShowState is as permissive as this.", HelpBoxAttribute.MessageType.Info, OVRInput.InputDeviceShowState.ControllerNotInHand, ConditionalHideAttribute.DisplayMode.HideIfTrue)]
	private OVRInput.InputDeviceShowState _showState = OVRInput.InputDeviceShowState.ControllerNotInHand;

	public OVRInput.Hand HandType
	{
		get
		{
			return _handType;
		}
		set
		{
			_handType = value;
		}
	}

	public OVRInput.InputDeviceShowState ShowState
	{
		get
		{
			return _showState;
		}
		set
		{
			_showState = value;
		}
	}

	public bool Active
	{
		get
		{
			OVRInput.ControllerInHandState controllerIsInHandState = OVRInput.GetControllerIsInHandState(_handType);
			switch (_showState)
			{
			case OVRInput.InputDeviceShowState.Always:
				return true;
			case OVRInput.InputDeviceShowState.ControllerInHand:
				return controllerIsInHandState == OVRInput.ControllerInHandState.ControllerInHand;
			case OVRInput.InputDeviceShowState.ControllerInHandOrNoHand:
				if (controllerIsInHandState != OVRInput.ControllerInHandState.ControllerInHand)
				{
					return controllerIsInHandState == OVRInput.ControllerInHandState.NoHand;
				}
				return true;
			case OVRInput.InputDeviceShowState.ControllerNotInHand:
				return controllerIsInHandState == OVRInput.ControllerInHandState.ControllerNotInHand;
			case OVRInput.InputDeviceShowState.NoHand:
				return controllerIsInHandState == OVRInput.ControllerInHandState.NoHand;
			default:
				return false;
			}
		}
	}
}
