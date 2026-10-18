using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Meta.Net.NativeWebSocket;

namespace Meta.Voice.Net.WebSockets;

public class NativeWebSocketWrapper : IWebSocket
{
	private readonly WebSocket _webSocket;

	public WitWebSocketConnectionState State => _webSocket.State switch
	{
		WebSocketState.Connecting => WitWebSocketConnectionState.Connecting, 
		WebSocketState.Open => WitWebSocketConnectionState.Connected, 
		WebSocketState.Closing => WitWebSocketConnectionState.Disconnecting, 
		WebSocketState.Closed => WitWebSocketConnectionState.Disconnected, 
		_ => WitWebSocketConnectionState.Disconnected, 
	};

	public event Action OnOpen;

	public event Action<byte[], int, int> OnMessage;

	public event Action<string> OnError;

	public event Action<WebSocketCloseCode> OnClose;

	public NativeWebSocketWrapper(string url, Dictionary<string, string> headers)
	{
		_webSocket = new WebSocket(url, headers);
		_webSocket.OnOpen += RaiseOpen;
		_webSocket.OnMessage += RaiseMessage;
		_webSocket.OnError += RaiseError;
		_webSocket.OnClose += RaiseClose;
	}

	~NativeWebSocketWrapper()
	{
		_webSocket.OnOpen -= RaiseOpen;
		_webSocket.OnMessage -= RaiseMessage;
		_webSocket.OnError -= RaiseError;
		_webSocket.OnClose -= RaiseClose;
	}

	public async Task Connect()
	{
		await _webSocket.Connect();
	}

	private void RaiseOpen()
	{
		this.OnOpen?.Invoke();
	}

	public async Task Send(byte[] data)
	{
		await _webSocket.Send(data);
	}

	private void RaiseMessage(byte[] data, int offset, int length)
	{
		this.OnMessage?.Invoke(data, offset, length);
	}

	private void RaiseError(string error)
	{
		this.OnError?.Invoke(error);
	}

	public async Task Close()
	{
		await _webSocket.Close();
	}

	private void RaiseClose(Meta.Net.NativeWebSocket.WebSocketCloseCode closeCode)
	{
		this.OnClose?.Invoke((WebSocketCloseCode)closeCode);
	}
}
