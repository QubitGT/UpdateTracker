using UnityEngine;

namespace Oculus.Interaction.Samples;

public class AnchoredWorldSpaceDistanceScaler : MonoBehaviour
{
	public enum ScalingMode
	{
		TwoDimensional,
		ThreeDimensional
	}

	[SerializeField]
	private Transform _parentAnchor;

	[SerializeField]
	private Transform _localAnchor;

	[SerializeField]
	[Tooltip("Choose whether content should be scaled as two- or three-dimensional")]
	private ScalingMode _scalingMode;

	private Vector3 _parentAnchorOffset;

	private Vector3 _originalLocalScale;

	private Vector3 _originalParentLocalScale;

	private Vector3 _originalCombinedScale;

	private void Start()
	{
		_parentAnchorOffset = _parentAnchor.InverseTransformPoint(_localAnchor.position);
		_originalLocalScale = base.transform.localScale;
		_originalParentLocalScale = base.transform.parent.localScale;
		_originalCombinedScale = Vector3.Scale(_originalParentLocalScale, _originalLocalScale);
	}

	private void LateUpdate()
	{
		Vector3 position = _parentAnchor.TransformPoint(_parentAnchorOffset);
		Vector3 vector = base.transform.InverseTransformPoint(position);
		Vector3 localScale = base.transform.localScale;
		localScale.Scale(new Vector3((Mathf.Abs(_localAnchor.localPosition.x) < Mathf.Epsilon) ? 1f : (vector.x / _localAnchor.localPosition.x), (Mathf.Abs(_localAnchor.localPosition.y) < Mathf.Epsilon) ? 1f : (vector.y / _localAnchor.localPosition.y), (Mathf.Abs(_localAnchor.localPosition.z) < Mathf.Epsilon) ? 1f : (vector.z / _localAnchor.localPosition.z)));
		if (_scalingMode == ScalingMode.ThreeDimensional)
		{
			Vector3 vector2 = Vector3.Scale(base.transform.parent.localScale, localScale);
			if (vector2.x / vector2.y > _originalCombinedScale.x / _originalCombinedScale.y)
			{
				float num = vector2.y / _originalCombinedScale.y;
				localScale.x = _originalCombinedScale.x * num / base.transform.parent.localScale.x;
				localScale.z = _originalCombinedScale.z * num / base.transform.parent.localScale.z;
			}
			else
			{
				float num2 = vector2.x / _originalCombinedScale.x;
				localScale.y = _originalCombinedScale.y * num2 / base.transform.parent.localScale.y;
				localScale.z = _originalCombinedScale.z * num2 / base.transform.parent.localScale.z;
			}
		}
		else
		{
			localScale.z = _originalParentLocalScale.z * _originalLocalScale.z / base.transform.parent.localScale.z;
		}
		base.transform.localScale = localScale;
	}
}
