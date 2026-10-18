using System.Collections.Generic;
using System.Linq;
using Modio.Mods;
using Modio.Unity.UI.Components.Selectables;
using Modio.Unity.UI.Search;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Modio.Unity.UI.Components;

public class ModioUIFilterDisplay : MonoBehaviour
{
	private class TagEntry
	{
		public ModioUIToggle Toggle;

		public string TagName;
	}

	[SerializeField]
	private ModioUIToggle checkboxTagItemPrefab;

	private List<TagEntry> checkboxTagItems = new List<TagEntry>();

	[SerializeField]
	private ModioUIToggle radioTagItemPrefab;

	[SerializeField]
	private ModioUIToggle categoryItemPrefab;

	[SerializeField]
	private Transform _contentContainer;

	private List<ModioUIFilterTagCategory> categoryItems = new List<ModioUIFilterTagCategory>();

	private bool _hasRegisteredListener;

	private bool _hasLocalChanges;

	private void Start()
	{
		ModioClient.OnInitialized += UpdateTags;
		if (!_hasRegisteredListener)
		{
			RegisterListener();
		}
	}

	private void OnDestroy()
	{
		ModioClient.OnInitialized -= UpdateTags;
	}

	private void OnEnable()
	{
		RegisterListener();
	}

	private void RegisterListener()
	{
		if (!(ModioUISearch.Default == null))
		{
			_hasRegisteredListener = true;
			ModioUISearch.Default.OnSearchUpdatedUnityEvent.AddListener(UpdateActiveTags);
			UpdateActiveTags();
		}
	}

	private void OnDisable()
	{
		ModioUISearch.Default.OnSearchUpdatedUnityEvent.RemoveListener(UpdateActiveTags);
	}

	public GameObject GetDefaultSelection()
	{
		if (checkboxTagItems.Count <= 0)
		{
			return null;
		}
		return (checkboxTagItems.FirstOrDefault((TagEntry t) => t.Toggle.isOn) ?? checkboxTagItems.First()).Toggle.gameObject;
	}

	public void UpdateActiveTags()
	{
		if (_hasLocalChanges)
		{
			return;
		}
		ModSearchFilter lastSearchFilter = ModioUISearch.Default.LastSearchFilter;
		foreach (TagEntry checkboxTagItem in checkboxTagItems)
		{
			checkboxTagItem.Toggle.isOn = lastSearchFilter.GetTags().Contains(checkboxTagItem.TagName);
		}
	}

	public void ApplyFilter()
	{
		IEnumerable<string> tags = from tagItem in checkboxTagItems
			where tagItem.Toggle.isOn
			select tagItem.TagName;
		_hasLocalChanges = false;
		ModioUISearch.Default.ApplyTagsToSearch(tags);
	}

	public void ClearFilter()
	{
		foreach (TagEntry checkboxTagItem in checkboxTagItems)
		{
			checkboxTagItem.Toggle.isOn = false;
		}
		_hasLocalChanges = false;
	}

	private async void UpdateTags()
	{
		(Error, GameTagCategory[]) obj = await GameTagCategory.GetGameTagOptions();
		Error item = obj.Item1;
		GameTagCategory[] item2 = obj.Item2;
		_hasLocalChanges = false;
		if ((bool)item)
		{
			if (!item.IsSilent)
			{
				ModioLog.Error?.Log($"Unable to get tags {item}");
			}
		}
		else
		{
			if (item2.Length == 0)
			{
				return;
			}
			HideListCheckboxItems(checkboxTagItems);
			HideListItems<ModioUIFilterTagCategory>(ref categoryItems);
			GameTagCategory[] array = item2;
			foreach (GameTagCategory gameTagCategory in array)
			{
				if (gameTagCategory.Hidden)
				{
					continue;
				}
				ModioUIToggle modioUIToggle = Object.Instantiate(categoryItemPrefab, _contentContainer);
				modioUIToggle.gameObject.SetActive(value: true);
				modioUIToggle.transform.SetAsLastSibling();
				ModioUIFilterTagCategory categoryFilterToggle = modioUIToggle.GetComponent<ModioUIFilterTagCategory>();
				categoryFilterToggle.Setup(gameTagCategory);
				categoryItems.Add(categoryFilterToggle);
				ToggleGroup componentInChildren = modioUIToggle.GetComponentInChildren<ToggleGroup>();
				List<ModioUIToggle> childToggles = new List<ModioUIToggle>();
				modioUIToggle.isOn = true;
				modioUIToggle.onValueChanged.AddListener(delegate(bool expanded)
				{
					foreach (ModioUIToggle item3 in childToggles)
					{
						item3.gameObject.SetActive(expanded);
					}
				});
				ModTag[] tags = gameTagCategory.Tags;
				foreach (ModTag modTag in tags)
				{
					ModioUIToggle modioUIToggle2;
					if (gameTagCategory.MultiSelect)
					{
						modioUIToggle2 = Object.Instantiate(checkboxTagItemPrefab, _contentContainer);
					}
					else
					{
						modioUIToggle2 = Object.Instantiate(radioTagItemPrefab, _contentContainer);
						modioUIToggle2.group = componentInChildren;
					}
					modioUIToggle2.GetComponentInChildren<TMP_Text>().text = modTag.NameLocalized;
					modioUIToggle2.gameObject.SetActive(value: true);
					modioUIToggle2.transform.SetAsLastSibling();
					modioUIToggle2.onValueChanged.AddListener(delegate(bool isOn)
					{
						categoryFilterToggle.SetFilterCount(categoryFilterToggle.CurrentFilterCount + (isOn ? 1 : (-1)));
						_hasLocalChanges = true;
					});
					checkboxTagItems.Add(new TagEntry
					{
						Toggle = modioUIToggle2,
						TagName = modTag.NameLocalized
					});
					childToggles.Add(modioUIToggle2);
				}
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		}
		static void HideListCheckboxItems(List<TagEntry> pool)
		{
			foreach (TagEntry item4 in pool)
			{
				Object.Destroy(item4.Toggle.gameObject);
			}
			pool.Clear();
		}
		static void HideListItems<T>(ref List<T> pool) where T : MonoBehaviour
		{
			foreach (T item5 in pool)
			{
				Object.Destroy(item5.gameObject);
			}
			pool.Clear();
		}
	}
}
