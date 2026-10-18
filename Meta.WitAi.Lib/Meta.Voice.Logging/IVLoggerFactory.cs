namespace Meta.Voice.Logging;

public interface IVLoggerFactory
{
	IVLogger GetLogger(string category, ILogSink logSink);
}
