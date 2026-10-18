using System;
using UnityEngine;

namespace Oculus.Interaction;

public class TwoGrabPlaneTransformer : MonoBehaviour, ITransformer
{
	[Serializable]
	public class TwoGrabPlaneConstraints
	{
		public FloatConstraint MaxScale;

		public FloatConstraint MinScale;

		public FloatConstraint MaxY;

		public FloatConstraint MinY;
	}

	public struct TwoGrabPlaneState
	{
		public Pose Center;

		public float PlanarDistance;
	}

	[SerializeField]
	[Optional]
	private Transform _planeTransform;

	[SerializeField]
	[Optional]
	private Vector3 _localPlaneNormal = new Vector3(0f, 1f, 0f);

	[SerializeField]
	private TwoGrabPlaneConstraints _constraints;

	private IGrabbable _grabbable;

	private Pose _localToTarget;

	private float _localMagnitudeToTarget;

	public TwoGrabPlaneConstraints Constraints
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

	private Vector3 WorldPlaneNormal()
	{
		return ((_planeTransform != null) ? _planeTransform : _grabbable.Transform).TransformDirection(_localPlaneNormal).normalized;
	}

	public void BeginTransform()
	{
		Transform transform = _grabbable.Transform;
		Pose pose = _grabbable.GrabPoints[0];
		Pose pose2 = _grabbable.GrabPoints[1];
		TwoGrabPlaneState twoGrabPlaneState = TwoGrabPlane(planeNormal: WorldPlaneNormal(), p0: pose.position, p1: pose2.position);
		_localToTarget = TransformerUtils.WorldToLocalPose(twoGrabPlaneState.Center, transform.worldToLocalMatrix);
		_localMagnitudeToTarget = TransformerUtils.WorldToLocalMagnitude(twoGrabPlaneState.PlanarDistance, transform.worldToLocalMatrix);
	}

	public void UpdateTransform()
	{
		Transform transform = _grabbable.Transform;
		Pose pose = _grabbable.GrabPoints[0];
		Pose pose2 = _grabbable.GrabPoints[1];
		Vector3 vector = WorldPlaneNormal();
		TwoGrabPlaneState twoGrabPlaneState = TwoGrabPlane(pose.position, pose2.position, vector);
		float num = TransformerUtils.LocalToWorldMagnitude(_localMagnitudeToTarget, transform.localToWorldMatrix);
		float num2 = ((num != 0f) ? (twoGrabPlaneState.PlanarDistance / num) : 1f) * transform.localScale.x;
		if (_constraints.MinScale.Constrain)
		{
			num2 = Mathf.Max(_constraints.MinScale.Value, num2);
		}
		if (_constraints.MaxScale.Constrain)
		{
			num2 = Mathf.Min(_constraints.MaxScale.Value, num2);
		}
		transform.localScale = num2 / transform.localScale.x * transform.localScale;
		Pose pose3 = TransformerUtils.AlignLocalToWorldPose(transform.localToWorldMatrix, _localToTarget, twoGrabPlaneState.Center);
		transform.position = pose3.position;
		transform.rotation = pose3.rotation;
		transform.position = TransformerUtils.ConstrainAlongDirection(transform.position, (transform.parent != null) ? transform.parent.position : Vector3.zero, vector, _constraints.MinY, _constraints.MaxY);
	}

	public void EndTransform()
	{
	}

	public static TwoGrabPlaneState TwoGrabPlane(Vector3 p0, Vector3 p1, Vector3 planeNormal)
	{
		Vector3 position = p0 * 0.5f + p1 * 0.5f;
		Vector3 vector = Vector3.ProjectOnPlane(p0, planeNormal);
		Vector3 forward = Vector3.ProjectOnPlane(p1, planeNormal) - vector;
		Quaternion rotation = Quaternion.LookRotation(forward, planeNormal);
		TwoGrabPlaneState result = default(TwoGrabPlaneState);
		result.Center = new Pose(position, rotation);
		result.PlanarDistance = forward.magnitude;
		return result;
	}

	public void InjectOptionalPlaneTransform(Transform planeTransform)
	{
		_planeTransform = planeTransform;
	}

	public void InjectOptionalConstraints(TwoGrabPlaneConstraints constraints)
	{
		_constraints = constraints;
	}
}
