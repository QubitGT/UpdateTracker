using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Modio.API;
using Modio.Errors;
using Modio.Mods;
using Modio.Unity.Settings;
using Modio.Unity.UI.Components;
using Modio.Users;
using UnityEngine;
using UnityEngine.Events;

namespace Modio.Unity.UI.Search;

public class ModioUISearch : MonoBehaviour, IModioUIPropertiesOwner
{
	[SerializeField]
	private bool _isDefault = true;

	[Header("Optional Overrides")]
	[SerializeField]
	private ModioUISearchSettings _searchOnStart;

	[SerializeField]
	private ModioUISearchSettings _searchForUser;

	[SerializeField]
	private ModioUISearchSettings _searchForTag;

	[SerializeField]
	private int _defaultPageSize = 24;

	[SerializeField]
	[Tooltip("Allow search to run before we have an authenticated user")]
	private bool _allowSearchWithoutUser;

	private SpecialSearchType _searchPreset;

	public UnityEvent OnSearchUpdatedUnityEvent;

	private (ModSearchFilter searchFilter, SpecialSearchType specialSearchType, object shareFiltersWith) _resetToSearch;

	private (ModSearchFilter searchFilter, SpecialSearchType specialSearchType) _baseForCustomSearch;

	private int _lastPageIndex;

	private int _asyncSearchIndex;

	private object _shareFiltersWith;

	private List<Mod> _lastLocalQueryInFull;

	public static ModioUISearch Default { get; private set; }

	public ModSearchFilter LastSearchFilter { get; private set; } = new ModSearchFilter();


	public SpecialSearchType LastSearchPreset => _searchPreset;

	public bool IsSearching { get; private set; }

	public bool IsAdditiveSearch { get; private set; }

	public IReadOnlyList<Mod> LastSearchResultMods { get; private set; } = new Collection<Mod>();


	public int LastSearchResultModCount { get; private set; }

	public int LastSearchResultPageCount => Mathf.CeilToInt((float)LastSearchResultModCount / (float)Mathf.Max(LastSearchFilter.PageSize, LastSearchResultMods.Count));

	public bool CanGetMoreMods
	{
		get
		{
			if (LastSearchResultMods != null)
			{
				return LastSearchResultModCount > LastSearchResultMods.Count;
			}
			return false;
		}
	}

	public Error LastSearchError { get; private set; } = Error.None;


	public int LastSearchSelectionIndex { get; private set; }

	public ModioUISearchSettings LastSearchSettingsFrom { get; private set; }

	public bool SortByOverriden { get; private set; }

	public int DefaultPageSize => _defaultPageSize;

	public event Action AppliedSearchPreset;

	private void Awake()
	{
		if (_isDefault || Default == null)
		{
			Default = this;
		}
	}

	private void OnDestroy()
	{
		ModioClient.OnInitialized -= PluginReady;
		if (Default == this)
		{
			Default = null;
		}
		User.OnUserChanged -= PluginReady;
	}

	private void Start()
	{
		ModioClient.OnInitialized += PluginReady;
	}

	private void PluginReady(User _)
	{
		PluginReady();
	}

	private void PluginReady()
	{
		User.OnUserChanged -= PluginReady;
		if (!_allowSearchWithoutUser && (User.Current == null || !User.Current.IsInitialized))
		{
			LastSearchResultMods = new Collection<Mod>();
			User.OnUserChanged += PluginReady;
		}
		else if (_resetToSearch.searchFilter != null)
		{
			ClearSearch();
		}
		else if (_searchOnStart != null)
		{
			_searchOnStart.Search(this);
		}
		else
		{
			LastSearchResultMods = new Collection<Mod>();
			OnSearchUpdatedUnityEvent.Invoke();
		}
	}

	public void AddUpdatePropertiesListener(UnityAction listener)
	{
		OnSearchUpdatedUnityEvent.AddListener(listener);
	}

	public void RemoveUpdatePropertiesListener(UnityAction listener)
	{
		OnSearchUpdatedUnityEvent.RemoveListener(listener);
	}

	public void ApplySortBy(SortModsBy sortModsBy, bool ascending)
	{
		SortByOverriden = true;
		LastSearchFilter.SortBy = sortModsBy;
		LastSearchFilter.IsSortAscending = ascending;
		LastSearchFilter.PageIndex = 0;
		SetSearch(LastSearchFilter);
	}

