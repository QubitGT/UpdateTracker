using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.Voice.Net.Encoding.Wit;
using Meta.Voice.Net.PubSub;
using Meta.Voice.Net.WebSockets.Requests;
using Meta.WitAi;
using Meta.WitAi.Configuration;
using Meta.WitAi.Json;
using Meta.WitAi.Requests;
using UnityEngine;

namespace Meta.Voice.Net.WebSockets;

[LogCategory(LogCategory.Network)]
public sealed class WitWebSocketClient : IWitWebSocketClient, IPubSubSubscriber, ILogSource
{
	private class PubSubSubscription
	{
		public PubSubSubscriptionState state;

		public int referenceCount;
	}

	public WitRequestOptions Options;

	private string _lastRequestId;

	private int _uploadCount;

	private int _downloadCount;

	private Dictionary<string, IWitWebSocketRequest> _requests = new Dictionary<string, IWitWebSocketRequest>();

	private List<string> _untrackedRequests = new List<string>();

	private IWebSocket _socket;

	private readonly WitChunkConverter _decoder = new WitChunkConverter();

	private Dictionary<string, PubSubSubscription> _subscriptions = new Dictionary<string, PubSubSubscription>();

	public WitWebSocketSettings Settings { get; }

	public string ConnectionRequestId => Options.RequestId;

	public WitWebSocketConnectionState ConnectionState { get; private set; }

	public bool IsAuthenticated { get; private set; }

	public bool IsUploading => _uploadCount > 0;

	public bool IsDownloading => _downloadCount > 0;

	public bool IsReferenced => ReferenceCount > 0;

	public bool IsReconnecting
	{
		get
		{
			if (IsReferenced && ConnectionState == WitWebSocketConnectionState.Disconnected)
			{
				if (Settings.ReconnectAttempts >= 0)
				{
					return FailedConnectionAttempts <= Settings.ReconnectAttempts;
				}
				return true;
			}
			return false;
		}
	}

	public int ReferenceCount { get; private set; }

	public int FailedConnectionAttempts { get; private set; }

	public DateTime LastResponseTime { get; private set; }

