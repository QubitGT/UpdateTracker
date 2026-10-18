using System;
using UnityEngine;

namespace g3;

public struct AxisAlignedBox2f
{
	public enum ScaleMode
	{
		ScaleRight,
		ScaleLeft,
		ScaleUp,
		ScaleDown,
		ScaleCenter
	}

	public Vector2f Min;

	public Vector2f Max;

	public static readonly AxisAlignedBox2f Empty = new AxisAlignedBox2f(bIgnore: false);

	public static readonly AxisAlignedBox2f Zero = new AxisAlignedBox2f(0f);

	public static readonly AxisAlignedBox2f UnitPositive = new AxisAlignedBox2f(1f);

	public static readonly AxisAlignedBox2f Infinite = new AxisAlignedBox2f(float.MinValue, float.MinValue, float.MaxValue, float.MaxValue);

	public float Width => Math.Max(Max.x - Min.x, 0f);

	public float Height => Math.Max(Max.y - Min.y, 0f);

	public float Area => Width * Height;

	public float DiagonalLength => (float)Math.Sqrt((Max.x - Min.x) * (Max.x - Min.x) + (Max.y - Min.y) * (Max.y - Min.y));

	public float MaxDim => Math.Max(Width, Height);

	public Vector2f Diagonal => new Vector2f(Max.x - Min.x, Max.y - Min.y);

	public Vector2f Center => new Vector2f(0.5f * (Min.x + Max.x), 0.5f * (Min.y + Max.y));

	public Vector2f BottomLeft => Min;

	public Vector2f BottomRight => new Vector2f(Max.x, Min.y);

	public Vector2f TopLeft => new Vector2f(Min.x, Max.y);

	public Vector2f TopRight => Max;

	public Vector2f CenterLeft => new Vector2f(Min.x, (Min.y + Max.y) * 0.5f);

	public Vector2f CenterRight => new Vector2f(Max.x, (Min.y + Max.y) * 0.5f);

	public Vector2f CenterTop => new Vector2f((Min.x + Max.x) * 0.5f, Max.y);

	public Vector2f CenterBottom => new Vector2f((Min.x + Max.x) * 0.5f, Min.y);

	public AxisAlignedBox2f(bool bIgnore)
	{
		Min = new Vector2f(float.MaxValue, float.MaxValue);
		Max = new Vector2f(float.MinValue, float.MinValue);
	}

	public AxisAlignedBox2f(float xmin, float ymin, float xmax, float ymax)
	{
		Min = new Vector2f(xmin, ymin);
		Max = new Vector2f(xmax, ymax);
	}

	public AxisAlignedBox2f(float fSquareSize)
	{
		Min = new Vector2f(0f, 0f);
		Max = new Vector2f(fSquareSize, fSquareSize);
	}

	public AxisAlignedBox2f(float fWidth, float fHeight)
	{
		Min = new Vector2f(0f, 0f);
		Max = new Vector2f(fWidth, fHeight);
	}

