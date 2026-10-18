using Liv.Lck.DependencyInjection;
using UnityEngine;

namespace Liv.Lck.Streaming;

internal static class StreamingModuleInitializer
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Initialize()
	{
		LckModuleLoader.RegisterModule(delegate(LckDiContainer container)
		{
			container.AddSingleton<ILckNativeStreamingService, LckNativeStreamingService>();
			container.AddSingleton<ILckStreamer, LckStreamer>();
			LckLog.Log("LCK: Loaded module - Liv.Lck.Streaming", "Initialize", ".\\Packages\\tv.liv.lck-streaming\\Runtime\\Scripts\\LckStreamingModuleInitializer.cs", 18);
		}, "Liv.Lck.Streaming");
	}
}
