using System;
using UnityEngine;

namespace Oculus.Interaction;

[Obsolete("Use GrabFreeTransformer instead")]
public class TwoGrabFreeTransformer : MonoBehaviour, ITransformer
{
	[Serializable]
	public class TwoGrabFreeConstraints
	{
		[Tooltip("If true then the constraints are relative to the initial/base scale of the object if false, constraints are absolute with respect to the object's selected axes.")]
		public bool ConstraintsAreRelative;

		public FloatConstraint MinScale;

		public FloatConstraint MaxScale;

		public bool ConstrainXScale = true;

		public bool ConstrainYScale;

		public bool ConstrainZScale;
	}

	public struct TwoGrabFreeState
	{
		public Pose Center;

		public float Distance;
	}

	[SerializeField]
	private TwoGrabFreeConstraints _constraints;

	private IGrabbable _grabbable;

	private Vector3 _baseScale;

	private Pose _localToTarget;

	private float _localMagnitudeToTarget;

	private Pose _prevGrabA;

	private Pose _prevGrabB;

	private Quaternion _prevGrabRotation;

	public TwoGrabFreeConstraints Constraints
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
		_baseScale = _grabbable.Transform.localScale;
	}

	public void BeginTransform()
	{
		Transform transform = _grabbable.Transform;
		Pose prevGrabA = _grabbable.GrabPoints[0];
		Pose prevGrabB = _grabbable.GrabPoints[1];
		_prevGrabA = prevGrabA;
		_prevGrabB = prevGrabB;
		TwoGrabFreeState twoGrabFreeState = TwoGrabFreeInit(prevGrabA.position, prevGrabB.position);
		_prevGrabRotation = twoGrabFreeState.Center.rotation;
		_localToTarget = TransformerUtils.WorldToLocalPose(twoGrabFreeState.Center, transform.worldToLocalMatrix);
		_localMagnitudeToTarget = TransformerUtils.WorldToLocalMagnitude(twoGrabFreeState.Distance, transform.worldToLocalMatrix);
	}

	public void UpdateTransform()
	{
		Transform transform = _grabbable.Transform;
		Pose pose = _grabbable.GrabPoints[0];
		Pose pose2 = _grabbable.GrabPoints[1];
		TwoGrabFreeState twoGrabFreeState = TwoGrabFree(_prevGrabRotation, _prevGrabA, _prevGrabB, pose, pose2);
		float num = TransformerUtils.LocalToWorldMagnitude(_localMagnitudeToTarget, transform.localToWorldMatrix);
		float targetScale = ((num != 0f) ? (twoGrabFreeState.Distance / num) : 1f) * transform.localScale.x;
		float num2 = ConstrainScale(targetScale);
		transform.localScale = num2 / transform.localScale.x * transform.localScale;
		Pose pose3 = TransformerUtils.AlignLocalToWorldPose(transform.localToWorldMatrix, _localToTarget, twoGrabFreeState.Center);
		transform.position = pose3.position;
		transform.rotation = pose3.rotation;
		_prevGrabRotation = twoGrabFreeState.Center.rotation;
		_prevGrabA = pose;
		_prevGrabB = pose2;
	}

	private float ConstrainScale(float targetScale)
	{
		float num = targetScale;
		if (_constraints.MinScale.Constrain)
		{
			Vector3 vector = _constraints.MinScale.Value * (_constraints.ConstraintsAreRelative ? _baseScale : Vector3.one);
			if (_constraints.ConstrainXScale)
			{
				num = Mathf.Max(num, vector.x);
			}
			if (_constraints.ConstrainYScale)
			{
				num = Mathf.Max(num, vector.y);
			}
			if (_constraints.ConstrainZScale)
			{
				num = Mathf.Max(num, vector.z);
			}
		}
		if (_constraints.MinScale.Constrain)
		{
			Vector3 vector2 = _constraints.MaxScale.Value * (_constraints.ConstraintsAreRelative ? _baseScale : Vector3.one);
			if (_constraints.ConstrainXScale)
			{
				num = Mathf.Min(num, vector2.x);
			}
			if (_constraints.ConstrainYScale)
			{
				num = Mathf.Min(num, vector2.y);
			}
			if (_constraints.ConstrainZScale)
			{
				num = Mathf.Min(num, vector2.z);
			}
		}
		return num;
	}

	public static TwoGrabFreeState TwoGrabFreeInit(Vector3 a, Vector3 b)
	{
		Vector3 position = Vector3.Lerp(a, b, 0.5f);
		Vector3 vector = b - a;
		Vector3 upwards = (((double)Mathf.Abs(Vector3.Dot(vector, Vector3.up)) < 0.999) ? Vector3.up : Vector3.right);
		Quaternion rotation = Quaternion.LookRotation(vector, upwards);
		TwoGrabFreeState result = default(TwoGrabFreeState);
		result.Center = new Pose(position, rotation);
		result.Distance = vector.magnitude;
		return result;
	}

	public static TwoGrabFreeState TwoGrabFree(Quaternion initialRotation, Pose prevA, Pose prevB, Pose newA, Pose newB)
	{
		Vector3.Lerp(prevA.position, prevB.position, 0.5f);
		Vector3 position = Vector3.Lerp(newA.position, newB.position, 0.5f);
		Vector3 fromDirection = prevB.position - prevA.position;
		Vector3 vector = newB.position - newA.position;
		Quaternion quaternion = Quaternion.FromToRotation(fromDirection, vector);
		Quaternion normalized = Quaternion.Slerp(b: newA.rotation * Quaternion.Inverse(prevA.rotation), a: Quaternion.identity, t: 0.5f).normalized;
		Quaternion normalized2 = Quaternion.Slerp(b: newB.rotation * Quaternion.Inverse(prevB.rotation), a: Quaternion.identity, t: 0.5f).normalized;
		Vector3 upwards = quaternion * normalized * normalized2 * initialRotation * Vector3.up;
		Quaternion normalized3 = Quaternion.LookRotation(vector, upwards).normalized;
		TwoGrabFreeState result = default(TwoGrabFreeState);
		result.Center = new Pose(position, normalized3);
		result.Distance = (newB.position - newA.position).magnitude;
		return result;
	}

	public void MarkAsBaseScale()
	{
		_baseScale = _grabbable.Transform.localScale;
	}

	public void EndTransform()
	{
	}

	public void InjectOptionalConstraints(TwoGrabFreeConstraints constraints)
	{
		_constraints = constraints;
	}
}
