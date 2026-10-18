using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using Unity.XR.CoreUtils.Collections;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Gaze;
using UnityEngine.XR.Interaction.Toolkit.Interactables.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[SelectionBase]
[DefaultExecutionOrder(-98)]
public abstract class XRBaseInteractable : MonoBehaviour, IXRActivateInteractable, IXRInteractable, IXRHoverInteractable, IXRSelectInteractable, IXRFocusInteractable, IXRInteractionStrengthInteractable, IXROverridesGazeAutoSelect
{
	public enum MovementType
	{
		VelocityTracking,
		Kinematic,
		Instantaneous
	}

	public enum DistanceCalculationMode
	{
		TransformPosition,
		ColliderPosition,
		ColliderVolume
	}

	private const float k_InteractionStrengthHover = 0f;

	private const float k_InteractionStrengthSelect = 1f;

	[SerializeField]
	private XRInteractionManager m_InteractionManager;

	[SerializeField]
	private List<Collider> m_Colliders = new List<Collider>();

	[SerializeField]
	private InteractionLayerMask m_InteractionLayers = 1;

	[SerializeField]
	private DistanceCalculationMode m_DistanceCalculationMode = DistanceCalculationMode.ColliderPosition;

	[SerializeField]
	private InteractableSelectMode m_SelectMode;

	[SerializeField]
	private InteractableFocusMode m_FocusMode = InteractableFocusMode.Single;

	[SerializeField]
	private GameObject m_CustomReticle;

	[SerializeField]
	private bool m_AllowGazeInteraction;

	[SerializeField]
	private bool m_AllowGazeSelect;

	[SerializeField]
	private bool m_OverrideGazeTimeToSelect;

	[SerializeField]
	private float m_GazeTimeToSelect = 0.5f;

	[SerializeField]
	private bool m_OverrideTimeToAutoDeselectGaze;

	[SerializeField]
	private float m_TimeToAutoDeselectGaze = 3f;

	[SerializeField]
	private bool m_AllowGazeAssistance;

	[SerializeField]
	private HoverEnterEvent m_FirstHoverEntered = new HoverEnterEvent();

	[SerializeField]
	private HoverExitEvent m_LastHoverExited = new HoverExitEvent();

	[SerializeField]
	private HoverEnterEvent m_HoverEntered = new HoverEnterEvent();

	[SerializeField]
	private HoverExitEvent m_HoverExited = new HoverExitEvent();

	[SerializeField]
	private SelectEnterEvent m_FirstSelectEntered = new SelectEnterEvent();

	[SerializeField]
	private SelectExitEvent m_LastSelectExited = new SelectExitEvent();

	[SerializeField]
	private SelectEnterEvent m_SelectEntered = new SelectEnterEvent();

	[SerializeField]
	private SelectExitEvent m_SelectExited = new SelectExitEvent();

	[SerializeField]
	private FocusEnterEvent m_FirstFocusEntered = new FocusEnterEvent();

	[SerializeField]
	private FocusExitEvent m_LastFocusExited = new FocusExitEvent();

	[SerializeField]
	private FocusEnterEvent m_FocusEntered = new FocusEnterEvent();

	[SerializeField]
	private FocusExitEvent m_FocusExited = new FocusExitEvent();

	[SerializeField]
	private ActivateEvent m_Activated = new ActivateEvent();

	[SerializeField]
	private DeactivateEvent m_Deactivated = new DeactivateEvent();

	private readonly HashSetList<IXRHoverInteractor> m_InteractorsHovering = new HashSetList<IXRHoverInteractor>();

	private readonly HashSetList<IXRSelectInteractor> m_InteractorsSelecting = new HashSetList<IXRSelectInteractor>();

	private readonly HashSetList<IXRInteractionGroup> m_InteractionGroupsFocusing = new HashSetList<IXRInteractionGroup>();

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

	[SerializeField]
	[RequireInterface(typeof(IXRInteractionStrengthFilter))]
	private List<Object> m_StartingInteractionStrengthFilters = new List<Object>();

	private readonly ExposedRegistrationList<IXRInteractionStrengthFilter> m_InteractionStrengthFilters = new ExposedRegistrationList<IXRInteractionStrengthFilter>
	{
		bufferChanges = false
	};

	private readonly BindableVariable<float> m_LargestInteractionStrength = new BindableVariable<float>(0f);

	private bool m_ClearedLargestInteractionStrength;

	private readonly Dictionary<IXRSelectInteractor, Pose> m_AttachPoseOnSelect = new Dictionary<IXRSelectInteractor, Pose>();

	private readonly Dictionary<IXRSelectInteractor, Pose> m_LocalAttachPoseOnSelect = new Dictionary<IXRSelectInteractor, Pose>();

	private readonly Dictionary<IXRInteractor, GameObject> m_ReticleCache = new Dictionary<IXRInteractor, GameObject>();

	private readonly HashSetList<XRBaseInputInteractor> m_VariableSelectInteractors = new HashSetList<XRBaseInputInteractor>();

	private readonly Dictionary<IXRInteractor, float> m_InteractionStrengths = new Dictionary<IXRInteractor, float>();

	private XRInteractionManager m_RegisteredInteractionManager;

	private static readonly ProfilerMarker s_ProcessInteractionStrengthMarker = new ProfilerMarker("XRI.ProcessInteractionStrength.Interactables");

	private static readonly ProfilerMarker s_ProcessInteractionStrengthEventMarker = new ProfilerMarker("XRI.ProcessInteractionStrength.InteractablesEvent");

	private const string k_InteractionLayerMaskDeprecated = "interactionLayerMask has been deprecated. Use interactionLayers instead.";

	private const string k_OnHoverEnteringDeprecated = "OnHoverEntering(XRBaseInteractor) has been deprecated. Use OnHoverEntering(HoverEnterEventArgs) instead.";

