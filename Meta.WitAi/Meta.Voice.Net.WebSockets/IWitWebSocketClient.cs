using System;
using System.Collections.Generic;
using Meta.Voice.Net.PubSub;

namespace Meta.Voice.Net.WebSockets;

public interface IWitWebSocketClient : IPubSubSubscriber
{
	WitWebSocketSettings Settings { get; }

	WitWebSocketConnectionState ConnectionState { get; }

	bool IsAuthenticated { get; }

	bool IsUploading { get; }

	bool IsDownloading { get; }

	bool IsReferenced { get; }

	bool IsReconnecting { get; }

	int ReferenceCount { get; }

	int FailedConnectionAttempts { get; }

	DateTime LastResponseTime { get; }

	Dictionary<string, IWitWebSocketRequest> Requests { get; }

	event Action<WitWebSocketConnectionState> OnConnectionStateChanged;

	event WitWebSocketResponseProcessor OnProcessForwardedResponse;

	event Action<string, IWitWebSocketRequest> OnTopicRequestTracked;

	void Connect();

	void Disconnect();

	void ForceDisconnect();

	bool SendRequest(IWitWebSocketRequest request);

	bool TrackRequest(IWitWebSocketRequest request);

	bool UntrackRequest(IWitWebSocketRequest request);

	bool UntrackRequest(string requestId);

	void Unsubscribe(string topicId, bool ignoreRefCount);
}
