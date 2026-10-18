using UnityEngine;

namespace Oculus.Interaction.Samples;

public class StayInView : MonoBehaviour
{
	[SerializeField]
	private Transform _eyeCenter;

	[SerializeField]
	private float _extraDistanceForward;

	[SerializeField]
	private bool _zeroOutEyeHeight = true;

	private void Update()
	{
		base.transform.rotation = Quaternion.identity;
		base.transform.position = _eyeCenter.position;
		base.transform.Rotate(0f, _eyeCenter.rotation.eulerAngles.y, 0f, Space.Self);
		base.transform.position = _eyeCenter.position + base.transform.forward.normalized * _extraDistanceForward;
		if (_zeroOutEyeHeight)
		{
			base.transform.position = new Vector3(base.transform.position.x, 0f, base.transform.position.z);
		}
	}
}
