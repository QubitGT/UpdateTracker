using System;
using System.Collections.Generic;

namespace g3;

public class ContMinBox2
{
	protected enum RCFlags
	{
		F_NONE,
		F_LEFT,
		F_RIGHT,
		F_BOTTOM,
		F_TOP
	}

	private Box2d mMinBox;

	public Box2d MinBox => mMinBox;

	public ContMinBox2(IList<Vector2d> points, double epsilon, QueryNumberType queryType, bool isConvexPolygon)
	{
		IList<Vector2d> list;
		int num;
		if (isConvexPolygon)
		{
			list = points;
			num = list.Count;
		}
		else
		{
			ConvexHull2 convexHull = new ConvexHull2(points, epsilon, queryType);
			int dimension = convexHull.Dimension;
			int numSimplices = convexHull.NumSimplices;
			int[] hullIndices = convexHull.HullIndices;
			switch (dimension)
			{
			case 0:
				mMinBox.Center = points[0];
				mMinBox.AxisX = Vector2d.AxisX;
				mMinBox.AxisY = Vector2d.AxisY;
				mMinBox.Extent[0] = 0.0;
				mMinBox.Extent[1] = 0.0;
				return;
			case 1:
				throw new NotImplementedException("ContMinBox2: Have not implemented 1d case");
			}
			num = numSimplices;
			Vector2d[] array = new Vector2d[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = points[hullIndices[i]];
			}
			list = array;
		}
		int num2 = num - 1;
		Vector2d[] array2 = new Vector2d[num];
		bool[] array3 = new bool[num];
		for (int j = 0; j < num2; j++)
		{
			array2[j] = list[j + 1] - list[j];
			array2[j].Normalize();
			array3[j] = false;
		}
		array2[num2] = list[0] - list[num2];
		array2[num2].Normalize();
		array3[num2] = false;
		double x = list[0].x;
		double num3 = x;
		double y = list[0].y;
		double num4 = y;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		for (int k = 1; k < num; k++)
		{
			if (list[k].x <= x)
			{
				x = list[k].x;
				num5 = k;
			}
			if (list[k].x >= num3)
			{
				num3 = list[k].x;
				num6 = k;
			}
			if (list[k].y <= y)
			{
				y = list[k].y;
				num7 = k;
			}
			if (list[k].y >= num4)
			{
				num4 = list[k].y;
				num8 = k;
			}
		}
		if (num5 == num2 && list[0].x <= x)
		{
			x = list[0].x;
			num5 = 0;
		}
		if (num6 == num2 && list[0].x >= num3)
		{
			num3 = list[0].x;
			num6 = 0;
		}
		if (num7 == num2 && list[0].y <= y)
		{
			y = list[0].y;
			num7 = 0;
		}
		if (num8 == num2 && list[0].y >= num4)
		{
			num4 = list[0].y;
			num8 = 0;
		}
		mMinBox.Center.x = 0.5 * (x + num3);
		mMinBox.Center.y = 0.5 * (y + num4);
		mMinBox.AxisX = Vector2d.AxisX;
		mMinBox.AxisY = Vector2d.AxisY;
		mMinBox.Extent[0] = 0.5 * (num3 - x);
		mMinBox.Extent[1] = 0.5 * (num4 - y);
		double minAreaDiv = mMinBox.Extent[0] * mMinBox.Extent[1];
		Vector2d U = Vector2d.AxisX;
		Vector2d V = Vector2d.AxisY;
		bool flag = false;
		while (!flag)
		{
			RCFlags rCFlags = RCFlags.F_NONE;
			double num9 = 0.0;
			double num10 = U.Dot(array2[num7]);
			if (num10 > num9)
			{
				num9 = num10;
				rCFlags = RCFlags.F_BOTTOM;
			}
			num10 = V.Dot(array2[num6]);
			if (num10 > num9)
			{
				num9 = num10;
				rCFlags = RCFlags.F_RIGHT;
			}
			num10 = 0.0 - U.Dot(array2[num8]);
			if (num10 > num9)
			{
				num9 = num10;
				rCFlags = RCFlags.F_TOP;
			}
			num10 = 0.0 - V.Dot(array2[num5]);
			if (num10 > num9)
			{
				num9 = num10;
				rCFlags = RCFlags.F_LEFT;
			}
			switch (rCFlags)
			{
			case RCFlags.F_BOTTOM:
				if (array3[num7])
				{
					flag = true;
					break;
				}
				U = array2[num7];
				V = -U.Perp;
				UpdateBox(list[num5], list[num6], list[num7], list[num8], ref U, ref V, ref minAreaDiv);
				array3[num7] = true;
				if (++num7 == num)
				{
					num7 = 0;
				}
				break;
			case RCFlags.F_RIGHT:
				if (array3[num6])
				{
					flag = true;
					break;
				}
				V = array2[num6];
				U = V.Perp;
				UpdateBox(list[num5], list[num6], list[num7], list[num8], ref U, ref V, ref minAreaDiv);
				array3[num6] = true;
				if (++num6 == num)
				{
					num6 = 0;
				}
				break;
			case RCFlags.F_TOP:
				if (array3[num8])
				{
					flag = true;
					break;
				}
				U = -array2[num8];
				V = -U.Perp;
				UpdateBox(list[num5], list[num6], list[num7], list[num8], ref U, ref V, ref minAreaDiv);
				array3[num8] = true;
				if (++num8 == num)
				{
					num8 = 0;
				}
				break;
			case RCFlags.F_LEFT:
				if (array3[num5])
				{
					flag = true;
					break;
				}
				V = -array2[num5];
				U = V.Perp;
				UpdateBox(list[num5], list[num6], list[num7], list[num8], ref U, ref V, ref minAreaDiv);
				array3[num5] = true;
				if (++num5 == num)
				{
					num5 = 0;
				}
				break;
			case RCFlags.F_NONE:
				flag = true;
				break;
			}
		}
	}

	protected void UpdateBox(Vector2d LPoint, Vector2d RPoint, Vector2d BPoint, Vector2d TPoint, ref Vector2d U, ref Vector2d V, ref double minAreaDiv4)
	{
		Vector2d v = RPoint - LPoint;
		Vector2d v2 = TPoint - BPoint;
		double num = 0.5 * U.Dot(v);
		double num2 = 0.5 * V.Dot(v2);
		double num3 = num * num2;
		if (num3 < minAreaDiv4)
		{
			minAreaDiv4 = num3;
			mMinBox.AxisX = U;
			mMinBox.AxisY = V;
			mMinBox.Extent[0] = num;
			mMinBox.Extent[1] = num2;
			Vector2d v3 = LPoint - BPoint;
			mMinBox.Center = LPoint + U * num + V * (num2 - V.Dot(v3));
		}
	}
}
