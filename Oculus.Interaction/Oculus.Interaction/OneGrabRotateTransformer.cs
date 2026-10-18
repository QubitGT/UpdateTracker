using System;
using UnityEngine;

namespace Oculus.Interaction;

public class OneGrabRotateTransformer : MonoBehaviour, ITransformer
{
	public enum Axis
	{
		Right,
		Up,
		Forward
	}

	[Serializable]
	public class OneGrabRotateConstraints
	{
		public FloatConstraint MinAngle;

		public FloatConstraint MaxAngle;
	}

	[SerializeField]
	[Optional]
	private Transform _pivotTransform;

	[SerializeField]
	private Axis _rotationAxis = Axis.Up;

	[SerializeField]
	private OneGrabRotateConstraints _constraints = new OneGrabRotateConstraints
	{
		MinAngle = new FloatConstraint(),
		MaxAngle = new FloatConstraint()
	};

	private float _relativeAngle;

	private float _constrainedRelativeAngle;

	private IGrabbable _grabbable;

	private Vector3 _grabPositionInPivotSpace;

	private Pose _transformPoseInPivotSpace;

	private Pose _worldPivotPose;

	private Vector3 _previousVectorInPivotSpace;

	private Quaternion _localRotation;

	private float _startAngle;

	public Transform Pivot
	{
		get
		{
			if (!(_pivotTransform != null))
			{
				return base.transform;
			}
			return _pivotTransform;
		}
	}

	public Axis RotationAxis => _rotationAxis;

	public OneGrabRotateConstraints Constraints
	{
		get
		{
			return _constraints;
		}
		set
		{
			_constraints = value;
		}
	}

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
	}

	public Pose ComputeWorldPivotPose()
	{
		if (_pivotTransform != null)
		{
			return _pivotTransform.GetPose();
		}
		Transform transform = _grabbable.Transform;
		Vector3 position = transform.position;
		Quaternion rotation = ((transform.parent != null) ? (transform.parent.rotation * _localRotation) : _localRotation);
		return new Pose(position, rotation);
	}

	public void BeginTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		if (_pivotTransform == null)
		{
			_localRotation = transform.localRotation;
		}
		Vector3 zero = Vector3.zero;
		zero[(int)_rotationAxis] = 1f;
		_worldPivotPose = ComputeWorldPivotPose();
		Vector3 planeNormal = _worldPivotPose.rotation * zero;
		Quaternion quaternion = Quaternion.Inverse(_worldPivotPose.rotation);
		Vector3 vector = pose.position - _worldPivotPose.position;
		if (Mathf.Abs(vector.magnitude) < 0.001f)
		{
			Vector3 zero2 = Vector3.zero;
			zero2[(int)(_rotationAxis + 1) % 3] = 0.001f;
			vector = _worldPivotPose.rotation * zero2;
		}
		_grabPositionInPivotSpace = quaternion * vector;
		Vector3 position = quaternion * (transform.position - _worldPivotPose.position);
		Quaternion rotation = quaternion * transform.rotation;
		_transformPoseInPivotSpace = new Pose(position, rotation);
		Vector3 vector2 = Vector3.ProjectOnPlane(_worldPivotPose.rotation * _grabPositionInPivotSpace, planeNormal);
		_previousVectorInPivotSpace = Quaternion.Inverse(_worldPivotPose.rotation) * vector2;
		_startAngle = _constrainedRelativeAngle;
		_relativeAngle = _startAngle;
		float num = ((transform.parent != null) ? transform.parent.lossyScale.x : 1f);
		_transformPoseInPivotSpace.position /= num;
	}

	public void UpdateTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		Vector3 zero = Vector3.zero;
		zero[(int)_rotationAxis] = 1f;
		_worldPivotPose = ComputeWorldPivotPose();
		Vector3 vector = _worldPivotPose.rotation * zero;
		Vector3 vector2 = Vector3.ProjectOnPlane(pose.position - _worldPivotPose.position, vector);
		Vector3 from = _worldPivotPose.rotation * _previousVectorInPivotSpace;
		_previousVectorInPivotSpace = Quaternion.Inverse(_worldPivotPose.rotation) * vector2;
		float num = Vector3.SignedAngle(from, vector2, vector);
		_relativeAngle += num;
		_constrainedRelativeAngle = _relativeAngle;
		if (Constraints.MinAngle.Constrain)
		{
			_constrainedRelativeAngle = Mathf.Max(_constrainedRelativeAngle, Constraints.MinAngle.Value);
		}
		if (Constraints.MaxAngle.Constrain)
		{
			_constrainedRelativeAngle = Mathf.Min(_constrainedRelativeAngle, Constraints.MaxAngle.Value);
		}
		Quaternion quaternion = Quaternion.AngleAxis(_constrainedRelativeAngle - _startAngle, vector);
		float num2 = ((transform.parent != null) ? transform.parent.lossyScale.x : 1f);
		Pose pose2 = new Pose(_worldPivotPose.rotation * (num2 * _transformPoseInPivotSpace.position), _worldPivotPose.rotation * _transformPoseInPivotSpace.rotation);
		Pose pose3 = new Pose(quaternion * pose2.position, quaternion * pose2.rotation);
		transform.position = _worldPivotPose.position + pose3.position;
		transform.rotation = pose3.rotation;
	}

	public void EndTransform()
	{
	}

	public void InjectOptionalPivotTransform(Transform pivotTransform)
	{
		_pivotTransform = pivotTransform;
	}

	public void InjectOptionalRotationAxis(Axis rotationAxis)
	{
		_rotationAxis = rotationAxis;
	}

	public void InjectOptionalConstraints(OneGrabRotateConstraints constraints)
	{
		_constraints = constraints;
	}
}
