using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UnityEngine.XR.Interaction.Toolkit.Filtering;

public interface IXRTargetFilter
{
	bool canProcess { get; }

	void Link(IXRInteractor interactor);

	void Unlink(IXRInteractor interactor);

	void Process(IXRInteractor interactor, List<IXRInteractable> targets, List<IXRInteractable> results);
}
