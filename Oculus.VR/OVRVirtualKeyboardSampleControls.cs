using System.Collections;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(OVRVirtualKeyboardSampleInputHandler))]
[HelpURL("https://developer.oculus.com/documentation/unity/VK-unity-sample/")]
[Feature(Feature.VirtualKeyboard)]
public class OVRVirtualKeyboardSampleControls : MonoBehaviour
{
	private struct OVRVirtualKeyboardBackup
	{
		private readonly Vector3 _position;

		private readonly Quaternion _rotation;

		private readonly Vector3 _scale;

		private readonly Transform _rightControllerDirectTransform;

		private readonly Transform _rightControllerRootTransform;

		private readonly Transform _leftControllerDirectTransform;

		private readonly Transform _leftControllerRootTransform;

		private readonly bool _controllerRayInteraction;

		private readonly bool _controllerDirectInteraction;

		private readonly OVRHand _handLeft;

		private readonly OVRHand _handRight;

		private readonly bool _handRayInteraction;

		private readonly bool _handDirectInteraction;

		private readonly OVRPhysicsRaycaster _controllerRaycaster;

		private readonly OVRPhysicsRaycaster _handRaycaster;

		private readonly InputField _textHandlerField;

		public OVRVirtualKeyboardBackup(OVRVirtualKeyboard keyboard)
		{
			_position = keyboard.transform.position;
			_rotation = keyboard.transform.rotation;
			_scale = keyboard.transform.localScale;
			_rightControllerDirectTransform = keyboard.rightControllerDirectTransform;
			_rightControllerRootTransform = keyboard.rightControllerRootTransform;
			_leftControllerDirectTransform = keyboard.leftControllerDirectTransform;
			_leftControllerRootTransform = keyboard.leftControllerRootTransform;
			_controllerRayInteraction = keyboard.controllerRayInteraction;
			_controllerDirectInteraction = keyboard.controllerDirectInteraction;
			_controllerRaycaster = keyboard.controllerRaycaster;
			_handLeft = keyboard.handLeft;
			_handRight = keyboard.handRight;
			_handRayInteraction = keyboard.handRayInteraction;
			_handDirectInteraction = keyboard.handDirectInteraction;
			_handRaycaster = keyboard.handRaycaster;
			_textHandlerField = null;
			if (keyboard.TextHandler is OVRVirtualKeyboardInputFieldTextHandler oVRVirtualKeyboardInputFieldTextHandler)
			{
				_textHandlerField = oVRVirtualKeyboardInputFieldTextHandler.InputField;
			}
		}

		public void RestoreTo(OVRVirtualKeyboard keyboard)
		{
			keyboard.transform.SetPositionAndRotation(_position, _rotation);
			keyboard.transform.localScale = _scale;
			keyboard.rightControllerDirectTransform = _rightControllerDirectTransform;
			keyboard.rightControllerRootTransform = _rightControllerRootTransform;
			keyboard.leftControllerDirectTransform = _leftControllerDirectTransform;
			keyboard.leftControllerRootTransform = _leftControllerRootTransform;
			keyboard.controllerRayInteraction = _controllerRayInteraction;
			keyboard.controllerDirectInteraction = _controllerDirectInteraction;
			keyboard.controllerRaycaster = _controllerRaycaster;
			keyboard.handLeft = _handLeft;
			keyboard.handRight = _handRight;
			keyboard.handRayInteraction = _handRayInteraction;
			keyboard.handDirectInteraction = _handDirectInteraction;
			keyboard.handRaycaster = _handRaycaster;
			if (keyboard.TextHandler == null)
			{
				keyboard.TextHandler = keyboard.gameObject.AddComponent<OVRVirtualKeyboardInputFieldTextHandler>();
			}
			OVRVirtualKeyboardInputFieldTextHandler oVRVirtualKeyboardInputFieldTextHandler = keyboard.TextHandler as OVRVirtualKeyboardInputFieldTextHandler;
			if ((bool)oVRVirtualKeyboardInputFieldTextHandler)
			{
				oVRVirtualKeyboardInputFieldTextHandler.InputField = _textHandlerField;
			}
		}
	}

	private const float THUMBSTICK_DEADZONE = 0.2f;

	[SerializeField]
	private Button ShowButton;

	[SerializeField]
	private Button MoveButton;

	[SerializeField]
	private Button HideButton;

	[SerializeField]
	private Button MoveNearButton;

	[SerializeField]
	private Button MoveFarButton;

	[SerializeField]
	private Button DestroyKeyboardButton;

	[SerializeField]
	private OVRVirtualKeyboard keyboard;

	[SerializeField]
	private OVRVirtualKeyboard keyboardPrefab;

	private OVRVirtualKeyboardSampleInputHandler inputHandler;

	private bool isMovingKeyboard_;

	private bool isMovingKeyboardFinished_;

	private float keyboardMoveDistance_;

	private float keyboardScale_ = 1f;

	private OVRVirtualKeyboardBackup keyboardBackup;

	private void Start()
	{
		inputHandler = GetComponent<OVRVirtualKeyboardSampleInputHandler>();
		keyboard.KeyboardHidden.AddListener(OnHideKeyboard);
		if ((bool)MoveNearButton)
		{
			MoveNearButton.onClick.AddListener(MoveKeyboardNear);
		}
		if ((bool)MoveFarButton)
		{
			MoveFarButton.onClick.AddListener(MoveKeyboardFar);
		}
		if ((bool)DestroyKeyboardButton)
		{
			DestroyKeyboardButton.onClick.AddListener(DestroyKeyboard);
		}
		StartCoroutine(CreateKeyboard());
	}

