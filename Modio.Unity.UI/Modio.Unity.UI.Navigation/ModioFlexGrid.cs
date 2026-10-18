using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Navigation;

public class ModioFlexGrid : LayoutGroup
{
	[SerializeField]
	protected float m_Spacing;

	[SerializeField]
	protected bool m_ChildForceExpandWidth;

	[SerializeField]
	protected bool m_ChildForceExpandHeight;

	[SerializeField]
	protected bool m_ChildControlWidth = true;

	[SerializeField]
	protected bool m_ChildControlHeight = true;

	[SerializeField]
	protected bool m_ChildScaleWidth;

	[SerializeField]
	protected bool m_ChildScaleHeight;

	[SerializeField]
	protected bool m_ReverseArrangement;

	public override void CalculateLayoutInputHorizontal()
	{
		base.CalculateLayoutInputHorizontal();
		CalcAlongAxis(0, isVertical: false);
		SetLayoutInputForAxis(0f, 0f, 1f, 0);
	}

	public override void CalculateLayoutInputVertical()
	{
		SetLayoutInputForAxis(0f, 100f, 0f, 1);
		CalcAlongAxis(1, isVertical: false);
	}

	public override void SetLayoutHorizontal()
	{
		SetChildrenAlongAxis(0);
	}

	public override void SetLayoutVertical()
	{
		SetChildrenAlongAxis(1);
	}

	private void CalcAlongAxis(int axis, bool isVertical)
	{
		float num = ((axis == 0) ? base.padding.horizontal : base.padding.vertical);
		bool flag = ((axis == 0) ? m_ChildScaleWidth : m_ChildScaleHeight);
		float num2 = num;
		float num3 = num;
		float num4 = 0f;
		bool flag2 = isVertical ^ (axis == 1);
		int count = base.rectChildren.Count;
		float num5 = 0f;
		float num6 = 0f;
		float num7 = 0f;
		float num8 = 0f;
		float num9 = 0f;
		float num10 = 0f;
		float num11 = base.rectTransform.rect.width - (float)base.padding.horizontal;
		for (int i = 0; i < count; i++)
		{
			RectTransform rectTransform = base.rectChildren[i];
			GetChildSizes(rectTransform, 0, m_ChildControlWidth, m_ChildForceExpandWidth, out var min, out var preferred, out var flexible);
			GetChildSizes(rectTransform, 1, m_ChildControlHeight, m_ChildForceExpandHeight, out var min2, out var preferred2, out var flexible2);
			float num12 = ((axis == 0) ? min : min2);
			float num13 = ((axis == 0) ? preferred : preferred2);
			float num14 = ((axis == 0) ? flexible : flexible2);
			if (flag)
			{
				float num15 = rectTransform.localScale[axis];
				num12 *= num15;
				num13 *= num15;
				num14 *= num15;
			}
			num5 += min;
			num6 += preferred;
			num7 += flexible;
			num8 = Mathf.Max(min2, num8);
			num9 = Mathf.Max(preferred2, num9);
			num10 = Mathf.Max(flexible2, num10);
			if (num6 > num11)
			{
				if (axis == 1)
				{
					num2 += num8 + m_Spacing;
					num3 += num9 + m_Spacing;
					num4 += num10;
				}
				num5 = min;
				num6 = preferred;
				num7 = flexible;
			}
			if (axis == 0)
			{
				num2 = Mathf.Max(num5 + num, num2);
				num3 = Mathf.Max(num6 + num, num3);
				num4 = Mathf.Max(num7, num4);
			}
			if (flag2)
			{
				num2 = Mathf.Max(num12 + num, num2);
				num3 = Mathf.Max(num13 + num, num3);
				num4 = Mathf.Max(num14, num4);
			}
			else
			{
				num2 += num12 + m_Spacing;
				num3 += num13 + m_Spacing;
				num4 += num14;
			}
		}
		if (!flag2 && base.rectChildren.Count > 0)
		{
			num2 -= m_Spacing;
			num3 -= m_Spacing;
		}
		num3 = Mathf.Max(num2, num3);
		SetLayoutInputForAxis(num2, num3, num4, axis);
	}

	private void SetChildrenAlongAxis(int axis)
	{
		float num = base.rectTransform.rect.size[axis];
		int num2 = (m_ReverseArrangement ? (base.rectChildren.Count - 1) : 0);
		int num3 = ((!m_ReverseArrangement) ? base.rectChildren.Count : 0);
		int num4 = ((!m_ReverseArrangement) ? 1 : (-1));
		float num5 = 0f;
		float num6 = base.rectTransform.rect.width - (float)base.padding.horizontal;
		float num7 = ((axis == 0) ? base.padding.left : base.padding.top);
		if (num - GetTotalPreferredSize(axis) > 0f && GetTotalFlexibleSize(axis) == 0f)
		{
			num7 = GetStartOffset(axis, GetTotalPreferredSize(axis) - (float)((axis == 0) ? base.padding.horizontal : base.padding.vertical));
		}
		for (int i = num2; m_ReverseArrangement ? (i >= num3) : (i < num3); i += num4)
		{
			RectTransform rectTransform = base.rectChildren[i];
			float num8 = (m_ChildControlWidth ? LayoutUtility.GetPreferredSize(rectTransform, 0) : rectTransform.sizeDelta.x);
			float num9 = (m_ChildControlHeight ? LayoutUtility.GetPreferredSize(rectTransform, 1) : rectTransform.sizeDelta.y);
			num5 += num8;
			if (num5 > num6)
			{
				num7 = ((axis != 1) ? ((float)base.padding.left) : (num7 + (num9 + m_Spacing)));
				num5 = num8;
			}
			if (axis == 1)
			{
				SetChildAlongAxisWithScale(rectTransform, axis, num7, num9, 1f);
				continue;
			}
			SetChildAlongAxisWithScale(rectTransform, axis, num7, num8, 1f);
			num7 += num8 + m_Spacing;
		}
	}

	private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand, out float min, out float preferred, out float flexible)
	{
		if (!controlSize)
		{
			min = child.sizeDelta[axis];
			preferred = min;
			flexible = 0f;
		}
		else
		{
			min = LayoutUtility.GetMinSize(child, axis);
			preferred = LayoutUtility.GetPreferredSize(child, axis);
			flexible = LayoutUtility.GetFlexibleSize(child, axis);
		}
		if (childForceExpand)
		{
			flexible = Mathf.Max(flexible, 1f);
		}
	}
}
