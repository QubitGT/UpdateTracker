using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Fusion.Statistics;

public class FusionStatsCanvas : MonoBehaviour, IDragHandler, IEventSystemHandler, IEndDragHandler, IBeginDragHandler
{
	private enum DragMode
	{
		None,
		DragCanvas,
		ResizeContent
	}

	[Header("General References")]
	[SerializeField]
	private Canvas _canvas;

	[SerializeField]
	private CanvasScaler _canvasScaler;

	[SerializeField]
	private RectTransform _canvasPanel;

	[Space]
	[Header("Panel References")]
	[SerializeField]
	private RectTransform _contentPanel;

	[SerializeField]
	private RectTransform _contentContainer;

	[SerializeField]
	private RectTransform _bottomPanel;

	[SerializeField]
	private FusionStatsPanelHeader _header;

	[Space]
	[Header("Misc")]
	[SerializeField]
	private Button _hideButton;

	[SerializeField]
	private Button _closeButton;

	[Space]
	[Header("World Anchor Panel Settings")]
	[SerializeField]
	private FusionStatsConfig _config;

	private CanvasAnchor _anchor;

	private DragMode _dragMode;

	private static int _statsCanvasActiveCount;

	private bool _isColapsed => !_contentPanel.gameObject.activeSelf;

	internal void SetupStatsCanvas(FusionStatistics fusionStatistics, CanvasAnchor canvasAnchor, UnityAction closeButtonAction)
	{
		_anchor = canvasAnchor;
		_canvasPanel.anchoredPosition = GetDefinedAnchorPosition();
		int num = Mathf.Min(_statsCanvasActiveCount, 3);
		_canvasPanel.anchoredPosition += Vector2.down * (50f * (float)num);
		_closeButton.onClick.RemoveAllListeners();
		_closeButton.onClick.AddListener(closeButtonAction);
		_hideButton.onClick.RemoveAllListeners();
		_hideButton.onClick.AddListener(ToggleHide);
		_config.SetupStatisticReference(fusionStatistics);
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (!_config.IsWorldAnchored && _dragMode == DragMode.None)
		{
			Vector2 pressPosition = eventData.pressPosition;
			RectTransform bottomPanel = _bottomPanel;
			pressPosition = bottomPanel.InverseTransformPoint(pressPosition);
			bool flag = bottomPanel.rect.Contains(pressPosition) && eventData.button == PointerEventData.InputButton.Right;
			_dragMode = ((!flag) ? DragMode.DragCanvas : DragMode.ResizeContent);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (!_config.IsWorldAnchored)
		{
			switch (_dragMode)
			{
			case DragMode.DragCanvas:
				_canvasPanel.anchoredPosition += eventData.delta / _canvas.scaleFactor;
				break;
			case DragMode.ResizeContent:
				UpdateContentContainerHeight(eventData.delta.y / _canvas.scaleFactor);
				break;
			}
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (_config.IsWorldAnchored)
		{
			return;
		}
		if (!CheckDraggableRectVisibility(_canvasPanel))
		{
			SnapPanelBackToOriginPos();
		}
		if (_dragMode == DragMode.ResizeContent)
		{
			float y = _contentPanel.sizeDelta.y;
			float num = 0f;
			float yDelta = 0f;
			for (int i = 0; i < _contentContainer.childCount; i++)
			{
				float num2 = num;
				num += ((RectTransform)_contentContainer.GetChild(i)).sizeDelta.y + 10f;
				if (num >= y)
				{
					yDelta = ((!(y - num2 < num - y)) ? (0f - (num - y)) : (y - num2));
					break;
				}
			}
			UpdateContentContainerHeight(yDelta);
		}
		_dragMode = DragMode.None;
	}

	public void SnapPanelBackToOriginPos()
	{
		_canvasPanel.anchoredPosition = GetDefinedAnchorPosition();
	}

	private void UpdateContentContainerHeight(float yDelta)
	{
		float contentPanelHeight = _contentPanel.sizeDelta.y - yDelta;
		SetContentPanelHeight(contentPanelHeight);
	}

	internal void ToggleHide()
	{
		bool activeSelf = _contentPanel.gameObject.activeSelf;
		_hideButton.transform.rotation = (activeSelf ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity);
		_contentPanel.gameObject.SetActive(!activeSelf);
		_bottomPanel.gameObject.SetActive(!activeSelf);
	}

	private bool CheckDraggableRectVisibility(RectTransform rectTransform)
	{
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		Vector2 size = rectTransform.rect.size;
		if (Mathf.Abs(anchoredPosition.x) >= _canvasScaler.referenceResolution.x * 0.5f + size.x * 0.5f)
		{
			return false;
		}
		if (anchoredPosition.y >= _canvasScaler.referenceResolution.y * 0.5f + size.y || anchoredPosition.y <= (0f - _canvasScaler.referenceResolution.y) * 0.5f)
		{
			return false;
		}
		return true;
	}

	private void SetContentPanelHeight(float value)
	{
		if (value < 150f)
		{
			value = 150f;
		}
		else
		{
			float num = (float)Screen.height / _canvas.scaleFactor - 100f;
			if (value > num)
			{
				value = num;
			}
		}
		_contentPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
		_contentPanel.gameObject.SetActive(value: false);
		_contentPanel.gameObject.SetActive(value: true);
	}

	private void AdaptContentHeightToGraphs()
	{
		float num = 0f;
		for (int i = 0; i < _contentContainer.childCount; i++)
		{
			num += ((RectTransform)_contentContainer.GetChild(i)).sizeDelta.y + 10f;
		}
		float num2 = (float)Screen.height / _canvas.scaleFactor - 100f;
		if (num > num2)
		{
			num = num2;
		}
		if (num < 150f)
		{
			num = 150f;
		}
		SetContentPanelHeight(num);
	}

	private void OnEnable()
	{
		_statsCanvasActiveCount++;
		_header.OnRenderStatsUpdate += AdaptContentHeightToGraphs;
	}

	private void OnDisable()
	{
		_statsCanvasActiveCount--;
		_header.OnRenderStatsUpdate -= AdaptContentHeightToGraphs;
	}

	public void SetCanvasAnchor(CanvasAnchor anchor)
	{
		_anchor = anchor;
		SnapPanelBackToOriginPos();
	}

	private Vector2 GetDefinedAnchorPosition()
	{
		Vector2 referenceResolution = _canvasScaler.referenceResolution;
		switch (_anchor)
		{
		case CanvasAnchor.TopRight:
			return referenceResolution * 0.5f - Vector2.right * (_canvasPanel.sizeDelta.x * 0.5f);
		case CanvasAnchor.TopLeft:
			referenceResolution.x *= -1f;
			return referenceResolution * 0.5f + Vector2.right * (_canvasPanel.sizeDelta.x * 0.5f);
		default:
			return Vector2.zero;
		}
	}
}