	public void ApplySearchPhrase(string query)
	{
		ModSearchFilter modSearchFilter = LastSearchFilter;
		if (_baseForCustomSearch.searchFilter != null && _baseForCustomSearch.searchFilter != modSearchFilter)
		{
			modSearchFilter = _baseForCustomSearch.searchFilter;
			_searchPreset = _baseForCustomSearch.specialSearchType;
		}
		Filtering filtering = Filtering.Like;
		modSearchFilter.ClearSearchPhrases(filtering);
		if (!string.IsNullOrEmpty(query))
		{
			modSearchFilter.AddSearchPhrase(query, filtering);
		}
		modSearchFilter.PageIndex = 0;
		SetSearch(modSearchFilter);
	}

	public void ApplyTagsToSearch(IEnumerable<string> tags)
	{
		LastSearchFilter.ClearTags();
		LastSearchFilter.AddTags(tags);
		if (_searchPreset == SpecialSearchType.SearchForTag && !LastSearchFilter.GetTags().Any())
		{
			ClearSearch();
			return;
		}
		LastSearchFilter.PageIndex = 0;
		SetSearch(LastSearchFilter);
	}

	public bool HasCustomSearch()
	{
		if (LastSearchFilter.GetUsers().Count <= 0 && LastSearchFilter.GetSearchPhrase(Filtering.Like).Count <= 0 && _searchPreset != SpecialSearchType.SearchForTag)
		{
			return _searchPreset == SpecialSearchType.SearchForUser;
		}
		return true;
	}

	public void ClearSearch()
	{
		if (_resetToSearch.searchFilter != null)
		{
			ModSearchFilter item = _resetToSearch.searchFilter;
			item.ClearSearchPhrases(Filtering.Like);
			item.ClearTags();
			item.PageIndex = 0;
			SetSearch(item, _resetToSearch.specialSearchType);
		}
		else
		{
			Debug.LogWarning("No default search available to reset back to");
		}
	}

	public void SetSearchForUser(UserProfile user)
	{
		ModSearchFilter modSearchFilter = ((!(_searchForUser != null)) ? new ModSearchFilter(0, _defaultPageSize)
		{
			RevenueType = LastSearchFilter.RevenueType,
			ShowMatureContent = LastSearchFilter.ShowMatureContent
		} : _searchForUser.GetSearchFilter(_defaultPageSize));
		modSearchFilter.AddUser(user);
		SetSearch(modSearchFilter, SpecialSearchType.SearchForUser);
	}

	public void SetSearchForTag(ModTag tag)
	{
		ModSearchFilter modSearchFilter = ((!(_searchForTag != null)) ? new ModSearchFilter(0, _defaultPageSize)
		{
			RevenueType = LastSearchFilter.RevenueType,
			ShowMatureContent = LastSearchFilter.ShowMatureContent
		} : _searchForTag.GetSearchFilter(_defaultPageSize));
		modSearchFilter.AddTag(tag.ApiName);
		SetSearch(modSearchFilter, SpecialSearchType.SearchForTag);
	}

	public void GetNextPageAdditivelyForLastSearch()
	{
		LastSearchFilter.PageIndex = _lastPageIndex + 1;
		if (_lastLocalQueryInFull != null)
		{
			LastSearchSelectionIndex = LastSearchResultMods.Count;
			int count = Mathf.Min(_lastLocalQueryInFull.Count, LastSearchResultMods.Count + LastSearchFilter.PageSize);
			LastSearchResultMods = _lastLocalQueryInFull.Take(count).ToList();
			OnSearchUpdatedUnityEvent.Invoke();
		}
		else
		{
			SetSearch(LastSearchFilter, isAdditiveSearch: true);
		}
	}

	public void SetPageForCurrentSearch(int page)
	{
		LastSearchFilter.PageIndex = page;
		SetSearch(LastSearchFilter);
	}

