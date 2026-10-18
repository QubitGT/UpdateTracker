using System;
using UnityEngine;

namespace Oculus.Interaction;

public class OneGrabTranslateTransformer : MonoBehaviour, ITransformer
{
	[Serializable]
	public class OneGrabTranslateConstraints
	{
		public bool ConstraintsAreRelative;

		public FloatConstraint MinX;

		public FloatConstraint MaxX;

		public FloatConstraint MinY;

		public FloatConstraint MaxY;

		public FloatConstraint MinZ;

		public FloatConstraint MaxZ;
	}

	[SerializeField]
	private OneGrabTranslateConstraints _constraints = new OneGrabTranslateConstraints
	{
		MinX = new FloatConstraint(),
		MaxX = new FloatConstraint(),
		MinY = new FloatConstraint(),
		MaxY = new FloatConstraint(),
		MinZ = new FloatConstraint(),
		MaxZ = new FloatConstraint()
	};

	private OneGrabTranslateConstraints _parentConstraints;

	private Vector3 _initialPosition;

	private IGrabbable _grabbable;

	private Pose _localToTarget;

	public OneGrabTranslateConstraints Constraints
	{
		get
		{
			return _constraints;
		}
		set
		{
			_constraints = value;
			GenerateParentConstraints();
		}
	}

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
		_initialPosition = _grabbable.Transform.localPosition;
		GenerateParentConstraints();
	}

	private void GenerateParentConstraints()
	{
		if (!_constraints.ConstraintsAreRelative)
		{
			_parentConstraints = _constraints;
			return;
		}
		_parentConstraints = new OneGrabTranslateConstraints();
		_parentConstraints.MinX = new FloatConstraint();
		_parentConstraints.MinY = new FloatConstraint();
		_parentConstraints.MinZ = new FloatConstraint();
		_parentConstraints.MaxX = new FloatConstraint();
		_parentConstraints.MaxY = new FloatConstraint();
		_parentConstraints.MaxZ = new FloatConstraint();
		if (_constraints.MinX.Constrain)
		{
			_parentConstraints.MinX.Constrain = true;
			_parentConstraints.MinX.Value = _constraints.MinX.Value + _initialPosition.x;
		}
		if (_constraints.MaxX.Constrain)
		{
			_parentConstraints.MaxX.Constrain = true;
			_parentConstraints.MaxX.Value = _constraints.MaxX.Value + _initialPosition.x;
		}
		if (_constraints.MinY.Constrain)
		{
			_parentConstraints.MinY.Constrain = true;
			_parentConstraints.MinY.Value = _constraints.MinY.Value + _initialPosition.y;
		}
		if (_constraints.MaxY.Constrain)
		{
			_parentConstraints.MaxY.Constrain = true;
			_parentConstraints.MaxY.Value = _constraints.MaxY.Value + _initialPosition.y;
		}
		if (_constraints.MinZ.Constrain)
		{
			_parentConstraints.MinZ.Constrain = true;
			_parentConstraints.MinZ.Value = _constraints.MinZ.Value + _initialPosition.z;
		}
		if (_constraints.MaxZ.Constrain)
		{
			_parentConstraints.MaxZ.Constrain = true;
			_parentConstraints.MaxZ.Value = _constraints.MaxZ.Value + _initialPosition.z;
		}
	}

	public void BeginTransform()
	{
		Pose worldPose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		_localToTarget = TransformerUtils.WorldToLocalPose(worldPose, transform.worldToLocalMatrix);
	}

	public void UpdateTransform()
	{
		Transform obj = _grabbable.Transform;
		Pose pose = _grabbable.GrabPoints[0];
		Pose pose2 = TransformerUtils.AlignLocalToWorldPose(world: new Pose(rotation: obj.rotation * _localToTarget.rotation, position: pose.position), localToWorld: obj.localToWorldMatrix, local: _localToTarget);
		obj.position = pose2.position;
		obj.rotation = pose2.rotation;
		ConstrainTransform();
	}

	private void ConstrainTransform()
	{
		Transform obj = _grabbable.Transform;
		Vector3 localPosition = obj.localPosition;
		if (_parentConstraints.MinX.Constrain)
		{
			localPosition.x = Mathf.Max(localPosition.x, _parentConstraints.MinX.Value);
		}
		if (_parentConstraints.MaxX.Constrain)
		{
			localPosition.x = Mathf.Min(localPosition.x, _parentConstraints.MaxX.Value);
		}
		if (_parentConstraints.MinY.Constrain)
		{
			localPosition.y = Mathf.Max(localPosition.y, _parentConstraints.MinY.Value);
		}
		if (_parentConstraints.MaxY.Constrain)
		{
			localPosition.y = Mathf.Min(localPosition.y, _parentConstraints.MaxY.Value);
		}
		if (_parentConstraints.MinZ.Constrain)
		{
			localPosition.z = Mathf.Max(localPosition.z, _parentConstraints.MinZ.Value);
		}
		if (_parentConstraints.MaxZ.Constrain)
		{
			localPosition.z = Mathf.Min(localPosition.z, _parentConstraints.MaxZ.Value);
		}
		obj.localPosition = localPosition;
	}

	public void EndTransform()
	{
	}

	public void InjectOptionalConstraints(OneGrabTranslateConstraints constraints)
	{
		_constraints = constraints;
	}
}
