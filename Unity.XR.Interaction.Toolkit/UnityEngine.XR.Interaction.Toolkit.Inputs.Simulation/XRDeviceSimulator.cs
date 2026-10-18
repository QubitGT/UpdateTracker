using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[AddComponentMenu("XR/Debug/XR Device Simulator", 11)]
[DefaultExecutionOrder(-29991)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.XRDeviceSimulator.html")]
public class XRDeviceSimulator : MonoBehaviour
{
	public enum Space
	{
		Local,
		Parent,
		Screen
	}

	public enum TransformationMode
	{
		Translate,
		Rotate
	}

	[Flags]
	internal enum TargetedDevices
	{
		None = 0,
		FPS = 1,
		LeftDevice = 2,
		RightDevice = 4,
		HMD = 8
	}

	[Flags]
	public enum Axis2DTargets
	{
		None = 0,
		Position = 1,
		Primary2DAxis = 2,
		Secondary2DAxis = 4
	}

	[Serializable]
	[Obsolete("XRDeviceSimulator.SimulatedHandExpression has been deprecated in XRI 3.1.0. Update the XR Device Simulator sample in Package Manager or use the unnested version of SimulatedHandExpression instead.")]
	public class SimulatedHandExpression : ISerializationCallbackReceiver
	{
		[SerializeField]
		[Tooltip("The unique name for the hand expression.")]
		[Delayed]
		private string m_Name;

		[SerializeField]
		[Tooltip("The input action to trigger the hand expression.")]
		private InputActionReference m_ToggleAction;

		[SerializeField]
		[Tooltip("The captured hand expression to simulate when the input action is performed.")]
		private HandExpressionCapture m_Capture;

		private HandExpressionName m_ExpressionName;

		private Action<SimulatedHandExpression, InputAction.CallbackContext> m_Performed;

		private bool m_Subscribed;

		public string name => m_ExpressionName.ToString();

		public InputActionReference toggleAction => m_ToggleAction;

		internal HandExpressionCapture capture
		{
			get
			{
				return m_Capture;
			}
			set
			{
				m_Capture = value;
			}
		}

		internal HandExpressionName expressionName
		{
			get
			{
				return m_ExpressionName;
			}
			set
			{
				m_ExpressionName = value;
			}
		}

		public Sprite icon => m_Capture.icon;

		public event Action<SimulatedHandExpression, InputAction.CallbackContext> performed
		{
			add
			{
				m_Performed = (Action<SimulatedHandExpression, InputAction.CallbackContext>)Delegate.Combine(m_Performed, value);
				if (!m_Subscribed)
				{
					m_Subscribed = true;
					m_ToggleAction.action.performed += OnActionPerformed;
				}
			}
			remove
			{
				m_Performed = (Action<SimulatedHandExpression, InputAction.CallbackContext>)Delegate.Remove(m_Performed, value);
				if (m_Performed == null)
				{
					m_Subscribed = false;
					m_ToggleAction.action.performed -= OnActionPerformed;
				}
			}
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			m_Name = m_ExpressionName.ToString();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			m_ExpressionName = new HandExpressionName(m_Name);
		}

		private void OnActionPerformed(InputAction.CallbackContext context)
		{
			m_Performed?.Invoke(this, context);
		}
	}

	[Obsolete("DeviceMode has been deprecated in XRI 3.1.0 due to being moved out XR Device Simulator. Use DeviceMode in the SimulatedDeviceLifecycleManager instead.")]
	public enum DeviceMode
	{
		Controller,
		Hand
	}

	[SerializeField]
	[Tooltip("Input Action asset containing controls for the simulator itself. Unity will automatically enable and disable it with this component.")]
	private InputActionAsset m_DeviceSimulatorActionAsset;

	[SerializeField]
	[Tooltip("Input Action asset containing controls for the simulated controllers. Unity will automatically enable and disable it as needed.")]
	private InputActionAsset m_ControllerActionAsset;

	[SerializeField]
	[Tooltip("The Input System Action used to translate in the x-axis (left/right) while held. Must be a Value Axis Control.")]
	private InputActionReference m_KeyboardXTranslateAction;

	[SerializeField]
	[Tooltip("The Input System Action used to translate in the y-axis (up/down) while held. Must be a Value Axis Control.")]
	private InputActionReference m_KeyboardYTranslateAction;

	[SerializeField]
	[Tooltip("The Input System Action used to translate in the z-axis (forward/back) while held. Must be a Value Axis Control.")]
	private InputActionReference m_KeyboardZTranslateAction;

	[SerializeField]
	[Tooltip("The Input System Action used to enable manipulation of the left-hand controller while held. Must be a Button Control.")]
	private InputActionReference m_ManipulateLeftAction;

	[SerializeField]
	[Tooltip("The Input System Action used to enable manipulation of the right-hand controller while held. Must be a Button Control.")]
	private InputActionReference m_ManipulateRightAction;

	[SerializeField]
	[Tooltip("The Input System Action used to toggle enable manipulation of the left-hand controller when pressed. Must be a Button Control.")]
	private InputActionReference m_ToggleManipulateLeftAction;

	[SerializeField]
	[Tooltip("The Input System Action used to toggle enable manipulation of the right-hand controller when pressed. Must be a Button Control.")]
	private InputActionReference m_ToggleManipulateRightAction;

	[SerializeField]
	[Tooltip("The Input System Action used to toggle enable looking around with the HMD and controllers. Must be a Button Control.")]
	private InputActionReference m_ToggleManipulateBodyAction;

	[SerializeField]
	[Tooltip("The Input System Action used to enable manipulation of the HMD while held. Must be a Button Control.")]
	private InputActionReference m_ManipulateHeadAction;

	[SerializeField]
	[Tooltip("The Input System Action used to change between hand and controller mode. Must be a Button Control.")]
	private InputActionReference m_HandControllerModeAction;

	[SerializeField]
	[Tooltip("The Input System Action used to cycle between the different available devices. Must be a Button Control.")]
	private InputActionReference m_CycleDevicesAction;

	[SerializeField]
	[Tooltip("The Input System Action used to stop all manipulation. Must be a Button Control.")]
	private InputActionReference m_StopManipulationAction;

	[SerializeField]
	[Tooltip("The Input System Action used to translate or rotate by a scaled amount along or about the x- and y-axes. Must be a Value Vector2 Control.")]
	private InputActionReference m_MouseDeltaAction;

	[SerializeField]
	[Tooltip("The Input System Action used to translate or rotate by a scaled amount along or about the z-axis. Must be a Value Vector2 Control.")]
	private InputActionReference m_MouseScrollAction;

	[SerializeField]
	[Tooltip("The Input System Action used to cause the manipulated device(s) to rotate when moving the mouse when held. Must be a Button Control.")]
	private InputActionReference m_RotateModeOverrideAction;

	[SerializeField]
	[Tooltip("The Input System Action used to toggle between translating or rotating the manipulated device(s) when moving the mouse when pressed. Must be a Button Control.")]
	private InputActionReference m_ToggleMouseTransformationModeAction;

	[SerializeField]
	[Tooltip("The Input System Action used to cause the manipulated device(s) to rotate when moving the mouse while held when it would normally translate, and vice-versa. Must be a Button Control.")]
	private InputActionReference m_NegateModeAction;

	[SerializeField]
	[Tooltip("The Input System Action used to constrain the translation or rotation to the x-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
	private InputActionReference m_XConstraintAction;

	[SerializeField]
	[Tooltip("The Input System Action used to constrain the translation or rotation to the y-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
	private InputActionReference m_YConstraintAction;

	[SerializeField]
	[Tooltip("The Input System Action used to constrain the translation or rotation to the z-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
	private InputActionReference m_ZConstraintAction;

	[SerializeField]
	[Tooltip("The Input System Action used to cause the manipulated device(s) to reset position or rotation (depending on the effective manipulation mode). Must be a Button Control.")]
	private InputActionReference m_ResetAction;

	[SerializeField]
	[Tooltip("The Input System Action used to toggle the cursor lock mode for the game window when pressed. Must be a Button Control.")]
	private InputActionReference m_ToggleCursorLockAction;

	[SerializeField]
	[Tooltip("The Input System Action used to toggle enable translation from keyboard inputs when pressed. Must be a Button Control.")]
	private InputActionReference m_ToggleDevicePositionTargetAction;

	[SerializeField]
	[Tooltip("The Input System Action used to toggle enable manipulation of the Primary2DAxis of the controllers when pressed. Must be a Button Control.")]
	private InputActionReference m_TogglePrimary2DAxisTargetAction;

	[SerializeField]
	[Tooltip("The Input System Action used to toggle enable manipulation of the Secondary2DAxis of the controllers when pressed. Must be a Button Control.")]
	private InputActionReference m_ToggleSecondary2DAxisTargetAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the value of one or more 2D Axis controls on the manipulated controller device(s). Must be a Value Vector2 Control.")]
	private InputActionReference m_Axis2DAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control one or more 2D Axis controls on the opposite hand of the exclusively manipulated controller device. Must be a Value Vector2 Control.")]
	private InputActionReference m_RestingHandAxis2DAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the Grip control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_GripAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the Trigger control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_TriggerAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the PrimaryButton control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_PrimaryButtonAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the SecondaryButton control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_SecondaryButtonAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the Menu control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_MenuAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the Primary2DAxisClick control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_Primary2DAxisClickAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the Secondary2DAxisClick control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_Secondary2DAxisClickAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the Primary2DAxisTouch control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_Primary2DAxisTouchAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the Secondary2DAxisTouch control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_Secondary2DAxisTouchAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the PrimaryTouch control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_PrimaryTouchAction;

	[SerializeField]
	[Tooltip("The Input System Action used to control the SecondaryTouch control of the manipulated controller device(s). Must be a Button Control.")]
	private InputActionReference m_SecondaryTouchAction;

	[SerializeField]
	[Tooltip("Input Action asset containing controls for the simulated hands. Unity will automatically enable and disable it as needed.")]
	private InputActionAsset m_HandActionAsset;

	[SerializeField]
	[Tooltip("The Transform that contains the Camera. This is usually the \"Head\" of XR Origins. Automatically set to the first enabled camera tagged MainCamera if unset.")]
	private Transform m_CameraTransform;

	[SerializeField]
	[Tooltip("The coordinate space in which keyboard translation should operate.")]
	private Space m_KeyboardTranslateSpace;

	[SerializeField]
	[Tooltip("The coordinate space in which mouse translation should operate.")]
	private Space m_MouseTranslateSpace = Space.Screen;

	[SerializeField]
	[Tooltip("Speed of translation in the x-axis (left/right) when triggered by keyboard input.")]
	private float m_KeyboardXTranslateSpeed = 0.2f;

	[SerializeField]
	[Tooltip("Speed of translation in the y-axis (up/down) when triggered by keyboard input.")]
	private float m_KeyboardYTranslateSpeed = 0.2f;

	[SerializeField]
	[Tooltip("Speed of translation in the z-axis (forward/back) when triggered by keyboard input.")]
	private float m_KeyboardZTranslateSpeed = 0.2f;

	[SerializeField]
	[Tooltip("Speed multiplier applied for body translation when triggered by keyboard input.")]
	private float m_KeyboardBodyTranslateMultiplier = 5f;

	[SerializeField]
	[Tooltip("Sensitivity of translation in the x-axis (left/right) when triggered by mouse input.")]
	private float m_MouseXTranslateSensitivity = 0.0004f;

	[SerializeField]
	[Tooltip("Sensitivity of translation in the y-axis (up/down) when triggered by mouse input.")]
	private float m_MouseYTranslateSensitivity = 0.0004f;

	[SerializeField]
	[Tooltip("Sensitivity of translation in the z-axis (forward/back) when triggered by mouse scroll input.")]
	private float m_MouseScrollTranslateSensitivity = 0.0002f;

	[SerializeField]
	[Tooltip("Sensitivity of rotation along the x-axis (pitch) when triggered by mouse input.")]
	private float m_MouseXRotateSensitivity = 0.2f;

	[SerializeField]
	[Tooltip("Sensitivity of rotation along the y-axis (yaw) when triggered by mouse input.")]
	private float m_MouseYRotateSensitivity = 0.2f;

	[SerializeField]
	[Tooltip("Sensitivity of rotation along the z-axis (roll) when triggered by mouse scroll input.")]
	private float m_MouseScrollRotateSensitivity = 0.05f;

	[SerializeField]
	[Tooltip("A boolean value of whether to invert the y-axis of mouse input when rotating by mouse input.\nA false value (default) means typical FPS style where moving the mouse up/down pitches up/down.\nA true value means flight control style where moving the mouse up/down pitches down/up.")]
	private bool m_MouseYRotateInvert;

	[SerializeField]
	[Tooltip("The desired cursor lock mode to toggle to from None (either Locked or Confined).")]
	private CursorLockMode m_DesiredCursorLockMode = CursorLockMode.Locked;

	[SerializeField]
	[Tooltip("The optional Device Simulator UI prefab to use along with the XR Device Simulator.")]
	private GameObject m_DeviceSimulatorUI;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("The amount of the simulated grip on the controller when the Grip control is pressed.")]
	private float m_GripAmount = 1f;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("The amount of the simulated trigger pull on the controller when the Trigger control is pressed.")]
	private float m_TriggerAmount = 1f;

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

	internal static Action<bool> instanceChanged;

	private TargetedDevices m_TargetedDeviceInput = TargetedDevices.FPS;

	private (Transform transform, Camera camera) m_CachedCamera;

	private float m_KeyboardXTranslateInput;

	private float m_KeyboardYTranslateInput;

	private float m_KeyboardZTranslateInput;

	private Vector2 m_MouseDeltaInput;

	private Vector2 m_MouseScrollInput;

	private bool m_RotateModeOverrideInput;

	private bool m_XConstraintInput;

	private bool m_YConstraintInput;

	private bool m_ZConstraintInput;

	private bool m_ResetInput;

	private Vector2 m_Axis2DInput;

	private Vector2 m_RestingHandAxis2DInput;

	private bool m_GripInput;

	private bool m_TriggerInput;

	private bool m_PrimaryButtonInput;

	private bool m_SecondaryButtonInput;

	private bool m_MenuInput;

	private bool m_Primary2DAxisClickInput;

	private bool m_Secondary2DAxisClickInput;

	private bool m_Primary2DAxisTouchInput;

	private bool m_Secondary2DAxisTouchInput;

	private bool m_PrimaryTouchInput;

	private bool m_SecondaryTouchInput;

	private bool m_ManipulatedRestingHandAxis2D;

	private Vector3 m_LeftControllerEuler;

	private Vector3 m_RightControllerEuler;

	private Vector3 m_CenterEyeEuler;

	private XRSimulatedHMDState m_HMDState;

	private XRSimulatedControllerState m_LeftControllerState;

	private XRSimulatedControllerState m_RightControllerState;

	private XRSimulatedHandState m_LeftHandState;

	private XRSimulatedHandState m_RightHandState;

	private SimulatedDeviceLifecycleManager m_DeviceLifecycleManager;

	private SimulatedHandExpressionManager m_HandExpressionManager;

	[SerializeField]
	[Obsolete("m_RestingHandExpressionCapture has been deprecated in XRI 3.1.0 and moved to SimulatedHandExpressionManager.")]
	private HandExpressionCapture m_RestingHandExpressionCapture;

	[SerializeField]
	[Tooltip("The list of hand expressions to simulate.")]
	[Obsolete("m_SimulatedHandExpressions has been deprecated in XRI 3.1.0 and moved to SimulatedHandExpressionManager.")]
	private List<SimulatedHandExpression> m_SimulatedHandExpressions = new List<SimulatedHandExpression>();

	public InputActionAsset deviceSimulatorActionAsset
	{
		get
		{
			return m_DeviceSimulatorActionAsset;
		}
		set
		{
			m_DeviceSimulatorActionAsset = value;
		}
	}

	public InputActionAsset controllerActionAsset
	{
		get
		{
			return m_ControllerActionAsset;
		}
		set
		{
			m_ControllerActionAsset = value;
		}
	}

	public InputActionReference keyboardXTranslateAction
	{
		get
		{
			return m_KeyboardXTranslateAction;
		}
		set
		{
			UnsubscribeKeyboardXTranslateAction();
			m_KeyboardXTranslateAction = value;
			SubscribeKeyboardXTranslateAction();
		}
	}

	public InputActionReference keyboardYTranslateAction
	{
		get
		{
			return m_KeyboardYTranslateAction;
		}
		set
		{
			UnsubscribeKeyboardYTranslateAction();
			m_KeyboardYTranslateAction = value;
			SubscribeKeyboardYTranslateAction();
		}
	}

	public InputActionReference keyboardZTranslateAction
	{
		get
		{
			return m_KeyboardZTranslateAction;
		}
		set
		{
			UnsubscribeKeyboardZTranslateAction();
			m_KeyboardZTranslateAction = value;
			SubscribeKeyboardZTranslateAction();
		}
	}

	public InputActionReference manipulateLeftAction
	{
		get
		{
			return m_ManipulateLeftAction;
		}
		set
		{
			UnsubscribeManipulateLeftAction();
			m_ManipulateLeftAction = value;
			SubscribeManipulateLeftAction();
		}
	}

	public InputActionReference manipulateRightAction
	{
		get
		{
			return m_ManipulateRightAction;
		}
		set
		{
			UnsubscribeManipulateRightAction();
			m_ManipulateRightAction = value;
			SubscribeManipulateRightAction();
		}
	}

	public InputActionReference toggleManipulateLeftAction
	{
		get
		{
			return m_ToggleManipulateLeftAction;
		}
		set
		{
			UnsubscribeToggleManipulateLeftAction();
			m_ToggleManipulateLeftAction = value;
			SubscribeToggleManipulateLeftAction();
		}
	}

	public InputActionReference toggleManipulateRightAction
	{
		get
		{
			return m_ToggleManipulateRightAction;
		}
		set
		{
			UnsubscribeToggleManipulateRightAction();
			m_ToggleManipulateRightAction = value;
			SubscribeToggleManipulateRightAction();
		}
	}

	public InputActionReference toggleManipulateBodyAction
	{
		get
		{
			return m_ToggleManipulateBodyAction;
		}
		set
		{
			UnsubscribeToggleManipulateBodyAction();
			m_ToggleManipulateBodyAction = value;
			SubscribeToggleManipulateBodyAction();
		}
	}

	public InputActionReference manipulateHeadAction
	{
		get
		{
			return m_ManipulateHeadAction;
		}
		set
		{
			UnsubscribeManipulateHeadAction();
			m_ManipulateHeadAction = value;
			SubscribeManipulateHeadAction();
		}
	}

	public InputActionReference handControllerModeAction
	{
		get
		{
			return m_HandControllerModeAction;
		}
		set
		{
			UnsubscribeHandControllerModeAction();
			m_HandControllerModeAction = value;
			SubscribeHandControllerModeAction();
		}
	}

	public InputActionReference cycleDevicesAction
	{
		get
		{
			return m_CycleDevicesAction;
		}
		set
		{
			UnsubscribeCycleDevicesAction();
			m_CycleDevicesAction = value;
			SubscribeCycleDevicesAction();
		}
	}

	public InputActionReference stopManipulationAction
	{
		get
		{
			return m_StopManipulationAction;
		}
		set
		{
			UnsubscribeStopManipulationAction();
			m_StopManipulationAction = value;
			SubscribeStopManipulationAction();
		}
	}

	public InputActionReference mouseDeltaAction
	{
		get
		{
			return m_MouseDeltaAction;
		}
		set
		{
			UnsubscribeMouseDeltaAction();
			m_MouseDeltaAction = value;
			SubscribeMouseDeltaAction();
		}
	}

	public InputActionReference mouseScrollAction
	{
		get
		{
			return m_MouseScrollAction;
		}
		set
		{
			UnsubscribeMouseScrollAction();
			m_MouseScrollAction = value;
			SubscribeMouseScrollAction();
		}
	}

	public InputActionReference rotateModeOverrideAction
	{
		get
		{
			return m_RotateModeOverrideAction;
		}
		set
		{
			UnsubscribeRotateModeOverrideAction();
			m_RotateModeOverrideAction = value;
			SubscribeRotateModeOverrideAction();
		}
	}

	public InputActionReference toggleMouseTransformationModeAction
	{
		get
		{
			return m_ToggleMouseTransformationModeAction;
		}
		set
		{
			UnsubscribeToggleMouseTransformationModeAction();
			m_ToggleMouseTransformationModeAction = value;
			SubscribeToggleMouseTransformationModeAction();
		}
	}

	public InputActionReference negateModeAction
	{
		get
		{
			return m_NegateModeAction;
		}
		set
		{
			UnsubscribeNegateModeAction();
			m_NegateModeAction = value;
			SubscribeNegateModeAction();
		}
	}

	public InputActionReference xConstraintAction
	{
		get
		{
			return m_XConstraintAction;
		}
		set
		{
			UnsubscribeXConstraintAction();
			m_XConstraintAction = value;
			SubscribeXConstraintAction();
		}
	}

	public InputActionReference yConstraintAction
	{
		get
		{
			return m_YConstraintAction;
		}
		set
		{
			UnsubscribeYConstraintAction();
			m_YConstraintAction = value;
			SubscribeYConstraintAction();
		}
	}

	public InputActionReference zConstraintAction
	{
		get
		{
			return m_ZConstraintAction;
		}
		set
		{
			UnsubscribeZConstraintAction();
			m_ZConstraintAction = value;
			SubscribeZConstraintAction();
		}
	}

	public InputActionReference resetAction
	{
		get
		{
			return m_ResetAction;
		}
		set
		{
			UnsubscribeResetAction();
			m_ResetAction = value;
			SubscribeResetAction();
		}
	}

	public InputActionReference toggleCursorLockAction
	{
		get
		{
			return m_ToggleCursorLockAction;
		}
		set
		{
			UnsubscribeToggleCursorLockAction();
			m_ToggleCursorLockAction = value;
			SubscribeToggleCursorLockAction();
		}
	}

	public InputActionReference toggleDevicePositionTargetAction
	{
		get
		{
			return m_ToggleDevicePositionTargetAction;
		}
		set
		{
			UnsubscribeToggleDevicePositionTargetAction();
			m_ToggleDevicePositionTargetAction = value;
			SubscribeToggleDevicePositionTargetAction();
		}
	}

	public InputActionReference togglePrimary2DAxisTargetAction
	{
		get
		{
			return m_TogglePrimary2DAxisTargetAction;
		}
		set
		{
			UnsubscribeTogglePrimary2DAxisTargetAction();
			m_TogglePrimary2DAxisTargetAction = value;
			SubscribeTogglePrimary2DAxisTargetAction();
		}
	}

	public InputActionReference toggleSecondary2DAxisTargetAction
	{
		get
		{
			return m_ToggleSecondary2DAxisTargetAction;
		}
		set
		{
			UnsubscribeToggleSecondary2DAxisTargetAction();
			m_ToggleSecondary2DAxisTargetAction = value;
			SubscribeToggleSecondary2DAxisTargetAction();
		}
	}

	public InputActionReference axis2DAction
	{
		get
		{
			return m_Axis2DAction;
		}
		set
		{
			UnsubscribeAxis2DAction();
			m_Axis2DAction = value;
			SubscribeAxis2DAction();
		}
	}

	public InputActionReference restingHandAxis2DAction
	{
		get
		{
			return m_RestingHandAxis2DAction;
		}
		set
		{
			UnsubscribeRestingHandAxis2DAction();
			m_RestingHandAxis2DAction = value;
			SubscribeRestingHandAxis2DAction();
		}
	}

	public InputActionReference gripAction
	{
		get
		{
			return m_GripAction;
		}
		set
		{
			UnsubscribeGripAction();
			m_GripAction = value;
			SubscribeGripAction();
		}
	}

	public InputActionReference triggerAction
	{
		get
		{
			return m_TriggerAction;
		}
		set
		{
			UnsubscribeTriggerAction();
			m_TriggerAction = value;
			SubscribeTriggerAction();
		}
	}

	public InputActionReference primaryButtonAction
	{
		get
		{
			return m_PrimaryButtonAction;
		}
		set
		{
			UnsubscribePrimaryButtonAction();
			m_PrimaryButtonAction = value;
			SubscribePrimaryButtonAction();
		}
	}

	public InputActionReference secondaryButtonAction
	{
		get
		{
			return m_SecondaryButtonAction;
		}
		set
		{
			UnsubscribeSecondaryButtonAction();
			m_SecondaryButtonAction = value;
			SubscribeSecondaryButtonAction();
		}
	}

	public InputActionReference menuAction
	{
		get
		{
			return m_MenuAction;
		}
		set
		{
			UnsubscribeMenuAction();
			m_MenuAction = value;
			SubscribeMenuAction();
		}
	}

	public InputActionReference primary2DAxisClickAction
	{
		get
		{
			return m_Primary2DAxisClickAction;
		}
		set
		{
			UnsubscribePrimary2DAxisClickAction();
			m_Primary2DAxisClickAction = value;
			SubscribePrimary2DAxisClickAction();
		}
	}

	public InputActionReference secondary2DAxisClickAction
	{
		get
		{
			return m_Secondary2DAxisClickAction;
		}
		set
		{
			UnsubscribeSecondary2DAxisClickAction();
			m_Secondary2DAxisClickAction = value;
			SubscribeSecondary2DAxisClickAction();
		}
	}

	public InputActionReference primary2DAxisTouchAction
	{
		get
		{
			return m_Primary2DAxisTouchAction;
		}
		set
		{
			UnsubscribePrimary2DAxisTouchAction();
			m_Primary2DAxisTouchAction = value;
			SubscribePrimary2DAxisTouchAction();
		}
	}

	public InputActionReference secondary2DAxisTouchAction
	{
		get
		{
			return m_Secondary2DAxisTouchAction;
		}
		set
		{
			UnsubscribeSecondary2DAxisTouchAction();
			m_Secondary2DAxisTouchAction = value;
			SubscribeSecondary2DAxisTouchAction();
		}
	}

	public InputActionReference primaryTouchAction
	{
		get
		{
			return m_PrimaryTouchAction;
		}
		set
		{
			UnsubscribePrimaryTouchAction();
			m_PrimaryTouchAction = value;
			SubscribePrimaryTouchAction();
		}
	}

	public InputActionReference secondaryTouchAction
	{
		get
		{
			return m_SecondaryTouchAction;
		}
		set
		{
			UnsubscribeSecondaryTouchAction();
			m_SecondaryTouchAction = value;
			SubscribeSecondaryTouchAction();
		}
	}

	public InputActionAsset handActionAsset
	{
		get
		{
			return m_HandActionAsset;
		}
		set
		{
			m_HandActionAsset = value;
		}
	}

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

	public Space keyboardTranslateSpace
	{
		get
		{
			return m_KeyboardTranslateSpace;
		}
		set
		{
			m_KeyboardTranslateSpace = value;
		}
	}

	public Space mouseTranslateSpace
	{
		get
		{
			return m_MouseTranslateSpace;
		}
		set
		{
			m_MouseTranslateSpace = value;
		}
	}

	public float keyboardXTranslateSpeed
	{
		get
		{
			return m_KeyboardXTranslateSpeed;
		}
		set
		{
			m_KeyboardXTranslateSpeed = value;
		}
	}

	public float keyboardYTranslateSpeed
	{
		get
		{
			return m_KeyboardYTranslateSpeed;
		}
		set
		{
			m_KeyboardYTranslateSpeed = value;
		}
	}

	public float keyboardZTranslateSpeed
	{
		get
		{
			return m_KeyboardZTranslateSpeed;
		}
		set
		{
			m_KeyboardZTranslateSpeed = value;
		}
	}

	public float keyboardBodyTranslateMultiplier
	{
		get
		{
			return m_KeyboardBodyTranslateMultiplier;
		}
		set
		{
			m_KeyboardBodyTranslateMultiplier = value;
		}
	}

	public float mouseXTranslateSensitivity
	{
		get
		{
			return m_MouseXTranslateSensitivity;
		}
		set
		{
			m_MouseXTranslateSensitivity = value;
		}
	}

	public float mouseYTranslateSensitivity
	{
		get
		{
			return m_MouseYTranslateSensitivity;
		}
		set
		{
			m_MouseYTranslateSensitivity = value;
		}
	}

	public float mouseScrollTranslateSensitivity
	{
		get
		{
			return m_MouseScrollTranslateSensitivity;
		}
		set
		{
			m_MouseScrollTranslateSensitivity = value;
		}
	}

	public float mouseXRotateSensitivity
	{
		get
		{
			return m_MouseXRotateSensitivity;
		}
		set
		{
			m_MouseXRotateSensitivity = value;
		}
	}

	public float mouseYRotateSensitivity
	{
		get
		{
			return m_MouseYRotateSensitivity;
		}
		set
		{
			m_MouseYRotateSensitivity = value;
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

	public bool mouseYRotateInvert
	{
		get
		{
			return m_MouseYRotateInvert;
		}
		set
		{
			m_MouseYRotateInvert = value;
		}
	}

	public CursorLockMode desiredCursorLockMode
	{
		get
		{
			return m_DesiredCursorLockMode;
		}
		set
		{
			m_DesiredCursorLockMode = value;
		}
	}

	public GameObject deviceSimulatorUI
	{
		get
		{
			return m_DeviceSimulatorUI;
		}
		set
		{
			m_DeviceSimulatorUI = value;
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

	public TransformationMode mouseTransformationMode { get; set; } = TransformationMode.Rotate;


	public bool negateMode { get; private set; }

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

	public bool manipulatingFPS => m_TargetedDeviceInput == TargetedDevices.FPS;

	public static XRDeviceSimulator instance { get; private set; }

	private TargetedDevices targetedDeviceInput
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

	[Obsolete("simulatedHandExpressions has been deprecated in XRI 3.1.0. Update the XR Device Simulator sample in Package Manager or use simulatedHandExpressions in the SimulatedHandExpressionManager instead.")]
	public List<SimulatedHandExpression> simulatedHandExpressions => m_SimulatedHandExpressions;

	[Obsolete("removeOtherHMDDevices has been deprecated in XRI 3.1.0. Use removeOtherHMDDevices in the SimulatedDeviceLifecycleManager instead.")]
	public bool removeOtherHMDDevices
	{
		get
		{
			if (!(m_DeviceLifecycleManager != null))
			{
				return false;
			}
			return m_DeviceLifecycleManager.removeOtherHMDDevices;
		}
		set
		{
			if (m_DeviceLifecycleManager != null)
			{
				m_DeviceLifecycleManager.removeOtherHMDDevices = value;
			}
		}
	}

	[Obsolete("handTrackingCapability has been deprecated in XRI 3.1.0. Use handTrackingCapability in the SimulatedDeviceLifecycleManager instead.")]
	public bool handTrackingCapability
	{
		get
		{
			if (!(m_DeviceLifecycleManager != null))
			{
				return false;
			}
			return m_DeviceLifecycleManager.handTrackingCapability;
		}
		set
		{
			if (m_DeviceLifecycleManager != null)
			{
				m_DeviceLifecycleManager.handTrackingCapability = value;
			}
		}
	}

	[Obsolete("deviceMode has been deprecated in XRI 3.1.0 due to being moved out XR Device Simulator. Use deviceMode in the SimulatedDeviceLifecycleManager instead.")]
	public DeviceMode deviceMode
	{
		get
		{
			if (!(m_DeviceLifecycleManager != null))
			{
				return DeviceMode.Controller;
			}
			return (DeviceMode)m_DeviceLifecycleManager.deviceMode;
		}
	}

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = this;
			instanceChanged?.Invoke(obj: true);
		}
		else if (instance != this)
		{
			Debug.LogWarning($"Another instance of XR Device Simulator already exists ({instance}), destroying {base.gameObject}.", this);
			Object.Destroy(base.gameObject);
			return;
		}
		m_DeviceLifecycleManager = XRSimulatorUtility.FindCreateSimulatedDeviceLifecycleManager(base.gameObject);
		m_HandExpressionManager = XRSimulatorUtility.FindCreateSimulatedHandExpressionManager(base.gameObject);
		if (m_DeviceSimulatorActionAsset == null)
		{
			if (m_ManipulateLeftAction != null)
			{
				m_DeviceSimulatorActionAsset = m_ManipulateLeftAction.asset;
			}
			if (m_DeviceSimulatorActionAsset == null && m_ManipulateRightAction != null)
			{
				m_DeviceSimulatorActionAsset = m_ManipulateRightAction.asset;
			}
			if (m_DeviceSimulatorActionAsset == null)
			{
				Debug.LogError("No Device Simulator Action Asset has been defined, please assign one for the XR Device Simulator to work.", this);
			}
			else
			{
				Debug.LogWarning("No Device Simulator Action Asset has been defined for the XR Device Simulator, using a default one: " + m_DeviceSimulatorActionAsset.name, m_DeviceSimulatorActionAsset);
			}
		}
		if (m_ControllerActionAsset == null)
		{
			if (gripAction != null)
			{
				m_ControllerActionAsset = gripAction.asset;
			}
			if (m_ControllerActionAsset == null)
			{
				Debug.LogError("No Controller Action Asset has been defined, please assign one for the XR Device Simulator to work.", this);
			}
			else
			{
				Debug.LogWarning("No Controller Action Asset has been defined for the XR Device Simulator, using a default one: " + m_ControllerActionAsset.name, m_ControllerActionAsset);
			}
		}
		if (m_HandActionAsset == null)
		{
			if (m_SimulatedHandExpressions.Count > 0)
			{
				if (m_SimulatedHandExpressions[0].toggleAction != null)
				{
					m_HandActionAsset = m_SimulatedHandExpressions[0].toggleAction.asset;
				}
			}
			else if (m_HandExpressionManager.simulatedHandExpressions.Count > 0 && m_HandExpressionManager.simulatedHandExpressions[0].toggleInput.inputActionReferencePerformed != null)
			{
				m_HandActionAsset = m_HandExpressionManager.simulatedHandExpressions[0].toggleInput.inputActionReferencePerformed.asset;
			}
			if (m_HandActionAsset == null)
			{
				Debug.LogError("No Hand Action Asset has been defined, please assign one for the XR Device Simulator to work.", this);
			}
			else
			{
				Debug.LogWarning("No Hand Action Asset has been defined for the XR Device Simulator, using a default one: " + m_HandActionAsset.name, m_HandActionAsset);
			}
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
		if (m_DeviceSimulatorUI != null)
		{
			Object.Instantiate(m_DeviceSimulatorUI, base.transform);
		}
	}

	protected virtual void OnEnable()
	{
		XRSimulatorUtility.FindCameraTransform(ref m_CachedCamera, ref m_CameraTransform);
		SubscribeKeyboardXTranslateAction();
		SubscribeKeyboardYTranslateAction();
		SubscribeKeyboardZTranslateAction();
		SubscribeManipulateLeftAction();
		SubscribeToggleManipulateLeftAction();
		SubscribeManipulateRightAction();
		SubscribeToggleManipulateRightAction();
		SubscribeToggleManipulateBodyAction();
		SubscribeManipulateHeadAction();
		SubscribeStopManipulationAction();
		SubscribeHandControllerModeAction();
		SubscribeCycleDevicesAction();
		SubscribeMouseDeltaAction();
		SubscribeMouseScrollAction();
		SubscribeRotateModeOverrideAction();
		SubscribeToggleMouseTransformationModeAction();
		SubscribeNegateModeAction();
		SubscribeXConstraintAction();
		SubscribeYConstraintAction();
		SubscribeZConstraintAction();
		SubscribeResetAction();
		SubscribeToggleCursorLockAction();
		SubscribeToggleDevicePositionTargetAction();
		SubscribeTogglePrimary2DAxisTargetAction();
		SubscribeToggleSecondary2DAxisTargetAction();
		SubscribeAxis2DAction();
		SubscribeRestingHandAxis2DAction();
		SubscribeGripAction();
		SubscribeTriggerAction();
		SubscribePrimaryButtonAction();
		SubscribeSecondaryButtonAction();
		SubscribeMenuAction();
		SubscribePrimary2DAxisClickAction();
		SubscribeSecondary2DAxisClickAction();
		SubscribePrimary2DAxisTouchAction();
		SubscribeSecondary2DAxisTouchAction();
		SubscribePrimaryTouchAction();
		SubscribeSecondaryTouchAction();
		if (m_ControllerActionAsset != null)
		{
			m_ControllerActionAsset.Enable();
		}
		if (m_DeviceSimulatorActionAsset != null)
		{
			m_DeviceSimulatorActionAsset.Enable();
		}
	}

	protected virtual void OnDisable()
	{
		UnsubscribeKeyboardXTranslateAction();
		UnsubscribeKeyboardYTranslateAction();
		UnsubscribeKeyboardZTranslateAction();
		UnsubscribeManipulateLeftAction();
		UnsubscribeToggleManipulateLeftAction();
		UnsubscribeManipulateRightAction();
		UnsubscribeToggleManipulateRightAction();
		UnsubscribeToggleManipulateBodyAction();
		UnsubscribeManipulateHeadAction();
		UnsubscribeStopManipulationAction();
		UnsubscribeHandControllerModeAction();
		UnsubscribeCycleDevicesAction();
		UnsubscribeMouseDeltaAction();
		UnsubscribeMouseScrollAction();
		UnsubscribeRotateModeOverrideAction();
		UnsubscribeToggleMouseTransformationModeAction();
		UnsubscribeNegateModeAction();
		UnsubscribeXConstraintAction();
		UnsubscribeYConstraintAction();
		UnsubscribeZConstraintAction();
		UnsubscribeResetAction();
		UnsubscribeToggleCursorLockAction();
		UnsubscribeToggleDevicePositionTargetAction();
		UnsubscribeTogglePrimary2DAxisTargetAction();
		UnsubscribeToggleSecondary2DAxisTargetAction();
		UnsubscribeAxis2DAction();
		UnsubscribeRestingHandAxis2DAction();
		UnsubscribeGripAction();
		UnsubscribeTriggerAction();
		UnsubscribePrimaryButtonAction();
		UnsubscribeSecondaryButtonAction();
		UnsubscribeMenuAction();
		UnsubscribePrimary2DAxisClickAction();
		UnsubscribeSecondary2DAxisClickAction();
		UnsubscribePrimary2DAxisTouchAction();
		UnsubscribeSecondary2DAxisTouchAction();
		UnsubscribePrimaryTouchAction();
		UnsubscribeSecondaryTouchAction();
		if (m_ControllerActionAsset != null)
		{
			m_ControllerActionAsset.Disable();
		}
		if (m_DeviceSimulatorActionAsset != null)
		{
			m_DeviceSimulatorActionAsset.Disable();
		}
	}

	protected virtual void OnDestroy()
	{
		if (instance == this)
		{
			instanceChanged?.Invoke(obj: false);
		}
	}

	protected virtual void Start()
	{
		InitializeHandExpressions();
	}

	protected virtual void Update()
	{
		ProcessPoseInput();
		ProcessControlInput();
		ProcessHandExpressionInput();
		m_DeviceLifecycleManager.ApplyHandState(m_LeftHandState, m_RightHandState);
		m_DeviceLifecycleManager.ApplyHMDState(m_HMDState);
		m_DeviceLifecycleManager.ApplyControllerState(m_LeftControllerState, m_RightControllerState);
	}

	protected virtual void ProcessPoseInput()
	{
		m_LeftControllerState.isTracked = m_LeftControllerIsTracked;
		m_RightControllerState.isTracked = m_RightControllerIsTracked;
		m_LeftHandState.isTracked = m_LeftHandIsTracked;
		m_RightHandState.isTracked = m_RightHandIsTracked;
		m_HMDState.isTracked = m_HMDIsTracked;
		m_LeftControllerState.trackingState = (int)m_LeftControllerTrackingState;
		m_RightControllerState.trackingState = (int)m_RightControllerTrackingState;
		m_HMDState.trackingState = (int)m_HMDTrackingState;
		if (m_TargetedDeviceInput == TargetedDevices.None || !XRSimulatorUtility.FindCameraTransform(ref m_CachedCamera, ref m_CameraTransform))
		{
			return;
		}
		Transform parent = m_CameraTransform.parent;
		Quaternion quaternion = ((parent != null) ? parent.rotation : Quaternion.identity);
		Quaternion inverseCameraParentRotation = Quaternion.Inverse(quaternion);
		if (m_TargetedDeviceInput == TargetedDevices.FPS && Time.time > 1f)
		{
			float xTranslateInput = m_KeyboardXTranslateInput * m_KeyboardXTranslateSpeed * m_KeyboardBodyTranslateMultiplier * Time.deltaTime;
			float yTranslateInput = m_KeyboardYTranslateInput * m_KeyboardYTranslateSpeed * m_KeyboardBodyTranslateMultiplier * Time.deltaTime;
			float zTranslateInput = m_KeyboardZTranslateInput * m_KeyboardZTranslateSpeed * m_KeyboardBodyTranslateMultiplier * Time.deltaTime;
			Vector3 translationInDeviceSpace = XRSimulatorUtility.GetTranslationInDeviceSpace(xTranslateInput, yTranslateInput, zTranslateInput, m_CameraTransform, quaternion, inverseCameraParentRotation);
			m_LeftControllerState.devicePosition += translationInDeviceSpace;
			m_RightControllerState.devicePosition += translationInDeviceSpace;
			m_LeftHandState.position += translationInDeviceSpace;
			m_RightHandState.position += translationInDeviceSpace;
			m_HMDState.centerEyePosition += translationInDeviceSpace;
			m_HMDState.devicePosition = m_HMDState.centerEyePosition;
			Vector3 vector = new Vector3(m_MouseDeltaInput.x * m_MouseXRotateSensitivity, m_MouseDeltaInput.y * m_MouseYRotateSensitivity * (m_MouseYRotateInvert ? 1f : (-1f)), m_MouseScrollInput.y * m_MouseScrollRotateSensitivity);
			Vector3 vector2 = ((m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput) ? new Vector3(0f - vector.x + vector.y, 0f, 0f) : ((m_XConstraintInput || !m_YConstraintInput || m_ZConstraintInput) ? new Vector3(vector.y, vector.x, 0f) : new Vector3(0f, vector.x + (0f - vector.y), 0f)));
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
			if (m_ResetInput)
			{
				m_LeftControllerState.devicePosition = XRSimulatorUtility.leftDeviceDefaultInitialPosition;
				m_RightControllerState.devicePosition = XRSimulatorUtility.rightDeviceDefaultInitialPosition;
				m_LeftControllerEuler = Vector3.zero;
				m_LeftControllerState.deviceRotation = Quaternion.Euler(m_LeftControllerEuler);
				m_RightControllerEuler = Vector3.zero;
				m_RightControllerState.deviceRotation = Quaternion.Euler(m_RightControllerEuler);
				m_LeftHandState.position = XRSimulatorUtility.leftDeviceDefaultInitialPosition;
				m_RightHandState.position = XRSimulatorUtility.rightDeviceDefaultInitialPosition;
				m_LeftHandState.euler = Vector3.zero;
				m_LeftHandState.rotation = Quaternion.Euler(m_LeftHandState.euler);
				m_RightHandState.euler = Vector3.zero;
				m_RightHandState.rotation = Quaternion.Euler(m_RightHandState.euler);
				m_HMDState.centerEyePosition = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);
				m_HMDState.devicePosition = m_HMDState.centerEyePosition;
				m_CenterEyeEuler = Vector3.zero;
				m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
				m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
			}
		}
		if ((axis2DTargets & Axis2DTargets.Position) != 0)
		{
			XRSimulatorUtility.GetAxes((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_KeyboardTranslateSpace, m_CameraTransform, out var right, out var up, out var forward);
			Vector3 vector3 = right * (m_KeyboardXTranslateInput * m_KeyboardXTranslateSpeed * Time.deltaTime) + up * (m_KeyboardYTranslateInput * m_KeyboardYTranslateSpeed * Time.deltaTime) + forward * (m_KeyboardZTranslateInput * m_KeyboardZTranslateSpeed * Time.deltaTime);
			if (manipulatingLeftController)
			{
				Quaternion deltaRotation = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_KeyboardTranslateSpace, in m_LeftControllerState, in inverseCameraParentRotation);
				m_LeftControllerState.devicePosition += deltaRotation * vector3;
			}
			if (manipulatingRightController)
			{
				Quaternion deltaRotation2 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_KeyboardTranslateSpace, in m_RightControllerState, in inverseCameraParentRotation);
				m_RightControllerState.devicePosition += deltaRotation2 * vector3;
			}
			if (manipulatingLeftHand)
			{
				Quaternion deltaRotation3 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_KeyboardTranslateSpace, in m_LeftHandState, in inverseCameraParentRotation);
				m_LeftHandState.position += deltaRotation3 * vector3;
			}
			if (manipulatingRightHand)
			{
				Quaternion deltaRotation4 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_KeyboardTranslateSpace, in m_RightHandState, in inverseCameraParentRotation);
				m_RightHandState.position += deltaRotation4 * vector3;
			}
			if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
			{
				Quaternion deltaRotation5 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_KeyboardTranslateSpace, in m_HMDState, in inverseCameraParentRotation);
				m_HMDState.centerEyePosition += deltaRotation5 * vector3;
				m_HMDState.devicePosition = m_HMDState.centerEyePosition;
			}
		}
		if ((mouseTransformationMode == TransformationMode.Translate && !m_RotateModeOverrideInput && !negateMode) || ((mouseTransformationMode == TransformationMode.Rotate || m_RotateModeOverrideInput) && negateMode))
		{
			XRSimulatorUtility.GetAxes((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_MouseTranslateSpace, m_CameraTransform, out var right2, out var up2, out var forward2);
			Vector3 vector4 = new Vector3(m_MouseDeltaInput.x * m_MouseXTranslateSensitivity, m_MouseDeltaInput.y * m_MouseYTranslateSensitivity, m_MouseScrollInput.y * m_MouseScrollTranslateSensitivity);
			Vector3 vector5 = ((m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) ? (right2 * vector4.x + forward2 * vector4.y) : ((!m_XConstraintInput && m_YConstraintInput && m_ZConstraintInput) ? (up2 * vector4.y + forward2 * vector4.x) : ((m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput) ? (right2 * (vector4.x + vector4.y)) : ((!m_XConstraintInput && m_YConstraintInput && !m_ZConstraintInput) ? (up2 * (vector4.x + vector4.y)) : ((m_XConstraintInput || m_YConstraintInput || !m_ZConstraintInput) ? (right2 * vector4.x + up2 * vector4.y) : (forward2 * (vector4.x + vector4.y)))))));
			vector5 += forward2 * vector4.z;
			if (manipulatingLeftController)
			{
				Quaternion deltaRotation6 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_MouseTranslateSpace, in m_LeftControllerState, in inverseCameraParentRotation);
				m_LeftControllerState.devicePosition += deltaRotation6 * vector5;
			}
			if (manipulatingRightController)
			{
				Quaternion deltaRotation7 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_MouseTranslateSpace, in m_RightControllerState, in inverseCameraParentRotation);
				m_RightControllerState.devicePosition += deltaRotation7 * vector5;
			}
			if (manipulatingLeftHand)
			{
				Quaternion deltaRotation8 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_MouseTranslateSpace, in m_LeftHandState, in inverseCameraParentRotation);
				m_LeftHandState.position += deltaRotation8 * vector5;
			}
			if (manipulatingRightHand)
			{
				Quaternion deltaRotation9 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)mouseTranslateSpace, in m_RightHandState, in inverseCameraParentRotation);
				m_RightHandState.position += deltaRotation9 * vector5;
			}
			if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
			{
				Quaternion deltaRotation10 = XRSimulatorUtility.GetDeltaRotation((UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Space)m_MouseTranslateSpace, in m_HMDState, in inverseCameraParentRotation);
				m_HMDState.centerEyePosition += deltaRotation10 * vector5;
				m_HMDState.devicePosition = m_HMDState.centerEyePosition;
			}
			if (!m_ResetInput)
			{
				return;
			}
			Vector3 resetScale = GetResetScale();
			if (manipulatingLeftController)
			{
				Vector3 devicePosition = Vector3.Scale(m_LeftControllerState.devicePosition, resetScale);
				if (devicePosition.magnitude <= 0f)
				{
					devicePosition = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);
				}
				m_LeftControllerState.devicePosition = devicePosition;
			}
			if (manipulatingRightController)
			{
				Vector3 devicePosition2 = Vector3.Scale(m_RightControllerState.devicePosition, resetScale);
				if (devicePosition2.magnitude <= 0f)
				{
					devicePosition2 = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);
				}
				m_RightControllerState.devicePosition = devicePosition2;
			}
			if (manipulatingLeftHand)
			{
				Vector3 position = Vector3.Scale(m_LeftHandState.position, resetScale);
				if (position.magnitude <= 0f)
				{
					position = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);
				}
				m_LeftHandState.position = position;
			}
			if (manipulatingRightHand)
			{
				Vector3 position2 = Vector3.Scale(m_RightHandState.position, resetScale);
				if (position2.magnitude <= 0f)
				{
					position2 = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);
				}
				m_RightHandState.position = position2;
			}
			if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
			{
				Vector3 centerEyePosition2 = Vector3.Scale(m_HMDState.centerEyePosition, resetScale);
				if (centerEyePosition2.magnitude <= 0f)
				{
					centerEyePosition2 = new Vector3(Mathf.Epsilon, Mathf.Epsilon, Mathf.Epsilon);
				}
				m_HMDState.centerEyePosition = centerEyePosition2;
				m_HMDState.devicePosition = m_HMDState.centerEyePosition;
			}
			return;
		}
		Vector3 vector6 = new Vector3(m_MouseDeltaInput.x * m_MouseXRotateSensitivity, m_MouseDeltaInput.y * m_MouseYRotateSensitivity * (m_MouseYRotateInvert ? 1f : (-1f)), m_MouseScrollInput.y * m_MouseScrollRotateSensitivity);
		Vector3 vector7 = ((m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) ? new Vector3(vector6.y, 0f, 0f - vector6.x) : ((!m_XConstraintInput && m_YConstraintInput && m_ZConstraintInput) ? new Vector3(0f, vector6.x, 0f - vector6.y) : ((m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput) ? new Vector3(0f - vector6.x + vector6.y, 0f, 0f) : ((!m_XConstraintInput && m_YConstraintInput && !m_ZConstraintInput) ? new Vector3(0f, vector6.x + (0f - vector6.y), 0f) : ((m_XConstraintInput || m_YConstraintInput || !m_ZConstraintInput) ? new Vector3(vector6.y, vector6.x, 0f) : new Vector3(0f, 0f, 0f - vector6.x + (0f - vector6.y)))))));
		vector7 += new Vector3(0f, 0f, vector6.z);
		if (manipulatingLeftController)
		{
			m_LeftControllerEuler += vector7;
			m_LeftControllerState.deviceRotation = Quaternion.Euler(m_LeftControllerEuler);
		}
		if (manipulatingRightController)
		{
			m_RightControllerEuler += vector7;
			m_RightControllerState.deviceRotation = Quaternion.Euler(m_RightControllerEuler);
		}
		if (manipulatingLeftHand)
		{
			m_LeftHandState.euler += vector7;
			m_LeftHandState.rotation = Quaternion.Euler(m_LeftHandState.euler);
		}
		if (manipulatingRightHand)
		{
			m_RightHandState.euler += vector7;
			m_RightHandState.rotation = Quaternion.Euler(m_RightHandState.euler);
		}
		if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
		{
			m_CenterEyeEuler += vector7;
			m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
			m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
		}
		if (m_ResetInput)
		{
			Vector3 resetScale2 = GetResetScale();
			if (manipulatingLeftController)
			{
				m_LeftControllerEuler = Vector3.Scale(m_LeftControllerEuler, resetScale2);
				m_LeftControllerState.deviceRotation = Quaternion.Euler(m_LeftControllerEuler);
			}
			if (manipulatingRightController)
			{
				m_RightControllerEuler = Vector3.Scale(m_RightControllerEuler, resetScale2);
				m_RightControllerState.deviceRotation = Quaternion.Euler(m_RightControllerEuler);
			}
			if (manipulatingLeftHand)
			{
				m_LeftHandState.euler = Vector3.Scale(m_LeftHandState.euler, resetScale2);
				m_LeftHandState.rotation = Quaternion.Euler(m_LeftHandState.euler);
			}
			if (manipulatingRightHand)
			{
				m_RightHandState.euler = Vector3.Scale(m_RightHandState.euler, resetScale2);
				m_RightHandState.rotation = Quaternion.Euler(m_RightHandState.euler);
			}
			if (m_TargetedDeviceInput.HasDevice(TargetedDevices.HMD))
			{
				m_CenterEyeEuler = Vector3.Scale(m_CenterEyeEuler, resetScale2);
				m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
				m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
			}
		}
	}

	protected virtual void ProcessControlInput()
	{
		if (m_DeviceLifecycleManager.deviceMode == SimulatedDeviceLifecycleManager.DeviceMode.Controller)
		{
			ProcessAxis2DControlInput();
			if (manipulatingLeftController)
			{
				ProcessButtonControlInput(ref m_LeftControllerState);
			}
			else
			{
				ProcessAnalogButtonControlInput(ref m_LeftControllerState);
			}
			if (manipulatingRightController)
			{
				ProcessButtonControlInput(ref m_RightControllerState);
			}
			else
			{
				ProcessAnalogButtonControlInput(ref m_RightControllerState);
			}
		}
	}

	private void ProcessHandExpressionInput()
	{
	}

	private void ToggleHandExpression(UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.SimulatedHandExpression simulatedExpression)
	{
	}

	protected virtual void ProcessAxis2DControlInput()
	{
		if ((m_TargetedDeviceInput & (TargetedDevices.LeftDevice | TargetedDevices.RightDevice)) == 0)
		{
			return;
		}
		if ((axis2DTargets & Axis2DTargets.Primary2DAxis) != 0)
		{
			if (manipulatingLeftController)
			{
				m_LeftControllerState.primary2DAxis = m_Axis2DInput;
			}
			if (manipulatingRightController)
			{
				m_RightControllerState.primary2DAxis = m_Axis2DInput;
			}
			if (manipulatingLeftController ^ manipulatingRightController)
			{
				if (m_RestingHandAxis2DInput != Vector2.zero || m_ManipulatedRestingHandAxis2D)
				{
					if (manipulatingLeftController)
					{
						m_RightControllerState.primary2DAxis = m_RestingHandAxis2DInput;
					}
					if (manipulatingRightController)
					{
						m_LeftControllerState.primary2DAxis = m_RestingHandAxis2DInput;
					}
					m_ManipulatedRestingHandAxis2D = m_RestingHandAxis2DInput != Vector2.zero;
				}
				else
				{
					m_ManipulatedRestingHandAxis2D = false;
				}
			}
		}
		if ((axis2DTargets & Axis2DTargets.Secondary2DAxis) == 0)
		{
			return;
		}
		if (manipulatingLeftController)
		{
			m_LeftControllerState.secondary2DAxis = m_Axis2DInput;
		}
		if (manipulatingRightController)
		{
			m_RightControllerState.secondary2DAxis = m_Axis2DInput;
		}
		if (!(manipulatingLeftController ^ manipulatingRightController))
		{
			return;
		}
		if (m_RestingHandAxis2DInput != Vector2.zero || m_ManipulatedRestingHandAxis2D)
		{
			if (manipulatingLeftController)
			{
				m_RightControllerState.secondary2DAxis = m_RestingHandAxis2DInput;
			}
			if (manipulatingRightController)
			{
				m_LeftControllerState.secondary2DAxis = m_RestingHandAxis2DInput;
			}
			m_ManipulatedRestingHandAxis2D = m_RestingHandAxis2DInput != Vector2.zero;
		}
		else
		{
			m_ManipulatedRestingHandAxis2D = false;
		}
	}

	protected virtual void ProcessButtonControlInput(ref XRSimulatedControllerState controllerState)
	{
		controllerState.grip = (m_GripInput ? m_GripAmount : 0f);
		controllerState.WithButton(ControllerButton.GripButton, m_GripInput);
		controllerState.trigger = (m_TriggerInput ? m_TriggerAmount : 0f);
		controllerState.WithButton(ControllerButton.TriggerButton, m_TriggerInput);
		controllerState.WithButton(ControllerButton.PrimaryButton, m_PrimaryButtonInput);
		controllerState.WithButton(ControllerButton.SecondaryButton, m_SecondaryButtonInput);
		controllerState.WithButton(ControllerButton.MenuButton, m_MenuInput);
		controllerState.WithButton(ControllerButton.Primary2DAxisClick, m_Primary2DAxisClickInput);
		controllerState.WithButton(ControllerButton.Secondary2DAxisClick, m_Secondary2DAxisClickInput);
		controllerState.WithButton(ControllerButton.Primary2DAxisTouch, m_Primary2DAxisTouchInput);
		controllerState.WithButton(ControllerButton.Secondary2DAxisTouch, m_Secondary2DAxisTouchInput);
		controllerState.WithButton(ControllerButton.PrimaryTouch, m_PrimaryTouchInput);
		controllerState.WithButton(ControllerButton.SecondaryTouch, m_SecondaryTouchInput);
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
		if (!m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput)
		{
			return Vector3.zero;
		}
		return new Vector3(m_XConstraintInput ? 0f : 1f, m_YConstraintInput ? 0f : 1f, m_ZConstraintInput ? 0f : 1f);
	}

	public static TransformationMode Negate(TransformationMode mode)
	{
		return mode switch
		{
			TransformationMode.Rotate => TransformationMode.Translate, 
			TransformationMode.Translate => TransformationMode.Rotate, 
			_ => TransformationMode.Rotate, 
		};
	}

	private CursorLockMode Negate(CursorLockMode mode)
	{
		switch (mode)
		{
		case CursorLockMode.None:
			return m_DesiredCursorLockMode;
		case CursorLockMode.Locked:
		case CursorLockMode.Confined:
			return CursorLockMode.None;
		default:
			return CursorLockMode.None;
		}
	}

	private void SubscribeKeyboardXTranslateAction()
	{
		XRSimulatorUtility.Subscribe(m_KeyboardXTranslateAction, OnKeyboardXTranslatePerformed, OnKeyboardXTranslateCanceled);
	}

	private void UnsubscribeKeyboardXTranslateAction()
	{
		XRSimulatorUtility.Unsubscribe(m_KeyboardXTranslateAction, OnKeyboardXTranslatePerformed, OnKeyboardXTranslateCanceled);
	}

	private void SubscribeKeyboardYTranslateAction()
	{
		XRSimulatorUtility.Subscribe(m_KeyboardYTranslateAction, OnKeyboardYTranslatePerformed, OnKeyboardYTranslateCanceled);
	}

	private void UnsubscribeKeyboardYTranslateAction()
	{
		XRSimulatorUtility.Unsubscribe(m_KeyboardYTranslateAction, OnKeyboardYTranslatePerformed, OnKeyboardYTranslateCanceled);
	}

	private void SubscribeKeyboardZTranslateAction()
	{
		XRSimulatorUtility.Subscribe(m_KeyboardZTranslateAction, OnKeyboardZTranslatePerformed, OnKeyboardZTranslateCanceled);
	}

	private void UnsubscribeKeyboardZTranslateAction()
	{
		XRSimulatorUtility.Unsubscribe(m_KeyboardZTranslateAction, OnKeyboardZTranslatePerformed, OnKeyboardZTranslateCanceled);
	}

	private void SubscribeManipulateLeftAction()
	{
		XRSimulatorUtility.Subscribe(m_ManipulateLeftAction, OnManipulateLeftPerformed, OnManipulateLeftCanceled);
	}

	private void UnsubscribeManipulateLeftAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ManipulateLeftAction, OnManipulateLeftPerformed, OnManipulateLeftCanceled);
	}

	private void SubscribeManipulateRightAction()
	{
		XRSimulatorUtility.Subscribe(m_ManipulateRightAction, OnManipulateRightPerformed, OnManipulateRightCanceled);
	}

	private void UnsubscribeManipulateRightAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ManipulateRightAction, OnManipulateRightPerformed, OnManipulateRightCanceled);
	}

	private void SubscribeToggleManipulateLeftAction()
	{
		XRSimulatorUtility.Subscribe(m_ToggleManipulateLeftAction, OnToggleManipulateLeftPerformed);
	}

	private void UnsubscribeToggleManipulateLeftAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ToggleManipulateLeftAction, OnToggleManipulateLeftPerformed);
	}

	private void SubscribeToggleManipulateRightAction()
	{
		XRSimulatorUtility.Subscribe(m_ToggleManipulateRightAction, OnToggleManipulateRightPerformed);
	}

	private void UnsubscribeToggleManipulateRightAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ToggleManipulateRightAction, OnToggleManipulateRightPerformed);
	}

	private void SubscribeToggleManipulateBodyAction()
	{
		XRSimulatorUtility.Subscribe(m_ToggleManipulateBodyAction, OnToggleManipulateBodyPerformed);
	}

	private void UnsubscribeToggleManipulateBodyAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ToggleManipulateBodyAction, OnToggleManipulateBodyPerformed);
	}

	private void SubscribeManipulateHeadAction()
	{
		XRSimulatorUtility.Subscribe(m_ManipulateHeadAction, OnManipulateHeadPerformed, OnManipulateHeadCanceled);
	}

	private void UnsubscribeManipulateHeadAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ManipulateHeadAction, OnManipulateHeadPerformed, OnManipulateHeadCanceled);
	}

	private void SubscribeHandControllerModeAction()
	{
		XRSimulatorUtility.Subscribe(m_HandControllerModeAction, OnHandControllerModePerformed);
	}

	private void UnsubscribeHandControllerModeAction()
	{
		XRSimulatorUtility.Unsubscribe(m_HandControllerModeAction, OnHandControllerModePerformed);
	}

	private void SubscribeCycleDevicesAction()
	{
		XRSimulatorUtility.Subscribe(m_CycleDevicesAction, OnCycleDevicesPerformed);
	}

	private void UnsubscribeCycleDevicesAction()
	{
		XRSimulatorUtility.Unsubscribe(m_CycleDevicesAction, OnCycleDevicesPerformed);
	}

	private void SubscribeStopManipulationAction()
	{
		XRSimulatorUtility.Subscribe(m_StopManipulationAction, OnStopManipulationPerformed);
	}

	private void UnsubscribeStopManipulationAction()
	{
		XRSimulatorUtility.Unsubscribe(m_StopManipulationAction, OnStopManipulationPerformed);
	}

	private void SubscribeMouseDeltaAction()
	{
		XRSimulatorUtility.Subscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);
	}

	private void UnsubscribeMouseDeltaAction()
	{
		XRSimulatorUtility.Unsubscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);
	}

	private void SubscribeMouseScrollAction()
	{
		XRSimulatorUtility.Subscribe(m_MouseScrollAction, OnMouseScrollPerformed, OnMouseScrollCanceled);
	}

	private void UnsubscribeMouseScrollAction()
	{
		XRSimulatorUtility.Unsubscribe(m_MouseScrollAction, OnMouseScrollPerformed, OnMouseScrollCanceled);
	}

	private void SubscribeRotateModeOverrideAction()
	{
		XRSimulatorUtility.Subscribe(m_RotateModeOverrideAction, OnRotateModeOverridePerformed, OnRotateModeOverrideCanceled);
	}

	private void UnsubscribeRotateModeOverrideAction()
	{
		XRSimulatorUtility.Unsubscribe(m_RotateModeOverrideAction, OnRotateModeOverridePerformed, OnRotateModeOverrideCanceled);
	}

	private void SubscribeToggleMouseTransformationModeAction()
	{
		XRSimulatorUtility.Subscribe(m_ToggleMouseTransformationModeAction, OnToggleMouseTransformationModePerformed);
	}

	private void UnsubscribeToggleMouseTransformationModeAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ToggleMouseTransformationModeAction, OnToggleMouseTransformationModePerformed);
	}

	private void SubscribeNegateModeAction()
	{
		XRSimulatorUtility.Subscribe(m_NegateModeAction, OnNegateModePerformed, OnNegateModeCanceled);
	}

	private void UnsubscribeNegateModeAction()
	{
		XRSimulatorUtility.Unsubscribe(m_NegateModeAction, OnNegateModePerformed, OnNegateModeCanceled);
	}

	private void SubscribeXConstraintAction()
	{
		XRSimulatorUtility.Subscribe(m_XConstraintAction, OnXConstraintPerformed, OnXConstraintCanceled);
	}

	private void UnsubscribeXConstraintAction()
	{
		XRSimulatorUtility.Unsubscribe(m_XConstraintAction, OnXConstraintPerformed, OnXConstraintCanceled);
	}

	private void SubscribeYConstraintAction()
	{
		XRSimulatorUtility.Subscribe(m_YConstraintAction, OnYConstraintPerformed, OnYConstraintCanceled);
	}

	private void UnsubscribeYConstraintAction()
	{
		XRSimulatorUtility.Unsubscribe(m_YConstraintAction, OnYConstraintPerformed, OnYConstraintCanceled);
	}

	private void SubscribeZConstraintAction()
	{
		XRSimulatorUtility.Subscribe(m_ZConstraintAction, OnZConstraintPerformed, OnZConstraintCanceled);
	}

	private void UnsubscribeZConstraintAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ZConstraintAction, OnZConstraintPerformed, OnZConstraintCanceled);
	}

	private void SubscribeResetAction()
	{
		XRSimulatorUtility.Subscribe(m_ResetAction, OnResetPerformed, OnResetCanceled);
	}

	private void UnsubscribeResetAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ResetAction, OnResetPerformed, OnResetCanceled);
	}

	private void SubscribeToggleCursorLockAction()
	{
		XRSimulatorUtility.Subscribe(m_ToggleCursorLockAction, OnToggleCursorLockPerformed);
	}

	private void UnsubscribeToggleCursorLockAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ToggleCursorLockAction, OnToggleCursorLockPerformed);
	}

	private void SubscribeToggleDevicePositionTargetAction()
	{
		XRSimulatorUtility.Subscribe(m_ToggleDevicePositionTargetAction, OnToggleDevicePositionTargetPerformed);
	}

	private void UnsubscribeToggleDevicePositionTargetAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ToggleDevicePositionTargetAction, OnToggleDevicePositionTargetPerformed);
	}

	private void SubscribeTogglePrimary2DAxisTargetAction()
	{
		XRSimulatorUtility.Subscribe(m_TogglePrimary2DAxisTargetAction, OnTogglePrimary2DAxisTargetPerformed);
	}

	private void UnsubscribeTogglePrimary2DAxisTargetAction()
	{
		XRSimulatorUtility.Unsubscribe(m_TogglePrimary2DAxisTargetAction, OnTogglePrimary2DAxisTargetPerformed);
	}

	private void SubscribeToggleSecondary2DAxisTargetAction()
	{
		XRSimulatorUtility.Subscribe(m_ToggleSecondary2DAxisTargetAction, OnToggleSecondary2DAxisTargetPerformed);
	}

	private void UnsubscribeToggleSecondary2DAxisTargetAction()
	{
		XRSimulatorUtility.Unsubscribe(m_ToggleSecondary2DAxisTargetAction, OnToggleSecondary2DAxisTargetPerformed);
	}

	private void SubscribeAxis2DAction()
	{
		XRSimulatorUtility.Subscribe(m_Axis2DAction, OnAxis2DPerformed, OnAxis2DCanceled);
	}

	private void UnsubscribeAxis2DAction()
	{
		XRSimulatorUtility.Unsubscribe(m_Axis2DAction, OnAxis2DPerformed, OnAxis2DCanceled);
	}

	private void SubscribeRestingHandAxis2DAction()
	{
		XRSimulatorUtility.Subscribe(m_RestingHandAxis2DAction, OnRestingHandAxis2DPerformed, OnRestingHandAxis2DCanceled);
	}

	private void UnsubscribeRestingHandAxis2DAction()
	{
		XRSimulatorUtility.Unsubscribe(m_RestingHandAxis2DAction, OnRestingHandAxis2DPerformed, OnRestingHandAxis2DCanceled);
	}

	private void SubscribeGripAction()
	{
		XRSimulatorUtility.Subscribe(m_GripAction, OnGripPerformed, OnGripCanceled);
	}

	private void UnsubscribeGripAction()
	{
		XRSimulatorUtility.Unsubscribe(m_GripAction, OnGripPerformed, OnGripCanceled);
	}

	private void SubscribeTriggerAction()
	{
		XRSimulatorUtility.Subscribe(m_TriggerAction, OnTriggerPerformed, OnTriggerCanceled);
	}

	private void UnsubscribeTriggerAction()
	{
		XRSimulatorUtility.Unsubscribe(m_TriggerAction, OnTriggerPerformed, OnTriggerCanceled);
	}

	private void SubscribePrimaryButtonAction()
	{
		XRSimulatorUtility.Subscribe(m_PrimaryButtonAction, OnPrimaryButtonPerformed, OnPrimaryButtonCanceled);
	}

	private void UnsubscribePrimaryButtonAction()
	{
		XRSimulatorUtility.Unsubscribe(m_PrimaryButtonAction, OnPrimaryButtonPerformed, OnPrimaryButtonCanceled);
	}

	private void SubscribeSecondaryButtonAction()
	{
		XRSimulatorUtility.Subscribe(m_SecondaryButtonAction, OnSecondaryButtonPerformed, OnSecondaryButtonCanceled);
	}

	private void UnsubscribeSecondaryButtonAction()
	{
		XRSimulatorUtility.Unsubscribe(m_SecondaryButtonAction, OnSecondaryButtonPerformed, OnSecondaryButtonCanceled);
	}

	private void SubscribeMenuAction()
	{
		XRSimulatorUtility.Subscribe(m_MenuAction, OnMenuPerformed, OnMenuCanceled);
	}

	private void UnsubscribeMenuAction()
	{
		XRSimulatorUtility.Unsubscribe(m_MenuAction, OnMenuPerformed, OnMenuCanceled);
	}

	private void SubscribePrimary2DAxisClickAction()
	{
		XRSimulatorUtility.Subscribe(m_Primary2DAxisClickAction, OnPrimary2DAxisClickPerformed, OnPrimary2DAxisClickCanceled);
	}

	private void UnsubscribePrimary2DAxisClickAction()
	{
		XRSimulatorUtility.Unsubscribe(m_Primary2DAxisClickAction, OnPrimary2DAxisClickPerformed, OnPrimary2DAxisClickCanceled);
	}

	private void SubscribeSecondary2DAxisClickAction()
	{
		XRSimulatorUtility.Subscribe(m_Secondary2DAxisClickAction, OnSecondary2DAxisClickPerformed, OnSecondary2DAxisClickCanceled);
	}

	private void UnsubscribeSecondary2DAxisClickAction()
	{
		XRSimulatorUtility.Unsubscribe(m_Secondary2DAxisClickAction, OnSecondary2DAxisClickPerformed, OnSecondary2DAxisClickCanceled);
	}

	private void SubscribePrimary2DAxisTouchAction()
	{
		XRSimulatorUtility.Subscribe(m_Primary2DAxisTouchAction, OnPrimary2DAxisTouchPerformed, OnPrimary2DAxisTouchCanceled);
	}

	private void UnsubscribePrimary2DAxisTouchAction()
	{
		XRSimulatorUtility.Unsubscribe(m_Primary2DAxisTouchAction, OnPrimary2DAxisTouchPerformed, OnPrimary2DAxisTouchCanceled);
	}

	private void SubscribeSecondary2DAxisTouchAction()
	{
		XRSimulatorUtility.Subscribe(m_Secondary2DAxisTouchAction, OnSecondary2DAxisTouchPerformed, OnSecondary2DAxisTouchCanceled);
	}

	private void UnsubscribeSecondary2DAxisTouchAction()
	{
		XRSimulatorUtility.Unsubscribe(m_Secondary2DAxisTouchAction, OnSecondary2DAxisTouchPerformed, OnSecondary2DAxisTouchCanceled);
	}

	private void SubscribePrimaryTouchAction()
	{
		XRSimulatorUtility.Subscribe(m_PrimaryTouchAction, OnPrimaryTouchPerformed, OnPrimaryTouchCanceled);
	}

	private void UnsubscribePrimaryTouchAction()
	{
		XRSimulatorUtility.Unsubscribe(m_PrimaryTouchAction, OnPrimaryTouchPerformed, OnPrimaryTouchCanceled);
	}

	private void SubscribeSecondaryTouchAction()
	{
		XRSimulatorUtility.Subscribe(m_SecondaryTouchAction, OnSecondaryTouchPerformed, OnSecondaryTouchCanceled);
	}

	private void UnsubscribeSecondaryTouchAction()
	{
		XRSimulatorUtility.Unsubscribe(m_SecondaryTouchAction, OnSecondaryTouchPerformed, OnSecondaryTouchCanceled);
	}

	private void OnKeyboardXTranslatePerformed(InputAction.CallbackContext context)
	{
		m_KeyboardXTranslateInput = context.ReadValue<float>();
	}

	private void OnKeyboardXTranslateCanceled(InputAction.CallbackContext context)
	{
		m_KeyboardXTranslateInput = 0f;
	}

	private void OnKeyboardYTranslatePerformed(InputAction.CallbackContext context)
	{
		m_KeyboardYTranslateInput = context.ReadValue<float>();
	}

	private void OnKeyboardYTranslateCanceled(InputAction.CallbackContext context)
	{
		m_KeyboardYTranslateInput = 0f;
	}

	private void OnKeyboardZTranslatePerformed(InputAction.CallbackContext context)
	{
		m_KeyboardZTranslateInput = context.ReadValue<float>();
	}

	private void OnKeyboardZTranslateCanceled(InputAction.CallbackContext context)
	{
		m_KeyboardZTranslateInput = 0f;
	}

	private void OnManipulateLeftPerformed(InputAction.CallbackContext context)
	{
		targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.LeftDevice);
	}

	private void OnManipulateLeftCanceled(InputAction.CallbackContext context)
	{
		targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.LeftDevice);
	}

	private void OnManipulateRightPerformed(InputAction.CallbackContext context)
	{
		targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.RightDevice);
	}

	private void OnManipulateRightCanceled(InputAction.CallbackContext context)
	{
		targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.RightDevice);
	}

	private void OnToggleManipulateLeftPerformed(InputAction.CallbackContext context)
	{
		targetedDeviceInput = (targetedDeviceInput.HasDevice(TargetedDevices.LeftDevice) ? TargetedDevices.FPS : targetedDeviceInput.WithDevice(TargetedDevices.LeftDevice).WithoutDevice(TargetedDevices.RightDevice));
	}

	private void OnToggleManipulateRightPerformed(InputAction.CallbackContext context)
	{
		targetedDeviceInput = (targetedDeviceInput.HasDevice(TargetedDevices.RightDevice) ? TargetedDevices.FPS : targetedDeviceInput.WithDevice(TargetedDevices.RightDevice).WithoutDevice(TargetedDevices.LeftDevice));
	}

	private void OnToggleManipulateBodyPerformed(InputAction.CallbackContext context)
	{
		targetedDeviceInput = TargetedDevices.FPS;
	}

	private void OnManipulateHeadPerformed(InputAction.CallbackContext context)
	{
		targetedDeviceInput = targetedDeviceInput.WithDevice(TargetedDevices.HMD);
	}

	private void OnManipulateHeadCanceled(InputAction.CallbackContext context)
	{
		targetedDeviceInput = targetedDeviceInput.WithoutDevice(TargetedDevices.HMD);
	}

	private void OnHandControllerModePerformed(InputAction.CallbackContext context)
	{
		if (m_DeviceLifecycleManager != null)
		{
			m_DeviceLifecycleManager.SwitchDeviceMode();
		}
	}

	private void OnCycleDevicesPerformed(InputAction.CallbackContext context)
	{
		if (targetedDeviceInput == TargetedDevices.None)
		{
			targetedDeviceInput = TargetedDevices.FPS;
		}
		else if (targetedDeviceInput == TargetedDevices.FPS)
		{
			targetedDeviceInput = TargetedDevices.LeftDevice;
		}
		else if (targetedDeviceInput.HasDevice(TargetedDevices.LeftDevice))
		{
			targetedDeviceInput = TargetedDevices.RightDevice;
		}
		else if (targetedDeviceInput.HasDevice(TargetedDevices.RightDevice))
		{
			targetedDeviceInput = TargetedDevices.FPS;
		}
	}

	private void OnStopManipulationPerformed(InputAction.CallbackContext context)
	{
		targetedDeviceInput = TargetedDevices.None;
	}

	private void OnMouseDeltaPerformed(InputAction.CallbackContext context)
	{
		m_MouseDeltaInput = context.ReadValue<Vector2>();
	}

	private void OnMouseDeltaCanceled(InputAction.CallbackContext context)
	{
		m_MouseDeltaInput = Vector2.zero;
	}

	private void OnMouseScrollPerformed(InputAction.CallbackContext context)
	{
		m_MouseScrollInput = context.ReadValue<Vector2>();
	}

	private void OnMouseScrollCanceled(InputAction.CallbackContext context)
	{
		m_MouseScrollInput = Vector2.zero;
	}

	private void OnRotateModeOverridePerformed(InputAction.CallbackContext context)
	{
		m_RotateModeOverrideInput = true;
	}

	private void OnRotateModeOverrideCanceled(InputAction.CallbackContext context)
	{
		m_RotateModeOverrideInput = false;
	}

	private void OnToggleMouseTransformationModePerformed(InputAction.CallbackContext context)
	{
		mouseTransformationMode = Negate(mouseTransformationMode);
	}

	private void OnNegateModePerformed(InputAction.CallbackContext context)
	{
		negateMode = true;
	}

	private void OnNegateModeCanceled(InputAction.CallbackContext context)
	{
		negateMode = false;
	}

	private void OnXConstraintPerformed(InputAction.CallbackContext context)
	{
		m_XConstraintInput = true;
	}

	private void OnXConstraintCanceled(InputAction.CallbackContext context)
	{
		m_XConstraintInput = false;
	}

	private void OnYConstraintPerformed(InputAction.CallbackContext context)
	{
		m_YConstraintInput = true;
	}

	private void OnYConstraintCanceled(InputAction.CallbackContext context)
	{
		m_YConstraintInput = false;
	}

	private void OnZConstraintPerformed(InputAction.CallbackContext context)
	{
		m_ZConstraintInput = true;
	}

	private void OnZConstraintCanceled(InputAction.CallbackContext context)
	{
		m_ZConstraintInput = false;
	}

	private void OnResetPerformed(InputAction.CallbackContext context)
	{
		m_ResetInput = true;
	}

	private void OnResetCanceled(InputAction.CallbackContext context)
	{
		m_ResetInput = false;
	}

	private void OnToggleCursorLockPerformed(InputAction.CallbackContext context)
	{
		Cursor.lockState = Negate(Cursor.lockState);
	}

	private void OnToggleDevicePositionTargetPerformed(InputAction.CallbackContext context)
	{
		axis2DTargets = (((axis2DTargets & Axis2DTargets.Position) == 0) ? Axis2DTargets.Position : Axis2DTargets.None);
	}

	private void OnTogglePrimary2DAxisTargetPerformed(InputAction.CallbackContext context)
	{
		axis2DTargets = (((axis2DTargets & Axis2DTargets.Primary2DAxis) == 0) ? Axis2DTargets.Primary2DAxis : Axis2DTargets.None);
	}

	private void OnToggleSecondary2DAxisTargetPerformed(InputAction.CallbackContext context)
	{
		axis2DTargets = (((axis2DTargets & Axis2DTargets.Secondary2DAxis) == 0) ? Axis2DTargets.Secondary2DAxis : Axis2DTargets.None);
	}

	private void OnAxis2DPerformed(InputAction.CallbackContext context)
	{
		m_Axis2DInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
	}

	private void OnAxis2DCanceled(InputAction.CallbackContext context)
	{
		m_Axis2DInput = Vector2.zero;
	}

	private void OnRestingHandAxis2DPerformed(InputAction.CallbackContext context)
	{
		m_RestingHandAxis2DInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
	}

	private void OnRestingHandAxis2DCanceled(InputAction.CallbackContext context)
	{
		m_RestingHandAxis2DInput = Vector2.zero;
	}

	private void OnGripPerformed(InputAction.CallbackContext context)
	{
		m_GripInput = true;
	}

	private void OnGripCanceled(InputAction.CallbackContext context)
	{
		m_GripInput = false;
	}

	private void OnTriggerPerformed(InputAction.CallbackContext context)
	{
		m_TriggerInput = true;
	}

	private void OnTriggerCanceled(InputAction.CallbackContext context)
	{
		m_TriggerInput = false;
	}

	private void OnPrimaryButtonPerformed(InputAction.CallbackContext context)
	{
		m_PrimaryButtonInput = true;
	}

	private void OnPrimaryButtonCanceled(InputAction.CallbackContext context)
	{
		m_PrimaryButtonInput = false;
	}

	private void OnSecondaryButtonPerformed(InputAction.CallbackContext context)
	{
		m_SecondaryButtonInput = true;
	}

	private void OnSecondaryButtonCanceled(InputAction.CallbackContext context)
	{
		m_SecondaryButtonInput = false;
	}

	private void OnMenuPerformed(InputAction.CallbackContext context)
	{
		m_MenuInput = true;
	}

	private void OnMenuCanceled(InputAction.CallbackContext context)
	{
		m_MenuInput = false;
	}

	private void OnPrimary2DAxisClickPerformed(InputAction.CallbackContext context)
	{
		m_Primary2DAxisClickInput = true;
	}

	private void OnPrimary2DAxisClickCanceled(InputAction.CallbackContext context)
	{
		m_Primary2DAxisClickInput = false;
	}

	private void OnSecondary2DAxisClickPerformed(InputAction.CallbackContext context)
	{
		m_Secondary2DAxisClickInput = true;
	}

	private void OnSecondary2DAxisClickCanceled(InputAction.CallbackContext context)
	{
		m_Secondary2DAxisClickInput = false;
	}

	private void OnPrimary2DAxisTouchPerformed(InputAction.CallbackContext context)
	{
		m_Primary2DAxisTouchInput = true;
	}

	private void OnPrimary2DAxisTouchCanceled(InputAction.CallbackContext context)
	{
		m_Primary2DAxisTouchInput = false;
	}

	private void OnSecondary2DAxisTouchPerformed(InputAction.CallbackContext context)
	{
		m_Secondary2DAxisTouchInput = true;
	}

	private void OnSecondary2DAxisTouchCanceled(InputAction.CallbackContext context)
	{
		m_Secondary2DAxisTouchInput = false;
	}

	private void OnPrimaryTouchPerformed(InputAction.CallbackContext context)
	{
		m_PrimaryTouchInput = true;
	}

	private void OnPrimaryTouchCanceled(InputAction.CallbackContext context)
	{
		m_PrimaryTouchInput = false;
	}

	private void OnSecondaryTouchPerformed(InputAction.CallbackContext context)
	{
		m_SecondaryTouchInput = true;
	}

	private void OnSecondaryTouchCanceled(InputAction.CallbackContext context)
	{
		m_SecondaryTouchInput = false;
	}

	[Obsolete("AddDevices has been deprecated in XRI 3.1.0 and will be removed in a future release. It has instead been moved to the SimulatedDeviceLifecycleManager.", false)]
	protected virtual void AddDevices()
	{
		if (m_DeviceLifecycleManager != null)
		{
			m_DeviceLifecycleManager.AddDevices();
		}
		else
		{
			Debug.LogError("No Simulated Device Lifecycle Manager has been found so AddDevices() will not be called.", this);
		}
	}

	[Obsolete("RemoveDevices has been deprecated in XRI 3.1.0 and will be removed in a future release. It has instead been moved to the SimulatedDeviceLifecycleManager.", false)]
	protected virtual void RemoveDevices()
	{
		if (m_DeviceLifecycleManager != null)
		{
			m_DeviceLifecycleManager.RemoveDevices();
		}
		else
		{
			Debug.LogError("No Simulated Device Lifecycle Manager has been found so RemoveDevices() will not be called.", this);
		}
	}

	[Obsolete("InitializeHandExpressions has been deprecated in XRI 3.1.0 and moved to SimulatedHandExpressionManager.")]
	private void InitializeHandExpressions()
	{
	}

	[Obsolete("ToggleHandExpressionDeprecated has been deprecated in XRI 3.1.0 and replaced with ToggleHandExpression.")]
	private void ToggleHandExpressionDeprecated(SimulatedHandExpression simulatedExpression)
	{
	}
}
