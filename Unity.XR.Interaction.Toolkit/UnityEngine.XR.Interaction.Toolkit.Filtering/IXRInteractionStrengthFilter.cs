using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public interface IXRInteractionStrengthFilter
{
	bool canProcess { get; }

	float Process(IXRInteractor interactor, IXRInteractable interactable, float interactionStrength);
}
