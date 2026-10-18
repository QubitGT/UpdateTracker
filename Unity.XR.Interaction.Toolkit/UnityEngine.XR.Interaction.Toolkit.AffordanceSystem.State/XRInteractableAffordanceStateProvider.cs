using System;
using System.Collections;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Datums;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

[AddComponentMenu("Affordance System/XR Interactable Affordance State Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State.XRInteractableAffordanceStateProvider.html")]
[DisallowMultipleComponent]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class XRInteractableAffordanceStateProvider : BaseAffordanceStateProvider
{
	public enum SelectClickAnimationMode
	{
		None,
		SelectEntered,
		SelectExited
	}

	public enum ActivateClickAnimationMode
	{
		None,
		Activated,
		Deactivated
	}

	[SerializeField]
	[RequireInterface(typeof(IXRInteractable))]
	[Tooltip("The interactable component that drives the affordance states. If null, Unity will try and find an interactable component attached.")]
	private Object m_InteractableSource;

	[Header("Event Constraints")]
	[SerializeField]
	[Tooltip("When hover events are registered and this is true, the state will fallback to idle or disabled.")]
	private bool m_IgnoreHoverEvents;

	[SerializeField]
	[Tooltip("When this is true, the state will fallback to hover if the later is not ignored. When this is false, this provider will check if the Interactable Source has priority for selection when hovered, and update its state accordingly.")]
	private bool m_IgnoreHoverPriorityEvents = true;

	[SerializeField]
	[Tooltip("When focus events are registered and this is true, the state will fallback to idle or disabled.")]
	private bool m_IgnoreFocusEvents = true;

	[SerializeField]
	[Tooltip("When select events are registered and this is true, the state will fallback to idle or disabled. Note this will not affect click animations which can be disabled separately.")]
	private bool m_IgnoreSelectEvents;

	[SerializeField]
	[Tooltip("When activate events are registered and this is true, the state will fallback to idle or disabled.Note this will not affect click animations which can be disabled separately.")]
	private bool m_IgnoreActivateEvents;

	[Header("Click Animation Config")]
	[SerializeField]
	[Tooltip("Condition to trigger click animation for Selected interaction events.")]
	private SelectClickAnimationMode m_SelectClickAnimationMode = SelectClickAnimationMode.SelectEntered;

	[SerializeField]
	[Tooltip("Condition to trigger click animation for activated interaction events.")]
	private ActivateClickAnimationMode m_ActivateClickAnimationMode;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Duration of click animations for selected and activated events.")]
	private float m_ClickAnimationDuration = 0.25f;

	[SerializeField]
	[Tooltip("Animation curve reference for click animation events. Select the More menu (⋮) to choose between a direct reference and a reusable scriptable object animation curve datum.")]
	private AnimationCurveDatumProperty m_ClickAnimationCurve = new AnimationCurveDatumProperty(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

	private IXRInteractable m_Interactable;

	private IXRHoverInteractable m_HoverInteractable;

	private IXRSelectInteractable m_SelectInteractable;

	private IXRFocusInteractable m_FocusInteractable;

	private IXRActivateInteractable m_ActivateInteractable;

	private IXRInteractionStrengthInteractable m_InteractionStrengthInteractable;

	private Coroutine m_SelectedClickAnimation;

	private Coroutine m_ActivatedClickAnimation;

	private Coroutine m_HoveredPriorityRoutine;

	private bool m_IsBoundToInteractionEvents;

	private bool m_IsActivated;

	private bool m_IsRegistered;

	private bool m_IsHoveredPriority;

	private bool m_HasHoverInteractable;

	private bool m_HasSelectInteractable;

	private bool m_HasInteractionStrengthInteractable;

	private int m_HoveringPriorityInteractorCount;

	public Object interactableSource
	{
		get
		{
			return m_InteractableSource;
		}
		set
		{
			m_InteractableSource = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				SetBoundInteractionReceiver(value as IXRInteractable);
			}
		}
	}

	public bool ignoreHoverEvents
	{
		get
		{
			return m_IgnoreHoverEvents;
		}
		set
		{
			m_IgnoreHoverEvents = value;
		}
	}

	public bool ignoreHoverPriorityEvents
	{
		get
		{
			return m_IgnoreHoverPriorityEvents;
		}
		set
		{
			if (Application.isPlaying && base.isActiveAndEnabled && !m_IgnoreHoverPriorityEvents && value)
			{
				StopHoveredPriorityRoutine();
				RefreshState();
			}
			m_IgnoreHoverPriorityEvents = value;
		}
	}

	public bool ignoreFocusEvents
	{
		get
		{
			return m_IgnoreFocusEvents;
		}
		set
		{
			m_IgnoreFocusEvents = value;
		}
	}

	public bool ignoreSelectEvents
	{
		get
		{
			return m_IgnoreSelectEvents;
		}
		set
		{
			m_IgnoreSelectEvents = value;
		}
	}

	public bool ignoreActivateEvents
	{
		get
		{
			return m_IgnoreActivateEvents;
		}
		set
		{
			m_IgnoreActivateEvents = value;
		}
	}

	public SelectClickAnimationMode selectClickAnimationMode
	{
		get
		{
			return m_SelectClickAnimationMode;
		}
		set
		{
			m_SelectClickAnimationMode = value;
		}
	}

	public ActivateClickAnimationMode activateClickAnimationMode
	{
		get
		{
			return m_ActivateClickAnimationMode;
		}
		set
		{
			m_ActivateClickAnimationMode = value;
		}
	}

	public float clickAnimationDuration
	{
		get
		{
			return m_ClickAnimationDuration;
		}
		set
		{
			m_ClickAnimationDuration = value;
		}
	}

	public AnimationCurveDatumProperty clickAnimationCurve
	{
		get
		{
			return m_ClickAnimationCurve;
		}
		set
		{
			m_ClickAnimationCurve = value;
		}
	}

	protected virtual bool isHovered
	{
		get
		{
			if (m_HasHoverInteractable)
			{
				return m_HoverInteractable.isHovered;
			}
			return false;
		}
	}

	protected virtual bool isSelected
	{
		get
		{
			if (m_HasSelectInteractable)
			{
				return m_SelectInteractable.isSelected;
			}
			return false;
		}
	}

	protected virtual bool isFocused
	{
		get
		{
			if (m_FocusInteractable != null)
			{
				return m_FocusInteractable.isFocused;
			}
			return false;
		}
	}

	protected virtual bool isActivated => m_IsActivated;

	protected virtual bool isRegistered => m_IsRegistered;

	protected void Awake()
	{
		IXRInteractable boundInteractionReceiver = ((m_InteractableSource != null && m_InteractableSource is IXRInteractable iXRInteractable) ? iXRInteractable : GetComponentInParent<IXRInteractable>());
		if (!SetBoundInteractionReceiver(boundInteractionReceiver))
		{
			XRLoggingUtils.LogWarning($"Could not find required interactable component on {base.gameObject}" + " for which to provide affordance states.", this);
			base.enabled = false;
		}
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		if (Application.isPlaying && base.isActiveAndEnabled && m_IgnoreHoverPriorityEvents)
		{
			StopHoveredPriorityRoutine();
			RefreshState();
		}
	}

	public bool SetBoundInteractionReceiver(IXRInteractable receiver)
	{
		ClearBindings();
		int num;
		if (receiver is Object @object)
		{
			num = ((@object != null) ? 1 : 0);
			if (num != 0)
			{
				m_Interactable = receiver;
				if (m_Interactable is IXRHoverInteractable hoverInteractable)
				{
					m_HoverInteractable = hoverInteractable;
				}
				if (m_Interactable is IXRSelectInteractable selectInteractable)
				{
					m_SelectInteractable = selectInteractable;
				}
				if (m_Interactable is IXRFocusInteractable focusInteractable)
				{
					m_FocusInteractable = focusInteractable;
				}
				if (m_Interactable is IXRActivateInteractable activateInteractable)
				{
					m_ActivateInteractable = activateInteractable;
				}
				if (m_Interactable is IXRInteractionStrengthInteractable interactionStrengthInteractable)
				{
					m_InteractionStrengthInteractable = interactionStrengthInteractable;
				}
				goto IL_00c4;
			}
		}
		else
		{
			num = 0;
		}
		m_Interactable = null;
		m_HoverInteractable = null;
		m_SelectInteractable = null;
		m_FocusInteractable = null;
		m_ActivateInteractable = null;
		m_InteractionStrengthInteractable = null;
		goto IL_00c4;
		IL_00c4:
		m_HasHoverInteractable = m_HoverInteractable != null;
		m_HasSelectInteractable = m_SelectInteractable != null;
		m_HasInteractionStrengthInteractable = m_InteractionStrengthInteractable != null;
		BindToProviders();
		return (byte)num != 0;
	}

	protected virtual void OnRegistered(InteractableRegisteredEventArgs args)
	{
		m_IsRegistered = true;
		RefreshState();
	}

	protected virtual void OnUnregistered(InteractableUnregisteredEventArgs args)
	{
		m_IsRegistered = false;
		RefreshState();
	}

	protected virtual void OnFirstHoverEntered(HoverEnterEventArgs args)
	{
		RefreshState();
	}

	protected virtual void OnLastHoverExited(HoverExitEventArgs args)
	{
		RefreshState();
	}

	protected virtual void OnHoverEntered(HoverEnterEventArgs args)
	{
		if (!m_IgnoreHoverPriorityEvents && args.interactorObject is IXRTargetPriorityInteractor iXRTargetPriorityInteractor)
		{
			m_HoveringPriorityInteractorCount++;
			if (iXRTargetPriorityInteractor.targetPriorityMode != 0)
			{
				m_HoveredPriorityRoutine = m_HoveredPriorityRoutine ?? StartCoroutine(HoveredPriorityRoutine());
			}
		}
	}

	protected virtual void OnHoverExited(HoverExitEventArgs args)
	{
		if (!m_IgnoreHoverPriorityEvents && args.interactorObject is IXRTargetPriorityInteractor)
		{
			m_HoveringPriorityInteractorCount--;
			if (m_HoveringPriorityInteractorCount <= 0)
			{
				StopHoveredPriorityRoutine();
				RefreshState();
			}
		}
	}

	private void StopHoveredPriorityRoutine()
	{
		m_HoveringPriorityInteractorCount = 0;
		m_IsHoveredPriority = false;
		if (m_HoveredPriorityRoutine != null)
		{
			StopCoroutine(m_HoveredPriorityRoutine);
			m_HoveredPriorityRoutine = null;
		}
	}

	protected virtual void OnFirstSelectEntered(SelectEnterEventArgs args)
	{
		if (m_IgnoreSelectEvents || m_SelectClickAnimationMode != SelectClickAnimationMode.SelectEntered || m_ClickAnimationDuration < Mathf.Epsilon)
		{
			RefreshState();
		}
		else
		{
			SelectedClickBehavior();
		}
	}

	protected virtual void OnLastSelectExited(SelectExitEventArgs args)
	{
		if (m_IgnoreSelectEvents || m_SelectClickAnimationMode != SelectClickAnimationMode.SelectExited || m_ClickAnimationDuration < Mathf.Epsilon)
		{
			if (m_SelectedClickAnimation == null)
			{
				RefreshState();
			}
		}
		else
		{
			SelectedClickBehavior();
		}
	}

	protected virtual void OnFirstFocusEntered(FocusEnterEventArgs args)
	{
		RefreshState();
	}

	protected virtual void OnLastFocusExited(FocusExitEventArgs args)
	{
		RefreshState();
	}

	protected virtual void OnActivatedEvent(ActivateEventArgs args)
	{
		m_IsActivated = true;
		if (m_IgnoreActivateEvents || m_ActivateClickAnimationMode != ActivateClickAnimationMode.Activated || m_ClickAnimationDuration < Mathf.Epsilon)
		{
			RefreshState();
		}
		else
		{
			ActivatedClickBehavior();
		}
	}

	protected virtual void OnDeactivatedEvent(DeactivateEventArgs args)
	{
		m_IsActivated = false;
		if (m_IgnoreActivateEvents || m_ActivateClickAnimationMode != ActivateClickAnimationMode.Deactivated || m_ClickAnimationDuration < Mathf.Epsilon)
		{
			if (m_ActivatedClickAnimation == null)
			{
				RefreshState();
			}
		}
		else
		{
			ActivatedClickBehavior();
		}
	}

	protected virtual void OnLargestInteractionStrengthChanged(float value)
	{
		if (m_SelectedClickAnimation == null && m_ActivatedClickAnimation == null)
		{
			RefreshState();
		}
	}

	protected virtual void SelectedClickBehavior()
	{
		StopAllClickAnimations();
		m_SelectedClickAnimation = StartCoroutine(ClickAnimation(4, m_ClickAnimationDuration, delegate
		{
			m_SelectedClickAnimation = null;
		}));
	}

	protected virtual void ActivatedClickBehavior()
	{
		StopAllClickAnimations();
		m_ActivatedClickAnimation = StartCoroutine(ClickAnimation(5, m_ClickAnimationDuration, delegate
		{
			m_ActivatedClickAnimation = null;
		}));
	}

	private void StopActivatedCoroutine()
	{
		if (m_ActivatedClickAnimation != null)
		{
			StopCoroutine(m_ActivatedClickAnimation);
			m_ActivatedClickAnimation = null;
		}
	}

	private void StopSelectedCoroutine()
	{
		if (m_SelectedClickAnimation != null)
		{
			StopCoroutine(m_SelectedClickAnimation);
			m_SelectedClickAnimation = null;
		}
	}

	private void StopAllClickAnimations()
	{
		StopActivatedCoroutine();
		StopSelectedCoroutine();
	}

	protected virtual IEnumerator ClickAnimation(byte targetStateIndex, float duration, Action onComplete = null)
	{
		for (float elapsedTime = 0f; elapsedTime < duration; elapsedTime += Time.deltaTime)
		{
			float time = Mathf.Clamp01(elapsedTime / duration);
			float transitionAmount = m_ClickAnimationCurve.Value.Evaluate(time);
			AffordanceStateData newAffordanceStateData = new AffordanceStateData(targetStateIndex, transitionAmount);
			UpdateAffordanceState(newAffordanceStateData);
			yield return null;
		}
		yield return null;
		RefreshState();
		onComplete?.Invoke();
	}

	protected virtual AffordanceStateData GenerateNewAffordanceState()
	{
		if (!m_IsBoundToInteractionEvents)
		{
			return base.currentAffordanceStateData.Value;
		}
		if (isActivated && !m_IgnoreActivateEvents)
		{
			return AffordanceStateShortcuts.activatedState;
		}
		if (!isActivated && isSelected && !m_IgnoreSelectEvents)
		{
			float transitionAmount = (m_HasInteractionStrengthInteractable ? m_InteractionStrengthInteractable.largestInteractionStrength.Value : 1f);
			return new AffordanceStateData(4, transitionAmount);
		}
		if (!isActivated && !isSelected && isHovered && !m_IgnoreHoverEvents)
		{
			int stateIndex = (m_IsHoveredPriority ? 3 : 2);
			float transitionAmount2 = (m_HasInteractionStrengthInteractable ? m_InteractionStrengthInteractable.largestInteractionStrength.Value : 0f);
			return new AffordanceStateData((byte)stateIndex, transitionAmount2);
		}
		if (!isActivated && !isSelected && !isHovered && isFocused && !m_IgnoreFocusEvents)
		{
			return AffordanceStateShortcuts.focusedState;
		}
		if (!isRegistered)
		{
			return AffordanceStateShortcuts.disabledState;
		}
		return AffordanceStateShortcuts.idleState;
	}

	private IEnumerator HoveredPriorityRoutine()
	{
		do
		{
			if (m_HoverInteractable is XRBaseInteractable xRBaseInteractable && xRBaseInteractable.interactionManager != null && xRBaseInteractable.interactionManager.IsHighestPriorityTarget(xRBaseInteractable) != m_IsHoveredPriority)
			{
				m_IsHoveredPriority = !m_IsHoveredPriority;
				RefreshState();
			}
			yield return null;
		}
		while (m_HoveringPriorityInteractorCount > 0);
		m_HoveredPriorityRoutine = null;
	}

	protected override void BindToProviders()
	{
		base.BindToProviders();
		m_IsBoundToInteractionEvents = m_Interactable is Object @object && @object != null;
		if (m_IsBoundToInteractionEvents)
		{
			m_Interactable.registered += OnRegistered;
			m_Interactable.unregistered += OnUnregistered;
			if (m_HoverInteractable != null)
			{
				m_HoverInteractable.firstHoverEntered.AddListener(OnFirstHoverEntered);
				m_HoverInteractable.lastHoverExited.AddListener(OnLastHoverExited);
				m_HoverInteractable.hoverEntered.AddListener(OnHoverEntered);
				m_HoverInteractable.hoverExited.AddListener(OnHoverExited);
			}
			if (m_SelectInteractable != null)
			{
				m_SelectInteractable.firstSelectEntered.AddListener(OnFirstSelectEntered);
				m_SelectInteractable.lastSelectExited.AddListener(OnLastSelectExited);
			}
			if (m_FocusInteractable != null)
			{
				m_FocusInteractable.firstFocusEntered.AddListener(OnFirstFocusEntered);
				m_FocusInteractable.lastFocusExited.AddListener(OnLastFocusExited);
			}
			if (m_ActivateInteractable != null)
			{
				m_ActivateInteractable.activated.AddListener(OnActivatedEvent);
				m_ActivateInteractable.deactivated.AddListener(OnDeactivatedEvent);
			}
			if (m_InteractionStrengthInteractable != null)
			{
				AddBinding(m_InteractionStrengthInteractable.largestInteractionStrength.Subscribe(OnLargestInteractionStrengthChanged));
			}
			m_IsActivated = false;
			if (m_Interactable is XRBaseInteractable xRBaseInteractable)
			{
				m_IsRegistered = xRBaseInteractable.interactionManager != null && xRBaseInteractable.interactionManager.IsRegistered(m_Interactable);
			}
			else if (m_Interactable is Behaviour behaviour)
			{
				m_IsRegistered = behaviour.isActiveAndEnabled;
			}
			else
			{
				m_IsRegistered = true;
			}
		}
		RefreshState();
	}

	public void RefreshState()
	{
		AffordanceStateData newAffordanceStateData = GenerateNewAffordanceState();
		if (newAffordanceStateData.stateIndex != 4)
		{
			StopSelectedCoroutine();
		}
		if (newAffordanceStateData.stateIndex != 5)
		{
			StopActivatedCoroutine();
		}
		UpdateAffordanceState(newAffordanceStateData);
	}

	protected override void ClearBindings()
	{
		base.ClearBindings();
		if (m_IsBoundToInteractionEvents)
		{
			m_Interactable.registered -= OnRegistered;
			m_Interactable.unregistered -= OnUnregistered;
			if (m_HoverInteractable != null)
			{
				m_HoverInteractable.firstHoverEntered.RemoveListener(OnFirstHoverEntered);
				m_HoverInteractable.lastHoverExited.RemoveListener(OnLastHoverExited);
				m_HoverInteractable.hoverEntered.RemoveListener(OnHoverEntered);
				m_HoverInteractable.hoverExited.RemoveListener(OnHoverExited);
			}
			if (m_SelectInteractable != null)
			{
				m_SelectInteractable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
				m_SelectInteractable.lastSelectExited.RemoveListener(OnLastSelectExited);
			}
			if (m_FocusInteractable != null)
			{
				m_FocusInteractable.firstFocusEntered.RemoveListener(OnFirstFocusEntered);
				m_FocusInteractable.lastFocusExited.RemoveListener(OnLastFocusExited);
			}
			if (m_ActivateInteractable != null)
			{
				m_ActivateInteractable.activated.RemoveListener(OnActivatedEvent);
				m_ActivateInteractable.deactivated.RemoveListener(OnDeactivatedEvent);
			}
		}
		m_IsBoundToInteractionEvents = false;
	}
}
