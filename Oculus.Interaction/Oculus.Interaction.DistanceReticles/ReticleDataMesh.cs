using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public class ReticleDataMesh : MonoBehaviour, IReticleData
{
	[Tooltip("The mesh of the GameObject to outline.")]
	[SerializeField]
	private MeshFilter _filter;

	public MeshFilter Filter
	{
		get
		{
			return _filter;
		}
		set
		{
			_filter = value;
		}
	}

	public Transform Target => _filter.transform;

	public Vector3 ProcessHitPoint(Vector3 hitPoint)
	{
		return _filter.transform.position;
	}
}
