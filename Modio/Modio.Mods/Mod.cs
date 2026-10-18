using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Caching;
using Modio.Errors;
using Modio.Extensions;
using Modio.Images;
using Modio.Mods.Builder;
using Modio.Reports;
using Modio.Users;
using UnityEngine;

namespace Modio.Mods;

public class Mod
{
	public enum LogoResolution
	{
		X320_Y180,
		X640_Y360,
		X1280_Y720,
		Original
	}

	public enum GalleryResolution
	{
		X320_Y180,
		X1280_Y720,
		Original
	}

	private static readonly Dictionary<ModChangeType, Action<Mod, ModChangeType>> ChangeSubscribers = new Dictionary<ModChangeType, Action<Mod, ModChangeType>>();

	private string _summaryDecoded;

	private string _summaryHtmlEncoded;

	public ModId Id { get; }

	public string Name { get; private set; }

	public string Summary => _summaryDecoded ?? (_summaryDecoded = WebUtility.HtmlDecode(_summaryHtmlEncoded));

	public string Description { get; private set; }

	public DateTime DateLive { get; private set; }

	public DateTime DateUpdated { get; private set; }

	public ModTag[] Tags { get; private set; }

	public string MetadataBlob { get; private set; }

	public Dictionary<string, string> MetadataKvps { get; private set; }

	public ModCommunityOptions CommunityOptions { get; private set; }

	public ModMaturityOptions MaturityOptions { get; private set; }

	public Modfile File { get; private set; }

	public ModStats Stats { get; private set; }

	public long Price { get; private set; }

	public bool IsMonetized { get; private set; }

	public ModioImageSource<LogoResolution> Logo { get; private set; }

	public ModioImageSource<GalleryResolution>[] Gallery { get; private set; }

	public UserProfile Creator { get; private set; }

	public ModDependencies Dependencies { get; private set; }

	public ModRating CurrentUserRating { get; private set; }

	public bool IsSubscribed { get; private set; }

	public bool IsPurchased { get; private set; }

	public bool IsEnabled { get; internal set; } = true;


	internal ModObject LastModObject { get; private set; }

	public event Action OnModUpdated;

	public static void AddChangeListener(ModChangeType subscribedChange, Action<Mod, ModChangeType> listener)
	{
		if (ChangeSubscribers.TryGetValue(subscribedChange, out var value))
		{
			listener = (Action<Mod, ModChangeType>)Delegate.Combine(value, listener);
		}
		ChangeSubscribers[subscribedChange] = listener;
	}

	public static void RemoveChangeListener(ModChangeType subscribedChange, Action<Mod, ModChangeType> listener)
	{
		if (ChangeSubscribers.TryGetValue(subscribedChange, out var _))
		{
			Dictionary<ModChangeType, Action<Mod, ModChangeType>> changeSubscribers = ChangeSubscribers;
			changeSubscribers[subscribedChange] = (Action<Mod, ModChangeType>)Delegate.Remove(changeSubscribers[subscribedChange], listener);
		}
	}

	public static ModBuilder Create()
	{
		return new ModBuilder();
	}

	public ModBuilder Edit()
	{
		return new ModBuilder(this);
	}

	internal Mod(ModId id)
	{
		Id = id;
	}

	internal Mod(ModObject modObject)
	{
		Id = modObject.Id;
		Debug.Log($"[mod.io] creating mod from modObject, ID:{Id}");
		ApplyDetailsFromModObject(modObject);
	}

	public static Mod Get(long id)
	{
		return ModCache.GetMod(new ModId(id));
	}

