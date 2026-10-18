using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Fusion.Statistics;

public class FusionStatsConfig : MonoBehaviour
{
	[SerializeField]
	private Button _worldAnchorButtonPrefab;

	[SerializeField]
	private Transform _worldAnchorListContainer;

	[SerializeField]
	private GameObject _configPanel;

	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private RectTransform _renderPanelRectTransform;

	private Transform _worldTransformAnchor;

	private float _worldCanvasScale = 0.005f;

	private FusionStatistics _fusionStatistics;

	private static List<Transform> _worldAnchorCandidates = new List<Transform>();

	public bool IsWorldAnchored => _worldTransformAnchor != null;

	private static event Action _onWorldAnchorCandidatesUpdate;

	internal static void SetWorldAnchorCandidate(Transform candidate, bool register)
	{
		if (register)
		{
			if (!_worldAnchorCandidates.Contains(candidate))
			{
				_worldAnchorCandidates.Add(candidate);
			}
		}
		else
		{
			_worldAnchorCandidates.Remove(candidate);
		}
		FusionStatsConfig._onWorldAnchorCandidatesUpdate?.Invoke();
	}

	internal void SetupStatisticReference(FusionStatistics fusionStatistics)
	{
		_fusionStatistics = fusionStatistics;
	}

	public void ToggleConfigPanel()
	{
		_configPanel.SetActive(!_configPanel.activeSelf);
	}

	public void ToggleUseWorldAnchor(bool value)
	{
		if (!value)
		{
			ResetToCanvasAnchor();
		}
	}

	internal void SetWorldAnchor(Transform worldTransformAnchor)
	{
		_canvas.renderMode = RenderMode.WorldSpace;
		_renderPanelRectTransform.localScale = Vector3.one * _worldCanvasScale;
		_renderPanelRectTransform.localPosition = Vector3.zero;
		if (!(worldTransformAnchor == _worldTransformAnchor))
		{
			_renderPanelRectTransform.SetParent(worldTransformAnchor);
			_worldTransformAnchor = worldTransformAnchor;
			_renderPanelRectTransform.localPosition = Vector3.zero;
		}
	}

	public void SetWorldCanvasScale(float value)
	{
		_worldCanvasScale = value;
	}

	internal void ResetToCanvasAnchor()
	{
		if ((bool)_fusionStatistics)
		{
			RectTransform obj = (RectTransform)_renderPanelRectTransform.GetChild(0);
			_renderPanelRectTransform.SetParent(_fusionStatistics.transform);
			_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			_renderPanelRectTransform.localScale = Vector3.one;
			_renderPanelRectTransform.localPosition = Vector3.zero;
			obj.localPosition = Vector3.zero;
			obj.anchoredPosition = Vector3.zero;
			_worldTransformAnchor = null;
		}
	}

	private void UpdateWorldAnchorButtons()
	{
		for (int num = _worldAnchorListContainer.childCount - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(_worldAnchorListContainer.GetChild(num).gameObject);
		}
		foreach (Transform candidate in _worldAnchorCandidates)
		{
			Button button = UnityEngine.Object.Instantiate(_worldAnchorButtonPrefab, _worldAnchorListContainer);
			button.onClick.AddListener(delegate
			{
				SetWorldAnchor(candidate);
			});
			button.GetComponentInChildren<Text>().text = candidate.name;
		}
	}

	private void OnEnable()
	{
		_onWorldAnchorCandidatesUpdate -= UpdateWorldAnchorButtons;
		_onWorldAnchorCandidatesUpdate += UpdateWorldAnchorButtons;
		UpdateWorldAnchorButtons();
	}

	private void OnDestroy()
	{
		_onWorldAnchorCandidatesUpdate -= UpdateWorldAnchorButtons;
	}
}
