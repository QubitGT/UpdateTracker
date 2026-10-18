using UnityEngine;

namespace Fusion.Statistics;

internal static class FusionStatisticsHelper
{
	public const float DEFAULT_GRAPH_HEIGHT = 150f;

	public const float DEFAULT_HEADER_HEIGHT = 50f;

	internal static void GetStatGraphDefaultSettings(RenderSimStats stat, out string valueTextFormat, out float valueTextMultiplier, out bool ignoreZeroOnAverage, out bool ignoreZeroOnBuffer, out int accumulateTimeMs)
	{
		valueTextFormat = "{0:0}";
		valueTextMultiplier = 1f;
		ignoreZeroOnAverage = false;
		ignoreZeroOnBuffer = false;
		accumulateTimeMs = 0;
		switch (stat)
		{
		case RenderSimStats.InPackets:
		case RenderSimStats.OutPackets:
		case RenderSimStats.InObjectUpdates:
		case RenderSimStats.OutObjectUpdates:
			valueTextFormat = "{0:0}";
			accumulateTimeMs = 1000;
			break;
		case RenderSimStats.RTT:
			valueTextFormat = "{0:0} ms";
			valueTextMultiplier = 1000f;
			ignoreZeroOnAverage = true;
			ignoreZeroOnBuffer = true;
			break;
		case RenderSimStats.InBandwidth:
		case RenderSimStats.OutBandwidth:
		case RenderSimStats.InputInBandwidth:
		case RenderSimStats.InputOutBandwidth:
			valueTextFormat = "{0:0} B";
			accumulateTimeMs = 1000;
			break;
		case RenderSimStats.AverageInPacketSize:
		case RenderSimStats.AverageOutPacketSize:
			valueTextFormat = "{0:0} B";
			ignoreZeroOnBuffer = true;
			ignoreZeroOnAverage = true;
			break;
		case RenderSimStats.Resimulations:
			valueTextFormat = "{0:0}";
			break;
		case RenderSimStats.ForwardTicks:
			valueTextFormat = "{0:0}";
			break;
		case RenderSimStats.TimeResets:
		case RenderSimStats.SimulationSpeed:
		case RenderSimStats.InterpolationSpeed:
			valueTextFormat = "{0:0}";
			break;
		case RenderSimStats.InputReceiveDelta:
		case RenderSimStats.StateReceiveDelta:
		case RenderSimStats.SimulationTimeOffset:
		case RenderSimStats.InterpolationOffset:
			valueTextMultiplier = 1000f;
			valueTextFormat = "{0:0} ms";
			break;
		case RenderSimStats.ObjectsAllocatedMemoryInUse:
		case RenderSimStats.GeneralAllocatedMemoryInUse:
		case RenderSimStats.ObjectsAllocatedMemoryFree:
		case RenderSimStats.GeneralAllocatedMemoryFree:
			valueTextFormat = "{0:0} B";
			break;
		case RenderSimStats.WordsWrittenCount:
		case RenderSimStats.WordsReadCount:
			valueTextFormat = "{0:0}";
			ignoreZeroOnBuffer = true;
			accumulateTimeMs = 1000;
			break;
		case RenderSimStats.WordsWrittenSize:
		case RenderSimStats.WordsReadSize:
			valueTextFormat = "{0:0} B";
			ignoreZeroOnBuffer = true;
			accumulateTimeMs = 1000;
			break;
		default:
			valueTextFormat = "{0:0}";
			break;
		}
	}

	internal static float GetStatDataFromSnapshot(RenderSimStats stat, FusionStatisticsSnapshot simulationStatsSnapshot)
	{
		return stat switch
		{
			RenderSimStats.InPackets => simulationStatsSnapshot.InPackets, 
			RenderSimStats.OutPackets => simulationStatsSnapshot.OutPackets, 
			RenderSimStats.RTT => simulationStatsSnapshot.RoundTripTime, 
			RenderSimStats.InBandwidth => simulationStatsSnapshot.InBandwidth, 
			RenderSimStats.OutBandwidth => simulationStatsSnapshot.OutBandwidth, 
			RenderSimStats.Resimulations => simulationStatsSnapshot.Resimulations, 
			RenderSimStats.ForwardTicks => simulationStatsSnapshot.ForwardTicks, 
			RenderSimStats.InputInBandwidth => simulationStatsSnapshot.InputInBandwidth, 
			RenderSimStats.InputOutBandwidth => simulationStatsSnapshot.InputOutBandwidth, 
			RenderSimStats.AverageInPacketSize => simulationStatsSnapshot.InBandwidth / (float)Mathf.Max(simulationStatsSnapshot.InPackets, 1), 
			RenderSimStats.AverageOutPacketSize => simulationStatsSnapshot.OutBandwidth / (float)Mathf.Max(simulationStatsSnapshot.OutPackets, 1), 
			RenderSimStats.InObjectUpdates => simulationStatsSnapshot.InObjectUpdates, 
			RenderSimStats.OutObjectUpdates => simulationStatsSnapshot.OutObjectUpdates, 
			RenderSimStats.ObjectsAllocatedMemoryInUse => simulationStatsSnapshot.ObjectsAllocMemoryUsedInBytes, 
			RenderSimStats.GeneralAllocatedMemoryInUse => simulationStatsSnapshot.GeneralAllocMemoryUsedInBytes, 
			RenderSimStats.ObjectsAllocatedMemoryFree => simulationStatsSnapshot.ObjectsAllocMemoryFreeInBytes, 
			RenderSimStats.GeneralAllocatedMemoryFree => simulationStatsSnapshot.GeneralAllocMemoryFreeInBytes, 
			RenderSimStats.WordsWrittenCount => simulationStatsSnapshot.WordsWrittenCount, 
			RenderSimStats.WordsWrittenSize => simulationStatsSnapshot.WordsWrittenSize, 
			RenderSimStats.WordsReadCount => simulationStatsSnapshot.WordsReadCount, 
			RenderSimStats.WordsReadSize => simulationStatsSnapshot.WordsReadSize, 
			RenderSimStats.InputReceiveDelta => simulationStatsSnapshot.InputReceiveDelta, 
			RenderSimStats.TimeResets => simulationStatsSnapshot.TimeResets, 
			RenderSimStats.StateReceiveDelta => simulationStatsSnapshot.StateReceiveDelta, 
			RenderSimStats.SimulationTimeOffset => simulationStatsSnapshot.SimulationTimeOffset, 
			RenderSimStats.SimulationSpeed => simulationStatsSnapshot.SimulationSpeed, 
			RenderSimStats.InterpolationOffset => simulationStatsSnapshot.InterpolationOffset, 
			RenderSimStats.InterpolationSpeed => simulationStatsSnapshot.InterpolationSpeed, 
			_ => 0f, 
		};
	}
}
