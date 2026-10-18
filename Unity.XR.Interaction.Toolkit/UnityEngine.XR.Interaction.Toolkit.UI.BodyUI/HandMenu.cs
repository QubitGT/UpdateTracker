using System;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.Primitives;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Tweenables.SmartTweenableVariables;

namespace UnityEngine.XR.Interaction.Toolkit.UI.BodyUI;

[AddComponentMenu("XR/Hand Menu", 22)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.UI.BodyUI.HandMenu.html")]
public class HandMenu : MonoBehaviour
{
	public enum UpDirection
	{
		WorldUp,
		TransformUp,
		CameraUp
	}

	public enum MenuHandedness
	{
		None,
		Left,
		Right,
		Either
	}

	[SerializeField]
	[Tooltip("Child GameObject used to hold the hand menu UI. This is the transform that moves each frame.")]
	private GameObject m_HandMenuUIGameObject;

	[Header("Hand alignment")]
	[SerializeField]
	[Tooltip("Which hand should the menu anchor to. None will disable the hand menu. Either will try to follow the first hand to meet requirements.")]
	private MenuHandedness m_MenuHandedness = MenuHandedness.Either;

	[SerializeField]
	[Tooltip("Determines the up direction of the menu when the hand menu is looking at the camera.")]
	private UpDirection m_HandMenuUpDirection = UpDirection.TransformUp;

	[Header("Palm anchor")]
	[SerializeField]
	[Tooltip("Anchor associated with the left palm pose for the hand.")]
	private Transform m_LeftPalmAnchor;

	[SerializeField]
	[Tooltip("Anchor associated with the right palm pose for the hand.")]
	private Transform m_RightPalmAnchor;

	[Header("Position follow config.")]
	[SerializeField]
	[Tooltip("Minimum distance in meters from target before which tween starts.")]
	private float m_MinFollowDistance = 0.005f;

	[SerializeField]
	[Tooltip("Maximum distance in meters from target before tween targets, when time threshold is reached.")]
	private float m_MaxFollowDistance = 0.03f;

	[SerializeField]
	[Tooltip("Time required to elapse before the max distance allowed goes from the min distance to the max.")]
	private float m_MinToMaxDelaySeconds = 1f;

	[Header("Gaze Alignment Config")]
	[SerializeField]
	[Tooltip("If true, menu will hide when gaze to menu origin's divergence angle is above the threshold. In other words, the menu will only show if looking roughly in it's direction.")]
	private bool m_HideMenuWhenGazeDiverges = true;

	[SerializeField]
	[Tooltip("Only show menu if gaze to menu origin's divergence angle is below this value.")]
	private float m_MenuVisibleGazeAngleDivergenceThreshold = 35f;

	private float m_MenuVisibilityDotThreshold;

	private readonly SmartFollowVector3TweenableVariable m_HandAnchorSmartFollow = new SmartFollowVector3TweenableVariable();

	private readonly QuaternionTweenableVariable m_RotTweenFollow = new QuaternionTweenableVariable();

	private readonly Vector3TweenableVariable m_MenuScaleTweenable = new Vector3TweenableVariable();

	private readonly BindingsGroup m_BindingsGroup = new BindingsGroup();

	private Transform m_CameraTransform;

	private bool m_WasMenuHiddenLastFrame = true;

	private MenuHandedness m_LastHandThatMetRequirements = MenuHandedness.Left;

	[Header("Animation Settings")]
	[SerializeField]
	[Tooltip("Should the menu animate when it is revealed or hidden.")]
	private bool m_AnimateMenuHideAndReveal = true;

	[SerializeField]
	[Tooltip("Duration of the reveal/hide animation in seconds.")]
	private float m_RevealHideAnimationDuration = 0.15f;

	[Header("Selection Behavior")]
	[SerializeField]
	[Tooltip("Should the menu hide when a selection is made with the hand for which the menu is anchored to.")]
	private bool m_HideMenuOnSelect = true;

	[SerializeField]
	[Tooltip("XR Interaction Manager used to determine if a hand is selecting. Will find one if None. Used for Hide Menu On Select.")]
	private XRInteractionManager m_InteractionManager;

	[Header("Follow presets")]
	[SerializeField]
	private FollowPresetDatumProperty m_HandTrackingFollowPreset;

	[SerializeField]
	private FollowPresetDatumProperty m_ControllerFollowPreset;

	private XRInputModalityManager.InputMode m_CurrentInputMode;

	private Transform m_LeftOffsetRoot;

	private Transform m_RightOffsetRoot;

	private Coroutine m_HideCoroutine;

	private Coroutine m_ShowCoroutine;

	private Transform m_LastValidCameraTransform;

	private Transform m_LastValidPalmAnchor;

	private Transform m_LastValidPalmAnchorOffset;

	private Vector3 m_InitialMenuLocalScale = Vector3.one;

	private readonly BindableVariable<bool> m_MenuVisibleBindableVariable = new BindableVariable<bool>(initialValue: false);

	private float m_LastValidTrackingTime;

	public GameObject handMenuUIGameObject
	{
		get
		{
			return m_HandMenuUIGameObject;
		}
		set
		{
			m_HandMenuUIGameObject = value;
		}
	}

	public MenuHandedness menuHandedness
	{
		get
		{
			return m_MenuHandedness;
		}
		set
		{
			m_MenuHandedness = value;
		}
	}

	public UpDirection handMenuUpDirection
	{
		get
		{
			return m_HandMenuUpDirection;
		}
		set
		{
			m_HandMenuUpDirection = value;
		}
	}

	public Transform leftPalmAnchor
	{
		get
		{
			return m_LeftPalmAnchor;
		}
		set
		{
			m_LeftPalmAnchor = value;
		}
	}

	public Transform rightPalmAnchor
	{
		get
		{
			return m_RightPalmAnchor;
		}
		set
		{
			m_RightPalmAnchor = value;
		}
	}

	public float minFollowDistance
	{
		get
		{
			return m_MinFollowDistance;
		}
		set
		{
			m_MinFollowDistance = value;
			m_HandAnchorSmartFollow.minDistanceAllowed = value;
		}
	}

	public float maxFollowDistance
	{
		get
		{
			return m_MaxFollowDistance;
		}
		set
		{
			m_MaxFollowDistance = value;
			m_HandAnchorSmartFollow.maxDistanceAllowed = value;
		}
	}

	public float minToMaxDelaySeconds
	{
		get
		{
			return m_MinToMaxDelaySeconds;
		}
		set
		{
			m_MinToMaxDelaySeconds = value;
			m_HandAnchorSmartFollow.minToMaxDelaySeconds = value;
		}
	}

	public bool hideMenuWhenGazeDiverges
	{
		get
		{
			return m_HideMenuWhenGazeDiverges;
		}
		set
		{
			m_HideMenuWhenGazeDiverges = value;
		}
	}

	public float menuVisibleGazeDivergenceThreshold
	{
		get
		{
			return m_MenuVisibleGazeAngleDivergenceThreshold;
		}
		set
		{
			m_MenuVisibleGazeAngleDivergenceThreshold = value;
			m_MenuVisibilityDotThreshold = AngleToDot(value);
		}
	}

	public bool animateMenuHideAndRevel
	{
		get
		{
			return m_AnimateMenuHideAndReveal;
		}
		set
		{
			m_AnimateMenuHideAndReveal = value;
		}
	}

	public float revealHideAnimationDuration
	{
		get
		{
			return m_RevealHideAnimationDuration;
		}
		set
		{
			m_RevealHideAnimationDuration = value;
		}
	}

	public bool hideMenuOnSelect
	{
		get
		{
			return m_HideMenuOnSelect;
		}
		set
		{
			m_HideMenuOnSelect = value;
		}
	}

	public XRInteractionManager interactionManager
	{
		get
		{
			return m_InteractionManager;
		}
		set
		{
			m_InteractionManager = value;
		}
	}

	protected void Awake()
	{
		m_HandAnchorSmartFollow.minDistanceAllowed = m_MinFollowDistance;
		m_HandAnchorSmartFollow.maxDistanceAllowed = m_MaxFollowDistance;
		m_HandAnchorSmartFollow.minToMaxDelaySeconds = m_MinToMaxDelaySeconds;
		m_RightOffsetRoot = new GameObject("Right Offset Root").transform;
		m_RightOffsetRoot.transform.SetParent(m_RightPalmAnchor);
		m_LeftOffsetRoot = new GameObject("Left Offset Root").transform;
		m_LeftOffsetRoot.transform.SetParent(m_LeftPalmAnchor);
	}

	protected void OnEnable()
	{
		if (m_LeftPalmAnchor == null || m_RightPalmAnchor == null)
		{
			Debug.LogError($"Missing palm anchor transform reference. Disabling {this} component.", this);
			base.enabled = false;
			return;
		}
		if (m_HandMenuUIGameObject == null)
		{
			Debug.LogError($"Missing Hand Menu UI GameObject reference. Disabling {this} component.", this);
			base.enabled = false;
			return;
		}
		if (m_ControllerFollowPreset == null || m_HandTrackingFollowPreset == null)
		{
			Debug.LogError($"Missing Follow Preset reference. Disabling {this} component.", this);
			base.enabled = false;
			return;
		}
		m_HandAnchorSmartFollow.Value = m_HandMenuUIGameObject.transform.position;
		m_BindingsGroup.AddBinding(m_HandAnchorSmartFollow.Subscribe(delegate(float3 newPosition)
		{
			m_HandMenuUIGameObject.transform.position = newPosition;
		}));
		m_RotTweenFollow.Value = m_HandMenuUIGameObject.transform.rotation;
		m_BindingsGroup.AddBinding(m_RotTweenFollow.Subscribe(delegate(Quaternion newRot)
		{
			m_HandMenuUIGameObject.transform.rotation = newRot;
		}));
		m_InitialMenuLocalScale = m_HandMenuUIGameObject.transform.localScale;
		m_MenuScaleTweenable.Value = m_InitialMenuLocalScale;
		m_BindingsGroup.AddBinding(m_MenuScaleTweenable.Subscribe(delegate(float3 value)
		{
			m_HandMenuUIGameObject.transform.localScale = value;
		}));
		m_BindingsGroup.AddBinding(XRInputModalityManager.currentInputMode.SubscribeAndUpdate(OnInputModeChanged));
		m_MenuVisibleBindableVariable.Value = false;
		m_BindingsGroup.AddBinding(m_MenuVisibleBindableVariable.SubscribeAndUpdate(delegate(bool value)
		{
			if (value)
			{
				ShowMenu();
			}
			else
			{
				HideMenu();
			}
		}));
	}

	protected void OnDisable()
	{
		if (m_ShowCoroutine != null)
		{
			StopCoroutine(m_ShowCoroutine);
			m_ShowCoroutine = null;
		}
		if (m_HideCoroutine != null)
		{
			StopCoroutine(m_HideCoroutine);
			m_HideCoroutine = null;
		}
		m_BindingsGroup.Clear();
		m_HandMenuUIGameObject.transform.localScale = m_InitialMenuLocalScale;
		m_HandMenuUIGameObject.SetActive(value: true);
		OnMenuVisible();
	}

	protected void OnDestroy()
	{
		m_HandAnchorSmartFollow.Dispose();
	}

	protected void OnValidate()
	{
		m_HandAnchorSmartFollow.minDistanceAllowed = m_MinFollowDistance;
		m_HandAnchorSmartFollow.maxDistanceAllowed = m_MaxFollowDistance;
		m_HandAnchorSmartFollow.minToMaxDelaySeconds = m_MinToMaxDelaySeconds;
		m_MenuVisibilityDotThreshold = AngleToDot(m_MenuVisibleGazeAngleDivergenceThreshold);
	}

	private void OnInputModeChanged(XRInputModalityManager.InputMode newInputMode)
	{
		m_CurrentInputMode = newInputMode;
		GetCurrentPreset()?.ApplyPreset(m_LeftOffsetRoot, m_RightOffsetRoot);
	}

	private FollowPreset GetCurrentPreset()
	{
		if (m_CurrentInputMode == XRInputModalityManager.InputMode.MotionController)
		{
			return m_ControllerFollowPreset.Value;
		}
		return m_HandTrackingFollowPreset.Value;
	}

	private void ShowMenu()
	{
		if (m_HideCoroutine != null)
		{
			StopCoroutine(m_HideCoroutine);
			m_HideCoroutine = null;
		}
		m_HandMenuUIGameObject.SetActive(value: true);
		if (m_AnimateMenuHideAndReveal && m_ShowCoroutine == null)
		{
			m_ShowCoroutine = StartCoroutine(m_MenuScaleTweenable.PlaySequence(m_MenuScaleTweenable.Value, m_InitialMenuLocalScale, m_RevealHideAnimationDuration, OnMenuVisible));
		}
		else
		{
			OnMenuVisible();
		}
	}

	private void OnMenuVisible()
	{
		m_ShowCoroutine = null;
		m_WasMenuHiddenLastFrame = false;
	}

	private void HideMenu()
	{
		if (m_ShowCoroutine != null)
		{
			StopCoroutine(m_ShowCoroutine);
			m_ShowCoroutine = null;
		}
		if (m_AnimateMenuHideAndReveal && m_HideCoroutine == null)
		{
			m_HideCoroutine = StartCoroutine(m_MenuScaleTweenable.PlaySequence(m_MenuScaleTweenable.Value, Vector3.zero, m_RevealHideAnimationDuration, OnMenuHidden));
		}
		else
		{
			OnMenuHidden();
		}
	}

	private void OnMenuHidden()
	{
		m_HandMenuUIGameObject.SetActive(value: false);
		m_WasMenuHiddenLastFrame = true;
		m_HideCoroutine = null;
	}

	protected void LateUpdate()
	{
		if (m_CurrentInputMode == XRInputModalityManager.InputMode.None)
		{
			m_MenuVisibleBindableVariable.Value = false;
			return;
		}
		bool flag = false;
		FollowPreset currentPreset = GetCurrentPreset();
		if (TryGetTrackedAnchors(m_MenuHandedness, in currentPreset, out var targetHandedness, out var cameraTransform, out var palmAnchor, out var palmAnchorOffset))
		{
			m_LastValidCameraTransform = cameraTransform;
			m_LastValidPalmAnchor = palmAnchor;
			m_LastValidPalmAnchorOffset = palmAnchorOffset;
			m_LastValidTrackingTime = Time.unscaledTime;
			flag = true;
		}
		if (!flag)
		{
			if (Time.unscaledTime - m_LastValidTrackingTime > currentPreset.hideDelaySeconds)
			{
				m_MenuVisibleBindableVariable.Value = false;
			}
			if (m_LastValidCameraTransform == null || m_LastValidPalmAnchor == null || m_LastValidPalmAnchorOffset == null)
			{
				return;
			}
		}
		Vector3 forward = (m_LastValidPalmAnchorOffset.position - m_LastValidCameraTransform.position).normalized;
		if (flag)
		{
			if (m_HideMenuWhenGazeDiverges)
			{
				Vector3 forward2 = m_LastValidCameraTransform.forward;
				flag = Vector3.Dot(forward, forward2) > m_MenuVisibilityDotThreshold;
			}
			m_MenuVisibleBindableVariable.Value = flag;
		}
		if (!m_HandMenuUIGameObject.activeSelf)
		{
			return;
		}
		Pose worldPose = m_LastValidPalmAnchorOffset.GetWorldPose();
		Vector3 position = worldPose.position;
		Quaternion lookRotation = worldPose.rotation;
		if (targetHandedness == MenuHandedness.Left || targetHandedness == MenuHandedness.Right)
		{
			Vector3 referenceAxisForTrackingAnchor = currentPreset.GetReferenceAxisForTrackingAnchor(m_LastValidPalmAnchor, targetHandedness == MenuHandedness.Right);
			Vector3 rhs = -forward;
			if (currentPreset.snapToGaze && Vector3.Dot(referenceAxisForTrackingAnchor, rhs) > currentPreset.snapToGazeDotThreshold)
			{
				Vector3 referenceUp = GetReferenceUpDirection(m_LastValidCameraTransform);
				BurstMathUtility.OrthogonalLookRotation(in forward, in referenceUp, out lookRotation);
			}
		}
		m_HandAnchorSmartFollow.target = position;
		m_RotTweenFollow.target = lookRotation;
		if (m_WasMenuHiddenLastFrame || !currentPreset.allowSmoothing)
		{
			m_HandAnchorSmartFollow.HandleTween(1f);
			if (currentPreset.allowSmoothing)
			{
				m_RotTweenFollow.HandleTween(Time.deltaTime * currentPreset.followLowerSmoothingValue);
			}
			else
			{
				m_RotTweenFollow.HandleTween(1f);
			}
		}
		else
		{
			m_HandAnchorSmartFollow.HandleSmartTween(Time.deltaTime, currentPreset.followLowerSmoothingValue, currentPreset.followUpperSmoothingValue);
			m_RotTweenFollow.HandleTween(Time.deltaTime * currentPreset.followLowerSmoothingValue);
		}
	}

	private bool TryGetTrackedAnchors(MenuHandedness desiredHandedness, in FollowPreset currentPreset, out MenuHandedness targetHandedness, out Transform cameraTransform, out Transform palmAnchor, out Transform palmAnchorOffset)
	{
		palmAnchor = null;
		palmAnchorOffset = null;
		targetHandedness = MenuHandedness.None;
		if (!TryGetCamera(out cameraTransform) || desiredHandedness == MenuHandedness.None)
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		if (m_HideMenuOnSelect && TryGetInteractionManager(out var manager))
		{
			flag = manager.IsHandSelecting(InteractorHandedness.Left);
			flag2 = manager.IsHandSelecting(InteractorHandedness.Right);
		}
		bool flag3 = !flag && PalmMeetsRequirements(cameraTransform, m_LeftPalmAnchor, isRightHand: false, in currentPreset);
		bool flag4 = !flag2 && PalmMeetsRequirements(cameraTransform, m_RightPalmAnchor, isRightHand: true, in currentPreset);
		if (!flag3 && !flag4)
		{
			return false;
		}
		switch (desiredHandedness)
		{
		case MenuHandedness.Either:
			if (flag3 && flag4)
			{
				MenuHandedness menuHandedness = ((m_LastHandThatMetRequirements != MenuHandedness.Right) ? MenuHandedness.Left : MenuHandedness.Right);
				GetTransformAnchorsForHandedness(menuHandedness, out palmAnchor, out palmAnchorOffset);
				targetHandedness = menuHandedness;
				return true;
			}
			if (flag3)
			{
				GetTransformAnchorsForHandedness(MenuHandedness.Left, out palmAnchor, out palmAnchorOffset);
				m_LastHandThatMetRequirements = MenuHandedness.Left;
				targetHandedness = MenuHandedness.Left;
				return true;
			}
			GetTransformAnchorsForHandedness(MenuHandedness.Right, out palmAnchor, out palmAnchorOffset);
			m_LastHandThatMetRequirements = MenuHandedness.Right;
			targetHandedness = MenuHandedness.Right;
			return true;
		case MenuHandedness.Left:
			if (flag3)
			{
				GetTransformAnchorsForHandedness(MenuHandedness.Left, out palmAnchor, out palmAnchorOffset);
				m_LastHandThatMetRequirements = MenuHandedness.Left;
				targetHandedness = MenuHandedness.Left;
				return true;
			}
			palmAnchor = null;
			palmAnchorOffset = null;
			return false;
		case MenuHandedness.Right:
			if (flag4)
			{
				GetTransformAnchorsForHandedness(MenuHandedness.Right, out palmAnchor, out palmAnchorOffset);
				m_LastHandThatMetRequirements = MenuHandedness.Right;
				targetHandedness = MenuHandedness.Right;
				return true;
			}
			palmAnchor = null;
			palmAnchorOffset = null;
			return false;
		default:
			return false;
		}
	}

	private bool TryGetInteractionManager(out XRInteractionManager manager)
	{
		if (m_InteractionManager != null)
		{
			manager = m_InteractionManager;
			return true;
		}
		if (ComponentLocatorUtility<XRInteractionManager>.TryFindComponent(out m_InteractionManager))
		{
			manager = m_InteractionManager;
			return true;
		}
		manager = null;
		return false;
	}

	private void GetTransformAnchorsForHandedness(MenuHandedness handedness, out Transform palmAnchor, out Transform palmAnchorOffset)
	{
		switch (handedness)
		{
		case MenuHandedness.Left:
			palmAnchor = m_LeftPalmAnchor;
			palmAnchorOffset = m_LeftOffsetRoot;
			break;
		case MenuHandedness.Right:
			palmAnchor = m_RightPalmAnchor;
			palmAnchorOffset = m_RightOffsetRoot;
			break;
		default:
			palmAnchor = null;
			palmAnchorOffset = null;
			break;
		}
	}

	private Vector3 GetReferenceUpDirection(Transform cameraTransform)
	{
		return m_HandMenuUpDirection switch
		{
			UpDirection.WorldUp => Vector3.up, 
			UpDirection.CameraUp => cameraTransform.up, 
			_ => base.transform.up, 
		};
	}

	private bool PalmMeetsRequirements(Transform cameraTransform, Transform palmAnchor, bool isRightHand, in FollowPreset currentPresent)
	{
		if (currentPresent == null)
		{
			return false;
		}
		Vector3 referenceAxisForTrackingAnchor = currentPresent.GetReferenceAxisForTrackingAnchor(palmAnchor, isRightHand);
		Vector3 referenceUpDirection = GetReferenceUpDirection(cameraTransform);
		bool num = !currentPresent.requirePalmFacingUser || Vector3.Dot(referenceAxisForTrackingAnchor, -cameraTransform.forward) > currentPresent.palmFacingUserDotThreshold;
		bool flag = !currentPresent.requirePalmFacingUp || Vector3.Dot(referenceAxisForTrackingAnchor, referenceUpDirection) > currentPresent.palmFacingUpDotThreshold;
		return num && flag;
	}

	private bool TryGetCamera(out Transform cameraTransform)
	{
		if (m_CameraTransform == null)
		{
			Camera main = Camera.main;
			if (main == null)
			{
				cameraTransform = null;
				return false;
			}
			m_CameraTransform = main.transform;
		}
		cameraTransform = m_CameraTransform;
		return true;
	}

	private static float AngleToDot(float angleDeg)
	{
		return Mathf.Cos(MathF.PI / 180f * angleDeg);
	}
}
