using System;
using System.Collections.Generic;
using System.Linq;
using Modio.API;
using Modio.Users;

namespace Modio.Mods;

[Serializable]
public class ModSearchFilter
{
	private Dictionary<Filtering, List<string>> _searchPhrases;

	private List<string> _tags;

	private List<UserProfile> _users;

	private int _pageSize;

	private int _pageIndex;

	public int PageIndex
	{
		get
		{
			return _pageIndex;
		}
		set
		{
			_pageIndex = ((value >= 0) ? value : 0);
		}
	}

	public int PageSize
	{
		get
		{
			return _pageSize;
		}
		set
		{
			_pageSize = ((value < 1) ? 1 : ((value > 100) ? 100 : value));
		}
	}

	public bool ShowMatureContent { get; set; }

	public SearchFilterPlatformStatus PlatformStatus { get; set; }

	public SortModsBy SortBy { get; set; } = SortModsBy.DateSubmitted;


	public bool IsSortAscending { get; set; } = true;


	public RevenueType RevenueType { get; set; }

	public ModSearchFilter(int pageIndex = 0, int pageSize = 100)
	{
		PageIndex = pageIndex;
		PageSize = pageSize;
	}

	public void AddSearchPhrase(string phrase, Filtering filtering = Filtering.Like)
	{
		if (!string.IsNullOrEmpty(phrase))
		{
			if (_searchPhrases == null)
			{
				_searchPhrases = new Dictionary<Filtering, List<string>>();
			}
			if (!_searchPhrases.TryGetValue(filtering, out var value))
			{
				value = new List<string>();
				_searchPhrases.Add(filtering, value);
			}
			value.Add(phrase);
		}
	}

	public void AddSearchPhrases(ICollection<string> phrase, Filtering filtering = Filtering.Like)
	{
		if (phrase != null && phrase.Count != 0)
		{
			if (_searchPhrases == null)
			{
				_searchPhrases = new Dictionary<Filtering, List<string>>();
			}
			if (!_searchPhrases.TryGetValue(filtering, out var value))
			{
				value = new List<string>();
				_searchPhrases.Add(filtering, value);
			}
			value.AddRange(phrase);
		}
	}

	public void ClearSearchPhrases()
	{
		_searchPhrases?.Clear();
	}

	public void ClearSearchPhrases(Filtering filtering)
	{
		_searchPhrases?.Remove(filtering);
	}

	public IList<string> GetSearchPhrase(Filtering filtering)
	{
		if (_searchPhrases != null && _searchPhrases.TryGetValue(filtering, out var value))
		{
			return value;
		}
		return Array.Empty<string>();
	}

	public void AddTag(string tag)
	{
		if (_tags == null)
		{
			_tags = new List<string>();
		}
		_tags.Add(tag);
	}

	public void AddTags(IEnumerable<string> tags)
	{
		if (_tags == null)
		{
			_tags = new List<string>();
		}
		_tags.AddRange(tags);
	}

	public void ClearTags()
	{
		_tags?.Clear();
	}

	public IReadOnlyList<string> GetTags()
	{
		IReadOnlyList<string> tags = _tags;
		return tags ?? Array.Empty<string>();
	}

	public void AddUser(UserProfile user)
	{
		if (_users == null)
		{
			_users = new List<UserProfile>();
		}
		_users.Add(user);
	}

	public IReadOnlyList<UserProfile> GetUsers()
	{
		IReadOnlyList<UserProfile> users = _users;
		return users ?? Array.Empty<UserProfile>();
	}

	public ModioAPI.Mods.GetModsFilter GetModsFilter()
	{
		ModioAPI.Mods.GetModsFilter getModsFilter = ModioAPI.Mods.FilterGetMods(PageIndex, PageSize);
		for (Filtering filtering = Filtering.None; filtering <= Filtering.BitwiseAnd; filtering++)
		{
			IList<string> searchPhrase = GetSearchPhrase(filtering);
			if (searchPhrase.Count > 0)
			{
				getModsFilter.Name("*" + searchPhrase[0] + "*", filtering);
			}
		}
		if (_tags != null && _tags.Count > 0)
		{
			getModsFilter.Tags(_tags);
		}
		if (_users != null && _users.Count > 0)
		{
			getModsFilter.SubmittedBy(_users.Select((UserProfile u) => u.UserId).ToArray());
		}
		getModsFilter.MaturityOption(ShowMatureContent ? 15 : 0, ShowMatureContent ? Filtering.BitwiseAnd : Filtering.None);
		string text = PlatformStatus switch
		{
			SearchFilterPlatformStatus.None => null, 
			SearchFilterPlatformStatus.PendingOnly => "pending_only", 
			SearchFilterPlatformStatus.LiveAndPending => "live_and_pending", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (text != null)
		{
			getModsFilter.PlatformStatus(text);
		}
		getModsFilter.SortByStringType(SortBy switch
		{
			SortModsBy.Name => "name", 
			SortModsBy.Price => "price", 
			SortModsBy.Rating => "ratings_weighted_aggregate", 
			SortModsBy.Popular => "downloads_today", 
			SortModsBy.Downloads => "downloads_total", 
			SortModsBy.Subscribers => "subscribers_total", 
			SortModsBy.DateSubmitted => "id", 
			_ => throw new ArgumentOutOfRangeException(), 
		}, IsSortAscending);
		getModsFilter.RevenueType((long)RevenueType);
		return getModsFilter;
	}
}
