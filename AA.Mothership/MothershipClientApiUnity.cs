using System;
using System.Runtime.InteropServices;
using NativeWebSocket;
using UnityEngine;

public static class MothershipClientApiUnity
{
	private static bool isEnabled;

	public static string MothershipBaseUrl;

	public static string TitleId;

	public static string EnvironmentId;

	public static string DeploymentId;

	public static string MothershipWebSocketUrl;

	public static string SessionId;

	private static MothershipClientApiClient client;

	private static MothershipHttpClientUnity http;

	private static MothershipWebSocketWrapper websocket;

	private static MothershipWebSocketDispatcher websocketDispatcher;

	private static MothershipLogCallback logCallback;

	private static MothershipAuthCallback auth;

	private static MothershipAuthRefreshRequiredCallback authRefreshRequiredCallback;

	private static MothershipBeginQuestCallback beginQuestCallback;

	private static MothershipBeginSteamCallback beginSteamCallback;

	private static MothershipGetUserDataCallback getUserdataCallback;

	private static MothershipSetUserDataCallback setUserDataCallback;

	private static MothershipCreateReportCallback createReportCallback;

	private static MothershipGetUserInventoryCallback getUserInventoryCallback;

	private static MothershipGetMergedInventoryCallback getMergedInventoryCallback;

	private static MothershipGetStorefrontCallback getStorefrontCallback;

	private static MothershipPurchaseOfferCallback purchaseOfferCallback;

	private static MothershipNotificationsWrapper notificationWrapper;

	private static MothershipGetPlayerProgressionCallback getProgressionCallback;

	private static MothershipGetPlayerProgressionTressCallback getProgressionTreesCallback;

	private static MothershipWriteEventsCallback writeEventsCallback;

	private static MothershipListTitleDataCallback listMothershipTitleDataCallback;

	private static MothershipGetMySubscriptionCallback getMySubscriptionCallback;

	private static MothershipGetRoomPlayersSubscriptionsCallback getRoomPlayersSubscriptionsCallback;

	private static MothershipInitSteamSubscriptionPurchaseCallback initSteamSubPurchaseCallback;

	private static MothershipFinalizeSteamSubscriptionPurchaseCallback finalizeSteamSubPurchaseCallback;

	private static MothershipGetFileDetailsCompleteCallback getFileDetailsCompleteCallback;

	private static MothershipConsumeCompleteCallback consumeConsumableCompleteCallback;

	private static MothershipSteamInitTransactionCallback initSteamPurchaseCallback;

	private static MothershipSteamFinalizeTransactionCallback finalizeSteamPurchaseCallback;

	private static MothershipRefreshIAPCallback refreshIAPCallback;

	public static event Action<nint> OnOpenNotificationSocket;

	public static event Action<NotificationsMessageResponse, nint> OnMessageNotificationSocket;

	public static event Action<nint> OnCloseNotificationSocket;

	public static event Action<nint> OnErrorNotificationSocket;

	public static MothershipSharedSettings GetSharedSettingsObject()
	{
		return Resources.Load<MothershipSharedSettings>("MothershipSharedSettings");
	}

