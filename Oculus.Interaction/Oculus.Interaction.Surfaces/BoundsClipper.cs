using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class BoundsClipper : MonoBehaviour, IBoundsClipper
{
	[Tooltip("The offset of the bounding box center relative to the transform origin, in local space.")]
	[SerializeField]
	private Vector3 _position = Vector3.zero;

	[Tooltip("The size of the bounding box in local space.")]
	[SerializeField]
	private Vector3 _size = Vector3.one;

	public Vector3 Position
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public Vector3 Size
	{
		get
		{
			return _size;
		}
		set
		{
			_size = value;
		}
	}

	public bool GetLocalBounds(Transform localTo, out Bounds bounds)
	{
		Vector3 center = localTo.InverseTransformPoint(base.transform.TransformPoint(Position));
		Vector3 size = localTo.InverseTransformVector(base.transform.TransformVector(_size));
		bounds = new Bounds(center, size);
		return base.isActiveAndEnabled;
	}
}
