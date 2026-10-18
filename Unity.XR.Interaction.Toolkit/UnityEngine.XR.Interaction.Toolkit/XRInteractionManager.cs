using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

namespace UnityEngine.XR.Interaction.Toolkit;

[AddComponentMenu("XR/XR Interaction Manager", 11)]
[DisallowMultipleComponent]
[DefaultExecutionOrder(-105)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.XRInteractionManager.html")]
public class XRInteractionManager : MonoBehaviour
{
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

	private readonly Dictionary<Collider, IXRInteractable> m_ColliderToInteractableMap = new Dictionary<Collider, IXRInteractable>();

	private readonly Dictionary<Collider, XRInteractableSnapVolume> m_ColliderToSnapVolumes = new Dictionary<Collider, XRInteractableSnapVolume>();

	private readonly RegistrationList<IXRInteractor> m_Interactors = new RegistrationList<IXRInteractor>();

	private readonly RegistrationList<IXRInteractionGroup> m_InteractionGroups = new RegistrationList<IXRInteractionGroup>();

	private readonly RegistrationList<IXRInteractable> m_Interactables = new RegistrationList<IXRInteractable>();

	private readonly List<IXRHoverInteractable> m_CurrentHovered = new List<IXRHoverInteractable>();

	private readonly List<IXRSelectInteractable> m_CurrentSelected = new List<IXRSelectInteractable>();

	private readonly Dictionary<IXRSelectInteractable, List<IXRTargetPriorityInteractor>> m_HighestPriorityTargetMap = new Dictionary<IXRSelectInteractable, List<IXRTargetPriorityInteractor>>();

	private static readonly LinkedPool<List<IXRTargetPriorityInteractor>> s_TargetPriorityInteractorListPool = new LinkedPool<List<IXRTargetPriorityInteractor>>(() => new List<IXRTargetPriorityInteractor>(), null, delegate(List<IXRTargetPriorityInteractor> list)
	{
		list.Clear();
	}, null, collectionCheck: false);

	private readonly List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();

	private readonly HashSet<IXRInteractable> m_UnorderedValidTargets = new HashSet<IXRInteractable>();

	private readonly HashSet<IXRInteractor> m_InteractorsInGroup = new HashSet<IXRInteractor>();

	private readonly HashSet<IXRInteractionGroup> m_GroupsInGroup = new HashSet<IXRInteractionGroup>();

	private readonly List<IXRInteractionGroup> m_ScratchInteractionGroups = new List<IXRInteractionGroup>();

	private readonly List<IXRInteractor> m_ScratchInteractors = new List<IXRInteractor>();

	private readonly LinkedPool<FocusEnterEventArgs> m_FocusEnterEventArgs = new LinkedPool<FocusEnterEventArgs>(() => new FocusEnterEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<FocusExitEventArgs> m_FocusExitEventArgs = new LinkedPool<FocusExitEventArgs>(() => new FocusExitEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<SelectEnterEventArgs> m_SelectEnterEventArgs = new LinkedPool<SelectEnterEventArgs>(() => new SelectEnterEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<SelectExitEventArgs> m_SelectExitEventArgs = new LinkedPool<SelectExitEventArgs>(() => new SelectExitEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<HoverEnterEventArgs> m_HoverEnterEventArgs = new LinkedPool<HoverEnterEventArgs>(() => new HoverEnterEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<HoverExitEventArgs> m_HoverExitEventArgs = new LinkedPool<HoverExitEventArgs>(() => new HoverExitEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<InteractionGroupRegisteredEventArgs> m_InteractionGroupRegisteredEventArgs = new LinkedPool<InteractionGroupRegisteredEventArgs>(() => new InteractionGroupRegisteredEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<InteractionGroupUnregisteredEventArgs> m_InteractionGroupUnregisteredEventArgs = new LinkedPool<InteractionGroupUnregisteredEventArgs>(() => new InteractionGroupUnregisteredEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<InteractorRegisteredEventArgs> m_InteractorRegisteredEventArgs = new LinkedPool<InteractorRegisteredEventArgs>(() => new InteractorRegisteredEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<InteractorUnregisteredEventArgs> m_InteractorUnregisteredEventArgs = new LinkedPool<InteractorUnregisteredEventArgs>(() => new InteractorUnregisteredEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<InteractableRegisteredEventArgs> m_InteractableRegisteredEventArgs = new LinkedPool<InteractableRegisteredEventArgs>(() => new InteractableRegisteredEventArgs(), null, null, null, collectionCheck: false);

	private readonly LinkedPool<InteractableUnregisteredEventArgs> m_InteractableUnregisteredEventArgs = new LinkedPool<InteractableUnregisteredEventArgs>(() => new InteractableUnregisteredEventArgs(), null, null, null, collectionCheck: false);

	private static readonly ProfilerMarker s_PreprocessInteractorsMarker = new ProfilerMarker("XRI.PreprocessInteractors");

	private static readonly ProfilerMarker s_ProcessInteractionStrengthMarker = new ProfilerMarker("XRI.ProcessInteractionStrength");

	private static readonly ProfilerMarker s_ProcessInteractorsMarker = new ProfilerMarker("XRI.ProcessInteractors");

	private static readonly ProfilerMarker s_ProcessInteractablesMarker = new ProfilerMarker("XRI.ProcessInteractables");

	private static readonly ProfilerMarker s_UpdateGroupMemberInteractionsMarker = new ProfilerMarker("XRI.UpdateGroupMemberInteractions");

	internal static readonly ProfilerMarker s_GetValidTargetsMarker = new ProfilerMarker("XRI.GetValidTargets");

	private static readonly ProfilerMarker s_FilterRegisteredValidTargetsMarker = new ProfilerMarker("XRI.FilterRegisteredValidTargets");

	internal static readonly ProfilerMarker s_EvaluateInvalidFocusMarker = new ProfilerMarker("XRI.EvaluateInvalidFocus");

	internal static readonly ProfilerMarker s_EvaluateInvalidSelectionsMarker = new ProfilerMarker("XRI.EvaluateInvalidSelections");

	internal static readonly ProfilerMarker s_EvaluateInvalidHoversMarker = new ProfilerMarker("XRI.EvaluateInvalidHovers");

	internal static readonly ProfilerMarker s_EvaluateValidSelectionsMarker = new ProfilerMarker("XRI.EvaluateValidSelections");

	internal static readonly ProfilerMarker s_EvaluateValidHoversMarker = new ProfilerMarker("XRI.EvaluateValidHovers");

	private static readonly ProfilerMarker s_FocusEnterMarker = new ProfilerMarker("XRI.FocusEnter");

	private static readonly ProfilerMarker s_FocusExitMarker = new ProfilerMarker("XRI.FocusExit");

	private static readonly ProfilerMarker s_SelectEnterMarker = new ProfilerMarker("XRI.SelectEnter");

	private static readonly ProfilerMarker s_SelectExitMarker = new ProfilerMarker("XRI.SelectExit");

	private static readonly ProfilerMarker s_HoverEnterMarker = new ProfilerMarker("XRI.HoverEnter");

	private static readonly ProfilerMarker s_HoverExitMarker = new ProfilerMarker("XRI.HoverExit");

	private const string k_RegisterInteractorDeprecated = "RegisterInteractor(XRBaseInteractor) has been deprecated. Use RegisterInteractor(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `RegisterInteractor((IXRInteractor)this)` instead.";

	private const string k_UnregisterInteractorDeprecated = "UnregisterInteractor(XRBaseInteractor) has been deprecated. Use UnregisterInteractor(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `UnregisterInteractor((IXRInteractor)this)` instead.";

	private const string k_RegisterInteractableDeprecated = "RegisterInteractable(XRBaseInteractable) has been deprecated. Use RegisterInteractable(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `RegisterInteractable((IXRInteractable)this)` instead.";

	private const string k_UnregisterInteractableDeprecated = "UnregisterInteractable(XRBaseInteractable) has been deprecated. Use UnregisterInteractable(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `UnregisterInteractable((IXRInteractable)this)` instead.";

	private const string k_GetRegisteredInteractorsDeprecated = "GetRegisteredInteractors(List<XRBaseInteractor>) has been deprecated. Use GetRegisteredInteractors(List<IXRInteractor>) instead.";

	private const string k_GetRegisteredInteractablesDeprecated = "GetRegisteredInteractables(List<XRBaseInteractable>) has been deprecated. Use GetRegisteredInteractables(List<IXRInteractable>) instead.";

	private const string k_IsRegisteredInteractorDeprecated = "IsRegistered(XRBaseInteractor) has been deprecated. Use IsRegistered(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `IsRegistered((IXRInteractor)this)` instead.";

	private const string k_IsRegisteredInteractableDeprecated = "IsRegistered(XRBaseInteractable) has been deprecated. Use IsRegistered(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `IsRegistered((IXRInteractable)this)` instead.";

	private const string k_TryGetInteractableForColliderDeprecated = "TryGetInteractableForCollider has been deprecated. Use GetInteractableForCollider instead. (UnityUpgradable) -> GetInteractableForCollider(*)";

	private const string k_GetInteractableForColliderDeprecated = "GetInteractableForCollider has been deprecated. Use TryGetInteractableForCollider(Collider, out IXRInteractable) instead.";

	private const string k_GetColliderToInteractableMapDeprecated = "GetColliderToInteractableMap has been deprecated. The signature no longer matches the field used by the XRInteractionManager, so a copy is returned instead of a ref. Changes to the returned Dictionary will not be observed by the XRInteractionManager.";

	private const string k_GetValidTargetsDeprecated = "GetValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use GetValidTargets(IXRInteractor, List<IXRInteractable>) instead.";

	private const string k_ForceSelectDeprecated = "ForceSelect(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable) instead.";

	private const string k_ClearInteractorSelectionDeprecated = "ClearInteractorSelection(XRBaseInteractor) has been deprecated. Use ClearInteractorSelection(IXRSelectInteractor, List<IXRInteractable>) instead.";

	private const string k_CancelInteractorSelectionDeprecated = "CancelInteractorSelection(XRBaseInteractor) has been deprecated. Use CancelInteractorSelection(IXRSelectInteractor) instead.";

	private const string k_CancelInteractableSelectionDeprecated = "CancelInteractableSelection(XRBaseInteractable) has been deprecated. Use CancelInteractableSelection(IXRSelectInteractable) instead.";

	private const string k_ClearInteractorHoverDeprecated = "ClearInteractorHover(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use ClearInteractorHover(IXRHoverInteractor, List<IXRInteractable>) instead.";

	private const string k_CancelInteractorHoverDeprecated = "CancelInteractorHover(XRBaseInteractor) has been deprecated. Use CancelInteractorHover(IXRHoverInteractor) instead.";

	private const string k_CancelInteractableHoverDeprecated = "CancelInteractableHover(XRBaseInteractable) has been deprecated. Use CancelInteractableHover(IXRHoverInteractable) instead.";

	private const string k_SelectEnterDeprecated = "SelectEnter(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.";

	private const string k_SelectExitDeprecated = "SelectExit(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectExit(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectExit((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.";

	private const string k_SelectCancelDeprecated = "SelectCancel(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectCancel(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectCancel((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.";

	private const string k_HoverEnterDeprecated = "HoverEnter(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverEnter(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverEnter((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.";

	private const string k_HoverExitDeprecated = "HoverExit(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverExit(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverExit((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.";

	private const string k_HoverCancelDeprecated = "HoverCancel(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverCancel(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverCancel((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.";

	private const string k_SelectEnterProtectedDeprecated = "SelectEnter(XRBaseInteractor, XRBaseInteractable, SelectEnterEventArgs) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable, SelectEnterEventArgs) instead.";

	private const string k_SelectExitProtectedDeprecated = "SelectExit(XRBaseInteractor, XRBaseInteractable, SelectExitEventArgs) has been deprecated. Use SelectExit(IXRSelectInteractor, IXRSelectInteractable, SelectExitEventArgs) instead.";

	private const string k_HoverEnterProtectedDeprecated = "HoverEnter(XRBaseInteractor, XRBaseInteractable, HoverEnterEventArgs) has been deprecated. Use HoverEnter(IXRHoverInteractor, IXRHoverInteractable, HoverEnterEventArgs) instead.";

	private const string k_HoverExitProtectedDeprecated = "HoverExit(XRBaseInteractor, XRBaseInteractable, HoverExitEventArgs) has been deprecated. Use HoverExit(IXRHoverInteractor, IXRHoverInteractable, HoverExitEventArgs) instead.";

	private const string k_InteractorSelectValidTargetsDeprecated = "InteractorSelectValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use InteractorSelectValidTargets(IXRSelectInteractor, List<IXRInteractable>) instead.";

	private const string k_InteractorHoverValidTargetsDeprecated = "InteractorHoverValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use InteractorHoverValidTargets(IXRHoverInteractor, List<IXRInteractable>) instead.";

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

	public IXRFocusInteractable lastFocused { get; protected set; }

	internal static List<XRInteractionManager> activeInteractionManagers { get; } = new List<XRInteractionManager>();


	public event Action<InteractionGroupRegisteredEventArgs> interactionGroupRegistered;

	public event Action<InteractionGroupUnregisteredEventArgs> interactionGroupUnregistered;

	public event Action<InteractorRegisteredEventArgs> interactorRegistered;

	public event Action<InteractorUnregisteredEventArgs> interactorUnregistered;

	public event Action<InteractableRegisteredEventArgs> interactableRegistered;

	public event Action<InteractableUnregisteredEventArgs> interactableUnregistered;

	public event Action<FocusEnterEventArgs> focusGained;

	public event Action<FocusExitEventArgs> focusLost;

	internal static event Action<XRInteractionManager, bool> activeInteractionManagersChanged;

	protected virtual void Awake()
	{
		m_HoverFilters.RegisterReferences(m_StartingHoverFilters, this);
		m_SelectFilters.RegisterReferences(m_StartingSelectFilters, this);
	}

	protected virtual void OnEnable()
	{
		if (activeInteractionManagers.Count > 0)
		{
			Debug.LogWarning("There are multiple active and enabled XR Interaction Manager components in the loaded scenes. This is supported, but may not be intended since interactors and interactables are not able to interact with those registered to a different manager. You can use the <b>Window</b> > <b>Analysis</b> > <b>XR Interaction Debugger</b> window to verify the interactors and interactables registered with each.", this);
		}
		activeInteractionManagers.Add(this);
		XRInteractionManager.activeInteractionManagersChanged?.Invoke(this, arg2: true);
		Application.onBeforeRender += OnBeforeRender;
	}

	protected virtual void OnDisable()
	{
		Application.onBeforeRender -= OnBeforeRender;
		activeInteractionManagers.Remove(this);
		XRInteractionManager.activeInteractionManagersChanged?.Invoke(this, arg2: false);
		ClearPriorityForSelectionMap();
	}

	protected virtual void Update()
	{
		ClearPriorityForSelectionMap();
		FlushRegistration();
		using (s_PreprocessInteractorsMarker.Auto())
		{
			PreprocessInteractors(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
		}
		foreach (IXRInteractionGroup item in m_InteractionGroups.registeredSnapshot)
		{
			if (m_InteractionGroups.IsStillRegistered(item) && !m_GroupsInGroup.Contains(item))
			{
				using (s_EvaluateInvalidFocusMarker.Auto())
				{
					ClearInteractionGroupFocus(item);
				}
				using (s_UpdateGroupMemberInteractionsMarker.Auto())
				{
					item.UpdateGroupMemberInteractions();
				}
			}
		}
		foreach (IXRInteractor item2 in m_Interactors.registeredSnapshot)
		{
			if (!m_Interactors.IsStillRegistered(item2) || m_InteractorsInGroup.Contains(item2))
			{
				continue;
			}
			using (s_GetValidTargetsMarker.Auto())
			{
				GetValidTargets(item2, m_ValidTargets);
			}
			IXRSelectInteractor iXRSelectInteractor = item2 as IXRSelectInteractor;
			IXRHoverInteractor iXRHoverInteractor = item2 as IXRHoverInteractor;
			if (iXRSelectInteractor != null)
			{
				using (s_EvaluateInvalidSelectionsMarker.Auto())
				{
					ClearInteractorSelection(iXRSelectInteractor, m_ValidTargets);
				}
			}
			if (iXRHoverInteractor != null)
			{
				using (s_EvaluateInvalidHoversMarker.Auto())
				{
					ClearInteractorHover(iXRHoverInteractor, m_ValidTargets);
				}
			}
			if (iXRSelectInteractor != null)
			{
				using (s_EvaluateValidSelectionsMarker.Auto())
				{
					InteractorSelectValidTargets(iXRSelectInteractor, m_ValidTargets);
				}
			}
			if (iXRHoverInteractor != null)
			{
				using (s_EvaluateValidHoversMarker.Auto())
				{
					InteractorHoverValidTargets(iXRHoverInteractor, m_ValidTargets);
				}
			}
		}
		using (s_ProcessInteractionStrengthMarker.Auto())
		{
			ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
		}
		using (s_ProcessInteractorsMarker.Auto())
		{
			ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
		}
		using (s_ProcessInteractablesMarker.Auto())
		{
			ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
		}
	}

	protected virtual void LateUpdate()
	{
		FlushRegistration();
		using (s_ProcessInteractorsMarker.Auto())
		{
			ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Late);
		}
		using (s_ProcessInteractablesMarker.Auto())
		{
			ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Late);
		}
	}

	protected virtual void FixedUpdate()
	{
		FlushRegistration();
		using (s_ProcessInteractorsMarker.Auto())
		{
			ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.Fixed);
		}
		using (s_ProcessInteractablesMarker.Auto())
		{
			ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.Fixed);
		}
	}

	[BeforeRenderOrder(100)]
	protected virtual void OnBeforeRender()
	{
		FlushRegistration();
		using (s_ProcessInteractorsMarker.Auto())
		{
			ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender);
		}
		using (s_ProcessInteractablesMarker.Auto())
		{
			ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase.OnBeforeRender);
		}
	}

	protected virtual void PreprocessInteractors(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		foreach (IXRInteractionGroup item in m_InteractionGroups.registeredSnapshot)
		{
			if (m_InteractionGroups.IsStillRegistered(item) && !m_GroupsInGroup.Contains(item))
			{
				item.PreprocessGroupMembers(updatePhase);
			}
		}
		foreach (IXRInteractor item2 in m_Interactors.registeredSnapshot)
		{
			if (m_Interactors.IsStillRegistered(item2) && !m_InteractorsInGroup.Contains(item2))
			{
				item2.PreprocessInteractor(updatePhase);
			}
		}
		XRUIToolkitHandler.UpdateEventSystem();
	}

	protected virtual void ProcessInteractors(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		foreach (IXRInteractionGroup item in m_InteractionGroups.registeredSnapshot)
		{
			if (m_InteractionGroups.IsStillRegistered(item) && !m_GroupsInGroup.Contains(item))
			{
				item.ProcessGroupMembers(updatePhase);
			}
		}
		foreach (IXRInteractor item2 in m_Interactors.registeredSnapshot)
		{
			if (m_Interactors.IsStillRegistered(item2) && !m_InteractorsInGroup.Contains(item2))
			{
				item2.ProcessInteractor(updatePhase);
			}
		}
	}

	protected virtual void ProcessInteractables(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		foreach (IXRInteractable item in m_Interactables.registeredSnapshot)
		{
			if (m_Interactables.IsStillRegistered(item))
			{
				item.ProcessInteractable(updatePhase);
			}
		}
	}

	protected virtual void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		foreach (IXRInteractable item in m_Interactables.registeredSnapshot)
		{
			if (m_Interactables.IsStillRegistered(item) && item is IXRInteractionStrengthInteractable iXRInteractionStrengthInteractable)
			{
				iXRInteractionStrengthInteractable.ProcessInteractionStrength(updatePhase);
			}
		}
		foreach (IXRInteractor item2 in m_Interactors.registeredSnapshot)
		{
			if (m_Interactors.IsStillRegistered(item2) && item2 is IXRInteractionStrengthInteractor iXRInteractionStrengthInteractor)
			{
				iXRInteractionStrengthInteractor.ProcessInteractionStrength(updatePhase);
			}
		}
	}

	public virtual bool CanHover(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
	{
		if (interactor.isHoverActive)
		{
			return IsHoverPossible(interactor, interactable);
		}
		return false;
	}

	public bool IsHoverPossible(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
	{
		if (HasInteractionLayerOverlap(interactor, interactable) && ProcessHoverFilters(interactor, interactable) && interactor.CanHover(interactable))
		{
			return interactable.IsHoverableBy(interactor);
		}
		return false;
	}

	public virtual bool CanSelect(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		if (interactor.isSelectActive)
		{
			return IsSelectPossible(interactor, interactable);
		}
		return false;
	}

	public bool IsSelectPossible(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		if (HasInteractionLayerOverlap(interactor, interactable) && ProcessSelectFilters(interactor, interactable) && interactor.CanSelect(interactable))
		{
			return interactable.IsSelectableBy(interactor);
		}
		return false;
	}

	public virtual bool CanFocus(IXRInteractor interactor, IXRFocusInteractable interactable)
	{
		return IsFocusPossible(interactor, interactable);
	}

	public bool IsFocusPossible(IXRInteractor interactor, IXRFocusInteractable interactable)
	{
		if (interactable.canFocus && HasInteractionLayerOverlap(interactor, interactable) && interactor is IXRGroupMember iXRGroupMember)
		{
			return iXRGroupMember.containingGroup != null;
		}
		return false;
	}

	public virtual void RegisterInteractionGroup(IXRInteractionGroup interactionGroup)
	{
		IXRInteractionGroup iXRInteractionGroup = null;
		if (interactionGroup is IXRGroupMember iXRGroupMember)
		{
			iXRInteractionGroup = iXRGroupMember.containingGroup;
		}
		if (iXRInteractionGroup != null && !IsRegistered(iXRInteractionGroup))
		{
			Debug.LogError($"Cannot register {interactionGroup} with Interaction Manager before its containing " + "Interaction Group is registered.", this);
		}
		else if (m_InteractionGroups.Register(interactionGroup))
		{
			if (iXRInteractionGroup != null)
			{
				m_GroupsInGroup.Add(interactionGroup);
			}
			InteractionGroupRegisteredEventArgs v;
			using (m_InteractionGroupRegisteredEventArgs.Get(out v))
			{
				v.manager = this;
				v.interactionGroupObject = interactionGroup;
				v.containingGroupObject = iXRInteractionGroup;
				OnRegistered(v);
			}
		}
	}

	protected virtual void OnRegistered(InteractionGroupRegisteredEventArgs args)
	{
		args.interactionGroupObject.OnRegistered(args);
		this.interactionGroupRegistered?.Invoke(args);
	}

	public virtual void UnregisterInteractionGroup(IXRInteractionGroup interactionGroup)
	{
		if (!IsRegistered(interactionGroup))
		{
			return;
		}
		interactionGroup.OnBeforeUnregistered();
		if (m_InteractionGroups.flushedCount > 0)
		{
			m_InteractionGroups.GetRegisteredItems(m_ScratchInteractionGroups);
			foreach (IXRInteractionGroup scratchInteractionGroup in m_ScratchInteractionGroups)
			{
				if (scratchInteractionGroup is IXRGroupMember iXRGroupMember && iXRGroupMember.containingGroup == interactionGroup)
				{
					Debug.LogError($"Cannot unregister {interactionGroup} with Interaction Manager before its " + "Group Members have been unregistered or re-registered as not part of the Group.", this);
					return;
				}
			}
		}
		if (m_Interactors.flushedCount > 0)
		{
			m_Interactors.GetRegisteredItems(m_ScratchInteractors);
			foreach (IXRInteractor scratchInteractor in m_ScratchInteractors)
			{
				if (scratchInteractor is IXRGroupMember iXRGroupMember2 && iXRGroupMember2.containingGroup == interactionGroup)
				{
					Debug.LogError($"Cannot unregister {interactionGroup} with Interaction Manager before its " + "Group Members have been unregistered or re-registered as not part of the Group.", this);
					return;
				}
			}
		}
		if (!m_InteractionGroups.Unregister(interactionGroup))
		{
			return;
		}
		m_GroupsInGroup.Remove(interactionGroup);
		InteractionGroupUnregisteredEventArgs v;
		using (m_InteractionGroupUnregisteredEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactionGroupObject = interactionGroup;
			OnUnregistered(v);
		}
	}

	protected virtual void OnUnregistered(InteractionGroupUnregisteredEventArgs args)
	{
		args.interactionGroupObject.OnUnregistered(args);
		this.interactionGroupUnregistered?.Invoke(args);
	}

	public void GetInteractionGroups(List<IXRInteractionGroup> interactionGroups)
	{
		m_InteractionGroups.GetRegisteredItems(interactionGroups);
	}

	public IXRInteractionGroup GetInteractionGroup(string groupName)
	{
		foreach (IXRInteractionGroup item in m_InteractionGroups.registeredSnapshot)
		{
			if (item.groupName == groupName)
			{
				return item;
			}
		}
		return null;
	}

	public virtual void RegisterInteractor(IXRInteractor interactor)
	{
		IXRInteractionGroup iXRInteractionGroup = null;
		if (interactor is IXRGroupMember iXRGroupMember)
		{
			iXRInteractionGroup = iXRGroupMember.containingGroup;
		}
		if (iXRInteractionGroup != null && !IsRegistered(iXRInteractionGroup))
		{
			Debug.LogError($"Cannot register {interactor} with Interaction Manager before its containing " + "Interaction Group is registered.", this);
		}
		else if (m_Interactors.Register(interactor))
		{
			if (iXRInteractionGroup != null)
			{
				m_InteractorsInGroup.Add(interactor);
			}
			InteractorRegisteredEventArgs v;
			using (m_InteractorRegisteredEventArgs.Get(out v))
			{
				v.manager = this;
				v.interactorObject = interactor;
				v.containingGroupObject = iXRInteractionGroup;
				OnRegistered(v);
			}
		}
	}

	protected virtual void OnRegistered(InteractorRegisteredEventArgs args)
	{
		args.interactorObject.OnRegistered(args);
		this.interactorRegistered?.Invoke(args);
	}

	public virtual void UnregisterInteractor(IXRInteractor interactor)
	{
		if (!IsRegistered(interactor))
		{
			return;
		}
		Transform transform = interactor.transform;
		if (transform == null || transform.gameObject.activeSelf)
		{
			CancelInteractorFocus(interactor);
		}
		if (interactor is IXRSelectInteractor interactor2)
		{
			CancelInteractorSelection(interactor2);
		}
		if (interactor is IXRHoverInteractor interactor3)
		{
			CancelInteractorHover(interactor3);
		}
		if (!m_Interactors.Unregister(interactor))
		{
			return;
		}
		m_InteractorsInGroup.Remove(interactor);
		InteractorUnregisteredEventArgs v;
		using (m_InteractorUnregisteredEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = interactor;
			OnUnregistered(v);
		}
	}

	protected virtual void OnUnregistered(InteractorUnregisteredEventArgs args)
	{
		args.interactorObject.OnUnregistered(args);
		this.interactorUnregistered?.Invoke(args);
	}

	public virtual void RegisterInteractable(IXRInteractable interactable)
	{
		if (!m_Interactables.Register(interactable))
		{
			return;
		}
		foreach (Collider collider in interactable.colliders)
		{
			if (!(collider == null))
			{
				if (!m_ColliderToInteractableMap.TryGetValue(collider, out var value))
				{
					m_ColliderToInteractableMap.Add(collider, interactable);
				}
				else
				{
					Debug.LogWarning("A collider used by an Interactable object is already registered with another Interactable object." + $" The {collider} will remain associated with {value}, which was registered before {interactable}." + " The value returned by XRInteractionManager.TryGetInteractableForCollider will be the first association.", interactable as Object);
				}
			}
		}
		InteractableRegisteredEventArgs v;
		using (m_InteractableRegisteredEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactableObject = interactable;
			OnRegistered(v);
		}
	}

	protected virtual void OnRegistered(InteractableRegisteredEventArgs args)
	{
		args.interactableObject.OnRegistered(args);
		this.interactableRegistered?.Invoke(args);
	}

	public virtual void UnregisterInteractable(IXRInteractable interactable)
	{
		if (!IsRegistered(interactable))
		{
			return;
		}
		if (interactable is IXRFocusInteractable interactable2)
		{
			CancelInteractableFocus(interactable2);
		}
		if (interactable is IXRSelectInteractable interactable3)
		{
			CancelInteractableSelection(interactable3);
		}
		if (interactable is IXRHoverInteractable interactable4)
		{
			CancelInteractableHover(interactable4);
		}
		if (!m_Interactables.Unregister(interactable))
		{
			return;
		}
		foreach (Collider collider in interactable.colliders)
		{
			if (!(collider == null) && m_ColliderToInteractableMap.TryGetValue(collider, out var value) && value == interactable)
			{
				m_ColliderToInteractableMap.Remove(collider);
			}
		}
		InteractableUnregisteredEventArgs v;
		using (m_InteractableUnregisteredEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactableObject = interactable;
			OnUnregistered(v);
		}
	}

	protected virtual void OnUnregistered(InteractableUnregisteredEventArgs args)
	{
		args.interactableObject.OnUnregistered(args);
		this.interactableUnregistered?.Invoke(args);
	}

	public void RegisterSnapVolume(XRInteractableSnapVolume snapVolume)
	{
		if (snapVolume == null)
		{
			return;
		}
		Collider snapCollider = snapVolume.snapCollider;
		if (!(snapCollider == null))
		{
			if (!m_ColliderToSnapVolumes.TryGetValue(snapCollider, out var value))
			{
				m_ColliderToSnapVolumes.Add(snapCollider, snapVolume);
			}
			else
			{
				Debug.LogWarning("A collider used by a snap volume component is already registered with another snap volume component." + $" The {snapCollider} will remain associated with {value}, which was registered before {snapVolume}." + " The value returned by XRInteractionManager.TryGetInteractableForCollider will be the first association.", snapVolume);
			}
		}
	}

	public void UnregisterSnapVolume(XRInteractableSnapVolume snapVolume)
	{
		if (!(snapVolume == null))
		{
			Collider snapCollider = snapVolume.snapCollider;
			if (!(snapCollider == null) && m_ColliderToSnapVolumes.TryGetValue(snapCollider, out var value) && value == snapVolume)
			{
				m_ColliderToSnapVolumes.Remove(snapCollider);
			}
		}
	}

	public void GetRegisteredInteractionGroups(List<IXRInteractionGroup> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		m_InteractionGroups.GetRegisteredItems(results);
	}

	public void GetRegisteredInteractors(List<IXRInteractor> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		m_Interactors.GetRegisteredItems(results);
	}

	public void GetRegisteredInteractables(List<IXRInteractable> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		m_Interactables.GetRegisteredItems(results);
	}

	public bool IsRegistered(IXRInteractionGroup interactionGroup)
	{
		return m_InteractionGroups.IsRegistered(interactionGroup);
	}

	public bool IsRegistered(IXRInteractor interactor)
	{
		return m_Interactors.IsRegistered(interactor);
	}

	public bool IsRegistered(IXRInteractable interactable)
	{
		return m_Interactables.IsRegistered(interactable);
	}

	public bool TryGetInteractableForCollider(Collider interactableCollider, out IXRInteractable interactable)
	{
		interactable = null;
		if (interactableCollider == null)
		{
			return false;
		}
		if (!m_ColliderToInteractableMap.TryGetValue(interactableCollider, out interactable) && m_ColliderToSnapVolumes.TryGetValue(interactableCollider, out var value) && value != null)
		{
			interactable = value.interactable;
		}
		if (interactable != null)
		{
			if (interactable is Object @object)
			{
				return @object != null;
			}
			return true;
		}
		return false;
	}

	public bool TryGetInteractableForCollider(Collider interactableCollider, out IXRInteractable interactable, out XRInteractableSnapVolume snapVolume)
	{
		interactable = null;
		snapVolume = null;
		if (interactableCollider == null)
		{
			return false;
		}
		bool flag = m_ColliderToInteractableMap.TryGetValue(interactableCollider, out interactable);
		if (m_ColliderToSnapVolumes.TryGetValue(interactableCollider, out snapVolume) && snapVolume != null)
		{
			if (flag)
			{
				if (snapVolume.interactable != interactable)
				{
					snapVolume = null;
				}
			}
			else
			{
				interactable = snapVolume.interactable;
			}
		}
		if (interactable != null)
		{
			if (interactable is Object @object)
			{
				return @object != null;
			}
			return true;
		}
		return false;
	}

	public bool IsColliderRegisteredToInteractable(in Collider colliderToCheck)
	{
		if (!m_ColliderToInteractableMap.ContainsKey(colliderToCheck))
		{
			return m_ColliderToSnapVolumes.ContainsKey(colliderToCheck);
		}
		return true;
	}

	public bool IsColliderRegisteredSnapVolume(in Collider potentialSnapVolumeCollider)
	{
		return m_ColliderToSnapVolumes.ContainsKey(potentialSnapVolumeCollider);
	}

	public bool IsHighestPriorityTarget(IXRSelectInteractable target, List<IXRTargetPriorityInteractor> interactors = null)
	{
		if (!m_HighestPriorityTargetMap.TryGetValue(target, out var value))
		{
			return false;
		}
		if (interactors == null)
		{
			return true;
		}
		interactors.Clear();
		interactors.AddRange(value);
		return true;
	}

	public bool IsHandSelecting(InteractorHandedness hand)
	{
		foreach (IXRInteractor item in m_Interactors.registeredSnapshot)
		{
			if (m_Interactors.IsStillRegistered(item) && item.handedness == hand && item is IXRSelectInteractor { hasSelection: not false })
			{
				return true;
			}
		}
		return false;
	}

	public void GetValidTargets(IXRInteractor interactor, List<IXRInteractable> targets)
	{
		targets.Clear();
		interactor.GetValidTargets(targets);
		using (s_FilterRegisteredValidTargetsMarker.Auto())
		{
			RemoveAllUnregistered(this, targets);
		}
	}

	internal static int RemoveAllUnregistered(XRInteractionManager manager, List<IXRInteractable> interactables)
	{
		int num = 0;
		for (int num2 = interactables.Count - 1; num2 >= 0; num2--)
		{
			if (!manager.m_Interactables.IsRegistered(interactables[num2]))
			{
				interactables.RemoveAt(num2);
				num++;
			}
		}
		return num;
	}

	protected virtual void ClearInteractionGroupFocus(IXRInteractionGroup interactionGroup)
	{
		IXRInteractor focusInteractor = interactionGroup.focusInteractor;
		IXRFocusInteractable focusInteractable = interactionGroup.focusInteractable;
		if (focusInteractor != null && focusInteractable != null)
		{
			bool flag = false;
			if (focusInteractor is IXRSelectInteractor iXRSelectInteractor)
			{
				flag = ((!(focusInteractable is IXRSelectInteractable interactable)) ? iXRSelectInteractor.isSelectActive : (iXRSelectInteractor.isSelectActive && !iXRSelectInteractor.IsSelecting(interactable)));
			}
			if (flag || !CanFocus(focusInteractor, focusInteractable))
			{
				FocusExit(interactionGroup, focusInteractable);
			}
		}
	}

	private void CancelInteractorFocus(IXRInteractor interactor)
	{
		IXRInteractionGroup iXRInteractionGroup = ((interactor is IXRGroupMember iXRGroupMember) ? iXRGroupMember.containingGroup : null);
		if (iXRInteractionGroup != null && iXRInteractionGroup.focusInteractable != null && iXRInteractionGroup.focusInteractor == interactor)
		{
			FocusCancel(iXRInteractionGroup, iXRInteractionGroup.focusInteractable);
		}
	}

	public virtual void CancelInteractableFocus(IXRFocusInteractable interactable)
	{
		for (int num = interactable.interactionGroupsFocusing.Count - 1; num >= 0; num--)
		{
			FocusCancel(interactable.interactionGroupsFocusing[num], interactable);
		}
	}

	protected internal virtual void ClearInteractorSelection(IXRSelectInteractor interactor, List<IXRInteractable> validTargets)
	{
		if (!interactor.hasSelection)
		{
			return;
		}
		m_CurrentSelected.Clear();
		m_CurrentSelected.AddRange(interactor.interactablesSelected);
		m_UnorderedValidTargets.Clear();
		if (validTargets.Count > 0)
		{
			foreach (IXRInteractable validTarget in validTargets)
			{
				m_UnorderedValidTargets.Add(validTarget);
			}
		}
		for (int num = m_CurrentSelected.Count - 1; num >= 0; num--)
		{
			IXRSelectInteractable iXRSelectInteractable = m_CurrentSelected[num];
			if (!CanSelect(interactor, iXRSelectInteractable) || (!interactor.keepSelectedTargetValid && !m_UnorderedValidTargets.Contains(iXRSelectInteractable)))
			{
				SelectExit(interactor, iXRSelectInteractable);
			}
		}
	}

	public virtual void CancelInteractorSelection(IXRSelectInteractor interactor)
	{
		for (int num = interactor.interactablesSelected.Count - 1; num >= 0; num--)
		{
			SelectCancel(interactor, interactor.interactablesSelected[num]);
		}
	}

	public virtual void CancelInteractableSelection(IXRSelectInteractable interactable)
	{
		for (int num = interactable.interactorsSelecting.Count - 1; num >= 0; num--)
		{
			SelectCancel(interactable.interactorsSelecting[num], interactable);
		}
	}

	protected internal virtual void ClearInteractorHover(IXRHoverInteractor interactor, List<IXRInteractable> validTargets)
	{
		if (!interactor.hasHover)
		{
			return;
		}
		m_CurrentHovered.Clear();
		m_CurrentHovered.AddRange(interactor.interactablesHovered);
		m_UnorderedValidTargets.Clear();
		if (validTargets.Count > 0)
		{
			foreach (IXRInteractable validTarget in validTargets)
			{
				m_UnorderedValidTargets.Add(validTarget);
			}
		}
		for (int num = m_CurrentHovered.Count - 1; num >= 0; num--)
		{
			IXRHoverInteractable iXRHoverInteractable = m_CurrentHovered[num];
			if (!CanHover(interactor, iXRHoverInteractable) || !m_UnorderedValidTargets.Contains(iXRHoverInteractable))
			{
				HoverExit(interactor, iXRHoverInteractable);
			}
		}
	}

	public virtual void CancelInteractorHover(IXRHoverInteractor interactor)
	{
		for (int num = interactor.interactablesHovered.Count - 1; num >= 0; num--)
		{
			HoverCancel(interactor, interactor.interactablesHovered[num]);
		}
	}

	public virtual void CancelInteractableHover(IXRHoverInteractable interactable)
	{
		for (int num = interactable.interactorsHovering.Count - 1; num >= 0; num--)
		{
			HoverCancel(interactable.interactorsHovering[num], interactable);
		}
	}

	public virtual void FocusEnter(IXRInteractor interactor, IXRFocusInteractable interactable)
	{
		IXRInteractionGroup iXRInteractionGroup = (interactor as IXRGroupMember)?.containingGroup;
		if (iXRInteractionGroup == null || !CanFocus(interactor, interactable) || (interactable.isFocused && !ResolveExistingFocus(iXRInteractionGroup, interactable)))
		{
			return;
		}
		FocusEnterEventArgs v;
		using (m_FocusEnterEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = interactor;
			v.interactableObject = interactable;
			v.interactionGroup = iXRInteractionGroup;
			FocusEnter(iXRInteractionGroup, interactable, v);
		}
	}

	public virtual void FocusExit(IXRInteractionGroup group, IXRFocusInteractable interactable)
	{
		FocusExitEventArgs v;
		using (m_FocusExitEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = group.focusInteractor;
			v.interactableObject = interactable;
			v.interactionGroup = group;
			v.isCanceled = false;
			FocusExit(group, interactable, v);
		}
	}

	public virtual void FocusCancel(IXRInteractionGroup group, IXRFocusInteractable interactable)
	{
		FocusExitEventArgs v;
		using (m_FocusExitEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = group.focusInteractor;
			v.interactableObject = interactable;
			v.interactionGroup = group;
			v.isCanceled = true;
			FocusExit(group, interactable, v);
		}
	}

	public virtual void SelectEnter(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		if (!interactable.isSelected || ResolveExistingSelect(interactor, interactable))
		{
			SelectEnterEventArgs v;
			using (m_SelectEnterEventArgs.Get(out v))
			{
				v.manager = this;
				v.interactorObject = interactor;
				v.interactableObject = interactable;
				SelectEnter(interactor, interactable, v);
			}
			if (interactable is IXRFocusInteractable interactable2)
			{
				FocusEnter(interactor, interactable2);
			}
		}
	}

	public virtual void SelectExit(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		SelectExitEventArgs v;
		using (m_SelectExitEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = interactor;
			v.interactableObject = interactable;
			v.isCanceled = false;
			SelectExit(interactor, interactable, v);
		}
	}

	public virtual void SelectCancel(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		SelectExitEventArgs v;
		using (m_SelectExitEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = interactor;
			v.interactableObject = interactable;
			v.isCanceled = true;
			SelectExit(interactor, interactable, v);
		}
	}

	public virtual void HoverEnter(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
	{
		HoverEnterEventArgs v;
		using (m_HoverEnterEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = interactor;
			v.interactableObject = interactable;
			HoverEnter(interactor, interactable, v);
		}
	}

	public virtual void HoverExit(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
	{
		HoverExitEventArgs v;
		using (m_HoverExitEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = interactor;
			v.interactableObject = interactable;
			v.isCanceled = false;
			HoverExit(interactor, interactable, v);
		}
	}

	public virtual void HoverCancel(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
	{
		HoverExitEventArgs v;
		using (m_HoverExitEventArgs.Get(out v))
		{
			v.manager = this;
			v.interactorObject = interactor;
			v.interactableObject = interactable;
			v.isCanceled = true;
			HoverExit(interactor, interactable, v);
		}
	}

	protected virtual void FocusEnter(IXRInteractionGroup group, IXRFocusInteractable interactable, FocusEnterEventArgs args)
	{
		args.manager = this;
		using (s_FocusEnterMarker.Auto())
		{
			group.OnFocusEntering(args);
			if (group is IXRGroupMember { containingGroup: var iXRInteractionGroup })
			{
				while (iXRInteractionGroup != null)
				{
					iXRInteractionGroup.OnFocusEntering(args);
					iXRInteractionGroup = (iXRInteractionGroup as IXRGroupMember)?.containingGroup;
				}
			}
			interactable.OnFocusEntering(args);
			interactable.OnFocusEntered(args);
		}
		lastFocused = interactable;
		this.focusGained?.Invoke(args);
	}

	protected virtual void FocusExit(IXRInteractionGroup group, IXRFocusInteractable interactable, FocusExitEventArgs args)
	{
		args.manager = this;
		using (s_FocusExitMarker.Auto())
		{
			group.OnFocusExiting(args);
			if (group is IXRGroupMember { containingGroup: var iXRInteractionGroup })
			{
				while (iXRInteractionGroup != null)
				{
					iXRInteractionGroup.OnFocusExiting(args);
					iXRInteractionGroup = (iXRInteractionGroup as IXRGroupMember)?.containingGroup;
				}
			}
			interactable.OnFocusExiting(args);
			interactable.OnFocusExited(args);
		}
		if (interactable == lastFocused)
		{
			lastFocused = null;
		}
		this.focusLost?.Invoke(args);
	}

	protected virtual void SelectEnter(IXRSelectInteractor interactor, IXRSelectInteractable interactable, SelectEnterEventArgs args)
	{
		args.manager = this;
		using (s_SelectEnterMarker.Auto())
		{
			interactor.OnSelectEntering(args);
			interactable.OnSelectEntering(args);
			interactor.OnSelectEntered(args);
			interactable.OnSelectEntered(args);
		}
	}

	protected virtual void SelectExit(IXRSelectInteractor interactor, IXRSelectInteractable interactable, SelectExitEventArgs args)
	{
		args.manager = this;
		using (s_SelectExitMarker.Auto())
		{
			interactor.OnSelectExiting(args);
			interactable.OnSelectExiting(args);
			interactor.OnSelectExited(args);
			interactable.OnSelectExited(args);
		}
	}

	protected virtual void HoverEnter(IXRHoverInteractor interactor, IXRHoverInteractable interactable, HoverEnterEventArgs args)
	{
		args.manager = this;
		using (s_HoverEnterMarker.Auto())
		{
			interactor.OnHoverEntering(args);
			interactable.OnHoverEntering(args);
			interactor.OnHoverEntered(args);
			interactable.OnHoverEntered(args);
		}
	}

	protected virtual void HoverExit(IXRHoverInteractor interactor, IXRHoverInteractable interactable, HoverExitEventArgs args)
	{
		args.manager = this;
		using (s_HoverExitMarker.Auto())
		{
			interactor.OnHoverExiting(args);
			interactable.OnHoverExiting(args);
			interactor.OnHoverExited(args);
			interactable.OnHoverExited(args);
		}
	}

	protected internal virtual void InteractorSelectValidTargets(IXRSelectInteractor interactor, List<IXRInteractable> validTargets)
	{
		if (validTargets.Count == 0)
		{
			return;
		}
		IXRTargetPriorityInteractor iXRTargetPriorityInteractor = interactor as IXRTargetPriorityInteractor;
		TargetPriorityMode targetPriorityMode = TargetPriorityMode.None;
		if (iXRTargetPriorityInteractor != null)
		{
			targetPriorityMode = iXRTargetPriorityInteractor.targetPriorityMode;
		}
		bool flag = false;
		foreach (IXRInteractable validTarget in validTargets)
		{
			if (!(validTarget is IXRSelectInteractable iXRSelectInteractable))
			{
				continue;
			}
			if (targetPriorityMode == TargetPriorityMode.None || (targetPriorityMode == TargetPriorityMode.HighestPriorityOnly && flag))
			{
				if (CanSelect(interactor, iXRSelectInteractable))
				{
					SelectEnter(interactor, iXRSelectInteractable);
				}
			}
			else
			{
				if (!IsSelectPossible(interactor, iXRSelectInteractable))
				{
					continue;
				}
				if (!flag)
				{
					flag = true;
					if (!m_HighestPriorityTargetMap.TryGetValue(iXRSelectInteractable, out var value))
					{
						value = s_TargetPriorityInteractorListPool.Get();
						m_HighestPriorityTargetMap[iXRSelectInteractable] = value;
					}
					value.Add(iXRTargetPriorityInteractor);
				}
				iXRTargetPriorityInteractor.targetsForSelection?.Add(iXRSelectInteractable);
				if (interactor.isSelectActive)
				{
					SelectEnter(interactor, iXRSelectInteractable);
				}
			}
		}
	}

	protected internal virtual void InteractorHoverValidTargets(IXRHoverInteractor interactor, List<IXRInteractable> validTargets)
	{
		if (validTargets.Count == 0)
		{
			return;
		}
		foreach (IXRInteractable validTarget in validTargets)
		{
			if (validTarget is IXRHoverInteractable interactable && CanHover(interactor, interactable) && !interactor.IsHovering(interactable))
			{
				HoverEnter(interactor, interactable);
			}
		}
	}

	protected virtual bool ResolveExistingFocus(IXRInteractionGroup interactionGroup, IXRFocusInteractable interactable)
	{
		if (interactionGroup.focusInteractable == interactable)
		{
			return false;
		}
		switch (interactable.focusMode)
		{
		case InteractableFocusMode.Single:
			ExitInteractableFocus(interactable);
			break;
		default:
			return false;
		case InteractableFocusMode.Multiple:
			break;
		}
		return true;
	}

	protected virtual bool ResolveExistingSelect(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		if (interactor.IsSelecting(interactable))
		{
			return false;
		}
		switch (interactable.selectMode)
		{
		case InteractableSelectMode.Single:
			ExitInteractableSelection(interactable);
			break;
		default:
			return false;
		case InteractableSelectMode.Multiple:
			break;
		}
		return true;
	}

	protected static bool HasInteractionLayerOverlap(IXRInteractor interactor, IXRInteractable interactable)
	{
		return ((int)interactor.interactionLayers & (int)interactable.interactionLayers) != 0;
	}

	protected bool ProcessHoverFilters(IXRHoverInteractor interactor, IXRHoverInteractable interactable)
	{
		return XRFilterUtility.Process(m_HoverFilters, interactor, interactable);
	}

	protected bool ProcessSelectFilters(IXRSelectInteractor interactor, IXRSelectInteractable interactable)
	{
		return XRFilterUtility.Process(m_SelectFilters, interactor, interactable);
	}

	private void ExitInteractableSelection(IXRSelectInteractable interactable)
	{
		for (int num = interactable.interactorsSelecting.Count - 1; num >= 0; num--)
		{
			SelectExit(interactable.interactorsSelecting[num], interactable);
		}
	}

	private void ExitInteractableFocus(IXRFocusInteractable interactable)
	{
		for (int num = interactable.interactionGroupsFocusing.Count - 1; num >= 0; num--)
		{
			FocusExit(interactable.interactionGroupsFocusing[num], interactable);
		}
	}

	private void ClearPriorityForSelectionMap()
	{
		if (m_HighestPriorityTargetMap.Count == 0)
		{
			return;
		}
		foreach (List<IXRTargetPriorityInteractor> value in m_HighestPriorityTargetMap.Values)
		{
			foreach (IXRTargetPriorityInteractor item in value)
			{
				item?.targetsForSelection?.Clear();
			}
			s_TargetPriorityInteractorListPool.Release(value);
		}
		m_HighestPriorityTargetMap.Clear();
	}

	private void FlushRegistration()
	{
		m_InteractionGroups.Flush();
		m_Interactors.Flush();
		m_Interactables.Flush();
	}

	[Obsolete("RegisterInteractor(XRBaseInteractor) has been deprecated. Use RegisterInteractor(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `RegisterInteractor((IXRInteractor)this)` instead.", true)]
	public virtual void RegisterInteractor(XRBaseInteractor interactor)
	{
		Debug.LogError("RegisterInteractor(XRBaseInteractor) has been deprecated. Use RegisterInteractor(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `RegisterInteractor((IXRInteractor)this)` instead.", this);
		throw new NotSupportedException("RegisterInteractor(XRBaseInteractor) has been deprecated. Use RegisterInteractor(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `RegisterInteractor((IXRInteractor)this)` instead.");
	}

	[Obsolete("UnregisterInteractor(XRBaseInteractor) has been deprecated. Use UnregisterInteractor(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `UnregisterInteractor((IXRInteractor)this)` instead.", true)]
	public virtual void UnregisterInteractor(XRBaseInteractor interactor)
	{
		Debug.LogError("UnregisterInteractor(XRBaseInteractor) has been deprecated. Use UnregisterInteractor(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `UnregisterInteractor((IXRInteractor)this)` instead.", this);
		throw new NotSupportedException("UnregisterInteractor(XRBaseInteractor) has been deprecated. Use UnregisterInteractor(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `UnregisterInteractor((IXRInteractor)this)` instead.");
	}

	[Obsolete("RegisterInteractable(XRBaseInteractable) has been deprecated. Use RegisterInteractable(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `RegisterInteractable((IXRInteractable)this)` instead.", true)]
	public virtual void RegisterInteractable(XRBaseInteractable interactable)
	{
		Debug.LogError("RegisterInteractable(XRBaseInteractable) has been deprecated. Use RegisterInteractable(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `RegisterInteractable((IXRInteractable)this)` instead.", this);
		throw new NotSupportedException("RegisterInteractable(XRBaseInteractable) has been deprecated. Use RegisterInteractable(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `RegisterInteractable((IXRInteractable)this)` instead.");
	}

	[Obsolete("UnregisterInteractable(XRBaseInteractable) has been deprecated. Use UnregisterInteractable(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `UnregisterInteractable((IXRInteractable)this)` instead.", true)]
	public virtual void UnregisterInteractable(XRBaseInteractable interactable)
	{
		Debug.LogError("UnregisterInteractable(XRBaseInteractable) has been deprecated. Use UnregisterInteractable(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `UnregisterInteractable((IXRInteractable)this)` instead.", this);
		throw new NotSupportedException("UnregisterInteractable(XRBaseInteractable) has been deprecated. Use UnregisterInteractable(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `UnregisterInteractable((IXRInteractable)this)` instead.");
	}

	[Obsolete("GetRegisteredInteractors(List<XRBaseInteractor>) has been deprecated. Use GetRegisteredInteractors(List<IXRInteractor>) instead.", true)]
	public void GetRegisteredInteractors(List<XRBaseInteractor> results)
	{
		Debug.LogError("GetRegisteredInteractors(List<XRBaseInteractor>) has been deprecated. Use GetRegisteredInteractors(List<IXRInteractor>) instead.", this);
		throw new NotSupportedException("GetRegisteredInteractors(List<XRBaseInteractor>) has been deprecated. Use GetRegisteredInteractors(List<IXRInteractor>) instead.");
	}

	[Obsolete("GetRegisteredInteractables(List<XRBaseInteractable>) has been deprecated. Use GetRegisteredInteractables(List<IXRInteractable>) instead.", true)]
	public void GetRegisteredInteractables(List<XRBaseInteractable> results)
	{
		Debug.LogError("GetRegisteredInteractables(List<XRBaseInteractable>) has been deprecated. Use GetRegisteredInteractables(List<IXRInteractable>) instead.", this);
		throw new NotSupportedException("GetRegisteredInteractables(List<XRBaseInteractable>) has been deprecated. Use GetRegisteredInteractables(List<IXRInteractable>) instead.");
	}

	[Obsolete("IsRegistered(XRBaseInteractor) has been deprecated. Use IsRegistered(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `IsRegistered((IXRInteractor)this)` instead.", true)]
	public bool IsRegistered(XRBaseInteractor interactor)
	{
		Debug.LogError("IsRegistered(XRBaseInteractor) has been deprecated. Use IsRegistered(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `IsRegistered((IXRInteractor)this)` instead.", this);
		throw new NotSupportedException("IsRegistered(XRBaseInteractor) has been deprecated. Use IsRegistered(IXRInteractor) instead. You may need to modify your code by casting the argument to call the intended method, such as `IsRegistered((IXRInteractor)this)` instead.");
	}

	[Obsolete("IsRegistered(XRBaseInteractable) has been deprecated. Use IsRegistered(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `IsRegistered((IXRInteractable)this)` instead.", true)]
	public bool IsRegistered(XRBaseInteractable interactable)
	{
		Debug.LogError("IsRegistered(XRBaseInteractable) has been deprecated. Use IsRegistered(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `IsRegistered((IXRInteractable)this)` instead.", this);
		throw new NotSupportedException("IsRegistered(XRBaseInteractable) has been deprecated. Use IsRegistered(IXRInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `IsRegistered((IXRInteractable)this)` instead.");
	}

	[Obsolete("TryGetInteractableForCollider has been deprecated. Use GetInteractableForCollider instead. (UnityUpgradable) -> GetInteractableForCollider(*)", true)]
	public XRBaseInteractable TryGetInteractableForCollider(Collider interactableCollider)
	{
		Debug.LogError("TryGetInteractableForCollider has been deprecated. Use GetInteractableForCollider instead. (UnityUpgradable) -> GetInteractableForCollider(*)", this);
		throw new NotSupportedException("TryGetInteractableForCollider has been deprecated. Use GetInteractableForCollider instead. (UnityUpgradable) -> GetInteractableForCollider(*)");
	}

	[Obsolete("GetInteractableForCollider has been deprecated. Use TryGetInteractableForCollider(Collider, out IXRInteractable) instead.", true)]
	public XRBaseInteractable GetInteractableForCollider(Collider interactableCollider)
	{
		Debug.LogError("GetInteractableForCollider has been deprecated. Use TryGetInteractableForCollider(Collider, out IXRInteractable) instead.", this);
		throw new NotSupportedException("GetInteractableForCollider has been deprecated. Use TryGetInteractableForCollider(Collider, out IXRInteractable) instead.");
	}

	[Obsolete("GetColliderToInteractableMap has been deprecated. The signature no longer matches the field used by the XRInteractionManager, so a copy is returned instead of a ref. Changes to the returned Dictionary will not be observed by the XRInteractionManager.", true)]
	public void GetColliderToInteractableMap(ref Dictionary<Collider, XRBaseInteractable> map)
	{
		Debug.LogError("GetColliderToInteractableMap has been deprecated. The signature no longer matches the field used by the XRInteractionManager, so a copy is returned instead of a ref. Changes to the returned Dictionary will not be observed by the XRInteractionManager.", this);
		throw new NotSupportedException("GetColliderToInteractableMap has been deprecated. The signature no longer matches the field used by the XRInteractionManager, so a copy is returned instead of a ref. Changes to the returned Dictionary will not be observed by the XRInteractionManager.");
	}

	[Obsolete("GetValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use GetValidTargets(IXRInteractor, List<IXRInteractable>) instead.", true)]
	public List<XRBaseInteractable> GetValidTargets(XRBaseInteractor interactor, List<XRBaseInteractable> validTargets)
	{
		Debug.LogError("GetValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use GetValidTargets(IXRInteractor, List<IXRInteractable>) instead.", this);
		throw new NotSupportedException("GetValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use GetValidTargets(IXRInteractor, List<IXRInteractable>) instead.");
	}

	[Obsolete("ForceSelect(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable) instead.", true)]
	public void ForceSelect(XRBaseInteractor interactor, XRBaseInteractable interactable)
	{
		Debug.LogError("ForceSelect(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable) instead.", this);
		throw new NotSupportedException("ForceSelect(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable) instead.");
	}

	[Obsolete("ClearInteractorSelection(XRBaseInteractor) has been deprecated. Use ClearInteractorSelection(IXRSelectInteractor, List<IXRInteractable>) instead.", true)]
	public virtual void ClearInteractorSelection(XRBaseInteractor interactor)
	{
		Debug.LogError("ClearInteractorSelection(XRBaseInteractor) has been deprecated. Use ClearInteractorSelection(IXRSelectInteractor, List<IXRInteractable>) instead.", this);
		throw new NotSupportedException("ClearInteractorSelection(XRBaseInteractor) has been deprecated. Use ClearInteractorSelection(IXRSelectInteractor, List<IXRInteractable>) instead.");
	}

	[Obsolete("CancelInteractorSelection(XRBaseInteractor) has been deprecated. Use CancelInteractorSelection(IXRSelectInteractor) instead.", true)]
	public virtual void CancelInteractorSelection(XRBaseInteractor interactor)
	{
		Debug.LogError("CancelInteractorSelection(XRBaseInteractor) has been deprecated. Use CancelInteractorSelection(IXRSelectInteractor) instead.", this);
		throw new NotSupportedException("CancelInteractorSelection(XRBaseInteractor) has been deprecated. Use CancelInteractorSelection(IXRSelectInteractor) instead.");
	}

	[Obsolete("CancelInteractableSelection(XRBaseInteractable) has been deprecated. Use CancelInteractableSelection(IXRSelectInteractable) instead.", true)]
	public virtual void CancelInteractableSelection(XRBaseInteractable interactable)
	{
		Debug.LogError("CancelInteractableSelection(XRBaseInteractable) has been deprecated. Use CancelInteractableSelection(IXRSelectInteractable) instead.", this);
		throw new NotSupportedException("CancelInteractableSelection(XRBaseInteractable) has been deprecated. Use CancelInteractableSelection(IXRSelectInteractable) instead.");
	}

	[Obsolete("ClearInteractorHover(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use ClearInteractorHover(IXRHoverInteractor, List<IXRInteractable>) instead.", true)]
	public virtual void ClearInteractorHover(XRBaseInteractor interactor, List<XRBaseInteractable> validTargets)
	{
		Debug.LogError("ClearInteractorHover(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use ClearInteractorHover(IXRHoverInteractor, List<IXRInteractable>) instead.", this);
		throw new NotSupportedException("ClearInteractorHover(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use ClearInteractorHover(IXRHoverInteractor, List<IXRInteractable>) instead.");
	}

	[Obsolete("CancelInteractorHover(XRBaseInteractor) has been deprecated. Use CancelInteractorHover(IXRHoverInteractor) instead.", true)]
	public virtual void CancelInteractorHover(XRBaseInteractor interactor)
	{
		Debug.LogError("CancelInteractorHover(XRBaseInteractor) has been deprecated. Use CancelInteractorHover(IXRHoverInteractor) instead.", this);
		throw new NotSupportedException("CancelInteractorHover(XRBaseInteractor) has been deprecated. Use CancelInteractorHover(IXRHoverInteractor) instead.");
	}

	[Obsolete("CancelInteractableHover(XRBaseInteractable) has been deprecated. Use CancelInteractableHover(IXRHoverInteractable) instead.", true)]
	public virtual void CancelInteractableHover(XRBaseInteractable interactable)
	{
		Debug.LogError("CancelInteractableHover(XRBaseInteractable) has been deprecated. Use CancelInteractableHover(IXRHoverInteractable) instead.", this);
		throw new NotSupportedException("CancelInteractableHover(XRBaseInteractable) has been deprecated. Use CancelInteractableHover(IXRHoverInteractable) instead.");
	}

	[Obsolete("SelectEnter(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.", true)]
	public virtual void SelectEnter(XRBaseInteractor interactor, XRBaseInteractable interactable)
	{
		Debug.LogError("SelectEnter(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.", this);
		throw new NotSupportedException("SelectEnter(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectEnter((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.");
	}

	[Obsolete("SelectExit(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectExit(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectExit((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.", true)]
	public virtual void SelectExit(XRBaseInteractor interactor, XRBaseInteractable interactable)
	{
		Debug.LogError("SelectExit(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectExit(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectExit((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.", this);
		throw new NotSupportedException("SelectExit(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectExit(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectExit((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.");
	}

	[Obsolete("SelectCancel(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectCancel(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectCancel((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.", true)]
	public virtual void SelectCancel(XRBaseInteractor interactor, XRBaseInteractable interactable)
	{
		Debug.LogError("SelectCancel(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectCancel(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectCancel((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.", this);
		throw new NotSupportedException("SelectCancel(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use SelectCancel(IXRSelectInteractor, IXRSelectInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `SelectCancel((IXRSelectInteractor)interactor, (IXRSelectInteractable)interactable)` instead.");
	}

	[Obsolete("HoverEnter(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverEnter(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverEnter((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.", true)]
	public virtual void HoverEnter(XRBaseInteractor interactor, XRBaseInteractable interactable)
	{
		Debug.LogError("HoverEnter(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverEnter(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverEnter((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.", this);
		throw new NotSupportedException("HoverEnter(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverEnter(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverEnter((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.");
	}

	[Obsolete("HoverExit(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverExit(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverExit((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.", true)]
	public virtual void HoverExit(XRBaseInteractor interactor, XRBaseInteractable interactable)
	{
		Debug.LogError("HoverExit(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverExit(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverExit((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.", this);
		throw new NotSupportedException("HoverExit(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverExit(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverExit((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.");
	}

	[Obsolete("HoverCancel(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverCancel(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverCancel((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.", true)]
	public virtual void HoverCancel(XRBaseInteractor interactor, XRBaseInteractable interactable)
	{
		Debug.LogError("HoverCancel(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverCancel(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverCancel((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.", this);
		throw new NotSupportedException("HoverCancel(XRBaseInteractor, XRBaseInteractable) has been deprecated. Use HoverCancel(IXRHoverInteractor, IXRHoverInteractable) instead. You may need to modify your code by casting the argument to call the intended method, such as `HoverCancel((IXRHoverInteractor)interactor, (IXRHoverInteractable)interactable)` instead.");
	}

	[Obsolete("SelectEnter(XRBaseInteractor, XRBaseInteractable, SelectEnterEventArgs) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable, SelectEnterEventArgs) instead.", true)]
	protected virtual void SelectEnter(XRBaseInteractor interactor, XRBaseInteractable interactable, SelectEnterEventArgs args)
	{
		Debug.LogError("SelectEnter(XRBaseInteractor, XRBaseInteractable, SelectEnterEventArgs) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable, SelectEnterEventArgs) instead.", this);
		throw new NotSupportedException("SelectEnter(XRBaseInteractor, XRBaseInteractable, SelectEnterEventArgs) has been deprecated. Use SelectEnter(IXRSelectInteractor, IXRSelectInteractable, SelectEnterEventArgs) instead.");
	}

	[Obsolete("SelectExit(XRBaseInteractor, XRBaseInteractable, SelectExitEventArgs) has been deprecated. Use SelectExit(IXRSelectInteractor, IXRSelectInteractable, SelectExitEventArgs) instead.", true)]
	protected virtual void SelectExit(XRBaseInteractor interactor, XRBaseInteractable interactable, SelectExitEventArgs args)
	{
		Debug.LogError("SelectExit(XRBaseInteractor, XRBaseInteractable, SelectExitEventArgs) has been deprecated. Use SelectExit(IXRSelectInteractor, IXRSelectInteractable, SelectExitEventArgs) instead.", this);
		throw new NotSupportedException("SelectExit(XRBaseInteractor, XRBaseInteractable, SelectExitEventArgs) has been deprecated. Use SelectExit(IXRSelectInteractor, IXRSelectInteractable, SelectExitEventArgs) instead.");
	}

	[Obsolete("HoverEnter(XRBaseInteractor, XRBaseInteractable, HoverEnterEventArgs) has been deprecated. Use HoverEnter(IXRHoverInteractor, IXRHoverInteractable, HoverEnterEventArgs) instead.", true)]
	protected virtual void HoverEnter(XRBaseInteractor interactor, XRBaseInteractable interactable, HoverEnterEventArgs args)
	{
		Debug.LogError("HoverEnter(XRBaseInteractor, XRBaseInteractable, HoverEnterEventArgs) has been deprecated. Use HoverEnter(IXRHoverInteractor, IXRHoverInteractable, HoverEnterEventArgs) instead.", this);
		throw new NotSupportedException("HoverEnter(XRBaseInteractor, XRBaseInteractable, HoverEnterEventArgs) has been deprecated. Use HoverEnter(IXRHoverInteractor, IXRHoverInteractable, HoverEnterEventArgs) instead.");
	}

	[Obsolete("HoverExit(XRBaseInteractor, XRBaseInteractable, HoverExitEventArgs) has been deprecated. Use HoverExit(IXRHoverInteractor, IXRHoverInteractable, HoverExitEventArgs) instead.", true)]
	protected virtual void HoverExit(XRBaseInteractor interactor, XRBaseInteractable interactable, HoverExitEventArgs args)
	{
		Debug.LogError("HoverExit(XRBaseInteractor, XRBaseInteractable, HoverExitEventArgs) has been deprecated. Use HoverExit(IXRHoverInteractor, IXRHoverInteractable, HoverExitEventArgs) instead.", this);
		throw new NotSupportedException("HoverExit(XRBaseInteractor, XRBaseInteractable, HoverExitEventArgs) has been deprecated. Use HoverExit(IXRHoverInteractor, IXRHoverInteractable, HoverExitEventArgs) instead.");
	}

	[Obsolete("InteractorSelectValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use InteractorSelectValidTargets(IXRSelectInteractor, List<IXRInteractable>) instead.", true)]
	protected virtual void InteractorSelectValidTargets(XRBaseInteractor interactor, List<XRBaseInteractable> validTargets)
	{
		Debug.LogError("InteractorSelectValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use InteractorSelectValidTargets(IXRSelectInteractor, List<IXRInteractable>) instead.", this);
		throw new NotSupportedException("InteractorSelectValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use InteractorSelectValidTargets(IXRSelectInteractor, List<IXRInteractable>) instead.");
	}

	[Obsolete("InteractorHoverValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use InteractorHoverValidTargets(IXRHoverInteractor, List<IXRInteractable>) instead.", true)]
	protected virtual void InteractorHoverValidTargets(XRBaseInteractor interactor, List<XRBaseInteractable> validTargets)
	{
		Debug.LogError("InteractorHoverValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use InteractorHoverValidTargets(IXRHoverInteractor, List<IXRInteractable>) instead.", this);
		throw new NotSupportedException("InteractorHoverValidTargets(XRBaseInteractor, List<XRBaseInteractable>) has been deprecated. Use InteractorHoverValidTargets(IXRHoverInteractor, List<IXRInteractable>) instead.");
	}
}
