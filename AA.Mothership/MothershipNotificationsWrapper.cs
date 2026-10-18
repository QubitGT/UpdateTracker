using System;
using NativeWebSocket;
using UnityEngine;

public class MothershipNotificationsWrapper : NotificationsMessageDelegateWrapper
{
	private WebSocketState _state;

	private readonly Action<nint> _onOpen;

	private readonly Action<NotificationsMessageResponse, nint> _onMessage;

	private readonly Action<nint> _onClose;

	private readonly Action<nint> _onError;

	public WebSocketState SocketState => _state;

	public MothershipNotificationsWrapper(Action<nint> onOpen = null, Action<NotificationsMessageResponse, nint> onMessage = null, Action<nint> onClose = null, Action<nint> onError = null)
	{
		swigCMemOwn = false;
		_onOpen = onOpen;
		_onMessage = onMessage;
		_onClose = onClose;
		_onError = onError;
		_state = WebSocketState.Closed;
	}

	public override void OnOpenCallback(nint userData)
	{
		_state = WebSocketState.Open;
		_onOpen?.Invoke(userData);
	}

	public override void OnMessageCallback(MothershipWebSocketMessage message, nint userData)
	{
		NotificationsMessageResponse notificationsMessageResponse = NotificationsMessageResponse.FromWebSocketMessage(message);
		if (notificationsMessageResponse == null)
		{
			Debug.LogError("Notification message is invalid");
		}
		else
		{
			_onMessage?.Invoke(notificationsMessageResponse, userData);
		}
	}

	public override void OnCloseCallback(nint userData)
	{
		_state = WebSocketState.Closed;
		_onClose?.Invoke(userData);
	}

	public override void OnErrorCallback(nint userData)
	{
		_onError?.Invoke(userData);
	}
}
