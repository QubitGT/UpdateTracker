using System;

namespace Fusion;

public class ConsoleLogStream : TextWriterLogStream
{
	private readonly ConsoleColor _color;

	public ConsoleLogStream(ConsoleColor color, string prefix = null)
		: base(Console.Out, disposeWriter: false, prefix)
	{
		_color = color;
	}

	public override void Log(ILogSource source, string message)
	{
		if (Console.ForegroundColor == _color)
		{
			base.Log(source, message);
			return;
		}
		Console.ForegroundColor = _color;
		try
		{
			base.Log(source, message);
		}
		finally
		{
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}

	public override void Log(ILogSource source, string message, Exception error)
	{
		if (Console.ForegroundColor == ConsoleColor.Red)
		{
			base.Log(source, message, error);
			return;
		}
		Console.ForegroundColor = _color;
		try
		{
			base.Log(source, message, error);
		}
		finally
		{
			Console.ForegroundColor = ConsoleColor.Gray;
		}
	}
}
