using UnityEngine;

namespace Oculus.Interaction.Samples;

public class ListSnapPoseDelegateRoundedBoxVisual : MonoBehaviour
{
	[SerializeField]
	private ListSnapPoseDelegate _listSnapPoseDelegate;

	[SerializeField]
	private RoundedBoxProperties _properties;

	[SerializeField]
	private SnapInteractable _snapInteractable;

	[SerializeField]
	private float _minSize;

	[SerializeField]
	private ProgressCurve _curve;

	private float _targetWidth;

	private float _startWidth;

	protected virtual void LateUpdate()
	{
		float num = Mathf.Max(_listSnapPoseDelegate.Size, _minSize);
		if (num != _targetWidth)
		{
			_targetWidth = num;
			_curve.Start();
			_startWidth = _properties.Width;
		}
		_properties.Width = Mathf.Lerp(_startWidth, _targetWidth, _curve.Progress());
		_properties.BorderColor = ((_snapInteractable.Interactors.Count != _snapInteractable.SelectingInteractors.Count) ? new Color(1f, 1f, 1f, 1f) : new Color(1f, 1f, 1f, 0.5f));
	}
}
