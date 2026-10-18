using System.Collections.Generic;
using Modio.Mods;
using UnityEngine;

namespace Modio.Unity.UI.Search;

public class ModioUISearchSettings : MonoBehaviour
{
	public string DisplayAs;

	public string DisplayAsLocalisedKey;

	public Sprite Icon;

	public bool HiddenIfMonetizationDisabled;

	public SpecialSearchType searchType = SpecialSearchType.Nothing;

	public string searchPhrase;

	public List<string> searchTags;

	public SortModsBy sortModsBy;

	public bool showMatureContent;

	public bool isAscending;

	public RevenueType filterRevenueType;

	public Object shareFilterSettingsWith;

	public ModSearchFilter GetSearchFilter(int paginationSize)
	{
		ModSearchFilter modSearchFilter = new ModSearchFilter(0, paginationSize);
		modSearchFilter.SortBy = sortModsBy;
		modSearchFilter.ShowMatureContent = showMatureContent;
		modSearchFilter.IsSortAscending = isAscending;
		modSearchFilter.RevenueType = filterRevenueType;
		modSearchFilter.AddTags(searchTags);
		modSearchFilter.AddSearchPhrase(searchPhrase);
		return modSearchFilter;
	}

	public void Search(ModioUISearch searchWith)
	{
		if (searchWith == null)
		{
			searchWith = ModioUISearch.Default;
		}
		ModSearchFilter searchFilter = GetSearchFilter(searchWith.DefaultPageSize);
		searchWith.SetSearch(searchFilter, searchType, resetToThis: true, shareFilterSettingsWith, this);
	}

	public void SetAsCustomSearchBase(ModioUISearch searchWith)
	{
		if (searchWith == null)
		{
			searchWith = ModioUISearch.Default;
		}
		ModSearchFilter searchFilter = GetSearchFilter(searchWith.DefaultPageSize);
		searchWith.SetCustomSearchBase(searchFilter, searchType);
	}
}
