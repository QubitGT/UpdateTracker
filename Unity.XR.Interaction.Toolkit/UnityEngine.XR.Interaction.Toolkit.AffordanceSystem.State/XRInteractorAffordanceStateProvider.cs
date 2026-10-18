using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Datums;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

[AddComponentMenu("Affordance System/XR Interactor Affordance State Provider", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State.XRInteractorAffordanceStateProvider.html")]
[DisallowMultipleComponent]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class XRInteractorAffordanceStateProvider : BaseAffordanceStateProvider
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
	[RequireInterface(typeof(IXRInteractor))]
	[Tooltip("The interactor component that drives the affordance states. If null, Unity will try and find an interactor component attached.")]
	private Object m_InteractorSource;

	[Header("Event Constraints")]
	[SerializeField]
	[Tooltip("When hover events are registered and this is true, the state will fallback to idle or disabled.")]
	private bool m_IgnoreHoverEvents;

	[SerializeField]
	[Tooltip("When select events are registered and this is true, the state will fallback to idle or disabled. \nNote: Click animations must be disabled separately.")]
	private bool m_IgnoreSelectEvents;

	[SerializeField]
	[Tooltip("When activate events are registered and this is true, the state will fallback to idle or disabled.\nNote: Click animations must be disabled separately.")]
	private bool m_IgnoreActivateEvents = true;

	[SerializeField]
	[Tooltip("With the XR Ray Interactor it is possible to trigger select events from the ray interactor overlapping with a canvas.")]
	private bool m_IgnoreUGUIHover;

	[SerializeField]
	[Tooltip("With the XR Ray Interactor it is possible to trigger select events from the ray interactor overlapping with a canvas and triggering the select input.")]
	private bool m_IgnoreUGUISelect;

	[SerializeField]
	[Tooltip("This option will prevent Hover, Select, and Activate events from being triggered when they come from the XR Interaction Manager. UGUI hover and select events will still come through.")]
	private bool m_IgnoreXRInteractionEvents;

	[Header("Click Animation Config")]
	[SerializeField]
	[Tooltip("Condition to trigger click animation for Selected interaction events.")]
	private SelectClickAnimationMode m_SelectClickAnimationMode = SelectClickAnimationMode.SelectEntered;

	[SerializeField]
	[Tooltip("Condition to trigger click animation for activated interaction events.")]
	private ActivateClickAnimationMode m_ActivateClickAnimationMode = ActivateClickAnimationMode.Activated;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Duration of click animations for selected and activated events.")]
	private float m_ClickAnimationDuration = 0.25f;

	[SerializeField]
	[Tooltip("Animation curve reference for click animation events. Select the More menu (⋮) to choose between a direct reference and a reusable asset.")]
	private AnimationCurveDatumProperty m_ClickAnimationCurve = new AnimationCurveDatumProperty(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

	private IXRInteractor m_Interactor;

	private IXRHoverInteractor m_HoverInteractor;

	private IXRSelectInteractor m_SelectInteractor;

	private IXRInteractionStrengthInteractor m_InteractionStrengthInteractor;

	private XRRayInteractor m_RayInteractor;

	private ICurveInteractionDataProvider m_CurveInteractionDataProvider;

	private bool m_IsBoundToInteractionEvents;

	private bool m_HasRayInteractor;

	private bool m_HasCurveInteractionDataProvider;

	private bool m_HasHoverInteractor;

	private bool m_HasSelectInteractor;

	private bool m_HasInteractionStrengthInteractor;

	private bool m_IsIXRInteractor;

	private Coroutine m_SelectedClickAnimation;

	private Coroutine m_ActivatedClickAnimation;

	private bool m_IsActivated;

	private bool m_IsRegistered;

	private readonly HashSet<IXRActivateInteractable> m_BoundActivateInteractable = new HashSet<IXRActivateInteractable>();

	private bool m_UIHovering;

	private bool m_UISelecting;

	private Coroutine m_UGUIUpdateCoroutine;

	public Object interactorSource
	{
		get
		{
			return m_InteractorSource;
		}
		set
		{
			m_InteractorSource = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				SetBoundInteractionReceiver(value as IXRInteractor);
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

	public bool ignoreUGUIHover
	{
		get
		{
			return m_IgnoreUGUIHover;
		}
		set
		{
			m_IgnoreUGUIHover = value;
		}
	}

	public bool ignoreUGUISelect
	{
		get
		{
			return m_IgnoreUGUISelect;
		}
		set
		{
			m_IgnoreUGUISelect = value;
		}
	}

	public bool ignoreXRInteractionEvents
	{
		get
		{
			return m_IgnoreXRInteractionEvents;
		}
		set
		{
			m_IgnoreXRInteractionEvents = value;
		}
	}

	protected virtual bool hasXRHover
	{
		get
		{
			if (!m_IgnoreXRInteractionEvents && m_HasHoverInteractor)
			{
				return m_HoverInteractor.hasHover;
			}
			return false;
		}
	}

	protected virtual bool hasUIHover
	{
		get
		{
			if (!m_IgnoreUGUIHover)
			{
				return m_UIHovering;
			}
			return false;
		}
	}

	protected virtual bool hasXRSelection
	{
		get
		{
			if (!m_IgnoreXRInteractionEvents && m_HasSelectInteractor)
			{
				return m_SelectInteractor.hasSelection;
			}
			return false;
		}
	}

	protected virtual bool hasUISelection
	{
		get
		{
			if (!m_IgnoreUGUISelect)
			{
				return m_UISelecting;
			}
			return false;
		}
	}

	protected virtual bool isActivated
	{
		get
		{
			if (!m_IgnoreXRInteractionEvents)
			{
				return m_IsActivated;
			}
			return false;
		}
	}

	protected virtual bool isRegistered => m_IsRegistered;

	protected virtual bool isBlockedByGroup
	{
		get
		{
			if (m_IsIXRInteractor)
			{
				return !m_Interactor.IsBlockedByInteractionWithinGroup();
			}
			return false;
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

	protected void Awake()
	{
		IXRInteractor boundInteractionReceiver = ((m_InteractorSource != null && m_InteractorSource is IXRInteractor iXRInteractor) ? iXRInteractor : GetComponentInParent<IXRInteractor>());
		if (!SetBoundInteractionReceiver(boundInteractionReceiver))
		{
			XRLoggingUtils.LogWarning($"Could not find required interactor component on {base.gameObject}" + " for which to provide affordance states.", this);
			base.enabled = false;
		}
	}

	public bool SetBoundInteractionReceiver(IXRInteractor interactor)
	{
		ClearBindings();
		int num;
		if (interactor is Object @object)
		{
			num = ((@object != null) ? 1 : 0);
			if (num != 0)
			{
				m_Interactor = interactor;
				if (m_Interactor is IXRHoverInteractor hoverInteractor)
				{
					m_HoverInteractor = hoverInteractor;
				}
				if (m_Interactor is IXRSelectInteractor selectInteractor)
				{
					m_SelectInteractor = selectInteractor;
				}
				if (m_Interactor is IXRInteractionStrengthInteractor interactionStrengthInteractor)
				{
					m_InteractionStrengthInteractor = interactionStrengthInteractor;
				}
				if (m_Interactor is XRRayInteractor rayInteractor)
				{
					m_RayInteractor = rayInteractor;
				}
				if (m_Interactor is ICurveInteractionDataProvider curveInteractionDataProvider)
				{
					m_CurveInteractionDataProvider = curveInteractionDataProvider;
				}
				goto IL_00c4;
			}
		}
		else
		{
			num = 0;
		}
		m_Interactor = null;
		m_HoverInteractor = null;
		m_SelectInteractor = null;
		m_InteractionStrengthInteractor = null;
		m_RayInteractor = null;
		m_CurveInteractionDataProvider = null;
		goto IL_00c4;
		IL_00c4:
		m_IsIXRInteractor = m_Interactor != null;
		m_HasHoverInteractor = m_HoverInteractor != null;
		m_HasSelectInteractor = m_SelectInteractor != null;
		m_HasInteractionStrengthInteractor = m_InteractionStrengthInteractor != null;
		m_HasRayInteractor = m_RayInteractor != null;
		m_HasCurveInteractionDataProvider = m_CurveInteractionDataProvider != null;
		BindToProviders();
		return (byte)num != 0;
	}

	protected override void BindToProviders()
	{
		base.BindToProviders();
		m_IsBoundToInteractionEvents = m_Interactor is Object @object && @object != null;
		if (m_IsBoundToInteractionEvents)
		{
			m_Interactor.registered += OnRegistered;
			m_Interactor.unregistered += OnUnregistered;
			if (m_HasHoverInteractor)
			{
				m_HoverInteractor.hoverEntered.AddListener(OnHoverEntered);
				m_HoverInteractor.hoverExited.AddListener(OnHoverExited);
			}
			if (m_HasSelectInteractor)
			{
				m_SelectInteractor.selectEntered.AddListener(OnSelectEntered);
				m_SelectInteractor.selectExited.AddListener(OnSelectExited);
			}
			if (m_HasInteractionStrengthInteractor)
			{
				AddBinding(m_InteractionStrengthInteractor.largestInteractionStrength.Subscribe(OnLargestInteractionStrengthChanged));
			}
			m_IsActivated = false;
			if (m_Interactor is XRBaseInteractor xRBaseInteractor)
			{
				m_IsRegistered = xRBaseInteractor.interactionManager != null && xRBaseInteractor.interactionManager.IsRegistered(m_Interactor);
			}
			else if (m_Interactor is Behaviour behaviour)
			{
				m_IsRegistered = behaviour.isActiveAndEnabled;
			}
			else
			{
				m_IsRegistered = true;
			}
			if (m_UGUIUpdateCoroutine != null)
			{
				StopCoroutine(m_UGUIUpdateCoroutine);
			}
			m_UGUIUpdateCoroutine = StartCoroutine(UIUpdateCheckCoroutine());
		}
		RefreshState();
	}

	public void RefreshState()
	{
		UpdateAffordanceState(GenerateNewAffordanceState());
	}

	protected override void ClearBindings()
	{
		base.ClearBindings();
		if (m_IsBoundToInteractionEvents)
		{
			m_Interactor.registered -= OnRegistered;
			m_Interactor.unregistered -= OnUnregistered;
			if (m_HasHoverInteractor)
			{
				m_HoverInteractor.hoverEntered.RemoveListener(OnHoverEntered);
				m_HoverInteractor.hoverExited.RemoveListener(OnHoverExited);
			}
			if (m_HasSelectInteractor)
			{
				m_SelectInteractor.selectEntered.RemoveListener(OnSelectEntered);
				m_SelectInteractor.selectExited.RemoveListener(OnSelectExited);
			}
		}
		foreach (IXRActivateInteractable item in m_BoundActivateInteractable)
		{
			if (item != null)
			{
				item.activated.RemoveListener(OnActivated);
				item.deactivated.RemoveListener(OnDeactivated);
			}
		}
		m_BoundActivateInteractable.Clear();
		m_IsBoundToInteractionEvents = false;
		if (m_UGUIUpdateCoroutine != null)
		{
			StopCoroutine(m_UGUIUpdateCoroutine);
			m_UGUIUpdateCoroutine = null;
		}
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
		if ((hasXRSelection || hasUISelection) && !m_IgnoreSelectEvents)
		{
			float transitionAmount = (m_HasInteractionStrengthInteractor ? m_InteractionStrengthInteractor.largestInteractionStrength.Value : 1f);
			return new AffordanceStateData(4, transitionAmount);
		}
		if ((hasXRHover || hasUIHover) && !m_IgnoreHoverEvents)
		{
			float transitionAmount2 = (m_HasInteractionStrengthInteractor ? m_InteractionStrengthInteractor.largestInteractionStrength.Value : 0f);
			return new AffordanceStateData(2, transitionAmount2);
		}
		if (!isRegistered || isBlockedByGroup)
		{
			return AffordanceStateShortcuts.disabledState;
		}
		return AffordanceStateShortcuts.idleState;
	}

	protected virtual void OnRegistered(InteractorRegisteredEventArgs args)
	{
		m_IsRegistered = true;
		RefreshState();
	}

	protected virtual void OnUnregistered(InteractorUnregisteredEventArgs args)
	{
		m_IsRegistered = false;
		RefreshState();
	}

	protected virtual void OnHoverEntered(HoverEnterEventArgs args)
	{
		if (!m_IgnoreActivateEvents && args.interactableObject is IXRActivateInteractable iXRActivateInteractable && !m_BoundActivateInteractable.Contains(iXRActivateInteractable))
		{
			m_BoundActivateInteractable.Add(iXRActivateInteractable);
			iXRActivateInteractable.activated.RemoveListener(OnActivated);
			iXRActivateInteractable.deactivated.RemoveListener(OnDeactivated);
			iXRActivateInteractable.activated.AddListener(OnActivated);
			iXRActivateInteractable.deactivated.AddListener(OnDeactivated);
		}
		RefreshState();
	}

	protected virtual void OnHoverExited(HoverExitEventArgs args)
	{
		if (args.interactableObject is IXRActivateInteractable iXRActivateInteractable && m_BoundActivateInteractable.Contains(iXRActivateInteractable))
		{
			m_BoundActivateInteractable.Remove(iXRActivateInteractable);
			iXRActivateInteractable.activated.RemoveListener(OnActivated);
			iXRActivateInteractable.deactivated.RemoveListener(OnDeactivated);
		}
		RefreshState();
	}

	protected virtual void OnSelectEntered(SelectEnterEventArgs args)
	{
		if (!m_IgnoreActivateEvents && args.interactableObject is IXRActivateInteractable iXRActivateInteractable && !m_BoundActivateInteractable.Contains(iXRActivateInteractable))
		{
			m_BoundActivateInteractable.Add(iXRActivateInteractable);
			iXRActivateInteractable.activated.RemoveListener(OnActivated);
			iXRActivateInteractable.deactivated.RemoveListener(OnDeactivated);
			iXRActivateInteractable.activated.AddListener(OnActivated);
			iXRActivateInteractable.deactivated.AddListener(OnDeactivated);
		}
		if (m_IgnoreSelectEvents || m_IgnoreXRInteractionEvents || m_SelectClickAnimationMode != SelectClickAnimationMode.SelectEntered || m_ClickAnimationDuration < Mathf.Epsilon)
		{
			RefreshState();
		}
		else
		{
			SelectedClickBehavior();
		}
	}

	protected virtual void OnSelectExited(SelectExitEventArgs args)
	{
		if (!hasXRHover && args.interactableObject is IXRActivateInteractable iXRActivateInteractable && m_BoundActivateInteractable.Contains(iXRActivateInteractable))
		{
			m_BoundActivateInteractable.Remove(iXRActivateInteractable);
			iXRActivateInteractable.activated.RemoveListener(OnActivated);
			iXRActivateInteractable.deactivated.RemoveListener(OnDeactivated);
		}
		if (m_IgnoreSelectEvents || m_IgnoreXRInteractionEvents || m_SelectClickAnimationMode != SelectClickAnimationMode.SelectExited || m_ClickAnimationDuration < Mathf.Epsilon)
		{
			RefreshState();
		}
		else
		{
			SelectedClickBehavior();
		}
	}

	protected virtual void OnLargestInteractionStrengthChanged(float value)
	{
		if (m_SelectedClickAnimation == null && m_ActivatedClickAnimation == null)
		{
			RefreshState();
		}
	}

	private void OnActivated(ActivateEventArgs args)
	{
		m_IsActivated = true;
		if (m_IgnoreActivateEvents || m_IgnoreXRInteractionEvents || m_ActivateClickAnimationMode != ActivateClickAnimationMode.Activated || m_ClickAnimationDuration < Mathf.Epsilon)
		{
			RefreshState();
		}
		else
		{
			ActivatedClickBehavior();
		}
	}

	private void OnDeactivated(DeactivateEventArgs args)
	{
		m_IsActivated = false;
		if (m_IgnoreActivateEvents || m_IgnoreXRInteractionEvents || m_ActivateClickAnimationMode != ActivateClickAnimationMode.Deactivated || m_ClickAnimationDuration < Mathf.Epsilon)
		{
			RefreshState();
		}
		else
		{
			ActivatedClickBehavior();
		}
	}

	protected virtual void SelectedClickBehavior()
	{
		if (m_SelectedClickAnimation != null)
		{
			StopCoroutine(m_SelectedClickAnimation);
		}
		m_SelectedClickAnimation = StartCoroutine(ClickAnimation(4, m_ClickAnimationDuration, delegate
		{
			m_SelectedClickAnimation = null;
		}));
	}

	protected virtual void ActivatedClickBehavior()
	{
		if (m_ActivatedClickAnimation != null)
		{
			StopCoroutine(m_ActivatedClickAnimation);
		}
		m_ActivatedClickAnimation = StartCoroutine(ClickAnimation(5, m_ClickAnimationDuration, delegate
		{
			m_ActivatedClickAnimation = null;
		}));
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

	private IEnumerator UIUpdateCheckCoroutine()
	{
		while (true)
		{
			yield return null;
			if (!m_HasCurveInteractionDataProvider && !m_HasRayInteractor)
			{
				continue;
			}
			bool flag = false;
			bool flag2 = false;
			if ((!m_IgnoreHoverEvents || !m_IgnoreSelectEvents) && (m_HasCurveInteractionDataProvider ? (m_CurveInteractionDataProvider.TryGetCurveEndPoint(out var _) == EndPointType.UI) : (m_RayInteractor.TryGetCurrentUIRaycastResult(out var _, out var raycastEndpointIndex) && raycastEndpointIndex != 0)))
			{
				if (!m_IgnoreSelectEvents && !m_IgnoreUGUISelect)
				{
					flag2 = (m_HasCurveInteractionDataProvider ? m_CurveInteractionDataProvider.hasValidSelect : (m_RayInteractor.TryGetUIModel(out var model) && model.select));
				}
				if (!m_IgnoreHoverEvents && !m_IgnoreUGUIHover)
				{
					flag = true;
				}
			}
			bool num = flag != m_UIHovering || flag2 != m_UISelecting;
			m_UIHovering = flag;
			m_UISelecting = flag2;
			if (num)
			{
				RefreshState();
			}
		}
	}
}
