using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Statistics;

public class FusionStatsPanelHeader : MonoBehaviour
{
	[SerializeField]
	private Text _statsHeaderTitle;

	[SerializeField]
	private Dropdown _statsDropdown;

	[SerializeField]
	private FusionStatsGraphDefault _defaultGraphPrefab;

	public RectTransform ContentRect;

	private Dictionary<RenderSimStats, FusionStatsGraphDefault> _defaultStatsGraph;

	private FusionStatistics _fusionStatistics;

	private RenderSimStats _statsToRender;

	public event Action OnRenderStatsUpdate;

	public void SetupHeader(string title, FusionStatistics fusionStatistics)
	{
		_statsHeaderTitle.text = title;
		_fusionStatistics = fusionStatistics;
		SetupDropdown();
	}

	private void SetupDropdown()
	{
		_defaultStatsGraph = new Dictionary<RenderSimStats, FusionStatsGraphDefault>();
		List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
		list.Add(new Dropdown.OptionData("Toggle Stats"));
		string[] names = Enum.GetNames(typeof(RenderSimStats));
		foreach (string text in names)
		{
			list.Add(new Dropdown.OptionData(text));
		}
		_statsDropdown.options = list;
		_statsDropdown.onValueChanged.AddListener(OnDropDownChanged);
	}

	internal void SetStatsToRender(RenderSimStats stats)
	{
		if (stats == _statsToRender)
		{
			return;
		}
		foreach (RenderSimStats value in Enum.GetValues(typeof(RenderSimStats)))
		{
			if ((stats & value) == value)
			{
				if ((_statsToRender & value) != value)
				{
					AddStat(value);
				}
			}
			else if ((_statsToRender & value) == value)
			{
				RemoveStat(value);
			}
		}
		_statsToRender = stats;
	}

	private void AddStat(RenderSimStats stat)
	{
		_statsToRender |= stat;
		InstantiateStatGraph(stat);
		InvokeRenderStatsUpdate();
	}

	private void RemoveStat(RenderSimStats stat)
	{
		_statsToRender &= ~stat;
		DestroyStatGraph(stat);
		InvokeRenderStatsUpdate();
	}

	private void InvokeRenderStatsUpdate()
	{
		this.OnRenderStatsUpdate?.Invoke();
	}

	private void OnDropDownChanged(int arg0)
	{
		if (arg0 > 0)
		{
			arg0--;
			RenderSimStats renderSimStats = (RenderSimStats)(1 << arg0);
			if ((_statsToRender & renderSimStats) == renderSimStats)
			{
				RemoveStat(renderSimStats);
			}
			else
			{
				AddStat(renderSimStats);
			}
			_statsDropdown.SetValueWithoutNotify(0);
			_fusionStatistics.UpdateStatsEnabled(_statsToRender);
		}
	}

	private void InstantiateStatGraph(RenderSimStats stat)
	{
		FusionStatsGraphDefault fusionStatsGraphDefault = UnityEngine.Object.Instantiate(_defaultGraphPrefab, ContentRect);
		fusionStatsGraphDefault.SetupDefaultGraph(stat);
		TryApplyCustomStatConfig(fusionStatsGraphDefault);
		_defaultStatsGraph.Add(stat, fusionStatsGraphDefault);
	}

	private void DestroyStatGraph(RenderSimStats stat)
	{
		if (_defaultStatsGraph.Remove(stat, out var value))
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
	}

	private void TryApplyCustomStatConfig(FusionStatsGraphDefault graph)
	{
		foreach (FusionStatistics.FusionStatisticsStatCustomConfig item in _fusionStatistics.StatsCustomConfig)
		{
			if (item.Stat == graph.Stat)
			{
				ApplyCustomStatsConfig(graph, item);
			}
		}
	}

	private void ApplyCustomStatsConfig(FusionStatsGraphDefault graph, FusionStatistics.FusionStatisticsStatCustomConfig config)
	{
		graph.ApplyCustomStatsConfig(config);
	}

	internal void ApplyStatsConfig(List<FusionStatistics.FusionStatisticsStatCustomConfig> statsConfig)
	{
		foreach (FusionStatistics.FusionStatisticsStatCustomConfig item in statsConfig)
		{
			if (_defaultStatsGraph.TryGetValue(item.Stat, out var value))
			{
				ApplyCustomStatsConfig(value, item);
			}
		}
	}
}
