using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Mods;
using Modio.Users;

namespace Modio.Caching;

internal static class ModCache
{
	private class ModQueryCachedResponse
	{
		internal readonly Dictionary<long, Mod[]> Results = new Dictionary<long, Mod[]>();

		internal long ResultTotal { get; private set; }

		public void AddResults(Mod[] mods, long pageIndex, long resultTotal)
		{
			ResultTotal = resultTotal;
			Results[pageIndex] = mods;
		}
	}

	private static readonly Dictionary<ModId, Mod> Mods;

	private static readonly Dictionary<string, ModQueryCachedResponse> ModSearches;

	internal static int SearchesNotInCache;

	internal static int SearchesSavedByCache;

	private static readonly StringBuilder StringBuilder;

	internal static Mod GetMod(ModId modId)
	{
		if (!Mods.TryGetValue(modId, out var value))
		{
			return Mods[modId] = new Mod(modId);
		}
		return value;
	}

	internal static Mod GetMod(ModObject modObject)
	{
		if (!Mods.TryGetValue(modObject.Id, out var value))
		{
			return Mods[modObject.Id] = new Mod(modObject);
		}
		return value.ApplyDetailsFromModObject(modObject);
	}

	internal static bool TryGetMod(ModId modId, out Mod mod)
	{
		return Mods.TryGetValue(modId, out mod);
	}

	static ModCache()
	{
		Mods = new Dictionary<ModId, Mod>();
		ModSearches = new Dictionary<string, ModQueryCachedResponse>();
		StringBuilder = new StringBuilder();
		ModioClient.OnShutdown += Clear;
	}

	public static void Clear()
	{
		Mods.Clear();
		ModSearches.Clear();
		SearchesNotInCache = 0;
		SearchesSavedByCache = 0;
	}

	public static void RemoveModFromCache(ModId modId)
	{
		Mods.Remove(modId);
	}

	internal static bool GetCachedModSearch(ModioAPI.Mods.GetModsFilter filter, string searchKey, out Mod[] cachedMods, out long resultTotal)
	{
		if (ModSearches.TryGetValue(searchKey, out var value) && value.Results.TryGetValue(filter.PageIndex, out cachedMods))
		{
			resultTotal = value.ResultTotal;
			SearchesSavedByCache++;
			return true;
		}
		cachedMods = null;
		resultTotal = 0L;
		SearchesNotInCache++;
		return false;
	}

	internal static void CacheModSearch(string searchKey, Mod[] mods, long pageIndex, long resultTotal)
	{
		if (!ModSearches.TryGetValue(searchKey, out var value))
		{
			value = (ModSearches[searchKey] = new ModQueryCachedResponse());
		}
		value.AddResults(mods, pageIndex, resultTotal);
	}

	internal static void RemoveCachedModSearch(string searchKey)
	{
		ModSearches.Remove(searchKey);
	}

	internal static void ClearModSearchCache()
	{
		ModSearches.Clear();
		SearchesNotInCache = 0;
		SearchesSavedByCache = 0;
	}

	internal static string ConstructFilterKey(ModioAPI.Mods.GetModsFilter filter)
	{
		StringBuilder.Clear();
		StringBuilder.Append("pageSize:");
		StringBuilder.Append(filter.PageSize);
		StringBuilder.Append(",index:");
		StringBuilder.Append(filter.PageIndex);
		foreach (KeyValuePair<string, object> parameter in filter.Parameters)
		{
			if (!(parameter.Value is string) && parameter.Value is IEnumerable enumerable)
			{
				StringBuilder.AppendFormat(",{0}:[", parameter.Key);
				bool flag = true;
				foreach (object item in enumerable)
				{
					if (!flag)
					{
						StringBuilder.Append(',');
					}
					flag = false;
					StringBuilder.Append(item);
				}
				StringBuilder.Append(']');
			}
			else
			{
				StringBuilder.AppendFormat(",{0}:{1}", parameter.Key, parameter.Value);
			}
		}
		string result = StringBuilder.ToString();
		StringBuilder.Clear();
		return result;
	}

	public static Mod CreateHiddenModFromCachedIndexData(ModId modId, ModIndex tempIndex = null)
	{
		return GetMod(ModInstallationManagement.GetHiddenModObjectFromIndex(modId, tempIndex));
	}

	public static async Task<Error> RefreshPotentiallyHiddenCachedMods()
	{
		if (!User.Current.IsAuthenticated)
		{
			return Error.None;
		}
		List<long> list = new List<long>();
		foreach (KeyValuePair<ModId, Mod> mod in Mods)
		{
			if (mod.Value.Creator == null)
			{
				list.Add(mod.Key);
			}
		}
		ModioLog.Message?.Log($"Refreshing {list.Count} hidden cached mods...");
		return (await Mod.GetMods(list, forceRefresh: true)).Item1;
	}
}
