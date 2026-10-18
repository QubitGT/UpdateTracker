using UnityEngine;

namespace Oculus.Interaction.Surfaces;

public class CylinderClipper : MonoBehaviour, ICylinderClipper
{
	[Tooltip("The rotation of the center of the clip area around the y axis, in degrees.")]
	[SerializeField]
	[Range(-180f, 180f)]
	private float _rotation;

	[Tooltip("The arc degrees of the clip area, centered at the rotation value.")]
	[SerializeField]
	[Range(0f, 360f)]
	private float _arcDegrees = 360f;

	[Tooltip("The bottom extent of the clip area, along the y axis.")]
	[SerializeField]
	private float _bottom = -1f;

	[Tooltip("The top extent of the clip area, along the y axis.")]
	[SerializeField]
	private float _top = 1f;

	public float ArcDegrees
	{
		get
		{
			return _arcDegrees;
		}
		set
		{
			_arcDegrees = value;
		}
	}

	public float Rotation
	{
		get
		{
			return _rotation;
		}
		set
		{
			_rotation = value;
		}
	}

	public float Bottom
	{
		get
		{
			return _bottom;
		}
		set
		{
			_bottom = value;
		}
	}

	public float Top
	{
		get
		{
			return _top;
		}
		set
		{
			_top = value;
		}
	}

	public bool GetCylinderSegment(out CylinderSegment segment)
	{
		segment = new CylinderSegment(_rotation, _arcDegrees, _bottom, _top);
		return base.isActiveAndEnabled;
	}
}
