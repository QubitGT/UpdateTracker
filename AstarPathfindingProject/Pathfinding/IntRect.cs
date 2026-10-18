using System;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[Serializable]
public struct IntRect
{
	public int xmin;

	public int ymin;

	public int xmax;

	public int ymax;

	public Int2 Min => new Int2(xmin, ymin);

	public Int2 Max => new Int2(xmax, ymax);

	public int Width => xmax - xmin + 1;

	public int Height => ymax - ymin + 1;

	public int Area => Width * Height;

	public IntRect(int xmin, int ymin, int xmax, int ymax)
	{
		this.xmin = xmin;
		this.xmax = xmax;
		this.ymin = ymin;
		this.ymax = ymax;
	}

	public bool Contains(int x, int y)
	{
		if (x >= xmin && y >= ymin && x <= xmax)
		{
			return y <= ymax;
		}
		return false;
	}

	public bool IsValid()
	{
		if (xmin <= xmax)
		{
			return ymin <= ymax;
		}
		return false;
	}

	public static bool operator ==(IntRect a, IntRect b)
	{
		if (a.xmin == b.xmin && a.xmax == b.xmax && a.ymin == b.ymin)
		{
			return a.ymax == b.ymax;
		}
		return false;
	}

	public static bool operator !=(IntRect a, IntRect b)
	{
		if (a.xmin == b.xmin && a.xmax == b.xmax && a.ymin == b.ymin)
		{
			return a.ymax != b.ymax;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		IntRect intRect = (IntRect)obj;
		if (xmin == intRect.xmin && xmax == intRect.xmax && ymin == intRect.ymin)
		{
			return ymax == intRect.ymax;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (xmin * 131071) ^ (xmax * 3571) ^ (ymin * 3109) ^ (ymax * 7);
	}

	public static IntRect Intersection(IntRect a, IntRect b)
	{
		return new IntRect(Math.Max(a.xmin, b.xmin), Math.Max(a.ymin, b.ymin), Math.Min(a.xmax, b.xmax), Math.Min(a.ymax, b.ymax));
	}

	public static bool Intersects(IntRect a, IntRect b)
	{
		if (a.xmin <= b.xmax && a.ymin <= b.ymax && a.xmax >= b.xmin)
		{
			return a.ymax >= b.ymin;
		}
		return false;
	}

	public static IntRect Union(IntRect a, IntRect b)
	{
		return new IntRect(Math.Min(a.xmin, b.xmin), Math.Min(a.ymin, b.ymin), Math.Max(a.xmax, b.xmax), Math.Max(a.ymax, b.ymax));
	}

	public IntRect ExpandToContain(int x, int y)
	{
		return new IntRect(Math.Min(xmin, x), Math.Min(ymin, y), Math.Max(xmax, x), Math.Max(ymax, y));
	}

	public IntRect Expand(int range)
	{
		return new IntRect(xmin - range, ymin - range, xmax + range, ymax + range);
	}

	public override string ToString()
	{
		return "[x: " + xmin + "..." + xmax + ", y: " + ymin + "..." + ymax + "]";
	}

	public void DebugDraw(GraphTransform transform, Color color)
	{
		Vector3 vector = transform.Transform(new Vector3(xmin, 0f, ymin));
		Vector3 vector2 = transform.Transform(new Vector3(xmin, 0f, ymax));
		Vector3 vector3 = transform.Transform(new Vector3(xmax, 0f, ymax));
		Vector3 vector4 = transform.Transform(new Vector3(xmax, 0f, ymin));
		Debug.DrawLine(vector, vector2, color);
		Debug.DrawLine(vector2, vector3, color);
		Debug.DrawLine(vector3, vector4, color);
		Debug.DrawLine(vector4, vector, color);
	}
}
