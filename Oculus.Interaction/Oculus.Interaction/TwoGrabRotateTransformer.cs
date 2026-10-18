using System;
using UnityEngine;

namespace Oculus.Interaction;

public class TwoGrabRotateTransformer : MonoBehaviour, ITransformer
{
	public enum Axis
	{
		Right,
		Up,
		Forward
	}

	[Serializable]
	public class TwoGrabRotateConstraints
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
	private TwoGrabRotateConstraints _constraints;

	private float _relativeAngle;

	private float _constrainedRelativeAngle;

	private IGrabbable _grabbable;

	private Vector3 _previousHandsVectorOnPlane;

	private Transform PivotTransform
	{
		get
		{
			if (!(_pivotTransform != null))
			{
				return _grabbable.Transform;
			}
			return _pivotTransform;
		}
	}

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
	}

	public void BeginTransform()
	{
		Vector3 planeNormal = CalculateRotationAxisInWorldSpace();
		_previousHandsVectorOnPlane = CalculateHandsVectorOnPlane(planeNormal);
		_relativeAngle = _constrainedRelativeAngle;
	}

	public void UpdateTransform()
	{
		Vector3 vector = CalculateRotationAxisInWorldSpace();
		Vector3 vector2 = CalculateHandsVectorOnPlane(vector);
		float num = Vector3.SignedAngle(_previousHandsVectorOnPlane, vector2, vector);
		float constrainedRelativeAngle = _constrainedRelativeAngle;
		_relativeAngle += num;
		_constrainedRelativeAngle = _relativeAngle;
		if (_constraints.MinAngle.Constrain)
		{
			_constrainedRelativeAngle = Mathf.Max(_constrainedRelativeAngle, _constraints.MinAngle.Value);
		}
		if (_constraints.MaxAngle.Constrain)
		{
			_constrainedRelativeAngle = Mathf.Min(_constrainedRelativeAngle, _constraints.MaxAngle.Value);
		}
		num = _constrainedRelativeAngle - constrainedRelativeAngle;
		_grabbable.Transform.RotateAround(PivotTransform.position, vector, num);
		_previousHandsVectorOnPlane = vector2;
	}

	public void EndTransform()
	{
	}

	private Vector3 CalculateRotationAxisInWorldSpace()
	{
		Vector3 zero = Vector3.zero;
		zero[(int)_rotationAxis] = 1f;
		return PivotTransform.TransformDirection(zero);
	}

	private Vector3 CalculateHandsVectorOnPlane(Vector3 planeNormal)
	{
		Vector3[] array = new Vector3[2]
		{
			Vector3.ProjectOnPlane(_grabbable.GrabPoints[0].position, planeNormal),
			Vector3.ProjectOnPlane(_grabbable.GrabPoints[1].position, planeNormal)
		};
		return array[1] - array[0];
	}

	public void InjectOptionalPivotTransform(Transform pivotTransform)
	{
		_pivotTransform = pivotTransform;
	}

	public void InjectOptionalRotationAxis(Axis rotationAxis)
	{
		_rotationAxis = rotationAxis;
	}

	public void InjectOptionalConstraints(TwoGrabRotateConstraints constraints)
	{
		_constraints = constraints;
	}
}
