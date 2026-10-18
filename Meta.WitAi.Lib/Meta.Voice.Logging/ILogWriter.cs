namespace Meta.Voice.Logging;

public interface ILogWriter
{
	void WriteVerbose(string message);

	void WriteDebug(string message);

	void WriteInfo(string message);

	void WriteWarning(string message);

	void WriteError(string message);
}
