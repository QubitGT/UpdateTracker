using System;
using System.Threading.Tasks;

namespace Meta.Voice.Net.WebSockets;

public interface IWebSocket
{
	WitWebSocketConnectionState State { get; }

	event Action OnOpen;

	event Action<byte[], int, int> OnMessage;

	event Action<string> OnError;

	event Action<WebSocketCloseCode> OnClose;

	Task Connect();

	Task Send(byte[] data);

	Task Close();
}