	private const string k_OnHoverEnteredDeprecated = "OnHoverEntered(XRBaseInteractor) has been deprecated. Use OnHoverEntered(HoverEnterEventArgs) instead.";

	private const string k_OnHoverExitingDeprecated = "OnHoverExiting(XRBaseInteractor) has been deprecated. Use OnHoverExiting(HoverExitEventArgs) instead.";

	private const string k_OnHoverExitedDeprecated = "OnHoverExited(XRBaseInteractor) has been deprecated. Use OnHoverExited(HoverExitEventArgs) instead.";

	private const string k_OnSelectEnteringDeprecated = "OnSelectEntering(XRBaseInteractor) has been deprecated. Use OnSelectEntering(SelectEnterEventArgs) instead.";

	private const string k_OnSelectEnteredDeprecated = "OnSelectEntered(XRBaseInteractor) has been deprecated. Use OnSelectEntered(SelectEnterEventArgs) instead.";

	private const string k_OnSelectExitingDeprecated = "OnSelectExiting(XRBaseInteractor) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) and check for !args.isCanceled instead.";

	private const string k_OnSelectExitedDeprecated = "OnSelectExited(XRBaseInteractor) has been deprecated. Use OnSelectExited(SelectExitEventArgs) and check for !args.isCanceled instead.";

	private const string k_OnSelectCancelingDeprecated = "OnSelectCanceling(XRBaseInteractor) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) and check for args.isCanceled instead.";

	private const string k_OnSelectCanceledDeprecated = "OnSelectCanceled(XRBaseInteractor) has been deprecated. Use OnSelectExited(SelectExitEventArgs) and check for args.isCanceled instead.";

	private const string k_OnActivateDeprecated = "OnActivate(XRBaseInteractor) has been deprecated. Use OnActivated(ActivateEventArgs) instead.";

	private const string k_OnDeactivateDeprecated = "OnDeactivate(XRBaseInteractor) has been deprecated. Use OnDeactivated(DeactivateEventArgs) instead.";

	private const string k_GetDistanceSqrToInteractorDeprecated = "GetDistanceSqrToInteractor(XRBaseInteractor) has been deprecated. Use GetDistanceSqrToInteractor(IXRInteractor) instead.";

	private const string k_AttachCustomReticleDeprecated = "AttachCustomReticle(XRBaseInteractor) has been deprecated. Use AttachCustomReticle(IXRInteractor) instead.";

	private const string k_RemoveCustomReticleDeprecated = "RemoveCustomReticle(XRBaseInteractor) has been deprecated. Use RemoveCustomReticle(IXRInteractor) instead.";

	private const string k_HoveringInteractorsDeprecated = "hoveringInteractors has been deprecated. Use interactorsHovering instead.";

	private const string k_SelectingInteractorDeprecated = "selectingInteractor has been deprecated. Use interactorsSelecting, GetOldestInteractorSelecting, or isSelected for similar functionality.";

	private const string k_IsHoverableByDeprecated = "IsHoverableBy(XRBaseInteractor) has been deprecated. Use IsHoverableBy(IXRHoverInteractor) instead.";

	private const string k_IsSelectableByDeprecated = "IsSelectableBy(XRBaseInteractor) has been deprecated. Use IsSelectableBy(IXRSelectInteractor) instead.";

	public Func<IXRInteractable, Vector3, DistanceInfo> getDistanceOverride { get; set; }

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

	public List<Collider> colliders => m_Colliders;

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

	public DistanceCalculationMode distanceCalculationMode
	{
		get
		{
			return m_DistanceCalculationMode;
		}
		set
		{
			m_DistanceCalculationMode = value;
		}
	}

	public InteractableSelectMode selectMode
	{
		get
		{
			return m_SelectMode;
		}
		set
		{
			m_SelectMode = value;
		}
	}

	public InteractableFocusMode focusMode
	{
		get
		{
			return m_FocusMode;
		}
		set
		{
			m_FocusMode = value;
		}
	}

	public GameObject customReticle
	{
		get
		{
			return m_CustomReticle;
		}
		set
		{
			m_CustomReticle = value;
		}
	}

	public bool allowGazeInteraction
	{
		get
		{
			return m_AllowGazeInteraction;
		}
		set
		{
			m_AllowGazeInteraction = value;
		}
	}

	public bool allowGazeSelect
	{
		get
		{
			return m_AllowGazeSelect;
		}
		set
		{
			m_AllowGazeSelect = value;
		}
	}

	public bool overrideGazeTimeToSelect
	{
		get
		{
			return m_OverrideGazeTimeToSelect;
		}
		set
		{
			m_OverrideGazeTimeToSelect = value;
		}
	}

	public float gazeTimeToSelect
	{
		get
		{
			return m_GazeTimeToSelect;
		}
		set
		{
			m_GazeTimeToSelect = value;
		}
	}

	public bool overrideTimeToAutoDeselectGaze
	{
		get
		{
			return m_OverrideTimeToAutoDeselectGaze;
		}
		set
		{
			m_OverrideTimeToAutoDeselectGaze = value;
		}
	}

	public float timeToAutoDeselectGaze
	{
		get
		{
			return m_TimeToAutoDeselectGaze;
		}
		set
		{
			m_TimeToAutoDeselectGaze = value;
		}
	}

	public bool allowGazeAssistance
	{
		get
		{
			return m_AllowGazeAssistance;
		}
		set
		{
			m_AllowGazeAssistance = value;
		}
	}

	public HoverEnterEvent firstHoverEntered
	{
		get
		{
			return m_FirstHoverEntered;
		}
		set
		{
			m_FirstHoverEntered = value;
		}
	}