	public TaskCompletionSource<bool> ConnectionCompletion { get; private set; } = new TaskCompletionSource<bool>();


	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Network);


	public Dictionary<string, IWitWebSocketRequest> Requests => new Dictionary<string, IWitWebSocketRequest>(_requests);

	public event Action<WitWebSocketConnectionState> OnConnectionStateChanged;

	public event WitWebSocketResponseProcessor OnProcessForwardedResponse;

	public event PubSubTopicSubscriptionDelegate OnTopicSubscriptionStateChange;

	public event Action<string, IWitWebSocketRequest> OnTopicRequestTracked;

	public WitWebSocketClient(WitWebSocketSettings settings)
	{
		Settings = settings;
	}

	public WitWebSocketClient(IWitRequestConfiguration configuration)
		: this(new WitWebSocketSettings(configuration))
	{
	}

	private void SetConnectionState(WitWebSocketConnectionState newConnectionState)
	{
		if (newConnectionState == ConnectionState)
		{
			return;
		}
		ConnectionState = newConnectionState;
		Logger.Info(ConnectionState.ToString(), null, null, null, null, "SetConnectionState", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 174);
		this.OnConnectionStateChanged?.Invoke(ConnectionState);
		if (ConnectionState == WitWebSocketConnectionState.Connected)
		{
			if (!ConnectionCompletion.Task.IsCompleted)
			{
				ConnectionCompletion.SetResult(result: true);
			}
		}
		else if (ConnectionState == WitWebSocketConnectionState.Disconnected)
		{
			TaskCompletionSource<bool> connectionCompletion = ConnectionCompletion;
			ConnectionCompletion = new TaskCompletionSource<bool>();
			if (!connectionCompletion.Task.IsCompleted)
			{
				connectionCompletion.SetResult(result: false);
			}
		}
	}

	public void Connect()
	{
		ReferenceCount++;
		if (IsReferenced)
		{
			ConnectSafely();
		}
	}

	private void ConnectSafely()
	{
		if (ConnectionState != WitWebSocketConnectionState.Connecting && ConnectionState != WitWebSocketConnectionState.Connected)
		{
			SetConnectionState(WitWebSocketConnectionState.Connecting);
			WaitForConnectionTimeout().WrapErrors();
			ThreadUtility.BackgroundAsync(Logger, ConnectAsync).WrapErrors();
		}
	}

	private async Task ConnectAsync()
	{
		try
		{
			Options = new WitRequestOptions(WitConstants.GetUniqueId(), WitRequestSettings.LocalClientUserId, null);
			Dictionary<string, string> headers = WitRequestSettings.GetHeaders(Settings.Configuration, Options, useServerToken: false);
			if (headers.ContainsKey("Authorization"))
			{
				headers.Remove("Authorization");
			}
			_socket = GenerateWebSocket(Settings.ServerUrl, headers);
			_socket.OnOpen += HandleSocketConnected;
			_socket.OnMessage += HandleSocketResponse;
			_socket.OnError += HandleSocketError;
			_socket.OnClose += HandleSocketDisconnect;
			await _socket.Connect();
		}
		catch (OperationCanceledException)
		{
			HandleSetupFailed("timeout");
		}
		catch (Exception arg)
		{
			HandleSetupFailed($"Connection connect error caught\n{arg}");
		}
	}

	private async Task WaitForConnectionTimeout()
	{
		await Task.WhenAny(ConnectionCompletion.Task, Task.Delay(Settings.ServerConnectionTimeoutMs));
		if (_socket != null && _socket.State == WitWebSocketConnectionState.Connecting)
		{
			HandleSetupFailed("timeout");
		}
	}

	private IWebSocket GenerateWebSocket(string url, Dictionary<string, string> headers)
	{
		if (Settings.WebSocketProvider != null)
		{
			IWebSocket webSocket = Settings.WebSocketProvider.GetWebSocket(url, headers);
			if (webSocket != null)
			{
				return webSocket;
			}
		}
		return new NativeWebSocketWrapper(url, headers);
	}

	private void HandleSocketError(string errorMessage)
	{
		if (ConnectionState == WitWebSocketConnectionState.Connecting)
		{
			HandleSetupFailed(errorMessage);
			return;
		}
		Logger.Warning("Socket Error\nMessage: {0}", errorMessage);
	}

	private void HandleSocketConnected()
	{
		if (ConnectionState != WitWebSocketConnectionState.Connecting)
		{
			HandleSetupFailed($"State changed to {ConnectionState} during connection.");
		}
		else if (_socket == null)
		{
			HandleSetupFailed("WebSocket client no longer exists.");
		}
		else if (_socket.State != WitWebSocketConnectionState.Connected)
		{
			HandleSetupFailed($"Socket is {_socket.State}");
		}
		else if (ConnectionState != WitWebSocketConnectionState.Connected)
		{
			ThreadUtility.BackgroundAsync(Logger, SetupAsync).WrapErrors();
		}
	}

	private async Task SetupAsync()
	{
		string text = Settings?.Configuration?.GetClientAccessToken();
		if (string.IsNullOrEmpty(text))
		{
			HandleSetupFailed("Cannot connect to Wit server without client access token");
			return;
		}
		string versionTag = Settings?.Configuration?.GetVersionTag();
		Dictionary<string, string> parameters = Settings?.AdditionalAuthParameters;
		WitWebSocketAuthRequest request = new WitWebSocketAuthRequest(text, versionTag, parameters);
		string text2 = await SendRequestAsync(request);
		IsAuthenticated = string.IsNullOrEmpty(text2);
		if (!IsAuthenticated)
		{
			Settings.ReconnectAttempts = 0;
			HandleSetupFailed(text2);
			return;
		}
		if (ConnectionState != WitWebSocketConnectionState.Connecting)
		{
			HandleSetupFailed($"State changed to {ConnectionState} during authentication.");
			return;
		}
		FailedConnectionAttempts = 0;
		SetConnectionState(WitWebSocketConnectionState.Connected);
		string[] array = _subscriptions.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Subscribe(array[i], ignoreRefCount: true);
		}
	}

	private void HandleSetupFailed(string error)
	{
		if (ConnectionState == WitWebSocketConnectionState.Connecting)
		{
			Logger.Error("Connection Failed\nConnection Request Id: {0}\nMessage: {1}", Options.RequestId, error);
			FailedConnectionAttempts++;
			if (Settings.ReconnectAttempts >= 0 && FailedConnectionAttempts > Settings.ReconnectAttempts)
			{
				Logger.Error("Connection Refused\nConnection Request Id: {0}\nMessage: {1}\nFailed Attempts: {2}", Options.RequestId, error, FailedConnectionAttempts);
			}
			else
			{
				Logger.Warning("Connection Refused - Will Retry\nConnection Request Id: {0}\nMessage: {1}\nFailed Attempts: {2}", Options.RequestId, error, FailedConnectionAttempts);
			}
			ForceDisconnect();
		}
		else
		{
			Logger.Warning("Connection Cancelled\nConnection Request Id: {0}\nMessage: {1}", Options.RequestId, error);
		}
	}

	private void HandleSocketDisconnect(WebSocketCloseCode closeCode)
	{
		if (ConnectionState == WitWebSocketConnectionState.Connected)
		{
			Logger.Warning("Socket Closed\nConnection Request Id: {0}\nReason: {1}", Options.RequestId, closeCode);
			ForceDisconnect();
		}
	}

	public void Disconnect()
	{
		ReferenceCount--;
		if (!IsReferenced)
		{
			ReferenceCount = 0;
			ForceDisconnect();
		}
	}

	public void ForceDisconnect()
	{
		if (ConnectionState != WitWebSocketConnectionState.Disconnecting && ConnectionState != 0)
		{
			DisconnectAsync().WrapErrors();
		}
	}

	private async Task DisconnectAsync()
	{
		SetConnectionState(WitWebSocketConnectionState.Disconnecting);
		await BreakdownAsync();
		if (ConnectionState != WitWebSocketConnectionState.Disconnecting)
		{
			Logger.Warning("State changed to {0} during breakdown.", ConnectionState);
			return;
		}
		SetConnectionState(WitWebSocketConnectionState.Disconnected);
		if (IsReferenced)
		{
			Reconnect();
		}
	}

	private async Task BreakdownAsync()
	{
		IsAuthenticated = false;
		string[] array = _requests.Keys.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			UntrackRequest(array[i]);
		}
		lock (_requests)
		{
			_untrackedRequests.Clear();
		}
		string[] array2 = _subscriptions.Keys.ToArray();
		foreach (string topicId in array2)
		{
			PubSubSubscriptionState topicSubscriptionState = GetTopicSubscriptionState(topicId);
			if (topicSubscriptionState == PubSubSubscriptionState.Subscribing || topicSubscriptionState == PubSubSubscriptionState.Subscribed || topicSubscriptionState == PubSubSubscriptionState.SubscribeError)
			{
				Subscribe(topicId, ignoreRefCount: true);
			}
			else
			{
				Unsubscribe(topicId, ignoreRefCount: true);
			}
		}
		if (_socket != null)
		{
			_socket.OnOpen -= HandleSocketConnected;
			_socket.OnMessage -= HandleSocketResponse;
			_socket.OnError -= HandleSocketError;
			_socket.OnClose -= HandleSocketDisconnect;
			try
			{
				await _socket.Close();
			}
			catch (Exception ex)
			{
				Logger.Error("Close Socket Failed\n{0}", ex);
			}
			_socket = null;
		}
		_uploadCount = 0;
		_downloadCount = 0;
	}

	private void Reconnect()
	{
		if (IsReferenced && ConnectionState == WitWebSocketConnectionState.Disconnected)
		{
			if (Settings.ReconnectAttempts >= 0 && FailedConnectionAttempts > Settings.ReconnectAttempts)
			{
				Logger.Error("Reconnect Failed\nToo many failed reconnect attempts\nFailures: {0}\nAttempts Allowed: {1}", FailedConnectionAttempts, Settings.ReconnectAttempts);
			}
			else
			{
				ThreadUtility.BackgroundAsync(Logger, WaitAndConnect).WrapErrors();
			}
		}
	}

	private async Task WaitAndConnect()
	{
		await Task.Delay(Mathf.Max(100, Mathf.RoundToInt(Settings.ReconnectInterval * 1000f)));
		if (IsReconnecting)
		{
			Logger.Info($"Reconnect Attempt {FailedConnectionAttempts}", null, null, null, null, "WaitAndConnect", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 646);
			ConnectSafely();
		}
	}

	public bool SendRequest(IWitWebSocketRequest request)
	{
		if (!TrackRequest(request))
		{
			return false;
		}
		request.HandleUpload(SendChunk);
		return true;
	}

	public async Task<string> SendRequestAsync(IWitWebSocketRequest request)
	{
		if (!TrackRequest(request))
		{
			return "Request is already tracked";
		}
		ThreadUtility.Background(Logger, delegate
		{
			request.HandleUpload(SendChunk);
		}).WrapErrors();
		await request.Completion.Task;
		return request.Error;
	}

	private void SendChunk(string requestId, WitResponseNode requestJsonData, byte[] requestBinaryData)
	{
		ThreadUtility.BackgroundAsync(Logger, () => SendChunkAsync(requestId, requestJsonData, requestBinaryData));
	}

	private async Task SendChunkAsync(string requestId, WitResponseNode requestJsonData, byte[] requestBinaryData)
	{
		if (!_requests.TryGetValue(requestId, out var value) || !(value is WitWebSocketAuthRequest))
		{
			await ConnectionCompletion.Task;
			if (ConnectionState != WitWebSocketConnectionState.Connected)
			{
				return;
			}
		}
		else if (ConnectionState != WitWebSocketConnectionState.Connecting)
		{
			return;
		}
		if (!_requests.TryGetValue(requestId, out var request))
		{
			return;
		}
		_uploadCount++;
		if (requestJsonData == null)
		{
			requestJsonData = new WitResponseClass();
		}
		requestJsonData["client_request_id"] = requestId;
		WitChunk witChunk = default(WitChunk);
		witChunk.jsonString = requestJsonData?.ToString();
		witChunk.jsonData = requestJsonData;
		witChunk.binaryData = requestBinaryData;
		WitChunk chunk = witChunk;
		byte[] array = EncodeChunk(chunk);
		if (array != null)
		{
			if (Settings.VerboseJsonLogging)
			{
				Logger.Verbose("Upload Chunk:\n{0}\n", chunk.jsonString, null, null, null, "SendChunkAsync", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 750);
			}
			await _socket.Send(array);
		}
		TrySimulateError(request);
		_uploadCount--;
	}

	private void TrySimulateError(IWitWebSocketRequest request)
	{
		if (request.SimulatedErrorType == VoiceErrorSimulationType.Disconnect)
		{
			Logger.Info("[DEBUG] Simulating Abnormal Disconnect\nState: {0}\nRequest: {1}", ConnectionState, request.RequestId, null, null, "TrySimulateError", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 769);
			HandleSocketDisconnect(WebSocketCloseCode.Abnormal);
		}
		else if (request.SimulatedErrorType == VoiceErrorSimulationType.Server)
		{
			Logger.Info("[DEBUG] Simulating Server Error\nRequest: {0}", request.RequestId, null, null, null, "TrySimulateError", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 777);
			WitResponseClass witResponseClass = new WitResponseClass();
			witResponseClass["client_request_id"] = new WitResponseData(request.RequestId);
			witResponseClass["client_user_id"] = new WitResponseData(request.ClientUserId);
			witResponseClass["operation_id"] = new WitResponseData(request.OperationId);
			witResponseClass["code"] = new WitResponseData(500);
			witResponseClass["error"] = new WitResponseData("Simulated Server Error");
			request.HandleDownload(witResponseClass.ToString(), witResponseClass, null);
		}
	}

	private byte[] EncodeChunk(WitChunk chunk)
	{
		return WitChunkConverter.Encode(chunk);
	}

	private void HandleSocketResponse(byte[] rawBytes, int offset, int length)
	{
		_downloadCount++;
		_decoder.Decode(rawBytes, offset, length, ApplyDecodedChunk);
		_downloadCount--;
	}

	private void ApplyDecodedChunk(WitChunk chunk)
	{
		if (Settings.VerboseJsonLogging)
		{
			Logger.Verbose("Downloaded Chunk:\n{0}\n", chunk.jsonString, null, null, null, "ApplyDecodedChunk", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 816);
		}
		WitResponseNode witResponseNode = chunk.jsonData?["client_request_id"];
		if (string.IsNullOrEmpty(witResponseNode))
		{
			if (string.IsNullOrEmpty(_lastRequestId))
			{
				Logger.Error("Download Chunk Failed\nError: no request id found in chunk\nJson: {0}", chunk.jsonString ?? "Null");
				return;
			}
			witResponseNode = _lastRequestId;
			if (chunk.jsonData == null)
			{
				chunk.jsonData = new WitResponseClass();
				chunk.jsonData["client_request_id"] = witResponseNode;
			}
		}
		else
		{
			_lastRequestId = witResponseNode;
		}
		if (!_requests.TryGetValue(witResponseNode, out var value))
		{
			ProcessForwardedResponse(witResponseNode, chunk);
			_requests.TryGetValue(witResponseNode, out value);
		}
		if (value == null || value.SimulatedErrorType != (VoiceErrorSimulationType)(-1))
		{
			return;
		}
		try
		{
			value.HandleDownload(chunk.jsonString, chunk.jsonData, chunk.binaryData);
		}
		catch (Exception ex)
		{
			Logger.Error("Request HandleDownload method exception caught\n{0}\n\n{1}\n", value, ex);
			UntrackRequest(value);
		}
	}

	private void ProcessForwardedResponse(string requestId, WitChunk chunk)
	{
		if (_untrackedRequests.Contains(requestId))
		{
			Logger.Verbose("Process Forwarded Response - Ignored\nReason: Request has been cancelled\nRequest Id: {0}\nJson:\n{1}", requestId, chunk.jsonString ?? "Null", null, null, "ProcessForwardedResponse", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 884);
			return;
		}
		string value = chunk.jsonData["topic"].Value;
		if (string.IsNullOrEmpty(value))
		{
			Logger.Warning("Process Forwarded Response - Failed\nReason: No topic id provided in response\nRequest Id: {0}\nJson:\n{1}", requestId, chunk.jsonString ?? "Null");
			return;
		}
		PubSubSubscriptionState topicSubscriptionState = GetTopicSubscriptionState(value);
		if (topicSubscriptionState != PubSubSubscriptionState.Subscribed && topicSubscriptionState != PubSubSubscriptionState.Subscribing)
		{
			Logger.Warning("Process Forwarded Response - Failed\nReason: Topic id is not currently subscribed to\nTopic Id: {0}\nRequest Id: {1}\nJson:\n{2}", value, requestId, chunk.jsonString ?? "Null");
			return;
		}
		string text = chunk.jsonData["client_user_id"].Value;
		if (string.IsNullOrEmpty(text))
		{
			text = "unknown";
		}
		bool flag = false;
		if (this.OnProcessForwardedResponse != null)
		{
			Delegate[] invocationList = this.OnProcessForwardedResponse.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (invocationList[i] is WitWebSocketResponseProcessor witWebSocketResponseProcessor)
				{
					flag |= witWebSocketResponseProcessor(value, requestId, text, chunk);
				}
			}
		}
		if (!flag)
		{
			Logger.Warning("Process Forwarded Response - Ignored\nReason: No OnProcessForwardedResponse events handled the response\nTopic Id: {0}\nRequest Id: {1}\nClient User Id: {2}", value, requestId, text);
		}
	}

	public bool TrackRequest(IWitWebSocketRequest request)
	{
		if (request == null)
		{
			return false;
		}
		lock (_requests)
		{
			if (_requests.ContainsValue(request))
			{
				return false;
			}
			_requests[request.RequestId] = request;
		}
		request.TimeoutMs = ((request.TimeoutMs > 0) ? request.TimeoutMs : Settings.RequestTimeoutMs);
		request.OnComplete = (Action<IWitWebSocketRequest>)Delegate.Combine(request.OnComplete, new Action<IWitWebSocketRequest>(CompleteRequestTracking));
		Logger.Info($"Track Request\n{request}", null, null, null, null, "TrackRequest", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 962);
		string topicId = request.TopicId;
		if (GetTopicSubscriptionState(topicId) != 0)
		{
			this.OnTopicRequestTracked?.Invoke(topicId, request);
		}
		return true;
	}

	private void CompleteRequestTracking(IWitWebSocketRequest request)
	{
		UntrackRequest(request);
		if (request is WitWebSocketSubscriptionRequest request2)
		{
			FinalizeSubscription(request2);
		}
	}

	public bool UntrackRequest(IWitWebSocketRequest request)
	{
		if (request == null)
		{
			return false;
		}
		return UntrackRequest(request.RequestId);
	}

	public bool UntrackRequest(string requestId)
	{
		if (string.IsNullOrEmpty(requestId))
		{
			return false;
		}
		IWitWebSocketRequest witWebSocketRequest;
		lock (_requests)
		{
			if (!_requests.ContainsKey(requestId))
			{
				return false;
			}
			witWebSocketRequest = _requests[requestId];
			_requests.Remove(requestId);
			_untrackedRequests.Add(requestId);
		}
		witWebSocketRequest.OnComplete = (Action<IWitWebSocketRequest>)Delegate.Remove(witWebSocketRequest.OnComplete, new Action<IWitWebSocketRequest>(CompleteRequestTracking));
		if (!witWebSocketRequest.IsComplete)
		{
			if (ConnectionState == WitWebSocketConnectionState.Disconnecting || ConnectionState == WitWebSocketConnectionState.Disconnected)
			{
				WitResponseClass witResponseClass = new WitResponseClass();
				witResponseClass["client_request_id"] = new WitResponseData(witWebSocketRequest.RequestId);
				witResponseClass["code"] = new WitResponseData(499);
				witResponseClass["error"] = new WitResponseData("WebSocket disconnected");
				witWebSocketRequest.HandleDownload(witResponseClass.ToString(), witResponseClass, null);
			}
			else
			{
				witWebSocketRequest.Cancel();
			}
		}
		Logger.Info($"Untrack Request\n{witWebSocketRequest}", null, null, null, null, "UntrackRequest", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 1043);
		return true;
	}

	public PubSubSubscriptionState GetTopicSubscriptionState(string topicId)
	{
		if (!string.IsNullOrEmpty(topicId) && _subscriptions.TryGetValue(topicId, out var value))
		{
			return value.state;
		}
		return PubSubSubscriptionState.NotSubscribed;
	}

	public void Subscribe(string topicId)
	{
		Subscribe(topicId, ignoreRefCount: false);
	}

	private void Subscribe(string topicId, bool ignoreRefCount)
	{
		if (!string.IsNullOrEmpty(topicId))
		{
			if (!_subscriptions.TryGetValue(topicId, out var value))
			{
				value = new PubSubSubscription();
			}
			if (!ignoreRefCount)
			{
				value.referenceCount++;
			}
			if (ConnectionState != WitWebSocketConnectionState.Connected && ConnectionState != WitWebSocketConnectionState.Connecting)
			{
				SetTopicSubscriptionState(value, topicId, PubSubSubscriptionState.SubscribeError, "Not connected.  Will retry once connected.");
			}
			else if (value.state != PubSubSubscriptionState.Subscribing && value.state != PubSubSubscriptionState.Subscribed)
			{
				SetTopicSubscriptionState(value, topicId, PubSubSubscriptionState.Subscribing);
				WitWebSocketSubscriptionRequest request = new WitWebSocketSubscriptionRequest(topicId, WitWebSocketSubscriptionType.Subscribe);
				SendRequest(request);
			}
		}
	}

	public void Unsubscribe(string topicId)
	{
		Unsubscribe(topicId, ignoreRefCount: false);
	}

	public void Unsubscribe(string topicId, bool ignoreRefCount)
	{
		if (string.IsNullOrEmpty(topicId) || !_subscriptions.TryGetValue(topicId, out var value))
		{
			return;
		}
		if (!ignoreRefCount)
		{
			value.referenceCount = Mathf.Max(0, value.referenceCount - 1);
		}
		if (value.referenceCount <= 0)
		{
			if (ConnectionState != WitWebSocketConnectionState.Connected)
			{
				SetTopicSubscriptionState(value, topicId, PubSubSubscriptionState.Unsubscribing);
				SetTopicSubscriptionState(value, topicId, PubSubSubscriptionState.NotSubscribed);
			}
			else if (value.state != PubSubSubscriptionState.Unsubscribing && value.state != 0)
			{
				SetTopicSubscriptionState(value, topicId, PubSubSubscriptionState.Unsubscribing);
				WitWebSocketSubscriptionRequest request = new WitWebSocketSubscriptionRequest(topicId, WitWebSocketSubscriptionType.Unsubscribe);
				SendRequest(request);
			}
		}
	}

	private void FinalizeSubscription(WitWebSocketSubscriptionRequest request)
	{
		string topicId = request.TopicId;
		if (_subscriptions.TryGetValue(topicId, out var value))
		{
			bool flag = request.SubscriptionType == WitWebSocketSubscriptionType.Subscribe;
			if (!string.IsNullOrEmpty(request.Error))
			{
				PubSubSubscriptionState state = (flag ? PubSubSubscriptionState.SubscribeError : PubSubSubscriptionState.UnsubscribeError);
				SetTopicSubscriptionState(value, topicId, state, request.Error);
				WaitAndRetry(flag, topicId).WrapErrors();
			}
			else
			{
				PubSubSubscriptionState state2 = (flag ? PubSubSubscriptionState.Subscribed : PubSubSubscriptionState.NotSubscribed);
				SetTopicSubscriptionState(value, topicId, state2);
			}
		}
	}

	private async Task WaitAndRetry(bool subscribing, string topicId)
	{
		await Task.Delay(10);
		if (subscribing)
		{
			Subscribe(topicId, ignoreRefCount: true);
		}
		else
		{
			Unsubscribe(topicId, ignoreRefCount: true);
		}
	}

	private void SetTopicSubscriptionState(PubSubSubscription subscription, string topicId, PubSubSubscriptionState state, string error = null)
	{
		if (subscription.state != state)
		{
			subscription.state = state;
			if (state == PubSubSubscriptionState.NotSubscribed)
			{
				_subscriptions.Remove(topicId);
			}
			else
			{
				_subscriptions[topicId] = subscription;
			}
			if (!string.IsNullOrEmpty(error))
			{
				Logger.Warning("Set State Failed\nState: {0}\nError: {1}\nTopic Id: {2}", state, error, topicId);
			}
			else
			{
				Logger.Info($"{state}\nTopic Id: {topicId}", null, null, null, null, "SetTopicSubscriptionState", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketClient.cs", 1278);
			}
			this.OnTopicSubscriptionStateChange?.Invoke(topicId, state);
		}
	}
}
