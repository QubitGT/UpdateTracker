using System;
using System.Collections.Generic;
using PlayFab.AuthenticationModels;
using PlayFab.Internal;

namespace PlayFab;

public static class PlayFabAuthenticationAPI
{
	static PlayFabAuthenticationAPI()
	{
	}

	public static bool IsEntityLoggedIn()
	{
		return PlayFabSettings.staticPlayer.IsEntityLoggedIn();
	}

	public static void ForgetAllCredentials()
	{
		PlayFabSettings.staticPlayer.ForgetAllCredentials();
	}

	public static void GetEntityToken(GetEntityTokenRequest request, Action<GetEntityTokenResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		AuthType authType = AuthType.None;
		if (playFabAuthenticationContext.IsClientLoggedIn())
		{
			authType = AuthType.LoginSession;
		}
		if (playFabAuthenticationContext.IsEntityLoggedIn())
		{
			authType = AuthType.EntityToken;
		}
		PlayFabHttp.MakeApiCall("/Authentication/GetEntityToken", request, authType, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}

	public static void ValidateEntityToken(ValidateEntityTokenRequest request, Action<ValidateEntityTokenResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? PlayFabSettings.staticPlayer;
		PlayFabApiSettings staticSettings = PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Authentication/ValidateEntityToken", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, staticSettings);
	}
}