	public HoverExitEvent lastHoverExited
	{
		get
		{
			return m_LastHoverExited;
		}
		set
		{
			m_LastHoverExited = value;
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

	public SelectEnterEvent firstSelectEntered
	{
		get
		{
			return m_FirstSelectEntered;
		}
		set
		{
			m_FirstSelectEntered = value;
		}
	}

	public SelectExitEvent lastSelectExited
	{
		get
		{
			return m_LastSelectExited;
		}
		set
		{
			m_LastSelectExited = value;
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

	public FocusEnterEvent firstFocusEntered
	{
		get
		{
			return m_FirstFocusEntered;
		}
		set
		{
			m_FirstFocusEntered = value;
		}
	}

	public FocusExitEvent lastFocusExited
	{
		get
		{
			return m_LastFocusExited;
		}
		set
		{
			m_LastFocusExited = value;
		}
	}

	public FocusEnterEvent focusEntered
	{
		get
		{
			return m_FocusEntered;
		}
		set
		{
			m_FocusEntered = value;
		}
	}

	public FocusExitEvent focusExited
	{
		get
		{
			return m_FocusExited;
		}
		set
		{
			m_FocusExited = value;
		}
	}

	public ActivateEvent activated
	{
		get
		{
			return m_Activated;
		}
		set
		{
			m_Activated = value;
		}
	}

	public DeactivateEvent deactivated
	{
		get
		{
			return m_Deactivated;
		}
		set
		{
			m_Deactivated = value;
		}
	}

	public List<IXRHoverInteractor> interactorsHovering => (List<IXRHoverInteractor>)m_InteractorsHovering.AsList();

	public bool isHovered { get; private set; }

	public List<IXRSelectInteractor> interactorsSelecting => (List<IXRSelectInteractor>)m_InteractorsSelecting.AsList();

	public IXRSelectInteractor firstInteractorSelecting { get; private set; }

	public bool isSelected { get; private set; }

	public List<IXRInteractionGroup> interactionGroupsFocusing => (List<IXRInteractionGroup>)m_InteractionGroupsFocusing.AsList();

	public IXRInteractionGroup firstInteractionGroupFocusing { get; private set; }

	public bool isFocused { get; private set; }

	public bool canFocus => m_FocusMode != InteractableFocusMode.None;

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

	public List<Object> startingInteractionStrengthFilters
	{
		get
		{
			return m_StartingInteractionStrengthFilters;
		}
		set
		{
			m_StartingInteractionStrengthFilters = value;
		}
	}

	public IXRFilterList<IXRInteractionStrengthFilter> interactionStrengthFilters => m_InteractionStrengthFilters;

	public IReadOnlyBindableVariable<float> largestInteractionStrength => m_LargestInteractionStrength;

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

	[Obsolete("onFirstHoverEntered has been deprecated. Use firstHoverEntered with updated signature instead.", true)]
	public XRInteractableEvent onFirstHoverEntered
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onLastHoverExited has been deprecated. Use lastHoverExited with updated signature instead.", true)]
	public XRInteractableEvent onLastHoverExited
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onHoverEntered has been deprecated. Use hoverEntered with updated signature instead.", true)]
	public XRInteractableEvent onHoverEntered
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
	public XRInteractableEvent onHoverExited
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
	public XRInteractableEvent onSelectEntered
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onSelectExited has been deprecated. Use selectExited with updated signature and check for !args.isCanceled instead.", true)]
	public XRInteractableEvent onSelectExited
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onSelectCanceled has been deprecated. Use selectExited with updated signature and check for args.isCanceled instead.", true)]
	public XRInteractableEvent onSelectCanceled
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onActivate has been deprecated. Use activated with updated signature instead.", true)]
	public XRInteractableEvent onActivate
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onDeactivate has been deprecated. Use deactivated with updated signature instead.", true)]
	public XRInteractableEvent onDeactivate
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	[Obsolete("onFirstHoverEnter has been deprecated. Use onFirstHoverEntered instead. (UnityUpgradable) -> onFirstHoverEntered", true)]
	public XRInteractableEvent onFirstHoverEnter => null;

	[Obsolete("onHoverEnter has been deprecated. Use onHoverEntered instead. (UnityUpgradable) -> onHoverEntered", true)]
	public XRInteractableEvent onHoverEnter => null;

	[Obsolete("onHoverExit has been deprecated. Use onHoverExited instead. (UnityUpgradable) -> onHoverExited", true)]
	public XRInteractableEvent onHoverExit => null;

	[Obsolete("onLastHoverExit has been deprecated. Use onLastHoverExited instead. (UnityUpgradable) -> onLastHoverExited", true)]
	public XRInteractableEvent onLastHoverExit => null;

	[Obsolete("onSelectEnter has been deprecated. Use onSelectEntered instead. (UnityUpgradable) -> onSelectEntered", true)]
	public XRInteractableEvent onSelectEnter => null;

	[Obsolete("onSelectExit has been deprecated. Use onSelectExited instead. (UnityUpgradable) -> onSelectExited", true)]
	public XRInteractableEvent onSelectExit => null;

	[Obsolete("onSelectCancel has been deprecated. Use onSelectCanceled instead. (UnityUpgradable) -> onSelectCanceled", true)]
	public XRInteractableEvent onSelectCancel => null;

	[Obsolete("hoveringInteractors has been deprecated. Use interactorsHovering instead.", true)]
	public List<XRBaseInteractor> hoveringInteractors
	{
		get
		{
			Debug.LogError("hoveringInteractors has been deprecated. Use interactorsHovering instead.", this);
			throw new NotSupportedException("hoveringInteractors has been deprecated. Use interactorsHovering instead.");
		}
	}

	[Obsolete("selectingInteractor has been deprecated. Use interactorsSelecting, GetOldestInteractorSelecting, or isSelected for similar functionality.", true)]
	public XRBaseInteractor selectingInteractor
	{
		get
		{
			Debug.LogError("selectingInteractor has been deprecated. Use interactorsSelecting, GetOldestInteractorSelecting, or isSelected for similar functionality.", this);
			throw new NotSupportedException("selectingInteractor has been deprecated. Use interactorsSelecting, GetOldestInteractorSelecting, or isSelected for similar functionality.");
		}
		protected set
		{
			Debug.LogError("selectingInteractor has been deprecated. Use interactorsSelecting, GetOldestInteractorSelecting, or isSelected for similar functionality.", this);
			throw new NotSupportedException("selectingInteractor has been deprecated. Use interactorsSelecting, GetOldestInteractorSelecting, or isSelected for similar functionality.");
		}
	}

	public event Action<InteractableRegisteredEventArgs> registered;

	public event Action<InteractableUnregisteredEventArgs> unregistered;

	[Conditional("UNITY_EDITOR")]
	protected virtual void Reset()
	{
	}

	protected virtual void Awake()
	{
		if (m_Colliders.Count == 0)
		{
			GetComponentsInChildren(m_Colliders);
			m_Colliders.RemoveAll((Collider col) => col.isTrigger);
		}
		m_HoverFilters.RegisterReferences(m_StartingHoverFilters, this);
		m_SelectFilters.RegisterReferences(m_StartingSelectFilters, this);
		m_InteractionStrengthFilters.RegisterReferences(m_StartingInteractionStrengthFilters, this);
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

	protected virtual void OnDestroy()
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
				m_InteractionManager.RegisterInteractable((IXRInteractable)this);
				m_RegisteredInteractionManager = m_InteractionManager;
			}
		}
	}

	private void UnregisterWithInteractionManager()
	{
		if (m_RegisteredInteractionManager != null)
		{
			m_RegisteredInteractionManager.UnregisterInteractable((IXRInteractable)this);
			m_RegisteredInteractionManager = null;
		}
	}

	public virtual Transform GetAttachTransform(IXRInteractor interactor)
	{
		return base.transform;
	}

	public Pose GetAttachPoseOnSelect(IXRSelectInteractor interactor)
	{
		if (!m_AttachPoseOnSelect.TryGetValue(interactor, out var value))
		{
			return Pose.identity;
		}
		return value;
	}

	public Pose GetLocalAttachPoseOnSelect(IXRSelectInteractor interactor)
	{
		if (!m_LocalAttachPoseOnSelect.TryGetValue(interactor, out var value))
		{
			return Pose.identity;
		}
		return value;
	}

	public virtual float GetDistanceSqrToInteractor(IXRInteractor interactor)
	{
		Transform transform = interactor?.GetAttachTransform(this);
		if (transform == null)
		{
			return float.MaxValue;
		}
		Vector3 position = transform.position;
		return GetDistance(position).distanceSqr;
	}

	public virtual DistanceInfo GetDistance(Vector3 position)
	{
		if (getDistanceOverride != null)
		{
			return getDistanceOverride(this, position);
		}
		DistanceInfo distanceInfo;
		switch (m_DistanceCalculationMode)
		{
		default:
		{
			Vector3 position2 = base.transform.position;
			Vector3 vector = position2 - position;
			DistanceInfo result = default(DistanceInfo);
			result.point = position2;
			result.distanceSqr = vector.sqrMagnitude;
			return result;
		}
		case DistanceCalculationMode.ColliderPosition:
			XRInteractableUtility.TryGetClosestCollider(this, position, out distanceInfo);
			return distanceInfo;
		case DistanceCalculationMode.ColliderVolume:
			XRInteractableUtility.TryGetClosestPointOnCollider(this, position, out distanceInfo);
			return distanceInfo;
		}
	}

	public float GetInteractionStrength(IXRInteractor interactor)
	{
		if (m_InteractionStrengths.TryGetValue(interactor, out var value))
		{
			return value;
		}
		return 0f;
	}

	public virtual bool IsHoverableBy(IXRHoverInteractor interactor)
	{
		if (!m_AllowGazeInteraction)
		{
			return !(interactor is XRGazeInteractor);
		}
		return true;
	}

	public virtual bool IsSelectableBy(IXRSelectInteractor interactor)
	{
		if (!m_AllowGazeInteraction || !m_AllowGazeSelect)
		{
			return !(interactor is XRGazeInteractor);
		}
		return true;
	}

	public bool IsHovered(IXRHoverInteractor interactor)
	{
		if (isHovered)
		{
			return m_InteractorsHovering.Contains(interactor);
		}
		return false;
	}

	public bool IsSelected(IXRSelectInteractor interactor)
	{
		if (isSelected)
		{
			return m_InteractorsSelecting.Contains(interactor);
		}
		return false;
	}

	protected bool IsHovered(IXRInteractor interactor)
	{
		if (interactor is IXRHoverInteractor interactor2)
		{
			return IsHovered(interactor2);
		}
		return false;
	}

	protected bool IsSelected(IXRInteractor interactor)
	{
		if (interactor is IXRSelectInteractor interactor2)
		{
			return IsSelected(interactor2);
		}
		return false;
	}

	public virtual GameObject GetCustomReticle(IXRInteractor interactor)
	{
		if (m_ReticleCache.TryGetValue(interactor, out var value))
		{
			return value;
		}
		return null;
	}

	public virtual void AttachCustomReticle(IXRInteractor interactor)
	{
		Transform transform = interactor?.transform;
		if (transform == null)
		{
			return;
		}
		IXRCustomReticleProvider component = transform.GetComponent<IXRCustomReticleProvider>();
		if (component != null)
		{
			if (m_ReticleCache.TryGetValue(interactor, out var value))
			{
				Object.Destroy(value);
				m_ReticleCache.Remove(interactor);
			}
			if (m_CustomReticle != null)
			{
				GameObject gameObject = Object.Instantiate(m_CustomReticle);
				m_ReticleCache.Add(interactor, gameObject);
				component.AttachCustomReticle(gameObject);
				gameObject.GetComponent<IXRInteractableCustomReticle>()?.OnReticleAttached(this, component);
			}
		}
	}

	public virtual void RemoveCustomReticle(IXRInteractor interactor)
	{
		Transform transform = interactor?.transform;
		if (!(transform == null))
		{
			IXRCustomReticleProvider component = transform.GetComponent<IXRCustomReticleProvider>();
			if (component != null && m_ReticleCache.TryGetValue(interactor, out var value))
			{
				value.GetComponent<IXRInteractableCustomReticle>()?.OnReticleDetaching();
				Object.Destroy(value);
				m_ReticleCache.Remove(interactor);
				component.RemoveCustomReticle();
			}
		}
	}

	protected void CaptureAttachPose(IXRSelectInteractor interactor)
	{
		Transform attachTransform = GetAttachTransform(interactor);
		if (attachTransform != null)
		{
			m_AttachPoseOnSelect[interactor] = attachTransform.GetWorldPose();
			m_LocalAttachPoseOnSelect[interactor] = attachTransform.GetLocalPose();
		}
		else
		{
			m_AttachPoseOnSelect.Remove(interactor);
			m_LocalAttachPoseOnSelect.Remove(interactor);
		}
	}

	public virtual void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
	}

	void IXRInteractionStrengthInteractable.ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		ProcessInteractionStrength(updatePhase);
	}

