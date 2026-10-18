using UnityEngine;
using UnityEngine.UI;

namespace Oculus.Interaction.Samples;

public class CarouselView : MonoBehaviour
{
	[SerializeField]
	private RectTransform _viewport;

	[SerializeField]
	private RectTransform _content;

	[SerializeField]
	private AnimationCurve _easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	[SerializeField]
	[Optional]
	private GameObject _emptyCarouselVisuals;

	private int _currentChildIndex;

	private float _scrollVal;

	public int CurrentChildIndex => _currentChildIndex;

	public RectTransform ContentArea => _content;

	protected virtual void Start()
	{
	}

	public void ScrollRight()
	{
		if (_content.childCount > 1)
		{
			if (_currentChildIndex > 0)
			{
				RectTransform currentChild = GetCurrentChild();
				_content.GetChild(0).SetAsLastSibling();
				LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
				ScrollToChild(currentChild, 1f);
			}
			else
			{
				_currentChildIndex++;
			}
			_scrollVal = Time.time;
		}
	}

	public void ScrollLeft()
	{
		if (_content.childCount > 1)
		{
			if (_currentChildIndex < _content.childCount - 1)
			{
				RectTransform currentChild = GetCurrentChild();
				_content.GetChild(_content.childCount - 1).SetAsFirstSibling();
				LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
				ScrollToChild(currentChild, 1f);
			}
			else
			{
				_currentChildIndex--;
			}
			_scrollVal = Time.time;
		}
	}

	private RectTransform GetCurrentChild()
	{
		return _content.GetChild(_currentChildIndex) as RectTransform;
	}

	private void ScrollToChild(RectTransform child, float amount01)
	{
		if (!(child == null))
		{
			amount01 = Mathf.Clamp01(amount01);
			Vector3 vector = _viewport.TransformPoint(_viewport.rect.center);
			Vector3 vector2 = child.TransformPoint(child.rect.center) - vector;
			if (vector2.sqrMagnitude > float.Epsilon)
			{
				Vector3 b = _content.position - vector2;
				float t = Mathf.Clamp01(_easeCurve.Evaluate(amount01));
				_content.position = Vector3.Lerp(_content.position, b, t);
			}
		}
	}

	protected virtual void Update()
	{
		_currentChildIndex = Mathf.Clamp(_currentChildIndex, 0, _content.childCount - 1);
		bool flag = _content.childCount > 0;
		if (flag)
		{
			RectTransform currentChild = GetCurrentChild();
			ScrollToChild(currentChild, Time.time - _scrollVal);
		}
		if (_emptyCarouselVisuals != null)
		{
			_emptyCarouselVisuals.SetActive(!flag);
		}
	}
}
