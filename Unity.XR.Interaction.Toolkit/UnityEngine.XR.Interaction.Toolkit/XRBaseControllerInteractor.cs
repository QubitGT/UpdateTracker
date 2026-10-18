using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit;

[Obsolete("XRBaseControllerInteractor has been deprecated in version 3.0.0. It has been renamed to XRBaseInputInteractor. (UnityUpgradable) -> UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor")]
public abstract class XRBaseControllerInteractor : XRBaseInteractor, IXRActivateInteractor, IXRInteractor
{
	[Obsolete("XRBaseControllerInteractor.InputTriggerType has been deprecated in version 3.0.0. It has been moved to XRBaseInputInteractor.InputTriggerType.")]
	public enum InputTriggerType
	{
		State,
		StateChange,
		Toggle,
		Sticky
	}

	[SerializeField]
	private InputTriggerType m_SelectActionTrigger = InputTriggerType.StateChange;

	[SerializeField]
	private bool m_HideControllerOnSelect;

	[SerializeField]
	private bool m_AllowHoveredActivate;

	[SerializeField]
	private TargetPriorityMode m_TargetPriorityMode;

	[SerializeField]
	[FormerlySerializedAs("m_PlayAudioClipOnSelectEnter")]
	private bool m_PlayAudioClipOnSelectEntered;

	[SerializeField]
	[FormerlySerializedAs("m_AudioClipForOnSelectEnter")]
	private AudioClip m_AudioClipForOnSelectEntered;

	[SerializeField]
	[FormerlySerializedAs("m_PlayAudioClipOnSelectExit")]
	private bool m_PlayAudioClipOnSelectExited;

	[SerializeField]
	[FormerlySerializedAs("m_AudioClipForOnSelectExit")]
	private AudioClip m_AudioClipForOnSelectExited;

	[SerializeField]
	private bool m_PlayAudioClipOnSelectCanceled;

	[SerializeField]
	private AudioClip m_AudioClipForOnSelectCanceled;

	[SerializeField]
	[FormerlySerializedAs("m_PlayAudioClipOnHoverEnter")]
	private bool m_PlayAudioClipOnHoverEntered;

	[SerializeField]
	[FormerlySerializedAs("m_AudioClipForOnHoverEnter")]
	private AudioClip m_AudioClipForOnHoverEntered;

	[SerializeField]
	[FormerlySerializedAs("m_PlayAudioClipOnHoverExit")]
	private bool m_PlayAudioClipOnHoverExited;

	[SerializeField]
	[FormerlySerializedAs("m_AudioClipForOnHoverExit")]
	private AudioClip m_AudioClipForOnHoverExited;

	[SerializeField]
	private bool m_PlayAudioClipOnHoverCanceled;

	[SerializeField]
	private AudioClip m_AudioClipForOnHoverCanceled;

	[SerializeField]
	private bool m_AllowHoverAudioWhileSelecting = true;

	[SerializeField]
	[FormerlySerializedAs("m_PlayHapticsOnSelectEnter")]
	private bool m_PlayHapticsOnSelectEntered;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticSelectEnterIntensity;

	[SerializeField]
	private float m_HapticSelectEnterDuration;

	[SerializeField]
	[FormerlySerializedAs("m_PlayHapticsOnSelectExit")]
	private bool m_PlayHapticsOnSelectExited;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticSelectExitIntensity;

	[SerializeField]
	private float m_HapticSelectExitDuration;

	[SerializeField]
	private bool m_PlayHapticsOnSelectCanceled;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticSelectCancelIntensity;

	[SerializeField]
	private float m_HapticSelectCancelDuration;

	[SerializeField]
	[FormerlySerializedAs("m_PlayHapticsOnHoverEnter")]
	private bool m_PlayHapticsOnHoverEntered;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticHoverEnterIntensity;

	[SerializeField]
	private float m_HapticHoverEnterDuration;

	[SerializeField]
	[FormerlySerializedAs("m_PlayHapticsOnHoverExit")]
	private bool m_PlayHapticsOnHoverExited;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticHoverExitIntensity;

