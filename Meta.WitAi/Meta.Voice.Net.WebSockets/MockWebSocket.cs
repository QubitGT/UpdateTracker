using System;
using System.Threading.Tasks;
using Meta.Voice.Net.Encoding.Wit;
using Meta.WitAi.Json;

namespace Meta.Voice.Net.WebSockets;

public class MockWebSocket : IWebSocket
{
	public WitWebSocketConnectionState State { get; set; }

	public event Action OnOpen;

	public event Action<byte[], int, int> OnMessage;

	public event Action<string> OnError;

	public event Action<WebSocketCloseCode> OnClose;

	public event Action HandleConnect;

	public event Action<byte[]> HandleSend;

	public event Action HandleClose;

	public MockWebSocket(bool autoOpen = false)
	{
		if (autoOpen)
		{
			this.HandleConnect = SimulateOpen;
		}
	}

	public void SimulateOpen()
	{
		State = WitWebSocketConnectionState.Connected;
		this.OnOpen?.Invoke();
	}

	public void SimulateResponse(byte[] bytes, int offset, int length)
	{
		this.OnMessage?.Invoke(bytes, offset, length);
	}

	public void SimulateResponse(WitResponseNode jsonData, byte[] binaryData = null)
	{
		byte[] array = WitChunkConverter.Encode(jsonData, binaryData);
		SimulateResponse(array, 0, array.Length);
	}

	public void SimulateError(string error)
	{
		this.OnError?.Invoke(error);
		SimulateClose(WebSocketCloseCode.Abnormal);
	}

	private void SimulateClose(WebSocketCloseCode closeCode)
	{
		State = WitWebSocketConnectionState.Disconnected;
		this.OnClose?.Invoke(closeCode);
	}

	public async Task Connect()
	{
		await Task.Delay(1);
		State = WitWebSocketConnectionState.Connecting;
		this.HandleConnect?.Invoke();
	}

	public async Task Send(byte[] data)
	{
		await Task.Delay(1);
		this.HandleSend?.Invoke(data);
	}

	public Task Close()
	{
		State = WitWebSocketConnectionState.Disconnecting;
		this.HandleClose?.Invoke();
		SimulateClose(WebSocketCloseCode.Normal);
		return Task.CompletedTask;
	}
}
