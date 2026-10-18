namespace Meta.Voice.Logging;

internal class VLoggerFactory : IVLoggerFactory
{
	public IVLogger GetLogger(string category, ILogSink logSink)
	{
		return new VLogger(category, logSink);
	}
}
