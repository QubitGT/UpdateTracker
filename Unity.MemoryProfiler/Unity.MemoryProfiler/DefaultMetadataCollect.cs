using System;
using Unity.Profiling.Memory;
using UnityEngine;

namespace Unity.MemoryProfiler;

internal class DefaultMetadataCollect : MetadataCollect
{
	public DefaultMetadataCollect()
	{
		MetadataInjector.DefaultCollectorInjected = 1;
	}

	public override void CollectMetadata(MemorySnapshotMetadata data)
	{
		data.Description = data.Description + "Project name: " + Application.productName + "\n" + $"This Memory Snapshot capture started at {DateTime.UtcNow} (UTC)\n" + $"Time.frameCount: {Time.frameCount}\n" + "Time.realtimeSinceStartup: " + FormatSecondsToTime(Time.realtimeSinceStartupAsDouble) + "\n";
	}

	private string FormatSecondsToTime(double timeInSeconds)
	{
		int num = (int)timeInSeconds;
		int num2 = (int)((timeInSeconds - (double)num) * 1000.0);
		int num3 = num / 60;
		num %= 60;
		int num4 = num3 / 60;
		num3 %= 60;
		return $"{num4:00}:{num3:00}:{num:00}.{num2:000}";
	}
}
