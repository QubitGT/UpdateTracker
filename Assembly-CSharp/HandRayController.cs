using System.Text;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using Valve.VR;

[AddComponentMenu("GorillaTag/KID/Hand Ray Controller")]
public class HandRayController : MonoBehaviour
{
	private enum HandSide
	{
		Left,
		Right
	}

	[OnEnterPlay_SetNull]
	private static HandRayController instance;

	[SerializeField]
	private XRRayInteractor _leftHandRay;

	[SerializeField]
	private XRRayInteractor _rightHandRay;

	private bool _hasInitialised;

	private HandSide ActiveHand = HandSide.Right;

	private XRRayInteractor _activeHandRay;

	private int _activationCounter;

	private Transform _steamLeftRayOrigin;

	private Transform _steamRightRayOrigin;

	public static HandRayController Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Object.FindAnyObjectByType<HandRayController>();
				if (instance == null)
				{
					Debug.LogErrorFormat("[KID::UI::HAND_RAY_CONTROLLER] Not found in scene");
				}
			}
			return instance;
		}
	}

	private void Awake()
	{
		if (instance != null && instance != this)
		{
			Debug.LogErrorFormat(base.gameObject, "[KID::UI::HAND_RAY_CONTROLLER] Duplicate instance of HandRayController");
			Object.DestroyImmediate(this);
		}
		else
		{
			instance = this;
		}
	}

	private void Start()
	{
		XRRayInteractor leftHandRay = _leftHandRay;
		Transform attachTransform = (_leftHandRay.rayOriginTransform = GetRayOrigin(isLeftHand: true));
		leftHandRay.attachTransform = attachTransform;
		XRRayInteractor rightHandRay = _rightHandRay;
		attachTransform = (_rightHandRay.rayOriginTransform = GetRayOrigin(isLeftHand: false));
		rightHandRay.attachTransform = attachTransform;
		if (_activationCounter <= 0)
		{
			_activationCounter = 0;
			HideHands();
		}
	}

	private void OnDisable()
	{
		DisableHandRays();
	}

	public void EnableHandRays()
	{
		if (_activationCounter == 0)
		{
			ApplySteamTipPose(isLeftHand: true);
			ApplySteamTipPose(isLeftHand: false);
			if ((bool)ControllerBehaviour.Instance)
			{
				ControllerBehaviour.Instance.OnAction += PostUpdate;
			}
			ToggleHands();
		}
		_activationCounter++;
	}

	public void DisableHandRays()
	{
		if (_activationCounter <= 0)
		{
			_activationCounter = 0;
			return;
		}
		_activationCounter--;
		if (_activationCounter == 0)
		{
			if ((bool)ControllerBehaviour.Instance)
			{
				ControllerBehaviour.Instance.OnAction -= PostUpdate;
			}
			HideHands();
		}
	}

	public void PulseActiveHandray(float vibrationStrength, float vibrationDuration)
	{
		if (!(_activeHandRay == null))
		{
			_activeHandRay.SendHapticImpulse(vibrationStrength, vibrationDuration);
		}
	}

	private void PostUpdate()
	{
		if (!_hasInitialised || !ControllerBehaviour.Instance)
		{
			return;
		}
		if (ActiveHand == HandSide.Left)
		{
			if (ControllerBehaviour.Instance.RightButtonDown)
			{
				ToggleHands();
			}
		}
		else if (ControllerBehaviour.Instance.LeftButtonDown)
		{
			ToggleHands();
		}
	}

	private void ToggleRightHandRay(bool enabled)
	{
		Debug.LogFormat($"[KID::UI::HAND_RAY_CONTROLLER] RIGHT Hand is: {_rightHandRay.gameObject.activeInHierarchy}. Setting to: {enabled}");
		_rightHandRay.gameObject.SetActive(enabled);
		if (enabled)
		{
			_activeHandRay = _rightHandRay;
		}
	}

	private void ToggleLeftHandRay(bool enabled)
	{
		Debug.LogFormat($"[KID::UI::HAND_RAY_CONTROLLER] LEFT Hand is: {_leftHandRay.gameObject.activeInHierarchy}. Setting to: {enabled}");
		_leftHandRay.gameObject.SetActive(enabled);
		if (enabled)
		{
			_activeHandRay = _leftHandRay;
		}
	}

	private void InitialiseHands()
	{
		Debug.Log("[KID::UI::HAND_RAY_CONTROLLER] Initialising Hands");
		ToggleRightHandRay(ActiveHand == HandSide.Right);
		ToggleLeftHandRay(ActiveHand == HandSide.Left);
		_hasInitialised = true;
	}

	private void ToggleHands()
	{
		if (!_hasInitialised)
		{
			InitialiseHands();
			return;
		}
		HandSide handSide = ((ActiveHand == HandSide.Left) ? HandSide.Right : HandSide.Left);
		Debug.LogFormat("[KID::UI::HAND_RAY_CONTROLLER] Setting ActiveHand FROM: [" + ActiveHand.ToString() + "] TO: [" + handSide.ToString() + "]");
		ActiveHand = handSide;
		ToggleRightHandRay(handSide == HandSide.Right);
		ToggleLeftHandRay(handSide == HandSide.Left);
	}

	private void HideHands()
	{
		ToggleRightHandRay(enabled: false);
		ToggleLeftHandRay(enabled: false);
		_hasInitialised = false;
		_activeHandRay = null;
	}

	private Transform GetRayOrigin(bool isLeftHand)
	{
		Transform transform = ((GTPlayer.Instance != null) ? GTPlayer.Instance.GetControllerTransform(isLeftHand) : null);
		if (transform != null)
		{
			Transform transform2 = new GameObject(isLeftHand ? "KID_SteamRayOrigin_Left" : "KID_SteamRayOrigin_Right").transform;
			transform2.SetParent(transform, worldPositionStays: false);
			if (isLeftHand)
			{
				_steamLeftRayOrigin = transform2;
			}
			else
			{
				_steamRightRayOrigin = transform2;
			}
			ApplySteamTipPose(isLeftHand);
			return transform2;
		}
		Debug.LogWarning("[KID::UI::HAND_RAY_CONTROLLER] GTPlayer controller transform unavailable - falling back to gorilla hand for ray origin");
		return (isLeftHand ? KIDHandReference.LeftHand : KIDHandReference.RightHand).transform;
	}

	private void ApplySteamTipPose(bool isLeftHand)
	{
		Transform transform = (isLeftHand ? _steamLeftRayOrigin : _steamRightRayOrigin);
		if (!(transform == null))
		{
			if (TryGetSteamTipPose(isLeftHand, out var position, out var rotation))
			{
				transform.localPosition = position;
				transform.localRotation = rotation;
			}
			else
			{
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;
			}
		}
	}

	private static bool TryGetSteamTipPose(bool isLeftHand, out Vector3 position, out Quaternion rotation)
	{
		position = Vector3.zero;
		rotation = Quaternion.identity;
		CVRSystem system = OpenVR.System;
		CVRRenderModels renderModels = OpenVR.RenderModels;
		CVRInput input = OpenVR.Input;
		if (system == null || renderModels == null || input == null)
		{
			return false;
		}
		uint trackedDeviceIndexForControllerRole = system.GetTrackedDeviceIndexForControllerRole(isLeftHand ? ETrackedControllerRole.LeftHand : ETrackedControllerRole.RightHand);
		if (trackedDeviceIndexForControllerRole == uint.MaxValue)
		{
			return false;
		}
		ETrackedPropertyError pError = ETrackedPropertyError.TrackedProp_Success;
		uint stringTrackedDeviceProperty = system.GetStringTrackedDeviceProperty(trackedDeviceIndexForControllerRole, ETrackedDeviceProperty.Prop_RenderModelName_String, null, 0u, ref pError);
		if (stringTrackedDeviceProperty <= 1)
		{
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
		system.GetStringTrackedDeviceProperty(trackedDeviceIndexForControllerRole, ETrackedDeviceProperty.Prop_RenderModelName_String, stringBuilder, stringTrackedDeviceProperty, ref pError);
		if (pError != 0)
		{
			return false;
		}
		ulong pHandle = 0uL;
		if (input.GetInputSourceHandle(isLeftHand ? "/user/hand/left" : "/user/hand/right", ref pHandle) != 0)
		{
			return false;
		}
		RenderModel_ControllerMode_State_t pState = default(RenderModel_ControllerMode_State_t);
		RenderModel_ComponentState_t pComponentState = default(RenderModel_ComponentState_t);
		if (!renderModels.GetComponentStateForDevicePath(stringBuilder.ToString(), "tip", pHandle, ref pState, ref pComponentState))
		{
			return false;
		}
		position = pComponentState.mTrackingToComponentLocal.GetPosition();
		rotation = pComponentState.mTrackingToComponentLocal.GetRotation();
		return true;
	}
}
