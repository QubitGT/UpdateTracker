using UnityEngine;

namespace Oculus.Interaction.Samples;

public class ConstantRotation : MonoBehaviour
{
	[SerializeField]
	private float _rotationSpeed;

	[SerializeField]
	private Vector3 _localAxis = Vector3.up;

	public float RotationSpeed
	{
		get
		{
			return _rotationSpeed;
		}
		set
		{
			_rotationSpeed = value;
		}
	}

	public Vector3 LocalAxis
	{
		get
		{
			return _localAxis;
		}
		set
		{
			_localAxis = value;
		}
	}

	protected virtual void Update()
	{
		base.transform.Rotate(_localAxis, _rotationSpeed * Time.deltaTime, Space.Self);
	}
}
