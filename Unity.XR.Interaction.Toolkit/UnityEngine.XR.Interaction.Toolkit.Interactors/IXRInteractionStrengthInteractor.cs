using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRInteractionStrengthInteractor : IXRInteractor
{
	IReadOnlyBindableVariable<float> largestInteractionStrength { get; }

	float GetInteractionStrength(IXRInteractable interactable);

	void ProcessInteractionStrength(XRInteractionUpdateOrder.UpdatePhase updatePhase);
}
