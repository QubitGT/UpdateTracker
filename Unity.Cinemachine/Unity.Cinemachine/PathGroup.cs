using System.Collections.Generic;

namespace Unity.Cinemachine;

internal class PathGroup
{
	internal List<List<Point64>> _inPaths;

	internal List<Point64> _outPath;

	internal List<List<Point64>> _outPaths;

	internal JoinType _joinType;

	internal EndType _endType;

	internal bool _pathsReversed;

	public PathGroup(List<List<Point64>> paths, JoinType joinType, EndType endType = EndType.Polygon)
	{
		_inPaths = new List<List<Point64>>(paths);
		_joinType = joinType;
		_endType = endType;
		_outPath = new List<Point64>();
		_outPaths = new List<List<Point64>>();
		_pathsReversed = false;
	}
}
