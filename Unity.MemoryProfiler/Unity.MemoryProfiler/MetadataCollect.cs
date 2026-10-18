using System;
using Unity.Profiling.Memory;

namespace Unity.MemoryProfiler;

public abstract class MetadataCollect : IDisposable
{
	private bool disposed;

	protected MetadataCollect()
	{
		if (MetadataInjector.DefaultCollector != null && MetadataInjector.DefaultCollector != this && MetadataInjector.DefaultCollectorInjected != 0)
		{
			Unity.Profiling.Memory.MemoryProfiler.CreatingMetadata -= MetadataInjector.DefaultCollector.CollectMetadata;
			MetadataInjector.CollectorCount--;
			MetadataInjector.DefaultCollectorInjected = 0;
		}
		Unity.Profiling.Memory.MemoryProfiler.CreatingMetadata += CollectMetadata;
		MetadataInjector.CollectorCount++;
	}

	public abstract void CollectMetadata(MemorySnapshotMetadata data);

	public void Dispose()
	{
		if (!disposed)
		{
			disposed = true;
			Unity.Profiling.Memory.MemoryProfiler.CreatingMetadata -= CollectMetadata;
			MetadataInjector.CollectorCount--;
			if (MetadataInjector.DefaultCollector != null && MetadataInjector.CollectorCount < 1 && MetadataInjector.DefaultCollector != this)
			{
				MetadataInjector.DefaultCollectorInjected = 1;
				Unity.Profiling.Memory.MemoryProfiler.CreatingMetadata += MetadataInjector.DefaultCollector.CollectMetadata;
				MetadataInjector.CollectorCount++;
			}
		}
	}
}
