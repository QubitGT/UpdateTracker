using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRInteractable
{
	InteractionLayerMask interactionLayers { get; }

	List<Collider> colliders { get; }

	Transform transform { get; }

	event Action<InteractableRegisteredEventArgs> registered;

	event Action<InteractableUnregisteredEventArgs> unregistered;

	Transform GetAttachTransform(IXRInteractor interactor);

	void OnRegistered(InteractableRegisteredEventArgs args);

	void OnUnregistered(InteractableUnregisteredEventArgs args);

	void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase);

	float GetDistanceSqrToInteractor(IXRInteractor interactor);
}
