using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Unity.Cinemachine;

internal class ClipperOffset
{
	private readonly List<PathGroup> _pathGroups = new List<PathGroup>();

	private readonly List<PointD> _normals = new List<PointD>();

	private readonly List<List<Point64>> solution = new List<List<Point64>>();

	private double _delta;

	private double _tmpLimit;

	private double _stepsPerRad;

	private JoinType _joinType;

	private const double TwoPi = Math.PI * 2.0;

	public double ArcTolerance { get; set; }

	public bool MergeGroups { get; set; }

	public double MiterLimit { get; set; }

	public bool PreserveCollinear { get; set; }

	public bool ReverseSolution { get; set; }

	public ClipperOffset(double miterLimit = 2.0, double arcTolerance = 0.0, bool preserveCollinear = false, bool reverseSolution = false)
	{
		MiterLimit = miterLimit;
		ArcTolerance = arcTolerance;
		MergeGroups = true;
		PreserveCollinear = preserveCollinear;
		ReverseSolution = reverseSolution;
	}

	public void Clear()
	{
		_pathGroups.Clear();
	}

	public void AddPath(List<Point64> path, JoinType joinType, EndType endType)
	{
		if (path.Count != 0)
		{
			List<List<Point64>> paths = new List<List<Point64>>(1) { path };
			AddPaths(paths, joinType, endType);
		}
	}

	public void AddPaths(List<List<Point64>> paths, JoinType joinType, EndType endType)
	{
		if (paths.Count != 0)
		{
			_pathGroups.Add(new PathGroup(paths, joinType, endType));
		}
	}

	public void AddPath(List<PointD> path, JoinType joinType, EndType endType)
	{
		if (path.Count != 0)
		{
			List<List<PointD>> paths = new List<List<PointD>>(1) { path };
			AddPaths(paths, joinType, endType);
		}
	}

	public void AddPaths(List<List<PointD>> paths, JoinType joinType, EndType endType)
	{
		if (paths.Count != 0)
		{
			_pathGroups.Add(new PathGroup(Clipper.Paths64(paths), joinType, endType));
		}
	}

