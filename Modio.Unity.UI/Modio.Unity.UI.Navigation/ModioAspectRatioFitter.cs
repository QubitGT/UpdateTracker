using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Navigation;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class ModioAspectRatioFitter : MonoBehaviour, ILayoutSelfController, ILayoutController
{
	[SerializeField]
	private float _aspectRatio = 1.7777778f;

	[SerializeField]
	private RectOffset _margin;

	[SerializeField]
	private Vector2 _additionalPadding;

	[SerializeField]
	private Vector2 _maxSize;

	private DrivenRectTransformTracker _tracker;

	private bool _delayedSetDirty;

	private void OnEnable()
	{
		UpdateRect();
	}

	private void OnRectTransformDimensionsChange()
	{
		UpdateRect();
	}

	private void Update()
	{
		if (_delayedSetDirty)
		{
			_delayedSetDirty = false;
			UpdateRect();
		}
	}

	private void OnValidate()
	{
		_delayedSetDirty = true;
	}

	private void UpdateRect()
	{
		_tracker.Clear();
		RectTransform rectTransform = (RectTransform)base.transform;
		Vector2 vector = ((RectTransform)rectTransform.parent).rect.size - new Vector2(_margin.horizontal, _margin.vertical);
		if (_maxSize.x > 1f)
		{
			vector.x = Mathf.Min(vector.x, _maxSize.x);
		}
		if (_maxSize.y > 1f)
		{
			vector.y = Mathf.Min(vector.y, _maxSize.y);
		}
		Vector2 vector2 = vector - _additionalPadding;
		if (vector2.y * _aspectRatio < vector2.x)
		{
			vector2.x = vector2.y * _aspectRatio;
		}
		else
		{
			vector2.y = vector2.x / _aspectRatio;
		}
		vector = vector2 + _additionalPadding;
		_tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDelta);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, vector.x);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, vector.y);
	}

	public void SetLayoutHorizontal()
	{
	}

	public void SetLayoutVertical()
	{
	}
}
