using System;
using System.IO;
using System.Text;

namespace Fusion;

public class TextWriterLogStream : LogStream
{
	private StringBuilder _builder = new StringBuilder();

	private TextWriter _writer;

	private bool _disposeWriter;

	private string _prefix;

	public TextWriterLogStream(TextWriter writer, bool disposeWriter, string prefix = null)
	{
		_writer = writer ?? throw new ArgumentNullException("writer");
		_disposeWriter = disposeWriter;
		_prefix = prefix;
	}

	public override void Log(ILogSource source, string message)
	{
		Log(message);
	}

	public override void Log(string message)
	{
		try
		{
			if (!string.IsNullOrEmpty(_prefix))
			{
				_builder.Append(_prefix);
				_builder.Append(" ");
			}
			_builder.Append(message);
			_writer.WriteLine(_builder.ToString());
		}
		finally
		{
			_builder.Clear();
		}
	}

	public override void Log(ILogSource source, string message, Exception error)
	{
		Log(message, error);
	}

	public override void Log(string message, Exception error)
	{
		try
		{
			if (!string.IsNullOrEmpty(_prefix))
			{
				_builder.Append(_prefix);
				_builder.Append(" ");
			}
			if (!string.IsNullOrEmpty(message))
			{
				_builder.Append(message);
				_builder.Append(" ");
			}
			_builder.Append(error.Message);
			_writer.WriteLine(_builder.ToString());
			_writer.WriteLine(error.StackTrace);
		}
		finally
		{
			_builder.Clear();
		}
	}

	public override void Log(Exception error)
	{
		Log((string)null, error);
	}

	public override void Dispose()
	{
		if (_disposeWriter && _writer != null)
		{
			TextWriter writer = _writer;
			_writer = null;
			writer.Dispose();
		}
	}
}
