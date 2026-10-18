using System.Diagnostics;

namespace Fusion.Statistics;

internal class LagCompensationStatisticsManager
{
	private LagCompensationStatisticsSnapshot _previousUpdateSnapshot;

	private LagCompensationStatisticsSnapshot _currentUpdateSnapshot;

	public LagCompensationStatisticsSnapshot CompletedSnapshot => _previousUpdateSnapshot;

	internal LagCompensationStatisticsSnapshot PendingSnapshot => _currentUpdateSnapshot;

	internal LagCompensationStatisticsManager()
	{
		_previousUpdateSnapshot = new LagCompensationStatisticsSnapshot();
		_currentUpdateSnapshot = new LagCompensationStatisticsSnapshot();
	}

	[Conditional("DEBUG")]
	internal void FinishPendingSnapshot()
	{
		_previousUpdateSnapshot.CopyFromSnapshot(_currentUpdateSnapshot);
		_currentUpdateSnapshot.ClearSnapshot();
	}
}
