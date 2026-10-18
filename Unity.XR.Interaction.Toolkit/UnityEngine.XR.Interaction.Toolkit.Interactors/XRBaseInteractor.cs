using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[SelectionBase]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-99)]
public abstract class XRBaseInteractor : MonoBehaviour, IXRHoverInteractor, IXRInteractor, IXRSelectInteractor, IXRTargetPriorityInteractor, IXRGroupMember, IXRInteractionStrengthInteractor
{
	private const float k_InteractionStrengthHover = 0f;

	private const float k_InteractionStrengthSelect = 1f;

	[SerializeField]
	private XRInteractionManager m_InteractionManager;

	[SerializeField]
	private InteractionLayerMask m_InteractionLayers = -1;

	[SerializeField]
	private InteractorHandedness m_Handedness;

	[SerializeField]
	private Transform m_AttachTransform;

	[SerializeField]
	private bool m_KeepSelectedTargetValid = true;

	[SerializeField]
	private bool m_DisableVisualsWhenBlockedInGroup = true;

	[SerializeField]
	private XRBaseInteractable m_StartingSelectedInteractable;

	[SerializeField]
	private XRBaseTargetFilter m_StartingTargetFilter;

	[SerializeField]
	private HoverEnterEvent m_HoverEntered = new HoverEnterEvent();

	[SerializeField]
	private HoverExitEvent m_HoverExited = new HoverExitEvent();

	[SerializeField]
	private SelectEnterEvent m_SelectEntered = new SelectEnterEvent();

	[SerializeField]
	private SelectExitEvent m_SelectExited = new SelectExitEvent();

	private IXRTargetFilter m_TargetFilter;

	private bool m_AllowHover = true;

	private bool m_AllowSelect = true;

	private bool m_IsPerformingManualInteraction;

	private readonly HashSetList<IXRHoverInteractable> m_InteractablesHovered = new HashSetList<IXRHoverInteractable>();

	private readonly HashSetList<IXRSelectInteractable> m_InteractablesSelected = new HashSetList<IXRSelectInteractable>();

	[SerializeField]
	[RequireInterface(typeof(IXRHoverFilter))]
	private List<Object> m_StartingHoverFilters = new List<Object>();

	private readonly ExposedRegistrationList<IXRHoverFilter> m_HoverFilters = new ExposedRegistrationList<IXRHoverFilter>
	{
		bufferChanges = false
	};

	[SerializeField]
	[RequireInterface(typeof(IXRSelectFilter))]
	private List<Object> m_StartingSelectFilters = new List<Object>();

	private readonly ExposedRegistrationList<IXRSelectFilter> m_SelectFilters = new ExposedRegistrationList<IXRSelectFilter>
	{
		bufferChanges = false
	};

	private readonly BindableVariable<float> m_LargestInteractionStrength = new BindableVariable<float>(0f);

	private bool m_ClearedLargestInteractionStrength;

	private readonly Dictionary<IXRSelectInteractable, Pose> m_AttachPoseOnSelect = new Dictionary<IXRSelectInteractable, Pose>();

	private readonly Dictionary<IXRSelectInteractable, Pose> m_LocalAttachPoseOnSelect = new Dictionary<IXRSelectInteractable, Pose>();

	private readonly HashSetList<IXRInteractionStrengthInteractable> m_InteractionStrengthInteractables = new HashSetList<IXRInteractionStrengthInteractable>();

	private readonly Dictionary<IXRInteractable, float> m_InteractionStrengths = new Dictionary<IXRInteractable, float>();

	private IXRSelectInteractable m_ManualInteractionInteractable;

	private XRInteractionManager m_RegisteredInteractionManager;

	private static readonly ProfilerMarker s_ProcessInteractionStrengthMarker = new ProfilerMarker("XRI.ProcessInteractionStrength.Interactors");

	private static readonly ProfilerMarker s_ProcessInteractionStrengthEventMarker = new ProfilerMarker("XRI.ProcessInteractionStrength.InteractorsEvent");

	private Transform m_XROriginTransform;

	private bool m_HasXROrigin;

	private bool m_FailedToFindXROrigin;

	private const string k_InteractionLayerMaskDeprecated = "interactionLayerMask has been deprecated. Use interactionLayers instead.";

	private const string k_EnableInteractionsDeprecated = "enableInteractions has been deprecated. Use allowHover and allowSelect instead.";

	private const string k_OnHoverEnteringDeprecated = "OnHoverEntering(XRBaseInteractable) has been deprecated. Use OnHoverEntering(HoverEnterEventArgs) instead.";

	private const string k_OnHoverEnteredDeprecated = "OnHoverEntered(XRBaseInteractable) has been deprecated. Use OnHoverEntered(HoverEnterEventArgs) instead.";

