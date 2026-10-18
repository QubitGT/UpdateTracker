using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Fusion;

public abstract class LogStream : IDisposable
{
	public virtual void Log(ILogSource source, string message)
	{
		Log(message);
	}

	public abstract void Log(string message);

	public virtual void Log(ILogSource source, string message, Exception error)
	{
		Log(error);
	}

	public virtual void Log(ILogSource source, Exception error)
	{
		Log(error);
	}

	public virtual void Log(string message, Exception error)
	{
		Log(error);
	}

	public abstract void Log(Exception error);

	public virtual void Dispose()
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG")]
	internal void Log(object message)
	{
		Log($"{message}");
	}

	[CanBeNull]
	public LogStream Once(ref bool flag)
	{
		if (!flag)
		{
			flag = true;
			return this;
		}
		return null;
	}
}
