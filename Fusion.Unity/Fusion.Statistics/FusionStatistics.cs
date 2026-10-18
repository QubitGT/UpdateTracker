#define FUSION_LOGLEVEL_TRACE
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace Fusion.Statistics;

[RequireComponent(typeof(NetworkRunner))]
[AddComponentMenu("Fusion/Statistics/Fusion Statistics")]
public class FusionStatistics : SimulationBehaviour, ISpawned, IPublicFacingInterface
{
	[Serializable]
	public struct FusionStatisticsStatCustomConfig
	{
		public RenderSimStats Stat;

		public float Threshold1;

		public float Threshold2;

		public float Threshold3;

		public bool IgnoreZeroOnBuffer;

		public bool IgnoreZeroOnAverageCalculation;

		public int AccumulateTimeMs;
	}

	private GameObject _statsCanvasPrefab;

	private FusionNetworkObjectStatsGraphCombine _objectGraphCombinePrefab;

	private const string STATS_CANVAS_PREFAB_PATH = "FusionStatsResources/FusionStatsRenderPanel";

	private const string STATS_OBJECT_COMBINE_PREFAB_PATH = "FusionStatsResources/NetworkObjectStatistics";

	private List<FusionStatsGraphBase> _statsGraph;

	private FusionStatsPanelHeader _header;

	private FusionStatsConfig _config;

	private FusionStatsCanvas _statsCanvas;

	private GameObject _statsPanelObject;

	private Dictionary<FusionNetworkObjectStatistics, FusionNetworkObjectStatsGraphCombine> _objectStatsGraphCombines;

	[InlineHelp]
	[ExpandableEnum]
	[SerializeField]
	private RenderSimStats _statsEnabled;

	[InlineHelp]
	[SerializeField]
	private CanvasAnchor _canvasAnchor = CanvasAnchor.TopRight;

	[FormerlySerializedAs("_statsConfig")]
	[SerializeField]
	[Header("Custom configuration to override default values.\nSelect only one stat flag per configuration.")]
	private List<FusionStatisticsStatCustomConfig> _statsCustomConfig = new List<FusionStatisticsStatCustomConfig>();

	internal List<FusionStatsGraphBase> ActiveGraphs => _statsGraph;

	internal List<FusionStatisticsStatCustomConfig> StatsCustomConfig => _statsCustomConfig;

	public bool IsPanelActive => _statsPanelObject;

	private void Awake()
	{
		_statsGraph = new List<FusionStatsGraphBase>();
		_statsCanvasPrefab = Resources.Load<GameObject>("FusionStatsResources/FusionStatsRenderPanel");
		_objectGraphCombinePrefab = Resources.Load<FusionNetworkObjectStatsGraphCombine>("FusionStatsResources/NetworkObjectStatistics");
		if (_statsCanvasPrefab == null || _objectGraphCombinePrefab == null)
		{
			Log.Error("Error loading the required assets for Fusion Statistics, destroying stats instance. Make sure that the following paths are valid for the Fusion Statistics resource assets: \n 1. FusionStatsResources/FusionStatsRenderPanel \n 2. FusionStatsResources/NetworkObjectStatistics");
			UnityEngine.Object.Destroy(this);
		}
	}

	void ISpawned.Spawned()
	{
		SetupStatisticsPanel();
	}

	public void SetStatsCustomConfig(List<FusionStatisticsStatCustomConfig> customConfig)
	{
		if (customConfig == null)
		{
			Log.Warn("Trying to set a null Fusion Statistics custom stats config");
			return;
		}
		_statsCustomConfig = customConfig;
		ApplyCustomConfig();
	}

	public void SetCanvasAnchor(CanvasAnchor anchor)
	{
		_canvasAnchor = anchor;
		if ((bool)_statsCanvas)
		{
			_statsCanvas.SetCanvasAnchor(anchor);
		}
	}

	private void ApplyCustomConfig()
	{
		if ((bool)_header)
		{
			_header.ApplyStatsConfig(_statsCustomConfig);
		}
	}

	public void OnEditorChange()
	{
		RenderEnabledStats();
		ApplyCustomConfig();
		SetCanvasAnchor(_canvasAnchor);
	}

	private void RenderEnabledStats()
	{
		if (IsPanelActive)
		{
			_header.SetStatsToRender(_statsEnabled);
		}
	}

	internal void UpdateStatsEnabled(RenderSimStats stats)
	{
		_statsEnabled = stats;
	}

