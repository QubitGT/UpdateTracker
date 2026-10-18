using UnityEngine;
using UnityEngine.InputSystem;

public class OVRHeadsetEmulator : MonoBehaviour
{
	public enum OpMode
	{
		Off,
		EditorOnly,
		AlwaysOn
	}

	public OpMode opMode = OpMode.EditorOnly;

	public bool resetHmdPoseOnRelease = true;

	public bool resetHmdPoseByMiddleMouseButton = true;

	public KeyCode[] activateKeys = new KeyCode[3]
	{
		KeyCode.LeftControl,
		KeyCode.RightControl,
		KeyCode.F1
	};

	public string[] activateKeyBindings = new string[3] { "<Keyboard>/leftCtrl", "<Keyboard>/rightCtrl", "<Keyboard>/f1" };

	public KeyCode[] pitchKeys = new KeyCode[3]
	{
		KeyCode.LeftAlt,
		KeyCode.RightAlt,
		KeyCode.F2
	};

	public string[] pitchKeyBindings = new string[3] { "<Keyboard>/leftAlt", "<Keyboard>/rightAlt", "<Keyboard>/f2" };

	private InputAction[] activateKeyActions;

	private InputAction[] pitchKeyActions;

	private InputAction middleMouseButtonAction;

	private InputAction mouseScrollAction;

	private InputAction mouseMoveAction;

	private OVRManager manager;

	private const float MOUSE_SCALE_X = -2f;

	private const float MOUSE_SCALE_X_PITCH = -2f;

	private const float MOUSE_SCALE_Y = 2f;

	private const float MOUSE_SCALE_HEIGHT = 1f;

	private const float MAX_ROLL = 85f;

	private bool lastFrameEmulationActivated;

	private Vector3 recordedHeadPoseRelativeOffsetTranslation;

	private Vector3 recordedHeadPoseRelativeOffsetRotation;

	private bool hasSentEvent;

	private bool emulatorHasInitialized;

	private CursorLockMode previousCursorLockMode;

	private void Start()
	{
		activateKeyActions = new InputAction[activateKeyBindings.Length];
		for (int i = 0; i < activateKeyBindings.Length; i++)
		{
			activateKeyActions[i] = new InputAction(null, InputActionType.Value, activateKeyBindings[i]);
			activateKeyActions[i].Enable();
		}
		pitchKeyActions = new InputAction[pitchKeyBindings.Length];
		for (int j = 0; j < pitchKeyBindings.Length; j++)
		{
			pitchKeyActions[j] = new InputAction(null, InputActionType.Value, pitchKeyBindings[j]);
			pitchKeyActions[j].Enable();
		}
		middleMouseButtonAction = new InputAction(null, InputActionType.Button, "<Mouse>/middleButton");
		mouseScrollAction = new InputAction(null, InputActionType.Value, "<Mouse>/scroll/y");
		mouseMoveAction = new InputAction(null, InputActionType.Value, "<Mouse>/delta");
		middleMouseButtonAction.Enable();
		mouseScrollAction.Enable();
		mouseMoveAction.Enable();
	}

	private void Update()
	{
		if (!emulatorHasInitialized)
		{
			if (!OVRManager.OVRManagerinitialized)
			{
				return;
			}
			previousCursorLockMode = Cursor.lockState;
			manager = OVRManager.instance;
			recordedHeadPoseRelativeOffsetTranslation = manager.headPoseRelativeOffsetTranslation;
			recordedHeadPoseRelativeOffsetRotation = manager.headPoseRelativeOffsetRotation;
			emulatorHasInitialized = true;
			lastFrameEmulationActivated = false;
		}
		bool flag = IsEmulationActivated();
		if (flag)
		{
			if (!lastFrameEmulationActivated)
			{
				previousCursorLockMode = Cursor.lockState;
				Cursor.lockState = CursorLockMode.Locked;
			}
			if (!lastFrameEmulationActivated && resetHmdPoseOnRelease)
			{
				manager.headPoseRelativeOffsetTranslation = recordedHeadPoseRelativeOffsetTranslation;
				manager.headPoseRelativeOffsetRotation = recordedHeadPoseRelativeOffsetRotation;
			}
			bool flag2 = false;
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			flag2 = middleMouseButtonAction.phase == InputActionPhase.Performed;
			num = mouseScrollAction.ReadValue<float>();
			Vector2 vector = mouseMoveAction.ReadValue<Vector2>();
			num2 = vector.x * 0.05f;
			num3 = vector.y * 0.05f;
			if (resetHmdPoseByMiddleMouseButton && flag2)
			{
				manager.headPoseRelativeOffsetTranslation = Vector3.zero;
				manager.headPoseRelativeOffsetRotation = Vector3.zero;
			}
			else
			{
				Vector3 headPoseRelativeOffsetTranslation = manager.headPoseRelativeOffsetTranslation;
				float num4 = num * 1f;
				headPoseRelativeOffsetTranslation.y += num4;
				manager.headPoseRelativeOffsetTranslation = headPoseRelativeOffsetTranslation;
				Vector3 headPoseRelativeOffsetRotation = manager.headPoseRelativeOffsetRotation;
				float num5 = headPoseRelativeOffsetRotation.x;
				float num6 = headPoseRelativeOffsetRotation.y;
				float num7 = headPoseRelativeOffsetRotation.z;
				if (IsTweakingPitch())
				{
					num7 += num2 * -2f;
				}
				else
				{
					num5 += num3 * 2f;
					num6 += num2 * -2f;
				}
				manager.headPoseRelativeOffsetRotation = new Vector3(num5, num6, num7);
			}
			if (!hasSentEvent)
			{
				OVRPlugin.SendEvent("headset_emulator", "activated");
				hasSentEvent = true;
			}
		}
		else if (lastFrameEmulationActivated)
		{
			Cursor.lockState = previousCursorLockMode;
			recordedHeadPoseRelativeOffsetTranslation = manager.headPoseRelativeOffsetTranslation;
			recordedHeadPoseRelativeOffsetRotation = manager.headPoseRelativeOffsetRotation;
			if (resetHmdPoseOnRelease)
			{
				manager.headPoseRelativeOffsetTranslation = Vector3.zero;
				manager.headPoseRelativeOffsetRotation = Vector3.zero;
			}
		}
		lastFrameEmulationActivated = flag;
	}

	private bool IsEmulationActivated()
	{
		if (opMode == OpMode.Off)
		{
			return false;
		}
		if (opMode == OpMode.EditorOnly && !Application.isEditor)
		{
			return false;
		}
		InputAction[] array = activateKeyActions;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].phase == InputActionPhase.Started)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsTweakingPitch()
	{
		if (!IsEmulationActivated())
		{
			return false;
		}
		InputAction[] array = pitchKeyActions;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].phase == InputActionPhase.Started)
			{
				return true;
			}
		}
		return false;
	}

	private void OnDestroy()
	{
		if (activateKeyActions != null)
		{
			InputAction[] array = activateKeyActions;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Disable();
			}
		}
		if (pitchKeyActions != null)
		{
			InputAction[] array = pitchKeyActions;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Disable();
			}
		}
		middleMouseButtonAction?.Disable();
		mouseScrollAction?.Disable();
		mouseMoveAction?.Disable();
	}
}
