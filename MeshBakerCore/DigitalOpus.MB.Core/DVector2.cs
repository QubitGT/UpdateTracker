using System;
using UnityEngine;

namespace DigitalOpus.MB.Core;

public struct DVector2
{
	private static double epsilon = 1E-05;

	public double x;

	public double y;

	public static DVector2 Subtract(DVector2 a, DVector2 b)
	{
		return new DVector2(a.x - b.x, a.y - b.y);
	}

	public DVector2(double xx, double yy)
	{
		x = xx;
		y = yy;
	}

	public DVector2(DVector2 r)
	{
		x = r.x;
		y = r.y;
	}

	public Vector2 GetVector2()
	{
		return new Vector2((float)x, (float)y);
	}

	public bool IsContainedIn(DRect r)
	{
		if (x >= r.x && y >= r.y && x <= r.x + r.width && y <= r.y + r.height)
		{
			return true;
		}
		return false;
	}

	public bool IsContainedInWithMargin(DRect r)
	{
		if (x >= r.x - epsilon && y >= r.y - epsilon && x <= r.x + r.width + epsilon && y <= r.y + r.height + epsilon)
		{
			return true;
		}
		return false;
	}

	public override string ToString()
	{
		return $"({x},{y})";
	}

	public string ToString(string formatS)
	{
		return $"({x.ToString(formatS)},{y.ToString(formatS)})";
	}

	public static double Distance(DVector2 a, DVector2 b)
	{
		double num = b.x - a.x;
		double num2 = b.y - a.y;
		return Math.Sqrt(num * num + num2 * num2);
	}
}
