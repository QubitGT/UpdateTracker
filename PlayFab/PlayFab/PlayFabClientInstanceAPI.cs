using System;
using System.Collections.Generic;
using PlayFab.ClientModels;
using PlayFab.Internal;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabClientInstanceAPI : IPlayFabInstanceApi
{
	public readonly PlayFabApiSettings apiSettings;

	public readonly PlayFabAuthenticationContext authenticationContext;

	public PlayFabClientInstanceAPI()
	{
		authenticationContext = new PlayFabAuthenticationContext();
	}

	public PlayFabClientInstanceAPI(PlayFabApiSettings settings)
	{
		apiSettings = settings;
		authenticationContext = new PlayFabAuthenticationContext();
	}

	public PlayFabClientInstanceAPI(PlayFabAuthenticationContext context)
	{
		authenticationContext = context ?? new PlayFabAuthenticationContext();
	}

	public PlayFabClientInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
	{
		apiSettings = settings;
		authenticationContext = context ?? new PlayFabAuthenticationContext();
	}

	public bool IsClientLoggedIn()
	{
		if (authenticationContext != null)
		{
			return authenticationContext.IsClientLoggedIn();
		}
		return false;
	}

	public void ForgetAllCredentials()
	{
		if (authenticationContext != null)
		{
			authenticationContext.ForgetAllCredentials();
		}
	}

	public void AcceptTrade(AcceptTradeRequest request, Action<AcceptTradeResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AcceptTrade", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void AddFriend(AddFriendRequest request, Action<AddFriendResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AddFriend", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void AddGenericID(AddGenericIDRequest request, Action<AddGenericIDResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AddGenericID", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void AddOrUpdateContactEmail(AddOrUpdateContactEmailRequest request, Action<AddOrUpdateContactEmailResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AddOrUpdateContactEmail", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void AddSharedGroupMembers(AddSharedGroupMembersRequest request, Action<AddSharedGroupMembersResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AddSharedGroupMembers", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void AddUsernamePassword(AddUsernamePasswordRequest request, Action<AddUsernamePasswordResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AddUsernamePassword", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void AddUserVirtualCurrency(AddUserVirtualCurrencyRequest request, Action<ModifyUserVirtualCurrencyResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AddUserVirtualCurrency", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void AndroidDevicePushNotificationRegistration(AndroidDevicePushNotificationRegistrationRequest request, Action<AndroidDevicePushNotificationRegistrationResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AndroidDevicePushNotificationRegistration", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void AttributeInstall(AttributeInstallRequest request, Action<AttributeInstallResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/AttributeInstall", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void CancelTrade(CancelTradeRequest request, Action<CancelTradeResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/CancelTrade", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ConfirmPurchase(ConfirmPurchaseRequest request, Action<ConfirmPurchaseResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ConfirmPurchase", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ConsumeItem(ConsumeItemRequest request, Action<ConsumeItemResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ConsumeItem", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ConsumePSNEntitlements(ConsumePSNEntitlementsRequest request, Action<ConsumePSNEntitlementsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ConsumePSNEntitlements", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ConsumeXboxEntitlements(ConsumeXboxEntitlementsRequest request, Action<ConsumeXboxEntitlementsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ConsumeXboxEntitlements", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void CreateSharedGroup(CreateSharedGroupRequest request, Action<CreateSharedGroupResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/CreateSharedGroup", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ExecuteCloudScript(ExecuteCloudScriptRequest request, Action<ExecuteCloudScriptResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ExecuteCloudScript", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ExecuteCloudScript<TOut>(ExecuteCloudScriptRequest request, Action<ExecuteCloudScriptResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		Action<ExecuteCloudScriptResult> resultCallback2 = delegate(ExecuteCloudScriptResult wrappedResult)
		{
			ISerializerPlugin plugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
			string text = plugin.SerializeObject(wrappedResult.FunctionResult);
			try
			{
				wrappedResult.FunctionResult = plugin.DeserializeObject<TOut>(text);
			}
			catch (Exception)
			{
				wrappedResult.FunctionResult = text;
				wrappedResult.Logs.Add(new LogStatement
				{
					Level = "Warning",
					Data = text,
					Message = "Sdk Message: Could not deserialize result as: " + typeof(TOut).Name
				});
			}
			resultCallback(wrappedResult);
		};
		PlayFabHttp.MakeApiCall("/Client/ExecuteCloudScript", request, AuthType.LoginSession, resultCallback2, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetAccountInfo(GetAccountInfoRequest request, Action<GetAccountInfoResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetAccountInfo", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetAdPlacements(GetAdPlacementsRequest request, Action<GetAdPlacementsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetAdPlacements", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetAllUsersCharacters(ListUsersCharactersRequest request, Action<ListUsersCharactersResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetAllUsersCharacters", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetCatalogItems(GetCatalogItemsRequest request, Action<GetCatalogItemsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetCatalogItems", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetCharacterData(GetCharacterDataRequest request, Action<GetCharacterDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetCharacterData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetCharacterInventory(GetCharacterInventoryRequest request, Action<GetCharacterInventoryResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetCharacterInventory", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetCharacterLeaderboard(GetCharacterLeaderboardRequest request, Action<GetCharacterLeaderboardResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetCharacterLeaderboard", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetCharacterReadOnlyData(GetCharacterDataRequest request, Action<GetCharacterDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetCharacterReadOnlyData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetCharacterStatistics(GetCharacterStatisticsRequest request, Action<GetCharacterStatisticsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetCharacterStatistics", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetContentDownloadUrl(GetContentDownloadUrlRequest request, Action<GetContentDownloadUrlResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetContentDownloadUrl", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetCurrentGames(CurrentGamesRequest request, Action<CurrentGamesResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetCurrentGames", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetFriendLeaderboard(GetFriendLeaderboardRequest request, Action<GetLeaderboardResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetFriendLeaderboard", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetFriendLeaderboardAroundPlayer(GetFriendLeaderboardAroundPlayerRequest request, Action<GetFriendLeaderboardAroundPlayerResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetFriendLeaderboardAroundPlayer", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetFriendsList(GetFriendsListRequest request, Action<GetFriendsListResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetFriendsList", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetGameServerRegions(GameServerRegionsRequest request, Action<GameServerRegionsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetGameServerRegions", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetLeaderboard(GetLeaderboardRequest request, Action<GetLeaderboardResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetLeaderboard", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetLeaderboardAroundCharacter(GetLeaderboardAroundCharacterRequest request, Action<GetLeaderboardAroundCharacterResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetLeaderboardAroundCharacter", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetLeaderboardAroundPlayer(GetLeaderboardAroundPlayerRequest request, Action<GetLeaderboardAroundPlayerResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetLeaderboardAroundPlayer", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetLeaderboardForUserCharacters(GetLeaderboardForUsersCharactersRequest request, Action<GetLeaderboardForUsersCharactersResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetLeaderboardForUserCharacters", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPaymentToken(GetPaymentTokenRequest request, Action<GetPaymentTokenResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPaymentToken", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPhotonAuthenticationToken(GetPhotonAuthenticationTokenRequest request, Action<GetPhotonAuthenticationTokenResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPhotonAuthenticationToken", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayerCombinedInfo(GetPlayerCombinedInfoRequest request, Action<GetPlayerCombinedInfoResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayerCombinedInfo", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayerProfile(GetPlayerProfileRequest request, Action<GetPlayerProfileResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayerProfile", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayerSegments(GetPlayerSegmentsRequest request, Action<GetPlayerSegmentsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayerSegments", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayerStatistics(GetPlayerStatisticsRequest request, Action<GetPlayerStatisticsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayerStatistics", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayerStatisticVersions(GetPlayerStatisticVersionsRequest request, Action<GetPlayerStatisticVersionsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayerStatisticVersions", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayerTags(GetPlayerTagsRequest request, Action<GetPlayerTagsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayerTags", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayerTrades(GetPlayerTradesRequest request, Action<GetPlayerTradesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayerTrades", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromFacebookIDs(GetPlayFabIDsFromFacebookIDsRequest request, Action<GetPlayFabIDsFromFacebookIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromFacebookIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromFacebookInstantGamesIds(GetPlayFabIDsFromFacebookInstantGamesIdsRequest request, Action<GetPlayFabIDsFromFacebookInstantGamesIdsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromFacebookInstantGamesIds", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromGameCenterIDs(GetPlayFabIDsFromGameCenterIDsRequest request, Action<GetPlayFabIDsFromGameCenterIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromGameCenterIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromGenericIDs(GetPlayFabIDsFromGenericIDsRequest request, Action<GetPlayFabIDsFromGenericIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromGenericIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromGoogleIDs(GetPlayFabIDsFromGoogleIDsRequest request, Action<GetPlayFabIDsFromGoogleIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromGoogleIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromKongregateIDs(GetPlayFabIDsFromKongregateIDsRequest request, Action<GetPlayFabIDsFromKongregateIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromKongregateIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromNintendoSwitchDeviceIds(GetPlayFabIDsFromNintendoSwitchDeviceIdsRequest request, Action<GetPlayFabIDsFromNintendoSwitchDeviceIdsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromNintendoSwitchDeviceIds", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromPSNAccountIDs(GetPlayFabIDsFromPSNAccountIDsRequest request, Action<GetPlayFabIDsFromPSNAccountIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromPSNAccountIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromSteamIDs(GetPlayFabIDsFromSteamIDsRequest request, Action<GetPlayFabIDsFromSteamIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromSteamIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromTwitchIDs(GetPlayFabIDsFromTwitchIDsRequest request, Action<GetPlayFabIDsFromTwitchIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromTwitchIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPlayFabIDsFromXboxLiveIDs(GetPlayFabIDsFromXboxLiveIDsRequest request, Action<GetPlayFabIDsFromXboxLiveIDsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPlayFabIDsFromXboxLiveIDs", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPublisherData(GetPublisherDataRequest request, Action<GetPublisherDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPublisherData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetPurchase(GetPurchaseRequest request, Action<GetPurchaseResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetPurchase", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetSharedGroupData(GetSharedGroupDataRequest request, Action<GetSharedGroupDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetSharedGroupData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetStoreItems(GetStoreItemsRequest request, Action<GetStoreItemsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetStoreItems", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetTime(GetTimeRequest request, Action<GetTimeResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetTime", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetTitleData(GetTitleDataRequest request, Action<GetTitleDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetTitleData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetTitleNews(GetTitleNewsRequest request, Action<GetTitleNewsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetTitleNews", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetTitlePublicKey(GetTitlePublicKeyRequest request, Action<GetTitlePublicKeyResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		PlayFabHttp.MakeApiCall("/Client/GetTitlePublicKey", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetTradeStatus(GetTradeStatusRequest request, Action<GetTradeStatusResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetTradeStatus", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetUserData(GetUserDataRequest request, Action<GetUserDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetUserData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetUserInventory(GetUserInventoryRequest request, Action<GetUserInventoryResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetUserInventory", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetUserPublisherData(GetUserDataRequest request, Action<GetUserDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetUserPublisherData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetUserPublisherReadOnlyData(GetUserDataRequest request, Action<GetUserDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetUserPublisherReadOnlyData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetUserReadOnlyData(GetUserDataRequest request, Action<GetUserDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GetUserReadOnlyData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetWindowsHelloChallenge(GetWindowsHelloChallengeRequest request, Action<GetWindowsHelloChallengeResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		PlayFabHttp.MakeApiCall("/Client/GetWindowsHelloChallenge", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GrantCharacterToUser(GrantCharacterToUserRequest request, Action<GrantCharacterToUserResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/GrantCharacterToUser", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkAndroidDeviceID(LinkAndroidDeviceIDRequest request, Action<LinkAndroidDeviceIDResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkAndroidDeviceID", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkApple(LinkAppleRequest request, Action<EmptyResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkApple", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkCustomID(LinkCustomIDRequest request, Action<LinkCustomIDResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkCustomID", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkFacebookAccount(LinkFacebookAccountRequest request, Action<LinkFacebookAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkFacebookAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkFacebookInstantGamesId(LinkFacebookInstantGamesIdRequest request, Action<LinkFacebookInstantGamesIdResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkFacebookInstantGamesId", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkGameCenterAccount(LinkGameCenterAccountRequest request, Action<LinkGameCenterAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkGameCenterAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkGoogleAccount(LinkGoogleAccountRequest request, Action<LinkGoogleAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkGoogleAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkIOSDeviceID(LinkIOSDeviceIDRequest request, Action<LinkIOSDeviceIDResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkIOSDeviceID", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkKongregate(LinkKongregateAccountRequest request, Action<LinkKongregateAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkKongregate", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkNintendoAccount(LinkNintendoAccountRequest request, Action<EmptyResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkNintendoAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkNintendoSwitchDeviceId(LinkNintendoSwitchDeviceIdRequest request, Action<LinkNintendoSwitchDeviceIdResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkNintendoSwitchDeviceId", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkOpenIdConnect(LinkOpenIdConnectRequest request, Action<EmptyResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkOpenIdConnect", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkPSNAccount(LinkPSNAccountRequest request, Action<LinkPSNAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkPSNAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkSteamAccount(LinkSteamAccountRequest request, Action<LinkSteamAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkSteamAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkTwitch(LinkTwitchAccountRequest request, Action<LinkTwitchAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkTwitch", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkWindowsHello(LinkWindowsHelloAccountRequest request, Action<LinkWindowsHelloAccountResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkWindowsHello", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LinkXboxAccount(LinkXboxAccountRequest request, Action<LinkXboxAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/LinkXboxAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithAndroidDeviceID(LoginWithAndroidDeviceIDRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithAndroidDeviceID", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithApple(LoginWithAppleRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithApple", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithCustomID(LoginWithCustomIDRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithCustomID", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithEmailAddress(LoginWithEmailAddressRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithEmailAddress", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithFacebook(LoginWithFacebookRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithFacebook", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithFacebookInstantGamesId(LoginWithFacebookInstantGamesIdRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithFacebookInstantGamesId", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithGameCenter(LoginWithGameCenterRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithGameCenter", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithGoogleAccount(LoginWithGoogleAccountRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithGoogleAccount", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithIOSDeviceID(LoginWithIOSDeviceIDRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithIOSDeviceID", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithKongregate(LoginWithKongregateRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithKongregate", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithNintendoAccount(LoginWithNintendoAccountRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithNintendoAccount", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithNintendoSwitchDeviceId(LoginWithNintendoSwitchDeviceIdRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithNintendoSwitchDeviceId", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithOpenIdConnect(LoginWithOpenIdConnectRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithOpenIdConnect", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithPlayFab(LoginWithPlayFabRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithPlayFab", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithPSN(LoginWithPSNRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithPSN", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithSteam(LoginWithSteamRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithSteam", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithTwitch(LoginWithTwitchRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithTwitch", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithWindowsHello(LoginWithWindowsHelloRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithWindowsHello", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void LoginWithXbox(LoginWithXboxRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/LoginWithXbox", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void Matchmake(MatchmakeRequest request, Action<MatchmakeResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/Matchmake", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void OpenTrade(OpenTradeRequest request, Action<OpenTradeResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/OpenTrade", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void PayForPurchase(PayForPurchaseRequest request, Action<PayForPurchaseResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/PayForPurchase", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void PurchaseItem(PurchaseItemRequest request, Action<PurchaseItemResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/PurchaseItem", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RedeemCoupon(RedeemCouponRequest request, Action<RedeemCouponResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RedeemCoupon", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RefreshPSNAuthToken(RefreshPSNAuthTokenRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RefreshPSNAuthToken", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RegisterForIOSPushNotification(RegisterForIOSPushNotificationRequest request, Action<RegisterForIOSPushNotificationResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RegisterForIOSPushNotification", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RegisterPlayFabUser(RegisterPlayFabUserRequest request, Action<RegisterPlayFabUserResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/RegisterPlayFabUser", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RegisterWithWindowsHello(RegisterWithWindowsHelloRequest request, Action<LoginResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		request.TitleId = request.TitleId ?? playFabApiSettings.TitleId;
		PlayFabHttp.MakeApiCall("/Client/RegisterWithWindowsHello", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RemoveContactEmail(RemoveContactEmailRequest request, Action<RemoveContactEmailResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RemoveContactEmail", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RemoveFriend(RemoveFriendRequest request, Action<RemoveFriendResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RemoveFriend", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RemoveGenericID(RemoveGenericIDRequest request, Action<RemoveGenericIDResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RemoveGenericID", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RemoveSharedGroupMembers(RemoveSharedGroupMembersRequest request, Action<RemoveSharedGroupMembersResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RemoveSharedGroupMembers", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ReportAdActivity(ReportAdActivityRequest request, Action<ReportAdActivityResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ReportAdActivity", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ReportDeviceInfo(DeviceInfoRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ReportDeviceInfo", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ReportPlayer(ReportPlayerClientRequest request, Action<ReportPlayerClientResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ReportPlayer", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RestoreIOSPurchases(RestoreIOSPurchasesRequest request, Action<RestoreIOSPurchasesResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RestoreIOSPurchases", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void RewardAdActivity(RewardAdActivityRequest request, Action<RewardAdActivityResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/RewardAdActivity", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void SendAccountRecoveryEmail(SendAccountRecoveryEmailRequest request, Action<SendAccountRecoveryEmailResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		PlayFabHttp.MakeApiCall("/Client/SendAccountRecoveryEmail", request, AuthType.None, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void SetFriendTags(SetFriendTagsRequest request, Action<SetFriendTagsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/SetFriendTags", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void SetPlayerSecret(SetPlayerSecretRequest request, Action<SetPlayerSecretResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/SetPlayerSecret", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void StartGame(StartGameRequest request, Action<StartGameResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/StartGame", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void StartPurchase(StartPurchaseRequest request, Action<StartPurchaseResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/StartPurchase", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void SubtractUserVirtualCurrency(SubtractUserVirtualCurrencyRequest request, Action<ModifyUserVirtualCurrencyResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/SubtractUserVirtualCurrency", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkAndroidDeviceID(UnlinkAndroidDeviceIDRequest request, Action<UnlinkAndroidDeviceIDResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkAndroidDeviceID", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkApple(UnlinkAppleRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkApple", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkCustomID(UnlinkCustomIDRequest request, Action<UnlinkCustomIDResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkCustomID", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkFacebookAccount(UnlinkFacebookAccountRequest request, Action<UnlinkFacebookAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkFacebookAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkFacebookInstantGamesId(UnlinkFacebookInstantGamesIdRequest request, Action<UnlinkFacebookInstantGamesIdResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkFacebookInstantGamesId", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkGameCenterAccount(UnlinkGameCenterAccountRequest request, Action<UnlinkGameCenterAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkGameCenterAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkGoogleAccount(UnlinkGoogleAccountRequest request, Action<UnlinkGoogleAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkGoogleAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkIOSDeviceID(UnlinkIOSDeviceIDRequest request, Action<UnlinkIOSDeviceIDResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkIOSDeviceID", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkKongregate(UnlinkKongregateAccountRequest request, Action<UnlinkKongregateAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkKongregate", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkNintendoAccount(UnlinkNintendoAccountRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkNintendoAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkNintendoSwitchDeviceId(UnlinkNintendoSwitchDeviceIdRequest request, Action<UnlinkNintendoSwitchDeviceIdResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkNintendoSwitchDeviceId", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkOpenIdConnect(UnlinkOpenIdConnectRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkOpenIdConnect", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkPSNAccount(UnlinkPSNAccountRequest request, Action<UnlinkPSNAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkPSNAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkSteamAccount(UnlinkSteamAccountRequest request, Action<UnlinkSteamAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkSteamAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkTwitch(UnlinkTwitchAccountRequest request, Action<UnlinkTwitchAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkTwitch", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkWindowsHello(UnlinkWindowsHelloAccountRequest request, Action<UnlinkWindowsHelloAccountResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkWindowsHello", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlinkXboxAccount(UnlinkXboxAccountRequest request, Action<UnlinkXboxAccountResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlinkXboxAccount", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlockContainerInstance(UnlockContainerInstanceRequest request, Action<UnlockContainerItemResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlockContainerInstance", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UnlockContainerItem(UnlockContainerItemRequest request, Action<UnlockContainerItemResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UnlockContainerItem", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UpdateAvatarUrl(UpdateAvatarUrlRequest request, Action<EmptyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UpdateAvatarUrl", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UpdateCharacterData(UpdateCharacterDataRequest request, Action<UpdateCharacterDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UpdateCharacterData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UpdateCharacterStatistics(UpdateCharacterStatisticsRequest request, Action<UpdateCharacterStatisticsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UpdateCharacterStatistics", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UpdatePlayerStatistics(UpdatePlayerStatisticsRequest request, Action<UpdatePlayerStatisticsResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UpdatePlayerStatistics", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UpdateSharedGroupData(UpdateSharedGroupDataRequest request, Action<UpdateSharedGroupDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UpdateSharedGroupData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UpdateUserData(UpdateUserDataRequest request, Action<UpdateUserDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UpdateUserData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UpdateUserPublisherData(UpdateUserDataRequest request, Action<UpdateUserDataResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UpdateUserPublisherData", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void UpdateUserTitleDisplayName(UpdateUserTitleDisplayNameRequest request, Action<UpdateUserTitleDisplayNameResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/UpdateUserTitleDisplayName", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ValidateAmazonIAPReceipt(ValidateAmazonReceiptRequest request, Action<ValidateAmazonReceiptResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ValidateAmazonIAPReceipt", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ValidateGooglePlayPurchase(ValidateGooglePlayPurchaseRequest request, Action<ValidateGooglePlayPurchaseResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ValidateGooglePlayPurchase", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ValidateIOSReceipt(ValidateIOSReceiptRequest request, Action<ValidateIOSReceiptResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ValidateIOSReceipt", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ValidateWindowsStoreReceipt(ValidateWindowsReceiptRequest request, Action<ValidateWindowsReceiptResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/ValidateWindowsStoreReceipt", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void WriteCharacterEvent(WriteClientCharacterEventRequest request, Action<WriteEventResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/WriteCharacterEvent", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void WritePlayerEvent(WriteClientPlayerEventRequest request, Action<WriteEventResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/WritePlayerEvent", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void WriteTitleEvent(WriteTitleEventRequest request, Action<WriteEventResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsClientLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Client/WriteTitleEvent", request, AuthType.LoginSession, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}
}
