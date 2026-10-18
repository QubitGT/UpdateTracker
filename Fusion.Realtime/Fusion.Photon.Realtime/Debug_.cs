#define TRACE
using System;
using System.Diagnostics;

namespace Fusion.Photon.Realtime;

internal static class Debug_
{
	[Conditional("DEBUG")]
	public static void Log(string msg)
	{
		InternalLogStreams.LogTraceRealtime?.Log(msg);
	}

	[Conditional("DEBUG")]
	public static void LogWarning(string msg)
	{
		InternalLogStreams.LogWarn?.Log(msg);
	}

	[Conditional("DEBUG")]
	public static void LogError(string msg)
	{
		InternalLogStreams.LogError?.Log(msg);
	}

	[Conditional("DEBUG")]
	public static void LogException(Exception ex)
	{
		InternalLogStreams.LogException?.Log(ex);
	}
}
