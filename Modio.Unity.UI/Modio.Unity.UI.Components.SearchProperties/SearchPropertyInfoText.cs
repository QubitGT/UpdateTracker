using System;
using System.Collections.Generic;
using Modio.API;
using Modio.Mods;
using Modio.Unity.UI.Components.Localization;
using Modio.Unity.UI.Search;
using Modio.Users;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components.SearchProperties;

[Serializable]
public class SearchPropertyInfoText : ISearchProperty
{
	[SerializeField]
	private TMP_Text _searchText;

	[SerializeField]
	private GameObject _disableWhileShowingCustomText;

	[SerializeField]
	private GameObject _showWhileShowingCustomText;

	[SerializeField]
	private TMP_Text _searchCategoryName;

	[SerializeField]
	private ModioUILocalizedText _searchCategoryNameLocalized;

	[SerializeField]
	private Image _searchCategoryIcon;

	public void OnSearchUpdate(ModioUISearch search)
	{
		ModSearchFilter lastSearchFilter = search.LastSearchFilter;
		IList<string> searchPhrase = lastSearchFilter.GetSearchPhrase(Filtering.Like);
		bool flag = searchPhrase != null && searchPhrase.Count > 0;
		if (_searchText != null)
		{
			_searchText.enabled = flag;
			if (flag)
			{
				_searchText.text = string.Join(" ", searchPhrase) ?? "";
			}
			if (SpecialSearchType.SearchForTag == search.LastSearchPreset)
			{
				_searchText.enabled = true;
				IReadOnlyList<string> tags = lastSearchFilter.GetTags();
				_searchText.text = string.Join(" ", tags) ?? "";
			}
		}
		IReadOnlyList<UserProfile> users = lastSearchFilter.GetUsers();
		bool flag2 = false;
		if (users.Count > 0)
		{
			UserProfile userProfile = users[0];
			flag2 = userProfile != null;
			if (_searchText != null)
			{
				_searchText.enabled = flag2;
				if (flag2)
				{
					_searchText.text = userProfile.Username ?? "";
				}
			}
		}
		if (_disableWhileShowingCustomText != null)
		{
			_disableWhileShowingCustomText.SetActive(!(flag || flag2));
		}
		if (_showWhileShowingCustomText != null)
		{
			_showWhileShowingCustomText.SetActive(flag || flag2);
		}
		if (search.LastSearchSettingsFrom != null)
		{
			if (_searchCategoryName != null)
			{
				_searchCategoryName.text = search.LastSearchSettingsFrom.DisplayAs;
			}
			if (_searchCategoryNameLocalized != null)
			{
				_searchCategoryNameLocalized.SetKey(search.LastSearchSettingsFrom.DisplayAsLocalisedKey);
			}
			if (_searchCategoryIcon != null)
			{
				_searchCategoryIcon.sprite = search.LastSearchSettingsFrom.Icon;
				_searchCategoryIcon.enabled = search.LastSearchSettingsFrom.Icon != null;
			}
		}
	}
}
