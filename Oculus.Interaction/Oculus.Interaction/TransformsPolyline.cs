using UnityEngine;

namespace Oculus.Interaction;

public class TransformsPolyline : MonoBehaviour, IPolyline
{
	[SerializeField]
	private Transform[] _transforms;

	protected bool _started;

	public int PointsCount => _transforms.Length;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	public Vector3 PointAtIndex(int index)
	{
		return _transforms[index].position;
	}

	public void InjectAllTransformsPolyline(Transform[] transforms)
	{
		InjectTransforms(transforms);
	}

	public void InjectTransforms(Transform[] transforms)
	{
		_transforms = transforms;
	}
}
