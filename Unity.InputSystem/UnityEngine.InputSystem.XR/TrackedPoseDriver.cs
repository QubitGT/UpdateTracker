using System;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.XR;

namespace UnityEngine.InputSystem.XR;

[Serializable]
[AddComponentMenu("XR/Tracked Pose Driver (Input System)")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.inputsystem@1.14/manual/TrackedInputDevices.html#tracked-pose-driver")]
public class TrackedPoseDriver : MonoBehaviour, ISerializationCallbackReceiver
{
	public enum TrackingType
	{
		RotationAndPosition,
		RotationOnly,
		PositionOnly
	}

	[Flags]
	private enum TrackingStates
	{
		None = 0,
		Position = 1,
		Rotation = 2
	}

	public enum UpdateType
	{
		UpdateAndBeforeRender,
		Update,
		BeforeRender
	}

	[SerializeField]
	[Tooltip("Which Transform properties to update.")]
	private TrackingType m_TrackingType;

	[SerializeField]
	[Tooltip("Updates the Transform properties after these phases of Input System event processing.")]
	private UpdateType m_UpdateType;

	[SerializeField]
	[Tooltip("Ignore Tracking State and always treat the input pose as valid.")]
	private bool m_IgnoreTrackingState;

	[SerializeField]
	[Tooltip("The input action to read the position value of a tracked device. Must be a Vector 3 control type.")]
	private InputActionProperty m_PositionInput;

	[SerializeField]
	[Tooltip("The input action to read the rotation value of a tracked device. Must be a Quaternion control type.")]
	private InputActionProperty m_RotationInput;

	[SerializeField]
	[Tooltip("The input action to read the tracking state value of a tracked device. Identifies if position and rotation have valid data. Must be an Integer control type.")]
	private InputActionProperty m_TrackingStateInput;

	private Vector3 m_CurrentPosition = Vector3.zero;

	private Quaternion m_CurrentRotation = Quaternion.identity;

	private TrackingStates m_CurrentTrackingState = TrackingStates.Position | TrackingStates.Rotation;

	private bool m_RotationBound;

	private bool m_PositionBound;

	private bool m_TrackingStateBound;

	private bool m_IsFirstUpdate = true;

	[Obsolete]
	[SerializeField]
	[HideInInspector]
	private InputAction m_PositionAction;

	[Obsolete]
	[SerializeField]
	[HideInInspector]
	private InputAction m_RotationAction;

	public TrackingType trackingType
	{
		get
		{
			return m_TrackingType;
		}
		set
		{
			m_TrackingType = value;
		}
	}

	public UpdateType updateType
	{
		get
		{
			return m_UpdateType;
		}
		set
		{
			m_UpdateType = value;
		}
	}

	public bool ignoreTrackingState
	{
		get
		{
			return m_IgnoreTrackingState;
		}
		set
		{
			m_IgnoreTrackingState = value;
		}
	}

	public InputActionProperty positionInput
	{
		get
		{
			return m_PositionInput;
		}
		set
		{
			if (Application.isPlaying)
			{
				UnbindPosition();
			}
			m_PositionInput = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				BindPosition();
			}
		}
	}

	public InputActionProperty rotationInput
	{
		get
		{
			return m_RotationInput;
		}
		set
		{
			if (Application.isPlaying)
			{
				UnbindRotation();
			}
			m_RotationInput = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				BindRotation();
			}
		}
	}

	public InputActionProperty trackingStateInput
	{
		get
		{
			return m_TrackingStateInput;
		}
		set
		{
			if (Application.isPlaying)
			{
				UnbindTrackingState();
			}
			m_TrackingStateInput = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				BindTrackingState();
			}
		}
	}

	public InputAction positionAction
	{
		get
		{
			return m_PositionInput.action;
		}
		set
		{
			positionInput = new InputActionProperty(value);
		}
	}

	public InputAction rotationAction
	{
		get
		{
			return m_RotationInput.action;
		}
		set
		{
			rotationInput = new InputActionProperty(value);
		}
	}

	private void BindActions()
	{
		BindPosition();
		BindRotation();
		BindTrackingState();
	}

	private void UnbindActions()
	{
		UnbindPosition();
		UnbindRotation();
		UnbindTrackingState();
	}

	private void BindPosition()
	{
		if (m_PositionBound)
		{
			return;
		}
		InputAction action = m_PositionInput.action;
		if (action != null)
		{
			action.performed += OnPositionPerformed;
			action.canceled += OnPositionCanceled;
			m_PositionBound = true;
			if (m_PositionInput.reference == null)
			{
				RenameAndEnable(action, base.gameObject.name + " - TPD - Position");
			}
		}
	}

	private void BindRotation()
	{
		if (m_RotationBound)
		{
			return;
		}
		InputAction action = m_RotationInput.action;
		if (action != null)
		{
			action.performed += OnRotationPerformed;
			action.canceled += OnRotationCanceled;
			m_RotationBound = true;
			if (m_RotationInput.reference == null)
			{
				RenameAndEnable(action, base.gameObject.name + " - TPD - Rotation");
			}
		}
	}

	private void BindTrackingState()
	{
		if (m_TrackingStateBound)
		{
			return;
		}
		InputAction action = m_TrackingStateInput.action;
		if (action != null)
		{
			action.performed += OnTrackingStatePerformed;
			action.canceled += OnTrackingStateCanceled;
			m_TrackingStateBound = true;
			if (m_TrackingStateInput.reference == null)
			{
				RenameAndEnable(action, base.gameObject.name + " - TPD - Tracking State");
			}
		}
	}

	private static void RenameAndEnable(InputAction action, string name)
	{
		action.Rename(name);
		action.Enable();
	}

	private void UnbindPosition()
	{
		if (!m_PositionBound)
		{
			return;
		}
		InputAction action = m_PositionInput.action;
		if (action != null)
		{
			if (m_PositionInput.reference == null)
			{
				action.Disable();
			}
			action.performed -= OnPositionPerformed;
			action.canceled -= OnPositionCanceled;
			m_PositionBound = false;
		}
	}

	private void UnbindRotation()
	{
		if (!m_RotationBound)
		{
			return;
		}
		InputAction action = m_RotationInput.action;
		if (action != null)
		{
			if (m_RotationInput.reference == null)
			{
				action.Disable();
			}
			action.performed -= OnRotationPerformed;
			action.canceled -= OnRotationCanceled;
			m_RotationBound = false;
		}
	}

	private void UnbindTrackingState()
	{
		if (!m_TrackingStateBound)
		{
			return;
		}
		InputAction action = m_TrackingStateInput.action;
		if (action != null)
		{
			if (m_TrackingStateInput.reference == null)
			{
				action.Disable();
			}
			action.performed -= OnTrackingStatePerformed;
			action.canceled -= OnTrackingStateCanceled;
			m_TrackingStateBound = false;
		}
	}

	private void OnPositionPerformed(InputAction.CallbackContext context)
	{
		m_CurrentPosition = context.ReadValue<Vector3>();
	}

	private void OnPositionCanceled(InputAction.CallbackContext context)
	{
		m_CurrentPosition = Vector3.zero;
	}

	private void OnRotationPerformed(InputAction.CallbackContext context)
	{
		m_CurrentRotation = context.ReadValue<Quaternion>();
	}

	private void OnRotationCanceled(InputAction.CallbackContext context)
	{
		m_CurrentRotation = Quaternion.identity;
	}

	private void OnTrackingStatePerformed(InputAction.CallbackContext context)
	{
		m_CurrentTrackingState = (TrackingStates)context.ReadValue<int>();
	}

	private void OnTrackingStateCanceled(InputAction.CallbackContext context)
	{
		m_CurrentTrackingState = TrackingStates.None;
	}

	protected void Reset()
	{
		m_PositionInput = new InputActionProperty(new InputAction("Position", InputActionType.Value, null, null, null, "Vector3"));
		m_RotationInput = new InputActionProperty(new InputAction("Rotation", InputActionType.Value, null, null, null, "Quaternion"));
		m_TrackingStateInput = new InputActionProperty(new InputAction("Tracking State", InputActionType.Value, null, null, null, "Integer"));
	}

	protected virtual void Awake()
	{
		if (HasStereoCamera(out var cameraComponent))
		{
			XRDevice.DisableAutoXRCameraTracking(cameraComponent, disabled: true);
		}
	}

	protected void OnEnable()
	{
		InputSystem.onAfterUpdate += UpdateCallback;
		InputSystem.onDeviceChange += OnDeviceChanged;
		BindActions();
		m_IsFirstUpdate = true;
	}

	protected void OnDisable()
	{
		UnbindActions();
		InputSystem.onAfterUpdate -= UpdateCallback;
		InputSystem.onDeviceChange -= OnDeviceChanged;
	}

	protected virtual void OnDestroy()
	{
		if (HasStereoCamera(out var cameraComponent))
		{
			XRDevice.DisableAutoXRCameraTracking(cameraComponent, disabled: false);
		}
	}

	protected void UpdateCallback()
	{
		if (m_IsFirstUpdate)
		{
			if (HasResolvedControl(m_PositionInput.action))
			{
				m_CurrentPosition = m_PositionInput.action.ReadValue<Vector3>();
			}
			else
			{
				m_CurrentPosition = base.transform.localPosition;
			}
			if (HasResolvedControl(m_RotationInput.action))
			{
				m_CurrentRotation = m_RotationInput.action.ReadValue<Quaternion>();
			}
			else
			{
				m_CurrentRotation = base.transform.localRotation;
			}
			ReadTrackingState();
			m_IsFirstUpdate = false;
		}
		if (InputState.currentUpdateType == InputUpdateType.BeforeRender)
		{
			OnBeforeRender();
		}
		else
		{
			OnUpdate();
		}
	}

	private void OnDeviceChanged(InputDevice inputDevice, InputDeviceChange inputDeviceChange)
	{
		if (!m_IsFirstUpdate)
		{
			ReadTrackingStateWithoutTrackingAction();
		}
	}

	private void ReadTrackingStateWithoutTrackingAction()
	{
		InputAction action = m_TrackingStateInput.action;
		if (action == null || action.m_BindingsCount == 0)
		{
			bool flag = HasResolvedControl(m_PositionInput.action);
			bool flag2 = HasResolvedControl(m_RotationInput.action);
			if (flag && flag2)
			{
				m_CurrentTrackingState = TrackingStates.Position | TrackingStates.Rotation;
			}
			else if (flag)
			{
				m_CurrentTrackingState = TrackingStates.Position;
			}
			else if (flag2)
			{
				m_CurrentTrackingState = TrackingStates.Rotation;
			}
			else
			{
				m_CurrentTrackingState = TrackingStates.None;
			}
		}
	}

	private void ReadTrackingState()
	{
		InputAction action = m_TrackingStateInput.action;
		if (action != null && !action.enabled)
		{
			m_CurrentTrackingState = TrackingStates.None;
		}
		else if (HasResolvedControl(action))
		{
			m_CurrentTrackingState = (TrackingStates)action.ReadValue<int>();
		}
		else
		{
			ReadTrackingStateWithoutTrackingAction();
		}
	}

	protected virtual void OnUpdate()
	{
		if (m_UpdateType == UpdateType.Update || m_UpdateType == UpdateType.UpdateAndBeforeRender)
		{
			PerformUpdate();
		}
	}

	protected virtual void OnBeforeRender()
	{
		if (m_UpdateType == UpdateType.BeforeRender || m_UpdateType == UpdateType.UpdateAndBeforeRender)
		{
			PerformUpdate();
		}
	}

	protected virtual void PerformUpdate()
	{
		SetLocalTransform(m_CurrentPosition, m_CurrentRotation);
	}

	protected virtual void SetLocalTransform(Vector3 newPosition, Quaternion newRotation)
	{
		bool flag = m_IgnoreTrackingState || (m_CurrentTrackingState & TrackingStates.Position) != 0;
		bool flag2 = m_IgnoreTrackingState || (m_CurrentTrackingState & TrackingStates.Rotation) != 0;
		if (m_TrackingType == TrackingType.RotationAndPosition && flag2 && flag)
		{
			base.transform.SetLocalPositionAndRotation(newPosition, newRotation);
			return;
		}
		if (flag2 && (m_TrackingType == TrackingType.RotationAndPosition || m_TrackingType == TrackingType.RotationOnly))
		{
			base.transform.localRotation = newRotation;
		}
		if (flag && (m_TrackingType == TrackingType.RotationAndPosition || m_TrackingType == TrackingType.PositionOnly))
		{
			base.transform.localPosition = newPosition;
		}
	}

	private bool HasStereoCamera(out Camera cameraComponent)
	{
		if (TryGetComponent<Camera>(out cameraComponent))
		{
			return cameraComponent.stereoEnabled;
		}
		return false;
	}

	private unsafe static bool HasResolvedControl(InputAction action)
	{
		if (action == null)
		{
			return false;
		}
		InputActionMap orCreateActionMap = action.GetOrCreateActionMap();
		orCreateActionMap.ResolveBindingsIfNecessary();
		InputActionState state = orCreateActionMap.m_State;
		if (state == null)
		{
			return false;
		}
		int actionIndexInState = action.m_ActionIndexInState;
		int totalBindingCount = state.totalBindingCount;
		for (int i = 0; i < totalBindingCount; i++)
		{
			ref InputActionState.BindingState reference = ref state.bindingStates[i];
			if (reference.actionIndex == actionIndexInState && !reference.isComposite && reference.controlCount > 0)
			{
				return true;
			}
		}
		return false;
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if ((object)m_PositionInput.serializedReference == null && m_PositionInput.serializedAction == null && m_PositionAction != null)
		{
			m_PositionInput = new InputActionProperty(m_PositionAction);
		}
		if ((object)m_RotationInput.serializedReference == null && m_RotationInput.serializedAction == null && m_RotationAction != null)
		{
			m_RotationInput = new InputActionProperty(m_RotationAction);
		}
	}
}
