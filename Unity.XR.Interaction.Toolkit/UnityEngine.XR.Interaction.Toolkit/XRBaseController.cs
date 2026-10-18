using System;
using Unity.XR.CoreUtils;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

namespace UnityEngine.XR.Interaction.Toolkit;

[DefaultExecutionOrder(-29990)]
[DisallowMultipleComponent]
[Obsolete("XRBaseController has been deprecated in version 3.0.0. Its functionality has been distributed into different components.")]
public abstract class XRBaseController : MonoBehaviour, IXRHapticImpulseProvider
{
	public enum UpdateType
	{
		UpdateAndBeforeRender,
		Update,
		BeforeRender,
		Fixed
	}

	private class HapticImpulseChannel : IXRHapticImpulseChannel
	{
		private readonly XRBaseController m_Controller;

		private bool m_WarningLogged;

		public HapticImpulseChannel(XRBaseController controller)
		{
			m_Controller = controller;
		}

		public bool SendHapticImpulse(float amplitude, float duration, float frequency)
		{
			if (frequency > 0f && !m_WarningLogged)
			{
				Debug.LogWarning($"Frequency is not supported when using {m_Controller} as the haptic impulse channel." + " You may need to update the HapticImpulsePlayer to use an Input Action Reference with a Haptic control binding rather than using an Object Reference to the controller.", m_Controller);
				m_WarningLogged = true;
			}
			return m_Controller.SendHapticImpulse(amplitude, duration);
		}
	}

	[SerializeField]
	private UpdateType m_UpdateTrackingType;

	[SerializeField]
	private bool m_EnableInputTracking = true;

	[SerializeField]
	private bool m_EnableInputActions = true;

	[SerializeField]
	private Transform m_ModelPrefab;

	[SerializeField]
	[FormerlySerializedAs("m_ModelTransform")]
	private Transform m_ModelParent;

	[SerializeField]
	private Transform m_Model;

	[SerializeField]
	private bool m_AnimateModel;

	[SerializeField]
	private string m_ModelSelectTransition;

	[SerializeField]
	private string m_ModelDeSelectTransition;

	private bool m_HideControllerModel;

	private InteractionState m_SelectInteractionState;

	private InteractionState m_ActivateInteractionState;

	private InteractionState m_UIPressInteractionState;

	private Vector2 m_UIScrollValue;

	private XRControllerState m_ControllerState;

	private bool m_CreateControllerState = true;

	private Animator m_ModelAnimator;

	private bool m_HasWarnedAnimatorMissing;

	private bool m_PerformSetup = true;

	private HapticImpulseChannel m_HapticChannel;

	private HapticImpulseSingleChannelGroup m_HapticChannelGroup;

