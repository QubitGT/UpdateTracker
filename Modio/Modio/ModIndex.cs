using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Modio.API.SchemaDefinitions;
using Modio.Mods;
using Modio.Users;
using Newtonsoft.Json;

namespace Modio;

[Serializable]
public class ModIndex
{
	[Serializable]
	internal class IndexEntry
	{
		public const long ID_NONE = -1L;

		public long DownloadedModfileId = -1L;

		public long InstalledModfileId = -1L;

		public long InstallationSize;

		public List<long> Subscribers = new List<long>();

		public DateTime ExpiresAfter = DateTime.UnixEpoch;

		public ModFileState FileState;
	}

	[JsonProperty]
	internal Dictionary<long, IndexEntry> Index = new Dictionary<long, IndexEntry>();

	[JsonProperty]
	internal Dictionary<long, ModObject> ModObjectCache = new Dictionary<long, ModObject>();

	[JsonIgnore]
	public bool IsDirty { get; internal set; }

	public ModIndex()
	{
		Mod.AddChangeListener(ModChangeType.ModObject, OnModObjectUpdate);
	}

	internal static async Task<(Error error, ModIndex result)> CreateIndexFromScan()
	{
		ModIndex output = new ModIndex();
		(Error, UserSaveObject[]) tuple = await ModioClient.DataStorage.ReadAllSavedUserData();
		if (!tuple.Item1)
		{
			UserSaveObject[] item = tuple.Item2;
			foreach (UserSaveObject userSaveObject in item)
			{
				foreach (long subscribedMod in userSaveObject.SubscribedMods)
				{
					if (output.Index.TryGetValue(subscribedMod, out var value) && !value.Subscribers.Contains(userSaveObject.UserId))
					{
						value.Subscribers.Add(userSaveObject.UserId);
						continue;
					}
					output.Index[subscribedMod] = new IndexEntry
					{
						Subscribers = new List<long> { userSaveObject.UserId }
					};
				}
			}
		}
		(Error, List<(long, long)>) tuple2 = await ModioClient.DataStorage.ScanForModfiles();
		if (!tuple2.Item1)
		{
			foreach (var (key, downloadedModfileId) in tuple2.Item2)
			{
				if (output.Index.TryGetValue(key, out var value2))
				{
					value2.DownloadedModfileId = downloadedModfileId;
					continue;
				}
				output.Index[key] = new IndexEntry
				{
					DownloadedModfileId = downloadedModfileId,
					FileState = ModFileState.Downloaded
				};
			}
		}
		(Error, List<(long, long)>) tuple4 = await ModioClient.DataStorage.ScanForInstalledMods();
		if (!tuple4.Item1)
		{
			foreach (var (key2, installedModfileId) in tuple4.Item2)
			{
				if (output.Index.TryGetValue(key2, out var value3))
				{
					value3.InstalledModfileId = installedModfileId;
					continue;
				}
				output.Index[key2] = new IndexEntry
				{
					InstalledModfileId = installedModfileId,
					FileState = ModFileState.Installed
				};
			}
		}
		(Error, ICollection<Mod>) obj = await Mod.GetMods(output.Index.Keys, forceRefresh: false, output);
		IEnumerable<Mod> item2 = obj.Item2;
		Error item3 = obj.Item1;
		IEnumerable<Mod> enumerable = item2;
		if (!item3)
		{
			foreach (Mod item4 in enumerable)
			{
				output.ModObjectCache[item4.Id] = item4.LastModObject;
			}
		}
		return (error: Error.None, result: output);
	}

