using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PageScroll : UIBehaviour
{
	[Serializable]
	public struct Page
	{
		public Toggle toggle;

		public RectTransform container;

		public CanvasGroup canvasGroup;
	}

	[SerializeField]
	private ToggleGroup _toggleGroup;

	[SerializeField]
	private RectTransform _contentContainer;

	[SerializeField]
	private List<Page> _pages;

	[SerializeField]
	private int _pageIndex;

	public float animationSpeed;

	public AnimationCurve alphaTransitionCurve;

	private float _pageAnim;

	public void SetPageIndex(int pageIndex)
	{
		int num = ((pageIndex >= 0) ? ((pageIndex > _pages.Count - 1) ? (_pages.Count - 1) : pageIndex) : 0);
		if (_pageIndex != num)
		{
			_pageIndex = num;
			_pages[_pageIndex].toggle.isOn = true;
		}
	}

	public void ScrollPage(int direction)
	{
		int pageIndex = _pageIndex + direction;
		SetPageIndex(pageIndex);
	}

	protected override void OnEnable()
	{
		foreach (Page page in _pages)
		{
			page.toggle.onValueChanged.AddListener(delegate
			{
				ActiveToggleChanged(page.toggle);
			});
		}
	}

	protected override void OnDisable()
	{
		foreach (Page page in _pages)
		{
			page.toggle.onValueChanged.RemoveAllListeners();
		}
	}

	private void ActiveToggleChanged(Toggle toggle)
	{
		if (!(toggle == null) && toggle.isOn)
		{
			int num = _pages.FindIndex((Page page) => page.toggle == toggle);
			if (num >= 0)
			{
				_pageIndex = num;
			}
		}
	}

	protected override void Start()
	{
		StartCoroutine(LateStart());
	}

	private IEnumerator LateStart()
	{
		yield return null;
		if (_pages != null)
		{
			_pages[0].toggle.isOn = true;
		}
	}

	protected virtual void Update()
	{
		_pageAnim = Mathf.Lerp(_pageAnim, _pageIndex, animationSpeed * Time.deltaTime);
		_pageAnim = Mathf.Clamp(_pageAnim, 0f, _pages.Count - 1);
		UpdateVisial();
	}

	private void UpdateVisial()
	{
		if (Mathf.Abs(_pageAnim - (float)_pageIndex) < 0.005f)
		{
			Vector2 anchoredPosition = _pages[_pageIndex].container.anchoredPosition;
			_pages[_pageIndex].canvasGroup.alpha = 1f;
			SetOtherPagesTransparent(_pageIndex, -1);
			_contentContainer.anchoredPosition = anchoredPosition * new Vector2(-1f, 1f);
			return;
		}
		float num = Mathf.Clamp(_pageAnim, 0f, _pages.Count - 1);
		float num2 = Mathf.Floor(num);
		float num3 = Mathf.Ceil(num);
		int num4 = (int)num2;
		int num5 = (int)num3;
		float num6 = num - num2;
		Vector2 anchoredPosition2 = _pages[num4].container.anchoredPosition;
		Vector2 anchoredPosition3 = _pages[num5].container.anchoredPosition;
		SetOtherPagesTransparent(num4, num5);
		_pages[num4].canvasGroup.alpha = alphaTransitionCurve.Evaluate(1f - num6);
		_pages[num5].canvasGroup.alpha = alphaTransitionCurve.Evaluate(num6);
		_contentContainer.anchoredPosition = Vector2.Lerp(anchoredPosition2, anchoredPosition3, num6) * new Vector2(-1f, 1f);
	}

	private void SetOtherPagesTransparent(int index0, int index1)
	{
		for (int i = 0; i < _pages.Count; i++)
		{
			if (i != index0 && i != index1 && !(_pages[i].canvasGroup == null))
			{
				_pages[i].canvasGroup.alpha = 0f;
			}
		}
	}

	public void InjectAllPageScroll(ToggleGroup toggleGroup, RectTransform contentContainer, List<Page> pages, int pageIndex)
	{
		InjectToggleGroup(toggleGroup);
		InjectContentContainer(contentContainer);
		InjectPages(pages);
		InjectPageIndex(pageIndex);
	}

	public void InjectToggleGroup(ToggleGroup toggleGroup)
	{
		_toggleGroup = toggleGroup;
	}

	public void InjectContentContainer(RectTransform contentContainer)
	{
		_contentContainer = contentContainer;
	}

	public void InjectPages(List<Page> pages)
	{
		_pages = pages;
	}

	public void InjectPageIndex(int pageIndex)
	{
		_pageIndex = pageIndex;
	}
}
