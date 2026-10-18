using System.Collections.Generic;
using Modio.Unity.Settings;
using Modio.Unity.UI.Components.Localization;
using Modio.Unity.UI.Search;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Components;

public class ModioUISearchCategoryTabGroup : MonoBehaviour
{
	[SerializeField]
	private ModioUISearchCategoryTab _firstTab;

	[SerializeField]
	private GameObject _disableIfNoCategory;

	[SerializeField]
	private TMP_Text _categoryName;

	[SerializeField]
	private ModioUILocalizedText _categoryNameLocalized;

	private readonly List<ModioUISearchCategoryTab> _tabs = new List<ModioUISearchCategoryTab>();

	private bool _hasRunStart;

	private int _activeTabCount;

	private int _setCategoryOnFrame;

	public void ClearCategory()
	{
		if (_setCategoryOnFrame != Time.frameCount)
		{
			SetCategory(null);
		}
	}

	public void SetCategory(ModioUISearchCategory category)
	{
		if (_disableIfNoCategory != null)
		{
			_disableIfNoCategory.SetActive(category != null);
		}
		if (category == null)
		{
			return;
		}
		_setCategoryOnFrame = Time.frameCount;
		SetTabs(category.Tabs);
		if (ModioUISearch.Default != null)
		{
			if (category.CustomSearchBase != null)
			{
				category.CustomSearchBase.SetAsCustomSearchBase(ModioUISearch.Default);
			}
			else
			{
				ModioUISearch.Default.SetCustomSearchBase(null, (SpecialSearchType)0);
			}
		}
		if (_categoryName != null)
		{
			_categoryName.text = category.CategoryLabel;
		}
		if (_categoryNameLocalized != null)
		{
			_categoryNameLocalized.SetKey(category.CategoryLabelLocalized);
		}
	}

	private void Start()
	{
		if (_tabs.Count > 0)
		{
			_tabs[0].SetSelected();
		}
		else if (_disableIfNoCategory != null)
		{
			_disableIfNoCategory.SetActive(value: false);
		}
		if (_activeTabCount == 1)
		{
			_tabs[0].gameObject.SetActive(value: false);
		}
		_hasRunStart = true;
	}

	public void SetTabs(IEnumerable<ModioUISearchSettings> tabSearches)
	{
		int i = 0;
		bool flag = ModioClient.Settings.GetPlatformSettings<ModioComponentUISettings>()?.ShowMonetizationUI ?? false;
		foreach (ModioUISearchSettings tabSearch in tabSearches)
		{
			if (flag || !tabSearch.HiddenIfMonetizationDisabled)
			{
				ModioUISearchCategoryTab modioUISearchCategoryTab;
				if (i < _tabs.Count)
				{
					modioUISearchCategoryTab = _tabs[i];
					modioUISearchCategoryTab.gameObject.SetActive(value: true);
				}
				else if (i == 0)
				{
					_tabs.Add(_firstTab);
					modioUISearchCategoryTab = _firstTab;
				}
				else
				{
					modioUISearchCategoryTab = Object.Instantiate(_firstTab, _firstTab.transform.parent);
					modioUISearchCategoryTab.transform.SetSiblingIndex(_firstTab.transform.GetSiblingIndex() + i);
					_tabs.Add(modioUISearchCategoryTab);
				}
				modioUISearchCategoryTab.SetSearch(tabSearch);
				i++;
			}
		}
		for (_activeTabCount = i; i < _tabs.Count; i++)
		{
			_tabs[i].gameObject.SetActive(value: false);
		}
		if (_hasRunStart)
		{
			for (int j = 0; j < _tabs.Count; j++)
			{
				_tabs[j].SetSelected(j == 0);
			}
			if (_activeTabCount == 1)
			{
				_tabs[0].gameObject.SetActive(value: false);
			}
		}
		if (_disableIfNoCategory != null && _activeTabCount <= 1)
		{
			_disableIfNoCategory.SetActive(value: false);
		}
	}
}
