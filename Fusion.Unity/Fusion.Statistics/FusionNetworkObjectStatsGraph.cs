using System;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Statistics;

public class FusionNetworkObjectStatsGraph : FusionStatsGraphBase
{
	[SerializeField]
	private Text _description;

	private NetworkId _id;

	private NetworkObjectStat _stat;

	private FusionNetworkObjectStatsGraphCombine _combineParentGraph;

	public override void UpdateGraph(NetworkRunner runner, FusionStatisticsManager statisticsManager, ref DateTime now)
	{
		AddValueToBuffer(GetNetworkObjectStatValue(statisticsManager), ref now);
	}

	private float GetNetworkObjectStatValue(FusionStatisticsManager statisticsManager)
	{
		if (statisticsManager.ObjectStatisticsManager.GetNetworkObjectStatistics(_id, out var objectStatisticsSnapshot))
		{
			switch (_stat)
			{
			case NetworkObjectStat.InBandwidth:
				return objectStatisticsSnapshot.InBandwidth;
			case NetworkObjectStat.OutBandwidth:
				return objectStatisticsSnapshot.OutBandwidth;
			case NetworkObjectStat.InPackets:
				return objectStatisticsSnapshot.InPackets;
			case NetworkObjectStat.OutPackets:
				return objectStatisticsSnapshot.OutPackets;
			case NetworkObjectStat.AverageInPacketSize:
				return objectStatisticsSnapshot.InBandwidth / (float)Mathf.Max(1, objectStatisticsSnapshot.InPackets);
			case NetworkObjectStat.AverageOutPacketSize:
				return objectStatisticsSnapshot.OutBandwidth / (float)Mathf.Max(1, objectStatisticsSnapshot.OutPackets);
			}
		}
		return -1f;
	}

	internal void SetupNetworkObjectStat(NetworkId id, NetworkObjectStat stat)
	{
		_id = id;
		_stat = stat;
		_description.text = _stat.ToString();
		float threshold = 0f;
		float threshold2 = 0f;
		float threshold3 = 0f;
		float valueTextMultiplier = 1f;
		bool ignoreZeroOnAverage = false;
		bool ignoreZeroOnBuffer = false;
		int accumulateTimeMs = 0;
		string valueTextFormat;
		switch (stat)
		{
		case NetworkObjectStat.InBandwidth:
		case NetworkObjectStat.OutBandwidth:
			valueTextFormat = "{0:0} B";
			accumulateTimeMs = 1000;
			_description.text += " (Per second)";
			break;
		case NetworkObjectStat.AverageInPacketSize:
		case NetworkObjectStat.AverageOutPacketSize:
			valueTextFormat = "{0:0} B";
			ignoreZeroOnAverage = true;
			ignoreZeroOnBuffer = true;
			break;
		case NetworkObjectStat.InPackets:
		case NetworkObjectStat.OutPackets:
			valueTextFormat = "{0:0}";
			accumulateTimeMs = 1000;
			_description.text += " (Per second)";
			break;
		default:
			valueTextFormat = "{0:0}";
			break;
		}
		SetValueTextFormat(valueTextFormat);
		SetValueTextMultiplier(valueTextMultiplier);
		SetThresholds(threshold, threshold2, threshold3);
		SetIgnoreZeroValues(ignoreZeroOnAverage, ignoreZeroOnBuffer);
		Initialize(accumulateTimeMs);
	}
}
