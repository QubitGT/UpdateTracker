using JetBrains.Annotations;

namespace Fusion;

public static class InternalLogStreams
{
	[CanBeNull]
	public static DebugLogStream LogDebug;

	[CanBeNull]
	public static LogStream LogInfo;

	[CanBeNull]
	public static LogStream LogWarn;

	[CanBeNull]
	public static LogStream LogError;

	[CanBeNull]
	public static LogStream LogException;

	[CanBeNull]
	public static TraceLogStream LogTrace;

	[CanBeNull]
	internal static TraceLogStream LogTraceStun;

	[CanBeNull]
	internal static TraceLogStream LogTraceObject;

	[CanBeNull]
	internal static TraceLogStream LogTraceNetwork;

	[CanBeNull]
	internal static TraceLogStream LogTracePrefab;

	[CanBeNull]
	internal static TraceLogStream LogTraceSceneInfo;

	[CanBeNull]
	internal static TraceLogStream LogTraceSceneManager;

	[CanBeNull]
	internal static TraceLogStream LogTraceSimulationMessage;

	[CanBeNull]
	internal static TraceLogStream LogTraceHostMigration;

	[CanBeNull]
	internal static TraceLogStream LogTraceEncryption;

	[CanBeNull]
	internal static TraceLogStream LogTraceDummyTraffic;

	[CanBeNull]
	internal static TraceLogStream LogTraceRealtime;

	[CanBeNull]
	internal static TraceLogStream LogTraceMemoryTrack;

	[CanBeNull]
	internal static TraceLogStream LogTraceSnapshots;

	[CanBeNull]
	internal static TraceLogStream LogTraceTime;
}
