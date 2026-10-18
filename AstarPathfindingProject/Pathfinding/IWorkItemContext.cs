using System;

namespace Pathfinding;

public interface IWorkItemContext
{
	[Obsolete("Avoid using. This will force a full recalculation of the connected components. In most cases the HierarchicalGraph class takes care of things automatically behind the scenes now. In pretty much all cases you should be able to remove the call to this function.")]
	void QueueFloodFill();

	void EnsureValidFloodFill();

	void SetGraphDirty(NavGraph graph);
}
