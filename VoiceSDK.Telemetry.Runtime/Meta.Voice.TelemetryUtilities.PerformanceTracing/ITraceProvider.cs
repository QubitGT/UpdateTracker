namespace Meta.Voice.TelemetryUtilities.PerformanceTracing;

public interface ITraceProvider
{
	void BeginSample(string sampleName);

	void EndSample(string sampleName);
}
