using System;
using Modio.Unity.UI.Input;
using Modio.Unity.UI.Panels;
using Modio.Unity.UI.Search;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyPagedSearch : ISearchProperty, IPropertyMonoBehaviourEvents
{
	[SerializeField]
	private TMP_Text _pageCountText;

	[SerializeField]
	private string _pageCountString = "Page {0} of {1}";

	[SerializeField]
	private Button _prevPage;

	[SerializeField]
	private Button _nextPage;

	[SerializeField]
	private ModioPanelBase _whenPanelFocused;

	private ModioUISearch _search;

	public void OnSearchUpdate(ModioUISearch search)
	{
		_search = search;
		if (_pageCountText != null)
		{
			int num = search.LastSearchFilter.PageIndex + 1;
			int lastSearchResultPageCount = search.LastSearchResultPageCount;
			_pageCountText.text = string.Format(_pageCountString, num, lastSearchResultPageCount);
		}
	}

	public void Start()
	{
		if (_prevPage != null)
		{
			_prevPage.onClick.AddListener(OnPrevPageClicked);
		}
		if (_nextPage != null)
		{
			_nextPage.onClick.AddListener(OnNextPageClicked);
		}
	}

	private void OnPrevPageClicked()
	{
		if (!(_whenPanelFocused != null) || _whenPanelFocused.HasFocus)
		{
			int pageIndex = _search.LastSearchFilter.PageIndex;
			if (pageIndex > 0)
			{
				_search.SetPageForCurrentSearch(pageIndex - 1);
			}
		}
	}

	private void OnNextPageClicked()
	{
		if (!(_whenPanelFocused != null) || _whenPanelFocused.HasFocus)
		{
			int pageIndex = _search.LastSearchFilter.PageIndex;
			if (pageIndex + 1 < _search.LastSearchResultPageCount)
			{
				_search.SetPageForCurrentSearch(pageIndex + 1);
			}
		}
	}

	public void OnDestroy()
	{
	}

	public void OnEnable()
	{
		ModioUIInput.AddHandler(ModioUIInput.ModioAction.SearchPageLeft, OnPrevPageClicked);
		ModioUIInput.AddHandler(ModioUIInput.ModioAction.SearchPageRight, OnNextPageClicked);
	}

	public void OnDisable()
	{
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.SearchPageLeft, OnPrevPageClicked);
		ModioUIInput.RemoveHandler(ModioUIInput.ModioAction.SearchPageRight, OnNextPageClicked);
	}
}