	public void SetSearch(ModSearchFilter searchFilter, SpecialSearchType specialSearchType, bool resetToThis = false, object shareFiltersWith = null, ModioUISearchSettings settingsFrom = null)
	{
		if (resetToThis)
		{
			_resetToSearch = (searchFilter: searchFilter, specialSearchType: specialSearchType, shareFiltersWith: shareFiltersWith);
		}
		LastSearchSettingsFrom = settingsFrom;
		if (!ModioClient.IsInitialized || (!_allowSearchWithoutUser && (User.Current == null || !User.Current.IsInitialized)))
		{
			if (resetToThis)
			{
				ModioLog.Verbose?.Log("Attempting to set search before plugin is ready. Search will run once plugin is ready");
			}
			else
			{
				ModioLog.Warning?.Log("Attempting to set search before plugin is ready. As resetToThis is false, this search will be discarded");
			}
			return;
		}
		_searchPreset = specialSearchType;
		SortByOverriden = false;
		if (shareFiltersWith != null && shareFiltersWith == _shareFiltersWith)
		{
			searchFilter.AddTags(LastSearchFilter.GetTags());
			for (Filtering filtering = Filtering.None; filtering <= Filtering.BitwiseAnd; filtering++)
			{
				searchFilter.AddSearchPhrases(LastSearchFilter.GetSearchPhrase(filtering), filtering);
			}
		}
		_shareFiltersWith = shareFiltersWith;
		ModioComponentUISettings platformSettings = ModioClient.Settings.GetPlatformSettings<ModioComponentUISettings>();
		if (platformSettings == null || !platformSettings.ShowMonetizationUI)
		{
			searchFilter.RevenueType = RevenueType.Free;
		}
		SetSearch(searchFilter);
		this.AppliedSearchPreset?.Invoke();
	}

	public void SetCustomSearchBase(ModSearchFilter searchFilter, SpecialSearchType searchType)
	{
		_baseForCustomSearch = (searchFilter: searchFilter, specialSearchType: searchType);
	}

	private async void SetSearch(ModSearchFilter searchFilter, bool isAdditiveSearch = false, Task<(Error error, IReadOnlyList<Mod> mods, int totalCount)> customResultProvider = null)
	{
		LastSearchFilter = searchFilter;
		_lastPageIndex = LastSearchFilter.PageIndex;
		_lastLocalQueryInFull = null;
		IsSearching = true;
		IsAdditiveSearch = isAdditiveSearch;
		if (!isAdditiveSearch)
		{
			LastSearchResultMods = Array.Empty<Mod>();
		}
		LastSearchError = Error.None;
		OnSearchUpdatedUnityEvent.Invoke();
		int asyncSearchIndex = ++_asyncSearchIndex;
		(Error, IReadOnlyList<Mod>, int) tuple;
		if (customResultProvider != null)
		{
			tuple = await customResultProvider;
		}
		else
		{
			switch (_searchPreset)
			{
			case SpecialSearchType.Installed:
			case SpecialSearchType.Subscribed:
			case SpecialSearchType.InstalledOrSubscribed:
			case SpecialSearchType.Purchased:
				tuple = await GetModsViaLocalQuery();
				break;
			case SpecialSearchType.UserCreations:
				tuple = await GetCurrentUserCreationsQuery();
				break;
			default:
				tuple = await GetModsViaStandardQuery();
				break;
			}
		}
		if (asyncSearchIndex != _asyncSearchIndex)
		{
			return;
		}
		IsSearching = false;
		if (!isAdditiveSearch)
		{
			LastSearchResultMods = tuple.Item2 ?? Array.Empty<Mod>();
			LastSearchSelectionIndex = 0;
		}
		else
		{
			LastSearchSelectionIndex = LastSearchResultMods.Count;
			List<Mod> list = new List<Mod>(LastSearchResultMods);
			if (tuple.Item2 != null)
			{
				list.AddRange(tuple.Item2);
			}
			LastSearchResultMods = list;
		}
		LastSearchResultModCount = tuple.Item3;
		(LastSearchError, _, _) = tuple;
		if (tuple.Item1.Code != ErrorCode.SHUTTING_DOWN)
		{
			OnSearchUpdatedUnityEvent.Invoke();
		}
	}

