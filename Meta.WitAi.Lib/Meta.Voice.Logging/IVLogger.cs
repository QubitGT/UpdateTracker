namespace Meta.Voice.Logging;

public interface IVLogger : ICoreLogger
{
	void Flush(CorrelationID correlationId);

	void Flush();
}
