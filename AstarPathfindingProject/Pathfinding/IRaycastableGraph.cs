using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding;

public interface IRaycastableGraph
{
	bool Linecast(Vector3 start, Vector3 end);

	[Obsolete]
	bool Linecast(Vector3 start, Vector3 end, GraphNode hint);

	[Obsolete]
	bool Linecast(Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit);

	[Obsolete]
	bool Linecast(Vector3 start, Vector3 end, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace);

	bool Linecast(Vector3 start, Vector3 end, out GraphHitInfo hit, List<GraphNode> trace = null, Func<GraphNode, bool> filter = null);
}
