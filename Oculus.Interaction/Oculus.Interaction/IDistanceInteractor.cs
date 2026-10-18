using UnityEngine;

namespace Oculus.Interaction;

public interface IDistanceInteractor : IInteractorView
{
	Pose Origin { get; }

	Vector3 HitPoint { get; }

	IRelativeToRef DistanceInteractable { get; }
}
