using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components;

public class ModioUIMaximumHeight : MonoBehaviour, ILayoutElement
{
	[SerializeField]
	private float _restrictHeightTo = 100f;

	[SerializeField]
	private Toggle _expandAnyway;

	[SerializeField]
	private GameObject _showWhenRestrictingHeight;

	private bool _isRestrictingHeight;

	private Graphic _graphic;

	public float minWidth => -1f;

	public float preferredWidth => -1f;

	public float flexibleWidth => -1f;

	public float minHeight => -1f;

	public float preferredHeight
	{
		get
		{
			if (!_isRestrictingHeight)
			{
				return -1f;
			}
			return _restrictHeightTo;
		}
	}

	public float flexibleHeight => -1f;

	public int layoutPriority => 10;

	private void Awake()
	{
		if (_expandAnyway != null)
		{
			_expandAnyway.onValueChanged.AddListener(OnExpandAnywayChanged);
		}
		_graphic = GetComponent<Graphic>();
		if (_graphic != null)
		{
			_graphic.RegisterDirtyLayoutCallback(GraphicLayoutDirty);
		}
	}

	private void OnDestroy()
	{
		if (_graphic != null)
		{
			_graphic.UnregisterDirtyLayoutCallback(GraphicLayoutDirty);
		}
	}

	private void OnEnable()
	{
		if (_expandAnyway != null)
		{
			_expandAnyway.isOn = false;
		}
	}

	private void OnExpandAnywayChanged(bool isExpanded)
	{
		SetDirty();
	}

	public void CalculateLayoutInputHorizontal()
	{
	}

	public void CalculateLayoutInputVertical()
	{
		RecalculateRestrictingHeight(delayButtonActivation: true);
	}

	private void GraphicLayoutDirty()
	{
		RecalculateRestrictingHeight(delayButtonActivation: false);
	}

	private void RecalculateRestrictingHeight(bool delayButtonActivation)
	{
		_isRestrictingHeight = false;
		_isRestrictingHeight = LayoutUtility.GetPreferredHeight((RectTransform)base.transform) > _restrictHeightTo;
		if (_expandAnyway != null || _showWhenRestrictingHeight != null)
		{
			if (delayButtonActivation)
			{
				StartCoroutine(SetButtonsActiveDelayed(_isRestrictingHeight));
			}
			else
			{
				SetButtonsActive(_isRestrictingHeight);
			}
			if (_expandAnyway != null && _expandAnyway.isOn)
			{
				_isRestrictingHeight = false;
			}
		}
	}

	private IEnumerator SetButtonsActiveDelayed(bool shouldBeVisible)
	{
		yield return new WaitForEndOfFrame();
		SetButtonsActive(shouldBeVisible);
	}

	private void SetButtonsActive(bool shouldBeVisible)
	{
		if (_expandAnyway != null)
		{
			_expandAnyway.gameObject.SetActive(shouldBeVisible);
		}
		if (_showWhenRestrictingHeight != null)
		{
			_showWhenRestrictingHeight.SetActive(shouldBeVisible);
		}
	}

	private void SetDirty()
	{
		if (base.isActiveAndEnabled)
		{
			LayoutRebuilder.MarkLayoutForRebuild(base.transform as RectTransform);
		}
	}
}
