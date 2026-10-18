using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.XR.Interaction.Toolkit.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[DisallowMultipleComponent]
[AddComponentMenu("XR/XR Interaction Group", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Interactors.XRInteractionGroup.html")]
[DefaultExecutionOrder(-100)]
public class XRInteractionGroup : MonoBehaviour, IXRInteractionOverrideGroup, IXRInteractionGroup, IXRGroupMember
{
	public static class GroupNames
	{
		public static readonly string k_Left = "Left";

		public static readonly string k_Right = "Right";

		public static readonly string k_Center = "Center";
	}

	[Serializable]
	internal class GroupMemberAndOverridesPair
	{
		[RequireInterface(typeof(IXRGroupMember))]
		public Object groupMember;

		[RequireInterface(typeof(IXRGroupMember))]
		public List<Object> overrideGroupMembers = new List<Object>();
	}

	[SerializeField]
	[Tooltip("The name of the interaction group, which can be used to retrieve it from the Interaction Manager.")]
	private string m_GroupName;

	[SerializeField]
	[Tooltip("The XR Interaction Manager that this Interaction Group will communicate with (will find one if not set manually).")]
	private XRInteractionManager m_InteractionManager;

	private XRInteractionManager m_RegisteredInteractionManager;

	[SerializeField]
	[Tooltip("Ordered list of Interactors or Interaction Groups that are registered with the Group on Awake.")]
	[RequireInterface(typeof(IXRGroupMember))]
	private List<Object> m_StartingGroupMembers = new List<Object>();

	[SerializeField]
	[Tooltip("Configuration for each Group Member of which other Members are able to override its interaction when they attempt to select, despite the difference in priority order.")]
	private List<GroupMemberAndOverridesPair> m_StartingInteractionOverridesMap = new List<GroupMemberAndOverridesPair>();

	private readonly RegistrationList<IXRGroupMember> m_GroupMembers = new RegistrationList<IXRGroupMember>();

	private readonly List<IXRGroupMember> m_TempGroupMembers = new List<IXRGroupMember>();

	private bool m_IsProcessingGroupMembers;

	private readonly Dictionary<IXRGroupMember, HashSet<IXRGroupMember>> m_InteractionOverridesMap = new Dictionary<IXRGroupMember, HashSet<IXRGroupMember>>();

	private readonly List<IXRInteractable> m_ValidTargets = new List<IXRInteractable>();

	private static readonly List<IXRSelectInteractable> s_InteractablesSelected = new List<IXRSelectInteractable>();

	private static readonly List<IXRHoverInteractable> s_InteractablesHovered = new List<IXRHoverInteractable>();

	public string groupName => m_GroupName;

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

	public List<Object> startingGroupMembers
	{
		get
		{
			return m_StartingGroupMembers;
		}
		set
		{
			m_StartingGroupMembers = value;
			RemoveMissingMembersFromStartingOverridesMap();
		}
	}

	public IXRInteractor activeInteractor { get; private set; }

	public IXRInteractor focusInteractor { get; private set; }

	public IXRFocusInteractable focusInteractable { get; private set; }

	internal bool isRegisteredWithInteractionManager => m_RegisteredInteractionManager != null;

	internal bool hasRegisteredStartingMembers { get; private set; }

	public event Action<InteractionGroupRegisteredEventArgs> registered;

	public event Action<InteractionGroupUnregisteredEventArgs> unregistered;

	[Conditional("UNITY_EDITOR")]
	protected virtual void Reset()
	{
	}

	protected virtual void Awake()
	{
		FindCreateInteractionManager();
		RegisterWithInteractionManager();
		if (m_GroupMembers.flushedCount > 0)
		{
			int num = 0;
			foreach (Object startingGroupMember in m_StartingGroupMembers)
			{
				if (startingGroupMember != null && startingGroupMember is IXRGroupMember groupMember)
				{
					MoveGroupMemberTo(groupMember, num++);
				}
			}
		}
		else
		{
			foreach (Object startingGroupMember2 in m_StartingGroupMembers)
			{
				if (startingGroupMember2 != null && startingGroupMember2 is IXRGroupMember groupMember2)
				{
					AddGroupMember(groupMember2);
				}
			}
		}
		if (string.IsNullOrWhiteSpace(m_GroupName))
		{
			m_GroupName = base.gameObject.name;
		}
		RemoveMissingMembersFromStartingOverridesMap();
		foreach (GroupMemberAndOverridesPair item in m_StartingInteractionOverridesMap)
		{
			Object groupMember3 = item.groupMember;
			if (groupMember3 == null || !(groupMember3 is IXRGroupMember sourceGroupMember))
			{
				continue;
			}
			foreach (Object overrideGroupMember2 in item.overrideGroupMembers)
			{
				if (overrideGroupMember2 != null && overrideGroupMember2 is IXRGroupMember overrideGroupMember)
				{
					AddInteractionOverrideForGroupMember(sourceGroupMember, overrideGroupMember);
				}
			}
		}
		hasRegisteredStartingMembers = true;
	}

	internal void RemoveMissingMembersFromStartingOverridesMap()
	{
		for (int num = m_StartingInteractionOverridesMap.Count - 1; num >= 0; num--)
		{
			GroupMemberAndOverridesPair groupMemberAndOverridesPair = m_StartingInteractionOverridesMap[num];
			if (!m_StartingGroupMembers.Contains(groupMemberAndOverridesPair.groupMember))
			{
				m_StartingInteractionOverridesMap.RemoveAt(num);
			}
			else
			{
				List<Object> overrideGroupMembers = groupMemberAndOverridesPair.overrideGroupMembers;
				for (int num2 = overrideGroupMembers.Count - 1; num2 >= 0; num2--)
				{
					if (!m_StartingGroupMembers.Contains(overrideGroupMembers[num2]))
					{
						overrideGroupMembers.RemoveAt(num2);
					}
				}
			}
		}
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
		hasRegisteredStartingMembers = false;
		m_InteractionOverridesMap.Clear();
		ClearGroupMembers();
	}

	public void AddStartingInteractionOverride(Object sourceGroupMember, Object overrideGroupMember)
	{
		if (sourceGroupMember == null)
		{
			Debug.LogError("sourceGroupMember cannot be null.");
			return;
		}
		if (overrideGroupMember == null)
		{
			Debug.LogError("overrideGroupMember cannot be null.");
			return;
		}
		if (!m_StartingGroupMembers.Contains(sourceGroupMember))
		{
			Debug.LogError($"Cannot add starting override group member for source member {sourceGroupMember} " + $"because {sourceGroupMember} is not included in the starting group members.", this);
			return;
		}
		if (!m_StartingGroupMembers.Contains(overrideGroupMember))
		{
			Debug.LogError($"Cannot add override group member {overrideGroupMember} for source member " + $"because {overrideGroupMember} is not included in the starting group members.", this);
			return;
		}
		if (TryGetStartingGroupMemberAndOverridesPair(sourceGroupMember, out var groupMemberAndOverrides))
		{
			groupMemberAndOverrides.overrideGroupMembers.Add(overrideGroupMember);
			return;
		}
		m_StartingInteractionOverridesMap.Add(new GroupMemberAndOverridesPair
		{
			groupMember = sourceGroupMember,
			overrideGroupMembers = new List<Object> { overrideGroupMember }
		});
	}

	public bool RemoveStartingInteractionOverride(Object sourceGroupMember, Object overrideGroupMember)
	{
		if (sourceGroupMember == null)
		{
			Debug.LogError("sourceGroupMember cannot be null.");
			return false;
		}
		if (TryGetStartingGroupMemberAndOverridesPair(sourceGroupMember, out var groupMemberAndOverrides))
		{
			return groupMemberAndOverrides.overrideGroupMembers.Remove(overrideGroupMember);
		}
		return false;
	}

	private bool TryGetStartingGroupMemberAndOverridesPair(Object sourceGroupMember, out GroupMemberAndOverridesPair groupMemberAndOverrides)
	{
		if (sourceGroupMember == null)
		{
			groupMemberAndOverrides = null;
			return false;
		}
		foreach (GroupMemberAndOverridesPair item in m_StartingInteractionOverridesMap)
		{
			if (!(item.groupMember != sourceGroupMember))
			{
				groupMemberAndOverrides = item;
				return true;
			}
		}
		groupMemberAndOverrides = null;
		return false;
	}

	void IXRInteractionGroup.OnRegistered(InteractionGroupRegisteredEventArgs args)
	{
		if (args.manager != m_InteractionManager)
		{
			Debug.LogWarning("An Interaction Group was registered with an unexpected XRInteractionManager." + $" {this} was expecting to communicate with \"{m_InteractionManager}\" but was registered with \"{args.manager}\".", this);
		}
		m_RegisteredInteractionManager = args.manager;
		m_GroupMembers.Flush();
		m_IsProcessingGroupMembers = true;
		foreach (IXRGroupMember item in m_GroupMembers.registeredSnapshot)
		{
			if (m_GroupMembers.IsStillRegistered(item) && item.containingGroup == null)
			{
				RegisterAsGroupMember(item);
			}
		}
		m_IsProcessingGroupMembers = false;
		this.registered?.Invoke(args);
	}

	void IXRInteractionGroup.OnBeforeUnregistered()
	{
		m_GroupMembers.Flush();
		m_IsProcessingGroupMembers = true;
		foreach (IXRGroupMember item in m_GroupMembers.registeredSnapshot)
		{
			if (m_GroupMembers.IsStillRegistered(item))
			{
				RegisterAsNonGroupMember(item);
			}
		}
		m_IsProcessingGroupMembers = false;
	}

	void IXRInteractionGroup.OnUnregistered(InteractionGroupUnregisteredEventArgs args)
	{
		if (args.manager != m_RegisteredInteractionManager)
		{
			Debug.LogWarning("An Interaction Group was unregistered from an unexpected XRInteractionManager." + $" {this} was expecting to communicate with \"{m_RegisteredInteractionManager}\" but was unregistered from \"{args.manager}\".", this);
		}
		m_RegisteredInteractionManager = null;
		this.unregistered?.Invoke(args);
	}

	public void AddGroupMember(IXRGroupMember groupMember)
	{
		if (groupMember == null)
		{
			throw new ArgumentNullException("groupMember");
		}
		if (ValidateAddGroupMember(groupMember))
		{
			if (m_IsProcessingGroupMembers)
			{
				Debug.LogWarning($"{groupMember} added while {base.name} is processing Group members. It won't be processed until the next process.", this);
			}
			if (m_GroupMembers.Register(groupMember))
			{
				RegisterAsGroupMember(groupMember);
			}
		}
	}

	public void MoveGroupMemberTo(IXRGroupMember groupMember, int newIndex)
	{
		if (groupMember == null)
		{
			throw new ArgumentNullException("groupMember");
		}
		if (!ValidateAddGroupMember(groupMember))
		{
			return;
		}
		if (m_IsProcessingGroupMembers)
		{
			Debug.LogError($"Cannot move {groupMember} while {base.name} is processing Group members.", this);
			return;
		}
		m_GroupMembers.Flush();
		if (m_GroupMembers.MoveItemImmediately(groupMember, newIndex) && groupMember.containingGroup == null)
		{
			RegisterAsGroupMember(groupMember);
		}
	}

	private bool ValidateAddGroupMember(IXRGroupMember groupMember)
	{
		if (!(groupMember is IXRInteractor) && !(groupMember is IXRInteractionGroup))
		{
			Debug.LogError($"Group member {groupMember} must be either an Interactor or an Interaction Group.", this);
			return false;
		}
		if (groupMember.containingGroup != null && groupMember.containingGroup != this)
		{
			Debug.LogError($"Cannot add/move {groupMember} because it is already part of a Group. Remove the member from the Group first.", this);
			return false;
		}
		if (groupMember is IXRInteractionGroup iXRInteractionGroup && iXRInteractionGroup.HasDependencyOnGroup(this))
		{
			Debug.LogError($"Cannot add/move {groupMember} because this would create a circular dependency of groups.", this);
			return false;
		}
		return true;
	}

	public bool RemoveGroupMember(IXRGroupMember groupMember)
	{
		if (m_GroupMembers.Unregister(groupMember))
		{
			if (activeInteractor != null && GroupMemberIsOrContainsInteractor(groupMember, activeInteractor))
			{
				activeInteractor = null;
			}
			m_InteractionOverridesMap.Remove(groupMember);
			RegisterAsNonGroupMember(groupMember);
			return true;
		}
		return false;
	}

	private bool GroupMemberIsOrContainsInteractor(IXRGroupMember groupMember, IXRInteractor interactor)
	{
		if (groupMember == interactor)
		{
			return true;
		}
		if (!(groupMember is IXRInteractionGroup iXRInteractionGroup))
		{
			return false;
		}
		iXRInteractionGroup.GetGroupMembers(m_TempGroupMembers);
		foreach (IXRGroupMember tempGroupMember in m_TempGroupMembers)
		{
			if (GroupMemberIsOrContainsInteractor(tempGroupMember, interactor))
			{
				return true;
			}
		}
		return false;
	}

	public void ClearGroupMembers()
	{
		m_GroupMembers.Flush();
		for (int num = m_GroupMembers.flushedCount - 1; num >= 0; num--)
		{
			IXRGroupMember registeredItemAt = m_GroupMembers.GetRegisteredItemAt(num);
			RemoveGroupMember(registeredItemAt);
		}
	}

	public bool ContainsGroupMember(IXRGroupMember groupMember)
	{
		return m_GroupMembers.IsRegistered(groupMember);
	}

	public void GetGroupMembers(List<IXRGroupMember> results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		m_GroupMembers.GetRegisteredItems(results);
	}

	public bool HasDependencyOnGroup(IXRInteractionGroup group)
	{
		if (group == this)
		{
			return true;
		}
		GetGroupMembers(m_TempGroupMembers);
		foreach (IXRGroupMember tempGroupMember in m_TempGroupMembers)
		{
			if (tempGroupMember is IXRInteractionGroup iXRInteractionGroup && iXRInteractionGroup.HasDependencyOnGroup(group))
			{
				return true;
			}
		}
		return false;
	}

	public void AddInteractionOverrideForGroupMember(IXRGroupMember sourceGroupMember, IXRGroupMember overrideGroupMember)
	{
		if (sourceGroupMember == null)
		{
			Debug.LogError("sourceGroupMember cannot be null.");
			return;
		}
		if (overrideGroupMember == null)
		{
			Debug.LogError("overrideGroupMember cannot be null.");
			return;
		}
		if (!(overrideGroupMember is IXRSelectInteractor) && !(overrideGroupMember is IXRInteractionOverrideGroup))
		{
			Debug.LogError($"Override group member {overrideGroupMember} must implement either " + "IXRSelectInteractor or IXRInteractionOverrideGroup.", this);
			return;
		}
		if (!ContainsGroupMember(sourceGroupMember))
		{
			Debug.LogError($"Cannot add override group member for source member {sourceGroupMember} because {sourceGroupMember} " + "is not registered with the Group. Call AddGroupMember first.", this);
			return;
		}
		if (!ContainsGroupMember(overrideGroupMember))
		{
			Debug.LogError($"Cannot add override group member {overrideGroupMember} for source member because {overrideGroupMember} " + "is not registered with the Group. Call AddGroupMember first.", this);
			return;
		}
		if (GroupMemberIsPartOfOverrideChain(overrideGroupMember, sourceGroupMember))
		{
			Debug.LogError($"Cannot add {overrideGroupMember} as an override group member for {sourceGroupMember} " + "because this would create a loop of group member overrides.", this);
			return;
		}
		if (m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var value))
		{
			value.Add(overrideGroupMember);
			return;
		}
		m_InteractionOverridesMap[sourceGroupMember] = new HashSet<IXRGroupMember> { overrideGroupMember };
	}

	public bool GroupMemberIsPartOfOverrideChain(IXRGroupMember sourceGroupMember, IXRGroupMember potentialOverrideGroupMember)
	{
		if (potentialOverrideGroupMember == sourceGroupMember)
		{
			return true;
		}
		if (!m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var value))
		{
			return false;
		}
		foreach (IXRGroupMember item in value)
		{
			if (GroupMemberIsPartOfOverrideChain(item, potentialOverrideGroupMember))
			{
				return true;
			}
		}
		return false;
	}

	public bool RemoveInteractionOverrideForGroupMember(IXRGroupMember sourceGroupMember, IXRGroupMember overrideGroupMember)
	{
		if (sourceGroupMember == null)
		{
			Debug.LogError("sourceGroupMember cannot be null.");
			return false;
		}
		if (!ContainsGroupMember(sourceGroupMember))
		{
			Debug.LogError($"Cannot remove override group member for source member {sourceGroupMember} because {sourceGroupMember} " + "is not registered with the Group.", this);
			return false;
		}
		if (m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var value))
		{
			return value.Remove(overrideGroupMember);
		}
		return false;
	}

	public bool ClearInteractionOverridesForGroupMember(IXRGroupMember sourceGroupMember)
	{
		if (sourceGroupMember == null)
		{
			Debug.LogError("sourceGroupMember cannot be null.");
			return false;
		}
		if (!ContainsGroupMember(sourceGroupMember))
		{
			Debug.LogError($"Cannot clear override group members for source member {sourceGroupMember} because {sourceGroupMember} " + "is not registered with the Group.", this);
			return false;
		}
		if (!m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var value))
		{
			return false;
		}
		value.Clear();
		return true;
	}

	public void GetInteractionOverridesForGroupMember(IXRGroupMember sourceGroupMember, HashSet<IXRGroupMember> results)
	{
		if (sourceGroupMember == null)
		{
			Debug.LogError("sourceGroupMember cannot be null.");
			return;
		}
		if (results == null)
		{
			Debug.LogError("results cannot be null.");
			return;
		}
		if (!ContainsGroupMember(sourceGroupMember))
		{
			Debug.LogError($"Cannot get override group members for source member {sourceGroupMember} because {sourceGroupMember} " + "is not registered with the Group.", this);
			return;
		}
		results.Clear();
		if (m_InteractionOverridesMap.TryGetValue(sourceGroupMember, out var value))
		{
			results.UnionWith(value);
		}
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
				m_InteractionManager.RegisterInteractionGroup(this);
			}
		}
	}

	private void UnregisterWithInteractionManager()
	{
		if (!(m_RegisteredInteractionManager == null))
		{
			m_RegisteredInteractionManager.UnregisterInteractionGroup(this);
		}
	}

	private void RegisterAsGroupMember(IXRGroupMember groupMember)
	{
		if (!(m_RegisteredInteractionManager == null))
		{
			groupMember.OnRegisteringAsGroupMember(this);
			ReRegisterGroupMemberWithInteractionManager(groupMember);
		}
	}

	private void RegisterAsNonGroupMember(IXRGroupMember groupMember)
	{
		if (!(m_RegisteredInteractionManager == null))
		{
			groupMember.OnRegisteringAsNonGroupMember();
			ReRegisterGroupMemberWithInteractionManager(groupMember);
		}
	}

	private void ReRegisterGroupMemberWithInteractionManager(IXRGroupMember groupMember)
	{
		if (m_RegisteredInteractionManager == null)
		{
			return;
		}
		if (!(groupMember is IXRInteractor interactor))
		{
			if (groupMember is IXRInteractionGroup interactionGroup)
			{
				if (m_RegisteredInteractionManager.IsRegistered(interactionGroup))
				{
					m_RegisteredInteractionManager.UnregisterInteractionGroup(interactionGroup);
					m_RegisteredInteractionManager.RegisterInteractionGroup(interactionGroup);
				}
			}
			else
			{
				Debug.LogError($"Group member {groupMember} must be either an Interactor or an Interaction Group.", this);
			}
		}
		else if (m_RegisteredInteractionManager.IsRegistered(interactor))
		{
			m_RegisteredInteractionManager.UnregisterInteractor(interactor);
			m_RegisteredInteractionManager.RegisterInteractor(interactor);
		}
	}

	void IXRInteractionGroup.PreprocessGroupMembers(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		m_GroupMembers.Flush();
		m_IsProcessingGroupMembers = true;
		foreach (IXRGroupMember item in m_GroupMembers.registeredSnapshot)
		{
			if (!m_GroupMembers.IsStillRegistered(item))
			{
				continue;
			}
			if (!(item is IXRInteractor iXRInteractor))
			{
				if (item is IXRInteractionGroup iXRInteractionGroup && m_RegisteredInteractionManager.IsRegistered(iXRInteractionGroup))
				{
					iXRInteractionGroup.PreprocessGroupMembers(updatePhase);
				}
			}
			else if (m_RegisteredInteractionManager.IsRegistered(iXRInteractor))
			{
				iXRInteractor.PreprocessInteractor(updatePhase);
			}
		}
		m_IsProcessingGroupMembers = false;
	}

	void IXRInteractionGroup.ProcessGroupMembers(XRInteractionUpdateOrder.UpdatePhase updatePhase)
	{
		m_IsProcessingGroupMembers = true;
		foreach (IXRGroupMember item in m_GroupMembers.registeredSnapshot)
		{
			if (!m_GroupMembers.IsStillRegistered(item))
			{
				continue;
			}
			if (!(item is IXRInteractor iXRInteractor))
			{
				if (item is IXRInteractionGroup iXRInteractionGroup && m_RegisteredInteractionManager.IsRegistered(iXRInteractionGroup))
				{
					iXRInteractionGroup.ProcessGroupMembers(updatePhase);
				}
			}
			else if (m_RegisteredInteractionManager.IsRegistered(iXRInteractor))
			{
				iXRInteractor.ProcessInteractor(updatePhase);
			}
		}
		m_IsProcessingGroupMembers = false;
	}

	void IXRInteractionGroup.UpdateGroupMemberInteractions()
	{
		IXRInteractor prePrioritizedInteractor = null;
		if (activeInteractor != null && m_RegisteredInteractionManager.IsRegistered(activeInteractor) && activeInteractor is IXRSelectInteractor selectInteractor && CanStartOrContinueAnySelect(selectInteractor))
		{
			prePrioritizedInteractor = activeInteractor;
		}
		((IXRInteractionGroup)this).UpdateGroupMemberInteractions(prePrioritizedInteractor, out IXRInteractor interactorThatPerformedInteraction);
		activeInteractor = interactorThatPerformedInteraction;
	}

	private bool CanStartOrContinueAnySelect(IXRSelectInteractor selectInteractor)
	{
		if (selectInteractor.keepSelectedTargetValid)
		{
			foreach (IXRSelectInteractable item in selectInteractor.interactablesSelected)
			{
				if (m_RegisteredInteractionManager.CanSelect(selectInteractor, item))
				{
					return true;
				}
			}
		}
		m_RegisteredInteractionManager.GetValidTargets(selectInteractor, m_ValidTargets);
		foreach (IXRInteractable validTarget in m_ValidTargets)
		{
			if (validTarget is IXRSelectInteractable interactable && m_RegisteredInteractionManager.CanSelect(selectInteractor, interactable))
			{
				return true;
			}
		}
		return false;
	}

	void IXRInteractionGroup.UpdateGroupMemberInteractions(IXRInteractor prePrioritizedInteractor, out IXRInteractor interactorThatPerformedInteraction)
	{
		if (((IXRInteractionOverrideGroup)this).ShouldOverrideActiveInteraction(out IXRSelectInteractor overridingInteractor))
		{
			prePrioritizedInteractor = overridingInteractor;
		}
		interactorThatPerformedInteraction = null;
		m_IsProcessingGroupMembers = true;
		foreach (IXRGroupMember item in m_GroupMembers.registeredSnapshot)
		{
			if (!m_GroupMembers.IsStillRegistered(item))
			{
				continue;
			}
			if (!(item is IXRInteractor iXRInteractor))
			{
				if (item is IXRInteractionGroup iXRInteractionGroup && m_RegisteredInteractionManager.IsRegistered(iXRInteractionGroup))
				{
					iXRInteractionGroup.UpdateGroupMemberInteractions(prePrioritizedInteractor, out var interactorThatPerformedInteraction2);
					if (interactorThatPerformedInteraction2 != null)
					{
						interactorThatPerformedInteraction = interactorThatPerformedInteraction2;
						prePrioritizedInteractor = interactorThatPerformedInteraction2;
					}
				}
			}
			else if (m_RegisteredInteractionManager.IsRegistered(iXRInteractor))
			{
				bool preventInteraction = prePrioritizedInteractor != null && iXRInteractor != prePrioritizedInteractor;
				UpdateInteractorInteractions(iXRInteractor, preventInteraction, out var performedInteraction);
				if (performedInteraction)
				{
					interactorThatPerformedInteraction = iXRInteractor;
					prePrioritizedInteractor = iXRInteractor;
				}
			}
		}
		m_IsProcessingGroupMembers = false;
		activeInteractor = interactorThatPerformedInteraction;
	}

	bool IXRInteractionOverrideGroup.ShouldOverrideActiveInteraction(out IXRSelectInteractor overridingInteractor)
	{
		overridingInteractor = null;
		if (activeInteractor == null || !TryGetOverridesForContainedInteractor(activeInteractor, out var overrideGroupMembers))
		{
			return false;
		}
		bool result = false;
		m_IsProcessingGroupMembers = true;
		foreach (IXRGroupMember item in m_GroupMembers.registeredSnapshot)
		{
			if (m_GroupMembers.IsStillRegistered(item) && overrideGroupMembers.Contains(item) && ShouldGroupMemberOverrideInteraction(activeInteractor, item, out overridingInteractor))
			{
				result = true;
				break;
			}
		}
		m_IsProcessingGroupMembers = false;
		return result;
	}

	private bool TryGetOverridesForContainedInteractor(IXRInteractor interactor, out HashSet<IXRGroupMember> overrideGroupMembers)
	{
		overrideGroupMembers = null;
		if (!(interactor is IXRGroupMember { containingGroup: var iXRInteractionGroup } iXRGroupMember))
		{
			Debug.LogError(string.Format("Interactor {0} must be a {1}.", interactor, "IXRGroupMember"), this);
			return false;
		}
		IXRGroupMember key = iXRGroupMember;
		while (iXRInteractionGroup != null && iXRInteractionGroup != this)
		{
			if (iXRInteractionGroup is IXRGroupMember iXRGroupMember2)
			{
				iXRInteractionGroup = iXRGroupMember2.containingGroup;
				key = iXRGroupMember2;
			}
			else
			{
				iXRInteractionGroup = null;
			}
		}
		if (iXRInteractionGroup == null)
		{
			Debug.LogError($"Interactor {interactor} must be contained by this group or one of its sub-groups.", this);
			return false;
		}
		return m_InteractionOverridesMap.TryGetValue(key, out overrideGroupMembers);
	}

	bool IXRInteractionOverrideGroup.ShouldAnyMemberOverrideInteraction(IXRInteractor interactingInteractor, out IXRSelectInteractor overridingInteractor)
	{
		overridingInteractor = null;
		bool result = false;
		m_IsProcessingGroupMembers = true;
		foreach (IXRGroupMember item in m_GroupMembers.registeredSnapshot)
		{
			if (m_GroupMembers.IsStillRegistered(item) && ShouldGroupMemberOverrideInteraction(interactingInteractor, item, out overridingInteractor))
			{
				result = true;
				break;
			}
		}
		m_IsProcessingGroupMembers = false;
		return result;
	}

	private bool ShouldGroupMemberOverrideInteraction(IXRInteractor interactingInteractor, IXRGroupMember overrideGroupMember, out IXRSelectInteractor overridingInteractor)
	{
		overridingInteractor = null;
		if (!(overrideGroupMember is IXRSelectInteractor iXRSelectInteractor))
		{
			if (overrideGroupMember is IXRInteractionOverrideGroup iXRInteractionOverrideGroup)
			{
				if (!m_RegisteredInteractionManager.IsRegistered(iXRInteractionOverrideGroup))
				{
					return false;
				}
				if (iXRInteractionOverrideGroup.ShouldAnyMemberOverrideInteraction(interactingInteractor, out overridingInteractor))
				{
					return true;
				}
			}
		}
		else
		{
			if (!m_RegisteredInteractionManager.IsRegistered(iXRSelectInteractor))
			{
				return false;
			}
			if (ShouldInteractorOverrideInteraction(interactingInteractor, iXRSelectInteractor))
			{
				overridingInteractor = iXRSelectInteractor;
				return true;
			}
		}
		return false;
	}

	private bool ShouldInteractorOverrideInteraction(IXRInteractor interactingInteractor, IXRSelectInteractor overridingInteractor)
	{
		IXRSelectInteractor iXRSelectInteractor = interactingInteractor as IXRSelectInteractor;
		IXRHoverInteractor iXRHoverInteractor = interactingInteractor as IXRHoverInteractor;
		m_RegisteredInteractionManager.GetValidTargets(overridingInteractor, m_ValidTargets);
		foreach (IXRInteractable validTarget in m_ValidTargets)
		{
			if (validTarget is IXRSelectInteractable interactable && m_RegisteredInteractionManager.CanSelect(overridingInteractor, interactable))
			{
				if (iXRSelectInteractor != null && iXRSelectInteractor.IsSelecting(interactable))
				{
					return true;
				}
				if (iXRHoverInteractor != null && validTarget is IXRHoverInteractable interactable2 && iXRHoverInteractor.IsHovering(interactable2))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void UpdateInteractorInteractions(IXRInteractor interactor, bool preventInteraction, out bool performedInteraction)
	{
		performedInteraction = false;
		using (XRInteractionManager.s_GetValidTargetsMarker.Auto())
		{
			m_RegisteredInteractionManager.GetValidTargets(interactor, m_ValidTargets);
		}
		IXRSelectInteractor iXRSelectInteractor = interactor as IXRSelectInteractor;
		IXRHoverInteractor iXRHoverInteractor = interactor as IXRHoverInteractor;
		if (iXRSelectInteractor != null)
		{
			using (XRInteractionManager.s_EvaluateInvalidSelectionsMarker.Auto())
			{
				if (preventInteraction)
				{
					ClearAllInteractorSelections(iXRSelectInteractor);
				}
				else
				{
					m_RegisteredInteractionManager.ClearInteractorSelection(iXRSelectInteractor, m_ValidTargets);
				}
			}
		}
		if (iXRHoverInteractor != null)
		{
			using (XRInteractionManager.s_EvaluateInvalidHoversMarker.Auto())
			{
				if (preventInteraction)
				{
					ClearAllInteractorHovers(iXRHoverInteractor);
				}
				else
				{
					m_RegisteredInteractionManager.ClearInteractorHover(iXRHoverInteractor, m_ValidTargets);
				}
			}
		}
		if (preventInteraction)
		{
			return;
		}
		if (iXRSelectInteractor != null)
		{
			using (XRInteractionManager.s_EvaluateValidSelectionsMarker.Auto())
			{
				m_RegisteredInteractionManager.InteractorSelectValidTargets(iXRSelectInteractor, m_ValidTargets);
			}
			if (iXRSelectInteractor.hasSelection || (interactor is IUIInteractor interactor2 && TrackedDeviceGraphicRaycaster.IsPokeInteractingWithUI(interactor2)))
			{
				performedInteraction = true;
			}
		}
		if (iXRHoverInteractor != null)
		{
			using (XRInteractionManager.s_EvaluateValidHoversMarker.Auto())
			{
				m_RegisteredInteractionManager.InteractorHoverValidTargets(iXRHoverInteractor, m_ValidTargets);
			}
			if (iXRHoverInteractor.hasHover)
			{
				performedInteraction = true;
			}
		}
	}

	private void ClearAllInteractorSelections(IXRSelectInteractor selectInteractor)
	{
		if (selectInteractor.interactablesSelected.Count != 0)
		{
			s_InteractablesSelected.Clear();
			s_InteractablesSelected.AddRange(selectInteractor.interactablesSelected);
			for (int num = s_InteractablesSelected.Count - 1; num >= 0; num--)
			{
				IXRSelectInteractable interactable = s_InteractablesSelected[num];
				m_RegisteredInteractionManager.SelectExit(selectInteractor, interactable);
			}
		}
	}

	private void ClearAllInteractorHovers(IXRHoverInteractor hoverInteractor)
	{
		if (hoverInteractor.interactablesHovered.Count != 0)
		{
			s_InteractablesHovered.Clear();
			s_InteractablesHovered.AddRange(hoverInteractor.interactablesHovered);
			for (int num = s_InteractablesHovered.Count - 1; num >= 0; num--)
			{
				IXRHoverInteractable interactable = s_InteractablesHovered[num];
				m_RegisteredInteractionManager.HoverExit(hoverInteractor, interactable);
			}
		}
	}

	public void OnFocusEntering(FocusEnterEventArgs args)
	{
		focusInteractable = args.interactableObject;
		focusInteractor = args.interactorObject;
	}

	public void OnFocusExiting(FocusExitEventArgs args)
	{
		if (focusInteractable == args.interactableObject)
		{
			focusInteractable = null;
			focusInteractor = null;
		}
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
}