	private const string k_OnHoverExitingDeprecated = "OnHoverExiting(XRBaseInteractable) has been deprecated. Use OnHoverExiting(HoverExitEventArgs) instead.";

	private const string k_OnHoverExitedDeprecated = "OnHoverExited(XRBaseInteractable) has been deprecated. Use OnHoverExited(HoverExitEventArgs) instead.";

	private const string k_OnSelectEnteringDeprecated = "OnSelectEntering(XRBaseInteractable) has been deprecated. Use OnSelectEntering(SelectEnterEventArgs) instead.";

	private const string k_OnSelectEnteredDeprecated = "OnSelectEntered(XRBaseInteractable) has been deprecated. Use OnSelectEntered(SelectEnterEventArgs) instead.";

	private const string k_OnSelectExitingDeprecated = "OnSelectExiting(XRBaseInteractable) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) instead.";

	private const string k_OnSelectExitedDeprecated = "OnSelectExited(XRBaseInteractable) has been deprecated. Use OnSelectExited(SelectExitEventArgs) instead.";

	private const string k_SelectTargetDeprecated = "selectTarget has been deprecated. Use interactablesSelected, GetOldestInteractableSelected, hasSelection, or IsSelecting for similar functionality.";

	private const string k_HoverTargetsDeprecated = "hoverTargets has been deprecated. Use interactablesHovered instead.";

	private const string k_GetHoverTargetsDeprecated = "GetHoverTargets has been deprecated. Use interactablesHovered instead.";

	private const string k_GetValidTargetsDeprecated = "GetValidTargets(List<XRBaseInteractable>) has been deprecated. Override GetValidTargets(List<IXRInteractable>) instead.";

	private const string k_CanHoverDeprecated = "CanHover(XRBaseInteractable) has been deprecated. Use CanHover(IXRHoverInteractable) instead.";

	private const string k_CanSelectDeprecated = "CanSelect(XRBaseInteractable) has been deprecated. Use CanSelect(IXRSelectInteractable) instead.";

	private const string k_RequireSelectExclusiveDeprecated = "requireSelectExclusive has been deprecated. Put logic in CanSelect instead.";

	private const string k_StartManualInteractionDeprecated = "StartManualInteraction(XRBaseInteractable) has been deprecated. Use StartManualInteraction(IXRSelectInteractable) instead.";

	public XRInteractionManager interactionManager
	{
		get
		{
			return m_InteractionManager;
		}
		set
		{
			m_InteractionManager = value;
			if (Application.isPlaying && base.isActiveAndEnabled)
			{
				RegisterWithInteractionManager();
			}
		}
	}

	public IXRInteractionGroup containingGroup { get; private set; }

	public InteractionLayerMask interactionLayers
	{
		get
		{
			return m_InteractionLayers;
		}
		set
		{
			m_InteractionLayers = value;
		}
	}

	public InteractorHandedness handedness
	{
		get
		{
			return m_Handedness;
		}
		set
		{
			m_Handedness = value;
		}
	}

	public Transform attachTransform
	{
		get
		{
			return m_AttachTransform;
		}
		set
		{
			m_AttachTransform = value;
		}
	}

	public bool keepSelectedTargetValid
	{
		get
		{
			return m_KeepSelectedTargetValid;
		}
		set
		{
			m_KeepSelectedTargetValid = value;
		}
	}

	public bool disableVisualsWhenBlockedInGroup
	{
		get
		{
			return m_DisableVisualsWhenBlockedInGroup;
		}
		set
		{
			m_DisableVisualsWhenBlockedInGroup = value;
		}
	}

	public XRBaseInteractable startingSelectedInteractable
	{
		get
		{
			return m_StartingSelectedInteractable;
		}
		set
		{
			m_StartingSelectedInteractable = value;
		}
	}

	public XRBaseTargetFilter startingTargetFilter
	{
		get
		{
			return m_StartingTargetFilter;
		}
		set
		{
			m_StartingTargetFilter = value;
		}
	}

	public HoverEnterEvent hoverEntered
	{
		get
		{
			return m_HoverEntered;
		}
		set
		{
			m_HoverEntered = value;
		}
	}

	public HoverExitEvent hoverExited
	{
		get
		{
			return m_HoverExited;
		}
		set
		{
			m_HoverExited = value;
		}
	}

	public SelectEnterEvent selectEntered
	{
		get
		{
			return m_SelectEntered;
		}
		set
		{
			m_SelectEntered = value;
		}
	}

	public SelectExitEvent selectExited
	{
		get
		{
			return m_SelectExited;
		}
		set
		{
			m_SelectExited = value;
		}
	}

