using System.Collections.Generic;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRActivateInteractor : IXRInteractor
{
	bool shouldActivate { get; }

	bool shouldDeactivate { get; }

	void GetActivateTargets(List<IXRActivateInteractable> targets);
}
