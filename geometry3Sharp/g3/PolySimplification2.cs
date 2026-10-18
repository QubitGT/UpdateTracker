using System;
using System.Collections.Generic;

namespace g3;

public class PolySimplification2
{
	private List<Vector2d> Vertices;

	private bool IsLoop;

	public double StraightLineDeviationThreshold = 0.01;

	public double PreserveStraightSegLen = 2.0;

	public double SimplifyDeviationThreshold = 0.2;

	public List<Vector2d> Result;

	public PolySimplification2(Polygon2d polygon)
	{
		Vertices = new List<Vector2d>(polygon.Vertices);
		IsLoop = true;
	}

	public PolySimplification2(PolyLine2d polycurve)
	{
		Vertices = new List<Vector2d>(polycurve.Vertices);
		IsLoop = false;
	}

	public static void Simplify(GeneralPolygon2d solid, double deviationThresh)
	{
		PolySimplification2 polySimplification = new PolySimplification2(solid.Outer);
		polySimplification.SimplifyDeviationThreshold = deviationThresh;
		polySimplification.Simplify();
		solid.Outer.SetVertices(polySimplification.Result, bTakeOwnership: true);
		foreach (Polygon2d hole in solid.Holes)
		{
			PolySimplification2 polySimplification2 = new PolySimplification2(hole);
			polySimplification2.SimplifyDeviationThreshold = deviationThresh;
			polySimplification2.Simplify();
			hole.SetVertices(polySimplification2.Result, bTakeOwnership: true);
		}
	}

	public void Simplify()
	{
		bool[] array = new bool[Vertices.Count];
		Array.Clear(array, 0, array.Length);
		List<Vector2d> list = collapse_by_deviation_tol(Vertices, array, StraightLineDeviationThreshold);
		find_constrained_segments(list, array);
		Result = collapse_by_deviation_tol(list, array, SimplifyDeviationThreshold);
	}

	private void find_constrained_segments(List<Vector2d> vertices, bool[] markers)
	{
		int count = vertices.Count;
		int num = (IsLoop ? vertices.Count : (vertices.Count - 1));
		for (int i = 0; i < num; i++)
		{
			int num2 = i;
			int index = (i + 1) % count;
			if (vertices[num2].DistanceSquared(vertices[index]) > PreserveStraightSegLen)
			{
				markers[num2] = true;
			}
		}
	}

	private List<Vector2d> collapse_by_deviation_tol(List<Vector2d> input, bool[] keep_segments, double offset_threshold)
	{
		int count = input.Count;
		int num = (IsLoop ? input.Count : (input.Count - 1));
		List<Vector2d> list = new List<Vector2d>();
		list.Add(input[0]);
		double num2 = offset_threshold * offset_threshold;
		int num3 = 0;
		int num4 = 1;
		int num5 = 0;
		if (keep_segments[0])
		{
			list.Add(input[1]);
			num3 = 1;
			num4 = 2;
		}
		while (num4 < num)
		{
			int num6 = num4;
			int num7 = (num4 + 1) % count;
			if (keep_segments[num6])
			{
				if (num3 != num6 && input[num6].Distance(list[list.Count - 1]) > 2.220446049250313E-16)
				{
					list.Add(input[num6]);
				}
				list.Add(input[num7]);
				num3 = num7;
				num5 = 0;
				num4 = ((num7 != 0) ? num7 : num);
				continue;
			}
			Vector2d vector2d = input[num7] - input[num3];
			Line2d line2d = new Line2d(input[num3], vector2d.Normalized);
			double num8 = 0.0;
			for (int i = num3 + 1; i <= num4; i++)
			{
				double num9 = line2d.DistanceSquared(input[i]);
				if (num9 > num8)
				{
					num8 = num9;
				}
			}
			if (num8 > num2)
			{
				list.Add(input[num4]);
				num3 = num4;
				num4++;
				num5 = 0;
			}
			else
			{
				num4++;
				num5++;
			}
		}
		if (IsLoop)
		{
			if (list.Count < 3)
			{
				return handle_tiny_case(list, input, keep_segments, offset_threshold);
			}
			Line2d line2d2 = Line2d.FromPoints(input[num3], input[num4 % count]);
			bool flag = line2d2.DistanceSquared(list[0]) < num2;
			bool flag2 = line2d2.DistanceSquared(list[1]) < num2;
			if (flag && flag2 && list.Count > 3)
			{
				list[0] = input[num3];
				list.RemoveAt(list.Count - 1);
			}
			else if (!flag)
			{
				list.Add(input[input.Count - 1]);
			}
		}
		else
		{
			list.Add(input[input.Count - 1]);
		}
		return list;
	}

	private List<Vector2d> handle_tiny_case(List<Vector2d> result, List<Vector2d> input, bool[] keep_segments, double offset_threshold)
	{
		int count = input.Count;
		if (count == 3)
		{
			return input;
		}
		result.Clear();
		result.Add(input[0]);
		result.Add(input[count / 3]);
		result.Add(input[count - count / 3]);
		return result;
	}
}