	public IXRTargetFilter targetFilter
	{
		get
		{
			if (m_TargetFilter is Object @object && @object == null)
			{
				return null;
			}
			return m_TargetFilter;
		}
		set
		{
			if (Application.isPlaying)
			{
				targetFilter?.Unlink(this);
				m_TargetFilter = value;
				targetFilter?.Link(this);
			}
			else
			{
				m_TargetFilter = value;
			}
		}
	}

	public bool allowHover
	{
		get
		{
			return m_AllowHover;
		}
		set
		{
			m_AllowHover = value;
		}
	}

	public bool allowSelect
	{
		get
		{
			return m_AllowSelect;
		}
		set
		{
			m_AllowSelect = value;
		}
	}

	public bool isPerformingManualInteraction => m_IsPerformingManualInteraction;

	public List<IXRHoverInteractable> interactablesHovered => (List<IXRHoverInteractable>)m_InteractablesHovered.AsList();

	public bool hasHover { get; private set; }

	public List<IXRSelectInteractable> interactablesSelected => (List<IXRSelectInteractable>)m_InteractablesSelected.AsList();

	public IXRSelectInteractable firstInteractableSelected { get; private set; }

	public bool hasSelection { get; private set; }

	public List<Object> startingHoverFilters
	{
		get
		{
			return m_StartingHoverFilters;
		}
		set
		{
			m_StartingHoverFilters = value;
		}
	}

	public IXRFilterList<IXRHoverFilter> hoverFilters => m_HoverFilters;

	public List<Object> startingSelectFilters
	{
		get
		{
			return m_StartingSelectFilters;
		}
		set
		{
			m_StartingSelectFilters = value;
		}
	}

	public IXRFilterList<IXRSelectFilter> selectFilters => m_SelectFilters;

	public IReadOnlyBindableVariable<float> largestInteractionStrength => m_LargestInteractionStrength;

	public virtual bool isHoverActive => m_AllowHover;

	public virtual bool isSelectActive => m_AllowSelect;

	public virtual TargetPriorityMode targetPriorityMode { get; set; }

	public virtual List<IXRSelectInteractable> targetsForSelection { get; set; }

	public virtual XRBaseInteractable.MovementType? selectedInteractableMovementTypeOverride => null;

	[Obsolete("interactionLayerMask has been deprecated. Use interactionLayers instead.", true)]
	public LayerMask interactionLayerMask
	{
		get
		{
			Debug.LogError("interactionLayerMask has been deprecated. Use interactionLayers instead.", this);
			throw new NotSupportedException("interactionLayerMask has been deprecated. Use interactionLayers instead.");
		}
		set
		{
			Debug.LogError("interactionLayerMask has been deprecated. Use interactionLayers instead.", this);
			throw new NotSupportedException("interactionLayerMask has been deprecated. Use interactionLayers instead.");
		}
	}

	[Obsolete("enableInteractions has been deprecated. Use allowHover and allowSelect instead.", true)]
	public bool enableInteractions
	{
		get
		{
			Debug.LogError("enableInteractions has been deprecated. Use allowHover and allowSelect instead.", this);
			throw new NotSupportedException("enableInteractions has been deprecated. Use allowHover and allowSelect instead.");
		}
		set
		{
			Debug.LogError("enableInteractions has been deprecated. Use allowHover and allowSelect instead.", this);
			throw new NotSupportedException("enableInteractions has been deprecated. Use allowHover and allowSelect instead.");
		}
	}

	[Obsolete("onHoverEntered has been deprecated. Use hoverEntered with updated signature instead.", true)]
	public XRInteractorEvent onHoverEntered
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onHoverExited has been deprecated. Use hoverExited with updated signature instead.", true)]
	public XRInteractorEvent onHoverExited
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onSelectEntered has been deprecated. Use selectEntered with updated signature instead.", true)]
	public XRInteractorEvent onSelectEntered
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onSelectExited has been deprecated. Use selectExited with updated signature instead.", true)]
	public XRInteractorEvent onSelectExited
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onHoverEnter has been deprecated. Use onHoverEntered instead. (UnityUpgradable) -> onHoverEntered", true)]
	public XRInteractorEvent onHoverEnter => null;

	[Obsolete("onHoverExit has been deprecated. Use onHoverExited instead. (UnityUpgradable) -> onHoverExited", true)]
	public XRInteractorEvent onHoverExit => null;

	[Obsolete("onSelectEnter has been deprecated. Use onSelectEntered instead. (UnityUpgradable) -> onSelectEntered", true)]
	public XRInteractorEvent onSelectEnter => null;

	[Obsolete("onSelectExit has been deprecated. Use onSelectExited instead. (UnityUpgradable) -> onSelectExited", true)]
	public XRInteractorEvent onSelectExit => null;

