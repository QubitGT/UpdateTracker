using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.Public;
using PlayFab.SharedModels;
using UnityEngine;

namespace PlayFab.Internal;

public class PlayFabHttp : SingletonMonoBehaviour<PlayFabHttp>
{
	public delegate void ApiProcessingEvent<in TEventArgs>(TEventArgs e);

	public delegate void ApiProcessErrorEvent(PlayFabRequestCommon request, PlayFabError error);

	private static List<CallRequestContainer> _apiCallQueue = new List<CallRequestContainer>();

	public static readonly Dictionary<string, string> GlobalHeaderInjection = new Dictionary<string, string>();

	private static IPlayFabLogger _logger;

	private static IScreenTimeTracker screenTimeTracker = new ScreenTimeTracker();

	private const float delayBetweenBatches = 5f;

	private readonly Queue<IEnumerator> _injectedCoroutines = new Queue<IEnumerator>();

	private readonly Queue<Action> _injectedAction = new Queue<Action>();

	public static event ApiProcessingEvent<ApiProcessingEventArgs> ApiProcessingEventHandler;

	public static event ApiProcessErrorEvent ApiProcessingErrorEventHandler;

	public static int GetPendingMessages()
	{
		ITransportPlugin plugin = PluginManager.GetPlugin<ITransportPlugin>(PluginContract.PlayFab_Transport);
		if (!plugin.IsInitialized)
		{
			return 0;
		}
		return plugin.GetPendingMessages();
	}

	public static void InitializeHttp()
	{
		if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
		{
			throw new PlayFabException(PlayFabExceptionCode.TitleNotSet, "You must set PlayFabSettings.TitleId before making API Calls.");
		}
		ITransportPlugin plugin = PluginManager.GetPlugin<ITransportPlugin>(PluginContract.PlayFab_Transport);
		if (!plugin.IsInitialized)
		{
			plugin.Initialize();
			SingletonMonoBehaviour<PlayFabHttp>.CreateInstance();
		}
	}

	public static void InitializeLogger(IPlayFabLogger setLogger = null)
	{
		if (_logger != null)
		{
			throw new InvalidOperationException("Once initialized, the logger cannot be reset.");
		}
		if (setLogger == null)
		{
			setLogger = new PlayFabLogger();
		}
		_logger = setLogger;
	}

	public static void InitializeScreenTimeTracker(string entityId, string entityType, string playFabUserId)
	{
		screenTimeTracker.ClientSessionStart(entityId, entityType, playFabUserId);
		SingletonMonoBehaviour<PlayFabHttp>.instance.StartCoroutine(SendScreenTimeEvents(5f));
	}

	private static IEnumerator SendScreenTimeEvents(float secondsBetweenBatches)
	{
		WaitForSeconds delay = new WaitForSeconds(secondsBetweenBatches);
		while (!PlayFabSettings.DisableFocusTimeCollection)
		{
			screenTimeTracker.Send();
			yield return delay;
		}
	}

	public static void SimpleGetCall(string fullUrl, Action<byte[]> successCallback, Action<string> errorCallback)
	{
		InitializeHttp();
		PluginManager.GetPlugin<ITransportPlugin>(PluginContract.PlayFab_Transport).SimpleGetCall(fullUrl, successCallback, errorCallback);
	}

	public static void SimplePutCall(string fullUrl, byte[] payload, Action<byte[]> successCallback, Action<string> errorCallback)
	{
		InitializeHttp();
		PluginManager.GetPlugin<ITransportPlugin>(PluginContract.PlayFab_Transport).SimplePutCall(fullUrl, payload, successCallback, errorCallback);
	}

	public static void SimplePostCall(string fullUrl, byte[] payload, Action<byte[]> successCallback, Action<string> errorCallback)
	{
		InitializeHttp();
		PluginManager.GetPlugin<ITransportPlugin>(PluginContract.PlayFab_Transport).SimplePostCall(fullUrl, payload, successCallback, errorCallback);
	}

	protected internal static void MakeApiCall<TResult>(string apiEndpoint, PlayFabRequestCommon request, AuthType authType, Action<TResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null, PlayFabAuthenticationContext authenticationContext = null, PlayFabApiSettings apiSettings = null, IPlayFabInstanceApi instanceApi = null) where TResult : PlayFabResultCommon
	{
		apiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		string fullUrl = apiSettings.GetFullUrl(apiEndpoint, apiSettings.RequestGetParams);
		_MakeApiCall(apiEndpoint, fullUrl, request, authType, resultCallback, errorCallback, customData, extraHeaders, allowQueueing: false, authenticationContext, apiSettings, instanceApi);
	}

