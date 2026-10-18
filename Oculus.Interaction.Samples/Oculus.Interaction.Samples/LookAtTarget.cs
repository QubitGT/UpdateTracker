using UnityEngine;

namespace Oculus.Interaction.Samples;

public class LookAtTarget : MonoBehaviour
{
	[SerializeField]
	private Transform _toRotate;

	[SerializeField]
	private Transform _target;

	protected virtual void Start()
	{
	}

	protected virtual void Update()
	{
		Vector3 normalized = (_target.position - _toRotate.position).normalized;
		_toRotate.LookAt(_toRotate.position - normalized, Vector3.up);
	}
}
