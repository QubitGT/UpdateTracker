using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Panels;

public class ModioPopupPositioning : MonoBehaviour, ILayoutSelfController, ILayoutController
{
	[SerializeField]
	private RectTransform _containWithin;

	[SerializeField]
	private RectTransform _target;

	[SerializeField]
	private RectOffset _padding = new RectOffset();

	private static readonly Vector3[] FourCornersArray = new Vector3[4];

	public void PositionNextTo(RectTransform target)
	{
		_target = target;
		LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
	}

	public void SetLayoutHorizontal()
	{
		SetLayout(RectTransform.Axis.Horizontal);
	}

	public void SetLayoutVertical()
	{
		SetLayout(RectTransform.Axis.Vertical);
	}

	private void SetLayout(RectTransform.Axis axis)
	{
		RectTransform rectTransform = (RectTransform)base.transform;
		float preferredSize = LayoutUtility.GetPreferredSize(rectTransform, (int)axis);
		rectTransform.SetSizeWithCurrentAnchors(axis, preferredSize);
		if (_target == null)
		{
			return;
		}
		GetMinMax(_target, axis, out var min, out var max);
		GetMinMax(_containWithin, axis, out var min2, out var max2);
		int num = ((axis == RectTransform.Axis.Horizontal) ? _padding.horizontal : _padding.vertical);
		Vector3 position = rectTransform.position;
		if (axis == RectTransform.Axis.Horizontal)
		{
			if (max2 > max + preferredSize + (float)num)
			{
				position.x = max + (float)_padding.left;
			}
			else
			{
				position.x = min - (float)_padding.right - preferredSize;
			}
		}
		else
		{
			position.y = Mathf.Max(min - (float)_padding.top, min2 + preferredSize + (float)_padding.bottom);
		}
		rectTransform.position = position;
	}

	private void GetMinMax(RectTransform rectTransform, RectTransform.Axis axis, out float min, out float max)
	{
		rectTransform.GetWorldCorners(FourCornersArray);
		min = float.MaxValue;
		max = float.MinValue;
		Vector3[] fourCornersArray = FourCornersArray;
		for (int i = 0; i < fourCornersArray.Length; i++)
		{
			Vector3 vector = fourCornersArray[i];
			float b = ((axis == RectTransform.Axis.Horizontal) ? vector.x : vector.y);
			min = Mathf.Min(min, b);
			max = Mathf.Max(max, b);
		}
	}
}
