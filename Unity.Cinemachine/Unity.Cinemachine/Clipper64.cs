using System.Collections.Generic;

namespace Unity.Cinemachine;

internal class Clipper64 : ClipperBase
{
	internal new void AddPath(List<Point64> path, PathType polytype, bool isOpen = false)
	{
		base.AddPath(path, polytype, isOpen);
	}

	internal new void AddPaths(List<List<Point64>> paths, PathType polytype, bool isOpen = false)
	{
		base.AddPaths(paths, polytype, isOpen);
	}

	public void AddSubject(List<List<Point64>> paths)
	{
		AddPaths(paths, PathType.Subject);
	}

	public void AddOpenSubject(List<List<Point64>> paths)
	{
		AddPaths(paths, PathType.Subject, isOpen: true);
	}

	public void AddClip(List<List<Point64>> paths)
	{
		AddPaths(paths, PathType.Clip);
	}

	public bool Execute(ClipType clipType, FillRule fillRule, List<List<Point64>> solutionClosed, List<List<Point64>> solutionOpen)
	{
		solutionClosed.Clear();
		solutionOpen.Clear();
		try
		{
			ExecuteInternal(clipType, fillRule);
			BuildPaths(solutionClosed, solutionOpen);
		}
		catch
		{
			_succeeded = false;
		}
		ClearSolution();
		return _succeeded;
	}

	public bool Execute(ClipType clipType, FillRule fillRule, List<List<Point64>> solutionClosed)
	{
		return Execute(clipType, fillRule, solutionClosed, new List<List<Point64>>());
	}

	public bool Execute(ClipType clipType, FillRule fillRule, PolyTree64 polytree, List<List<Point64>> openPaths)
	{
		polytree.Clear();
		openPaths.Clear();
		_using_polytree = true;
		try
		{
			ExecuteInternal(clipType, fillRule);
			BuildTree(polytree, openPaths);
		}
		catch
		{
			_succeeded = false;
		}
		ClearSolution();
		return _succeeded;
	}

	public bool Execute(ClipType clipType, FillRule fillRule, PolyTree64 polytree)
	{
		return Execute(clipType, fillRule, polytree, new List<List<Point64>>());
	}
}
