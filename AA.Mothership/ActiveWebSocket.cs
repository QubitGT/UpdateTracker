using System;
using NativeWebSocket;

internal class ActiveWebSocket
{
	public WebSocket websocket;

	public MothershipOpenWebSocketEventArgs requestData;

	public Action<MothershipOpenWebSocketEventArgs> resetSocket;
}
