using System.Diagnostics;

namespace Fusion.Statistics;

public class FusionStatisticsSnapshot
{
	public int Resimulations { get; private set; }

	public int ForwardTicks { get; private set; }

	public int InPackets { get; private set; }

	public int OutPackets { get; private set; }

	public float InBandwidth { get; private set; }

	public float OutBandwidth { get; private set; }

	public float RoundTripTime { get; private set; }

	public float InputInBandwidth { get; private set; }

	public float InputOutBandwidth { get; private set; }

	public int InObjectUpdates { get; private set; }

	public int OutObjectUpdates { get; private set; }

	public int ObjectsAllocMemoryUsedInBytes { get; private set; }

	public int GeneralAllocMemoryUsedInBytes { get; private set; }

	public int ObjectsAllocMemoryFreeInBytes { get; private set; }

	public int GeneralAllocMemoryFreeInBytes { get; private set; }

	public int WordsWrittenCount { get; private set; }

	public int WordsReadCount { get; private set; }

	public int WordsWrittenSize => WordsWrittenCount * 4;

	public int WordsReadSize => WordsReadCount * 4;

	public float InputReceiveDelta { get; private set; }

	public int TimeResets { get; private set; }

	public float StateReceiveDelta { get; private set; }

	public float SimulationTimeOffset { get; private set; }

	public float SimulationSpeed { get; private set; }

	public float InterpolationOffset { get; private set; }

	public float InterpolationSpeed { get; private set; }

	[Conditional("DEBUG")]
	internal void ClearSnapshot()
	{
		Resimulations = 0;
		ForwardTicks = 0;
		InPackets = 0;
		OutPackets = 0;
		InBandwidth = 0f;
		OutBandwidth = 0f;
		RoundTripTime = 0f;
		InputInBandwidth = 0f;
		InputOutBandwidth = 0f;
		InObjectUpdates = 0;
		OutObjectUpdates = 0;
		ObjectsAllocMemoryUsedInBytes = 0;
		GeneralAllocMemoryUsedInBytes = 0;
		GeneralAllocMemoryFreeInBytes = 0;
		ObjectsAllocMemoryFreeInBytes = 0;
		WordsWrittenCount = 0;
		WordsReadCount = 0;
		InputReceiveDelta = 0f;
		TimeResets = 0;
		StateReceiveDelta = 0f;
		SimulationTimeOffset = 0f;
		SimulationSpeed = 0f;
		InterpolationOffset = 0f;
		InterpolationSpeed = 0f;
	}

	[Conditional("DEBUG")]
	internal void CopyFrom(FusionStatisticsSnapshot snapshot)
	{
		Resimulations = snapshot.Resimulations;
		ForwardTicks = snapshot.ForwardTicks;
		InPackets = snapshot.InPackets;
		OutPackets = snapshot.OutPackets;
		InBandwidth = snapshot.InBandwidth;
		OutBandwidth = snapshot.OutBandwidth;
		RoundTripTime = snapshot.RoundTripTime;
		InputInBandwidth = snapshot.InputInBandwidth;
		InputOutBandwidth = snapshot.InputOutBandwidth;
		InObjectUpdates = snapshot.InObjectUpdates;
		OutObjectUpdates = snapshot.OutObjectUpdates;
		ObjectsAllocMemoryUsedInBytes = snapshot.ObjectsAllocMemoryUsedInBytes;
		GeneralAllocMemoryUsedInBytes = snapshot.GeneralAllocMemoryUsedInBytes;
		ObjectsAllocMemoryFreeInBytes = snapshot.ObjectsAllocMemoryFreeInBytes;
		GeneralAllocMemoryFreeInBytes = snapshot.GeneralAllocMemoryFreeInBytes;
		WordsWrittenCount = snapshot.WordsWrittenCount;
		WordsReadCount = snapshot.WordsReadCount;
		InputReceiveDelta = snapshot.InputReceiveDelta;
		TimeResets = snapshot.TimeResets;
		StateReceiveDelta = snapshot.StateReceiveDelta;
		SimulationTimeOffset = snapshot.SimulationTimeOffset;
		SimulationSpeed = snapshot.SimulationSpeed;
		InterpolationOffset = snapshot.InterpolationOffset;
		InterpolationSpeed = snapshot.InterpolationSpeed;
	}

	[Conditional("DEBUG")]
	internal void AddToResimulationStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			Resimulations = value;
		}
		else
		{
			Resimulations += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToForwardTicksStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			ForwardTicks = value;
		}
		else
		{
			ForwardTicks += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToInPacketsStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			InPackets = value;
		}
		else
		{
			InPackets += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToOutPacketsStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			OutPackets = value;
		}
		else
		{
			OutPackets += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToInBandwidthStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			InBandwidth = value;
		}
		else
		{
			InBandwidth += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToOutBandwidthStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			OutBandwidth = value;
		}
		else
		{
			OutBandwidth += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToRoundTripTimeStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			RoundTripTime = value;
		}
		else
		{
			RoundTripTime += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToInputInBandwidthStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			InputInBandwidth = value;
		}
		else
		{
			InputInBandwidth += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToInputOutBandwidthStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			InputOutBandwidth = value;
		}
		else
		{
			InputOutBandwidth += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToInObjectUpdatesStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			InObjectUpdates = value;
		}
		else
		{
			InObjectUpdates += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToOutObjectUpdatesStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			OutObjectUpdates = value;
		}
		else
		{
			OutObjectUpdates += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToObjectsAllocMemoryUsedInBytesStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			ObjectsAllocMemoryUsedInBytes = value;
		}
		else
		{
			ObjectsAllocMemoryUsedInBytes += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToGeneralAllocMemoryUsedInBytesStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			GeneralAllocMemoryUsedInBytes = value;
		}
		else
		{
			GeneralAllocMemoryUsedInBytes += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToObjectsAllocMemoryFreeInBytesStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			ObjectsAllocMemoryFreeInBytes = value;
		}
		else
		{
			ObjectsAllocMemoryFreeInBytes += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToGeneralAllocMemoryFreeInBytesStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			GeneralAllocMemoryFreeInBytes = value;
		}
		else
		{
			GeneralAllocMemoryFreeInBytes += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToWordsWrittenCountStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			WordsWrittenCount = value;
		}
		else
		{
			WordsWrittenCount += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToWordsReadCountStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			WordsReadCount = value;
		}
		else
		{
			WordsReadCount += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToInputReceiveDeltaStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			InputReceiveDelta = value;
		}
		else
		{
			InputReceiveDelta += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToTimeResetsStat(int value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			TimeResets = value;
		}
		else
		{
			TimeResets += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToStateReceiveDeltaStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			StateReceiveDelta = value;
		}
		else
		{
			StateReceiveDelta += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToSimulationTimeOffsetStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			SimulationTimeOffset = value;
		}
		else
		{
			SimulationTimeOffset += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToSimulationSpeedStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			SimulationSpeed = value;
		}
		else
		{
			SimulationSpeed += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToInterpolationOffsetStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			InterpolationOffset = value;
		}
		else
		{
			InterpolationOffset += value;
		}
	}

	[Conditional("DEBUG")]
	internal void AddToInterpolationSpeedStat(float value, bool overrideValue = false)
	{
		if (overrideValue)
		{
			InterpolationSpeed = value;
		}
		else
		{
			InterpolationSpeed += value;
		}
	}
}
