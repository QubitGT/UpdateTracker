using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace NativeWebSocket;

public class WebSocket : IWebSocket
{
	private Uri uri;

	private Dictionary<string, string> headers;

	private List<string> subprotocols;

	private ClientWebSocket m_Socket = new ClientWebSocket();

	private CancellationTokenSource m_TokenSource;

	private CancellationToken m_CancellationToken;

	private readonly object OutgoingMessageLock = new object();

	private readonly object IncomingMessageLock = new object();

	private bool isSending;

	private List<ArraySegment<byte>> sendBytesQueue = new List<ArraySegment<byte>>();

	private List<ArraySegment<byte>> sendTextQueue = new List<ArraySegment<byte>>();

	private List<byte[]> m_MessageList = new List<byte[]>();

	public WebSocketState State
	{
		get
		{
			switch (m_Socket.State)
			{
			case System.Net.WebSockets.WebSocketState.Connecting:
				return WebSocketState.Connecting;
			case System.Net.WebSockets.WebSocketState.Open:
				return WebSocketState.Open;
			case System.Net.WebSockets.WebSocketState.CloseSent:
			case System.Net.WebSockets.WebSocketState.CloseReceived:
				return WebSocketState.Closing;
			case System.Net.WebSockets.WebSocketState.Closed:
				return WebSocketState.Closed;
			default:
				return WebSocketState.Closed;
			}
		}
	}

	public event WebSocketOpenEventHandler OnOpen;

	public event WebSocketMessageEventHandler OnMessage;

	public event WebSocketErrorEventHandler OnError;

	public event WebSocketCloseEventHandler OnClose;

	public WebSocket(string url, Dictionary<string, string> headers = null)
	{
		uri = new Uri(url);
		if (headers == null)
		{
			this.headers = new Dictionary<string, string>();
		}
		else
		{
			this.headers = headers;
		}
		subprotocols = new List<string>();
		string scheme = uri.Scheme;
		if (!scheme.Equals("ws") && !scheme.Equals("wss"))
		{
			throw new ArgumentException("Unsupported protocol: " + scheme);
		}
	}

	public WebSocket(string url, string subprotocol, Dictionary<string, string> headers = null)
	{
		uri = new Uri(url);
		if (headers == null)
		{
			this.headers = new Dictionary<string, string>();
		}
		else
		{
			this.headers = headers;
		}
		subprotocols = new List<string> { subprotocol };
		string scheme = uri.Scheme;
		if (!scheme.Equals("ws") && !scheme.Equals("wss"))
		{
			throw new ArgumentException("Unsupported protocol: " + scheme);
		}
	}

	public WebSocket(string url, List<string> subprotocols, Dictionary<string, string> headers = null)
	{
		uri = new Uri(url);
		if (headers == null)
		{
			this.headers = new Dictionary<string, string>();
		}
		else
		{
			this.headers = headers;
		}
		this.subprotocols = subprotocols;
		string scheme = uri.Scheme;
		if (!scheme.Equals("ws") && !scheme.Equals("wss"))
		{
			throw new ArgumentException("Unsupported protocol: " + scheme);
		}
	}

	public void SetRequestHeader(string name, string value)
	{
		headers[name] = value;
	}

	public void CancelConnection()
	{
		m_TokenSource?.Cancel();
	}

	public async Task Connect()
	{
		_ = 1;
		try
		{
			m_TokenSource = new CancellationTokenSource();
			m_CancellationToken = m_TokenSource.Token;
			m_Socket = new ClientWebSocket();
			foreach (KeyValuePair<string, string> header in headers)
			{
				m_Socket.Options.SetRequestHeader(header.Key, header.Value);
			}
			foreach (string subprotocol in subprotocols)
			{
				m_Socket.Options.AddSubProtocol(subprotocol);
			}
			await m_Socket.ConnectAsync(uri, m_CancellationToken);
			this.OnOpen?.Invoke();
			await Receive();
		}
		catch (Exception ex)
		{
			this.OnError?.Invoke(ex.Message);
			this.OnClose?.Invoke(WebSocketCloseCode.Abnormal);
		}
		finally
		{
			if (m_Socket != null)
			{
				m_TokenSource.Cancel();
				m_Socket.Dispose();
			}
		}
	}

