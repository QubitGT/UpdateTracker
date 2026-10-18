using UnityEngine;

namespace Oculus.Interaction.Samples;

public class ParentScaleInverter : MonoBehaviour
{
	private Vector3 _initialLocalScale;

	private Vector3 _initialParentScale;

	private void Start()
	{
		_initialLocalScale = base.transform.localScale;
		_initialParentScale = base.transform.parent.localScale;
	}

	private void LateUpdate()
	{
		base.transform.localScale = new Vector3(_initialParentScale.x * _initialLocalScale.x / base.transform.parent.localScale.x, _initialParentScale.y * _initialLocalScale.y / base.transform.parent.localScale.y, _initialParentScale.z * _initialLocalScale.z / base.transform.parent.localScale.z);
	}
}
