namespace Meta.Voice.TelemetryUtilities.PerformanceTracing;

public static class VsdkProfiler
{
	public static ITraceProvider traceProvider = new UnityProfilerTraceProvider();

	public static bool profilingEnabled = false;

	public static void BeginSample(string sampleName)
	{
		if (profilingEnabled)
		{
			traceProvider.BeginSample(sampleName);
		}
	}

	public static void EndSample(string sampleName)
	{
		if (profilingEnabled)
		{
			traceProvider.EndSample(sampleName);
		}
	}
}
