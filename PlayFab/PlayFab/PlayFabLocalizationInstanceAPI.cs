using System;
using System.Collections.Generic;
using PlayFab.Internal;
using PlayFab.LocalizationModels;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabLocalizationInstanceAPI : IPlayFabInstanceApi
{
	public readonly PlayFabApiSettings apiSettings;

	public readonly PlayFabAuthenticationContext authenticationContext;

	public PlayFabLocalizationInstanceAPI(PlayFabAuthenticationContext context)
	{
		if (context == null)
		{
			throw new PlayFabException(PlayFabExceptionCode.AuthContextRequired, "Context cannot be null, create a PlayFabAuthenticationContext for each player in advance, or call <PlayFabClientInstanceAPI>.GetAuthenticationContext()");
		}
		authenticationContext = context;
	}

	public PlayFabLocalizationInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
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

	public void GetLanguageList(GetLanguageListRequest request, Action<GetLanguageListResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Locale/GetLanguageList", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}
}
