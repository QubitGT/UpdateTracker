using System;
using Modio.Mods;
using Modio.Unity.UI.Search;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertySortType : ISearchProperty
{
	[SerializeField]
	private TMP_Text _searchText;

	public void OnSearchUpdate(ModioUISearch search)
	{
		if (search.SortByOverriden)
		{
			SortModsBy sortBy = search.LastSearchFilter.SortBy;
			string text = ((sortBy == SortModsBy.DateSubmitted) ? "Date Submitted" : sortBy.ToString());
			_searchText.text = "<b>SORT:</b> " + text;
		}
		else
		{
			_searchText.text = "SORT";
		}
	}
}