	static MothershipClientApiUnity()
	{
		MothershipBaseUrl = "";
		TitleId = "";
		EnvironmentId = "";
		DeploymentId = "";
		MothershipWebSocketUrl = "";
		SessionId = "";
		MothershipSharedSettings sharedSettingsObject = GetSharedSettingsObject();
		MothershipBaseUrl = sharedSettingsObject.BaseUrl;
		TitleId = sharedSettingsObject.TitleId;
		EnvironmentId = sharedSettingsObject.EnvironmentId;
		DeploymentId = sharedSettingsObject.DeploymentId;
		MothershipWebSocketUrl = sharedSettingsObject.WebSocketUrl;
		isEnabled = sharedSettingsObject.Enabled;
		if (isEnabled)
		{
			try
			{
				SessionId = Guid.NewGuid().ToString("D");
				Debug.Log("Mothership session ID is " + SessionId);
				client = new MothershipClientApiClient(MothershipBaseUrl, TitleId, EnvironmentId, DeploymentId, MothershipWebSocketUrl, enableRetryQueue: true, SessionId);
				http = new MothershipHttpClientUnity(client, sharedSettingsObject.RequestLoggingEnabled);
				client.SetHttpRequestDelegate(http);
				auth = new MothershipAuthCallback(client);
				client.SetLoginCompleteDelegate(auth);
				getUserdataCallback = new MothershipGetUserDataCallback(client);
				client.SetGetUserDataCompleteClientDelegateWrapper(getUserdataCallback);
				setUserDataCallback = new MothershipSetUserDataCallback(client);
				client.SetSetUserDataCompleteClientDelegateWrapper(setUserDataCallback);
				createReportCallback = new MothershipCreateReportCallback();
				client.SetCreateReportCompleteClientDelegateWrapper(createReportCallback);
				getUserInventoryCallback = new MothershipGetUserInventoryCallback();
				client.SetGetUserInventoryCompleteClientDelegateWrapper(getUserInventoryCallback);
				getMergedInventoryCallback = new MothershipGetMergedInventoryCallback();
				client.SetGetMergedInventoryCompleteClientDelegateWrapper(getMergedInventoryCallback);
				getStorefrontCallback = new MothershipGetStorefrontCallback();
				client.SetGetStorefrontCompleteClientDelegateWrapper(getStorefrontCallback);
				purchaseOfferCallback = new MothershipPurchaseOfferCallback();
				client.SetPurchaseCompleteClientDelegateWrapper(purchaseOfferCallback);
				beginQuestCallback = new MothershipBeginQuestCallback();
				client.SetQuestAuthV2BeginRequestCompleteClientDelegateWrapper(beginQuestCallback);
				beginSteamCallback = new MothershipBeginSteamCallback();
				client.SetSteamBeginRequestCompleteClientDelegateWrapper(beginSteamCallback);
				websocket = new MothershipWebSocketWrapper(client);
				client.SetWebSocketDelegate(websocket);
				websocketDispatcher = MothershipWebSocketDispatcher.instance;
				notificationWrapper = new MothershipNotificationsWrapper(new Action<IntPtr>(InvokeOpenNotificationSocket), new Action<NotificationsMessageResponse, IntPtr>(InvokeMessageNotificationSocket), new Action<IntPtr>(InvokeCloseNotificationSocket), new Action<IntPtr>(InvokeErrorNotificationSocket));
				client.SetNotificationsMessageDelegateWrapper(notificationWrapper);
				getProgressionCallback = new MothershipGetPlayerProgressionCallback();
				client.SetGetProgressionTrackValuesForPlayerCompleteClientDelegateWrapper(getProgressionCallback);
				getProgressionTreesCallback = new MothershipGetPlayerProgressionTressCallback();
				client.SetGetProgressionTreesForPlayerCompleteClientDelegateWrapper(getProgressionTreesCallback);
				writeEventsCallback = new MothershipWriteEventsCallback();
				client.SetWriteEventsCompleteClientDelegateWrapper(writeEventsCallback);
				listMothershipTitleDataCallback = new MothershipListTitleDataCallback();
				client.SetListMothershipTitleDataCompleteClientDelegateWrapper(listMothershipTitleDataCallback);
				getMySubscriptionCallback = new MothershipGetMySubscriptionCallback();
				client.SetClientGetMySubscriptionsDelegateWrapper(getMySubscriptionCallback);
				getRoomPlayersSubscriptionsCallback = new MothershipGetRoomPlayersSubscriptionsCallback();
				client.SetClientBulkGetSubscriptionsDelegateWrapper(getRoomPlayersSubscriptionsCallback);
				initSteamSubPurchaseCallback = new MothershipInitSteamSubscriptionPurchaseCallback();
				client.SetClientInitSteamSubscriptionPurchaseCompleteDelegateWrapper(initSteamSubPurchaseCallback);
				finalizeSteamSubPurchaseCallback = new MothershipFinalizeSteamSubscriptionPurchaseCallback();
				client.SetClientFinalizeSteamSubscriptionPurchaseCompleteDelegateWrapper(finalizeSteamSubPurchaseCallback);
				getFileDetailsCompleteCallback = new MothershipGetFileDetailsCompleteCallback();
				client.SetGetFileCompleteDelegateWrapper(getFileDetailsCompleteCallback);
				consumeConsumableCompleteCallback = new MothershipConsumeCompleteCallback();
				client.SetConsumeConsumableCompleteClientDelegateWrapper(consumeConsumableCompleteCallback);
				initSteamPurchaseCallback = new MothershipSteamInitTransactionCallback();
				client.SetInitSteamPurchaseCompleteDelegateWrapper(initSteamPurchaseCallback);
				finalizeSteamPurchaseCallback = new MothershipSteamFinalizeTransactionCallback();
				client.SetFinalizeSteamPurchaseCompleteDelegateWrapper(finalizeSteamPurchaseCallback);
				refreshIAPCallback = new MothershipRefreshIAPCallback();
				client.SetMothershipRefreshIAPCompleteDelegateWrapper(refreshIAPCallback);
			}
			catch (Exception exception)
			{
				isEnabled = false;
				Debug.LogException(exception);
				Debug.LogError("Mothership Client API initialization failed");
			}
		}
	}

