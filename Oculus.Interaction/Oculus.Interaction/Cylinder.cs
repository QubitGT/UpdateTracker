using UnityEngine;

namespace Oculus.Interaction;

public class Cylinder : MonoBehaviour
{
	[Tooltip("The radius of the cylinder.")]
	[SerializeField]
	private float _radius = 1f;

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}
}
