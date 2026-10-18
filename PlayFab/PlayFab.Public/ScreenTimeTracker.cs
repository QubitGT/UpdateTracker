using System;
using System.Collections.Generic;
using PlayFab.EventsModels;
using UnityEngine;

namespace PlayFab.Public;

public class ScreenTimeTracker : IScreenTimeTracker
{
	private Guid focusId;

	private Guid gameSessionID;

	private bool initialFocus = true;

	private bool isSending;

	private DateTime focusOffDateTime = DateTime.UtcNow;

	private DateTime focusOnDateTime = DateTime.UtcNow;

	private Queue<EventContents> eventsRequests = new Queue<EventContents>();

	private EntityKey entityKey = new EntityKey();

	private const string eventNamespace = "com.playfab.events.sessions";

	private const int maxBatchSizeInEvents = 10;

	private PlayFabEventsInstanceAPI eventApi;

	public ScreenTimeTracker()
	{
		eventApi = new PlayFabEventsInstanceAPI(PlayFabSettings.staticPlayer);
	}

	public void ClientSessionStart(string entityId, string entityType, string playFabUserId)
	{
		gameSessionID = Guid.NewGuid();
		entityKey.Id = entityId;
		entityKey.Type = entityType;
		EventContents eventContents = new EventContents();
		eventContents.Name = "client_session_start";
		eventContents.EventNamespace = "com.playfab.events.sessions";
		eventContents.Entity = entityKey;
		eventContents.OriginalTimestamp = DateTime.UtcNow;
		Dictionary<string, object> payload = new Dictionary<string, object>
		{
			{ "UserID", playFabUserId },
			{
				"DeviceType",
				SystemInfo.deviceType
			},
			{
				"DeviceModel",
				SystemInfo.deviceModel
			},
			{
				"OS",
				SystemInfo.operatingSystem
			},
			{ "ClientSessionID", gameSessionID }
		};
		eventContents.Payload = payload;
		eventsRequests.Enqueue(eventContents);
		OnApplicationFocus(isFocused: true);
	}

	public void OnApplicationFocus(bool isFocused)
	{
		EventContents eventContents = new EventContents();
		DateTime utcNow = DateTime.UtcNow;
		eventContents.Name = "client_focus_change";
		eventContents.EventNamespace = "com.playfab.events.sessions";
		eventContents.Entity = entityKey;
		double num = 0.0;
		if (initialFocus)
		{
			focusId = Guid.NewGuid();
		}
		if (isFocused)
		{
			focusOnDateTime = utcNow;
			focusId = Guid.NewGuid();
			if (!initialFocus)
			{
				num = (utcNow - focusOffDateTime).TotalSeconds;
				if (num < 0.0)
				{
					num = 0.0;
				}
			}
		}
		else
		{
			num = (utcNow - focusOnDateTime).TotalSeconds;
			if (num < 0.0)
			{
				num = 0.0;
			}
			focusOffDateTime = utcNow;
		}
		Dictionary<string, object> payload = new Dictionary<string, object>
		{
			{ "FocusID", focusId },
			{ "FocusState", isFocused },
			{ "FocusStateDuration", num },
			{ "EventTimestamp", utcNow },
			{ "ClientSessionID", gameSessionID }
		};
		eventContents.OriginalTimestamp = utcNow;
		eventContents.Payload = payload;
		eventsRequests.Enqueue(eventContents);
		initialFocus = false;
		if (!isFocused)
		{
			Send();
		}
	}

	public void Send()
	{
		if (PlayFabSettings.staticPlayer.IsClientLoggedIn() && !isSending)
		{
			isSending = true;
			WriteEventsRequest writeEventsRequest = new WriteEventsRequest();
			writeEventsRequest.Events = new List<EventContents>();
			while (eventsRequests.Count > 0 && writeEventsRequest.Events.Count < 10)
			{
				EventContents item = eventsRequests.Dequeue();
				writeEventsRequest.Events.Add(item);
			}
			if (writeEventsRequest.Events.Count > 0)
			{
				eventApi.WriteEvents(writeEventsRequest, EventSentSuccessfulCallback, EventSentErrorCallback);
			}
			isSending = false;
		}
	}

	private void EventSentSuccessfulCallback(WriteEventsResponse response)
	{
	}

	private void EventSentErrorCallback(PlayFabError response)
	{
		Debug.LogWarning("Failed to send session data. Error: " + response.GenerateErrorReport());
	}

	public void OnEnable()
	{
	}

	public void OnDisable()
	{
	}

	public void OnDestroy()
	{
	}

	public void OnApplicationQuit()
	{
		Send();
	}
}
