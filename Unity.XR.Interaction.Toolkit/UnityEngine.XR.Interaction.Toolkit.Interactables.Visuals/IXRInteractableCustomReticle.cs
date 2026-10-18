using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace UnityEngine.XR.Interaction.Toolkit.Interactables.Visuals;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRInteractableCustomReticle
{
	void OnReticleAttached(XRBaseInteractable interactable, IXRCustomReticleProvider reticleProvider);

	void OnReticleDetaching();
}