	internal Mod ApplyDetailsFromModObject(ModObject modObject)
	{
		LastModObject = modObject;
		Name = WebUtility.HtmlDecode(modObject.Name);
		_summaryHtmlEncoded = modObject.Summary;
		_summaryDecoded = null;
		Description = modObject.DescriptionPlaintext;
		if (File == null)
		{
			File = new Modfile(modObject.Modfile);
		}
		else if (modObject.Modfile.Id != 0L)
		{
			File.ApplyDetailsFromModfileObject(modObject.Modfile);
		}
		if (modObject.SubmittedBy.Id > 0)
		{
			Creator = UserProfile.Get(modObject.SubmittedBy);
			DateLive = modObject.DateLive.GetUtcDateTime();
			DateUpdated = modObject.DateUpdated.GetUtcDateTime();
			Tags = modObject.Tags.Select(ModTag.Get).ToArray();
			MetadataBlob = modObject.MetadataBlob;
			MetadataKvps = modObject.MetadataKvp.ToDictionary((MetadataKvpObject kvp) => kvp.Metakey, (MetadataKvpObject kvp) => kvp.Metavalue);
			CommunityOptions = (ModCommunityOptions)modObject.CommunityOptions;
			MaturityOptions = (ModMaturityOptions)modObject.MaturityOption;
			Stats = new ModStats(modObject.Stats, CurrentUserRating);
			IsMonetized = ((uint)(int)modObject.MonetizationOptions & (true ? 1u : 0u)) != 0 && ((int)modObject.MonetizationOptions & 2) != 0;
			Price = modObject.Price;
			Logo = new ModioImageSource<LogoResolution>(modObject.Logo.Filename, modObject.Logo.Thumb320X180, modObject.Logo.Thumb640X360, modObject.Logo.Thumb1280X720, modObject.Logo.Original);
			Gallery = modObject.Media.Images.Select((ImageObject imageObject) => new ModioImageSource<GalleryResolution>(imageObject.Filename, imageObject.Thumb320X180, imageObject.Thumb1280X720, imageObject.Original)).ToArray();
			Dependencies = new ModDependencies(this, modObject.Dependencies);
		}
		else
		{
			Creator = null;
			DateLive = 0L.GetUtcDateTime();
			DateUpdated = 0L.GetUtcDateTime();
			Tags = Array.Empty<ModTag>();
			MetadataBlob = "";
			MetadataKvps = null;
			CommunityOptions = ModCommunityOptions.None;
			MaturityOptions = ModMaturityOptions.None;
			Stats = null;
			IsMonetized = false;
			Price = 0L;
			Logo = null;
			Gallery = null;
		}
		InvokeModUpdated(ModChangeType.Everything);
		return this;
	}

	public Task<Error> Subscribe(bool includeDependencies = true)
	{
		return SetSubscribed(subscribed: true, includeDependencies);
	}

	public Task<Error> Unsubscribe()
	{
		return SetSubscribed(subscribed: false);
	}

	private async Task<Error> SetSubscribed(bool subscribed, bool includeDependencies = true)
	{
		if (IsSubscribed == subscribed)
		{
			return Error.None;
		}
		UpdateLocalSubscriptionStatus(subscribed);
		Error error2;
		if (subscribed)
		{
			ModObject? modObject;
			(error2, modObject) = await ModioAPI.Subscribe.SubscribeToMod(Id, new AddModSubscriptionRequest(includeDependencies));
			if (modObject.HasValue)
			{
				ApplyDetailsFromModObject(modObject.Value);
			}
		}
		else
		{
			error2 = (await ModioAPI.Subscribe.UnsubscribeFromMod(Id)).Item1;
		}
		if (subscribed && (bool)error2)
		{
			UpdateLocalSubscriptionStatus(isSubscribed: false);
			return error2;
		}
		if (subscribed && !(await ModInstallationManagement.IsThereAvailableSpaceFor(this)))
		{
			error2 = new FilesystemError(FilesystemErrorCode.INSUFFICIENT_SPACE);
			File.FileStateErrorCause = error2;
			File.State = ModFileState.FileOperationFailed;
			InvokeModUpdated(ModChangeType.FileState);
			return error2;
		}
		if (!subscribed && (bool)error2 && error2.Code != ErrorCode.CANNOT_OPEN_CONNECTION)
		{
			UpdateLocalSubscriptionStatus(isSubscribed: true);
			return error2;
		}
		if (subscribed && includeDependencies)
		{
			(Error, IReadOnlyList<Mod>) obj = await Dependencies.GetAllDependencies();
			IEnumerable<Mod> item = obj.Item2;
			Error item2 = obj.Item1;
			IEnumerable<Mod> enumerable = item;
			if (enumerable != null && !item2)
			{
				foreach (Mod item3 in enumerable)
				{
					item3.UpdateLocalSubscriptionStatus(isSubscribed: true);
				}
			}
		}
		InvokeModUpdated(ModChangeType.IsSubscribed);
		ModInstallationManagement.WakeUp();
		return error2;
	}

	public void SetIsEnabled(bool isEnabled)
	{
		UpdateLocalEnabledStatus(isEnabled);
	}

	internal void UpdateLocalSubscriptionStatus(bool isSubscribed)
	{
		if (IsSubscribed != isSubscribed)
		{
			IsSubscribed = isSubscribed;
			InvokeModUpdated(ModChangeType.IsSubscribed);
		}
	}

	internal void UpdateLocalEnabledStatus(bool isEnabled)
	{
		IsEnabled = isEnabled;
		InvokeModUpdated(ModChangeType.IsEnabled);
	}

	internal void UpdateLocalPurchaseStatus(bool isPurchased)
	{
		IsPurchased = isPurchased;
		InvokeModUpdated(ModChangeType.IsPurchased);
	}

	public static Task<(Error error, ModioPage<Mod> page)> GetMods(ModSearchFilter filter)
	{
		return GetMods(filter.GetModsFilter());
	}

