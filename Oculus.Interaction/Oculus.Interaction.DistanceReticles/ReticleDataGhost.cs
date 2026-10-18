using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public class ReticleDataGhost : MonoBehaviour, IReticleData
{
	[Tooltip("The GameObject that the ghost hand can interact with.")]
	[SerializeField]
	[Optional]
	private Transform _targetPoint;

	public Vector3 ProcessHitPoint(Vector3 hitPoint)
	{
		if (!(_targetPoint != null))
		{
			return base.transform.position;
		}
		return _targetPoint.position;
	}
}