	[Obsolete("selectTarget has been deprecated. Use interactablesSelected, GetOldestInteractableSelected, hasSelection, or IsSelecting for similar functionality.", true)]
	public XRBaseInteractable selectTarget
	{
		get
		{
			Debug.LogError("selectTarget has been deprecated. Use interactablesSelected, GetOldestInteractableSelected, hasSelection, or IsSelecting for similar functionality.", this);
			throw new NotSupportedException("selectTarget has been deprecated. Use interactablesSelected, GetOldestInteractableSelected, hasSelection, or IsSelecting for similar functionality.");
		}
		protected set
		{
			Debug.LogError("selectTarget has been deprecated. Use interactablesSelected, GetOldestInteractableSelected, hasSelection, or IsSelecting for similar functionality.", this);
			throw new NotSupportedException("selectTarget has been deprecated. Use interactablesSelected, GetOldestInteractableSelected, hasSelection, or IsSelecting for similar functionality.");
		}
	}

	[Obsolete("hoverTargets has been deprecated. Use interactablesHovered instead.", true)]
	protected List<XRBaseInteractable> hoverTargets
	{
		get
		{
			Debug.LogError("hoverTargets has been deprecated. Use interactablesHovered instead.", this);
			throw new NotSupportedException("hoverTargets has been deprecated. Use interactablesHovered instead.");
		}
	}

	[Obsolete("requireSelectExclusive has been deprecated. Put logic in CanSelect instead.", true)]
	public virtual bool requireSelectExclusive
	{
		get
		{
			Debug.LogError("requireSelectExclusive has been deprecated. Put logic in CanSelect instead.", this);
			throw new NotSupportedException("requireSelectExclusive has been deprecated. Put logic in CanSelect instead.");
		}
	}

	public event Action<InteractorRegisteredEventArgs> registered;

	public event Action<InteractorUnregisteredEventArgs> unregistered;

	private protected bool TryGetXROrigin(out Transform origin)
	{
		if (m_HasXROrigin)
		{
			origin = m_XROriginTransform;
			return true;
		}
		if (!m_FailedToFindXROrigin)
		{
			XROrigin componentInParent = GetComponentInParent<XROrigin>();
			if (componentInParent != null)
			{
				GameObject origin2 = componentInParent.Origin;
				if (origin2 != null)
				{
					m_XROriginTransform = origin2.transform;
					m_HasXROrigin = true;
					origin = m_XROriginTransform;
					return true;
				}
			}
			m_FailedToFindXROrigin = true;
		}
		origin = null;
		return false;
	}

	[Conditional("UNITY_EDITOR")]
	protected virtual void Reset()
	{
	}

	protected virtual void Awake()
	{
		CreateAttachTransform();
		if (m_StartingTargetFilter != null)
		{
			targetFilter = m_StartingTargetFilter;
		}
		m_HoverFilters.RegisterReferences(m_StartingHoverFilters, this);
		m_SelectFilters.RegisterReferences(m_StartingSelectFilters, this);
		FindCreateInteractionManager();
	}

	protected virtual void OnEnable()
	{
		FindCreateInteractionManager();
		RegisterWithInteractionManager();
	}

	protected virtual void OnDisable()
	{
		UnregisterWithInteractionManager();
	}

	protected virtual void Start()
	{
		if (m_InteractionManager != null && m_StartingSelectedInteractable != null)
		{
			m_InteractionManager.SelectEnter((IXRSelectInteractor)this, (IXRSelectInteractable)m_StartingSelectedInteractable);
		}
	}

	protected virtual void OnDestroy()
	{
		targetFilter?.Unlink(this);
		if (containingGroup != null && (!(containingGroup is Object @object) || @object != null))
		{
			containingGroup.RemoveGroupMember(this);
		}
	}

	public virtual Transform GetAttachTransform(IXRInteractable interactable)
	{
		if (!(m_AttachTransform != null))
		{
			return base.transform;
		}
		return m_AttachTransform;
	}

	public Pose GetAttachPoseOnSelect(IXRSelectInteractable interactable)
	{
		if (!m_AttachPoseOnSelect.TryGetValue(interactable, out var value))
		{
			return Pose.identity;
		}
		return value;
	}

	public Pose GetLocalAttachPoseOnSelect(IXRSelectInteractable interactable)
	{
		if (!m_LocalAttachPoseOnSelect.TryGetValue(interactable, out var value))
		{
			return Pose.identity;
		}
		return value;
	}

	public virtual void GetValidTargets(List<IXRInteractable> targets)
	{
	}

	private void FindCreateInteractionManager()
	{
		if (!(m_InteractionManager != null))
		{
			m_InteractionManager = ComponentLocatorUtility<XRInteractionManager>.FindOrCreateComponent();
		}
	}

