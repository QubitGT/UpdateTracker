namespace NativeWebSocket;

public interface IWebSocket
{
	WebSocketState State { get; }

	event WebSocketOpenEventHandler OnOpen;

	event WebSocketMessageEventHandler OnMessage;

	event WebSocketErrorEventHandler OnError;

	event WebSocketCloseEventHandler OnClose;
}
