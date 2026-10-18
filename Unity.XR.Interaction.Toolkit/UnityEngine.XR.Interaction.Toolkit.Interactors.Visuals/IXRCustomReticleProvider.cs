using UnityEngine.Scripting.APIUpdating;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
public interface IXRCustomReticleProvider
{
	bool AttachCustomReticle(GameObject reticleInstance);

	bool RemoveCustomReticle();
}
