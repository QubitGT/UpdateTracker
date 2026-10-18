using System;
using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRInteractor
{
	InteractionLayerMask interactionLayers { get; }

	InteractorHandedness handedness { get; }

	Transform transform { get; }

	event Action<InteractorRegisteredEventArgs> registered;

	event Action<InteractorUnregisteredEventArgs> unregistered;

	Transform GetAttachTransform(IXRInteractable interactable);

	void GetValidTargets(List<IXRInteractable> targets);

	void OnRegistered(InteractorRegisteredEventArgs args);

	void OnUnregistered(InteractorUnregisteredEventArgs args);

	void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase);

	void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase);
}
