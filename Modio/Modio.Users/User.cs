using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Modio.API;
using Modio.API.SchemaDefinitions;
using Modio.Authentication;
using Modio.Caching;
using Modio.Errors;
using Modio.Extensions;
using Modio.FileIO;
using Modio.Mods;
using Modio.Monetization;

namespace Modio.Users;

public class User
{
	private readonly Authentication _authentication;

	private bool _isWritingToDisk;

	private bool _needsSavingToDisk;

	public static User Current { get; private set; }

	public string LocalUserId { get; private set; }

	public long UserId => Profile.UserId;

	public bool IsInitialized { get; private set; }

	public bool HasAcceptedTermsOfUse { get; private set; }

	public bool IsAuthenticated { get; private set; }

	public bool IsUpdating { get; private set; }

	public UserProfile Profile { get; private set; }

	public Wallet Wallet { get; private set; }

	public ModRepository ModRepository { get; private set; }

	public string Token => _authentication.OAuthToken;

	public static event Action<User> OnUserChanged;

	public static event Action OnUserSyncComplete;

	public static async Task InitializeNewUser()
	{
		ModioLog.Verbose?.Log("Initializing New User");
		Current = new User();
		Current.IsUpdating = true;
		User current = Current;
		current.LocalUserId = await ModioServices.Resolve<IGetActiveUserIdentifier>().GetActiveUserIdentifier();
		_ = Error.None;
		UserSaveObject userObject;
		Error error;
		(error, userObject) = await ModioClient.DataStorage.ReadUserData(Current.LocalUserId);
		if ((bool)error)
		{
			ModioLog.Verbose?.Log($"Failed to find UserSaveObject, checking for legacy user data... {error}");
			LegacyUserSaveObject userSaveObject;
			(error, userSaveObject) = await ModioClient.DataStorage.ReadLegacyUserData(Current.LocalUserId);
			if (!error)
			{
				Current.ApplyDetailsFromLegacySaveObject(userSaveObject);
				Current.IsAuthenticated = true;
				Current.HasAcceptedTermsOfUse = true;
				await Current.Sync();
				ModioClient.DataStorage.DeleteLegacyUserData();
			}
			else
			{
				Current.IsUpdating = false;
			}
		}
		else
		{
			Current.ApplyDetailsFromSaveObject(userObject);
			Current.IsAuthenticated = true;
			Current.HasAcceptedTermsOfUse = true;
			await Current.Sync();
		}
	}

	private User()
	{
		_authentication = new Authentication();
		Wallet = new Wallet();
		ModRepository = new ModRepository();
		Profile = new UserProfile();
		IsUpdating = false;
		IsAuthenticated = false;
		HasAcceptedTermsOfUse = false;
		IsInitialized = true;
	}

	private void ApplyDetailsFromSaveObject(UserSaveObject userObject)
	{
		Profile.Username = userObject.Username;
		Profile.UserId = userObject.UserId;
		if (userObject.SubscribedMods != null)
		{
			foreach (long subscribedMod in userObject.SubscribedMods)
			{
				Mod.Get(subscribedMod).UpdateLocalSubscriptionStatus(isSubscribed: true);
			}
		}
		if (userObject.DisabledMods != null)
		{
			foreach (long disabledMod in userObject.DisabledMods)
			{
				Mod.Get(disabledMod).UpdateLocalEnabledStatus(isEnabled: false);
			}
		}
		if (userObject.PurchasedMods != null)
		{
			foreach (long purchasedMod in userObject.PurchasedMods)
			{
				Mod.Get(purchasedMod).UpdateLocalPurchaseStatus(isPurchased: true);
			}
		}
		_authentication.OAuthToken = userObject.AuthToken;
		User.OnUserChanged?.Invoke(this);
	}

	private void ApplyDetailsFromLegacySaveObject(LegacyUserSaveObject userSaveObject)
	{
		Profile.Username = userSaveObject.userObject.username;
		Profile.UserId = userSaveObject.userObject.id;
		_authentication.OAuthToken = userSaveObject.oAuthToken;
		User.OnUserChanged?.Invoke(this);
	}

