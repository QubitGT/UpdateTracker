using UnityEngine;

namespace Unity.MemoryProfiler;

internal static class MetadataInjector
{
	public static DefaultMetadataCollect DefaultCollector;

	public static long CollectorCount;

	public static byte DefaultCollectorInjected;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
	private static void PlayerInitMetadata()
	{
		if (!Application.isEditor)
		{
			DefaultCollector?.Dispose();
			DefaultCollector = null;
			DefaultCollectorInjected = 0;
			CollectorCount = 0L;
		}
		InitializeMetadataCollection();
	}

	private static void InitializeMetadataCollection()
	{
		DefaultCollector = new DefaultMetadataCollect();
	}
}
