using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public interface IInteractionCaster
{
	bool isInitialized { get; }

	Transform castOrigin { get; set; }

	Transform effectiveCastOrigin { get; }

	bool TryGetColliderTargets(XRInteractionManager interactionManager, List<Collider> targets);
}
