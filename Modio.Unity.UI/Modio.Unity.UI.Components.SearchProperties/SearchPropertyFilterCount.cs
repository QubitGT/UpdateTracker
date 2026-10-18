using System;
using Modio.Unity.UI.Search;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyFilterCount : ISearchProperty
{
	[SerializeField]
	private TMP_Text _filterCount;

	[SerializeField]
	private GameObject _filterCountBackground;

	public void OnSearchUpdate(ModioUISearch search)
	{
		int count = search.LastSearchFilter.GetTags().Count;
		if (_filterCount != null)
		{
			_filterCount.text = count.ToString();
		}
		if (_filterCountBackground != null)
		{
			_filterCountBackground.SetActive(count > 0);
		}
	}
}
