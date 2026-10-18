using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRInteractionGroup
{
	string groupName { get; }

	IXRInteractor activeInteractor { get; }

	IXRInteractor focusInteractor { get; }

	IXRFocusInteractable focusInteractable { get; }

	event Action<InteractionGroupRegisteredEventArgs> registered;

	event Action<InteractionGroupUnregisteredEventArgs> unregistered;

	void OnRegistered(InteractionGroupRegisteredEventArgs args);

	void OnBeforeUnregistered();

	void OnUnregistered(InteractionGroupUnregisteredEventArgs args);

	void AddGroupMember(IXRGroupMember groupMember);

	void MoveGroupMemberTo(IXRGroupMember groupMember, int newIndex);

	bool RemoveGroupMember(IXRGroupMember groupMember);

	void ClearGroupMembers();

	bool ContainsGroupMember(IXRGroupMember groupMember);

	void GetGroupMembers(List<IXRGroupMember> results);

	bool HasDependencyOnGroup(IXRInteractionGroup group);

	void PreprocessGroupMembers(XRInteractionUpdateOrder.UpdatePhase updatePhase);

	void ProcessGroupMembers(XRInteractionUpdateOrder.UpdatePhase updatePhase);

	void UpdateGroupMemberInteractions();

	void UpdateGroupMemberInteractions(IXRInteractor prePrioritizedInteractor, out IXRInteractor interactorThatPerformedInteraction);

	void OnFocusEntering(FocusEnterEventArgs args);

	void OnFocusExiting(FocusExitEventArgs args);
}
