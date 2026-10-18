using System;
using System.Collections.Generic;
using UnityEngine;

namespace g3;

public struct AxisAlignedBox2d
{
	public enum ScaleMode
	{
		ScaleRight,
		ScaleLeft,
		ScaleUp,
		ScaleDown,
		ScaleCenter
	}

	public Vector2d Min;

	public Vector2d Max;

	public static readonly AxisAlignedBox2d Empty = new AxisAlignedBox2d(bIgnore: false);

	public static readonly AxisAlignedBox2d Zero = new AxisAlignedBox2d(0.0);

	public static readonly AxisAlignedBox2d UnitPositive = new AxisAlignedBox2d(1.0);

	public static readonly AxisAlignedBox2d Infinite = new AxisAlignedBox2d(double.MinValue, double.MinValue, double.MaxValue, double.MaxValue);

	public double Width => Math.Max(Max.x - Min.x, 0.0);

	public double Height => Math.Max(Max.y - Min.y, 0.0);

	public double Area => Width * Height;

	public double DiagonalLength => Math.Sqrt((Max.x - Min.x) * (Max.x - Min.x) + (Max.y - Min.y) * (Max.y - Min.y));

	public double MaxDim => Math.Max(Width, Height);

	public double MinDim => Math.Min(Width, Height);

	public double MaxUnsignedCoordinate => Math.Max(Math.Max(Math.Abs(Min.x), Math.Abs(Max.x)), Math.Max(Math.Abs(Min.y), Math.Abs(Max.y)));

	public Vector2d Diagonal => new Vector2d(Max.x - Min.x, Max.y - Min.y);

	public Vector2d Center => new Vector2d(0.5 * (Min.x + Max.x), 0.5 * (Min.y + Max.y));

	public AxisAlignedBox2d(bool bIgnore)
	{
		Min = new Vector2d(double.MaxValue, double.MaxValue);
		Max = new Vector2d(double.MinValue, double.MinValue);
	}

	public AxisAlignedBox2d(double xmin, double ymin, double xmax, double ymax)
	{
		Min = new Vector2d(xmin, ymin);
		Max = new Vector2d(xmax, ymax);
	}

	public AxisAlignedBox2d(double fSquareSize)
	{
		Min = new Vector2d(0f, 0f);
		Max = new Vector2d(fSquareSize, fSquareSize);
	}

	public AxisAlignedBox2d(double fWidth, double fHeight)
	{
		Min = new Vector2d(0f, 0f);
		Max = new Vector2d(fWidth, fHeight);
	}

	public AxisAlignedBox2d(Vector2d vMin, Vector2d vMax)
	{
		Min = new Vector2d(Math.Min(vMin.x, vMax.x), Math.Min(vMin.y, vMax.y));
		Max = new Vector2d(Math.Max(vMin.x, vMax.x), Math.Max(vMin.y, vMax.y));
	}

	public AxisAlignedBox2d(Vector2d vCenter, double fHalfWidth, double fHalfHeight)
	{
		Min = new Vector2d(vCenter.x - fHalfWidth, vCenter.y - fHalfHeight);
		Max = new Vector2d(vCenter.x + fHalfWidth, vCenter.y + fHalfHeight);
	}

	public AxisAlignedBox2d(Vector2d vCenter, double fHalfWidth)
	{
		Min = new Vector2d(vCenter.x - fHalfWidth, vCenter.y - fHalfWidth);
		Max = new Vector2d(vCenter.x + fHalfWidth, vCenter.y + fHalfWidth);
	}

	public AxisAlignedBox2d(Vector2d vCenter)
	{
		Min = (Max = vCenter);
	}

	public AxisAlignedBox2d(AxisAlignedBox2d o)
	{
		Min = new Vector2d(o.Min);
		Max = new Vector2d(o.Max);
	}

	public Vector2d GetCorner(int i)
	{
		return new Vector2d((i % 3 == 0) ? Min.x : Max.x, (i < 2) ? Min.y : Max.y);
	}

	public Vector2d SampleT(double tx, double sy)
	{
		return new Vector2d((1.0 - tx) * Min.x + tx * Max.x, (1.0 - sy) * Min.y + sy * Max.y);
	}

	public void Expand(double fRadius)
	{
		Min.x -= fRadius;
		Min.y -= fRadius;
		Max.x += fRadius;
		Max.y += fRadius;
	}

	public void Contract(double fRadius)
	{
		Min.x += fRadius;
		Min.y += fRadius;
		Max.x -= fRadius;
		Max.y -= fRadius;
	}

	[Obsolete("This method name is confusing. Will remove in future. Use Add() instead")]
	public void Pad(double fPadLeft, double fPadRight, double fPadBottom, double fPadTop)
	{
		Min.x += fPadLeft;
		Min.y += fPadBottom;
		Max.x += fPadRight;
		Max.y += fPadTop;
	}

	public void Add(double left, double right, double bottom, double top)
	{
		Min.x += left;
		Min.y += bottom;
		Max.x += right;
		Max.y += top;
	}

