using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public interface IXRSelectFilter
{
	bool canProcess { get; }

	bool Process(IXRSelectInteractor interactor, IXRSelectInteractable interactable);
}