	private async Task<(Error error, IReadOnlyList<Mod> mods, int totalCount)> GetModsViaStandardQuery()
	{
		(Error, ModioPage<Mod>) tuple = await Mod.GetMods(LastSearchFilter.GetModsFilter());
		var (error, _) = tuple;
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log("Error getting mods: " + error.GetMessage());
			}
			return (error: error, mods: null, totalCount: 0);
		}
		return (error: error, mods: tuple.Item2.Data, totalCount: (int)tuple.Item2.TotalSearchResults);
	}

	private async Task<(Error error, IReadOnlyList<Mod> mods, int totalCount)> GetCurrentUserCreationsQuery()
	{
		ModRepository modRepository = User.Current.ModRepository;
		IEnumerable<Mod> enumerable = Enumerable.Empty<Mod>();
		if (_searchPreset == SpecialSearchType.UserCreations)
		{
			enumerable = modRepository.GetCreatedMods();
		}
		if (enumerable == null)
		{
			ModioLog.Error?.Log("Unable to construct local query results for " + _searchPreset);
			Error unknown = Error.Unknown;
			return (error: unknown, mods: null, totalCount: 0);
		}
		List<Mod> list = enumerable.Where(MatchesFilter).Distinct().ToList();
		list.Sort(SortModComparer);
		int count = list.Count;
		if (count > LastSearchFilter.PageSize)
		{
			_lastLocalQueryInFull = list;
			list = list.Skip(LastSearchFilter.PageSize * LastSearchFilter.PageIndex).Take(LastSearchFilter.PageSize).ToList();
		}
		return (error: Error.None, mods: list, totalCount: count);
	}

	private async Task<(Error error, IReadOnlyList<Mod> mods, int totalCount)> GetModsViaLocalQuery()
	{
		ModRepository repo = User.Current.ModRepository;
		IEnumerable<Mod> mods = Enumerable.Empty<Mod>();
		if (_searchPreset == SpecialSearchType.Subscribed || _searchPreset == SpecialSearchType.InstalledOrSubscribed)
		{
			mods = repo.GetSubscribed();
		}
		if (_searchPreset == SpecialSearchType.Installed || _searchPreset == SpecialSearchType.InstalledOrSubscribed)
		{
			ICollection<Mod> collection = await ModInstallationManagement.GetAllInstalledMods();
			if (mods == null)
			{
				mods = collection;
			}
			else if (collection != null)
			{
				mods = mods.Concat(collection);
			}
		}
		if (_searchPreset == SpecialSearchType.Purchased)
		{
			IEnumerable<Mod> purchased = repo.GetPurchased();
			if (mods == null)
			{
				mods = purchased;
			}
			else if (purchased != null)
			{
				mods = mods.Concat(purchased);
			}
		}
		if (mods == null)
		{
			ModioLog.Error?.Log("Unable to construct local query results for " + _searchPreset);
			Error unknown = Error.Unknown;
			return (error: unknown, mods: null, totalCount: 0);
		}
		List<Mod> list = mods.Where(MatchesFilter).Distinct().ToList();
		list.Sort(SortModComparer);
		int count = list.Count;
		if (count > LastSearchFilter.PageSize)
		{
			_lastLocalQueryInFull = list;
			list = list.Skip(LastSearchFilter.PageSize * LastSearchFilter.PageIndex).Take(LastSearchFilter.PageSize).ToList();
		}
		return (error: Error.None, mods: list, totalCount: count);
	}

	private bool MatchesFilter(Mod mod)
	{
		foreach (string tag in LastSearchFilter.GetTags())
		{
			if (mod.Tags.All((ModTag modTag) => modTag.ApiName != tag))
			{
				return false;
			}
		}
		foreach (string item in LastSearchFilter.GetSearchPhrase(Filtering.None))
		{
			if (mod.Name.IndexOf(item, StringComparison.InvariantCultureIgnoreCase) < 0)
			{
				return false;
			}
		}
		return true;
	}

	private int SortModComparer(Mod x, Mod y)
	{
		int num = LastSearchFilter.SortBy switch
		{
			SortModsBy.Name => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase), 
			SortModsBy.Price => -x.Price.CompareTo(y.Price), 
			SortModsBy.Rating => -x.Stats.RatingsPercent.CompareTo(y.Stats.RatingsPercent), 
			SortModsBy.Popular => -x.Stats.RatingsPositive.CompareTo(y.Stats.RatingsPositive), 
			SortModsBy.Downloads => -x.Stats.Downloads.CompareTo(y.Stats.Downloads), 
			SortModsBy.Subscribers => -x.Stats.Subscribers.CompareTo(y.Stats.Subscribers), 
			SortModsBy.DateSubmitted => -x.DateLive.CompareTo(y.DateLive), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (LastSearchFilter.IsSortAscending)
		{
			num = -num;
		}
		return num;
	}

	public void SetSearchForDependencies(Mod dependant)
	{
		SetSearch(new ModSearchFilter(), isAdditiveSearch: false, GetModsViaDependencies());
		async Task<(Error error, IReadOnlyList<Mod> dependencies, int totalCount)> GetModsViaDependencies()
		{
			if (!dependant.Dependencies.HasDependencies)
			{
				return (error: Error.None, dependencies: Array.Empty<Mod>(), totalCount: 0);
			}
			var (error, readOnlyList) = await dependant.Dependencies.GetAllDependencies();
			if ((bool)error)
			{
				return (error: error, dependencies: Array.Empty<Mod>(), totalCount: 0);
			}
			return (error: error, dependencies: readOnlyList, totalCount: readOnlyList.Count);
		}
	}
}
