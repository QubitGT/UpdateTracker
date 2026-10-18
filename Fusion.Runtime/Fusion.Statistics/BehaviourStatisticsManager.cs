#define DEBUG
using System.Diagnostics;

namespace Fusion.Statistics;

public class BehaviourStatisticsManager
{
	private BehaviourStatisticsSnapshot _previousUpdateSnapshot;

	private BehaviourStatisticsSnapshot _currentUpdateSnapshot;

	public BehaviourStatisticsSnapshot CompletedSnapshot => _previousUpdateSnapshot;

	internal BehaviourStatisticsSnapshot PendingSnapshot => _currentUpdateSnapshot;

	internal BehaviourStatisticsManager()
	{
		_previousUpdateSnapshot = new BehaviourStatisticsSnapshot();
		_currentUpdateSnapshot = new BehaviourStatisticsSnapshot();
	}

	[Conditional("DEBUG")]
	internal void FinishPendingSnapshot()
	{
		_previousUpdateSnapshot.CopyFromSnapshot(_currentUpdateSnapshot);
		_currentUpdateSnapshot.ClearSnapshot();
	}
}