	public static async Task<(Error error, ModioPage<Mod> page)> GetMods(ModioAPI.Mods.GetModsFilter filter, bool forceRefresh = false)
	{
		string searchCacheKey = ModCache.ConstructFilterKey(filter);
		Mod[] cachedMods;
		long resultTotal;
		if (forceRefresh)
		{
			ModCache.RemoveCachedModSearch(searchCacheKey);
		}
		else if (ModCache.GetCachedModSearch(filter, searchCacheKey, out cachedMods, out resultTotal))
		{
			ModioPage<Mod> item = new ModioPage<Mod>(cachedMods, filter.PageSize, filter.PageIndex, resultTotal);
			return (error: Error.None, page: item);
		}
		var (error, pagination) = await ModioAPI.Mods.GetMods(filter);
		if ((bool)error)
		{
			return (error: error, page: null);
		}
		int num = pagination.Value.Data.Length;
		Mod[] array = ((num == 0) ? Array.Empty<Mod>() : new Mod[num]);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ModCache.GetMod(pagination.Value.Data[i]);
		}
		long resultLimit = pagination.Value.ResultLimit;
		long pageIndex = pagination.Value.ResultOffset / resultLimit;
		ModCache.CacheModSearch(searchCacheKey, array, pageIndex, pagination.Value.ResultTotal);
		ModioPage<Mod> item2 = new ModioPage<Mod>(array, (int)resultLimit, pageIndex, pagination.Value.ResultTotal);
		return (error: Error.None, page: item2);
	}

	public async Task<(Error error, Mod result)> GetModDetailsFromServer()
	{
		var (error, modObject) = await ModioAPI.Mods.GetMod(Id);
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error getting Mod {Id}: {error.GetMessage()}");
			}
			return (error: error, result: this);
		}
		ApplyDetailsFromModObject(modObject.Value);
		return (error: error, result: this);
	}

	public static async Task<(Error error, Mod result)> GetMod(ModId modId, bool forceRefresh = false, ModIndex tempIndex = null, bool deferModInstallManagementRefresh = false)
	{
		if (!modId.IsValid())
		{
			return (error: new Error(ErrorCode.BAD_PARAMETER), result: null);
		}
		Mod mod = null;
		if (!forceRefresh && ModCache.TryGetMod(modId, out mod))
		{
			return (error: Error.None, result: mod);
		}
		var (error, modObject) = await ModioAPI.Mods.GetMod(modId);
		if ((bool)error)
		{
			if (error.Code == ErrorCode.REQUESTED_MOD_INACCESSIBLE)
			{
				ModioLog.Message?.Log($"Mod {modId} is inaccessible, using hidden mod placeholder...");
				mod = ModCache.CreateHiddenModFromCachedIndexData(modId, tempIndex);
				return (error: Error.None, result: mod);
			}
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error getting Mod {modId}: {error.GetMessage()}");
			}
			return (error: error, result: null);
		}
		mod = ModCache.GetMod(modObject.Value);
		if (ModInstallationManagement.IsInitialized && forceRefresh && !deferModInstallManagementRefresh)
		{
			ModInstallationManagement.RefreshMod(mod);
		}
		return (error: error, result: mod);
	}

	public static async Task<(Error error, ICollection<Mod>)> GetMods(ICollection<long> neededModIds, bool forceRefresh = false, ModIndex tempIndex = null)
	{
		if (neededModIds.Count <= 0)
		{
			return (error: Error.None, Array.Empty<Mod>());
		}
		List<Mod> output = new List<Mod>(neededModIds.Count);
		List<long> list = new List<long>();
		if (forceRefresh)
		{
			list.AddRange(neededModIds);
		}
		else
		{
			foreach (long neededModId in neededModIds)
			{
				if (ModCache.TryGetMod(neededModId, out var mod) && mod.File != null)
				{
					output.Add(mod);
				}
				else
				{
					list.Add(neededModId);
				}
			}
		}
		if (list.Count == 0)
		{
			return (error: Error.None, output);
		}
		ModioAPI.Mods.GetModsFilter filter = ModioAPI.Mods.FilterGetMods();
		filter.Id(list, Filtering.In);
		filter.RevenueType(2L);
		while (true)
		{
			var (error, modioPage) = await GetMods(filter, forceRefresh);
			if ((bool)error)
			{
				if (!error.IsSilent)
				{
					ModioLog.Error?.Log("Error getting Mods to populate cache from Index: " + error.GetMessage());
				}
				return (error: error, Array.Empty<Mod>());
			}
			output.AddRange(modioPage.Data);
			if (!modioPage.HasMoreResults())
			{
				break;
			}
			filter.PageIndex++;
		}
		List<Mod> modInstallManagementRefreshList = new List<Mod>();
		foreach (long modId in neededModIds)
		{
			bool flag = false;
			foreach (Mod item2 in output)
			{
				if (item2.Id == modId)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				Error error2 = Error.None;
				Mod item = null;
				if (User.Current.IsAuthenticated)
				{
					ModioLog.Message?.Log($"GetMods couldn't retrieve mod {modId}, attempting individual retrieval...");
					(error2, item) = await GetMod(modId, forceRefresh, tempIndex, deferModInstallManagementRefresh: true);
				}
				if (!User.Current.IsAuthenticated || (bool)error2)
				{
					item = ModCache.CreateHiddenModFromCachedIndexData(modId);
					output.Add(item);
				}
				else
				{
					output.Add(item);
				}
				modInstallManagementRefreshList.Add(item);
			}
		}
		if (ModInstallationManagement.IsInitialized)
		{
			ModInstallationManagement.RefreshMods(modInstallManagementRefreshList);
		}
		return (error: Error.None, output);
	}

	public async Task<Error> RateMod(ModRating rating)
	{
		ModRating previousRating = CurrentUserRating;
		UpdateStatsWithUserRating(rating);
		InvokeModUpdated(ModChangeType.Rating);
		Error item = (await ModioAPI.Ratings.AddModRating(body: new AddRatingRequest((long)rating), modId: Id)).Item1;
		if ((bool)item)
		{
			if (!item.IsSilent)
			{
				ModioLog.Warning?.Log($"Error rating mod {Id}: {item.GetMessage()}");
			}
			UpdateStatsWithUserRating(previousRating);
			InvokeModUpdated(ModChangeType.Rating);
		}
		return item;
		void UpdateStatsWithUserRating(ModRating userRating)
		{
			Stats.UpdateEstimateFromLocalRatingChange(userRating);
			CurrentUserRating = userRating;
			InvokeModUpdated(ModChangeType.Rating);
		}
	}

	internal void SetCurrentUserRating(ModRating rating)
	{
		CurrentUserRating = rating;
		Stats?.UpdatePreviousRating(rating);
		InvokeModUpdated(ModChangeType.Rating);
	}

	public async Task<Error> Report(ReportType reportType, ModNotWorkingReason reportReason, string contact, string summary)
	{
		if (User.Current == null || !User.Current.IsAuthenticated)
		{
			return (Error)ErrorCode.USER_NOT_AUTHENTICATED;
		}
		return (await ModioAPI.Reports.SubmitReport(new AddReportRequest("mods", Id, (long)reportType, (long)reportReason, null, User.Current.Profile.Username, contact, summary))).Item1;
	}

	internal void InvokeModUpdated(ModChangeType changeFlags)
	{
		foreach (var (modChangeType2, action2) in ChangeSubscribers)
		{
			if ((modChangeType2 & changeFlags) != 0)
			{
				action2?.Invoke(this, changeFlags);
			}
		}
		this.OnModUpdated?.Invoke();
	}

	internal void UpdateModfile(Modfile modfile)
	{
		File = modfile;
		InvokeModUpdated(ModChangeType.Modfile);
	}

	public async Task<Error> Purchase(bool subscribeOnPurchase)
	{
		string idempotent_key = $"{Id}";
		var (error, payObject) = await ModioAPI.Monetization.Purchase(Id, new PayRequest(Price, subscribeOnPurchase, idempotent_key));
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error purchasing mod {Id}: {error}");
			}
			return error;
		}
		IsPurchased = true;
		User.Current.ApplyWalletFromPurchase(payObject.Value);
		if (!subscribeOnPurchase)
		{
			InvokeModUpdated(ModChangeType.IsPurchased);
			return Error.None;
		}
		IsSubscribed = true;
		ApplyDetailsFromModObject(new ModObject?(payObject.Value.Mod).Value);
		InvokeModUpdated(ModChangeType.IsSubscribed | ModChangeType.IsPurchased);
		ModInstallationManagement.WakeUp();
		return Error.None;
	}

	public void UninstallOtherUserMod(bool force = false)
	{
		if (!force && IsSubscribed)
		{
			ModioLog.Warning?.Log($"Attempting to uninstall mod {Id}, but its subscribed. Cancelling.");
		}
		else
		{
			ModInstallationManagement.MarkModForUninstallation(this);
		}
	}

	public override string ToString()
	{
		return $"Mod({Id}:{Name})";
	}

	public bool IsHidden()
	{
		if (LastModObject.Status != 0L)
		{
			return LastModObject.Visible == 0;
		}
		return true;
	}

	public static async Task<Error> RefreshPotentiallyHiddenCachedMods()
	{
		return await ModCache.RefreshPotentiallyHiddenCachedMods();
	}
}