	internal async Task<bool> UpdateIndexWithMissingEntriesFromScan()
	{
		(Error, List<(long, long)>) tuple = await ModioClient.DataStorage.ScanForInstalledMods();
		List<long> list = null;
		if (!tuple.Item1)
		{
			foreach (var (num, installedModfileId) in tuple.Item2)
			{
				if (Index.TryGetValue(num, out var value))
				{
					if (value.InstalledModfileId == 0L)
					{
						value.InstalledModfileId = installedModfileId;
						value.FileState = ModFileState.Installed;
						IsDirty = true;
					}
					continue;
				}
				Index[num] = new IndexEntry
				{
					InstalledModfileId = installedModfileId,
					FileState = ModFileState.Installed
				};
				if (list == null)
				{
					list = new List<long>();
				}
				list.Add(num);
			}
		}
		if (list == null)
		{
			return false;
		}
		(Error, ICollection<Mod>) obj = await Mod.GetMods(list);
		IEnumerable<Mod> item = obj.Item2;
		Error item2 = obj.Item1;
		IEnumerable<Mod> enumerable = item;
		if (!item2)
		{
			foreach (Mod item3 in enumerable)
			{
				ModObjectCache[item3.Id] = item3.LastModObject;
			}
		}
		IsDirty = true;
		return true;
	}

	internal async Task<(bool modIsInstalled, long installedModFileId)> UpdateIndexForMod(Mod mod)
	{
		(Error, bool, long) tuple = await ModioClient.DataStorage.ScanForInstalledMod(mod);
		if ((bool)tuple.Item1 || !tuple.Item2)
		{
			return (modIsInstalled: false, installedModFileId: -1L);
		}
		if (Index.TryGetValue(mod.Id, out var value))
		{
			value.InstalledModfileId = tuple.Item3;
			value.FileState = ((mod.File.Id != tuple.Item3) ? ModFileState.Queued : ModFileState.Installed);
			IsDirty = true;
		}
		else
		{
			Index[mod.Id] = new IndexEntry
			{
				InstalledModfileId = tuple.Item3,
				FileState = ((mod.File.Id != tuple.Item3) ? ModFileState.Queued : ModFileState.Installed)
			};
		}
		ModObjectCache[mod.Id] = mod.LastModObject;
		IsDirty = true;
		return (modIsInstalled: true, installedModFileId: tuple.Item3);
	}

	internal IndexEntry GetEntry(Mod mod)
	{
		if (Index.TryGetValue(mod.Id, out var value))
		{
			return value;
		}
		value = new IndexEntry();
		Index[mod.Id] = value;
		ModObjectCache[mod.Id] = mod.LastModObject;
		mod.Logo?.CacheLowestResolutionOnDisk(shouldCache: true);
		return value;
	}

	internal bool TryGetEntry(ModId modId, out IndexEntry entry)
	{
		return Index.TryGetValue(modId, out entry);
	}

	internal void RemoveEntry(Mod mod)
	{
		Index.Remove(mod.Id);
		ModObjectCache.Remove(mod.Id);
		mod.Logo?.CacheLowestResolutionOnDisk(shouldCache: false);
	}

	internal void Shutdown()
	{
		Mod.RemoveChangeListener(ModChangeType.ModObject, OnModObjectUpdate);
	}

	private void OnModObjectUpdate(Mod mod, ModChangeType changeType)
	{
		if (TryGetEntry(mod.Id, out var _))
		{
			ModObjectCache[mod.Id] = mod.LastModObject;
			mod.Logo?.CacheLowestResolutionOnDisk(shouldCache: true);
		}
	}

	internal async Task RefreshIndexModObjects(bool installedOnly = true)
	{
		List<long> list = new List<long>();
		if (installedOnly)
		{
			foreach (KeyValuePair<long, IndexEntry> item3 in Index)
			{
				if (item3.Value.InstalledModfileId > 0)
				{
					list.Add(item3.Key);
				}
			}
		}
		else
		{
			list.AddRange(Index.Keys);
		}
		(Error, ICollection<Mod>) obj = await Mod.GetMods(list, forceRefresh: true);
		IEnumerable<Mod> item = obj.Item2;
		Error item2 = obj.Item1;
		IEnumerable<Mod> enumerable = item;
		if ((bool)item2)
		{
			return;
		}
		foreach (Mod item4 in enumerable)
		{
			ModObjectCache[item4.Id] = item4.LastModObject;
		}
	}
}
