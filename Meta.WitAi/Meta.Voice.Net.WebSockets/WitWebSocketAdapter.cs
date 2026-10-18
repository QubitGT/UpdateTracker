using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.Voice.Net.Encoding.Wit;
using Meta.Voice.Net.PubSub;
using Meta.WitAi.Attributes;
using Meta.WitAi.Interfaces;
using UnityEngine;
using UnityEngine.Events;

namespace Meta.Voice.Net.WebSockets;

[LogCategory(LogCategory.Network, LogCategory.WebSockets)]
public class WitWebSocketAdapter : MonoBehaviour, IPubSubAdapter, ILogSource, IWitInspectorTools
{
	[ObjectType(typeof(IWitWebSocketClientProvider), new Type[] { })]
	[SerializeField]
	private UnityEngine.Object _webSocketProvider;

	private PubSubSettings _settings;

	private bool _connected;

	private bool _active;

	private ConcurrentDictionary<string, PubSubSubscriptionState> _subscriptionsPerTopic = new ConcurrentDictionary<string, PubSubSubscriptionState>();

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.WebSockets);


	public IWitWebSocketClientProvider WebSocketProvider => _webSocketProvider as IWitWebSocketClientProvider;

	public IWitWebSocketClient WebSocketClient { get; private set; }

	public PubSubSettings Settings
	{
		get
		{
			return _settings;
		}
		set
		{
			SetSettings(value);
		}
	}

	public PubSubSubscriptionState SubscriptionState { get; private set; }

	public UnityEvent OnSubscribed { get; } = new UnityEvent();


	public UnityEvent OnUnsubscribed { get; } = new UnityEvent();


	public event Action<PubSubSubscriptionState> OnTopicSubscriptionStateChange;

	public event WitWebSocketResponseProcessor OnProcessForwardedResponse;

	public event Action<IWitWebSocketRequest> OnRequestGenerated;

	protected virtual void OnEnable()
	{
		_active = true;
		SetClientProvider(WebSocketProvider);
		Connect();
	}

	protected virtual bool RaiseProcessForwardedResponse(string topicId, string requestId, string clientUserId, WitChunk responseChunk)
	{
		if (!Settings.IsSubscribedTopicId(topicId))
		{
			return false;
		}
		return this.OnProcessForwardedResponse?.Invoke(topicId, requestId, clientUserId, responseChunk) ?? false;
	}

	protected virtual void HandleRequestGenerated(string topicId, IWitWebSocketRequest request)
	{
		if (Settings.IsSubscribedTopicId(topicId))
		{
			this.OnRequestGenerated?.Invoke(request);
		}
	}

	protected virtual void OnDisable()
	{
		_active = false;
		Disconnect();
	}

	protected virtual void OnDestroy()
	{
		WebSocketClient = null;
		Disconnect();
	}

	public void SetClientProvider(IWitWebSocketClientProvider clientProvider)
	{
		IWitWebSocketClient witWebSocketClient = clientProvider?.WebSocketClient;
		if (WebSocketClient == null || !WebSocketClient.Equals(witWebSocketClient))
		{
			if (_active)
			{
				Disconnect();
			}
			_webSocketProvider = clientProvider as UnityEngine.Object;
			WebSocketClient = witWebSocketClient;
			if (clientProvider != null && _webSocketProvider == null)
			{
				Logger.Warning("SetClientProvider failed\nReason: {0} does not inherit from UnityEngine.Object", clientProvider.GetType());
			}
			if (_active)
			{
				Connect();
			}
		}
	}

	private void Connect()
	{
		if (WebSocketClient != null && !_connected)
		{
			_connected = true;
			WebSocketClient.OnTopicSubscriptionStateChange += ApplySubscriptionPerTopic;
			WebSocketClient.OnProcessForwardedResponse += RaiseProcessForwardedResponse;
			WebSocketClient.OnTopicRequestTracked += HandleRequestGenerated;
			WebSocketClient.Connect();
			Subscribe();
		}
	}

	private void Disconnect()
	{
		if (WebSocketClient != null && _connected)
		{
			Unsubscribe();
			_connected = false;
			WebSocketClient.Disconnect();
			WebSocketClient.OnTopicSubscriptionStateChange -= ApplySubscriptionPerTopic;
			WebSocketClient.OnProcessForwardedResponse -= RaiseProcessForwardedResponse;
			WebSocketClient.OnTopicRequestTracked -= HandleRequestGenerated;
		}
	}

	public void SendRequest(IWitWebSocketRequest request)
	{
		request.TopicId = Settings.PubSubTopicId;
		request.PublishOptions = Settings.PublishOptions;
		WebSocketClient.SendRequest(request);
	}

	public void SetSettings(PubSubSettings settings)
	{
		if (!Settings.Equals(settings))
		{
			Unsubscribe();
			Logger.Verbose("Topic set to {0}\nFrom: {1}", settings.PubSubTopicId ?? "Null", Settings.PubSubTopicId ?? "Null", null, null, "SetSettings", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketAdapter.cs", 241);
			_settings = settings;
			Subscribe();
		}
	}

	private void Unsubscribe()
	{
		string pubSubTopicId = Settings.PubSubTopicId;
		if (string.IsNullOrEmpty(pubSubTopicId) || !_connected)
		{
			return;
		}
		Logger.Verbose("Unsubscribe from topic: {0}", pubSubTopicId, null, null, null, "Unsubscribe", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketAdapter.cs", 261);
		Dictionary<string, string> subscribeTopics = Settings.GetSubscribeTopics();
		foreach (string value in subscribeTopics.Values)
		{
			ApplySubscriptionPerTopic(value, PubSubSubscriptionState.Unsubscribing);
		}
		foreach (string value2 in subscribeTopics.Values)
		{
			WebSocketClient.Unsubscribe(value2);
			ApplySubscriptionPerTopic(value2, PubSubSubscriptionState.NotSubscribed);
		}
		_subscriptionsPerTopic.Clear();
	}

	private void Subscribe()
	{
		string pubSubTopicId = Settings.PubSubTopicId;
		if (string.IsNullOrEmpty(pubSubTopicId) || !_connected)
		{
			return;
		}
		Logger.Verbose("Subscribe to topic: {0}", pubSubTopicId, null, null, null, "Subscribe", ".\\Library\\PackageCache\\com.meta.xr.sdk.voice@d3f6f37b8e1c\\Lib\\Wit.ai\\Scripts\\Runtime\\Net\\WebSockets\\WitWebSocketAdapter.cs", 291);
		foreach (string value in Settings.GetSubscribeTopics().Values)
		{
			ApplySubscriptionPerTopic(value, PubSubSubscriptionState.Subscribing);
			WebSocketClient.Subscribe(value);
		}
	}

	protected virtual void ApplySubscriptionPerTopic(string topicId, PubSubSubscriptionState subscriptionState)
	{
		if (Settings.IsSubscribedTopicId(topicId) && (!_subscriptionsPerTopic.ContainsKey(topicId) || _subscriptionsPerTopic[topicId] != subscriptionState))
		{
			_subscriptionsPerTopic[topicId] = subscriptionState;
			RefreshSubscriptionState();
		}
	}

	private void RefreshSubscriptionState()
	{
		SetSubscriptionState(DetermineSubscriptionState());
	}

	protected PubSubSubscriptionState DetermineSubscriptionState()
	{
		PubSubSubscriptionState result = PubSubSubscriptionState.NotSubscribed;
		bool flag = _subscriptionsPerTopic.Keys.Count > 0;
		foreach (string key in _subscriptionsPerTopic.Keys)
		{
			if (_subscriptionsPerTopic.TryGetValue(key, out var value))
			{
				if (value == PubSubSubscriptionState.SubscribeError || value == PubSubSubscriptionState.UnsubscribeError)
				{
					return value;
				}
				if (flag && value != PubSubSubscriptionState.Subscribed)
				{
					flag = false;
				}
				if (value == PubSubSubscriptionState.Subscribing || value == PubSubSubscriptionState.Unsubscribing)
				{
					result = value;
				}
			}
		}
		if (flag)
		{
			return PubSubSubscriptionState.Subscribed;
		}
		return result;
	}

	private void SetSubscriptionState(PubSubSubscriptionState newSubState)
	{
		if (SubscriptionState != newSubState)
		{
			SubscriptionState = newSubState;
			this.OnTopicSubscriptionStateChange?.Invoke(SubscriptionState);
			if (SubscriptionState == PubSubSubscriptionState.Subscribed)
			{
				OnSubscribed?.Invoke();
			}
			else if (SubscriptionState == PubSubSubscriptionState.NotSubscribed)
			{
				OnUnsubscribed?.Invoke();
			}
		}
	}
}