	void IXRInteractable.OnRegistered(InteractableRegisteredEventArgs args)
	{
		OnRegistered(args);
	}

	void IXRInteractable.OnUnregistered(InteractableUnregisteredEventArgs args)
	{
		OnUnregistered(args);
	}

	void IXRActivateInteractable.OnActivated(ActivateEventArgs args)
	{
		OnActivated(args);
	}

	void IXRActivateInteractable.OnDeactivated(DeactivateEventArgs args)
	{
		OnDeactivated(args);
	}

	bool IXRHoverInteractable.IsHoverableBy(IXRHoverInteractor interactor)
	{
		if (IsHoverableBy(interactor))
		{
			return ProcessHoverFilters(interactor);
		}
		return false;
	}

	void IXRHoverInteractable.OnHoverEntering(HoverEnterEventArgs args)
	{
		OnHoverEntering(args);
	}

	void IXRHoverInteractable.OnHoverEntered(HoverEnterEventArgs args)
	{
		OnHoverEntered(args);
	}

	void IXRHoverInteractable.OnHoverExiting(HoverExitEventArgs args)
	{
		OnHoverExiting(args);
	}

	void IXRHoverInteractable.OnHoverExited(HoverExitEventArgs args)
	{
		OnHoverExited(args);
	}

	bool IXRSelectInteractable.IsSelectableBy(IXRSelectInteractor interactor)
	{
		if (IsSelectableBy(interactor))
		{
			return ProcessSelectFilters(interactor);
		}
		return false;
	}

