using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Platform;

public static class Callback
{
	private class RequestCallback
	{
		private Message.Callback messageCallback;

		public RequestCallback()
		{
		}

		public RequestCallback(Message.Callback callback)
		{
			messageCallback = callback;
		}

		public virtual void HandleMessage(Message msg)
		{
			if (messageCallback != null)
			{
				messageCallback(msg);
			}
		}
	}

	private sealed class RequestCallback<T> : RequestCallback
	{
		private Message<T>.Callback callback;

		public RequestCallback(Message<T>.Callback callback)
		{
			this.callback = callback;
		}

		public override void HandleMessage(Message msg)
		{
			if (callback != null)
			{
				if (msg is Message<T>)
				{
					callback((Message<T>)msg);
				}
				else
				{
					Debug.LogError("Unable to handle message: " + msg.GetType());
				}
			}
		}
	}

	private static Dictionary<ulong, Request> requestIDsToRequests = new Dictionary<ulong, Request>();

	private static Dictionary<Message.MessageType, RequestCallback> notificationCallbacks = new Dictionary<Message.MessageType, RequestCallback>();

	private static bool hasRegisteredJoinIntentNotificationHandler = false;

	private static Message latestPendingJoinIntentNotifications;

	internal static void SetNotificationCallback<T>(Message.MessageType type, Message<T>.Callback callback)
	{
		if (callback == null)
		{
			throw new Exception("Cannot provide a null notification callback.");
		}
		notificationCallbacks[type] = new RequestCallback<T>(callback);
		if (type == Message.MessageType.Notification_GroupPresence_JoinIntentReceived)
		{
			FlushJoinIntentNotificationQueue();
		}
	}

	internal static void SetNotificationCallback(Message.MessageType type, Message.Callback callback)
	{
		if (callback == null)
		{
			throw new Exception("Cannot provide a null notification callback.");
		}
		notificationCallbacks[type] = new RequestCallback(callback);
	}

	internal static void AddRequest(Request request)
	{
		if (request.RequestID == 0L)
		{
			Debug.LogError("An unknown error occurred. Request failed.");
		}
		else
		{
			requestIDsToRequests[request.RequestID] = request;
		}
	}

	internal static void RunCallbacks()
	{
		while (true)
		{
			Message message = Message.PopMessage();
			if (message != null)
			{
				HandleMessage(message);
				continue;
			}
			break;
		}
	}

	internal static void RunLimitedCallbacks(uint limit)
	{
		for (int i = 0; i < limit; i++)
		{
			Message message = Message.PopMessage();
			if (message != null)
			{
				HandleMessage(message);
				continue;
			}
			break;
		}
	}

	internal static void OnApplicationQuit()
	{
		requestIDsToRequests.Clear();
		notificationCallbacks.Clear();
	}

	private static void FlushJoinIntentNotificationQueue()
	{
		hasRegisteredJoinIntentNotificationHandler = true;
		if (latestPendingJoinIntentNotifications != null)
		{
			HandleMessage(latestPendingJoinIntentNotifications);
		}
		latestPendingJoinIntentNotifications = null;
	}

	internal static void HandleMessage(Message msg)
	{
		if (msg.RequestID != 0L && requestIDsToRequests.TryGetValue(msg.RequestID, out var value))
		{
			try
			{
				value.HandleMessage(msg);
				return;
			}
			finally
			{
				requestIDsToRequests.Remove(msg.RequestID);
			}
		}
		if (notificationCallbacks.TryGetValue(msg.Type, out var value2))
		{
			value2.HandleMessage(msg);
		}
		else if (!hasRegisteredJoinIntentNotificationHandler && msg.Type == Message.MessageType.Notification_GroupPresence_JoinIntentReceived)
		{
			latestPendingJoinIntentNotifications = msg;
		}
	}
}
