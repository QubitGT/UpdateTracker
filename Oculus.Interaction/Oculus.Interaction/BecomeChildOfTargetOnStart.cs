using UnityEngine;

namespace Oculus.Interaction;

public class BecomeChildOfTargetOnStart : MonoBehaviour
{
	[SerializeField]
	private Transform _target;

	[SerializeField]
	private bool _keepWorldPosition = true;

	protected virtual void Start()
	{
		base.transform.SetParent(_target, _keepWorldPosition);
	}

	public void InjectAllChildToTransform(Transform target)
	{
		InjectTarget(target);
	}

	public void InjectTarget(Transform target)
	{
		_target = target;
	}

	public void InjectOptionalKeepWorldPosition(bool keepWorldPosition)
	{
		_keepWorldPosition = keepWorldPosition;
	}
}