	private void RegisterWithInteractionManager()
	{
		if (!(m_RegisteredInteractionManager == m_InteractionManager))
		{
			UnregisterWithInteractionManager();
			if (m_InteractionManager != null)
			{
				m_InteractionManager.RegisterInteractor((IXRInteractor)this);
				m_RegisteredInteractionManager = m_InteractionManager;
			}
		}
	}

	private void UnregisterWithInteractionManager()
	{
		if (m_RegisteredInteractionManager != null)
		{
			m_RegisteredInteractionManager.UnregisterInteractor((IXRInteractor)this);
			m_RegisteredInteractionManager = null;
		}
	}

	public virtual bool CanHover(IXRHoverInteractable interactable)
	{
		return true;
	}

	public virtual bool CanSelect(IXRSelectInteractable interactable)
	{
		return true;
	}

	public bool IsHovering(IXRHoverInteractable interactable)
	{
		if (hasHover)
		{
			return m_InteractablesHovered.Contains(interactable);
		}
		return false;
	}

	public bool IsSelecting(IXRSelectInteractable interactable)
	{
		if (hasSelection)
		{
			return m_InteractablesSelected.Contains(interactable);
		}
		return false;
	}

	protected bool IsHovering(IXRInteractable interactable)
	{
		if (interactable is IXRHoverInteractable interactable2)
		{
			return IsHovering(interactable2);
		}
		return false;
	}

	protected bool IsSelecting(IXRInteractable interactable)
	{
		if (interactable is IXRSelectInteractable interactable2)
		{
			return IsSelecting(interactable2);
		}
		return false;
	}

	protected void CaptureAttachPose(IXRSelectInteractable interactable)
	{
		Transform transform = GetAttachTransform(interactable);
		if (transform != null)
		{
			m_AttachPoseOnSelect[interactable] = transform.GetWorldPose();
			m_LocalAttachPoseOnSelect[interactable] = transform.GetLocalPose();
		}
		else
		{
			m_AttachPoseOnSelect.Remove(interactable);
			m_LocalAttachPoseOnSelect.Remove(interactable);
		}
	}

	protected void CreateAttachTransform()
	{
		if (m_AttachTransform == null)
		{
			m_AttachTransform = new GameObject("[" + base.gameObject.name + "] Attach").transform;
			m_AttachTransform.SetParent(base.transform, worldPositionStays: false);
			m_AttachTransform.SetLocalPose(Pose.identity);
		}
	}