	internal void OnAcceptedTermsOfUse()
	{
		HasAcceptedTermsOfUse = true;
	}

	public void OnAuthenticated(string oAuthToken)
	{
		_authentication.OAuthToken = oAuthToken;
		HasAcceptedTermsOfUse = true;
		IsAuthenticated = true;
		Sync().ForgetTaskSafely();
	}

	internal string GetAuthToken()
	{
		return _authentication.OAuthToken;
	}

	public async Task<Error> Sync()
	{
		ModRepository.OnContentsChanged -= OnAnyModRepositoryChange;
		IsUpdating = true;
		Task<Error> task = SyncProfile();
		Task<Error> task2 = SyncSubscriptions();
		Task<Error> task3 = SyncPurchases();
		SyncRatings().ForgetTaskSafely();
		SyncWallet().ForgetTaskSafely();
		SyncEntitlements().ForgetTaskSafely();
		Error[] source = await Task.WhenAll<Error>(task, task2, task3);
		IsUpdating = false;
		ModRepository.OnContentsChanged += OnAnyModRepositoryChange;
		if (source.Any((Error error) => error))
		{
			return source.First((Error error) => error);
		}
		await SaveUserData();
		User.OnUserSyncComplete?.Invoke();
		return Error.None;
	}

	private void OnAnyModRepositoryChange()
	{
		SaveUserData().ForgetTaskSafely();
	}

	public async Task<Error> SyncProfile()
	{
		ModioLog.Verbose?.Log($"Syncing Profile {UserId}");
		var (error, userObject) = await ModioAPI.Me.GetAuthenticatedUser();
		if (error.Code == ErrorCode.EXPIRED_OR_REVOKED_ACCESS_TOKEN || error.Code == ErrorCode.HTTP_EXCEPTION)
		{
			IsUpdating = false;
			IsAuthenticated = false;
			_authentication.OAuthToken = null;
			await SaveUserData();
			return error;
		}
		if (!error)
		{
			Profile.ApplyDetailsFromUserObject(userObject.Value);
		}
		else if (!error.IsSilent)
		{
			ModioLog.Error?.Log($"Error syncing User {UserId} profile details: {error}");
		}
		IsAuthenticated = true;
		HasAcceptedTermsOfUse = true;
		User.OnUserChanged?.Invoke(this);
		ModioLog.Verbose?.Log($"Finished Syncing Profile {UserId} with result: {error}");
		return error;
	}

