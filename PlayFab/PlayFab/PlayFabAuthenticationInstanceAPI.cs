using System;
using System.Collections.Generic;
using PlayFab.AuthenticationModels;
using PlayFab.Internal;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabAuthenticationInstanceAPI : IPlayFabInstanceApi
{
	public readonly PlayFabApiSettings apiSettings;

	public readonly PlayFabAuthenticationContext authenticationContext;

	public PlayFabAuthenticationInstanceAPI()
	{
		authenticationContext = new PlayFabAuthenticationContext();
	}

	public PlayFabAuthenticationInstanceAPI(PlayFabApiSettings settings)
	{
		apiSettings = settings;
		authenticationContext = new PlayFabAuthenticationContext();
	}

	public PlayFabAuthenticationInstanceAPI(PlayFabAuthenticationContext context)
	{
		authenticationContext = context ?? new PlayFabAuthenticationContext();
	}

	public PlayFabAuthenticationInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
	{
		apiSettings = settings;
		authenticationContext = context ?? new PlayFabAuthenticationContext();
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

	public void GetEntityToken(GetEntityTokenRequest request, Action<GetEntityTokenResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		AuthType authType = AuthType.None;
		if (playFabAuthenticationContext.IsClientLoggedIn())
		{
			authType = AuthType.LoginSession;
		}
		if (playFabAuthenticationContext.IsEntityLoggedIn())
		{
			authType = AuthType.EntityToken;
		}
		PlayFabHttp.MakeApiCall("/Authentication/GetEntityToken", request, authType, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void ValidateEntityToken(ValidateEntityTokenRequest request, Action<ValidateEntityTokenResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Authentication/ValidateEntityToken", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}
}
