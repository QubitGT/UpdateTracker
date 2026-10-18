using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public interface IXRHoverFilter
{
	bool canProcess { get; }

	bool Process(IXRHoverInteractor interactor, IXRHoverInteractable interactable);
}
