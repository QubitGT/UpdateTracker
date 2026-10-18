using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRInteractionStrengthInteractable : IXRInteractable
{
	IReadOnlyBindableVariable<float> largestInteractionStrength { get; }

	float GetInteractionStrength(IXRInteractor interactor);

	void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase);
}
