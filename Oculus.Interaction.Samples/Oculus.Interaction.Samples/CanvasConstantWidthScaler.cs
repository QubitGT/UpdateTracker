using UnityEngine;

namespace Oculus.Interaction.Samples;

public class CanvasConstantWidthScaler : MonoBehaviour
{
	[SerializeField]
	private RectTransform _rect;

	private float _initialLocalScaleY;

	private float _initialWidth;

	private float _initialHeight;

	private void Start()
	{
		_initialLocalScaleY = base.transform.localScale.y;
		_initialWidth = _rect.sizeDelta.x;
		_initialHeight = _rect.sizeDelta.y;
	}

	private void Update()
	{
		base.transform.localScale = new Vector3(base.transform.localScale.x, _initialLocalScaleY * base.transform.parent.lossyScale.x / base.transform.parent.lossyScale.y, base.transform.localScale.z);
		_rect.sizeDelta = new Vector2(_initialWidth, _initialHeight * base.transform.localScale.x / base.transform.localScale.y);
	}
}