	protected internal static void MakeApiCallWithFullUri<TResult>(string fullUri, PlayFabRequestCommon request, AuthType authType, Action<TResult> resultCallback, Action<PlayFabError> errorCallback, object customData = null, Dictionary<string, string> extraHeaders = null, PlayFabAuthenticationContext authenticationContext = null, PlayFabApiSettings apiSettings = null, IPlayFabInstanceApi instanceApi = null) where TResult : PlayFabResultCommon
	{
		apiSettings = apiSettings ?? PlayFabSettings.staticSettings;
		_MakeApiCall(null, fullUri, request, authType, resultCallback, errorCallback, customData, extraHeaders, allowQueueing: false, authenticationContext, apiSettings, instanceApi);
	}

	private static void _MakeApiCall<TResult>(string apiEndpoint, string fullUrl, PlayFabRequestCommon request, AuthType authType, Action<TResult> resultCallback, Action<PlayFabError> errorCallback, object customData, Dictionary<string, string> extraHeaders, bool allowQueueing, PlayFabAuthenticationContext authenticationContext, PlayFabApiSettings apiSettings, IPlayFabInstanceApi instanceApi) where TResult : PlayFabResultCommon
	{
		InitializeHttp();
		SendEvent(apiEndpoint, request, null, ApiProcessingEventType.Pre);
		ISerializerPlugin serializer = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
		CallRequestContainer reqContainer = new CallRequestContainer
		{
			ApiEndpoint = apiEndpoint,
			FullUrl = fullUrl,
			settings = apiSettings,
			context = authenticationContext,
			CustomData = customData,
			Payload = Encoding.UTF8.GetBytes(serializer.SerializeObject(request)),
			ApiRequest = request,
			ErrorCallback = errorCallback,
			RequestHeaders = (extraHeaders ?? new Dictionary<string, string>()),
			instanceApi = instanceApi
		};
		foreach (KeyValuePair<string, string> item in GlobalHeaderInjection)
		{
			if (!reqContainer.RequestHeaders.ContainsKey(item.Key))
			{
				reqContainer.RequestHeaders[item.Key] = item.Value;
			}
		}
		ITransportPlugin plugin = PluginManager.GetPlugin<ITransportPlugin>(PluginContract.PlayFab_Transport);
		reqContainer.RequestHeaders["X-ReportErrorAsSuccess"] = "true";
		reqContainer.RequestHeaders["X-PlayFabSDK"] = "UnitySDK-2.87.200602";
		switch (authType)
		{
		case AuthType.LoginSession:
			if (authenticationContext != null)
			{
				reqContainer.RequestHeaders["X-Authorization"] = authenticationContext.ClientSessionTicket;
			}
			break;
		case AuthType.EntityToken:
			if (authenticationContext != null)
			{
				reqContainer.RequestHeaders["X-EntityToken"] = authenticationContext.EntityToken;
			}
			break;
		}
		reqContainer.DeserializeResultJson = delegate
		{
			reqContainer.ApiResult = serializer.DeserializeObject<TResult>(reqContainer.JsonResponse);
		};
		reqContainer.InvokeSuccessCallback = delegate
		{
			if (resultCallback != null)
			{
				resultCallback((TResult)reqContainer.ApiResult);
			}
		};
		if (allowQueueing && _apiCallQueue != null)
		{
			for (int num = _apiCallQueue.Count - 1; num >= 0; num--)
			{
				if (_apiCallQueue[num].ApiEndpoint == apiEndpoint)
				{
					_apiCallQueue.RemoveAt(num);
				}
			}
			_apiCallQueue.Add(reqContainer);
		}
		else
		{
			plugin.MakeApiCall(reqContainer);
		}
	}

	internal void OnPlayFabApiResult(CallRequestContainer reqContainer)
	{
		PlayFabResultCommon apiResult = reqContainer.ApiResult;
		if (apiResult is GetEntityTokenResponse getEntityTokenResponse)
		{
			PlayFabSettings.staticPlayer.EntityToken = getEntityTokenResponse.EntityToken;
		}
		LoginResult loginResult = apiResult as LoginResult;
		RegisterPlayFabUserResult registerPlayFabUserResult = apiResult as RegisterPlayFabUserResult;
		if (loginResult != null)
		{
			loginResult.AuthenticationContext = new PlayFabAuthenticationContext(loginResult.SessionTicket, loginResult.EntityToken.EntityToken, loginResult.PlayFabId, loginResult.EntityToken.Entity.Id, loginResult.EntityToken.Entity.Type);
			if (reqContainer.context != null)
			{
				reqContainer.context.CopyFrom(loginResult.AuthenticationContext);
			}
		}
		else if (registerPlayFabUserResult != null)
		{
			registerPlayFabUserResult.AuthenticationContext = new PlayFabAuthenticationContext(registerPlayFabUserResult.SessionTicket, registerPlayFabUserResult.EntityToken.EntityToken, registerPlayFabUserResult.PlayFabId, registerPlayFabUserResult.EntityToken.Entity.Id, registerPlayFabUserResult.EntityToken.Entity.Type);
			if (reqContainer.context != null)
			{
				reqContainer.context.CopyFrom(registerPlayFabUserResult.AuthenticationContext);
			}
		}
	}