	private void OnDestroy()
	{
		if (!(keyboard == null))
		{
			keyboard.KeyboardHidden.RemoveListener(OnHideKeyboard);
			if ((bool)MoveNearButton)
			{
				MoveNearButton.onClick.RemoveListener(MoveKeyboardNear);
			}
			if ((bool)MoveFarButton)
			{
				MoveFarButton.onClick.RemoveListener(MoveKeyboardFar);
			}
			if ((bool)DestroyKeyboardButton)
			{
				DestroyKeyboardButton.onClick.RemoveListener(DestroyKeyboard);
			}
		}
	}

	public void ShowKeyboard()
	{
		if (keyboard == null)
		{
			StartCoroutine(CreateKeyboard());
			return;
		}
		keyboard.gameObject.SetActive(value: true);
		UpdateButtonInteractable();
	}

	private IEnumerator CreateKeyboard()
	{
		Text showButtonText = null;
		if ((bool)ShowButton && (bool)HideButton && (bool)DestroyKeyboardButton)
		{
			showButtonText = ShowButton.GetComponentInChildren<Text>();
			showButtonText.text = "Creating Keyboard...";
			ShowButton.interactable = false;
			HideButton.interactable = false;
			DestroyKeyboardButton.interactable = false;
		}
		if (keyboard == null)
		{
			if ((bool)keyboardPrefab)
			{
				keyboard = Object.Instantiate(keyboardPrefab);
			}
			if (!keyboard)
			{
				GameObject gameObject = new GameObject();
				keyboard = gameObject.AddComponent<OVRVirtualKeyboard>();
			}
			keyboardBackup.RestoreTo(keyboard);
			inputHandler.OVRVirtualKeyboard = keyboard;
		}
		yield return new OVRVirtualKeyboard.WaitUntilKeyboardVisible(keyboard);
		if ((bool)showButtonText)
		{
			showButtonText.text = "Show Keyboard";
		}
		UpdateButtonInteractable();
	}

	public void MoveKeyboard()
	{
		if (keyboard.gameObject.activeSelf)
		{
			isMovingKeyboard_ = true;
			Transform transform = keyboard.transform;
			keyboardMoveDistance_ = (inputHandler.InputRayPosition - transform.position).magnitude;
			keyboardScale_ = transform.localScale.x;
			UpdateButtonInteractable();
			keyboard.InputEnabled = false;
		}
	}

	public void MoveKeyboardNear()
	{
		if (keyboard.gameObject.activeSelf)
		{
			keyboard.UseSuggestedLocation(OVRVirtualKeyboard.KeyboardPosition.Near);
		}
	}

	public void MoveKeyboardFar()
	{
		if (keyboard.gameObject.activeSelf)
		{
			keyboard.UseSuggestedLocation(OVRVirtualKeyboard.KeyboardPosition.Far);
		}
	}

	public void HideKeyboard()
	{
		keyboard.gameObject.SetActive(value: false);
		isMovingKeyboard_ = false;
		UpdateButtonInteractable();
	}

	public void DestroyKeyboard()
	{
		if (keyboard != null)
		{
			keyboardBackup = new OVRVirtualKeyboardBackup(keyboard);
			Object.Destroy(keyboard.gameObject);
			keyboard = null;
			UpdateButtonInteractable();
		}
	}

	private void OnHideKeyboard()
	{
		UpdateButtonInteractable();
	}

	private void UpdateButtonInteractable()
	{
		bool flag = keyboard != null;
		bool interactable = flag && keyboard.gameObject.activeSelf && !isMovingKeyboard_;
		if ((bool)ShowButton)
		{
			ShowButton.interactable = !flag || !keyboard.gameObject.activeSelf;
		}
		if ((bool)MoveButton)
		{
			MoveButton.interactable = interactable;
		}
		if ((bool)MoveNearButton)
		{
			MoveNearButton.interactable = interactable;
		}
		if ((bool)MoveFarButton)
		{
			MoveFarButton.interactable = interactable;
		}
		if ((bool)HideButton)
		{
			HideButton.interactable = interactable;
		}
		if ((bool)DestroyKeyboardButton)
		{
			DestroyKeyboardButton.interactable = flag;
		}
	}

	private void Update()
	{
		bool flag = OVRInput.Get(OVRInput.Button.One | OVRInput.Button.Three | OVRInput.Button.PrimaryIndexTrigger | OVRInput.Button.SecondaryIndexTrigger, OVRInput.Controller.All);
		if (isMovingKeyboardFinished_ && !flag)
		{
			keyboard.InputEnabled = true;
			isMovingKeyboard_ = false;
			isMovingKeyboardFinished_ = false;
			UpdateButtonInteractable();
		}
		if (isMovingKeyboard_ && !isMovingKeyboardFinished_)
		{
			keyboardMoveDistance_ *= 1f + inputHandler.AnalogStickY * 0.01f;
			keyboardMoveDistance_ = Mathf.Clamp(keyboardMoveDistance_, 0.1f, 100f);
			keyboardScale_ += inputHandler.AnalogStickX * 0.01f;
			keyboardScale_ = Mathf.Clamp(keyboardScale_, 0.25f, 2f);
			Quaternion inputRayRotation = inputHandler.InputRayRotation;
			Transform obj = keyboard.transform;
			obj.SetPositionAndRotation(inputHandler.InputRayPosition + keyboardMoveDistance_ * (inputRayRotation * Vector3.forward), inputRayRotation);
			obj.localScale = Vector3.one * keyboardScale_;
			if (flag)
			{
				isMovingKeyboardFinished_ = true;
			}
		}
	}
}
