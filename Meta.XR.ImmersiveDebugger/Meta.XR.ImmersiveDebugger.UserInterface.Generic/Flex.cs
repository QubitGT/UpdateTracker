using System;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class Flex : Controller
{
	private Vector2 _sizeDelta;

	private Vector2? _previousAnchoredPosition;

	internal Vector2 SizeDelta => _sizeDelta;

	internal Vector2 SizeDeltaWithMargin => _sizeDelta + base.LayoutStyle.TopLeftMargin + base.LayoutStyle.BottomRightMargin;

	internal ScrollViewport ScrollViewport { get; set; }

	private void UpdateAnchoredPosition(Controller controller, ref Vector2 offset, Vector2 direction)
	{
		Vector2 margin = controller.LayoutStyle.margin;
		Vector2 vector = new Vector2(margin.x, 0f - margin.y);
		Vector2 sizeDelta = controller.RectTransform.sizeDelta;
		controller.RectTransform.anchoredPosition = vector + offset;
		offset += direction * sizeDelta;
		offset += direction * _layoutStyle.spacing;
	}

	private void UpdateChildrenWidth()
	{
		if (_children == null || !_layoutStyle.autoFitChildren || _layoutStyle.size.x <= 0f)
		{
			return;
		}
		float num = base.RectTransform.sizeDelta.x;
		int num2 = 0;
		foreach (Controller child in _children)
		{
			if (child.LayoutStyle.layout == LayoutStyle.Layout.Fixed)
			{
				num -= child.LayoutStyle.size.x + child.LayoutStyle.margin.x * 2f + _layoutStyle.spacing;
				num2++;
			}
		}
		int num3 = _children.Count - num2;
		if (num3 == 0)
		{
			return;
		}
		foreach (Controller child2 in _children)
		{
			float width = (float)Mathf.RoundToInt(num / (float)num3) - _layoutStyle.spacing;
			if (child2.LayoutStyle.layout != 0)
			{
				child2.SetWidth(width);
			}
		}
	}

	private void RefreshVisibilities(bool force = false)
	{
		if (ScrollViewport == null || _children == null)
		{
			return;
		}
		Vector2 anchoredPosition = base.RectTransform.anchoredPosition;
		if (!force)
		{
			Vector2 value = anchoredPosition;
			Vector2? previousAnchoredPosition = _previousAnchoredPosition;
			if (value == previousAnchoredPosition)
			{
				return;
			}
		}
		_previousAnchoredPosition = anchoredPosition;
		Vector2 anchoredPosition2 = base.RectTransform.anchoredPosition;
		Rect viewportRect = new Rect(ScrollViewport.RectTransform.anchoredPosition, ScrollViewport.RectTransform.rect.size);
		bool flag = false;
		bool flag2 = false;
		foreach (Controller child in _children)
		{
			if (!flag2 && IsVerticallyInViewport(child, viewportRect, anchoredPosition2))
			{
				child.Show();
				flag = true;
				continue;
			}
			child.Hide();
			if (flag)
			{
				flag2 = true;
			}
		}
	}

	private static bool IsVerticallyInViewport(Controller controller, Rect viewportRect, Vector2 scroll)
	{
		Vector2 vector = -controller.RectTransform.anchoredPosition - scroll;
		if (vector.y >= viewportRect.yMin)
		{
			if (vector.y < viewportRect.yMax)
			{
				return true;
			}
		}
		else if (vector.y + controller.RectTransform.sizeDelta.y >= viewportRect.yMin)
		{
			return true;
		}
		return false;
	}

	protected override void RefreshLayoutPreChildren()
	{
		base.RefreshLayoutPreChildren();
		UpdateChildrenWidth();
	}

	protected override void RefreshLayoutPostChildren()
	{
		if (!_hasRectTransform)
		{
			return;
		}
		if (_children != null)
		{
			Vector2 direction = _layoutStyle.flexDirection switch
			{
				LayoutStyle.Direction.Left => Vector2.left, 
				LayoutStyle.Direction.Right => Vector2.right, 
				LayoutStyle.Direction.Down => Vector2.down, 
				LayoutStyle.Direction.Up => Vector2.up, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			Vector2 offset = Vector2.zero;
			foreach (Controller child in _children)
			{
				UpdateAnchoredPosition(child, ref offset, direction);
			}
			_previousAnchoredPosition = null;
			_sizeDelta = new Vector2(Mathf.Abs(offset.x), Mathf.Abs(offset.y));
		}
		if (base.LayoutStyle.adaptHeight)
		{
			base.RectTransform.sizeDelta = new Vector2(base.RectTransform.sizeDelta.x, Mathf.Abs(_sizeDelta.y));
		}
	}

	private void LateUpdate()
	{
		RefreshVisibilities();
	}

	internal void Forget(Controller controller)
	{
		Remove(controller, destroy: false);
		controller.Hide();
	}

	internal void Remember(Controller controller)
	{
		Append(controller);
		controller.Show();
	}

	internal void ForgetAll()
	{
		if (_children == null)
		{
			return;
		}
		foreach (Controller child in _children)
		{
			child.Hide();
		}
		Clear(destroy: false);
	}
}