	public virtual void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
	}

	public virtual void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
	}

	public float GetInteractionStrength(IXRInteractable interactable)
	{
		if (m_InteractionStrengths.TryGetValue(interactable, out var value))
		{
			return value;
		}
		return 0f;
	}

	void IXRInteractionStrengthInteractor.ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		ProcessInteractionStrength(updatePhase);
	}

	void IXRInteractor.OnRegistered(InteractorRegisteredEventArgs args)
	{
		OnRegistered(args);
	}

	void IXRInteractor.OnUnregistered(InteractorUnregisteredEventArgs args)
	{
		OnUnregistered(args);
	}

	bool IXRHoverInteractor.CanHover(IXRHoverInteractable interactable)
	{
		if (CanHover(interactable))
		{
			return ProcessHoverFilters(interactable);
		}
		return false;
	}

	void IXRHoverInteractor.OnHoverEntering(HoverEnterEventArgs args)
	{
		OnHoverEntering(args);
	}

	void IXRHoverInteractor.OnHoverEntered(HoverEnterEventArgs args)
	{
		OnHoverEntered(args);
	}

	void IXRHoverInteractor.OnHoverExiting(HoverExitEventArgs args)
	{
		OnHoverExiting(args);
	}

	void IXRHoverInteractor.OnHoverExited(HoverExitEventArgs args)
	{
		OnHoverExited(args);
	}

	bool IXRSelectInteractor.CanSelect(IXRSelectInteractable interactable)
	{
		if (CanSelect(interactable))
		{
			return ProcessSelectFilters(interactable);
		}
		return false;
	}

	void IXRSelectInteractor.OnSelectEntering(SelectEnterEventArgs args)
	{
		OnSelectEntering(args);
	}

	void IXRSelectInteractor.OnSelectEntered(SelectEnterEventArgs args)
	{
		OnSelectEntered(args);
	}

	void IXRSelectInteractor.OnSelectExiting(SelectExitEventArgs args)
	{
		OnSelectExiting(args);
	}

	void IXRSelectInteractor.OnSelectExited(SelectExitEventArgs args)
	{
		OnSelectExited(args);
	}

	protected virtual void OnRegistered(InteractorRegisteredEventArgs args)
	{
		if (args.manager != m_InteractionManager)
		{
			Debug.LogWarning("An Interactor was registered with an unexpected XRInteractionManager." + $" {this} was expecting to communicate with \"{m_InteractionManager}\" but was registered with \"{args.manager}\".", this);
		}
		this.registered?.Invoke(args);
	}

	protected virtual void OnUnregistered(InteractorUnregisteredEventArgs args)
	{
		if (args.manager != m_RegisteredInteractionManager)
		{
			Debug.LogWarning("An Interactor was unregistered from an unexpected XRInteractionManager." + $" {this} was expecting to communicate with \"{m_RegisteredInteractionManager}\" but was unregistered from \"{args.manager}\".", this);
		}
		this.unregistered?.Invoke(args);
	}

	protected virtual void OnHoverEntering(HoverEnterEventArgs args)
	{
		m_InteractablesHovered.Add(args.interactableObject);
		hasHover = true;
		if (args.interactableObject is IXRInteractionStrengthInteractable item)
		{
			m_InteractionStrengthInteractables.Add(item);
		}
	}

	protected virtual void OnHoverEntered(HoverEnterEventArgs args)
	{
		m_HoverEntered?.Invoke(args);
	}

	protected virtual void OnHoverExiting(HoverExitEventArgs args)
	{
		m_InteractablesHovered.Remove(args.interactableObject);
		if (m_InteractablesHovered.Count == 0)
		{
			hasHover = false;
		}
		if (!IsSelecting(args.interactableObject))
		{
			if (m_InteractionStrengths.Count > 0)
			{
				m_InteractionStrengths.Remove(args.interactableObject);
			}
			if (args.interactableObject is IXRInteractionStrengthInteractable item)
			{
				m_InteractionStrengthInteractables.Remove(item);
			}
		}
	}

	protected virtual void OnHoverExited(HoverExitEventArgs args)
	{
		m_HoverExited?.Invoke(args);
	}

	protected virtual void OnSelectEntering(SelectEnterEventArgs args)
	{
		m_InteractablesSelected.Add(args.interactableObject);
		hasSelection = true;
		if (args.interactableObject is IXRInteractionStrengthInteractable item)
		{
			m_InteractionStrengthInteractables.Add(item);
		}
		if (m_InteractablesSelected.Count == 1)
		{
			firstInteractableSelected = args.interactableObject;
		}
		CaptureAttachPose(args.interactableObject);
	}

	protected virtual void OnSelectEntered(SelectEnterEventArgs args)
	{
		m_SelectEntered?.Invoke(args);
	}

	protected virtual void OnSelectExiting(SelectExitEventArgs args)
	{
		m_InteractablesSelected.Remove(args.interactableObject);
		if (m_InteractablesSelected.Count == 0)
		{
			hasSelection = false;
		}
		if (!IsHovering(args.interactableObject))
		{
			if (m_InteractionStrengths.Count > 0)
			{
				m_InteractionStrengths.Remove(args.interactableObject);
			}
			if (args.interactableObject is IXRInteractionStrengthInteractable item)
			{
				m_InteractionStrengthInteractables.Remove(item);
			}
		}
	}

	protected virtual void OnSelectExited(SelectExitEventArgs args)
	{
		m_SelectExited?.Invoke(args);
		if (!hasSelection)
		{
			firstInteractableSelected = null;
			m_AttachPoseOnSelect.Clear();
			m_LocalAttachPoseOnSelect.Clear();
		}
	}

	protected virtual void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		float num = 0f;
		using (s_ProcessInteractionStrengthMarker.Auto())
		{
			if (!hasSelection && !hasHover)
			{
				if (!m_ClearedLargestInteractionStrength)
				{
					m_LargestInteractionStrength.Value = 0f;
					m_ClearedLargestInteractionStrength = true;
				}
				return;
			}
			m_ClearedLargestInteractionStrength = false;
			if (hasSelection)
			{
				int i = 0;
				for (int count = m_InteractablesSelected.Count; i < count; i++)
				{
					IXRSelectInteractable iXRSelectInteractable = m_InteractablesSelected[i];
					if (!(iXRSelectInteractable is IXRInteractionStrengthInteractable))
					{
						m_InteractionStrengths[iXRSelectInteractable] = 1f;
						num = 1f;
					}
				}
			}
			if (hasHover)
			{
				int j = 0;
				for (int count2 = m_InteractablesHovered.Count; j < count2; j++)
				{
					IXRHoverInteractable iXRHoverInteractable = m_InteractablesHovered[j];
					if (!(iXRHoverInteractable is IXRInteractionStrengthInteractable) && !IsSelecting(iXRHoverInteractable))
					{
						m_InteractionStrengths[iXRHoverInteractable] = 0f;
					}
				}
			}
			int k = 0;
			for (int count3 = m_InteractionStrengthInteractables.Count; k < count3; k++)
			{
				IXRInteractionStrengthInteractable iXRInteractionStrengthInteractable = m_InteractionStrengthInteractables[k];
				float interactionStrength = iXRInteractionStrengthInteractable.GetInteractionStrength(this);
				m_InteractionStrengths[iXRInteractionStrengthInteractable] = interactionStrength;
				num = Mathf.Max(num, interactionStrength);
			}
		}
		using (s_ProcessInteractionStrengthEventMarker.Auto())
		{
			m_LargestInteractionStrength.Value = num;
		}
	}

	public virtual void StartManualInteraction(IXRSelectInteractable interactable)
	{
		if (interactionManager == null)
		{
			Debug.LogWarning("Cannot start manual interaction without an Interaction Manager set.", this);
			return;
		}
		interactionManager.SelectEnter(this, interactable);
		m_IsPerformingManualInteraction = true;
		m_ManualInteractionInteractable = interactable;
	}

	public virtual void EndManualInteraction()
	{
		if (interactionManager == null)
		{
			Debug.LogWarning("Cannot end manual interaction without an Interaction Manager set.", this);
			return;
		}
		if (!m_IsPerformingManualInteraction)
		{
			Debug.LogWarning("Tried to end manual interaction but was not performing manual interaction. Ignoring request.", this);
			return;
		}
		interactionManager.SelectExit(this, m_ManualInteractionInteractable);
		m_IsPerformingManualInteraction = false;
		m_ManualInteractionInteractable = null;
	}

	protected bool ProcessHoverFilters(IXRHoverInteractable interactable)
	{
		return XRFilterUtility.Process(m_HoverFilters, this, interactable);
	}

	protected bool ProcessSelectFilters(IXRSelectInteractable interactable)
	{
		return XRFilterUtility.Process(m_SelectFilters, this, interactable);
	}

	void IXRGroupMember.OnRegisteringAsGroupMember(IXRInteractionGroup group)
	{
		if (containingGroup != null)
		{
			Debug.LogError(base.name + " is already part of a Group. Remove the member from the Group first.", this);
		}
		else if (!group.ContainsGroupMember(this))
		{
			Debug.LogError("OnRegisteringAsGroupMember was called but the Group does not contain " + base.name + ". Add the member to the Group rather than calling this method directly.", this);
		}
		else
		{
			containingGroup = group;
		}
	}

	void IXRGroupMember.OnRegisteringAsNonGroupMember()
	{
		containingGroup = null;
	}

	[Obsolete("OnHoverEntering(XRBaseInteractable) has been deprecated. Use OnHoverEntering(HoverEnterEventArgs) instead.", true)]
	protected virtual void OnHoverEntering(XRBaseInteractable interactable)
	{
		Debug.LogError("OnHoverEntering(XRBaseInteractable) has been deprecated. Use OnHoverEntering(HoverEnterEventArgs) instead.", this);
		throw new NotSupportedException("OnHoverEntering(XRBaseInteractable) has been deprecated. Use OnHoverEntering(HoverEnterEventArgs) instead.");
	}

	[Obsolete("OnHoverEntered(XRBaseInteractable) has been deprecated. Use OnHoverEntered(HoverEnterEventArgs) instead.", true)]
	protected virtual void OnHoverEntered(XRBaseInteractable interactable)
	{
		Debug.LogError("OnHoverEntered(XRBaseInteractable) has been deprecated. Use OnHoverEntered(HoverEnterEventArgs) instead.", this);
		throw new NotSupportedException("OnHoverEntered(XRBaseInteractable) has been deprecated. Use OnHoverEntered(HoverEnterEventArgs) instead.");
	}

	[Obsolete("OnHoverExiting(XRBaseInteractable) has been deprecated. Use OnHoverExiting(HoverExitEventArgs) instead.", true)]
	protected virtual void OnHoverExiting(XRBaseInteractable interactable)
	{
		Debug.LogError("OnHoverExiting(XRBaseInteractable) has been deprecated. Use OnHoverExiting(HoverExitEventArgs) instead.", this);
		throw new NotSupportedException("OnHoverExiting(XRBaseInteractable) has been deprecated. Use OnHoverExiting(HoverExitEventArgs) instead.");
	}

	[Obsolete("OnHoverExited(XRBaseInteractable) has been deprecated. Use OnHoverExited(HoverExitEventArgs) instead.", true)]
	protected virtual void OnHoverExited(XRBaseInteractable interactable)
	{
		Debug.LogError("OnHoverExited(XRBaseInteractable) has been deprecated. Use OnHoverExited(HoverExitEventArgs) instead.", this);
		throw new NotSupportedException("OnHoverExited(XRBaseInteractable) has been deprecated. Use OnHoverExited(HoverExitEventArgs) instead.");
	}

	[Obsolete("OnSelectEntering(XRBaseInteractable) has been deprecated. Use OnSelectEntering(SelectEnterEventArgs) instead.", true)]
	protected virtual void OnSelectEntering(XRBaseInteractable interactable)
	{
		Debug.LogError("OnSelectEntering(XRBaseInteractable) has been deprecated. Use OnSelectEntering(SelectEnterEventArgs) instead.", this);
		throw new NotSupportedException("OnSelectEntering(XRBaseInteractable) has been deprecated. Use OnSelectEntering(SelectEnterEventArgs) instead.");
	}

	[Obsolete("OnSelectEntered(XRBaseInteractable) has been deprecated. Use OnSelectEntered(SelectEnterEventArgs) instead.", true)]
	protected virtual void OnSelectEntered(XRBaseInteractable interactable)
	{
		Debug.LogError("OnSelectEntered(XRBaseInteractable) has been deprecated. Use OnSelectEntered(SelectEnterEventArgs) instead.", this);
		throw new NotSupportedException("OnSelectEntered(XRBaseInteractable) has been deprecated. Use OnSelectEntered(SelectEnterEventArgs) instead.");
	}

	[Obsolete("OnSelectExiting(XRBaseInteractable) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) instead.", true)]
	protected virtual void OnSelectExiting(XRBaseInteractable interactable)
	{
		Debug.LogError("OnSelectExiting(XRBaseInteractable) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) instead.", this);
		throw new NotSupportedException("OnSelectExiting(XRBaseInteractable) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) instead.");
	}

	[Obsolete("OnSelectExited(XRBaseInteractable) has been deprecated. Use OnSelectExited(SelectExitEventArgs) instead.", true)]
	protected virtual void OnSelectExited(XRBaseInteractable interactable)
	{
		Debug.LogError("OnSelectExited(XRBaseInteractable) has been deprecated. Use OnSelectExited(SelectExitEventArgs) instead.", this);
		throw new NotSupportedException("OnSelectExited(XRBaseInteractable) has been deprecated. Use OnSelectExited(SelectExitEventArgs) instead.");
	}

	[Obsolete("GetHoverTargets has been deprecated. Use interactablesHovered instead.", true)]
	public void GetHoverTargets(List<XRBaseInteractable> targets)
	{
		Debug.LogError("GetHoverTargets has been deprecated. Use interactablesHovered instead.", this);
		throw new NotSupportedException("GetHoverTargets has been deprecated. Use interactablesHovered instead.");
	}

	[Obsolete("GetValidTargets(List<XRBaseInteractable>) has been deprecated. Override GetValidTargets(List<IXRInteractable>) instead.", true)]
	public virtual void GetValidTargets(List<XRBaseInteractable> targets)
	{
		Debug.LogError("GetValidTargets(List<XRBaseInteractable>) has been deprecated. Override GetValidTargets(List<IXRInteractable>) instead.", this);
		throw new NotSupportedException("GetValidTargets(List<XRBaseInteractable>) has been deprecated. Override GetValidTargets(List<IXRInteractable>) instead.");
	}

	[Obsolete("CanHover(XRBaseInteractable) has been deprecated. Use CanHover(IXRHoverInteractable) instead.", true)]
	public virtual bool CanHover(XRBaseInteractable interactable)
	{
		Debug.LogError("CanHover(XRBaseInteractable) has been deprecated. Use CanHover(IXRHoverInteractable) instead.", this);
		throw new NotSupportedException("CanHover(XRBaseInteractable) has been deprecated. Use CanHover(IXRHoverInteractable) instead.");
	}

	[Obsolete("CanSelect(XRBaseInteractable) has been deprecated. Use CanSelect(IXRSelectInteractable) instead.", true)]
	public virtual bool CanSelect(XRBaseInteractable interactable)
	{
		Debug.LogError("CanSelect(XRBaseInteractable) has been deprecated. Use CanSelect(IXRSelectInteractable) instead.", this);
		throw new NotSupportedException("CanSelect(XRBaseInteractable) has been deprecated. Use CanSelect(IXRSelectInteractable) instead.");
	}

	[Obsolete("StartManualInteraction(XRBaseInteractable) has been deprecated. Use StartManualInteraction(IXRSelectInteractable) instead.", true)]
	public virtual void StartManualInteraction(XRBaseInteractable interactable)
	{
		Debug.LogError("StartManualInteraction(XRBaseInteractable) has been deprecated. Use StartManualInteraction(IXRSelectInteractable) instead.", this);
		throw new NotSupportedException("StartManualInteraction(XRBaseInteractable) has been deprecated. Use StartManualInteraction(IXRSelectInteractable) instead.");
	}

	Transform IXRInteractor.get_transform()
	{
		return base.transform;
	}
}