	public void SetupStatisticsPanel()
	{
		if (IsPanelActive)
		{
			return;
		}
		if (base.Runner == null)
		{
			NetworkRunner component = GetComponent<NetworkRunner>();
			if (!component.IsRunning)
			{
				Log.Warn($"Network Runner on ({component.gameObject}) is not yet running.");
			}
			else
			{
				component.AddGlobal(this);
			}
			return;
		}
		_objectStatsGraphCombines = new Dictionary<FusionNetworkObjectStatistics, FusionNetworkObjectStatsGraphCombine>();
		_statsPanelObject = UnityEngine.Object.Instantiate(_statsCanvasPrefab, base.transform);
		_statsCanvas = _statsPanelObject.GetComponentInChildren<FusionStatsCanvas>();
		_statsCanvas.SetupStatsCanvas(this, _canvasAnchor, DestroyStatisticsPanel);
		_header = _statsPanelObject.GetComponentInChildren<FusionStatsPanelHeader>();
		_header.SetupHeader(base.Runner.LocalPlayer.ToString(), this);
		_config = _statsPanelObject.GetComponentInChildren<FusionStatsConfig>(includeInactive: true);
		_statsPanelObject.AddComponent<FusionBasicBillboard>();
		ApplyCustomConfig();
		base.Runner.AddVisibilityNodes(_statsPanelObject);
		if (_statsEnabled != 0)
		{
			RenderEnabledStats();
		}
		if (!EventSystem.current)
		{
			new GameObject("EventSystem-FusionStatistics", typeof(EventSystem), typeof(StandaloneInputModule));
		}
	}

	public void SetWorldAnchor(FusionStatsWorldAnchor anchor, float scale)
	{
		_config.SetWorldCanvasScale(scale);
		if (anchor == null)
		{
			_config.ResetToCanvasAnchor();
		}
		else
		{
			_config.SetWorldAnchor(anchor.transform);
		}
	}

	public void DestroyStatisticsPanel()
	{
		FusionNetworkObjectStatistics[] array = _objectStatsGraphCombines?.Keys.ToArray();
		if (array != null)
		{
			FusionNetworkObjectStatistics[] array2 = array;
			foreach (FusionNetworkObjectStatistics fusionNetworkObjectStatistics in array2)
			{
				MonitorNetworkObject(fusionNetworkObjectStatistics.NetworkObject, fusionNetworkObjectStatistics, monitor: false);
			}
		}
		_objectStatsGraphCombines?.Clear();
		_statsGraph.Clear();
		UnityEngine.Object.Destroy(_statsPanelObject);
		_statsPanelObject = null;
		if ((bool)base.Runner && base.Runner.TryGetFusionStatistics(out var statisticsManager))
		{
			statisticsManager.ObjectStatisticsManager.ClearMonitoredNetworkObjects();
		}
	}

	public bool MonitorNetworkObject(NetworkObject networkObject, FusionNetworkObjectStatistics objectStatisticsInstance, bool monitor)
	{
		if (base.Runner.TryGetFusionStatistics(out var statisticsManager))
		{
			statisticsManager.ObjectStatisticsManager.MonitorNetworkObjectStatistics(networkObject.Id, monitor);
		}
		FusionNetworkObjectStatsGraphCombine value;
		if (monitor)
		{
			if (_objectStatsGraphCombines.ContainsKey(objectStatisticsInstance))
			{
				return false;
			}
			FusionNetworkObjectStatsGraphCombine fusionNetworkObjectStatsGraphCombine = UnityEngine.Object.Instantiate(_objectGraphCombinePrefab, _header.ContentRect);
			fusionNetworkObjectStatsGraphCombine.SetupNetworkObject(networkObject, this, objectStatisticsInstance);
			_objectStatsGraphCombines.Add(objectStatisticsInstance, fusionNetworkObjectStatsGraphCombine);
		}
		else if (_objectStatsGraphCombines.Remove(objectStatisticsInstance, out value))
		{
			UnityEngine.Object.Destroy(value.gameObject);
			UnityEngine.Object.Destroy(objectStatisticsInstance);
		}
		return true;
	}

	private void UpdateAllGraphs(FusionStatisticsManager statisticsManager)
	{
		DateTime now = DateTime.Now;
		foreach (FusionStatsGraphBase item in _statsGraph)
		{
			item.UpdateGraph(base.Runner, statisticsManager, ref now);
		}
	}

	public void RegisterGraph(FusionStatsGraphBase graph)
	{
		_statsGraph.Add(graph);
	}

	public void UnregisterGraph(FusionStatsGraphBase graph)
	{
		_statsGraph.Remove(graph);
	}

	private void Update()
	{
		if ((bool)base.Runner && base.Runner.TryGetFusionStatistics(out var statisticsManager))
		{
			UpdateAllGraphs(statisticsManager);
		}
	}
}
