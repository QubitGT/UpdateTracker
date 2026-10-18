using UnityEngine;

namespace Liv.Lck.Smoothing;

[DefaultExecutionOrder(1000)]
public class LckSimpleStabilizer : MonoBehaviour
{
	public Transform _stabilizationTarget;

	public Transform _targetToFollow;

	public float _followTime = 0.1f;

	public float _rotateTime = 0.1f;

	private Vector3 _velocity = Vector3.zero;

	private Vector3 _rotationVelocity = Vector3.zero;

	private Vector3 _lastPosition;

	private Quaternion _lastRotation;

	private void OnEnable()
	{
		_lastPosition = _targetToFollow.position;
		_lastRotation = _targetToFollow.rotation;
	}

	private void LateUpdate()
	{
		_stabilizationTarget.position = Vector3.SmoothDamp(_lastPosition, _targetToFollow.position, ref _velocity, _followTime);
		_stabilizationTarget.rotation = SmoothingUtils.SmoothDampQuaternion(_lastRotation, _targetToFollow.rotation, ref _rotationVelocity, _rotateTime);
		_lastPosition = _stabilizationTarget.position;
		_lastRotation = _stabilizationTarget.rotation;
	}

	public void ReachTargetInstantly()
	{
		_stabilizationTarget.position = _targetToFollow.position;
		_stabilizationTarget.rotation = _targetToFollow.rotation;
	}
}
