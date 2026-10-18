using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Navigation;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class ModioMaxSizeFitter : MonoBehaviour, ILayoutElement
{
	[SerializeField]
	[Tooltip("Leaving an axis at 0 will ignore it")]
	private Vector2 _maxSize;

	[SerializeField]
	private int _layoutPriority = 10;

	private bool _calculatingNestedSize;

	public float minWidth => -1f;

	public float preferredWidth => GetPreferredSize(RectTransform.Axis.Horizontal);

	public float flexibleWidth => -1f;

	public float minHeight => -1f;

	public float preferredHeight => GetPreferredSize(RectTransform.Axis.Vertical);

	public float flexibleHeight => -1f;

	public int layoutPriority
	{
		get
		{
			if (!_calculatingNestedSize)
			{
				return _layoutPriority;
			}
			return -1;
		}
	}

	private float GetPreferredSize(RectTransform.Axis axis)
	{
		float num = _maxSize[(int)axis];
		if (num < 0.01f || layoutPriority < 0)
		{
			return -1f;
		}
		RectTransform rect = (RectTransform)base.transform;
		_ = layoutPriority;
		_calculatingNestedSize = true;
		float preferredSize = LayoutUtility.GetPreferredSize(rect, (int)axis);
		_calculatingNestedSize = false;
		return Mathf.Min(preferredSize, num);
	}

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
	}
}
