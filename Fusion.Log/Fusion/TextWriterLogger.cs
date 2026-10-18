using System;
using System.IO;
using System.Text;

namespace Fusion;

[Obsolete]
public class TextWriterLogger : ILogger, IDisposable
{
	private StringBuilder _builder = new StringBuilder();

	private TextWriter _writer;

	private bool _disposeWriter;

	public TextWriterLogger(TextWriter writer, bool disposeWriter)
	{
		if (writer == null)
		{
			throw new ArgumentNullException("writer");
		}
		_writer = writer;
		_disposeWriter = disposeWriter;
	}

	public virtual void Dispose()
	{
		if (_disposeWriter && _writer != null)
		{
			TextWriter writer = _writer;
			_writer = null;
			writer.Dispose();
		}
	}

	public virtual void Log(LogType logType, object message, in LogContext logContext)
	{
		try
		{
			switch (logType)
			{
			case LogType.Debug:
				_builder.Append("[DEBUG] ");
				break;
			case LogType.Trace:
				_builder.Append("[TRACE] ");
				break;
			}
			if (!string.IsNullOrEmpty(logContext.Prefix))
			{
				_builder.Append(logContext.Prefix);
				_builder.Append(": ");
			}
			_builder.Append(message);
			_writer.WriteLine(_builder.ToString());
		}
		finally
		{
			_builder.Clear();
		}
	}

	public virtual void LogException(Exception ex, in LogContext logContext)
	{
		try
		{
			_builder.Append(logContext.Prefix);
			_builder.Append(ex.Message);
			_writer.WriteLine(_builder.ToString());
			_writer.WriteLine(ex.StackTrace);
		}
		finally
		{
			_builder.Clear();
		}
	}
}
