using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public interface IReticleData
{
	Vector3 ProcessHitPoint(Vector3 hitPoint);
}