	void IXRSelectInteractable.OnSelectEntering(SelectEnterEventArgs args)
	{
		OnSelectEntering(args);
	}

	void IXRSelectInteractable.OnSelectEntered(SelectEnterEventArgs args)
	{
		OnSelectEntered(args);
	}

	void IXRSelectInteractable.OnSelectExiting(SelectExitEventArgs args)
	{
		OnSelectExiting(args);
	}

	void IXRSelectInteractable.OnSelectExited(SelectExitEventArgs args)
	{
		OnSelectExited(args);
	}

	void IXRFocusInteractable.OnFocusEntering(FocusEnterEventArgs args)
	{
		OnFocusEntering(args);
	}

	void IXRFocusInteractable.OnFocusEntered(FocusEnterEventArgs args)
	{
		OnFocusEntered(args);
	}

	void IXRFocusInteractable.OnFocusExiting(FocusExitEventArgs args)
	{
		OnFocusExiting(args);
	}

	void IXRFocusInteractable.OnFocusExited(FocusExitEventArgs args)
	{
		OnFocusExited(args);
	}

	protected virtual void OnRegistered(InteractableRegisteredEventArgs args)
	{
		if (args.manager != m_InteractionManager)
		{
			Debug.LogWarning("An Interactable was registered with an unexpected XRInteractionManager." + $" {this} was expecting to communicate with \"{m_InteractionManager}\" but was registered with \"{args.manager}\".", this);
		}
		this.registered?.Invoke(args);
	}

	protected virtual void OnUnregistered(InteractableUnregisteredEventArgs args)
	{
		if (args.manager != m_RegisteredInteractionManager)
		{
			Debug.LogWarning("An Interactable was unregistered from an unexpected XRInteractionManager." + $" {this} was expecting to communicate with \"{m_RegisteredInteractionManager}\" but was unregistered from \"{args.manager}\".", this);
		}
		this.unregistered?.Invoke(args);
	}

	protected virtual void OnHoverEntering(HoverEnterEventArgs args)
	{
		if (m_CustomReticle != null)
		{
			AttachCustomReticle(args.interactorObject);
		}
		m_InteractorsHovering.Add(args.interactorObject);
		isHovered = true;
		if (args.interactorObject is XRBaseInputInteractor item)
		{
			m_VariableSelectInteractors.Add(item);
		}
	}

	protected virtual void OnHoverEntered(HoverEnterEventArgs args)
	{
		if (m_InteractorsHovering.Count == 1)
		{
			m_FirstHoverEntered?.Invoke(args);
		}
		m_HoverEntered?.Invoke(args);
	}