	public void SetWidth(double fNewWidth, ScaleMode eScaleMode)
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
			Vector2d center = Center;
			Min.x = center.x - 0.5 * fNewWidth;
			Max.x = center.x + 0.5 * fNewWidth;
			break;
		}
		default:
			throw new Exception("Invalid scale mode...");
		}
	}

	public void SetHeight(double fNewHeight, ScaleMode eScaleMode)
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
			Vector2d center = Center;
			Min.y = center.y - 0.5 * fNewHeight;
			Max.y = center.y + 0.5 * fNewHeight;
			break;
		}
		default:
			throw new Exception("Invalid scale mode...");
		}
	}

	public void Contain(Vector2d v)
	{
		if (v.x < Min.x)
		{
			Min.x = v.x;
		}
		if (v.x > Max.x)
		{
			Max.x = v.x;
		}
		if (v.y < Min.y)
		{
			Min.y = v.y;
		}
		if (v.y > Max.y)
		{
			Max.y = v.y;
		}
	}

	public void Contain(ref Vector2d v)
	{
		if (v.x < Min.x)
		{
			Min.x = v.x;
		}
		if (v.x > Max.x)
		{
			Max.x = v.x;
		}
		if (v.y < Min.y)
		{
			Min.y = v.y;
		}
		if (v.y > Max.y)
		{
			Max.y = v.y;
		}
	}

	public void Contain(AxisAlignedBox2d box)
	{
		if (box.Min.x < Min.x)
		{
			Min.x = box.Min.x;
		}
		if (box.Max.x > Max.x)
		{
			Max.x = box.Max.x;
		}
		if (box.Min.y < Min.y)
		{
			Min.y = box.Min.y;
		}
		if (box.Max.y > Max.y)
		{
			Max.y = box.Max.y;
		}
	}

	public void Contain(ref AxisAlignedBox2d box)
	{
		if (box.Min.x < Min.x)
		{
			Min.x = box.Min.x;
		}
		if (box.Max.x > Max.x)
		{
			Max.x = box.Max.x;
		}
		if (box.Min.y < Min.y)
		{
			Min.y = box.Min.y;
		}
		if (box.Max.y > Max.y)
		{
			Max.y = box.Max.y;
		}
	}

	public void Contain(IList<Vector2d> points)
	{
		int count = points.Count;
		if (count <= 0)
		{
			return;
		}
		Vector2d v = points[0];
		Contain(ref v);
		for (int i = 1; i < count; i++)
		{
			v = points[i];
			if (v.x < Min.x)
			{
				Min.x = v.x;
			}
			else if (v.x > Max.x)
			{
				Max.x = v.x;
			}
			if (v.y < Min.y)
			{
				Min.y = v.y;
			}
			else if (v.y > Max.y)
			{
				Max.y = v.y;
			}
		}
	}

	public AxisAlignedBox2d Intersect(AxisAlignedBox2d box)
	{
		AxisAlignedBox2d result = new AxisAlignedBox2d(Math.Max(Min.x, box.Min.x), Math.Max(Min.y, box.Min.y), Math.Min(Max.x, box.Max.x), Math.Min(Max.y, box.Max.y));
		if (result.Height <= 0.0 || result.Width <= 0.0)
		{
			return Empty;
		}
		return result;
	}

	public bool Contains(Vector2d v)
	{
		if (Min.x < v.x && Min.y < v.y && Max.x > v.x)
		{
			return Max.y > v.y;
		}
		return false;
	}

	public bool Contains(ref Vector2d v)
	{
		if (Min.x < v.x && Min.y < v.y && Max.x > v.x)
		{
			return Max.y > v.y;
		}
		return false;
	}

	public bool Contains(AxisAlignedBox2d box2)
	{
		if (Contains(ref box2.Min))
		{
			return Contains(ref box2.Max);
		}
		return false;
	}

	public bool Contains(ref AxisAlignedBox2d box2)
	{
		if (Contains(ref box2.Min))
		{
			return Contains(ref box2.Max);
		}
		return false;
	}

	public bool Intersects(AxisAlignedBox2d box)
	{
		if (!(box.Max.x < Min.x) && !(box.Min.x > Max.x) && !(box.Max.y < Min.y))
		{
			return !(box.Min.y > Max.y);
		}
		return false;
	}

	public bool Intersects(ref AxisAlignedBox2d box)
	{
		if (!(box.Max.x < Min.x) && !(box.Min.x > Max.x) && !(box.Max.y < Min.y))
		{
			return !(box.Min.y > Max.y);
		}
		return false;
	}

	public double Distance(Vector2d v)
	{
		double num = Math.Abs(v.x - Center.x);
		double num2 = Math.Abs(v.y - Center.y);
		double num3 = Width * 0.5;
		double num4 = Height * 0.5;
		if (num < num3 && num2 < num4)
		{
			return 0.0;
		}
		if (num > num3 && num2 > num4)
		{
			return Math.Sqrt((num - num3) * (num - num3) + (num2 - num4) * (num2 - num4));
		}
		if (num > num3)
		{
			return num - num3;
		}
		if (num2 > num4)
		{
			return num2 - num4;
		}
		return 0.0;
	}

	public void Translate(Vector2d vTranslate)
	{
		Min.Add(vTranslate);
		Max.Add(vTranslate);
	}

	public void Scale(double scale)
	{
		Min *= scale;
		Max *= scale;
	}

	public void Scale(double scale, Vector2d origin)
	{
		Min = (Min - origin) * scale + origin;
		Max = (Max - origin) * scale + origin;
	}

	public void MoveMin(Vector2d vNewMin)
	{
		Max.x = vNewMin.x + (Max.x - Min.x);
		Max.y = vNewMin.y + (Max.y - Min.y);
		Min.Set(vNewMin);
	}

	public void MoveMin(double fNewX, double fNewY)
	{
		Max.x = fNewX + (Max.x - Min.x);
		Max.y = fNewY + (Max.y - Min.y);
		Min.Set(fNewX, fNewY);
	}

	public override string ToString()
	{
		return $"[{Min.x:F8},{Max.x:F8}] [{Min.y:F8},{Max.y:F8}]";
	}

	public static implicit operator AxisAlignedBox2d(Rect b)
	{
		return new AxisAlignedBox2d(b.min, b.max);
	}

	public static explicit operator Rect(AxisAlignedBox2d b)
	{
		Rect result = default(Rect);
		result.min = (Vector2)b.Min;
		result.max = (Vector2)b.Max;
		return result;
	}
}