	public Task Send(byte[] bytes)
	{
		return SendMessage(sendBytesQueue, WebSocketMessageType.Binary, new ArraySegment<byte>(bytes));
	}

	public Task SendText(string message)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(message);
		return SendMessage(sendTextQueue, WebSocketMessageType.Text, new ArraySegment<byte>(bytes, 0, bytes.Length));
	}

	private async Task SendMessage(List<ArraySegment<byte>> queue, WebSocketMessageType messageType, ArraySegment<byte> buffer)
	{
		if (buffer.Count == 0)
		{
			return;
		}
		bool flag;
		lock (OutgoingMessageLock)
		{
			flag = isSending;
			if (!isSending)
			{
				isSending = true;
			}
		}
		if (!flag)
		{
			if (!Monitor.TryEnter(m_Socket, 1000))
			{
				await m_Socket.CloseAsync(WebSocketCloseStatus.InternalServerError, string.Empty, m_CancellationToken);
				return;
			}
			try
			{
				m_Socket.SendAsync(buffer, messageType, endOfMessage: true, m_CancellationToken).Wait(m_CancellationToken);
			}
			finally
			{
				Monitor.Exit(m_Socket);
				lock (OutgoingMessageLock)
				{
					isSending = false;
				}
			}
			await HandleQueue(queue, messageType);
			return;
		}
		lock (OutgoingMessageLock)
		{
			queue.Add(buffer);
		}
	}

	private async Task HandleQueue(List<ArraySegment<byte>> queue, WebSocketMessageType messageType)
	{
		ArraySegment<byte> buffer = default(ArraySegment<byte>);
		lock (OutgoingMessageLock)
		{
			if (queue.Count > 0)
			{
				buffer = queue[0];
				queue.RemoveAt(0);
			}
		}
		if (buffer.Count > 0)
		{
			await SendMessage(queue, messageType, buffer);
		}
	}

	public void DispatchMessageQueue()
	{
		if (m_MessageList.Count != 0)
		{
			List<byte[]> list;
			lock (IncomingMessageLock)
			{
				list = new List<byte[]>(m_MessageList);
				m_MessageList.Clear();
			}
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				this.OnMessage?.Invoke(list[i]);
			}
		}
	}

	public async Task Receive()
	{
		WebSocketCloseCode closeCode = WebSocketCloseCode.Abnormal;
		await new WaitForBackgroundThread();
		ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[8192]);
		object obj = null;
		try
		{
			int num;
			_ = num - 1;
			_ = 1;
			try
			{
				while (m_Socket.State == System.Net.WebSockets.WebSocketState.Open)
				{
					using MemoryStream ms = new MemoryStream();
					WebSocketReceiveResult result;
					do
					{
						result = await m_Socket.ReceiveAsync(buffer, m_CancellationToken);
						ms.Write(buffer.Array, buffer.Offset, result.Count);
					}
					while (!result.EndOfMessage);
					ms.Seek(0L, SeekOrigin.Begin);
					if (result.MessageType == WebSocketMessageType.Text)
					{
						lock (IncomingMessageLock)
						{
							m_MessageList.Add(ms.ToArray());
						}
					}
					else if (result.MessageType == WebSocketMessageType.Binary)
					{
						lock (IncomingMessageLock)
						{
							m_MessageList.Add(ms.ToArray());
						}
					}
					else if (result.MessageType == WebSocketMessageType.Close)
					{
						await Close();
						closeCode = WebSocketHelpers.ParseCloseCodeEnum((int)result.CloseStatus.Value);
						break;
					}
				}
			}
			catch (Exception)
			{
				Debug.LogError("WebSocket: Got exception on Receive()");
				m_TokenSource.Cancel();
			}
		}
		catch (object obj2)
		{
			obj = obj2;
		}
		await new WaitForUpdate();
		this.OnClose?.Invoke(closeCode);
		object obj3 = obj;
		if (obj3 != null)
		{
			ExceptionDispatchInfo.Capture((obj3 as Exception) ?? throw obj3).Throw();
		}
	}

	public async Task Close()
	{
		if (State == WebSocketState.Open)
		{
			await m_Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, m_CancellationToken);
		}
	}
}