	public AxisAlignedBox2f(Vector2f vMin, Vector2f vMax)
	{
		Min = new Vector2f(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
		Max = new Vector2f(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
	}

	public AxisAlignedBox2f(Vector2f vCenter, float fHalfWidth, float fHalfHeight)
	{
		Min = new Vector2f(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
		Max = new Vector2f(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
	}

	public AxisAlignedBox2f(Vector2f vCenter, float fHalfWidth)
	{
		Min = new Vector2f(vCenter.x - fHalfWidth, vCenter.y - fHalfWidth);
		Max = new Vector2f(vCenter.x + fHalfWidth, vCenter.y + fHalfWidth);
	}

	public AxisAlignedBox2f(Vector2f vCenter)
	{
		Min = (Max = vCenter);
	}

	public AxisAlignedBox2f(AxisAlignedBox2f o)
	{
		Min = new Vector2f(o.Min);
		Max = new Vector2f(o.Max);
	}

	public Vector2f GetCorner(int i)
	{
		return new Vector2f((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
	}

	public void Expand(float fRadius)
	{
		Min.x -= fRadius;
		Min.y -= fRadius;
		Max.x += fRadius;
		Max.y += fRadius;
	}

	public void Contract(float fRadius)
	{
		Min.x += fRadius;
		Min.y += fRadius;
		Max.x -= fRadius;
		Max.y -= fRadius;
	}

	[Obsolete("This method name is confusing. Will remove in future. Use Add() instead")]
	public void Pad(float fPadLeft, float fPadRight, float fPadBottom, float fPadTop)
	{
		Min.x += fPadLeft;
		Min.y += fPadBottom;
		Max.x += fPadRight;
		Max.y += fPadTop;
	}

	public void Add(float left, float right, float bottom, float top)
	{
		Min.x += left;
		Min.y += bottom;
		Max.x += right;
		Max.y += top;
	}

	public void SetWidth(float fNewWidth, ScaleMode eScaleMode)
	{
		switch (eScaleMode)
		{
		case ScaleMode.ScaleLeft:
			Min.x = Max.x - fNewWidth;
			break;
		case ScaleMode.ScaleRight:
			Max.x = Min.x + fNewWidth;
			break;
		case ScaleMode.ScaleCenter:
		{
			Vector2f center = Center;
			Min.x = center.x - 0.5f * fNewWidth;
			Max.x = center.x + 0.5f * fNewWidth;
			break;
		}
		default:
			throw new Exception("Invalid scale mode...");
		}
	}

	public void SetHeight(float fNewHeight, ScaleMode eScaleMode)
	{
		switch (eScaleMode)
		{
		case ScaleMode.ScaleDown:
			Min.y = Max.y - fNewHeight;
			break;
		case ScaleMode.ScaleUp:
			Max.y = Min.y + fNewHeight;
			break;
		case ScaleMode.ScaleCenter:
		{
			Vector2f center = Center;
			Min.y = center.y - 0.5f * fNewHeight;
			Max.y = center.y + 0.5f * fNewHeight;
			break;
		}
		default:
			throw new Exception("Invalid scale mode...");
		}
	}

	public void Contain(Vector2f v)
	{
		Min.x = Math.Min(Min.x, v.x);
		Min.y = Math.Min(Min.y, v.y);
		Max.x = Math.Max(Max.x, v.x);
		Max.y = Math.Max(Max.y, v.y);
	}

	public void Contain(AxisAlignedBox2f box)
	{
		Min.x = Math.Min(Min.x, box.Min.x);
		Min.y = Math.Min(Min.y, box.Min.y);
		Max.x = Math.Max(Max.x, box.Max.x);
		Max.y = Math.Max(Max.y, box.Max.y);
	}

	public AxisAlignedBox2f Intersect(AxisAlignedBox2f box)
	{
		AxisAlignedBox2f result = new AxisAlignedBox2f(Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y), Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y));
		if (result.Height <= 0f || result.Width <= 0f)
		{
			return Empty;
		}
		return result;
	}

	public bool Contains(Vector2f v)
	{
		if (Min.x < v.x && Min.y < v.y && Max.x > v.x)
		{
			return Max.y > v.y;
		}
		return false;
	}

	public bool Intersects(AxisAlignedBox2f box)
	{
		if (!(box.Max.x < Min.x) && !(box.Min.x > Max.x) && !(box.Max.y < Min.y))
		{
			return !(box.Min.y > Max.y);
		}
		return false;
	}

	public float Distance(Vector2f v)
	{
		float num = Math.Abs(v.x - Center.x);
		float num2 = Math.Abs(v.y - Center.y);
		float num3 = Width * 0.5f;
		float num4 = Height * 0.5f;
		if (num < num3 && num2 < num4)
		{
			return 0f;
		}
		if (num > num3 && num2 > num4)
		{
			return (float)Math.Sqrt((num - num3) * (num - num3) + (num2 - num4) * (num2 - num4));
		}
		if (num > num3)
		{
			return num - num3;
		}
		if (num2 > num4)
		{
			return num2 - num4;
		}
		return 0f;
	}

	public void Translate(Vector2f vTranslate)
	{
		Min.Add(vTranslate);
		Max.Add(vTranslate);
	}

	public void MoveMin(Vector2f vNewMin)
	{
		Max.x = vNewMin.x + (Max.x - Min.x);
		Max.y = vNewMin.y + (Max.y - Min.y);
		Min.Set(vNewMin);
	}

	public void MoveMin(float fNewX, float fNewY)
	{
		Max.x = fNewX + (Max.x - Min.x);
		Max.y = fNewY + (Max.y - Min.y);
		Min.Set(fNewX, fNewY);
	}

	public override string ToString()
	{
		return $"[{Min.x:F8},{Max.x:F8}] [{Min.y:F8},{Max.y:F8}]";
	}

	public static implicit operator AxisAlignedBox2f(Rect b)
	{
		return new AxisAlignedBox2f(b.min, b.max);
	}

	public static implicit operator Rect(AxisAlignedBox2f b)
	{
		Rect result = default(Rect);
		result.min = b.Min;
		result.max = b.Max;
		return result;
	}
}
