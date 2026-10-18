using System;
using Modio.Errors;
using Modio.Unity.UI.Search;
using UnityEngine;
using UnityEngine.Events;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyDisplayResults : ISearchProperty
{
	[SerializeField]
	private ModioUIGroup _modGroup;

	[SerializeField]
	[Tooltip("(Optional) Enable this gameObject when there are zero results")]
	private GameObject _displayWhenNoResults;

	[SerializeField]
	[Tooltip("(Optional) Enable this gameObject when there are network issues and there's no results")]
	private GameObject _displayWhenOffline;

	[SerializeField]
	private UnityEvent<Error> _errorHandler;

	public void OnSearchUpdate(ModioUISearch search)
	{
		if (!search.IsSearching)
		{
			if (_modGroup != null)
			{
				_modGroup.SetMods(search.LastSearchResultMods, search.LastSearchSelectionIndex);
			}
			bool flag = _displayWhenOffline != null && search.LastSearchError.Code == ErrorCode.CANNOT_OPEN_CONNECTION;
			if ((bool)search.LastSearchError && !flag)
			{
				_errorHandler.Invoke(search.LastSearchError);
			}
			if (_displayWhenOffline != null)
			{
				_displayWhenOffline.SetActive(flag && search.LastSearchResultMods.Count == 0);
			}
			if (_displayWhenNoResults != null)
			{
				_displayWhenNoResults.SetActive(!flag && search.LastSearchResultMods.Count == 0);
			}
		}
		else
		{
			if (_displayWhenOffline != null)
			{
				_displayWhenOffline.SetActive(value: false);
			}
			if (_displayWhenNoResults != null)
			{
				_displayWhenNoResults.SetActive(value: false);
			}
		}
	}
}
