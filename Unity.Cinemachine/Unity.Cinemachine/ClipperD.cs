using System;
using System.Collections.Generic;

namespace Unity.Cinemachine;

internal class ClipperD : ClipperBase
{
	private readonly double _scale;

	private readonly double _invScale;

	public ClipperD(int roundingDecimalPrecision = 2)
	{
		if (roundingDecimalPrecision < -8 || roundingDecimalPrecision > 8)
		{
			throw new ClipperLibException("Error - RoundingDecimalPrecision exceeds the allowed range.");
		}
		_scale = Math.Pow(10.0, roundingDecimalPrecision);
		_invScale = 1.0 / _scale;
	}

	public void AddPath(List<PointD> path, PathType polytype, bool isOpen = false)
	{
		AddPath(Clipper.ScalePath64(path, _scale), polytype, isOpen);
	}

	public void AddPaths(List<List<PointD>> paths, PathType polytype, bool isOpen = false)
	{
		AddPaths(Clipper.ScalePaths64(paths, _scale), polytype, isOpen);
	}

	public void AddSubject(List<PointD> path)
	{
		AddPath(path, PathType.Subject);
	}

	public void AddOpenSubject(List<PointD> path)
	{
		AddPath(path, PathType.Subject, isOpen: true);
	}

	public void AddClip(List<PointD> path)
	{
		AddPath(path, PathType.Clip);
	}

	public void AddSubject(List<List<PointD>> paths)
	{
		AddPaths(paths, PathType.Subject);
	}

	public void AddOpenSubject(List<List<PointD>> paths)
	{
		AddPaths(paths, PathType.Subject, isOpen: true);
	}

	public void AddClip(List<List<PointD>> paths)
	{
		AddPaths(paths, PathType.Clip);
	}

	public bool Execute(ClipType clipType, FillRule fillRule, List<List<PointD>> solutionClosed, List<List<PointD>> solutionOpen)
	{
		List<List<Point64>> list = new List<List<Point64>>();
		List<List<Point64>> list2 = new List<List<Point64>>();
		bool flag = true;
		solutionClosed.Clear();
		solutionOpen.Clear();
		try
		{
			ExecuteInternal(clipType, fillRule);
			BuildPaths(list, list2);
		}
		catch
		{
			flag = false;
		}
		ClearSolution();
		if (!flag)
		{
			return false;
		}
		solutionClosed.Capacity = list.Count;
		foreach (List<Point64> item in list)
		{
			solutionClosed.Add(Clipper.ScalePathD(item, _invScale));
		}
		solutionOpen.Capacity = list2.Count;
		foreach (List<Point64> item2 in list2)
		{
			solutionOpen.Add(Clipper.ScalePathD(item2, _invScale));
		}
		return true;
	}

	public bool Execute(ClipType clipType, FillRule fillRule, List<List<PointD>> solutionClosed)
	{
		return Execute(clipType, fillRule, solutionClosed, new List<List<PointD>>());
	}

	public bool Execute(ClipType clipType, FillRule fillRule, PolyTreeD polytree, List<List<PointD>> openPaths)
	{
		polytree.Clear();
		((PolyPathD)polytree).Scale = _scale;
		openPaths.Clear();
		List<List<Point64>> list = new List<List<Point64>>();
		bool flag = true;
		try
		{
			ExecuteInternal(clipType, fillRule);
			BuildTree(polytree, list);
		}
		catch
		{
			flag = false;
		}
		ClearSolution();
		if (!flag)
		{
			return false;
		}
		if (list.Count > 0)
		{
			openPaths.Capacity = list.Count;
			foreach (List<Point64> item in list)
			{
				openPaths.Add(Clipper.ScalePathD(item, _invScale));
			}
		}
		return true;
	}

	public bool Execute(ClipType clipType, FillRule fillRule, PolyTreeD polytree)
	{
		return Execute(clipType, fillRule, polytree, new List<List<PointD>>());
	}
}
