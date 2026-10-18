using System;
using Modio.Unity.UI.Search;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyLoadMoreResults : ISearchProperty, IPropertyMonoBehaviourEvents
{
	[SerializeField]
	private GameObject[] _displayWhenMoreResults;

	[SerializeField]
	private Button _loadMoreResultsButton;

	private ModioUISearch _search;

	public void OnSearchUpdate(ModioUISearch search)
	{
		_search = search;
		GameObject[] displayWhenMoreResults = _displayWhenMoreResults;
		for (int i = 0; i < displayWhenMoreResults.Length; i++)
		{
			displayWhenMoreResults[i].SetActive(!search.IsSearching && search.CanGetMoreMods);
		}
	}

	public void Start()
	{
	}

	public void OnDestroy()
	{
	}

	public void OnEnable()
	{
		if (_loadMoreResultsButton != null)
		{
			_loadMoreResultsButton.onClick.AddListener(LoadMoreClicked);
		}
	}

	public void OnDisable()
	{
		if (_loadMoreResultsButton != null)
		{
			_loadMoreResultsButton.onClick.RemoveListener(LoadMoreClicked);
		}
	}

	private void LoadMoreClicked()
	{
		_search.GetNextPageAdditivelyForLastSearch();
	}
}
