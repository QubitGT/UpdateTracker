using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Modio.Errors;
using Newtonsoft.Json;

namespace Modio.Customizations;

internal class SocketConnection : ISocketConnection
{
	private ClientWebSocket webSocket;

	private readonly Mutex _sending = new Mutex();

	private bool closingConnection;

	private Action<WssMessages> Receive { get; set; }

	private Action Disconnect { get; set; }

	public bool Connected()
	{
		ClientWebSocket clientWebSocket = webSocket;
		if (clientWebSocket == null)
		{
			return false;
		}
		return clientWebSocket.State == WebSocketState.Open;
	}

	public async Task<Error> SetupConnection(string url, Action<WssMessages> onReceive, Action onDisconnect)
	{
		ModioLog.Verbose?.Log("[Socket] Setting up connection for WebSocket (" + url + ")");
		if (Connected())
		{
			return Error.None;
		}
		webSocket = new ClientWebSocket();
		webSocket.Options.UseDefaultCredentials = true;
		webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(10.0);
		try
		{
			Uri uri = new Uri(url);
			await webSocket.ConnectAsync(uri, CancellationToken.None);
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log("[Socket] Failed to connect WSS Gateway.\nException: " + ex.Message);
			return new Error(ErrorCode.WSS_NOT_CONNECTED);
		}
		ModioLog.Verbose?.Log("[Socket] WSS Gateway connected");
		Receive = onReceive;
		Disconnect = onDisconnect;
		ReceiveMessages();
		return Error.None;
	}

	public async Task CloseConnection()
	{
		if (!Connected())
		{
			return;
		}
		try
		{
			closingConnection = true;
			await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", CancellationToken.None);
			webSocket.Dispose();
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log("[Socket] Failed to close the WebSocket connection. Exception: " + ex.Message);
		}
		finally
		{
			closingConnection = false;
			webSocket = null;
			ModioLog.Verbose?.Log("[Socket] CLOSED");
		}
	}

	private async void ReceiveMessages()
	{
		byte[] buffer = new byte[4096];
		while (webSocket.State == WebSocketState.Open)
		{
			try
			{
				WebSocketReceiveResult webSocketReceiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
				byte[] array = new byte[webSocketReceiveResult.Count];
				Array.Copy(buffer, array, webSocketReceiveResult.Count);
				string @string = Encoding.UTF8.GetString(array);
				ModioLog.Verbose?.Log("[Socket] RECEIVED [" + webSocketReceiveResult.MessageType.ToString() + ":" + webSocket.State.ToString() + ((webSocketReceiveResult.CloseStatusDescription != null) ? (":\"" + webSocketReceiveResult.CloseStatusDescription + "\"") : "") + "]\nmessage: " + @string);
				if (webSocketReceiveResult.MessageType == WebSocketMessageType.Close || webSocket.State == WebSocketState.CloseReceived)
				{
					Disconnect?.Invoke();
					break;
				}
				if (webSocketReceiveResult.MessageType == WebSocketMessageType.Text)
				{
					WssMessages obj = JsonConvert.DeserializeObject<WssMessages>(@string);
					Receive?.Invoke(obj);
				}
				await Task.Delay(16);
			}
			catch (Exception ex)
			{
				ModioLog.Error?.Log("[Socket] Exception caught during SocketConnection.Receive.\n" + ex.Message + "\nStacktrace: " + ex.StackTrace);
				if (!closingConnection)
				{
					if (Connected())
					{
						await CloseConnection();
					}
					Disconnect?.Invoke();
				}
				break;
			}
		}
	}

	public async Task<Error> SendData(WssMessages message)
	{
		if (!Connected())
		{
			return new Error(ErrorCode.WSS_NOT_CONNECTED);
		}
		_sending.WaitOne();
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
			await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
		}
		catch (Exception ex)
		{
			ModioLog.Error?.Log("[Socket] Failed to send data across the WSS Gateway.\nException: " + ex.Message);
			return new Error(ErrorCode.WSS_FAILED_TO_SEND);
		}
		finally
		{
			_sending.ReleaseMutex();
		}
		return Error.None;
	}
}