	private void OnEnable()
	{
		if (_logger != null)
		{
			_logger.OnEnable();
		}
		if (screenTimeTracker != null && !PlayFabSettings.DisableFocusTimeCollection)
		{
			screenTimeTracker.OnEnable();
		}
	}

	private void OnDisable()
	{
		if (_logger != null)
		{
			_logger.OnDisable();
		}
		if (screenTimeTracker != null && !PlayFabSettings.DisableFocusTimeCollection)
		{
			screenTimeTracker.OnDisable();
		}
	}

	private void OnDestroy()
	{
		ITransportPlugin plugin = PluginManager.GetPlugin<ITransportPlugin>(PluginContract.PlayFab_Transport);
		if (plugin.IsInitialized)
		{
			plugin.OnDestroy();
		}
		if (_logger != null)
		{
			_logger.OnDestroy();
		}
		if (screenTimeTracker != null && !PlayFabSettings.DisableFocusTimeCollection)
		{
			screenTimeTracker.OnDestroy();
		}
	}

	public void OnApplicationFocus(bool isFocused)
	{
		if (screenTimeTracker != null && !PlayFabSettings.DisableFocusTimeCollection)
		{
			screenTimeTracker.OnApplicationFocus(isFocused);
		}
	}

	public void OnApplicationQuit()
	{
		if (screenTimeTracker != null && !PlayFabSettings.DisableFocusTimeCollection)
		{
			screenTimeTracker.OnApplicationQuit();
		}
	}

	private void Update()
	{
		ITransportPlugin plugin = PluginManager.GetPlugin<ITransportPlugin>(PluginContract.PlayFab_Transport);
		if (plugin.IsInitialized)
		{
			if (_apiCallQueue != null)
			{
				foreach (CallRequestContainer item in _apiCallQueue)
				{
					plugin.MakeApiCall(item);
				}
				_apiCallQueue = null;
			}
			plugin.Update();
		}
		while (_injectedCoroutines.Count > 0)
		{
			StartCoroutine(_injectedCoroutines.Dequeue());
		}
		while (_injectedAction.Count > 0)
		{
			_injectedAction.Dequeue()?.Invoke();
		}
	}

	protected internal static PlayFabError GeneratePlayFabError(string apiEndpoint, string json, object customData)
	{
		Dictionary<string, object> dictionary = null;
		Dictionary<string, List<string>> errorDetails = null;
		ISerializerPlugin plugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
		try
		{
			dictionary = plugin.DeserializeObject<Dictionary<string, object>>(json);
		}
		catch (Exception)
		{
		}
		try
		{
			if (dictionary != null && dictionary.TryGetValue("errorDetails", out var value))
			{
				errorDetails = plugin.DeserializeObject<Dictionary<string, List<string>>>(value.ToString());
			}
		}
		catch (Exception)
		{
		}
		return new PlayFabError
		{
			ApiEndpoint = apiEndpoint,
			HttpCode = ((dictionary != null && dictionary.ContainsKey("code")) ? Convert.ToInt32(dictionary["code"]) : 400),
			HttpStatus = ((dictionary != null && dictionary.ContainsKey("status")) ? ((string)dictionary["status"]) : "BadRequest"),
			Error = ((dictionary != null && dictionary.ContainsKey("errorCode")) ? ((PlayFabErrorCode)Convert.ToInt32(dictionary["errorCode"])) : PlayFabErrorCode.ServiceUnavailable),
			ErrorMessage = ((dictionary != null && dictionary.ContainsKey("errorMessage")) ? ((string)dictionary["errorMessage"]) : json),
			ErrorDetails = errorDetails,
			CustomData = customData
		};
	}

	protected internal static void SendErrorEvent(PlayFabRequestCommon request, PlayFabError error)
	{
		if (PlayFabHttp.ApiProcessingErrorEventHandler == null)
		{
			return;
		}
		try
		{
			PlayFabHttp.ApiProcessingErrorEventHandler(request, error);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	protected internal static void SendEvent(string apiEndpoint, PlayFabRequestCommon request, PlayFabResultCommon result, ApiProcessingEventType eventType)
	{
		if (PlayFabHttp.ApiProcessingEventHandler == null)
		{
			return;
		}
		try
		{
			PlayFabHttp.ApiProcessingEventHandler(new ApiProcessingEventArgs
			{
				ApiEndpoint = apiEndpoint,
				EventType = eventType,
				Request = request,
				Result = result
			});
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static void ClearAllEvents()
	{
		PlayFabHttp.ApiProcessingEventHandler = null;
		PlayFabHttp.ApiProcessingErrorEventHandler = null;
	}

	public void InjectInUnityThread(IEnumerator x)
	{
		_injectedCoroutines.Enqueue(x);
	}

	public void InjectInUnityThread(Action action)
	{
		_injectedAction.Enqueue(action);
	}
}