	protected virtual void OnHoverExiting(HoverExitEventArgs args)
	{
		if (m_CustomReticle != null)
		{
			RemoveCustomReticle(args.interactorObject);
		}
		m_InteractorsHovering.Remove(args.interactorObject);
		if (m_InteractorsHovering.Count == 0)
		{
			isHovered = false;
		}
		if (!IsSelected(args.interactorObject))
		{
			if (m_InteractionStrengths.Count > 0)
			{
				m_InteractionStrengths.Remove(args.interactorObject);
			}
			if (args.interactorObject is XRBaseInputInteractor item)
			{
				m_VariableSelectInteractors.Remove(item);
			}
		}
	}

	protected virtual void OnHoverExited(HoverExitEventArgs args)
	{
		if (!isHovered)
		{
			m_LastHoverExited?.Invoke(args);
		}
		m_HoverExited?.Invoke(args);
	}

	protected virtual void OnSelectEntering(SelectEnterEventArgs args)
	{
		m_InteractorsSelecting.Add(args.interactorObject);
		isSelected = true;
		if (args.interactorObject is XRBaseInputInteractor item)
		{
			m_VariableSelectInteractors.Add(item);
		}
		if (m_InteractorsSelecting.Count == 1)
		{
			firstInteractorSelecting = args.interactorObject;
		}
		CaptureAttachPose(args.interactorObject);
	}

	protected virtual void OnSelectEntered(SelectEnterEventArgs args)
	{
		if (m_InteractorsSelecting.Count == 1)
		{
			m_FirstSelectEntered?.Invoke(args);
		}
		m_SelectEntered?.Invoke(args);
	}

	protected virtual void OnSelectExiting(SelectExitEventArgs args)
	{
		m_InteractorsSelecting.Remove(args.interactorObject);
		if (m_InteractorsSelecting.Count == 0)
		{
			isSelected = false;
		}
		if (!IsHovered(args.interactorObject))
		{
			if (m_InteractionStrengths.Count > 0)
			{
				m_InteractionStrengths.Remove(args.interactorObject);
			}
			if (args.interactorObject is XRBaseInputInteractor item)
			{
				m_VariableSelectInteractors.Remove(item);
			}
		}
	}

	protected virtual void OnSelectExited(SelectExitEventArgs args)
	{
		if (!isSelected)
		{
			m_LastSelectExited?.Invoke(args);
		}
		m_SelectExited?.Invoke(args);
		if (!isSelected)
		{
			firstInteractorSelecting = null;
			m_AttachPoseOnSelect.Clear();
			m_LocalAttachPoseOnSelect.Clear();
		}
	}

	protected virtual void OnFocusEntering(FocusEnterEventArgs args)
	{
		m_InteractionGroupsFocusing.Add(args.interactionGroup);
		isFocused = true;
		if (m_InteractionGroupsFocusing.Count == 1)
		{
			firstInteractionGroupFocusing = args.interactionGroup;
		}
	}

	protected virtual void OnFocusEntered(FocusEnterEventArgs args)
	{
		if (m_InteractionGroupsFocusing.Count == 1)
		{
			m_FirstFocusEntered?.Invoke(args);
		}
		m_FocusEntered?.Invoke(args);
	}

	protected virtual void OnFocusExiting(FocusExitEventArgs args)
	{
		m_InteractionGroupsFocusing.Remove(args.interactionGroup);
		if (m_InteractionGroupsFocusing.Count == 0)
		{
			isFocused = false;
		}
	}

	protected virtual void OnFocusExited(FocusExitEventArgs args)
	{
		if (!isFocused)
		{
			m_LastFocusExited?.Invoke(args);
		}
		m_FocusExited?.Invoke(args);
		if (!isFocused)
		{
			firstInteractionGroupFocusing = null;
		}
	}

	protected virtual void OnActivated(ActivateEventArgs args)
	{
		m_Activated?.Invoke(args);
	}

	protected virtual void OnDeactivated(DeactivateEventArgs args)
	{
		m_Deactivated?.Invoke(args);
	}

