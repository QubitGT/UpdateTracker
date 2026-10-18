using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRReticleDirectionProvider
{
	void GetReticleDirection(IXRInteractor interactor, Vector3 hitNormal, out Vector3 reticleUp, out Vector3? optionalReticleForward);
}
