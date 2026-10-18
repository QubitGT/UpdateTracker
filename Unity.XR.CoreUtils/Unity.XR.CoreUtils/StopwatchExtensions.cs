using System.Diagnostics;

namespace Unity.XR.CoreUtils;

public static class StopwatchExtensions
{
	public static void Restart(this Stopwatch stopwatch)
	{
		stopwatch.Stop();
		stopwatch.Reset();
		stopwatch.Start();
	}
}