	public async Task<Error> SyncSubscriptions()
	{
		ModioLog.Verbose?.Log($"Syncing Subscriptions {UserId}");
		var (error, source) = await CrawlAllPages(ModioAPI.Me.FilterGetUserSubscriptions().GameId(ModioClient.Settings.GameId), ModioAPI.Me.GetUserSubscriptions);
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error syncing subscriptions for {UserId}: {error}");
			}
			return error;
		}
		List<Mod> list = source.Select(ModCache.GetMod).ToList();
		HashSet<Mod> hashSet = new HashSet<Mod>(ModRepository.GetSubscribed());
		foreach (Mod item in list)
		{
			hashSet.Remove(item);
			item.UpdateLocalSubscriptionStatus(isSubscribed: true);
		}
		foreach (Mod item2 in hashSet)
		{
			item2.UpdateLocalSubscriptionStatus(isSubscribed: false);
		}
		ModRepository.HasGotSubscriptions = true;
		User.OnUserChanged?.Invoke(this);
		ModioLog.Verbose?.Log($"Finished Syncing Subscriptions for {LocalUserId} with result: {error}");
		ModInstallationManagement.WakeUp();
		return Error.None;
	}

	public async Task<Error> SyncPurchases()
	{
		ModioLog.Verbose?.Log($"Syncing Purchases for {UserId}");
		ModioSettings modioSettings = ModioServices.Resolve<ModioSettings>();
		if (!modioSettings.TryGetPlatformSettings<MonetizationSettings>(out var _))
		{
			ModioLog.Message?.Log($"No {typeof(MonetizationSettings)} settings found, skipping SyncPurchases");
			return Error.None;
		}
		var (error, source) = await CrawlAllPages(ModioAPI.Me.FilterGetUserPurchases().GameId(modioSettings.GameId), ModioAPI.Me.GetUserPurchases);
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error syncing purchases for {UserId}: {error}");
			}
			return error;
		}
		List<Mod> list = source.Where((ModObject modObject) => modObject.Visible != 0).Select(ModCache.GetMod).ToList();
		HashSet<Mod> hashSet = new HashSet<Mod>(ModRepository.GetPurchased());
		foreach (Mod item in list)
		{
			hashSet.Remove(item);
			item.UpdateLocalPurchaseStatus(isPurchased: true);
		}
		foreach (Mod item2 in hashSet)
		{
			item2.UpdateLocalPurchaseStatus(isPurchased: false);
		}
		User.OnUserChanged?.Invoke(this);
		ModioLog.Verbose?.Log($"Finished Syncing Purchases for {LocalUserId} with result: {error}");
		ModInstallationManagement.WakeUp();
		return Error.None;
	}

	public async Task<Error> SyncEntitlements()
	{
		ModioLog.Verbose?.Log("Syncing Entitlements " + LocalUserId);
		if (!ModioServices.Resolve<ModioSettings>().TryGetPlatformSettings<MonetizationSettings>(out var _))
		{
			ModioLog.Message?.Log($"No {typeof(MonetizationSettings)} settings found, skipping SyncEntitlements");
			return Error.None;
		}
		if (!ModioServices.TryResolve<IModioEntitlementService>(out var result))
		{
			return Error.None;
		}
		Error error = await result.SyncEntitlements();
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error syncing Entitlements for {UserId}: {error}");
			}
			return error;
		}
		ModioLog.Verbose?.Log($"Finished Syncing Entitlements {LocalUserId} with result: {Error.None}");
		await SyncWallet();
		return Error.None;
	}

	public async Task<Error> SyncWallet()
	{
		ModioLog.Verbose?.Log("Syncing Wallet " + LocalUserId);
		if (!ModioServices.Resolve<ModioSettings>().TryGetPlatformSettings<MonetizationSettings>(out var _))
		{
			ModioLog.Message?.Log($"No {typeof(MonetizationSettings)} settings found, skipping SyncWallet");
			return Error.None;
		}
		var (error, walletObject) = await ModioAPI.Me.GetUserWallet(ModioClient.Settings.GameId);
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error syncing Wallet for {UserId}: {error}");
			}
			return error;
		}
		Wallet.ApplyDetailsFromWalletObject(walletObject.Value);
		User.OnUserChanged?.Invoke(this);
		ModioLog.Verbose?.Log($"Finished Syncing Wallet {LocalUserId} with result: {error}");
		return Error.None;
	}

	internal void ApplyWalletFromPurchase(PayObject payObject)
	{
		Wallet.UpdateBalance(payObject.Balance);
		User.OnUserChanged?.Invoke(this);
	}

	public async Task<Error> SyncRatings()
	{
		ModioLog.Verbose?.Log("Syncing Ratings " + LocalUserId);
		ModioSettings modioSettings = ModioServices.Resolve<ModioSettings>();
		ModioAPI.Me.GetUserRatingsFilter getUserRatingsFilter = ModioAPI.Me.FilterGetUserRatings();
		getUserRatingsFilter.GameId(modioSettings.GameId);
		var (error, list) = await CrawlAllPages(getUserRatingsFilter, ModioAPI.Me.GetUserRatings);
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error syncing Ratings for {UserId}: {error}");
			}
			return error;
		}
		foreach (RatingObject item in list)
		{
			if (ModCache.TryGetMod(item.ModId, out var mod))
			{
				mod.SetCurrentUserRating((ModRating)item.Rating);
			}
		}
		ModioLog.Verbose?.Log($"Finished Syncing Ratings for {LocalUserId} with result: {error}");
		return Error.None;
	}

	public async Task<(Error error, IReadOnlyList<UserProfile> results)> GetMutedUsers()
	{
		var (error, pagination) = await ModioAPI.Me.GetUsersMuted();
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error getting muted users for user {Profile.Username}: {error}");
			}
			return (error: error, results: Array.Empty<UserProfile>());
		}
		List<UserProfile> item = pagination.Value.Data.Select(UserProfile.Get).ToList();
		return (error: Error.None, results: item);
	}

	public async Task<(Error error, IReadOnlyList<Mod> mods)> GetUserCreations(bool filterForGame = false)
	{
		ModioAPI.Me.GetUserModsFilter getUserModsFilter = ModioAPI.Me.FilterGetUserMods();
		if (filterForGame)
		{
			getUserModsFilter.GameId(ModioClient.Settings.GameId);
		}
		var (error, source) = await CrawlAllPages(getUserModsFilter, ModioAPI.Me.GetUserMods);
		if ((bool)error)
		{
			if (!error.IsSilent)
			{
				ModioLog.Error?.Log($"Error getting user creations for {UserId}: {error}");
			}
			return (error: error, mods: Array.Empty<Mod>());
		}
		List<Mod> item = source.Select(ModCache.GetMod).ToList();
		return (error: Error.None, mods: item);
	}

	[ModioDebugMenu(ShowInBrowserMenu = false, ShowInSettingsMenu = true)]
	public static void DeleteUserData()
	{
		ModioServices.Resolve<IModioDataStorage>().DeleteUserData(Current.LocalUserId).ForgetTaskSafely();
		LogOut();
	}

	public static void LogOut()
	{
		ModioLog.Verbose?.Log("Logging out " + Current?.LocalUserId);
		ModInstallationManagement.NotifyLoggingOut();
		Current?.ModRepository.Dispose();
		Current = new User();
		User.OnUserChanged?.Invoke(Current);
	}

	private static async Task<(Error error, List<T> results)> CrawlAllPages<F, T>(F filter, Func<F, Task<(Error error, Pagination<T[]>?)>> method) where F : SearchFilter<F>
	{
		List<T> output = new List<T>();
		while (true)
		{
			var (error, pagination) = await method(filter);
			if ((bool)error)
			{
				if (!error.IsSilent)
				{
					ModioLog.Warning?.Log($"Error crawling pages for user {Current.UserId}: {error.GetMessage()}");
				}
				return (error: error, results: new List<T>());
			}
			output.AddRange(pagination.Value.Data);
			if (pagination.Value.ResultOffset + pagination.Value.ResultCount >= pagination.Value.ResultTotal)
			{
				break;
			}
			filter.PageIndex++;
		}
		return (error: Error.None, results: output);
	}

	private async Task SaveUserData()
	{
		if (_isWritingToDisk)
		{
			_needsSavingToDisk = true;
			return;
		}
		_isWritingToDisk = true;
		_needsSavingToDisk = false;
		Error error = await ModioClient.DataStorage.WriteUserData(GetWritable());
		if ((bool)error)
		{
			ModioLog.Message?.Log("Error writing user data to disk: " + error.GetMessage());
		}
		_isWritingToDisk = false;
		if (_needsSavingToDisk)
		{
			await SaveUserData();
		}
	}

	private UserSaveObject GetWritable()
	{
		return new UserSaveObject
		{
			LocalUserId = LocalUserId,
			Username = Profile.Username,
			UserId = Profile.UserId,
			AuthToken = _authentication.OAuthToken,
			SubscribedMods = ModRepository.GetSubscribed().Select((Func<Mod, long>)((Mod mod) => mod.Id)).ToList(),
			DisabledMods = ModRepository.GetDisabled().Select((Func<Mod, long>)((Mod mod) => mod.Id)).ToList(),
			PurchasedMods = ModRepository.GetPurchased().Select((Func<Mod, long>)((Mod mod) => mod.Id)).ToList()
		};
	}
}