	[SerializeField]
	private float m_HapticHoverExitDuration;

	[SerializeField]
	private bool m_PlayHapticsOnHoverCanceled;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_HapticHoverCancelIntensity;

	[SerializeField]
	private float m_HapticHoverCancelDuration;

	[SerializeField]
	private bool m_AllowHoverHapticsWhileSelecting = true;

	private bool m_AllowActivate = true;

	private XRBaseController m_Controller;

	private readonly LinkedPool<ActivateEventArgs> m_ActivateEventArgs = new LinkedPool<ActivateEventArgs>(CreateActivateEventArgs, null, null, null, collectionCheck: false);

	private readonly LinkedPool<DeactivateEventArgs> m_DeactivateEventArgs = new LinkedPool<DeactivateEventArgs>(CreateDeactivateEventArgs, null, null, null, collectionCheck: false);

	private static readonly List<IXRActivateInteractable> s_ActivateTargets = new List<IXRActivateInteractable>();

	private bool m_ToggleSelectActive;

	private bool m_ToggleSelectDeactivatedThisFrame;

	private bool m_WaitingForSelectDeactivate;

	private AudioSource m_EffectsAudioSource;

	[Obsolete("playAudioClipOnSelectEnter has been deprecated. Use playAudioClipOnSelectEntered instead. (UnityUpgradable) -> playAudioClipOnSelectEntered", true)]
	public bool playAudioClipOnSelectEnter => false;

	[Obsolete("audioClipForOnSelectEnter has been deprecated. Use audioClipForOnSelectEntered instead. (UnityUpgradable) -> audioClipForOnSelectEntered", true)]
	public AudioClip audioClipForOnSelectEnter => null;

