using System;
using System.Collections.Generic;

namespace Unity.Cinemachine;

internal class Minkowski
{
	private static List<List<Point64>> MinkowskiInternal(List<Point64> pattern, List<Point64> path, bool isSum, bool isClosed)
	{
		int num = ((!isClosed) ? 1 : 0);
		int count = pattern.Count;
		int count2 = path.Count;
		List<List<Point64>> list = new List<List<Point64>>(count2);
		foreach (Point64 item in path)
		{
			List<Point64> list2 = new List<Point64>(count);
			if (isSum)
			{
				foreach (Point64 item2 in pattern)
				{
					list2.Add(item + item2);
				}
			}
			else
			{
				foreach (Point64 item3 in pattern)
				{
					list2.Add(item - item3);
				}
			}
			list.Add(list2);
		}
		List<List<Point64>> list3 = new List<List<Point64>>((count2 - num) * count);
		int index = (isClosed ? (count2 - 1) : 0);
		int index2 = count - 1;
		for (int i = num; i < count2; i++)
		{
			for (int j = 0; j < count; j++)
			{
				List<Point64> list4 = new List<Point64>(4)
				{
					list[index][index2],
					list[i][index2],
					list[i][j],
					list[index][j]
				};
				if (!Clipper.IsPositive(list4))
				{
					list3.Add(Clipper.ReversePath(list4));
				}
				else
				{
					list3.Add(list4);
				}
				index2 = j;
			}
			index = i;
		}
		return list3;
	}

	public static List<List<Point64>> Sum(List<Point64> pattern, List<Point64> path, bool isClosed)
	{
		return Clipper.Union(MinkowskiInternal(pattern, path, isSum: true, isClosed), FillRule.NonZero);
	}

	public static List<List<PointD>> Sum(List<PointD> pattern, List<PointD> path, bool isClosed, int decimalPlaces = 2)
	{
		double num = Math.Pow(10.0, decimalPlaces);
		return Clipper.ScalePathsD(Clipper.Union(MinkowskiInternal(Clipper.ScalePath64(pattern, num), Clipper.ScalePath64(path, num), isSum: true, isClosed), FillRule.NonZero), 1.0 / num);
	}

	public static List<List<Point64>> Diff(List<Point64> pattern, List<Point64> path, bool isClosed)
	{
		return Clipper.Union(MinkowskiInternal(pattern, path, isSum: false, isClosed), FillRule.NonZero);
	}

	public static List<List<PointD>> Diff(List<PointD> pattern, List<PointD> path, bool isClosed, int decimalPlaces = 2)
	{
		double num = Math.Pow(10.0, decimalPlaces);
		return Clipper.ScalePathsD(Clipper.Union(MinkowskiInternal(Clipper.ScalePath64(pattern, num), Clipper.ScalePath64(path, num), isSum: false, isClosed), FillRule.NonZero), 1.0 / num);
	}
}
