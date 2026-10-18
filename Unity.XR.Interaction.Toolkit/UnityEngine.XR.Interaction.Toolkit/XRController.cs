using System;
using UnityEngine.Experimental.XR.Interaction;
using UnityEngine.SpatialTracking;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("/XR Controller (Device-based)", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRController.html")]
[Obsolete("XRController has been deprecated in version 3.0.0. Its functionality has been distributed into different components.")]
public class XRController : XRBaseController
{
	[SerializeField]
	private XRNode m_ControllerNode = XRNode.RightHand;

	private XRNode m_InputDeviceControllerNode;

	[SerializeField]
	private InputHelpers.Button m_SelectUsage = InputHelpers.Button.Grip;

	[SerializeField]
	private InputHelpers.Button m_ActivateUsage = InputHelpers.Button.Trigger;

	[SerializeField]
	private InputHelpers.Button m_UIPressUsage = InputHelpers.Button.Trigger;

	[SerializeField]
	private float m_AxisToPressThreshold = 0.1f;

	[SerializeField]
	private InputHelpers.Button m_RotateAnchorLeft = InputHelpers.Button.PrimaryAxis2DLeft;

	[SerializeField]
	private InputHelpers.Button m_RotateAnchorRight = InputHelpers.Button.PrimaryAxis2DRight;

	[SerializeField]
	private InputHelpers.Button m_MoveObjectIn = InputHelpers.Button.PrimaryAxis2DUp;

	[SerializeField]
	private InputHelpers.Button m_MoveObjectOut = InputHelpers.Button.PrimaryAxis2DDown;

	[SerializeField]
	private InputHelpers.Axis2D m_DirectionalAnchorRotation = InputHelpers.Axis2D.PrimaryAxis2D;

	[SerializeField]
	private BasePoseProvider m_PoseProvider;

	private InputDevice m_InputDevice;

	public XRNode controllerNode
	{
		get
		{
			return m_ControllerNode;
		}
		set
		{
			m_ControllerNode = value;
		}
	}

	public InputHelpers.Button selectUsage
	{
		get
		{
			return m_SelectUsage;
		}
		set
		{
			m_SelectUsage = value;
		}
	}

	public InputHelpers.Button activateUsage
	{
		get
		{
			return m_ActivateUsage;
		}
		set
		{
			m_ActivateUsage = value;
		}
	}

	public InputHelpers.Button uiPressUsage
	{
		get
		{
			return m_UIPressUsage;
		}
		set
		{
			m_UIPressUsage = value;
		}
	}

	public float axisToPressThreshold
	{
		get
		{
			return m_AxisToPressThreshold;
		}
		set
		{
			m_AxisToPressThreshold = value;
		}
	}

	public InputHelpers.Button rotateObjectLeft
	{
		get
		{
			return m_RotateAnchorLeft;
		}
		set
		{
			m_RotateAnchorLeft = value;
		}
	}

	public InputHelpers.Button rotateObjectRight
	{
		get
		{
			return m_RotateAnchorRight;
		}
		set
		{
			m_RotateAnchorRight = value;
		}
	}

	public InputHelpers.Button moveObjectIn
	{
		get
		{
			return m_MoveObjectIn;
		}
		set
		{
			m_MoveObjectIn = value;
		}
	}

	public InputHelpers.Button moveObjectOut
	{
		get
		{
			return m_MoveObjectOut;
		}
		set
		{
			m_MoveObjectOut = value;
		}
	}

	public InputHelpers.Axis2D directionalAnchorRotation
	{
		get
		{
			return m_DirectionalAnchorRotation;
		}
		set
		{
			m_DirectionalAnchorRotation = value;
		}
	}

	public BasePoseProvider poseProvider
	{
		get
		{
			return m_PoseProvider;
		}
		set
		{
			m_PoseProvider = value;
		}
	}

	public InputDevice inputDevice
	{
		get
		{
			if (m_InputDeviceControllerNode != m_ControllerNode || !m_InputDevice.isValid)
			{
				m_InputDevice = InputDevices.GetDeviceAtXRNode(m_ControllerNode);
				m_InputDeviceControllerNode = m_ControllerNode;
			}
			return m_InputDevice;
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void UpdateTrackingInput(XRControllerState controllerState)
	{
		base.UpdateTrackingInput(controllerState);
		if (controllerState == null)
		{
			return;
		}
		controllerState.isTracked = inputDevice.TryGetFeatureValue(CommonUsages.isTracked, out var value) && value;
		controllerState.inputTrackingState = InputTrackingState.None;
		InputTrackingState value2;
		if (m_PoseProvider != null)
		{
			Pose output;
			PoseDataFlags poseFromProvider = m_PoseProvider.GetPoseFromProvider(out output);
			if ((poseFromProvider & PoseDataFlags.Position) != 0)
			{
				controllerState.position = output.position;
				controllerState.inputTrackingState |= InputTrackingState.Position;
			}
			if ((poseFromProvider & PoseDataFlags.Rotation) != 0)
			{
				controllerState.rotation = output.rotation;
				controllerState.inputTrackingState |= InputTrackingState.Rotation;
			}
		}
		else if (inputDevice.TryGetFeatureValue(CommonUsages.trackingState, out value2))
		{
			controllerState.inputTrackingState = value2;
			if ((value2 & InputTrackingState.Position) != 0 && inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var value3))
			{
				controllerState.position = value3;
			}
			if ((value2 & InputTrackingState.Rotation) != 0 && inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out var value4))
			{
				controllerState.rotation = value4;
			}
		}
	}

	protected override void UpdateInput(XRControllerState controllerState)
	{
		base.UpdateInput(controllerState);
		if (controllerState != null)
		{
			controllerState.ResetFrameDependentStates();
			controllerState.selectInteractionState.SetFrameState(IsPressed(m_SelectUsage), ReadValue(m_SelectUsage));
			controllerState.activateInteractionState.SetFrameState(IsPressed(m_ActivateUsage), ReadValue(m_ActivateUsage));
			controllerState.uiPressInteractionState.SetFrameState(IsPressed(m_UIPressUsage), ReadValue(m_UIPressUsage));
		}
	}

	protected virtual bool IsPressed(InputHelpers.Button button)
	{
		inputDevice.IsPressed(button, out var isPressed, m_AxisToPressThreshold);
		return isPressed;
	}

	protected virtual float ReadValue(InputHelpers.Button button)
	{
		inputDevice.TryReadSingleValue(button, out var singleValue);
		return singleValue;
	}

	public override bool SendHapticImpulse(float amplitude, float duration)
	{
		if (inputDevice.TryGetHapticCapabilities(out var capabilities) && capabilities.supportsImpulse)
		{
			return inputDevice.SendHapticImpulse(0u, amplitude, duration);
		}
		return false;
	}
}
