using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Statistics;

public class FusionStatsGraphDefault : FusionStatsGraphBase
{
	private RenderSimStats _selectedStats;

	[SerializeField]
	private Text _descriptionText;

	private Dictionary<RenderSimStats, string> _statsAdditionalInfo = new Dictionary<RenderSimStats, string>
	{
		{
			RenderSimStats.InPackets,
			"(Per second)"
		},
		{
			RenderSimStats.OutPackets,
			"(Per second)"
		},
		{
			RenderSimStats.InObjectUpdates,
			"(Per second)"
		},
		{
			RenderSimStats.OutObjectUpdates,
			"(Per second)"
		},
		{
			RenderSimStats.InBandwidth,
			"(Per second)"
		},
		{
			RenderSimStats.OutBandwidth,
			"(Per second)"
		},
		{
			RenderSimStats.InputInBandwidth,
			"(Per second)"
		},
		{
			RenderSimStats.InputOutBandwidth,
			"(Per second)"
		},
		{
			RenderSimStats.StateReceiveDelta,
			"(Per second)"
		},
		{
			RenderSimStats.WordsWrittenSize,
			"(Per second)"
		},
		{
			RenderSimStats.WordsWrittenCount,
			"(Per second)"
		},
		{
			RenderSimStats.WordsReadCount,
			"(Per second)"
		},
		{
			RenderSimStats.WordsReadSize,
			"(Per second)"
		}
	};

	internal RenderSimStats Stat => _selectedStats;

	protected override void Initialize(int accumulateTimeMs)
	{
		base.Initialize(accumulateTimeMs);
		_descriptionText.text = _selectedStats.ToString();
		if (_statsAdditionalInfo.TryGetValue(Stat, out var value))
		{
			Text descriptionText = _descriptionText;
			descriptionText.text = descriptionText.text + " " + value;
		}
	}

	public override void UpdateGraph(NetworkRunner runner, FusionStatisticsManager statisticsManager, ref DateTime now)
	{
		float statDataFromSnapshot = FusionStatisticsHelper.GetStatDataFromSnapshot(_selectedStats, statisticsManager.CompleteSnapshot);
		AddValueToBuffer(statDataFromSnapshot, ref now);
	}

	public virtual void ApplyCustomStatsConfig(FusionStatistics.FusionStatisticsStatCustomConfig config)
	{
		SetThresholds(config.Threshold1, config.Threshold2, config.Threshold3);
		SetIgnoreZeroValues(config.IgnoreZeroOnAverageCalculation, config.IgnoreZeroOnBuffer);
		SetAccumulateTime(config.AccumulateTimeMs);
	}

	internal void SetupDefaultGraph(RenderSimStats stat)
	{
		_selectedStats = stat;
		FusionStatisticsHelper.GetStatGraphDefaultSettings(_selectedStats, out var valueTextFormat, out var valueTextMultiplier, out var ignoreZeroOnAverage, out var ignoreZeroOnBuffer, out var accumulateTimeMs);
		SetValueTextFormat(valueTextFormat);
		SetValueTextMultiplier(valueTextMultiplier);
		SetIgnoreZeroValues(ignoreZeroOnAverage, ignoreZeroOnBuffer);
		Initialize(accumulateTimeMs);
	}
}
