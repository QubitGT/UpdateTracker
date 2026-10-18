using UnityEngine;

[RequireComponent(typeof(Camera))]
public class DroneFakeCameraMimicer : MonoBehaviour
{
	[SerializeField]
	private Camera _target;

	private Camera _mimicer;

	private void Awake()
	{
		_mimicer = GetComponent<Camera>();
	}

	private void LateUpdate()
	{
		if (_target == null)
		{
			Debug.LogWarning("No target assigned to DroneFakeCameraMimicer");
		}
		else
		{
			_mimicer.fieldOfView = _target.fieldOfView;
		}
	}
}
