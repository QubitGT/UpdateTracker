using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

public class LayoutStyle : Style
{
	public enum Layout
	{
		Fixed,
		Fill,
		FillHorizontal,
		FillVertical
	}

	public enum Direction
	{
		Left,
		Right,
		Down,
		Up
	}

	public Direction flexDirection;

	public Layout layout;

	public TextAnchor anchor;

	public TextAnchor pivot;

	public Vector2 size;

	public Vector2 margin;

	public bool useBottomRightMargin;

	public Vector2 bottomRightMargin;

	public float spacing;

	public bool masks;

	public bool adaptHeight;

	public bool autoFitChildren;

	public bool isOverlayCanvas;

	public float LeftMargin => margin.x;

	public float TopMargin => margin.y;

	public float RightMargin
	{
		get
		{
			if (!useBottomRightMargin)
			{
				return margin.x;
			}
			return bottomRightMargin.x;
		}
	}

	public float BottomMargin
	{
		get
		{
			if (!useBottomRightMargin)
			{
				return margin.y;
			}
			return bottomRightMargin.y;
		}
	}

	public Vector2 TopLeftMargin => margin;

	public Vector2 BottomRightMargin
	{
		get
		{
			if (!useBottomRightMargin)
			{
				return margin;
			}
			return bottomRightMargin;
		}
	}

	internal bool SetHeight(float height)
	{
		if (!_instantiated || size.y == height)
		{
			return false;
		}
		size.y = height;
		return true;
	}

	internal bool SetWidth(float width)
	{
		if (!_instantiated || size.x == width)
		{
			return false;
		}
		size.x = width;
		return true;
	}

	internal bool SetIndent(float value)
	{
		if (!_instantiated || margin.x == value)
		{
			return false;
		}
		if (!useBottomRightMargin)
		{
			useBottomRightMargin = true;
			bottomRightMargin.x = margin.x;
			bottomRightMargin.y = margin.y;
		}
		margin.x = value;
		return true;
	}
}
