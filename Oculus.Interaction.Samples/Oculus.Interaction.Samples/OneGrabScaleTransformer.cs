using System;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class OneGrabScaleTransformer : MonoBehaviour, ITransformer
{
	[Serializable]
	public class OneGrabScaleConstraints
	{
		public bool IgnoreFixedAxes;

		public bool ConstrainXYAspectRatio;

		public FloatConstraint MinX;

		public FloatConstraint MaxX;

		public FloatConstraint MinY;

		public FloatConstraint MaxY;

		public FloatConstraint MinZ;

		public FloatConstraint MaxZ;
	}

	[SerializeField]
	[Tooltip("Constraints for allowable values on different axes")]
	private OneGrabScaleConstraints _constraints = new OneGrabScaleConstraints
	{
		IgnoreFixedAxes = false,
		ConstrainXYAspectRatio = false,
		MinX = new FloatConstraint(),
		MaxX = new FloatConstraint(),
		MinY = new FloatConstraint(),
		MaxY = new FloatConstraint(),
		MinZ = new FloatConstraint(),
		MaxZ = new FloatConstraint()
	};

	private Vector3 _initialLocalScale;

	private Vector3 _initialLocalPosition;

	private IGrabbable _grabbable;

	public OneGrabScaleConstraints Constraints
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

	public void BeginTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		_initialLocalPosition = transform.InverseTransformPointUnscaled(pose.position);
		_initialLocalScale = transform.localScale;
	}

	public void UpdateTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		Vector3 vector = transform.InverseTransformPointUnscaled(pose.position);
		float num = _initialLocalScale.x * vector.x / _initialLocalPosition.x;
		float num2 = _initialLocalScale.y * vector.y / _initialLocalPosition.y;
		float num3 = _initialLocalScale.z * vector.z / _initialLocalPosition.z;
		if (_constraints.MinX.Constrain)
		{
			num = Mathf.Max(_constraints.MinX.Value, num);
		}
		if (_constraints.MinY.Constrain)
		{
			num2 = Mathf.Max(_constraints.MinY.Value, num2);
		}
		if (_constraints.MinZ.Constrain)
		{
			num3 = Mathf.Max(_constraints.MinZ.Value, num3);
		}
		if (_constraints.MaxX.Constrain)
		{
			num = Mathf.Min(_constraints.MaxX.Value, num);
		}
		if (_constraints.MaxY.Constrain)
		{
			num2 = Mathf.Min(_constraints.MaxY.Value, num2);
		}
		if (_constraints.MaxZ.Constrain)
		{
			num3 = Mathf.Min(_constraints.MaxZ.Value, num3);
		}
		if (_constraints.IgnoreFixedAxes)
		{
			if (_constraints.MinX.Constrain && _constraints.MaxX.Constrain && _constraints.MinX.Value == _constraints.MaxX.Value)
			{
				num = transform.localScale.x;
			}
			if (_constraints.MinY.Constrain && _constraints.MaxY.Constrain && _constraints.MinY.Value == _constraints.MaxY.Value)
			{
				num2 = transform.localScale.y;
			}
			if (_constraints.MinZ.Constrain && _constraints.MaxZ.Constrain && _constraints.MinZ.Value == _constraints.MaxZ.Value)
			{
				num3 = transform.localScale.z;
			}
		}
		if (_constraints.ConstrainXYAspectRatio)
		{
			if (num / num2 < _initialLocalScale.x / _initialLocalScale.y)
			{
				num2 = num * _initialLocalScale.y / _initialLocalScale.x;
			}
			else
			{
				num = num2 * _initialLocalScale.x / _initialLocalScale.y;
			}
		}
		transform.localScale = new Vector3(num, num2, num3);
	}

	public void EndTransform()
	{
	}
}
