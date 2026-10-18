using System.Collections.Generic;
using Unity.Collections;

namespace UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public interface ICurveInteractionCaster : IInteractionCaster
{
	NativeArray<Vector3> samplePoints { get; }

	Vector3 lastSamplePoint { get; }

	bool TryGetColliderTargets(XRInteractionManager interactionManager, List<Collider> colliders, List<RaycastHit> raycastHits);
}
