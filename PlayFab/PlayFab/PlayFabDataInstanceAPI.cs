using System;
using System.Collections.Generic;
using PlayFab.DataModels;
using PlayFab.Internal;
using PlayFab.SharedModels;

namespace PlayFab;

public class PlayFabDataInstanceAPI : IPlayFabInstanceApi
{
	public readonly PlayFabApiSettings apiSettings;

	public readonly PlayFabAuthenticationContext authenticationContext;

	public PlayFabDataInstanceAPI(PlayFabAuthenticationContext context)
	{
		if (context == null)
		{
			throw new PlayFabException(PlayFabExceptionCode.AuthContextRequired, "Context cannot be null, create a PlayFabAuthenticationContext for each player in advance, or call <PlayFabClientInstanceAPI>.GetAuthenticationContext()");
		}
		authenticationContext = context;
	}

	public PlayFabDataInstanceAPI(PlayFabApiSettings settings, PlayFabAuthenticationContext context)
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

	public void AbortFileUploads(AbortFileUploadsRequest request, Action<AbortFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/File/AbortFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void DeleteFiles(DeleteFilesRequest request, Action<DeleteFilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/File/DeleteFiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void FinalizeFileUploads(FinalizeFileUploadsRequest request, Action<FinalizeFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/File/FinalizeFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetFiles(GetFilesRequest request, Action<GetFilesResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/File/GetFiles", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void GetObjects(GetObjectsRequest request, Action<GetObjectsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Object/GetObjects", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void InitiateFileUploads(InitiateFileUploadsRequest request, Action<InitiateFileUploadsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/File/InitiateFileUploads", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}

	public void SetObjects(SetObjectsRequest request, Action<SetObjectsResponse> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null)
	{
		PlayFabAuthenticationContext playFabAuthenticationContext = request?.AuthenticationContext ?? authenticationContext;
		PlayFabApiSettings playFabApiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		if (!playFabAuthenticationContext.IsEntityLoggedIn())
		{
			throw new PlayFabException(PlayFabExceptionCode.NotLoggedIn, "Must be logged in to call this method");
		}
		PlayFabHttp.MakeApiCall("/Object/SetObjects", request, AuthType.EntityToken, resultCallback, errorCallback, customData, extraHeaders, playFabAuthenticationContext, playFabApiSettings, this);
	}
}