	public List<List<Point64>> Execute(double delta)
	{
		solution.Clear();
		if (Math.Abs(delta) < 0.5)
		{
			foreach (PathGroup pathGroup in _pathGroups)
			{
				foreach (List<Point64> inPath in pathGroup._inPaths)
				{
					solution.Add(inPath);
				}
			}
			return solution;
		}
		_tmpLimit = ((MiterLimit <= 1.0) ? 2.0 : (2.0 / Clipper.Sqr(MiterLimit)));
		foreach (PathGroup pathGroup2 in _pathGroups)
		{
			DoGroupOffset(pathGroup2, delta);
		}
		if (MergeGroups && _pathGroups.Count > 0)
		{
			Clipper64 clipper = new Clipper64
			{
				PreserveCollinear = PreserveCollinear,
				ReverseSolution = (ReverseSolution != _pathGroups[0]._pathsReversed)
			};
			clipper.AddSubject(solution);
			if (_pathGroups[0]._pathsReversed)
			{
				clipper.Execute(ClipType.Union, FillRule.Negative, solution);
			}
			else
			{
				clipper.Execute(ClipType.Union, FillRule.Positive, solution);
			}
		}
		return solution;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static PointD GetUnitNormal(Point64 pt1, Point64 pt2)
	{
		double num = pt2.X - pt1.X;
		double num2 = pt2.Y - pt1.Y;
		if (num == 0.0 && num2 == 0.0)
		{
			return default(PointD);
		}
		double num3 = 1.0 / Math.Sqrt(num * num + num2 * num2);
		num *= num3;
		num2 *= num3;
		return new PointD(num2, 0.0 - num);
	}

	private int GetLowestPolygonIdx(List<List<Point64>> paths)
	{
		Point64 point = new Point64(0L, long.MinValue);
		int result = -1;
		for (int i = 0; i < paths.Count; i++)
		{
			List<Point64> list = paths[i];
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j].Y >= point.Y && (list[j].Y > point.Y || list[j].X < point.X))
				{
					result = i;
					point = list[j];
				}
			}
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private PointD TranslatePoint(PointD pt, double dx, double dy)
	{
		return new PointD(pt.x + dx, pt.y + dy);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private PointD ReflectPoint(PointD pt, PointD pivot)
	{
		return new PointD(pivot.x + (pivot.x - pt.x), pivot.y + (pivot.y - pt.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool AlmostZero(double value, double epsilon = 0.001)
	{
		return Math.Abs(value) < epsilon;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private double Hypotenuse(double x, double y)
	{
		return Math.Sqrt(Math.Pow(x, 2.0) + Math.Pow(y, 2.0));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private PointD NormalizeVector(PointD vec)
	{
		double num = Hypotenuse(vec.x, vec.y);
		if (AlmostZero(num))
		{
			return new PointD(0L, 0L);
		}
		double num2 = 1.0 / num;
		return new PointD(vec.x * num2, vec.y * num2);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private PointD GetAvgUnitVector(PointD vec1, PointD vec2)
	{
		return NormalizeVector(new PointD(vec1.x + vec2.x, vec1.y + vec2.y));
	}

	private PointD IntersectPoint(PointD pt1a, PointD pt1b, PointD pt2a, PointD pt2b)
	{
		if (pt1a.x == pt1b.x)
		{
			if (pt2a.x == pt2b.x)
			{
				return new PointD(0L, 0L);
			}
			double num = (pt2b.y - pt2a.y) / (pt2b.x - pt2a.x);
			double num2 = pt2a.y - num * pt2a.x;
			return new PointD(pt1a.x, num * pt1a.x + num2);
		}
		if (pt2a.x == pt2b.x)
		{
			double num3 = (pt1b.y - pt1a.y) / (pt1b.x - pt1a.x);
			double num4 = pt1a.y - num3 * pt1a.x;
			return new PointD(pt2a.x, num3 * pt2a.x + num4);
		}
		double num5 = (pt1b.y - pt1a.y) / (pt1b.x - pt1a.x);
		double num6 = pt1a.y - num5 * pt1a.x;
		double num7 = (pt2b.y - pt2a.y) / (pt2b.x - pt2a.x);
		double num8 = pt2a.y - num7 * pt2a.x;
		if (num5 == num7)
		{
			return new PointD(0L, 0L);
		}
		double num9 = (num8 - num6) / (num5 - num7);
		return new PointD(num9, num5 * num9 + num6);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DoSquare(PathGroup group, List<Point64> path, int j, int k)
	{
		PointD avgUnitVector = GetAvgUnitVector(new PointD(0.0 - _normals[k].y, _normals[k].x), new PointD(_normals[j].y, 0.0 - _normals[j].x));
		PointD pt = new PointD(path[j]);
		pt = TranslatePoint(pt, _delta * avgUnitVector.x, _delta * avgUnitVector.y);
		PointD pt1a = TranslatePoint(pt, _delta * avgUnitVector.y, _delta * (0.0 - avgUnitVector.x));
		PointD pt1b = TranslatePoint(pt, _delta * (0.0 - avgUnitVector.y), _delta * avgUnitVector.x);
		PointD pt2a = new PointD((double)path[k].X + _normals[k].x * _delta, (double)path[k].Y + _normals[k].y * _delta);
		PointD pt2b = new PointD((double)path[j].X + _normals[k].x * _delta, (double)path[j].Y + _normals[k].y * _delta);
		PointD pt2 = IntersectPoint(pt1a, pt1b, pt2a, pt2b);
		group._outPath.Add(new Point64(pt2));
		group._outPath.Add(new Point64(ReflectPoint(pt2, pt)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void DoMiter(PathGroup group, List<Point64> path, int j, int k, double cosA)
	{
		double num = _delta / (cosA + 1.0);
		group._outPath.Add(new Point64((double)path[j].X + (_normals[k].x + _normals[j].x) * num, (double)path[j].Y + (_normals[k].y + _normals[j].y) * num));
	}

	private void DoRound(PathGroup group, Point64 pt, PointD normal1, PointD normal2, double angle)
	{
		PointD pointD = new PointD(normal2.x * _delta, normal2.y * _delta);
		int num = (int)Math.Round(_stepsPerRad * Math.Abs(angle) + 0.501);
		group._outPath.Add(new Point64((double)pt.X + pointD.x, (double)pt.Y + pointD.y));
		double num2 = Math.Sin(angle / (double)num);
		double num3 = Math.Cos(angle / (double)num);
		for (int i = 0; i < num; i++)
		{
			pointD = new PointD(pointD.x * num3 - num2 * pointD.y, pointD.x * num2 + pointD.y * num3);
			group._outPath.Add(new Point64((double)pt.X + pointD.x, (double)pt.Y + pointD.y));
		}
		group._outPath.Add(new Point64((double)pt.X + normal1.x * _delta, (double)pt.Y + normal1.y * _delta));
	}

	private void BuildNormals(List<Point64> path)
	{
		int count = path.Count;
		_normals.Clear();
		_normals.Capacity = count;
		for (int i = 0; i < count - 1; i++)
		{
			_normals.Add(GetUnitNormal(path[i], path[i + 1]));
		}
		_normals.Add(GetUnitNormal(path[count - 1], path[0]));
	}

	private void OffsetPoint(PathGroup group, List<Point64> path, int j, ref int k)
	{
		double num = _normals[k].x * _normals[j].y - _normals[j].x * _normals[k].y;
		if (num > 1.0)
		{
			num = 1.0;
		}
		else if (num < -1.0)
		{
			num = -1.0;
		}
		if (num * _delta < 0.0)
		{
			Point64 point = new Point64((double)path[j].X + _normals[k].x * _delta, (double)path[j].Y + _normals[k].y * _delta);
			Point64 point2 = new Point64((double)path[j].X + _normals[j].x * _delta, (double)path[j].Y + _normals[j].y * _delta);
			group._outPath.Add(point);
			if (point != point2)
			{
				group._outPath.Add(path[j]);
				group._outPath.Add(point2);
			}
		}
		else
		{
			double num2 = InternalClipper.DotProduct(_normals[j], _normals[k]);
			switch (_joinType)
			{
			case JoinType.Miter:
				if (1.0 + num2 < _tmpLimit)
				{
					DoSquare(group, path, j, k);
				}
				else
				{
					DoMiter(group, path, j, k, num2);
				}
				break;
			case JoinType.Square:
				if (num2 >= 0.0)
				{
					DoMiter(group, path, j, k, num2);
				}
				else
				{
					DoSquare(group, path, j, k);
				}
				break;
			default:
				DoRound(group, path[j], _normals[j], _normals[k], Math.Atan2(num, num2));
				break;
			}
		}
		k = j;
	}

	private void OffsetPolygon(PathGroup group, List<Point64> path)
	{
		group._outPath = new List<Point64>();
		int count = path.Count;
		int k = count - 1;
		for (int i = 0; i < count; i++)
		{
			OffsetPoint(group, path, i, ref k);
		}
		group._outPaths.Add(group._outPath);
	}

	private void OffsetOpenJoined(PathGroup group, List<Point64> path)
	{
		OffsetPolygon(group, path);
		path = Clipper.ReversePath(path);
		BuildNormals(path);
		OffsetPolygon(group, path);
	}

	private void OffsetOpenPath(PathGroup group, List<Point64> path, EndType endType)
	{
		group._outPath = new List<Point64>();
		int num = path.Count - 1;
		int k = 0;
		for (int i = 1; i < num; i++)
		{
			OffsetPoint(group, path, i, ref k);
		}
		num++;
		_normals[num - 1] = new PointD(0.0 - _normals[num - 2].x, 0.0 - _normals[num - 2].y);
		switch (endType)
		{
		case EndType.Butt:
			group._outPath.Add(new Point64((double)path[num - 1].X + _normals[num - 2].x * _delta, (double)path[num - 1].Y + _normals[num - 2].y * _delta));
			group._outPath.Add(new Point64((double)path[num - 1].X - _normals[num - 2].x * _delta, (double)path[num - 1].Y - _normals[num - 2].y * _delta));
			break;
		case EndType.Round:
			DoRound(group, path[num - 1], _normals[num - 1], _normals[num - 2], Math.PI);
			break;
		default:
			DoSquare(group, path, num - 1, num - 2);
			break;
		}
		for (int num2 = num - 2; num2 > 0; num2--)
		{
			_normals[num2] = new PointD(0.0 - _normals[num2 - 1].x, 0.0 - _normals[num2 - 1].y);
		}
		_normals[0] = new PointD(0.0 - _normals[1].x, 0.0 - _normals[1].y);
		k = num - 1;
		for (int num3 = num - 2; num3 > 0; num3--)
		{
			OffsetPoint(group, path, num3, ref k);
		}
		switch (endType)
		{
		case EndType.Butt:
			group._outPath.Add(new Point64((double)path[0].X + _normals[1].x * _delta, (double)path[0].Y + _normals[1].y * _delta));
			group._outPath.Add(new Point64((double)path[0].X - _normals[1].x * _delta, (double)path[0].Y - _normals[1].y * _delta));
			break;
		case EndType.Round:
			DoRound(group, path[0], _normals[0], _normals[1], Math.PI);
			break;
		default:
			DoSquare(group, path, 0, 1);
			break;
		}
		group._outPaths.Add(group._outPath);
	}

	private bool IsFullyOpenEndType(EndType et)
	{
		if (et != 0)
		{
			return et != EndType.Joined;
		}
		return false;
	}

	private void DoGroupOffset(PathGroup group, double delta)
	{
		if (group._endType != 0)
		{
			delta = Math.Abs(delta) / 2.0;
		}
		bool flag = !IsFullyOpenEndType(group._endType);
		if (flag)
		{
			int lowestPolygonIdx = GetLowestPolygonIdx(group._inPaths);
			if (lowestPolygonIdx < 0)
			{
				return;
			}
			double num = Clipper.Area(group._inPaths[lowestPolygonIdx]);
			if (num == 0.0)
			{
				return;
			}
			group._pathsReversed = num < 0.0;
			if (group._pathsReversed)
			{
				delta = 0.0 - delta;
			}
		}
		else
		{
			group._pathsReversed = false;
		}
		_delta = delta;
		double num2 = Math.Abs(_delta);
		_joinType = group._joinType;
		if (group._joinType == JoinType.Round || group._endType == EndType.Round)
		{
			double num3 = ((ArcTolerance > 0.01) ? ArcTolerance : (Math.Log10(2.0 + num2) * 0.25));
			_stepsPerRad = Math.PI / Math.Acos(1.0 - num3 / num2) / (Math.PI * 2.0);
		}
		foreach (List<Point64> inPath in group._inPaths)
		{
			List<Point64> list = Clipper.StripDuplicates(inPath, flag);
			int count = list.Count;
			if (count == 0 || (count < 3 && !IsFullyOpenEndType(group._endType)))
			{
				continue;
			}
			if (count == 1)
			{
				group._outPath = new List<Point64>();
				if (group._endType == EndType.Round)
				{
					DoRound(group, list[0], new PointD(1.0, 0.0), new PointD(-1.0, 0.0), Math.PI * 2.0);
				}
				else
				{
					group._outPath.Capacity = 4;
					group._outPath.Add(new Point64((double)list[0].X - _delta, (double)list[0].Y - _delta));
					group._outPath.Add(new Point64((double)list[0].X + _delta, (double)list[0].Y - _delta));
					group._outPath.Add(new Point64((double)list[0].X + _delta, (double)list[0].Y + _delta));
					group._outPath.Add(new Point64((double)list[0].X - _delta, (double)list[0].Y + _delta));
				}
				group._outPaths.Add(group._outPath);
			}
			else
			{
				BuildNormals(list);
				if (group._endType == EndType.Polygon)
				{
					OffsetPolygon(group, list);
				}
				else if (group._endType == EndType.Joined)
				{
					OffsetOpenJoined(group, list);
				}
				else
				{
					OffsetOpenPath(group, list, group._endType);
				}
			}
		}
		if (!MergeGroups)
		{
			Clipper64 clipper = new Clipper64
			{
				PreserveCollinear = PreserveCollinear,
				ReverseSolution = (ReverseSolution != group._pathsReversed)
			};
			clipper.AddSubject(group._outPaths);
			if (group._pathsReversed)
			{
				clipper.Execute(ClipType.Union, FillRule.Negative, group._outPaths);
			}
			else
			{
				clipper.Execute(ClipType.Union, FillRule.Positive, group._outPaths);
			}
		}
		solution.AddRange(group._outPaths);
		group._outPaths.Clear();
	}
}