	protected virtual void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		float num = 0f;
		using (s_ProcessInteractionStrengthMarker.Auto())
		{
			if (!isSelected && !isHovered)
			{
				if (!m_ClearedLargestInteractionStrength)
				{
					m_LargestInteractionStrength.Value = 0f;
					m_ClearedLargestInteractionStrength = true;
				}
				return;
			}
			m_ClearedLargestInteractionStrength = false;
			bool flag = m_InteractionStrengthFilters.registeredSnapshot.Count > 0;
			if (isSelected)
			{
				int i = 0;
				for (int count = m_InteractorsSelecting.Count; i < count; i++)
				{
					IXRSelectInteractor iXRSelectInteractor = m_InteractorsSelecting[i];
					if (!(iXRSelectInteractor is XRBaseInputInteractor))
					{
						float num2 = (flag ? ProcessInteractionStrengthFilters(iXRSelectInteractor, 1f) : 1f);
						m_InteractionStrengths[iXRSelectInteractor] = num2;
						num = Mathf.Max(num, num2);
					}
				}
			}
			if (isHovered)
			{
				int j = 0;
				for (int count2 = m_InteractorsHovering.Count; j < count2; j++)
				{
					IXRHoverInteractor iXRHoverInteractor = m_InteractorsHovering[j];
					if (!(iXRHoverInteractor is XRBaseInputInteractor) && !IsSelected(iXRHoverInteractor))
					{
						float num3 = (flag ? ProcessInteractionStrengthFilters(iXRHoverInteractor, 0f) : 0f);
						m_InteractionStrengths[iXRHoverInteractor] = num3;
						num = Mathf.Max(num, num3);
					}
				}
			}
			int k = 0;
			for (int count3 = m_VariableSelectInteractors.Count; k < count3; k++)
			{
				XRBaseInputInteractor xRBaseInputInteractor = m_VariableSelectInteractors[k];
				float num4 = (flag ? ProcessInteractionStrengthFilters(xRBaseInputInteractor, ReadInteractionStrength(xRBaseInputInteractor)) : ReadInteractionStrength(xRBaseInputInteractor));
				m_InteractionStrengths[xRBaseInputInteractor] = num4;
				num = Mathf.Max(num, num4);
			}
		}
		using (s_ProcessInteractionStrengthEventMarker.Auto())
		{
			m_LargestInteractionStrength.Value = num;
		}
	}

	private float ReadInteractionStrength(XRBaseInputInteractor interactor)
	{
		if (!interactor.forceDeprecatedInput)
		{
			return interactor.selectInput.ReadValue();
		}
		if (interactor.xrController != null)
		{
			return interactor.xrController.selectInteractionState.value;
		}
		if (!IsSelected(interactor))
		{
			return 0f;
		}
		return 1f;
	}

	protected bool ProcessHoverFilters(IXRHoverInteractor interactor)
	{
		return XRFilterUtility.Process(m_HoverFilters, interactor, this);
	}

	protected bool ProcessSelectFilters(IXRSelectInteractor interactor)
	{
		return XRFilterUtility.Process(m_SelectFilters, interactor, this);
	}

	protected float ProcessInteractionStrengthFilters(IXRInteractor interactor, float interactionStrength)
	{
		return XRFilterUtility.Process(m_InteractionStrengthFilters, interactor, this, interactionStrength);
	}

	[Obsolete("OnHoverEntering(XRBaseInteractor) has been deprecated. Use OnHoverEntering(HoverEnterEventArgs) instead.", true)]
	protected virtual void OnHoverEntering(XRBaseInteractor interactor)
	{
		Debug.LogError("OnHoverEntering(XRBaseInteractor) has been deprecated. Use OnHoverEntering(HoverEnterEventArgs) instead.", this);
		throw new NotSupportedException("OnHoverEntering(XRBaseInteractor) has been deprecated. Use OnHoverEntering(HoverEnterEventArgs) instead.");
	}

	[Obsolete("OnHoverEntered(XRBaseInteractor) has been deprecated. Use OnHoverEntered(HoverEnterEventArgs) instead.", true)]
	protected virtual void OnHoverEntered(XRBaseInteractor interactor)
	{
		Debug.LogError("OnHoverEntered(XRBaseInteractor) has been deprecated. Use OnHoverEntered(HoverEnterEventArgs) instead.", this);
		throw new NotSupportedException("OnHoverEntered(XRBaseInteractor) has been deprecated. Use OnHoverEntered(HoverEnterEventArgs) instead.");
	}

	[Obsolete("OnHoverExiting(XRBaseInteractor) has been deprecated. Use OnHoverExiting(HoverExitEventArgs) instead.", true)]
	protected virtual void OnHoverExiting(XRBaseInteractor interactor)
	{
		Debug.LogError("OnHoverExiting(XRBaseInteractor) has been deprecated. Use OnHoverExiting(HoverExitEventArgs) instead.", this);
		throw new NotSupportedException("OnHoverExiting(XRBaseInteractor) has been deprecated. Use OnHoverExiting(HoverExitEventArgs) instead.");
	}

	[Obsolete("OnHoverExited(XRBaseInteractor) has been deprecated. Use OnHoverExited(HoverExitEventArgs) instead.", true)]
	protected virtual void OnHoverExited(XRBaseInteractor interactor)
	{
		Debug.LogError("OnHoverExited(XRBaseInteractor) has been deprecated. Use OnHoverExited(HoverExitEventArgs) instead.", this);
		throw new NotSupportedException("OnHoverExited(XRBaseInteractor) has been deprecated. Use OnHoverExited(HoverExitEventArgs) instead.");
	}

	[Obsolete("OnSelectEntering(XRBaseInteractor) has been deprecated. Use OnSelectEntering(SelectEnterEventArgs) instead.", true)]
	protected virtual void OnSelectEntering(XRBaseInteractor interactor)
	{
		Debug.LogError("OnSelectEntering(XRBaseInteractor) has been deprecated. Use OnSelectEntering(SelectEnterEventArgs) instead.", this);
		throw new NotSupportedException("OnSelectEntering(XRBaseInteractor) has been deprecated. Use OnSelectEntering(SelectEnterEventArgs) instead.");
	}

	[Obsolete("OnSelectEntered(XRBaseInteractor) has been deprecated. Use OnSelectEntered(SelectEnterEventArgs) instead.", true)]
	protected virtual void OnSelectEntered(XRBaseInteractor interactor)
	{
		Debug.LogError("OnSelectEntered(XRBaseInteractor) has been deprecated. Use OnSelectEntered(SelectEnterEventArgs) instead.", this);
		throw new NotSupportedException("OnSelectEntered(XRBaseInteractor) has been deprecated. Use OnSelectEntered(SelectEnterEventArgs) instead.");
	}

	[Obsolete("OnSelectExiting(XRBaseInteractor) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) and check for !args.isCanceled instead.", true)]
	protected virtual void OnSelectExiting(XRBaseInteractor interactor)
	{
		Debug.LogError("OnSelectExiting(XRBaseInteractor) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) and check for !args.isCanceled instead.", this);
		throw new NotSupportedException("OnSelectExiting(XRBaseInteractor) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) and check for !args.isCanceled instead.");
	}

	[Obsolete("OnSelectExited(XRBaseInteractor) has been deprecated. Use OnSelectExited(SelectExitEventArgs) and check for !args.isCanceled instead.", true)]
	protected virtual void OnSelectExited(XRBaseInteractor interactor)
	{
		Debug.LogError("OnSelectExited(XRBaseInteractor) has been deprecated. Use OnSelectExited(SelectExitEventArgs) and check for !args.isCanceled instead.", this);
		throw new NotSupportedException("OnSelectExited(XRBaseInteractor) has been deprecated. Use OnSelectExited(SelectExitEventArgs) and check for !args.isCanceled instead.");
	}

	[Obsolete("OnSelectCanceling(XRBaseInteractor) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) and check for args.isCanceled instead.", true)]
	protected virtual void OnSelectCanceling(XRBaseInteractor interactor)
	{
		Debug.LogError("OnSelectCanceling(XRBaseInteractor) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) and check for args.isCanceled instead.", this);
		throw new NotSupportedException("OnSelectCanceling(XRBaseInteractor) has been deprecated. Use OnSelectExiting(SelectExitEventArgs) and check for args.isCanceled instead.");
	}

	[Obsolete("OnSelectCanceled(XRBaseInteractor) has been deprecated. Use OnSelectExited(SelectExitEventArgs) and check for args.isCanceled instead.", true)]
	protected virtual void OnSelectCanceled(XRBaseInteractor interactor)
	{
		Debug.LogError("OnSelectCanceled(XRBaseInteractor) has been deprecated. Use OnSelectExited(SelectExitEventArgs) and check for args.isCanceled instead.", this);
		throw new NotSupportedException("OnSelectCanceled(XRBaseInteractor) has been deprecated. Use OnSelectExited(SelectExitEventArgs) and check for args.isCanceled instead.");
	}

	[Obsolete("OnActivate(XRBaseInteractor) has been deprecated. Use OnActivated(ActivateEventArgs) instead.", true)]
	protected virtual void OnActivate(XRBaseInteractor interactor)
	{
		Debug.LogError("OnActivate(XRBaseInteractor) has been deprecated. Use OnActivated(ActivateEventArgs) instead.", this);
		throw new NotSupportedException("OnActivate(XRBaseInteractor) has been deprecated. Use OnActivated(ActivateEventArgs) instead.");
	}

	[Obsolete("OnDeactivate(XRBaseInteractor) has been deprecated. Use OnDeactivated(DeactivateEventArgs) instead.", true)]
	protected virtual void OnDeactivate(XRBaseInteractor interactor)
	{
		Debug.LogError("OnDeactivate(XRBaseInteractor) has been deprecated. Use OnDeactivated(DeactivateEventArgs) instead.", this);
		throw new NotSupportedException("OnDeactivate(XRBaseInteractor) has been deprecated. Use OnDeactivated(DeactivateEventArgs) instead.");
	}

	[Obsolete("GetDistanceSqrToInteractor(XRBaseInteractor) has been deprecated. Use GetDistanceSqrToInteractor(IXRInteractor) instead.", true)]
	public virtual float GetDistanceSqrToInteractor(XRBaseInteractor interactor)
	{
		Debug.LogError("GetDistanceSqrToInteractor(XRBaseInteractor) has been deprecated. Use GetDistanceSqrToInteractor(IXRInteractor) instead.", this);
		throw new NotSupportedException("GetDistanceSqrToInteractor(XRBaseInteractor) has been deprecated. Use GetDistanceSqrToInteractor(IXRInteractor) instead.");
	}

	[Obsolete("AttachCustomReticle(XRBaseInteractor) has been deprecated. Use AttachCustomReticle(IXRInteractor) instead.", true)]
	public virtual void AttachCustomReticle(XRBaseInteractor interactor)
	{
		Debug.LogError("AttachCustomReticle(XRBaseInteractor) has been deprecated. Use AttachCustomReticle(IXRInteractor) instead.", this);
		throw new NotSupportedException("AttachCustomReticle(XRBaseInteractor) has been deprecated. Use AttachCustomReticle(IXRInteractor) instead.");
	}

	[Obsolete("RemoveCustomReticle(XRBaseInteractor) has been deprecated. Use RemoveCustomReticle(IXRInteractor) instead.", true)]
	public virtual void RemoveCustomReticle(XRBaseInteractor interactor)
	{
		Debug.LogError("RemoveCustomReticle(XRBaseInteractor) has been deprecated. Use RemoveCustomReticle(IXRInteractor) instead.", this);
		throw new NotSupportedException("RemoveCustomReticle(XRBaseInteractor) has been deprecated. Use RemoveCustomReticle(IXRInteractor) instead.");
	}

	[Obsolete("IsHoverableBy(XRBaseInteractor) has been deprecated. Use IsHoverableBy(IXRHoverInteractor) instead.", true)]
	public virtual bool IsHoverableBy(XRBaseInteractor interactor)
	{
		Debug.LogError("IsHoverableBy(XRBaseInteractor) has been deprecated. Use IsHoverableBy(IXRHoverInteractor) instead.", this);
		throw new NotSupportedException("IsHoverableBy(XRBaseInteractor) has been deprecated. Use IsHoverableBy(IXRHoverInteractor) instead.");
	}

	[Obsolete("IsSelectableBy(XRBaseInteractor) has been deprecated. Use IsSelectableBy(IXRSelectInteractor) instead.", true)]
	public virtual bool IsSelectableBy(XRBaseInteractor interactor)
	{
		Debug.LogError("IsSelectableBy(XRBaseInteractor) has been deprecated. Use IsSelectableBy(IXRSelectInteractor) instead.", this);
		throw new NotSupportedException("IsSelectableBy(XRBaseInteractor) has been deprecated. Use IsSelectableBy(IXRSelectInteractor) instead.");
	}

	Transform IXRInteractable.get_transform()
	{
		return base.transform;
	}
}
