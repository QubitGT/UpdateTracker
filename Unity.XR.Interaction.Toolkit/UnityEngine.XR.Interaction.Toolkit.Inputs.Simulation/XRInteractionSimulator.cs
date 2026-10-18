using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[AddComponentMenu("XR/Debug/XR Interaction Simulator", 11)]
[DefaultExecutionOrder(-29991)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRInteractionSimulator.html")]
public class XRInteractionSimulator : MonoBehaviour
{
	private const float k_DeviceLeftRightOffsetAmount = 0.1f;

	private const float k_DeviceForwardOffsetAmount = 0.3f;

	private const float k_DeviceDownOffsetAmount = 0.045f;

	[SerializeField]
	[Tooltip("The Transform that contains the Camera. This is usually the \"Head\" of XR Origins. Automatically set to the first enabled camera tagged MainCamera if unset.")]
	private Transform m_CameraTransform;

	[SerializeField]
	[Tooltip("The corresponding manager for this simulator that handles the lifecycle of the simulated devices.")]
	private SimulatedDeviceLifecycleManager m_DeviceLifecycleManager;

	[SerializeField]
	[Tooltip("The corresponding manager for this simulator that handles the hand expressions.")]
	private SimulatedHandExpressionManager m_HandExpressionManager;

	[SerializeField]
	[Tooltip("The optional Interaction Simulator UI prefab to use along with the XR Interaction Simulator.")]
	private GameObject m_InteractionSimulatorUI;

	[SerializeField]
	[Tooltip("Whether the HMD should report the pose as fully tracked or unavailable/inferred.")]
	private bool m_HMDIsTracked = true;

	[SerializeField]
	[Tooltip("Which tracking values the HMD should report as being valid or meaningful to use, which could mean either tracked or inferred.")]
	private InputTrackingState m_HMDTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;

	[SerializeField]
	[Tooltip("Whether the left-hand controller should report the pose as fully tracked or unavailable/inferred.")]
	private bool m_LeftControllerIsTracked = true;

	[SerializeField]
	[Tooltip("Which tracking values the left-hand controller should report as being valid or meaningful to use, which could mean either tracked or inferred.")]
	private InputTrackingState m_LeftControllerTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;

	[SerializeField]
	[Tooltip("Whether the right-hand controller should report the pose as fully tracked or unavailable/inferred.")]
	private bool m_RightControllerIsTracked = true;

	[SerializeField]
	[Tooltip("Which tracking values the right-hand controller should report as being valid or meaningful to use, which could mean either tracked or inferred.")]
	private InputTrackingState m_RightControllerTrackingState = InputTrackingState.Position | InputTrackingState.Rotation;

	[SerializeField]
	[Tooltip("Whether the left hand should report the pose as fully tracked or unavailable/inferred.")]
	private bool m_LeftHandIsTracked = true;

	[SerializeField]
	[Tooltip("Whether the right hand should report the pose as fully tracked or unavailable/inferred.")]
	private bool m_RightHandIsTracked = true;

	[SerializeField]
	[Tooltip("The input used to translate in the x-axis (left/right) while held.")]
	private XRInputValueReader<float> m_TranslateXInput = new XRInputValueReader<float>("Translate X Input");

	[SerializeField]
	[Tooltip("The input used to translate in the y-axis (up/down) while held.")]
	private XRInputValueReader<float> m_TranslateYInput = new XRInputValueReader<float>("Translate Y Input");

	[SerializeField]
	[Tooltip("The input used to translate in the z-axis (forward/back) while held.")]
	private XRInputValueReader<float> m_TranslateZInput = new XRInputValueReader<float>("Translate Z Input");

	[SerializeField]
	[Tooltip("The input used to toggle enable manipulation of the left-hand controller when pressed.")]
	private XRInputButtonReader m_ToggleManipulateLeftInput;

	[SerializeField]
	[Tooltip("The input used to toggle enable manipulation of the right-hand controller when pressed")]
	private XRInputButtonReader m_ToggleManipulateRightInput;

	[SerializeField]
	[Tooltip("The input used for controlling the left-hand device's actions for buttons or hand expressions.")]
	private XRInputButtonReader m_LeftDeviceActionsInput;

	[SerializeField]
	[Tooltip("The input used to cycle between the different available devices.")]
	private XRInputButtonReader m_CycleDevicesInput;

	[SerializeField]
	[Tooltip("The keyboard input used to rotate by a scaled amount along or about the x- and y-axes.")]
	private XRInputValueReader<Vector2> m_KeyboardRotationDeltaInput = new XRInputValueReader<Vector2>("Keyboard Rotation Delta Input");

	[SerializeField]
	[Tooltip("The input used to toggle associated inputs from a mouse device.")]
	private XRInputButtonReader m_ToggleMouseInput;

	[SerializeField]
	[Tooltip("The mouse input used to rotate by a scaled amount along or about the x- and y-axes.")]
	private XRInputValueReader<Vector2> m_MouseRotationDeltaInput = new XRInputValueReader<Vector2>("Mouse Rotation Delta Input");

	[SerializeField]
	[Tooltip("The input used to translate or rotate by a scaled amount along or about the z-axis.")]
	private XRInputValueReader<Vector2> m_MouseScrollInput;

	[SerializeField]
	[Tooltip("The input used to control the Grip control of the manipulated controller device(s).")]
	private XRInputButtonReader m_GripInput;

	[SerializeField]
	[Tooltip("The input used to control the Trigger control of the manipulated controller device(s).")]
	private XRInputButtonReader m_TriggerInput;

	[SerializeField]
	[Tooltip("The input used to control the PrimaryButton control of the manipulated controller device(s).")]
	private XRInputButtonReader m_PrimaryButtonInput;

	[SerializeField]
	[Tooltip("The input used to control the SecondaryButton control of the manipulated controller device(s).")]
	private XRInputButtonReader m_SecondaryButtonInput;

	[SerializeField]
	[Tooltip("The input used to control the Menu control of the manipulated controller device(s).")]
	private XRInputButtonReader m_MenuInput;

	[SerializeField]
	[Tooltip("The input used to control the Primary2DAxisClick control of the manipulated controller device(s).")]
	private XRInputButtonReader m_Primary2DAxisClickInput;

	[SerializeField]
	[Tooltip("The input used to control the Secondary2DAxisClick control of the manipulated controller device(s).")]
	private XRInputButtonReader m_Secondary2DAxisClickInput;

	[SerializeField]
	[Tooltip("The input used to control the Primary2DAxisTouch control of the manipulated controller device(s).")]
	private XRInputButtonReader m_Primary2DAxisTouchInput;

	[SerializeField]
	[Tooltip("The input used to control the Secondary2DAxisTouch control of the manipulated controller device(s).")]
	private XRInputButtonReader m_Secondary2DAxisTouchInput;

	[SerializeField]
	[Tooltip("The input used to control the PrimaryTouch control of the manipulated controller device(s).")]
	private XRInputButtonReader m_PrimaryTouchInput;

	[SerializeField]
	[Tooltip("The input used to control the SecondaryTouch control of the manipulated controller device(s).")]
	private XRInputButtonReader m_SecondaryTouchInput;

	[SerializeField]
	[Tooltip("The input used to constrain the translation or rotation to the x-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane.")]
	private XRInputButtonReader m_XConstraintInput;

	[SerializeField]
	[Tooltip("The input used to constrain the translation or rotation to the y-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane.")]
	private XRInputButtonReader m_YConstraintInput;

	[SerializeField]
	[Tooltip("The input used to constrain the translation or rotation to the z-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane.")]
	private XRInputButtonReader m_ZConstraintInput;

	[SerializeField]
	[Tooltip("The input used to cause the manipulated device(s) to reset position or rotation (depending on the effective manipulation mode).")]
	private XRInputButtonReader m_ResetInput;

	[SerializeField]
	[Tooltip("The input used to control the value of one or more 2D Axis controls on the manipulated controller device(s).")]
	private XRInputValueReader<Vector2> m_Axis2DInput;

	[SerializeField]
	[Tooltip("The input used to toggle enable manipulation of the Primary2DAxis of the controllers when pressed.")]
	private XRInputButtonReader m_TogglePrimary2DAxisTargetInput;

	[SerializeField]
	[Tooltip("The input used to toggle enable manipulation of the Secondary2DAxis of the controllers when pressed.")]
	private XRInputButtonReader m_ToggleSecondary2DAxisTargetInput;

	[SerializeField]
	[Tooltip("The input used to cycle the quick-action for controller inputs or hand expressions.")]
	private XRInputButtonReader m_CycleQuickActionInput;

	[SerializeField]
	[Tooltip("The input used to perform the currently active quick-action controller input or hand expression.")]
	private XRInputButtonReader m_TogglePerformQuickActionInput;

	[SerializeField]
	[Tooltip("The input used to toggle manipulation of only the head pose.")]
	private XRInputButtonReader m_ToggleManipulateHeadInput;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("The amount of the simulated grip on the controller when the Grip control is pressed.")]
	private float m_GripAmount = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("The amount of the simulated trigger pull on the controller when the Trigger control is pressed.")]
	private float m_TriggerAmount = 1f;

	[SerializeField]
	[Tooltip("Speed of translation in the x-axis (left/right) when triggered by input.")]
	private float m_TranslateXSpeed = 0.2f;

	[SerializeField]
	[Tooltip("Speed of translation in the y-axis (up/down) when triggered by input.")]
	private float m_TranslateYSpeed = 0.2f;

	[SerializeField]
	[Tooltip("Speed of translation in the z-axis (forward/back) when triggered by input.")]
	private float m_TranslateZSpeed = 0.2f;

	[SerializeField]
	[Tooltip("Speed multiplier applied for body translation when triggered by input.")]
	private float m_BodyTranslateMultiplier = 5f;

	[SerializeField]
	[Tooltip("Sensitivity of rotation along the x-axis (pitch) when triggered by input.")]
	private float m_RotateXSensitivity = 0.2f;

	[SerializeField]
	[Tooltip("Sensitivity of rotation along the y-axis (yaw) when triggered by input.")]
	private float m_RotateYSensitivity = 0.2f;

	[SerializeField]
	[Tooltip("Sensitivity of rotation along the z-axis (roll) when triggered by mouse scroll input.")]
	private float m_MouseScrollRotateSensitivity = 0.05f;

	[SerializeField]
	[Tooltip("A boolean value of whether to invert the y-axis when rotating.\nA false value (default) means typical FPS style where moving up/down pitches up/down.\nA true value means flight control style where moving up/down pitches down/up.")]
	private bool m_RotateYInvert;

	[SerializeField]
	[Tooltip("The coordinate space in which translation should operate.")]
	private Space m_TranslateSpace = Space.Screen;

	[SerializeField]
	[Tooltip("The subset of quick-action controller buttons/inputs that a user can shift through in the simulator.")]
	private List<ControllerInputMode> m_QuickActionControllerInputModes = new List<ControllerInputMode>();

	private TargetedDevices m_TargetedDeviceInput = TargetedDevices.FPS;

	private ControllerInputMode m_ControllerInputMode = ControllerInputMode.Trigger;

	private SimulatedHandExpression m_CurrentHandExpression = new SimulatedHandExpression();

	internal static Action<bool> instanceChanged;

	private (Transform transform, Camera camera) m_CachedCamera;

	private float m_TranslateXValue;

	private float m_TranslateYValue;

	private float m_TranslateZValue;

	private Vector2 m_RotationDeltaValue;

	private Vector2 m_MouseScrollValue;

	private bool m_XConstraintValue;

	private bool m_YConstraintValue;

	private bool m_ZConstraintValue;

	private bool m_ResetValue;

	private Vector2 m_Axis2DValue;

	private int m_ControllerInputModeIndex;

	private int m_HandExpressionIndex = -1;

	private bool m_ToggleManipulateWaitingForReleaseBoth;

	private Vector3 m_LeftControllerEuler;

	private Vector3 m_RightControllerEuler;

	private Vector3 m_CenterEyeEuler;

	private XRSimulatedHMDState m_HMDState;

	private XRSimulatedControllerState m_LeftControllerState;

	private XRSimulatedControllerState m_RightControllerState;

	private XRSimulatedHandState m_LeftHandState;

	private XRSimulatedHandState m_RightHandState;

	private TargetedDevices m_PreviousTargetedDevices;

	public Transform cameraTransform
	{
		get
		{
			return m_CameraTransform;
		}
		set
		{
			m_CameraTransform = value;
		}
	}

	public SimulatedDeviceLifecycleManager deviceLifecycleManager
	{
		get
		{
			return m_DeviceLifecycleManager;
		}
		set
		{
			m_DeviceLifecycleManager = value;
		}
	}

	public SimulatedHandExpressionManager handExpressionManager
	{
		get
		{
			return m_HandExpressionManager;
		}
		set
		{
			m_HandExpressionManager = value;
		}
	}

	public GameObject interactionSimulatorUI
	{
		get
		{
			return m_InteractionSimulatorUI;
		}
		set
		{
			m_InteractionSimulatorUI = value;
		}
	}

	public bool hmdIsTracked
	{
		get
		{
			return m_HMDIsTracked;
		}
		set
		{
			m_HMDIsTracked = value;
		}
	}

	public InputTrackingState hmdTrackingState
	{
		get
		{
			return m_HMDTrackingState;
		}
		set
		{
			m_HMDTrackingState = value;
		}
	}

	public bool leftControllerIsTracked
	{
		get
		{
			return m_LeftControllerIsTracked;
		}
		set
		{
			m_LeftControllerIsTracked = value;
		}
	}

	public InputTrackingState leftControllerTrackingState
	{
		get
		{
			return m_LeftControllerTrackingState;
		}
		set
		{
			m_LeftControllerTrackingState = value;
		}
	}

	public bool rightControllerIsTracked
	{
		get
		{
			return m_RightControllerIsTracked;
		}
		set
		{
			m_RightControllerIsTracked = value;
		}
	}

	public InputTrackingState rightControllerTrackingState
	{
		get
		{
			return m_RightControllerTrackingState;
		}
		set
		{
			m_RightControllerTrackingState = value;
		}
	}

	public bool leftHandIsTracked
	{
		get
		{
			return m_LeftHandIsTracked;
		}
		set
		{
			m_LeftHandIsTracked = value;
		}
	}

	public bool rightHandIsTracked
	{
		get
		{
			return m_RightHandIsTracked;
		}
		set
		{
			m_RightHandIsTracked = value;
		}
	}

	public XRInputValueReader<float> translateXInput
	{
		get
		{
			return m_TranslateXInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TranslateXInput, value, this);
		}
	}

	public XRInputValueReader<float> translateYInput
	{
		get
		{
			return m_TranslateYInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TranslateYInput, value, this);
		}
	}

	public XRInputValueReader<float> translateZInput
	{
		get
		{
			return m_TranslateZInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TranslateZInput, value, this);
		}
	}

	public XRInputButtonReader toggleManipulateLeftInput
	{
		get
		{
			return m_ToggleManipulateLeftInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ToggleManipulateLeftInput, value, this);
		}
	}

	public XRInputButtonReader toggleManipulateRightInput
	{
		get
		{
			return m_ToggleManipulateRightInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ToggleManipulateRightInput, value, this);
		}
	}

	public XRInputButtonReader leftDeviceActionsInput
	{
		get
		{
			return m_LeftDeviceActionsInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_LeftDeviceActionsInput, value, this);
		}
	}

	public XRInputButtonReader cycleDevicesInput
	{
		get
		{
			return m_CycleDevicesInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_CycleDevicesInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> keyboardRotationDeltaInput
	{
		get
		{
			return m_KeyboardRotationDeltaInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_KeyboardRotationDeltaInput, value, this);
		}
	}

	public XRInputButtonReader toggleMouseInput
	{
		get
		{
			return m_ToggleMouseInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ToggleMouseInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> mouseRotationDeltaInput
	{
		get
		{
			return m_MouseRotationDeltaInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_MouseRotationDeltaInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> mouseScrollInput
	{
		get
		{
			return m_MouseScrollInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_MouseScrollInput, value, this);
		}
	}

	public XRInputButtonReader gripInput
	{
		get
		{
			return m_GripInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_GripInput, value, this);
		}
	}

	public XRInputButtonReader triggerInput
	{
		get
		{
			return m_TriggerInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TriggerInput, value, this);
		}
	}

	public XRInputButtonReader primaryButtonInput
	{
		get
		{
			return m_PrimaryButtonInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_PrimaryButtonInput, value, this);
		}
	}

	public XRInputButtonReader secondaryButtonInput
	{
		get
		{
			return m_SecondaryButtonInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_SecondaryButtonInput, value, this);
		}
	}

	public XRInputButtonReader menuInput
	{
		get
		{
			return m_MenuInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_MenuInput, value, this);
		}
	}

	public XRInputButtonReader primary2DAxisClickInput
	{
		get
		{
			return m_Primary2DAxisClickInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_Primary2DAxisClickInput, value, this);
		}
	}

	public XRInputButtonReader secondary2DAxisClickInput
	{
		get
		{
			return m_Secondary2DAxisClickInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_Secondary2DAxisClickInput, value, this);
		}
	}

	public XRInputButtonReader primary2DAxisTouchInput
	{
		get
		{
			return m_Primary2DAxisTouchInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_Primary2DAxisTouchInput, value, this);
		}
	}

	public XRInputButtonReader secondary2DAxisTouchInput
	{
		get
		{
			return m_Secondary2DAxisTouchInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_Secondary2DAxisTouchInput, value, this);
		}
	}

	public XRInputButtonReader primaryTouchInput
	{
		get
		{
			return m_PrimaryTouchInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_PrimaryTouchInput, value, this);
		}
	}

	public XRInputButtonReader secondaryTouchInput
	{
		get
		{
			return m_SecondaryTouchInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_SecondaryTouchInput, value, this);
		}
	}

	public XRInputButtonReader xConstraintInput
	{
		get
		{
			return m_XConstraintInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_XConstraintInput, value, this);
		}
	}

	public XRInputButtonReader yConstraintInput
	{
		get
		{
			return m_YConstraintInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_YConstraintInput, value, this);
		}
	}

	public XRInputButtonReader zConstraintInput
	{
		get
		{
			return m_ZConstraintInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ZConstraintInput, value, this);
		}
	}

	public XRInputButtonReader resetInput
	{
		get
		{
			return m_ResetInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ResetInput, value, this);
		}
	}

	public XRInputValueReader<Vector2> axis2DInput
	{
		get
		{
			return m_Axis2DInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_Axis2DInput, value, this);
		}
	}

	public XRInputButtonReader togglePrimary2DAxisTargetInput
	{
		get
		{
			return m_TogglePrimary2DAxisTargetInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TogglePrimary2DAxisTargetInput, value, this);
		}
	}

	public XRInputButtonReader toggleSecondary2DAxisTargetInput
	{
		get
		{
			return m_ToggleSecondary2DAxisTargetInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ToggleSecondary2DAxisTargetInput, value, this);
		}
	}

	public XRInputButtonReader cycleQuickActionInput
	{
		get
		{
			return m_CycleQuickActionInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_CycleQuickActionInput, value, this);
		}
	}

	public XRInputButtonReader togglePerformQuickActionInput
	{
		get
		{
			return m_TogglePerformQuickActionInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_TogglePerformQuickActionInput, value, this);
		}
	}

	public XRInputButtonReader toggleManipulateHeadInput
	{
		get
		{
			return m_ToggleManipulateHeadInput;
		}
		set
		{
			XRInputReaderUtility.SetInputProperty(ref m_ToggleManipulateHeadInput, value, this);
		}
	}

	public float gripAmount
	{
		get
		{
			return m_GripAmount;
		}
		set
		{
			m_GripAmount = value;
		}
	}

	public float triggerAmount
	{
		get
		{
			return m_TriggerAmount;
		}
		set
		{
			m_TriggerAmount = value;
		}
	}

	public float translateXSpeed
	{
		get
		{
			return m_TranslateXSpeed;
		}
		set
		{
			m_TranslateXSpeed = value;
		}
	}

	public float translateYSpeed
	{
		get
		{
			return m_TranslateYSpeed;
		}
		set
		{
			m_TranslateYSpeed = value;
		}
	}

	public float translateZSpeed
	{
		get
		{
			return m_TranslateZSpeed;
		}
		set
		{
			m_TranslateZSpeed = value;
		}
	}

	public float bodyTranslateMultiplier
	{
		get
		{
			return m_BodyTranslateMultiplier;
		}
		set
		{
			m_BodyTranslateMultiplier = value;
		}
	}

	public float rotateXSensitivity
	{
		get
		{
			return m_RotateXSensitivity;
		}
		set
		{
			m_RotateXSensitivity = value;
		}
	}

	public float rotateYSensitivity
	{
		get
		{
			return m_RotateYSensitivity;
		}
		set
		{
			m_RotateYSensitivity = value;
		}
	}

	public float mouseScrollRotateSensitivity
	{
		get
		{
			return m_MouseScrollRotateSensitivity;
		}
		set
		{
			m_MouseScrollRotateSensitivity = value;
		}
	}

	public bool rotateYInvert
	{
		get
		{
			return m_RotateYInvert;
		}
		set
		{
			m_RotateYInvert = value;
		}
	}

	public Space translateSpace
	{
		get
		{
			return m_TranslateSpace;
		}
		set
		{
			m_TranslateSpace = value;
		}
	}

	public List<ControllerInputMode> quickActionControllerInputModes
	{
		get
		{
			return m_QuickActionControllerInputModes;
		}
		set
		{
			m_QuickActionControllerInputModes = value;
		}
	}

	public TargetedDevices targetedDeviceInput
	{
		get
		{
			return m_TargetedDeviceInput;
		}
		set
		{
			m_TargetedDeviceInput = value;
		}
	}

	public ControllerInputMode controllerInputMode => m_ControllerInputMode;

	public SimulatedHandExpression currentHandExpression => m_CurrentHandExpression;

	public Axis2DTargets axis2DTargets { get; set; } = Axis2DTargets.Primary2DAxis;


	public bool manipulatingLeftDevice => m_TargetedDeviceInput.HasDevice(TargetedDevices.LeftDevice);

	public bool manipulatingRightDevice => m_TargetedDeviceInput.HasDevice(TargetedDevices.RightDevice);

	public bool manipulatingLeftController
	{
		get
		{
			if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Controller)
			{
				return manipulatingLeftDevice;
			}
			return false;
		}
	}

	public bool manipulatingRightController
	{
		get
		{
			if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Controller)
			{
				return manipulatingRightDevice;
			}
			return false;
		}
	}

	public bool manipulatingLeftHand
	{
		get
		{
			if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Hand)
			{
				return manipulatingLeftDevice;
			}
			return false;
		}
	}

	public bool manipulatingRightHand
	{
		get
		{
			if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Hand)
			{
				return manipulatingRightDevice;
			}
			return false;
		}
	}

	public bool manipulatingHMD => m_TargetedDeviceInput == TargetedDevices.HMD;

	public bool manipulatingFPS => m_TargetedDeviceInput.HasDevice(TargetedDevices.FPS);

	public static XRInteractionSimulator instance { get; private set; }

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = this;
			instanceChanged?.Invoke(obj: true);
		}
		else if (instance != this)
		{
			Debug.LogWarning($"Another instance of XR Interaction Simulator already exists ({instance}), destroying {base.gameObject}.", this);
			Object.Destroy(base.gameObject);
			return;
		}
		if (m_DeviceLifecycleManager == null)
		{
			m_DeviceLifecycleManager = XRSimulatorUtility.FindCreateSimulatedDeviceLifecycleManager(base.gameObject);
		}
		if (m_HandExpressionManager == null)
		{
			m_HandExpressionManager = XRSimulatorUtility.FindCreateSimulatedHandExpressionManager(base.gameObject);
		}
		m_HMDState.Reset();
		m_LeftControllerState.Reset();
		m_RightControllerState.Reset();
		m_LeftHandState.Reset();
		m_RightHandState.Reset();
		m_LeftControllerState.devicePosition = XRSimulatorUtility.leftDeviceDefaultInitialPosition;
		m_RightControllerState.devicePosition = XRSimulatorUtility.rightDeviceDefaultInitialPosition;
		m_LeftHandState.position = XRSimulatorUtility.leftDeviceDefaultInitialPosition;
		m_RightHandState.position = XRSimulatorUtility.rightDeviceDefaultInitialPosition;
		if (m_InteractionSimulatorUI != null)
		{
			Object.Instantiate(m_InteractionSimulatorUI, base.transform);
		}
	}

	protected virtual void OnEnable()
	{
		XRSimulatorUtility.FindCameraTransform(ref m_CachedCamera, ref m_CameraTransform);
		if (m_QuickActionControllerInputModes.Count > 0)
		{
			m_ControllerInputMode = m_QuickActionControllerInputModes[0];
		}
		if (m_HandExpressionManager.simulatedHandExpressions.Count > 0)
		{
			CycleQuickActionHandExpression();
		}
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void OnDestroy()
	{
		if (instance == this)
		{
			instanceChanged?.Invoke(obj: false);
		}
	}

	protected virtual void Update()
	{
		ReadInputValues();
		HandleLeftOrRightDeviceToggle();
		if (m_CycleDevicesInput.ReadWasPerformedThisFrame())
		{
			CycleTargetDevices();
		}
		if (m_CycleQuickActionInput.ReadWasPerformedThisFrame() && !manipulatingFPS && !manipulatingHMD)
		{
			CycleQuickAction();
		}
		if (m_ToggleManipulateHeadInput.ReadWasPerformedThisFrame())
		{
			HandleHMDToggle();
		}
		if (m_TogglePerformQuickActionInput.ReadWasPerformedThisFrame())
		{
			PerformQuickAction();
		}
		ProcessPoseInput();
		ProcessControlInput();
		ProcessHandExpressionInput();
		m_DeviceLifecycleManager.ApplyHandState(m_LeftHandState, m_RightHandState);
		m_DeviceLifecycleManager.ApplyHMDState(m_HMDState);
		m_DeviceLifecycleManager.ApplyControllerState(m_LeftControllerState, m_RightControllerState);
	}

	protected virtual void ProcessPoseInput()
	{
		SetTrackedStates();
		if (m_TargetedDeviceInput == TargetedDevices.None || !XRSimulatorUtility.FindCameraTransform(ref m_CachedCamera, ref m_CameraTransform))
		{
			return;
		}
		Transform parent = m_CameraTransform.parent;
		Quaternion quaternion = ((parent != null) ? parent.rotation : Quaternion.identity);
		Quaternion inverseCameraParentRotation = Quaternion.Inverse(quaternion);
		if (manipulatingFPS && Time.time > 1f)
		{
			float xTranslateInput = m_TranslateXValue * m_TranslateXSpeed * m_BodyTranslateMultiplier * Time.deltaTime;
			float yTranslateInput = m_TranslateYValue * m_TranslateYSpeed * m_BodyTranslateMultiplier * Time.deltaTime;
			float zTranslateInput = m_TranslateZValue * m_TranslateZSpeed * m_BodyTranslateMultiplier * Time.deltaTime;
			Vector3 translationInDeviceSpace = XRSimulatorUtility.GetTranslationInDeviceSpace(xTranslateInput, yTranslateInput, zTranslateInput, m_CameraTransform, quaternion, inverseCameraParentRotation);
			m_LeftControllerState.devicePosition += translationInDeviceSpace;
			m_RightControllerState.devicePosition += translationInDeviceSpace;
			m_LeftHandState.position += translationInDeviceSpace;
			m_RightHandState.position += translationInDeviceSpace;
			m_HMDState.centerEyePosition += translationInDeviceSpace;
			m_HMDState.devicePosition = m_HMDState.centerEyePosition;
			Vector3 vector = new Vector3(m_RotationDeltaValue.x * m_RotateXSensitivity, m_RotationDeltaValue.y * m_RotateYSensitivity * (m_RotateYInvert ? 1f : (-1f)), m_MouseScrollValue.y * m_MouseScrollRotateSensitivity);
			Vector3 vector2 = ((m_XConstraintValue && !m_YConstraintValue && !m_ZConstraintValue) ? new Vector3(0f - vector.x + vector.y, 0f, 0f) : ((m_XConstraintValue || !m_YConstraintValue || m_ZConstraintValue) ? new Vector3(vector.y, vector.x, 0f) : new Vector3(0f, vector.x + (0f - vector.y), 0f)));
			m_CenterEyeEuler += vector2;
			m_CenterEyeEuler.x = Mathf.Clamp(m_CenterEyeEuler.x, 0f - XRSimulatorUtility.cameraMaxXAngle, XRSimulatorUtility.cameraMaxXAngle);
			m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
			m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
			Quaternion quaternion2 = Quaternion.AngleAxis(vector2.y, Quaternion.Euler(0f, m_CenterEyeEuler.y, 0f) * Vector3.up);
			Vector3 centerEyePosition = m_HMDState.centerEyePosition;
			m_LeftControllerState.devicePosition = quaternion2 * (m_LeftControllerState.devicePosition - centerEyePosition) + centerEyePosition;
			m_LeftControllerState.deviceRotation = quaternion2 * m_LeftControllerState.deviceRotation;
			m_RightControllerState.devicePosition = quaternion2 * (m_RightControllerState.devicePosition - centerEyePosition) + centerEyePosition;
			m_RightControllerState.deviceRotation = quaternion2 * m_RightControllerState.deviceRotation;
			m_LeftControllerEuler = m_LeftControllerState.deviceRotation.eulerAngles;
			m_RightControllerEuler = m_RightControllerState.deviceRotation.eulerAngles;
			m_LeftHandState.position = quaternion2 * (m_LeftHandState.position - centerEyePosition) + centerEyePosition;
			m_LeftHandState.rotation = quaternion2 * m_LeftHandState.rotation;
			m_RightHandState.position = quaternion2 * (m_RightHandState.position - centerEyePosition) + centerEyePosition;
			m_RightHandState.rotation = quaternion2 * m_RightHandState.rotation;
			m_LeftHandState.euler = m_LeftHandState.rotation.eulerAngles;
			m_RightHandState.euler = m_RightHandState.rotation.eulerAngles;
		}
		else if (!manipulatingFPS)
		{
			float xTranslateInput2 = m_TranslateXValue * m_TranslateXSpeed * m_BodyTranslateMultiplier * Time.deltaTime;
			float yTranslateInput2 = m_TranslateYValue * m_TranslateYSpeed * m_BodyTranslateMultiplier * Time.deltaTime;
			float zTranslateInput2 = m_TranslateZValue * m_TranslateZSpeed * m_BodyTranslateMultiplier * Time.deltaTime;
			Vector3 translationInDeviceSpace2 = XRSimulatorUtility.GetTranslationInDeviceSpace(xTranslateInput2, yTranslateInput2, zTranslateInput2, m_CameraTransform, quaternion, inverseCameraParentRotation);
			Vector3 vector3 = new Vector3(m_RotationDeltaValue.x * m_RotateXSensitivity, m_RotationDeltaValue.y * m_RotateYSensitivity * (m_RotateYInvert ? 1f : (-1f)), m_MouseScrollValue.y * m_MouseScrollRotateSensitivity);
			Vector3 vector4 = ((m_XConstraintValue && !m_YConstraintValue && m_ZConstraintValue) ? new Vector3(vector3.y, 0f, 0f - vector3.x) : ((!m_XConstraintValue && m_YConstraintValue && m_ZConstraintValue) ? new Vector3(0f, vector3.x, 0f - vector3.y) : ((m_XConstraintValue && !m_YConstraintValue && !m_ZConstraintValue) ? new Vector3(0f - vector3.x + vector3.y, 0f, 0f) : ((!m_XConstraintValue && m_YConstraintValue && !m_ZConstraintValue) ? new Vector3(0f, vector3.x + (0f - vector3.y), 0f) : ((m_XConstraintValue || m_YConstraintValue || !m_ZConstraintValue) ? new Vector3(vector3.y, vector3.x, 0f) : new Vector3(0f, 0f, 0f - vector3.x + (0f - vector3.y)))))));
			vector4 += new Vector3(0f, 0f, vector3.z);
			if (manipulatingLeftController)
			{
				Quaternion deltaRotation = XRSimulatorUtility.GetDeltaRotation(m_TranslateSpace, in m_LeftControllerState, in inverseCameraParentRotation);
				m_LeftControllerState.devicePosition += deltaRotation * translationInDeviceSpace2;
				m_LeftControllerEuler += vector4;
				m_LeftControllerState.deviceRotation = Quaternion.Euler(m_LeftControllerEuler);
			}
			if (manipulatingRightController)
			{
				Quaternion deltaRotation2 = XRSimulatorUtility.GetDeltaRotation(m_TranslateSpace, in m_RightControllerState, in inverseCameraParentRotation);
				m_RightControllerState.devicePosition += deltaRotation2 * translationInDeviceSpace2;
				m_RightControllerEuler += vector4;
				m_RightControllerState.deviceRotation = Quaternion.Euler(m_RightControllerEuler);
			}
			if (manipulatingLeftHand)
			{
				Quaternion deltaRotation3 = XRSimulatorUtility.GetDeltaRotation(m_TranslateSpace, in m_LeftHandState, in inverseCameraParentRotation);
				m_LeftHandState.position += deltaRotation3 * translationInDeviceSpace2;
				m_LeftHandState.euler += vector4;
				m_LeftHandState.rotation = Quaternion.Euler(m_LeftHandState.euler);
			}
			if (manipulatingRightHand)
			{
				Quaternion deltaRotation4 = XRSimulatorUtility.GetDeltaRotation(m_TranslateSpace, in m_RightHandState, in inverseCameraParentRotation);
				m_RightHandState.position += deltaRotation4 * translationInDeviceSpace2;
				m_RightHandState.euler += vector4;
				m_RightHandState.rotation = Quaternion.Euler(m_RightHandState.euler);
			}
			if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
			{
				Quaternion deltaRotation5 = XRSimulatorUtility.GetDeltaRotation(m_TranslateSpace, in m_HMDState, in inverseCameraParentRotation);
				m_HMDState.centerEyePosition += deltaRotation5 * translationInDeviceSpace2;
				m_HMDState.devicePosition = m_HMDState.centerEyePosition;
				m_CenterEyeEuler += vector4;
				m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
				m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
			}
		}
		if (m_ResetValue)
		{
			Vector3 vector5 = m_HMDState.deviceRotation * Vector3.forward * 0.3f;
			Vector3 vector6 = m_HMDState.deviceRotation * Vector3.down * 0.045f;
			Vector3 vector7 = m_HMDState.deviceRotation * Vector3.left * 0.1f;
			Vector3 vector8 = m_HMDState.deviceRotation * Vector3.right * 0.1f;
			m_LeftControllerState.devicePosition = m_HMDState.devicePosition + vector5 + vector6 + vector7;
			m_RightControllerState.devicePosition = m_HMDState.devicePosition + vector5 + vector6 + vector8;
			m_LeftControllerEuler = m_HMDState.deviceRotation.eulerAngles;
			m_LeftControllerState.deviceRotation = m_HMDState.deviceRotation;
			m_RightControllerEuler = m_HMDState.deviceRotation.eulerAngles;
			m_RightControllerState.deviceRotation = m_HMDState.deviceRotation;
			m_LeftHandState.position = m_HMDState.devicePosition + vector5 + vector6 + vector7;
			m_RightHandState.position = m_HMDState.devicePosition + vector5 + vector6 + vector8;
			m_LeftHandState.euler = m_HMDState.deviceRotation.eulerAngles;
			m_LeftHandState.rotation = m_HMDState.deviceRotation;
			m_RightHandState.euler = m_HMDState.deviceRotation.eulerAngles;
			m_RightHandState.rotation = m_HMDState.deviceRotation;
		}
	}

	protected virtual void ProcessControlInput()
	{
		if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Controller)
		{
			if (m_LeftDeviceActionsInput.ReadIsPerformed())
			{
				ProcessButtonControlInput(ref m_LeftControllerState);
				ProcessAxis2DControlInput(ref m_LeftControllerState);
			}
			else
			{
				ProcessButtonControlInput(ref m_RightControllerState);
				ProcessAxis2DControlInput(ref m_RightControllerState);
			}
			if (!manipulatingLeftController)
			{
				ProcessAnalogButtonControlInput(ref m_LeftControllerState);
			}
			if (!manipulatingRightController)
			{
				ProcessAnalogButtonControlInput(ref m_RightControllerState);
			}
		}
	}

	private void ProcessHandExpressionInput()
	{
	}

	private void ToggleHandExpression(SimulatedHandExpression simulatedExpression, bool leftHand, bool rightHand)
	{
	}

	protected virtual void ProcessAxis2DControlInput(ref XRSimulatedControllerState controllerState)
	{
		if ((axis2DTargets & Axis2DTargets.Primary2DAxis) != 0)
		{
			controllerState.primary2DAxis = m_Axis2DValue;
		}
		if ((axis2DTargets & Axis2DTargets.Secondary2DAxis) != 0)
		{
			controllerState.secondary2DAxis = m_Axis2DValue;
		}
	}

	protected virtual void ProcessButtonControlInput(ref XRSimulatedControllerState controllerState)
	{
		if (m_GripInput.ReadIsPerformed())
		{
			controllerState.grip = m_GripAmount;
			controllerState.WithButton(ControllerButton.GripButton);
		}
		else if (m_GripInput.ReadWasCompletedThisFrame())
		{
			controllerState.grip = 0f;
			controllerState.WithButton(ControllerButton.GripButton, state: false);
		}
		if (m_TriggerInput.ReadIsPerformed())
		{
			controllerState.trigger = m_TriggerAmount;
			controllerState.WithButton(ControllerButton.TriggerButton);
		}
		else if (m_TriggerInput.ReadWasCompletedThisFrame())
		{
			controllerState.trigger = 0f;
			controllerState.WithButton(ControllerButton.TriggerButton, state: false);
		}
		if (m_PrimaryButtonInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.PrimaryButton);
		}
		else if (m_PrimaryButtonInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.PrimaryButton, state: false);
		}
		if (m_SecondaryButtonInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.SecondaryButton);
		}
		else if (m_SecondaryButtonInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.SecondaryButton, state: false);
		}
		if (m_MenuInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.MenuButton);
		}
		else if (m_MenuInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.MenuButton, state: false);
		}
		if (m_Primary2DAxisClickInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.Primary2DAxisClick);
		}
		else if (m_Primary2DAxisClickInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.Primary2DAxisClick, state: false);
		}
		if (m_Secondary2DAxisClickInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.Secondary2DAxisClick);
		}
		else if (m_Secondary2DAxisClickInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.Secondary2DAxisClick, state: false);
		}
		if (m_Primary2DAxisTouchInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.Primary2DAxisTouch);
		}
		else if (m_Primary2DAxisTouchInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.Primary2DAxisTouch, state: false);
		}
		if (m_Secondary2DAxisTouchInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.Secondary2DAxisTouch);
		}
		else if (m_Secondary2DAxisTouchInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.Secondary2DAxisTouch, state: false);
		}
		if (m_PrimaryTouchInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.PrimaryTouch);
		}
		else if (m_PrimaryTouchInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.PrimaryTouch, state: false);
		}
		if (m_SecondaryTouchInput.ReadIsPerformed())
		{
			controllerState.WithButton(ControllerButton.SecondaryTouch);
		}
		else if (m_SecondaryTouchInput.ReadWasCompletedThisFrame())
		{
			controllerState.WithButton(ControllerButton.SecondaryTouch, state: false);
		}
	}

	protected virtual void ProcessAnalogButtonControlInput(ref XRSimulatedControllerState controllerState)
	{
		if (controllerState.HasButton(ControllerButton.GripButton))
		{
			controllerState.grip = m_GripAmount;
		}
		if (controllerState.HasButton(ControllerButton.TriggerButton))
		{
			controllerState.trigger = m_TriggerAmount;
		}
	}

	protected Vector3 GetResetScale()
	{
		if (!m_XConstraintValue && !m_YConstraintValue && !m_ZConstraintValue)
		{
			return Vector3.zero;
		}
		return new Vector3(m_XConstraintValue ? 0f : 1f, m_YConstraintValue ? 0f : 1f, m_ZConstraintValue ? 0f : 1f);
	}

	protected virtual void ReadInputValues()
	{
		m_TranslateXValue = m_TranslateXInput.ReadValue();
		m_TranslateYValue = m_TranslateYInput.ReadValue();
		m_TranslateZValue = m_TranslateZInput.ReadValue();
		m_RotationDeltaValue = m_KeyboardRotationDeltaInput.ReadValue();
		if (m_ToggleMouseInput.ReadIsPerformed())
		{
			Vector2 vector = m_MouseRotationDeltaInput.ReadValue();
			if (vector != Vector2.zero)
			{
				m_RotationDeltaValue = vector;
			}
			m_MouseScrollValue = m_MouseScrollInput.ReadValue();
			if (m_MouseScrollValue.y != 0f)
			{
				m_TranslateZValue = m_MouseScrollValue.y;
			}
		}
		m_XConstraintValue = m_XConstraintInput.ReadIsPerformed();
		m_YConstraintValue = m_YConstraintInput.ReadIsPerformed();
		m_ZConstraintValue = m_ZConstraintInput.ReadIsPerformed();
		m_ResetValue = m_ResetInput.ReadWasPerformedThisFrame();
		m_Axis2DValue = Vector2.ClampMagnitude(m_Axis2DInput.ReadValue(), 1f);
		if (m_TogglePrimary2DAxisTargetInput.ReadWasPerformedThisFrame())
		{
			axis2DTargets = Axis2DTargets.Primary2DAxis;
		}
		if (m_ToggleSecondary2DAxisTargetInput.ReadWasPerformedThisFrame())
		{
			axis2DTargets = Axis2DTargets.Secondary2DAxis;
		}
	}

	private void CycleQuickAction()
	{
		if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Controller)
		{
			if (m_QuickActionControllerInputModes.Count == 0)
			{
				Debug.LogWarning("The key to switch between controller inputs has been pressed, but there doesn't seem to be any inputs set in the quick-action controller input modes.", this);
				return;
			}
			ClearControllerButtonInput(ref m_LeftControllerState);
			ClearControllerButtonInput(ref m_RightControllerState);
			m_ControllerInputModeIndex = ((m_ControllerInputModeIndex < m_QuickActionControllerInputModes.Count - 1) ? (m_ControllerInputModeIndex + 1) : 0);
			m_ControllerInputMode = m_QuickActionControllerInputModes[m_ControllerInputModeIndex];
		}
		else if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Hand)
		{
			CycleQuickActionHandExpression();
		}
	}

	private void PerformQuickAction()
	{
		if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Controller)
		{
			if (manipulatingLeftController)
			{
				ToggleControllerButtonInput(ref m_LeftControllerState);
			}
			else if (manipulatingRightController)
			{
				ToggleControllerButtonInput(ref m_RightControllerState);
			}
		}
		else if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Hand)
		{
			ToggleHandExpression(m_CurrentHandExpression, manipulatingLeftHand, manipulatingRightHand);
		}
	}

	private void ToggleControllerButtonInput(ref XRSimulatedControllerState controllerState)
	{
		switch (m_ControllerInputMode)
		{
		case ControllerInputMode.Trigger:
			controllerState.ToggleButton(ControllerButton.TriggerButton);
			controllerState.trigger = (controllerState.HasButton(ControllerButton.TriggerButton) ? m_TriggerAmount : 0f);
			break;
		case ControllerInputMode.Grip:
			controllerState.ToggleButton(ControllerButton.GripButton);
			controllerState.grip = (controllerState.HasButton(ControllerButton.GripButton) ? m_GripAmount : 0f);
			break;
		case ControllerInputMode.PrimaryButton:
			controllerState.ToggleButton(ControllerButton.PrimaryButton);
			break;
		case ControllerInputMode.SecondaryButton:
			controllerState.ToggleButton(ControllerButton.SecondaryButton);
			break;
		case ControllerInputMode.Menu:
			controllerState.ToggleButton(ControllerButton.MenuButton);
			break;
		case ControllerInputMode.Primary2DAxisClick:
			controllerState.ToggleButton(ControllerButton.Primary2DAxisClick);
			break;
		case ControllerInputMode.Secondary2DAxisClick:
			controllerState.ToggleButton(ControllerButton.Secondary2DAxisClick);
			break;
		case ControllerInputMode.Primary2DAxisTouch:
			controllerState.ToggleButton(ControllerButton.Primary2DAxisTouch);
			break;
		case ControllerInputMode.Secondary2DAxisTouch:
			controllerState.ToggleButton(ControllerButton.Secondary2DAxisTouch);
			break;
		case ControllerInputMode.PrimaryTouch:
			controllerState.ToggleButton(ControllerButton.PrimaryTouch);
			break;
		case ControllerInputMode.SecondaryTouch:
			controllerState.ToggleButton(ControllerButton.SecondaryTouch);
			break;
		case ControllerInputMode.None:
			break;
		}
	}

	private static void ClearControllerButtonInput(ref XRSimulatedControllerState controllerState)
	{
		controllerState.trigger = 0f;
		controllerState.grip = 0f;
		controllerState.buttons = 0;
	}

	private void SetTrackedStates()
	{
		m_LeftControllerState.isTracked = m_LeftControllerIsTracked;
		m_RightControllerState.isTracked = m_RightControllerIsTracked;
		m_LeftHandState.isTracked = m_LeftHandIsTracked;
		m_RightHandState.isTracked = m_RightHandIsTracked;
		m_HMDState.isTracked = m_HMDIsTracked;
		m_LeftControllerState.trackingState = (int)m_LeftControllerTrackingState;
		m_RightControllerState.trackingState = (int)m_RightControllerTrackingState;
		m_HMDState.trackingState = (int)m_HMDTrackingState;
	}

	private void CycleTargetDevices()
	{
		if (targetedDeviceInput.HasDevice(TargetedDevices.HMD))
		{
			targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.HMD);
		}
		if (targetedDeviceInput == TargetedDevices.None)
		{
			targetedDeviceInput = TargetedDevices.FPS;
		}
		else if (targetedDeviceInput.HasDevice(TargetedDevices.FPS))
		{
			targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.FPS);
			if (!targetedDeviceInput.HasDevice(TargetedDevices.LeftDevice) && !targetedDeviceInput.HasDevice(TargetedDevices.RightDevice))
			{
				targetedDeviceInput = TargetedDevices.LeftDevice | TargetedDevices.RightDevice;
			}
		}
		else if (targetedDeviceInput.HasDevice(TargetedDevices.LeftDevice) || targetedDeviceInput.HasDevice(TargetedDevices.RightDevice))
		{
			targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.FPS);
		}
	}

	private void HandleLeftOrRightDeviceToggle()
	{
		if (m_ToggleManipulateWaitingForReleaseBoth)
		{
			m_ToggleManipulateWaitingForReleaseBoth = m_ToggleManipulateLeftInput.ReadIsPerformed() || m_ToggleManipulateRightInput.ReadIsPerformed();
		}
		else if (m_ToggleManipulateLeftInput.ReadIsPerformed() && m_ToggleManipulateRightInput.ReadIsPerformed())
		{
			if (targetedDeviceInput.HasDevice(TargetedDevices.HMD))
			{
				targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.HMD);
			}
			m_ToggleManipulateWaitingForReleaseBoth = true;
			if (targetedDeviceInput == (TargetedDevices.LeftDevice | TargetedDevices.RightDevice))
			{
				m_DeviceLifecycleManager.SwitchDeviceMode();
			}
			else
			{
				targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.LeftDevice).WithDevice(TargetedDevices.RightDevice).WithoutDevice(TargetedDevices.FPS);
			}
		}
		else if (m_ToggleManipulateLeftInput.ReadWasCompletedThisFrame())
		{
			if (targetedDeviceInput.HasDevice(TargetedDevices.HMD))
			{
				targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.HMD);
			}
			if (targetedDeviceInput == TargetedDevices.LeftDevice)
			{
				m_DeviceLifecycleManager.SwitchDeviceMode();
			}
			else
			{
				targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.LeftDevice).WithoutDevice(TargetedDevices.RightDevice).WithoutDevice(TargetedDevices.FPS);
			}
		}
		else if (m_ToggleManipulateRightInput.ReadWasCompletedThisFrame())
		{
			if (targetedDeviceInput.HasDevice(TargetedDevices.HMD))
			{
				targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.HMD);
			}
			if (targetedDeviceInput == TargetedDevices.RightDevice)
			{
				m_DeviceLifecycleManager.SwitchDeviceMode();
			}
			else
			{
				targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.RightDevice).WithoutDevice(TargetedDevices.LeftDevice).WithoutDevice(TargetedDevices.FPS);
			}
		}
	}

	private void HandleHMDToggle()
	{
		if (targetedDeviceInput != TargetedDevices.HMD)
		{
			m_PreviousTargetedDevices = targetedDeviceInput;
			targetedDeviceInput = TargetedDevices.HMD;
		}
		else
		{
			targetedDeviceInput = m_PreviousTargetedDevices;
		}
	}

	private void CycleQuickActionHandExpression()
	{
		List<SimulatedHandExpression> simulatedHandExpressions = m_HandExpressionManager.simulatedHandExpressions;
		for (int i = 0; i < simulatedHandExpressions.Count; i++)
		{
			m_HandExpressionIndex = ((m_HandExpressionIndex < simulatedHandExpressions.Count - 1) ? (m_HandExpressionIndex + 1) : 0);
			if (simulatedHandExpressions[m_HandExpressionIndex].isQuickAction)
			{
				m_CurrentHandExpression = simulatedHandExpressions[m_HandExpressionIndex];
				return;
			}
		}
		m_HandExpressionIndex = -1;
		Debug.LogWarning("The key to switch between hand expressions has been pressed, but there doesn't seem to be any expressions set to quick-access in the Simulated Hand Expression Manager.", this);
	}
}
