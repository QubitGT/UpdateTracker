using System;
using UnityEngine;

namespace Oculus.Interaction;

[Obsolete("Use GrabFreeTransformer instead")]
public class OneGrabFreeTransformer : MonoBehaviour, ITransformer
{
	[SerializeField]
	private TransformerUtils.PositionConstraints _positionConstraints = new TransformerUtils.PositionConstraints
	{
		XAxis = default(TransformerUtils.ConstrainedAxis),
		YAxis = default(TransformerUtils.ConstrainedAxis),
		ZAxis = default(TransformerUtils.ConstrainedAxis)
	};

	[SerializeField]
	private TransformerUtils.RotationConstraints _rotationConstraints = new TransformerUtils.RotationConstraints
	{
		XAxis = default(TransformerUtils.ConstrainedAxis),
		YAxis = default(TransformerUtils.ConstrainedAxis),
		ZAxis = default(TransformerUtils.ConstrainedAxis)
	};

	private IGrabbable _grabbable;

	private Pose _grabDeltaInLocalSpace;

	private TransformerUtils.PositionConstraints _parentConstraints;

	private Pose _localToTarget;

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
		Vector3 localPosition = _grabbable.Transform.localPosition;
		_parentConstraints = TransformerUtils.GenerateParentConstraints(_positionConstraints, localPosition);
	}

	public void BeginTransform()
	{
		Pose worldPose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		_localToTarget = TransformerUtils.WorldToLocalPose(worldPose, transform.worldToLocalMatrix);
	}

	public void UpdateTransform()
	{
		Transform transform = _grabbable.Transform;
		Pose world = _grabbable.GrabPoints[0];
		Pose pose = TransformerUtils.AlignLocalToWorldPose(transform.localToWorldMatrix, _localToTarget, world);
		transform.rotation = TransformerUtils.GetConstrainedTransformRotation(pose.rotation, _rotationConstraints);
		transform.position = TransformerUtils.GetConstrainedTransformPosition(pose.position, _parentConstraints, transform.parent);
	}

	public void EndTransform()
	{
	}
}
