using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Statistics;

public class FusionNetworkObjectStatsGraphCombine : MonoBehaviour
{
	[SerializeField]
	private Text _titleText;

	[SerializeField]
	private Dropdown _statDropdown;

	[SerializeField]
	private NetworkObjectStat _statsToRender;

	[SerializeField]
	private RectTransform _rect;

	[SerializeField]
	private RectTransform _combinedGraphRender;

	[SerializeField]
	private Button _toggleButton;

	private float _headerHeight = 50f;

	private float _graphHeight = 150f;

	private Dictionary<NetworkObjectStat, FusionNetworkObjectStatsGraph> _statsGraphs;

	[SerializeField]
	private FusionNetworkObjectStatsGraph _statsGraphPrefab;

	private ContentSizeFitter _parentContentSizeFitter;

	private NetworkObject _networkObject;

	private FusionStatistics _fusionStatistics;

	private FusionNetworkObjectStatistics _objectStatisticsInstance;

	public NetworkId NetworkObjectID => _networkObject.Id;

	public void SetupNetworkObject(NetworkObject networkObject, FusionStatistics fusionStatistics, FusionNetworkObjectStatistics objectStatisticsInstance)
	{
		_networkObject = networkObject;
		_fusionStatistics = fusionStatistics;
		_objectStatisticsInstance = objectStatisticsInstance;
	}

	private void Start()
	{
		_statsGraphs = new Dictionary<NetworkObjectStat, FusionNetworkObjectStatsGraph>();
		_parentContentSizeFitter = GetComponentInParent<ContentSizeFitter>();
		List<Dropdown.OptionData> list = new List<Dropdown.OptionData>();
		list.Add(new Dropdown.OptionData("Toggle Stats"));
		string[] names = Enum.GetNames(typeof(NetworkObjectStat));
		foreach (string text in names)
		{
			list.Add(new Dropdown.OptionData(text));
		}
		_statDropdown.options = list;
		_statDropdown.onValueChanged.AddListener(OnDropDownChanged);
		UpdateHeight();
		_titleText.text = _networkObject.Name;
	}

	private void OnDropDownChanged(int arg0)
	{
		if (arg0 > 0)
		{
			arg0--;
			NetworkObjectStat networkObjectStat = (NetworkObjectStat)(1 << arg0);
			if ((_statsToRender & networkObjectStat) == networkObjectStat)
			{
				_statsToRender &= ~networkObjectStat;
				DestroyStatGraph(networkObjectStat);
			}
			else
			{
				_statsToRender |= networkObjectStat;
				InstantiateStatGraph(networkObjectStat);
			}
			UpdateHeight();
			_statDropdown.SetValueWithoutNotify(0);
		}
	}

	private void InstantiateStatGraph(NetworkObjectStat stat)
	{
		FusionNetworkObjectStatsGraph fusionNetworkObjectStatsGraph = UnityEngine.Object.Instantiate(_statsGraphPrefab, _combinedGraphRender);
		fusionNetworkObjectStatsGraph.SetupNetworkObjectStat(NetworkObjectID, stat);
		_statsGraphs.Add(stat, fusionNetworkObjectStatsGraph);
	}

	private void DestroyStatGraph(NetworkObjectStat stat)
	{
		_statsGraphs[stat].gameObject.SetActive(value: false);
		UnityEngine.Object.Destroy(_statsGraphs[stat].gameObject);
		_statsGraphs.Remove(stat);
	}

	private void UpdateHeight(float overrideValue = -1f)
	{
		Vector2 sizeDelta = _rect.sizeDelta;
		float y = ((overrideValue >= 0f) ? overrideValue : (_headerHeight + (float)_statsGraphs.Count * _graphHeight));
		_rect.sizeDelta = new Vector2(sizeDelta.x, y);
		_parentContentSizeFitter.enabled = false;
		_parentContentSizeFitter.enabled = true;
	}

	private void OnDisable()
	{
		if (_statsGraphs == null)
		{
			return;
		}
		foreach (FusionNetworkObjectStatsGraph value in _statsGraphs.Values)
		{
			value.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (_statsGraphs == null)
		{
			return;
		}
		foreach (FusionNetworkObjectStatsGraph value in _statsGraphs.Values)
		{
			value.gameObject.SetActive(value: true);
		}
	}

	public void ToggleRenderDisplay()
	{
		bool activeSelf = _combinedGraphRender.gameObject.activeSelf;
		_combinedGraphRender.gameObject.SetActive(!activeSelf);
		if (activeSelf)
		{
			OnDisable();
			UpdateHeight(_headerHeight);
			_toggleButton.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
		}
		else
		{
			OnEnable();
			UpdateHeight();
			_toggleButton.transform.rotation = Quaternion.identity;
		}
	}

	public void DestroyCombinedGraph()
	{
		_fusionStatistics.MonitorNetworkObject(_networkObject, _objectStatisticsInstance, monitor: false);
	}
}
