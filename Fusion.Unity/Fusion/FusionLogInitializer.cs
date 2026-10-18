using System.Threading;
using UnityEngine;

namespace Fusion;

public static class FusionLogInitializer
{
	private static FusionUnityLogger CreateLogger(bool isDarkMode)
	{
		return new FusionUnityLogger(Thread.CurrentThread, isDarkMode);
	}

	[RuntimeInitializeOnLoadMethod]
	public static void Initialize()
	{
		bool isDarkMode = false;
		LogLevel logLevel = LogLevel.Info;
		TraceChannels traceChannels = (TraceChannels)0;
		traceChannels = traceChannels.AddChannelsFromDefines();
		if (!Log.IsInitialized)
		{
			FusionUnityLogger @object = CreateLogger(isDarkMode);
			Log.Initialize(logLevel, @object.CreateLogStream, traceChannels);
		}
	}
}
