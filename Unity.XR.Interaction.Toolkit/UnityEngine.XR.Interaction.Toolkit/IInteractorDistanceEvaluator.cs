using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit;

public interface IInteractorDistanceEvaluator
{
	float EvaluateDistance(IXRInteractor interactor, IXRInteractable interactable);
}
