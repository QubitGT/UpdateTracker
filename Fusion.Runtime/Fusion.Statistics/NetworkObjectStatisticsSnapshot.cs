using System.Diagnostics;

namespace Fusion.Statistics;

public class NetworkObjectStatisticsSnapshot
{
	public int InPackets { get; private set; }

	public int OutPackets { get; private set; }

	public float InBandwidth { get; private set; }

	public float OutBandwidth { get; private set; }

	internal void Reset()
	{
		InPackets = 0;
		OutPackets = 0;
		InBandwidth = 0f;
		OutBandwidth = 0f;
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
}