	[Obsolete("modelTransform has been deprecated due to being renamed. Use modelParent instead. (UnityUpgradable) -> modelParent", true)]
	public Transform modelTransform
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("anchorControlDeadzone is obsolete. Please configure deadzone on the Rotate Anchor and Translate Anchor Actions.", true)]
	public float anchorControlDeadzone
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	[Obsolete("anchorControlOffAxisDeadzone is obsolete. Please configure deadzone on the Rotate Anchor and Translate Anchor Actions.", true)]
	public float anchorControlOffAxisDeadzone
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	public UpdateType updateTrackingType
	{
		get
		{
			return m_UpdateTrackingType;
		}
		set
		{
			m_UpdateTrackingType = value;
		}
	}

	public bool enableInputTracking
	{
		get
		{
			return m_EnableInputTracking;
		}
		set
		{
			m_EnableInputTracking = value;
		}
	}

	public bool enableInputActions
	{
		get
		{
			return m_EnableInputActions;
		}
		set
		{
			m_EnableInputActions = value;
		}
	}

	public Transform modelPrefab
	{
		get
		{
			return m_ModelPrefab;
		}
		set
		{
			m_ModelPrefab = value;
		}
	}

	public Transform modelParent
	{
		get
		{
			return m_ModelParent;
		}
		set
		{
			m_ModelParent = value;
			if (m_Model != null)
			{
				m_Model.parent = m_ModelParent;
			}
		}
	}

	public Transform model
	{
		get
		{
			return m_Model;
		}
		set
		{
			m_Model = value;
		}
	}

	public bool animateModel
	{
		get
		{
			return m_AnimateModel;
		}
		set
		{
			m_AnimateModel = value;
		}
	}

	public string modelSelectTransition
	{
		get
		{
			return m_ModelSelectTransition;
		}
		set
		{
			m_ModelSelectTransition = value;
		}
	}

	public string modelDeSelectTransition
	{
		get
		{
			return m_ModelDeSelectTransition;
		}
		set
		{
			m_ModelDeSelectTransition = value;
		}
	}

	public bool hideControllerModel
	{
		get
		{
			return m_HideControllerModel;
		}
		set
		{
			m_HideControllerModel = value;
			if (m_Model != null)
			{
				m_Model.gameObject.SetActive(!m_HideControllerModel);
			}
		}
	}

	public InteractionState selectInteractionState => m_SelectInteractionState;

	public InteractionState activateInteractionState => m_ActivateInteractionState;

	public InteractionState uiPressInteractionState => m_UIPressInteractionState;

	public Vector2 uiScrollValue => m_UIScrollValue;

	public XRControllerState currentControllerState
	{
		get
		{
			SetupControllerState();
			return m_ControllerState;
		}
		set
		{
			m_ControllerState = value;
			m_CreateControllerState = false;
		}
	}

	[Obsolete("GetControllerState has been deprecated. Use currentControllerState instead.", true)]
	public virtual bool GetControllerState(out XRControllerState controllerState)
	{
		controllerState = null;
		return false;
	}

	[Obsolete("SetControllerState has been deprecated. Use currentControllerState instead.", true)]
	public virtual void SetControllerState(XRControllerState controllerState)
	{
	}

	protected virtual void Awake()
	{
		if (m_ModelParent == null)
		{
			m_ModelParent = new GameObject("[" + base.gameObject.name + "] Model Parent").transform;
			m_ModelParent.SetParent(base.transform, worldPositionStays: false);
			m_ModelParent.SetLocalPose(Pose.identity);
		}
	}

	protected virtual void OnEnable()
	{
		Application.onBeforeRender += OnBeforeRender;
	}

	protected virtual void OnDisable()
	{
		Application.onBeforeRender -= OnBeforeRender;
	}

	protected void Update()
	{
		UpdateController();
	}

	private void SetupModel()
	{
		if (m_Model == null)
		{
			GameObject gameObject = GetModelPrefab();
			if (gameObject != null)
			{
				m_Model = Object.Instantiate(gameObject, m_ModelParent).transform;
			}
		}
		if (m_Model != null)
		{
			m_Model.gameObject.SetActive(!m_HideControllerModel);
		}
	}

	private void SetupControllerState()
	{
		if (m_ControllerState == null && m_CreateControllerState)
		{
			m_ControllerState = new XRControllerState();
		}
	}

	protected virtual GameObject GetModelPrefab()
	{
		if (!(m_ModelPrefab != null))
		{
			return null;
		}
		return m_ModelPrefab.gameObject;
	}

	protected virtual void UpdateController()
	{
		if (m_PerformSetup)
		{
			SetupModel();
			SetupControllerState();
			m_PerformSetup = false;
		}
		if (m_EnableInputTracking && (m_UpdateTrackingType == UpdateType.Update || m_UpdateTrackingType == UpdateType.UpdateAndBeforeRender))
		{
			UpdateTrackingInput(m_ControllerState);
		}
		if (m_EnableInputActions)
		{
			UpdateInput(m_ControllerState);
			UpdateControllerModelAnimation();
		}
		ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase.Dynamic, m_ControllerState);
	}

	protected virtual void OnBeforeRender()
	{
		if (m_EnableInputTracking && (m_UpdateTrackingType == UpdateType.BeforeRender || m_UpdateTrackingType == UpdateType.UpdateAndBeforeRender))
		{
			UpdateTrackingInput(m_ControllerState);
		}
		ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender, m_ControllerState);
	}

	protected virtual void FixedUpdate()
	{
		if (m_EnableInputTracking && m_UpdateTrackingType == UpdateType.Fixed)
		{
			UpdateTrackingInput(m_ControllerState);
		}
		ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase.Fixed, m_ControllerState);
	}

	protected virtual void ApplyControllerState(XRInteractionUpdateOrder.UpdatePhase updatePhase, XRControllerState controllerState)
	{
		if (controllerState == null)
		{
			return;
		}
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			m_SelectInteractionState = controllerState.selectInteractionState;
			m_ActivateInteractionState = controllerState.activateInteractionState;
			m_UIPressInteractionState = controllerState.uiPressInteractionState;
			m_UIScrollValue = controllerState.uiScrollValue;
		}
		if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic || updatePhase == XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender || updatePhase == XRInteractionUpdateOrder.UpdatePhase.Fixed)
		{
			bool flag = (controllerState.inputTrackingState & InputTrackingState.Position) != 0;
			bool flag2 = (controllerState.inputTrackingState & InputTrackingState.Rotation) != 0;
			if (flag && flag2)
			{
				base.transform.SetLocalPose(new Pose(controllerState.position, controllerState.rotation));
			}
			else if (flag)
			{
				base.transform.localPosition = controllerState.position;
			}
			else if (flag2)
			{
				base.transform.localRotation = controllerState.rotation;
			}
		}
	}

	protected virtual void UpdateTrackingInput(XRControllerState controllerState)
	{
	}

	protected virtual void UpdateInput(XRControllerState controllerState)
	{
	}

	protected virtual void UpdateControllerModelAnimation()
	{
		if (!m_AnimateModel || !(m_Model != null))
		{
			return;
		}
		if ((m_ModelAnimator == null || m_ModelAnimator.gameObject != m_Model.gameObject) && !m_Model.TryGetComponent<Animator>(out m_ModelAnimator))
		{
			if (!m_HasWarnedAnimatorMissing)
			{
				Debug.LogWarning("Animate Model is enabled, but there is no Animator component on the model. Unable to activate named triggers to animate the model.", this);
				m_HasWarnedAnimatorMissing = true;
			}
		}
		else if (m_SelectInteractionState.activatedThisFrame)
		{
			m_ModelAnimator.SetTrigger(m_ModelSelectTransition);
		}
		else if (m_SelectInteractionState.deactivatedThisFrame)
		{
			m_ModelAnimator.SetTrigger(m_ModelDeSelectTransition);
		}
	}

	public virtual bool SendHapticImpulse(float amplitude, float duration)
	{
		return false;
	}

	IXRHapticImpulseChannelGroup IXRHapticImpulseProvider.GetChannelGroup()
	{
		if (m_HapticChannel == null)
		{
			m_HapticChannel = new HapticImpulseChannel(this);
		}
		return m_HapticChannelGroup ?? (m_HapticChannelGroup = new HapticImpulseSingleChannelGroup(m_HapticChannel));
	}
}
