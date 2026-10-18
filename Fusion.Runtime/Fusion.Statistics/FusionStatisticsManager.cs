#define DEBUG
using System.Diagnostics;

namespace Fusion.Statistics;

public class FusionStatisticsManager
{
	private FusionStatisticsSnapshot _currentTickSnapshot;

	private FusionStatisticsSnapshot _previousTickSnapshot;

	private NetworkObjectStatisticsManager _objectStatisticsManager;

	public FusionStatisticsSnapshot CompleteSnapshot => _previousTickSnapshot;

	internal FusionStatisticsSnapshot PendingSnapshot => _currentTickSnapshot;

	public NetworkObjectStatisticsManager ObjectStatisticsManager => _objectStatisticsManager;

	internal FusionStatisticsManager()
	{
		_currentTickSnapshot = new FusionStatisticsSnapshot();
		_previousTickSnapshot = new FusionStatisticsSnapshot();
		_objectStatisticsManager = new NetworkObjectStatisticsManager();
	}

	[Conditional("DEBUG")]
	internal void FinishPendingSnapshot()
	{
		_previousTickSnapshot.CopyFrom(_currentTickSnapshot);
		_currentTickSnapshot.ClearSnapshot();
		_objectStatisticsManager.CollectStatistics();
	}
}
