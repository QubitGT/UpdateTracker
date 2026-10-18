using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modio.API;
using Modio.Errors;
using Newtonsoft.Json.Linq;

namespace Modio.Customizations;

public class GamePlacement
{
	public const string TYPE_FILTER = "filter";

	private static readonly Dictionary<string, GamePlacement[]> placementsCache = new Dictionary<string, GamePlacement[]>();

	private static Task<(Error error, GamePlacement[] placements)> pendingRequest;

	public long Id { get; private set; }

	public string Name { get; private set; }

	public string NameLocalized { get; private set; }

	public IReadOnlyDictionary<string, string> NameLocalization { get; private set; }

	public string Type { get; private set; }

	public int DisplayPosition { get; private set; }

	public int Total { get; private set; }

	public string Size { get; private set; }

	public bool Enabled { get; private set; }

	public IReadOnlyDictionary<string, string> Filters { get; private set; }

	public string Sort
	{
		get
		{
			if (Filters == null || !Filters.TryGetValue("_sort", out var value))
			{
				return null;
			}
			return value;
		}
	}

	public string Timespan
	{
		get
		{
			if (Filters == null || !Filters.TryGetValue("timespan", out var value))
			{
				return null;
			}
			return value;
		}
	}

	public bool IsFilterPlacement => string.Equals(Type, "filter", StringComparison.OrdinalIgnoreCase);

	private GamePlacement()
	{
	}

	public static GamePlacement CreateLocal(string name, string sort, string timespan = null, int total = 20, int displayPosition = 0)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!string.IsNullOrEmpty(sort))
		{
			dictionary["_sort"] = sort;
		}
		if (!string.IsNullOrEmpty(timespan))
		{
			dictionary["timespan"] = timespan;
		}
		return new GamePlacement
		{
			Id = -1L,
			Name = name,
			NameLocalized = name,
			NameLocalization = new Dictionary<string, string>(),
			Type = "filter",
			DisplayPosition = displayPosition,
			Total = total,
			Size = "small",
			Enabled = true,
			Filters = dictionary
		};
	}

	public string GetTitle(string languageCode = null)
	{
		if (!string.IsNullOrEmpty(languageCode) && NameLocalization != null && NameLocalization.TryGetValue(languageCode, out var value) && !string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (!string.IsNullOrEmpty(NameLocalized))
		{
			return NameLocalized;
		}
		return Name ?? string.Empty;
	}

	public ModioAPI.Mods.GetModsFilter ApplyTo(ModioAPI.Mods.GetModsFilter filter)
	{
		if (filter == null || Filters == null)
		{
			return filter;
		}
		foreach (KeyValuePair<string, string> filter2 in Filters)
		{
			if (!string.IsNullOrEmpty(filter2.Key))
			{
				filter.CustomParameter(filter2.Key, filter2.Value);
			}
		}
		return filter;
	}

	public ModioAPI.Mods.GetModsFilter CreateModsFilter(int pageIndex = 0, int pageSize = -1)
	{
		if (pageSize <= 0)
		{
			pageSize = ((Total > 0) ? Total : 20);
		}
		ModioAPI.Mods.GetModsFilter filter = ModioAPI.Mods.FilterGetMods(pageIndex, pageSize);
		return ApplyTo(filter);
	}

	public static async Task<(Error error, GamePlacement[] placements)> GetGamePlacements(bool forceRefresh = false)
	{
		string text = ModioAPI.LanguageCodeResponse ?? string.Empty;
		GamePlacement[] value;
		if (forceRefresh)
		{
			placementsCache.Remove(text);
		}
		else if (placementsCache.TryGetValue(text, out value))
		{
			return (error: Error.None, placements: value);
		}
		if (pendingRequest != null && !pendingRequest.IsCompleted)
		{
			return await pendingRequest;
		}
		pendingRequest = RequestPlacements(text);
		return await pendingRequest;
	}

	private static async Task<(Error error, GamePlacement[] placements)> RequestPlacements(string cacheKey)
	{
		var (error, gameObject) = await ModioAPI.Games.GetGameAsJToken();
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log("Error getting game placements: " + error.GetMessage());
			}
			return (error: error, placements: null);
		}
		GamePlacement[] array;
		try
		{
			array = ParsePlacements(gameObject);
		}
		catch (Exception arg)
		{
			ModioLog.Error?.Log($"Error parsing game placements: {arg}");
			return (error: new Error(ErrorCode.UNKNOWN, "Failed to parse game placements"), placements: null);
		}
		placementsCache[cacheKey] = array;
		return (error: Error.None, placements: array);
	}

	private static GamePlacement[] ParsePlacements(JToken gameObject)
	{
		if (gameObject == null || gameObject.Type != JTokenType.Object)
		{
			return Array.Empty<GamePlacement>();
		}
		if (!(gameObject["placements"] is JArray jArray))
		{
			return Array.Empty<GamePlacement>();
		}
		List<GamePlacement> list = new List<GamePlacement>(jArray.Count);
		foreach (JToken item2 in jArray)
		{
			if (item2 is JObject jObject)
			{
				GamePlacement item = new GamePlacement
				{
					Id = (jObject.Value<long?>("id") ?? (-1)),
					Name = (jObject.Value<string>("name") ?? string.Empty),
					NameLocalized = jObject.Value<string>("name_localized"),
					Type = (jObject.Value<string>("type") ?? "filter"),
					DisplayPosition = jObject.Value<int?>("display_position").GetValueOrDefault(),
					Total = jObject.Value<int?>("total").GetValueOrDefault(),
					Size = jObject.Value<string>("size"),
					Enabled = (jObject.Value<bool?>("enabled") ?? true),
					NameLocalization = ReadStringDictionary(jObject["name_localization"]),
					Filters = ReadStringDictionary(jObject["filters"])
				};
				list.Add(item);
			}
		}
		return (from p in list
			orderby p.DisplayPosition, p.Id
			select p).ToArray();
	}

	private static IReadOnlyDictionary<string, string> ReadStringDictionary(JToken token)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!(token is JObject jObject))
		{
			return dictionary;
		}
		foreach (JProperty item in jObject.Properties())
		{
			if (item.Value != null && item.Value.Type != JTokenType.Null && item.Value is JValue jValue)
			{
				dictionary[item.Name] = ((jValue.Type != JTokenType.Boolean) ? jValue.ToString() : (jValue.Value<bool>() ? "1" : "0"));
			}
		}
		return dictionary;
	}
}