	public static bool IsClientLoggedIn()
	{
		return MothershipClientContext.IsClientLoggedIn();
	}

	public static void ForgetAllCredentials()
	{
		MothershipClientContext.ForgetAllCredentials();
	}

	public static bool IsEnabled()
	{
		return isEnabled;
	}

	public static void Tick(float deltaTime)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to tick Mothership SDK, but Mothership is not enabled!");
		}
		else
		{
			client.Tick(deltaTime);
		}
	}

	public static void SetLanguage(string newLanguage)
	{
		Debug.Log("[MothershipClient] Set language to " + newLanguage);
		client.SetAcceptLanguage(newLanguage);
	}

	public static void SetLogCallback(Action<MothershipLogLevel, string> callback)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return;
		}
		logCallback = new MothershipLogCallback(callback);
		client.SetLogDelegate(logCallback);
	}

	public static void SetAuthRefreshedCallback(Action<string> callback)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return;
		}
		authRefreshRequiredCallback = new MothershipAuthRefreshRequiredCallback(callback);
		client.SetAuthRefreshRequiredDelegateWrapper(authRefreshRequiredCallback);
	}

	public static bool LogInWithInsecure1(string Username, string AccountId, Action<LoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<LoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.LoginWithInsecure1(Username, AccountId, userData);
	}

	public static bool LogInWithQuest(string nonce, string userId, Action<LoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<LoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.LoginWithQuest(nonce, userId, userData);
	}

	public static bool StartLogInWithQuest(string userId, Action<PlayerQuestBeginLoginV2Response> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<PlayerQuestBeginLoginV2Response>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.BeginQuestV2Auth(userId, userData);
	}

	public static bool CompleteLogInWithQuest(string userId, string attestationToken, string nonce, Action<LoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<LoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.CompleteQuestV2Auth(userId, attestationToken, nonce, userData);
	}

	public static bool LogInWithRift(string nonce, string userId, Action<LoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<LoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.LoginWithRift(nonce, userId, userData);
	}

	public static bool LogInWithGoogle(string token, string userId, Action<LoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<LoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.LoginWithGoogle(token, userId, userData);
	}

	public static bool LogInWithApple(string signature, string gamePlayerId, string teamPlayerId, string certUri, string salt, string timestamp, Action<LoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<LoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.LoginWithApple(signature, gamePlayerId, teamPlayerId, certUri, salt, timestamp, userData);
	}

	public static bool StartLoginWithSteam(Action<PlayerSteamBeginLoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<PlayerSteamBeginLoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.BeginSteamAuth(userData);
	}

	public static bool CompleteLoginWithSteam(string nonce, string steamTicket, Action<LoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<LoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.CompleteSteamAuth(nonce, steamTicket, userData);
	}

	public static bool LogInWithSynthesisVR(long DeviceId, Action<LoginResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled)
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<LoginResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.LoginWithSynthesisVR(DeviceId, userData);
	}

	public static bool GetUserDataValue(string keyName, Action<MothershipUserData> successAction, Action<MothershipError, int> errorAction, string targetId = "")
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<MothershipUserData>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		string userId = (string.IsNullOrEmpty(targetId) ? MothershipClientContext.MothershipId : targetId);
		return client.GetUserData(MothershipClientContext.MothershipId, userId, keyName, "", userData);
	}

	public static bool SetUserDataValue(string keyName, string value, Action<SetUserDataResponse> successAction, Action<MothershipError, int> errorAction, string targetId = "")
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<SetUserDataResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		string userId = (string.IsNullOrEmpty(targetId) ? MothershipClientContext.MothershipId : targetId);
		return client.SetUserData(MothershipClientContext.MothershipId, userId, keyName, value, -1, userData);
	}

	public static bool CreateReport(string reportedUserId, int category, bool moddedClient, string metadata, Action<CreateReportResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<CreateReportResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		string platform = "STEAM";
		return client.CreateReport(MothershipClientContext.MothershipId, reportedUserId, category, platform, moddedClient, metadata, userData);
	}

	public static bool GetUserInventory(Action<MothershipGetInventoryResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<MothershipGetInventoryResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.GetUserInventory(MothershipClientContext.MothershipId, userData);
	}

	public static bool GetUserInventory(string TargetPlayerMothershipId, Action<MothershipGetMergedInventoryResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<MothershipGetMergedInventoryResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.GetMergedInventory(MothershipClientContext.MothershipId, TargetPlayerMothershipId, userData);
	}

	public static bool GetStorefront(string[] offerDisplays, Action<MothershipGetStorefrontResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<MothershipGetStorefrontResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.GetStorefront(MothershipClientContext.MothershipId, new StringVector(offerDisplays), userData);
	}

	public static bool PurchaseOffer(string offerDisplayId, string offerId, int displayIndex, Action<MothershipPurchaseOfferResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<MothershipPurchaseOfferResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.PurchaseOffer(MothershipClientContext.MothershipId, offerDisplayId, offerId, displayIndex, userData);
	}

	public static bool ConsumeConsumable(string entitlementId, Action<MothershipConsumeConsumableResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<MothershipConsumeConsumableResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.ConsumeConsumable(MothershipClientContext.MothershipId, entitlementId, userData);
	}

	public static bool RefreshMetaIAP(Action<MothershipRefreshIAPResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<MothershipRefreshIAPResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.RefreshIAP(MothershipClientContext.MothershipId, userData);
	}

	public static bool InitSteamPurchase(string displayId, string offerId, int displayIndex, Action<InitSteamPurchaseResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<InitSteamPurchaseResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.InitSteamPurchase(MothershipClientContext.MothershipId, displayId, offerId, displayIndex, userData);
	}

	public static bool FinalizeSteamPurchase(string steamOrderId, Action<FinalizeSteamPurchaseResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<FinalizeSteamPurchaseResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.FinalizeSteamPurchase(MothershipClientContext.MothershipId, steamOrderId, userData);
	}

	public static bool OpenNotificationsSocket()
	{
		if (notificationWrapper.SocketState == WebSocketState.Open)
		{
			websocket.RefreshClientTokenHeaders();
			return false;
		}
		return client.OpenNotificationsSocket(MothershipClientContext.MothershipId, IntPtr.Zero);
	}

	public static void CloseWebSockets()
	{
		websocket?.CloseConnections();
	}

	public static void TickWebSockets(float deltaTime)
	{
		websocket?.TickWebSockets(deltaTime);
	}

	private static void InvokeOpenNotificationSocket(nint userData)
	{
		MothershipClientApiUnity.OnOpenNotificationSocket?.Invoke(userData);
		Debug.Log("NOTIFICATIONS opened");
	}

	private static void InvokeMessageNotificationSocket(NotificationsMessageResponse notification, nint userData)
	{
		MothershipClientApiUnity.OnMessageNotificationSocket?.Invoke(notification, userData);
		Debug.Log("NOTIFICATIONS messaged \n" + notification.Title + "\n" + notification.Body);
	}

	private static void InvokeCloseNotificationSocket(nint userData)
	{
		MothershipClientApiUnity.OnCloseNotificationSocket?.Invoke(userData);
		Debug.Log("NOTIFICATIONS closed");
	}

	private static void InvokeErrorNotificationSocket(nint userData)
	{
		MothershipClientApiUnity.OnErrorNotificationSocket?.Invoke(userData);
		Debug.Log("NOTIFICATIONS errored");
	}

	public static bool GetPlayerProgressionData(Action<GetProgressionTrackValuesForPlayerResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<GetProgressionTrackValuesForPlayerResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.GetProgressionTrackValuesForPlayer(MothershipClientContext.MothershipId, userData);
	}

	public static bool GetPlayerProgressionTreesData(Action<GetProgressionTreesForPlayerResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<GetProgressionTreesForPlayerResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.GetProgressionTreesForPlayer(MothershipClientContext.MothershipId, userData);
	}

	public static bool WriteEvents(string callerId, MothershipWriteEventsRequest req, Action<MothershipWriteEventsResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<MothershipWriteEventsResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.WriteEvents(callerId, req, userData);
	}

	public static bool ListMothershipTitleData(string titleId, string envId, string deploymentId, StringVector keys, Action<ListClientMothershipTitleDataResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<ListClientMothershipTitleDataResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		Debug.Log($"ListMothershipTitleData: {TitleId}, {EnvironmentId}, {DeploymentId}, {keys}");
		return client.ListClientMothershipTitleData(MothershipClientContext.MothershipId, keys, userData);
	}

	public static bool GetAndRefreshMySubscriptions(Action<GetMySubscriptionsResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<GetMySubscriptionsResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.ClientGetMySubscriptions(MothershipClientContext.MothershipId, userData);
	}

	public static bool GetRoomPlayerSubscriptions(string[] playerIds, Action<BulkGetSubscriptionsResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		_ = (IntPtr)GCHandle.Alloc(new CallbackPair<BulkGetSubscriptionsResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return false;
	}

	public static bool InitSteamSubscriptionTransaction(string sku, string frequencyUnit, int frequency, int priceInUSDCents, Action<InitSteamSubscriptionPurchaseResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<InitSteamSubscriptionPurchaseResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.ClientInitSteamSubscriptionPurchase(MothershipClientContext.MothershipId, sku, priceInUSDCents, frequency, frequencyUnit, userData);
	}

	public static bool FinalizeSteamSubscriptionTransaction(string steamOrderId, Action<FinalizeSteamSubscriptionPurchaseResponse> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<FinalizeSteamSubscriptionPurchaseResponse>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.ClientFinalizeSteamSubscriptionPurchase(MothershipClientContext.MothershipId, steamOrderId, userData);
	}

	public static bool GetDLCFileDetails(string fileId, Action<SharedDownloadableFileResult> successAction, Action<MothershipError, int> errorAction)
	{
		if (!isEnabled || !MothershipClientContext.IsClientLoggedIn())
		{
			Debug.LogError("Tried to call a Mothership API, but Mothership is not enabled or the user hasn't ever logged in");
			return false;
		}
		IntPtr userData = (IntPtr)GCHandle.Alloc(new CallbackPair<SharedDownloadableFileResult>
		{
			successCallback = successAction,
			errorCallback = errorAction
		});
		return client.ClientGetFileById(MothershipClientContext.MothershipId, fileId, userData);
	}
}
