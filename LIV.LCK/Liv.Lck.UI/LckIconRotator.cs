using UnityEngine;

namespace Liv.Lck.UI;

public class LckIconRotator : MonoBehaviour
{
	[SerializeField]
	private float _rotationOffset;

	[SerializeField]
	private Transform _iconTransform;

	public void Rotate()
	{
		float z = _iconTransform.localEulerAngles.z + _rotationOffset;
		_iconTransform.localEulerAngles = new Vector3(0f, 0f, z);
	}
}
