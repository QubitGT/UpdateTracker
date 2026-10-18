using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public class ReticleDataIcon : MonoBehaviour, IReticleData
{
	[Tooltip("The Mesh Renderer of the GameObject that the icon can appear on.")]
	[SerializeField]
	[Optional]
	private MeshRenderer _renderer;

	[Tooltip("The icon's appearance.")]
	[SerializeField]
	[Optional]
	private Texture _customIcon;

	[SerializeField]
	[Range(0f, 1f)]
	private float _snappiness;

	public Texture CustomIcon
	{
		get
		{
			return _customIcon;
		}
		set
		{
			_customIcon = value;
		}
	}

	public float Snappiness
	{
		get
		{
			return _snappiness;
		}
		set
		{
			_snappiness = value;
		}
	}

	public Vector3 GetTargetSize()
	{
		if (_renderer != null)
		{
			return _renderer.bounds.size;
		}
		return base.transform.localScale;
	}

	public Vector3 ProcessHitPoint(Vector3 hitPoint)
	{
		return Vector3.Lerp(hitPoint, base.transform.position, _snappiness);
	}

	public void InjectOptionalRenderer(MeshRenderer renderer)
	{
		_renderer = renderer;
	}
}
