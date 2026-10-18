using System;
using System.Collections.Generic;
using PlayFab.Internal;
using PlayFab.ProfilesModels;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabProfilesInstanceAPI : IPlayFabInstanceApi
{
	public readonly PlayFabApiSettings apiSettings;

	public readonly PlayFabAuthenticationContext authenticationContext;

	public PlayFabProfilesInstanceAPI(PlayFabAuthenticationContext context)
	{
		if (context == null)
		{
			throw new PlayFabException(PlayFabExceptionCode.AuthContextRequired, "Context cannot be null, create a PlayFabAuthenticationContext for each player in advance, or call <PlayFabClientInstanceAPI>.GetAuthenticationContext()");
		}
		authenticationContext = context;
	}

	public PlayFabProfilesInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
	{
		if (context == null)
		{
			throw new PlayFabException(PlayFabExceptionCode.AuthContextRequired, "Context cannot be null, create a PlayFabAuthenticationContext for each player in advance, or call <PlayFabClientInstanceAPI>.GetAuthenticationContext()");
		}
		apiSettings = settings;
		authenticationContext = context;
	}

	public bool IsEntityLoggedIn()
	{
		if (authenticationContext != null)
		{
			return authenticationContext.IsEntityLoggedIn();
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

	public void GetGlobalPolicy(GetGlobalPolicyRequest request, Action<GetGlobalPolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Profile/GetGlobalPolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetProfile(GetEntityProfileRequest request, Action<GetEntityProfileResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Profile/GetProfile", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetProfiles(GetEntityProfilesRequest request, Action<GetEntityProfilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Profile/GetProfiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetTitlePlayersFromMasterPlayerAccountIds(GetTitlePlayersFromMasterPlayerAccountIdsRequest request, Action<GetTitlePlayersFromMasterPlayerAccountIdsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Profile/GetTitlePlayersFromMasterPlayerAccountIds", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void SetGlobalPolicy(SetGlobalPolicyRequest request, Action<SetGlobalPolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Profile/SetGlobalPolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void SetProfileLanguage(SetProfileLanguageRequest request, Action<SetProfileLanguageResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Profile/SetProfileLanguage", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void SetProfilePolicy(SetEntityProfilePolicyRequest request, Action<SetEntityProfilePolicyResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Profile/SetProfilePolicy", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}
}
