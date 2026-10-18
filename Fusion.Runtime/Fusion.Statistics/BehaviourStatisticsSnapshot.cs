using System.Diagnostics;

namespace Fusion.Statistics;

public class BehaviourStatisticsSnapshot
{
	public int FixedUpdateNetworkExecutionCount { get; private set; }

	public int RenderExecutionCount { get; private set; }

	public double FixedUpdateNetworkExecutionTime { get; private set; }

	public double RenderExecutionTime { get; private set; }

	[Conditional("DEBUG")]
	internal void ClearSnapshot()
	{
		FixedUpdateNetworkExecutionCount = 0;
		FixedUpdateNetworkExecutionTime = 0.0;
		RenderExecutionCount = 0;
		RenderExecutionTime = 0.0;
	}

	[Conditional("DEBUG")]
	internal void CopyFromSnapshot(BehaviourStatisticsSnapshot snapshot)
	{
		FixedUpdateNetworkExecutionCount = snapshot.FixedUpdateNetworkExecutionCount;
		RenderExecutionCount = snapshot.RenderExecutionCount;
		FixedUpdateNetworkExecutionTime = snapshot.FixedUpdateNetworkExecutionTime;
		RenderExecutionTime = snapshot.RenderExecutionTime;
	}

	[Conditional("DEBUG")]
	internal void AccumulateFixedUpdateNetworkExecutionCount(int count)
	{
		FixedUpdateNetworkExecutionCount += count;
	}

	[Conditional("DEBUG")]
	internal void AccumulateRenderExecutionCount(int count)
	{
		RenderExecutionCount += count;
	}

	[Conditional("DEBUG")]
	internal void AccumulateFixedUpdateNetworkExecutionTime(double time)
	{
		FixedUpdateNetworkExecutionTime += time;
	}

	[Conditional("DEBUG")]
	internal void AccumulateRenderExecutionTime(double time)
	{
		RenderExecutionTime += time;
	}
}
