using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion;

public sealed class DebugLogStream : IDisposable
{
	public readonly LogStream InfoStream;

	public readonly LogStream WarnStream;

	public readonly LogStream ErrorStream;

	public DebugLogStream(LogStream innerStream, LogStream warnStream, LogStream errorStream)
	{
		InfoStream = innerStream ?? throw new ArgumentNullException("innerStream");
		WarnStream = warnStream ?? throw new ArgumentNullException("warnStream");
		ErrorStream = errorStream ?? throw new ArgumentNullException("errorStream");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Log(ILogSource source, string message)
	{
		InfoStream.Log(source, message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Log(string message)
	{
		InfoStream.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Info(ILogSource source, string message)
	{
		InfoStream.Log(source, message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Info(string message)
	{
		InfoStream.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Error(ILogSource source, string message)
	{
		ErrorStream.Log(source, message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Error(string message)
	{
		ErrorStream.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Error(Exception message)
	{
		ErrorStream.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Exception(Exception message)
	{
		ErrorStream.Log(message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Warn(ILogSource source, string message)
	{
		WarnStream.Log(source, message);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	public void Warn(string message)
	{
		WarnStream.Log(message);
	}

	public void Dispose()
	{
		InfoStream.Dispose();
		WarnStream.Dispose();
		ErrorStream.Dispose();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	internal void Log(object message)
	{
	}
}