	[Obsolete("AudioClipForOnSelectEnter has been deprecated. Use audioClipForOnSelectEntered instead. (UnityUpgradable) -> audioClipForOnSelectEntered", true)]
	public AudioClip AudioClipForOnSelectEnter
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("playAudioClipOnSelectExit has been deprecated. Use playAudioClipOnSelectExited instead. (UnityUpgradable) -> playAudioClipOnSelectExited", true)]
	public bool playAudioClipOnSelectExit => false;

	[Obsolete("audioClipForOnSelectExit has been deprecated. Use audioClipForOnSelectExited instead. (UnityUpgradable) -> audioClipForOnSelectExited", true)]
	public AudioClip audioClipForOnSelectExit => null;

	[Obsolete("AudioClipForOnSelectExit has been deprecated. Use audioClipForOnSelectExited instead. (UnityUpgradable) -> audioClipForOnSelectExited", true)]
	public AudioClip AudioClipForOnSelectExit
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("playAudioClipOnHoverEnter has been deprecated. Use playAudioClipOnHoverEntered instead. (UnityUpgradable) -> playAudioClipOnHoverEntered", true)]
	public bool playAudioClipOnHoverEnter => false;

	[Obsolete("audioClipForOnHoverEnter has been deprecated. Use audioClipForOnHoverEntered instead. (UnityUpgradable) -> audioClipForOnHoverEntered", true)]
	public AudioClip audioClipForOnHoverEnter => null;

	[Obsolete("AudioClipForOnHoverEnter has been deprecated. Use audioClipForOnHoverEntered instead. (UnityUpgradable) -> audioClipForOnHoverEntered", true)]
	public AudioClip AudioClipForOnHoverEnter
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("playAudioClipOnHoverExit has been deprecated. Use playAudioClipOnHoverExited instead. (UnityUpgradable) -> playAudioClipOnHoverExited", true)]
	public bool playAudioClipOnHoverExit => false;

	[Obsolete("audioClipForOnHoverExit has been deprecated. Use audioClipForOnHoverExited instead. (UnityUpgradable) -> audioClipForOnHoverExited", true)]
	public AudioClip audioClipForOnHoverExit => null;

	[Obsolete("AudioClipForOnHoverExit has been deprecated. Use audioClipForOnHoverExited instead. (UnityUpgradable) -> audioClipForOnHoverExited", true)]
	public AudioClip AudioClipForOnHoverExit
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("playHapticsOnSelectEnter has been deprecated. Use playHapticsOnSelectEntered instead. (UnityUpgradable) -> playHapticsOnSelectEntered", true)]
	public bool playHapticsOnSelectEnter => false;

	[Obsolete("playHapticsOnSelectExit has been deprecated. Use playHapticsOnSelectExited instead. (UnityUpgradable) -> playHapticsOnSelectExited", true)]
	public bool playHapticsOnSelectExit => false;

	[Obsolete("playHapticsOnHoverEnter has been deprecated. Use playHapticsOnHoverEntered instead. (UnityUpgradable) -> playHapticsOnHoverEntered", true)]
	public bool playHapticsOnHoverEnter => false;

	[Obsolete("validTargets has been deprecated. Use a property of type List<IXRInteractable> instead.", true)]
	protected virtual List<XRBaseInteractable> validTargets { get; }

	public InputTriggerType selectActionTrigger
	{
		get
		{
			return m_SelectActionTrigger;
		}
		set
		{
			m_SelectActionTrigger = value;
		}
	}

	public bool hideControllerOnSelect
	{
		get
		{
			return m_HideControllerOnSelect;
		}
		set
		{
			m_HideControllerOnSelect = value;
			if (!m_HideControllerOnSelect && m_Controller != null)
			{
				m_Controller.hideControllerModel = false;
			}
		}
	}

	public bool allowHoveredActivate
	{
		get
		{
			return m_AllowHoveredActivate;
		}
		set
		{
			m_AllowHoveredActivate = value;
		}
	}

	public override TargetPriorityMode targetPriorityMode
	{
		get
		{
			return m_TargetPriorityMode;
		}
		set
		{
			m_TargetPriorityMode = value;
		}
	}

	public bool playAudioClipOnSelectEntered
	{
		get
		{
			return m_PlayAudioClipOnSelectEntered;
		}
		set
		{
			m_PlayAudioClipOnSelectEntered = value;
		}
	}

	public AudioClip audioClipForOnSelectEntered
	{
		get
		{
			return m_AudioClipForOnSelectEntered;
		}
		set
		{
			m_AudioClipForOnSelectEntered = value;
		}
	}

	public bool playAudioClipOnSelectExited
	{
		get
		{
			return m_PlayAudioClipOnSelectExited;
		}
		set
		{
			m_PlayAudioClipOnSelectExited = value;
		}
	}

	public AudioClip audioClipForOnSelectExited
	{
		get
		{
			return m_AudioClipForOnSelectExited;
		}
		set
		{
			m_AudioClipForOnSelectExited = value;
		}
	}

	public bool playAudioClipOnSelectCanceled
	{
		get
		{
			return m_PlayAudioClipOnSelectCanceled;
		}
		set
		{
			m_PlayAudioClipOnSelectCanceled = value;
		}
	}

	public AudioClip audioClipForOnSelectCanceled
	{
		get
		{
			return m_AudioClipForOnSelectCanceled;
		}
		set
		{
			m_AudioClipForOnSelectCanceled = value;
		}
	}

	public bool playAudioClipOnHoverEntered
	{
		get
		{
			return m_PlayAudioClipOnHoverEntered;
		}
		set
		{
			m_PlayAudioClipOnHoverEntered = value;
		}
	}

	public AudioClip audioClipForOnHoverEntered
	{
		get
		{
			return m_AudioClipForOnHoverEntered;
		}
		set
		{
			m_AudioClipForOnHoverEntered = value;
		}
	}

	public bool playAudioClipOnHoverExited
	{
		get
		{
			return m_PlayAudioClipOnHoverExited;
		}
		set
		{
			m_PlayAudioClipOnHoverExited = value;
		}
	}

	public AudioClip audioClipForOnHoverExited
	{
		get
		{
			return m_AudioClipForOnHoverExited;
		}
		set
		{
			m_AudioClipForOnHoverExited = value;
		}
	}

	public bool playAudioClipOnHoverCanceled
	{
		get
		{
			return m_PlayAudioClipOnHoverCanceled;
		}
		set
		{
			m_PlayAudioClipOnHoverCanceled = value;
		}
	}

	public AudioClip audioClipForOnHoverCanceled
	{
		get
		{
			return m_AudioClipForOnHoverCanceled;
		}
		set
		{
			m_AudioClipForOnHoverCanceled = value;
		}
	}

	public bool allowHoverAudioWhileSelecting
	{
		get
		{
			return m_AllowHoverAudioWhileSelecting;
		}
		set
		{
			m_AllowHoverAudioWhileSelecting = value;
		}
	}

	public bool playHapticsOnSelectEntered
	{
		get
		{
			return m_PlayHapticsOnSelectEntered;
		}
		set
		{
			m_PlayHapticsOnSelectEntered = value;
		}
	}

	public float hapticSelectEnterIntensity
	{
		get
		{
			return m_HapticSelectEnterIntensity;
		}
		set
		{
			m_HapticSelectEnterIntensity = value;
		}
	}

	public float hapticSelectEnterDuration
	{
		get
		{
			return m_HapticSelectEnterDuration;
		}
		set
		{
			m_HapticSelectEnterDuration = value;
		}
	}

	public bool playHapticsOnSelectExited
	{
		get
		{
			return m_PlayHapticsOnSelectExited;
		}
		set
		{
			m_PlayHapticsOnSelectExited = value;
		}
	}

	public float hapticSelectExitIntensity
	{
		get
		{
			return m_HapticSelectExitIntensity;
		}
		set
		{
			m_HapticSelectExitIntensity = value;
		}
	}

	public float hapticSelectExitDuration
	{
		get
		{
			return m_HapticSelectExitDuration;
		}
		set
		{
			m_HapticSelectExitDuration = value;
		}
	}

	public bool playHapticsOnSelectCanceled
	{
		get
		{
			return m_PlayHapticsOnSelectCanceled;
		}
		set
		{
			m_PlayHapticsOnSelectCanceled = value;
		}
	}

	public float hapticSelectCancelIntensity
	{
		get
		{
			return m_HapticSelectCancelIntensity;
		}
		set
		{
			m_HapticSelectCancelIntensity = value;
		}
	}

	public float hapticSelectCancelDuration
	{
		get
		{
			return m_HapticSelectCancelDuration;
		}
		set
		{
			m_HapticSelectCancelDuration = value;
		}
	}

	public bool playHapticsOnHoverEntered
	{
		get
		{
			return m_PlayHapticsOnHoverEntered;
		}
		set
		{
			m_PlayHapticsOnHoverEntered = value;
		}
	}

	public float hapticHoverEnterIntensity
	{
		get
		{
			return m_HapticHoverEnterIntensity;
		}
		set
		{
			m_HapticHoverEnterIntensity = value;
		}
	}

	public float hapticHoverEnterDuration
	{
		get
		{
			return m_HapticHoverEnterDuration;
		}
		set
		{
			m_HapticHoverEnterDuration = value;
		}
	}

	public bool playHapticsOnHoverExited
	{
		get
		{
			return m_PlayHapticsOnHoverExited;
		}
		set
		{
			m_PlayHapticsOnHoverExited = value;
		}
	}

	public float hapticHoverExitIntensity
	{
		get
		{
			return m_HapticHoverExitIntensity;
		}
		set
		{
			m_HapticHoverExitIntensity = value;
		}
	}

	public float hapticHoverExitDuration
	{
		get
		{
			return m_HapticHoverExitDuration;
		}
		set
		{
			m_HapticHoverExitDuration = value;
		}
	}

	public bool playHapticsOnHoverCanceled
	{
		get
		{
			return m_PlayHapticsOnHoverCanceled;
		}
		set
		{
			m_PlayHapticsOnHoverCanceled = value;
		}
	}

	public float hapticHoverCancelIntensity
	{
		get
		{
			return m_HapticHoverCancelIntensity;
		}
		set
		{
			m_HapticHoverCancelIntensity = value;
		}
	}

	public float hapticHoverCancelDuration
	{
		get
		{
			return m_HapticHoverCancelDuration;
		}
		set
		{
			m_HapticHoverCancelDuration = value;
		}
	}

	public bool allowHoverHapticsWhileSelecting
	{
		get
		{
			return m_AllowHoverHapticsWhileSelecting;
		}
		set
		{
			m_AllowHoverHapticsWhileSelecting = value;
		}
	}

	public bool allowActivate
	{
		get
		{
			return m_AllowActivate;
		}
		set
		{
			m_AllowActivate = value;
		}
	}

	public XRBaseController xrController
	{
		get
		{
			return m_Controller;
		}
		set
		{
			if (m_Controller != value)
			{
				m_Controller = value;
				OnXRControllerChanged();
			}
		}
	}

	public override bool isSelectActive
	{
		get
		{
			if (!base.isSelectActive)
			{
				return false;
			}
			if (base.isPerformingManualInteraction)
			{
				return true;
			}
			switch (m_SelectActionTrigger)
			{
			case InputTriggerType.State:
				if (m_Controller != null)
				{
					return m_Controller.selectInteractionState.active;
				}
				return false;
			case InputTriggerType.StateChange:
				if (!(m_Controller != null) || !m_Controller.selectInteractionState.activatedThisFrame)
				{
					if (base.hasSelection && m_Controller != null)
					{
						return !m_Controller.selectInteractionState.deactivatedThisFrame;
					}
					return false;
				}
				return true;
			case InputTriggerType.Toggle:
				if (!m_ToggleSelectActive)
				{
					if (m_Controller != null && m_Controller.selectInteractionState.activatedThisFrame)
					{
						return !m_ToggleSelectDeactivatedThisFrame;
					}
					return false;
				}
				return true;
			case InputTriggerType.Sticky:
				if (!m_ToggleSelectActive && !m_WaitingForSelectDeactivate)
				{
					if (m_Controller != null)
					{
						return m_Controller.selectInteractionState.activatedThisFrame;
					}
					return false;
				}
				return true;
			default:
				return false;
			}
		}
	}

	protected virtual bool isUISelectActive
	{
		get
		{
			if (m_Controller != null)
			{
				return m_Controller.uiPressInteractionState.active;
			}
			return false;
		}
	}

	protected Vector2 uiScrollValue
	{
		get
		{
			if (!(m_Controller != null))
			{
				return Vector2.zero;
			}
			return m_Controller.uiScrollValue;
		}
	}

	public virtual bool shouldActivate
	{
		get
		{
			if (m_AllowActivate && (base.hasSelection || (m_AllowHoveredActivate && base.hasHover)) && m_Controller != null)
			{
				return m_Controller.activateInteractionState.activatedThisFrame;
			}
			return false;
		}
	}

	public virtual bool shouldDeactivate
	{
		get
		{
			if (m_AllowActivate && (base.hasSelection || (m_AllowHoveredActivate && base.hasHover)) && m_Controller != null)
			{
				return m_Controller.activateInteractionState.deactivatedThisFrame;
			}
			return false;
		}
	}

	private static ActivateEventArgs CreateActivateEventArgs()
	{
		return new ActivateEventArgs();
	}

	private static DeactivateEventArgs CreateDeactivateEventArgs()
	{
		return new DeactivateEventArgs();
	}

	protected override void Awake()
	{
		targetsForSelection = new List<IXRSelectInteractable>();
		base.Awake();
		xrController = base.gameObject.GetComponentInParent<XRBaseController>(includeInactive: true);
		if (xrController == null)
		{
			Debug.LogWarning(string.Format("Could not find {0} component on {1} or any of its parents.", "XRBaseController", base.gameObject), this);
		}
		if (m_SelectActionTrigger == InputTriggerType.Toggle && base.startingSelectedInteractable != null)
		{
			m_ToggleSelectActive = true;
		}
		if (m_PlayAudioClipOnSelectEntered || m_PlayAudioClipOnSelectExited || m_PlayAudioClipOnSelectCanceled || m_PlayAudioClipOnHoverEntered || m_PlayAudioClipOnHoverExited || m_PlayAudioClipOnHoverCanceled)
		{
			CreateEffectsAudioSource();
		}
	}

	private protected virtual void OnXRControllerChanged()
	{
	}

	public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.PreprocessInteractor(updatePhase);
		if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
		{
			return;
		}
		m_ToggleSelectDeactivatedThisFrame = false;
		if ((m_SelectActionTrigger == InputTriggerType.Toggle || m_SelectActionTrigger == InputTriggerType.Sticky) && !(m_Controller == null))
		{
			if (m_ToggleSelectActive && m_Controller.selectInteractionState.activatedThisFrame)
			{
				m_ToggleSelectActive = false;
				m_ToggleSelectDeactivatedThisFrame = true;
				m_WaitingForSelectDeactivate = true;
			}
			if (m_Controller.selectInteractionState.deactivatedThisFrame)
			{
				m_WaitingForSelectDeactivate = false;
			}
		}
	}

	public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		base.ProcessInteractor(updatePhase);
		if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic || !m_AllowActivate)
		{
			return;
		}
		bool flag = shouldActivate;
		bool flag2 = shouldDeactivate;
		if (flag || flag2)
		{
			GetActivateTargets(s_ActivateTargets);
			if (flag)
			{
				SendActivateEvent(s_ActivateTargets);
			}
			if (flag2)
			{
				SendDeactivateEvent(s_ActivateTargets);
			}
		}
	}

	private void SendActivateEvent(List<IXRActivateInteractable> targets)
	{
		foreach (IXRActivateInteractable target in targets)
		{
			if (target != null && !(target as Object == null))
			{
				ActivateEventArgs v;
				using (m_ActivateEventArgs.Get(out v))
				{
					v.interactorObject = this;
					v.interactableObject = target;
					target.OnActivated(v);
				}
			}
		}
	}

	private void SendDeactivateEvent(List<IXRActivateInteractable> targets)
	{
		foreach (IXRActivateInteractable target in targets)
		{
			if (target != null && !(target as Object == null))
			{
				DeactivateEventArgs v;
				using (m_DeactivateEventArgs.Get(out v))
				{
					v.interactorObject = this;
					v.interactableObject = target;
					target.OnDeactivated(v);
				}
			}
		}
	}

	public virtual void GetActivateTargets(List<IXRActivateInteractable> targets)
	{
		targets.Clear();
		if (base.hasSelection)
		{
			foreach (IXRSelectInteractable item3 in base.interactablesSelected)
			{
				if (item3 is IXRActivateInteractable item)
				{
					targets.Add(item);
				}
			}
			return;
		}
		if (!m_AllowHoveredActivate || !base.hasHover)
		{
			return;
		}
		foreach (IXRHoverInteractable item4 in base.interactablesHovered)
		{
			if (item4 is IXRActivateInteractable item2)
			{
				targets.Add(item2);
			}
		}
	}

	protected override void OnSelectEntering(SelectEnterEventArgs args)
	{
		base.OnSelectEntering(args);
		HandleSelecting();
		if (m_PlayHapticsOnSelectEntered)
		{
			SendHapticImpulse(m_HapticSelectEnterIntensity, m_HapticSelectEnterDuration);
		}
		if (m_PlayAudioClipOnSelectEntered)
		{
			PlayAudio(m_AudioClipForOnSelectEntered);
		}
	}

	protected override void OnSelectExiting(SelectExitEventArgs args)
	{
		base.OnSelectExiting(args);
		HandleDeselecting();
		if (args.isCanceled)
		{
			if (m_PlayHapticsOnSelectCanceled)
			{
				SendHapticImpulse(m_HapticSelectCancelIntensity, m_HapticSelectCancelDuration);
			}
			if (m_PlayAudioClipOnSelectCanceled)
			{
				PlayAudio(m_AudioClipForOnSelectCanceled);
			}
		}
		else
		{
			if (m_PlayHapticsOnSelectExited)
			{
				SendHapticImpulse(m_HapticSelectExitIntensity, m_HapticSelectExitDuration);
			}
			if (m_PlayAudioClipOnSelectExited)
			{
				PlayAudio(m_AudioClipForOnSelectExited);
			}
		}
	}

	protected override void OnHoverEntering(HoverEnterEventArgs args)
	{
		base.OnHoverEntering(args);
		IXRHoverInteractable interactableObject = args.interactableObject;
		if (m_PlayHapticsOnHoverEntered && CanPlayHoverHaptics(interactableObject))
		{
			SendHapticImpulse(m_HapticHoverEnterIntensity, m_HapticHoverEnterDuration);
		}
		if (m_PlayAudioClipOnHoverEntered && CanPlayHoverAudio(interactableObject))
		{
			PlayAudio(m_AudioClipForOnHoverEntered);
		}
	}

	protected override void OnHoverExiting(HoverExitEventArgs args)
	{
		base.OnHoverExiting(args);
		IXRHoverInteractable interactableObject = args.interactableObject;
		if (args.isCanceled)
		{
			if (m_PlayHapticsOnHoverCanceled && CanPlayHoverHaptics(interactableObject))
			{
				SendHapticImpulse(m_HapticHoverCancelIntensity, m_HapticHoverCancelDuration);
			}
			if (m_PlayAudioClipOnHoverCanceled && CanPlayHoverAudio(interactableObject))
			{
				PlayAudio(m_AudioClipForOnHoverCanceled);
			}
		}
		else
		{
			if (m_PlayHapticsOnHoverExited && CanPlayHoverHaptics(interactableObject))
			{
				SendHapticImpulse(m_HapticHoverExitIntensity, m_HapticHoverExitDuration);
			}
			if (m_PlayAudioClipOnHoverExited && CanPlayHoverAudio(interactableObject))
			{
				PlayAudio(m_AudioClipForOnHoverExited);
			}
		}
	}

	private bool CanPlayHoverAudio(IXRHoverInteractable hoveredInteractable)
	{
		if (!m_AllowHoverAudioWhileSelecting)
		{
			return !IsSelecting(hoveredInteractable);
		}
		return true;
	}

	private bool CanPlayHoverHaptics(IXRHoverInteractable hoveredInteractable)
	{
		if (!m_AllowHoverHapticsWhileSelecting)
		{
			return !IsSelecting(hoveredInteractable);
		}
		return true;
	}

	public bool SendHapticImpulse(float amplitude, float duration)
	{
		if (m_Controller != null)
		{
			return m_Controller.SendHapticImpulse(amplitude, duration);
		}
		return false;
	}

	protected virtual void PlayAudio(AudioClip audioClip)
	{
		if (!(audioClip == null))
		{
			if (m_EffectsAudioSource == null)
			{
				CreateEffectsAudioSource();
			}
			m_EffectsAudioSource.PlayOneShot(audioClip);
		}
	}

	private void CreateEffectsAudioSource()
	{
		m_EffectsAudioSource = base.gameObject.AddComponent<AudioSource>();
		m_EffectsAudioSource.loop = false;
		m_EffectsAudioSource.playOnAwake = false;
	}

	private void HandleSelecting()
	{
		m_ToggleSelectActive = true;
		m_WaitingForSelectDeactivate = false;
		if (m_HideControllerOnSelect && m_Controller != null)
		{
			m_Controller.hideControllerModel = true;
		}
	}

	private void HandleDeselecting()
	{
		if (!base.hasSelection)
		{
			m_ToggleSelectActive = false;
			m_WaitingForSelectDeactivate = false;
			if (m_Controller != null)
			{
				m_Controller.hideControllerModel = false;
			}
		}
	}

	Transform IXRInteractor.get_transform()
	{
		return base.transform;
	}
}
