using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NativeWebSocket;
using UnityEngine;

public class MothershipWebSocketWrapper : MothershipWebSocketDelegateWrapper
{
	private readonly MothershipClientApiClient _client;

	private readonly MothershipWebSocketRetryQueue _retryQueue;

	private readonly List<ActiveWebSocket> _websockets;

	public MothershipWebSocketWrapper(MothershipClientApiClient client)
	{
		swigCMemOwn = false;
		_client = client;
		_retryQueue = new MothershipWebSocketRetryQueue();
		_websockets = new List<ActiveWebSocket>();
	}

	public void RefreshClientTokenHeaders()
	{
		foreach (ActiveWebSocket websocket in _websockets)
		{
			websocket.websocket.SetRequestHeader(MothershipApi.MOTHERSHIP_CLIENT_TOKEN_HEADER, MothershipClientContext.Token);
			foreach (MothershipHttpHeader requestHeader in websocket.requestData.RequestHeaders)
			{
				if (requestHeader.Name == MothershipApi.MOTHERSHIP_CLIENT_TOKEN_HEADER)
				{
					requestHeader.Value = MothershipClientContext.Token;
					break;
				}
			}
		}
	}

	public void TickWebSockets(float deltaTime)
	{
		foreach (ActiveWebSocket websocket in _websockets)
		{
			if (websocket.websocket.State == WebSocketState.Open)
			{
				websocket.websocket.DispatchMessageQueue();
			}
		}
		_retryQueue.Tick(deltaTime);
	}

	public override bool CreateConnection(MothershipOpenWebSocketEventArgs request)
	{
		ActiveWebSocket activeWebSocket = _websockets.Find((ActiveWebSocket ws) => ws.requestData.Path == request.Path);
		if (activeWebSocket != null)
		{
			activeWebSocket.resetSocket?.Invoke(request);
			return true;
		}
		ActiveWebSocket aws = new ActiveWebSocket();
		CreateSocket(request);
		return true;
		void CreateSocket(MothershipOpenWebSocketEventArgs req)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (MothershipHttpHeader requestHeader in req.RequestHeaders)
			{
				dictionary.Add(requestHeader.Name, requestHeader.Value);
			}
			WebSocket webSocket = new WebSocket(req.Path, dictionary);
			aws.requestData = req;
			aws.resetSocket = CreateSocket;
			aws.websocket = webSocket;
			webSocket.OnOpen += OnOpen;
			webSocket.OnMessage += OnMessage;
			webSocket.OnClose += OnClose;
			webSocket.OnError += OnError;
			webSocket.Connect();
		}
		void OnClose(WebSocketCloseCode code)
		{
			if (code > WebSocketCloseCode.Normal)
			{
				Debug.Log($"WebSocket retrying due to close code: {code}");
				_retryQueue.RetrySocket(aws);
			}
			else
			{
				MothershipWebSocketResponse response = new MothershipWebSocketResponse
				{
					Event = MothershipWebSocketEvents.CLOSE,
					cbData = aws.requestData.cbData
				};
				_client.ReceiveWebsocketMessage(response);
				_retryQueue.RemoveSocket(aws);
				_websockets.Remove(aws);
			}
		}
		static void OnError(string error)
		{
			Debug.LogError("WebSocket erroring: " + error);
		}
		void OnMessage(byte[] data)
		{
			try
			{
				string @string = Encoding.UTF8.GetString(data);
				MothershipWebSocketResponse response2 = new MothershipWebSocketResponse
				{
					Event = MothershipWebSocketEvents.MESSAGE,
					Message = @string,
					cbData = aws.requestData.cbData
				};
				_client.ReceiveWebsocketMessage(response2);
			}
			catch (Exception arg)
			{
				Debug.LogError($"WebSocket exception in onMessage: {arg}");
			}
		}
		void OnOpen()
		{
			try
			{
				if (!_websockets.Contains(aws))
				{
					_websockets.Add(aws);
					_retryQueue.AddSocket(aws);
				}
				else
				{
					_retryQueue.ResetSocket(aws);
				}
				MothershipWebSocketResponse response3 = new MothershipWebSocketResponse
				{
					Event = MothershipWebSocketEvents.OPEN,
					cbData = aws.requestData.cbData
				};
				_client.ReceiveWebsocketMessage(response3);
			}
			catch (Exception arg2)
			{
				Debug.LogError($"WebSocket exception in OnOpen: {arg2}");
			}
		}
	}

	public override bool CloseConnection(MothershipCloseWebSocketEventArgs request)
	{
		for (int i = 0; i < _websockets.Count; i++)
		{
			ActiveWebSocket activeWebSocket = _websockets[i];
			if (!(request.Path != activeWebSocket.requestData.Path))
			{
				try
				{
					activeWebSocket.websocket.Close();
				}
				catch
				{
					Debug.LogError("WebSocket " + request.Path + " failed to close");
				}
				_retryQueue.RemoveSocket(activeWebSocket);
				_websockets.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public void CloseConnections()
	{
		Task.Run((Func<Task>)CloseConnectionsAsync).GetAwaiter().GetResult();
	}

	private async Task CloseConnectionsAsync()
	{
		foreach (ActiveWebSocket websocket in _websockets)
		{
			try
			{
				MothershipWebSocketResponse response = new MothershipWebSocketResponse
				{
					Event = MothershipWebSocketEvents.CLOSE,
					cbData = websocket.requestData.cbData
				};
				_client.ReceiveWebsocketMessage(response);
				await websocket.websocket.Close();
			}
			catch
			{
				Debug.LogError("WebSockets failed to close");
			}
		}
		_websockets.Clear();
		_retryQueue.ClearSockets();
	}
}
