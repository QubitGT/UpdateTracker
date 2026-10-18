using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Utils;

public class FilteredTransform : MonoBehaviour
{
	[SerializeField]
	private Transform _sourceTransform;

	[SerializeField]
	private bool _filterPosition;

	[SerializeField]
	private OneEuroFilterPropertyBlock _positionFilterProperties = new OneEuroFilterPropertyBlock(2f, 3f);

	[SerializeField]
	private bool _filterRotation;

	[SerializeField]
	private OneEuroFilterPropertyBlock _rotationFilterProperties = new OneEuroFilterPropertyBlock(2f, 3f);

	private IOneEuroFilter<Vector3> _positionFilter;

	private IOneEuroFilter<Quaternion> _rotationFilter;

	protected virtual void Start()
	{
		_positionFilter = OneEuroFilter.CreateVector3();
		_rotationFilter = OneEuroFilter.CreateQuaternion();
	}

	protected virtual void Update()
	{
		if (_filterPosition)
		{
			_ = _sourceTransform.position;
			_positionFilter.SetProperties(in _positionFilterProperties);
			base.transform.position = _positionFilter.Step(_sourceTransform.position, Time.deltaTime);
		}
		else
		{
			base.transform.position = _sourceTransform.position;
		}
		if (_filterRotation)
		{
			_rotationFilter.SetProperties(in _rotationFilterProperties);
			base.transform.rotation = _rotationFilter.Step(_sourceTransform.rotation, Time.deltaTime);
		}
		else
		{
			base.transform.rotation = _sourceTransform.rotation;
		}
	}

	public void InjectAllFilteredTransform(Transform sourceTransform)
	{
		InjectSourceTransform(sourceTransform);
	}

	public void InjectSourceTransform(Transform sourceTransform)
	{
		_sourceTransform = sourceTransform;
	}
}
