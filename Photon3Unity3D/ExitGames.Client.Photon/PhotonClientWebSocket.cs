using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace ExitGames.Client.Photon;

public class PhotonClientWebSocket : IPhotonSocket
{
	private ClientWebSocket clientWebSocket;

	private Task sendTask;

	[Preserve]
	public PhotonClientWebSocket(PeerBase peerBase)
		: base(peerBase)
	{
		if (ReportDebugOfLevel(DebugLevel.INFO))
		{
			EnqueueDebugReturn(DebugLevel.INFO, "PhotonClientWebSocket");
		}
	}

	public override bool Connect()
	{
		if (!base.Connect())
		{
			return false;
		}
		base.State = PhotonSocketState.Connecting;
		Thread thread = new Thread(AsyncConnectAndReceive);
		thread.IsBackground = true;
		thread.Start();
		return true;
	}

	private void AsyncConnectAndReceive()
	{
		Uri uri = null;
		try
		{
			uri = new Uri(ConnectAddress);
		}
		catch (Exception ex)
		{
			if (ReportDebugOfLevel(DebugLevel.ERROR))
			{
				base.Listener.DebugReturn(DebugLevel.ERROR, "Failed to create a URI from ConnectAddress (" + ConnectAddress + "). Exception: " + ex);
			}
		}
		if (uri != null && uri.HostNameType == UriHostNameType.Dns)
		{
			try
			{
				IPAddress[] hostAddresses = Dns.GetHostAddresses(uri.Host);
				IPAddress[] array = hostAddresses;
				foreach (IPAddress iPAddress in array)
				{
					if (iPAddress.AddressFamily == AddressFamily.InterNetworkV6)
					{
						base.AddressResolvedAsIpv6 = true;
						ConnectAddress += "&IPv6";
						break;
					}
				}
			}
			catch (Exception ex2)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "Dns.GetHostAddresses(" + uri.Host + ") failed: " + ex2);
			}
		}
		clientWebSocket = new ClientWebSocket();
		clientWebSocket.Options.AddSubProtocol(base.SerializationProtocol);
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(7000);
		Task task = clientWebSocket.ConnectAsync(new Uri(ConnectAddress), cancellationTokenSource.Token);
		try
		{
			task.Wait();
		}
		catch (Exception arg)
		{
			EnqueueDebugReturn(DebugLevel.ERROR, $"AsyncConnectAndReceive() caught exception on {ConnectAddress}: {arg}");
		}
		if (task.IsFaulted)
		{
			EnqueueDebugReturn(DebugLevel.ERROR, "ClientWebSocket IsFaulted: " + task.Exception);
		}
		if (clientWebSocket.State != WebSocketState.Open)
		{
			base.SocketErrorCode = (int)(clientWebSocket.CloseStatus.HasValue ? clientWebSocket.CloseStatus.Value : ((WebSocketCloseStatus)0));
			EnqueueDebugReturn(DebugLevel.ERROR, "ClientWebSocket is not open. State: " + clientWebSocket.State.ToString() + " CloseStatus: " + clientWebSocket.CloseStatus.ToString() + " Description: " + clientWebSocket.CloseStatusDescription);
			HandleException(StatusCode.ExceptionOnConnect);
			return;
		}
		base.State = PhotonSocketState.Connected;
		peerBase.OnConnect();
		MemoryStream memoryStream = new MemoryStream(base.MTU);
		bool flag = false;
		ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[base.MTU]);
		while (clientWebSocket.State == WebSocketState.Open)
		{
			Task<WebSocketReceiveResult> task2 = null;
			try
			{
				task2 = clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
				while (!task2.IsCompleted)
				{
					task2.Wait(50);
				}
			}
			catch (Exception)
			{
			}
			if (!task2.IsCompleted || clientWebSocket.State != WebSocketState.Open)
			{
				continue;
			}
			if (task2.IsCanceled)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "PhotonClientWebSocket readTask.IsCanceled: " + task2.Status.ToString() + " " + base.ServerAddress + ":" + base.ServerPort + " " + clientWebSocket.CloseStatusDescription);
				continue;
			}
			if (task2.Result.Count == 0)
			{
				EnqueueDebugReturn(DebugLevel.INFO, "PhotonClientWebSocket received 0 bytes. this.State: " + base.State.ToString() + " clientWebSocket.State: " + clientWebSocket.State.ToString() + " readTask.Status: " + task2.Status);
				continue;
			}
			if (!task2.Result.EndOfMessage)
			{
				flag = true;
				memoryStream.Write(buffer.Array, 0, task2.Result.Count);
				continue;
			}
			int num;
			byte[] array2;
			bool flag2;
			if (flag)
			{
				memoryStream.Write(buffer.Array, 0, task2.Result.Count);
				num = (int)memoryStream.Length;
				array2 = memoryStream.GetBuffer();
			}
			else
			{
				num = task2.Result.Count;
				array2 = buffer.Array;
				flag2 = array2[5] == 0;
			}
			flag2 = array2[5] == 0;
			HandleReceivedDatagram(array2, num, willBeReused: true);
			if (flag)
			{
				memoryStream.SetLength(0L);
				memoryStream.Position = 0L;
				flag = false;
			}
			if (peerBase.TrafficStatsEnabled)
			{
				if (flag2)
				{
					peerBase.TrafficStatsIncoming.CountReliableOpCommand(num);
				}
				else
				{
					peerBase.TrafficStatsIncoming.CountUnreliableOpCommand(num);
				}
			}
		}
		if (base.State != PhotonSocketState.Disconnecting && base.State != 0)
		{
			EnqueueDebugReturn(DebugLevel.INFO, "PhotonSocket.State is " + base.State.ToString() + " but can't receive anymore. ClientWebSocket.State: " + clientWebSocket.State);
			if (clientWebSocket.State == WebSocketState.CloseReceived)
			{
				HandleException(StatusCode.DisconnectByServerLogic);
			}
			if (clientWebSocket.State == WebSocketState.Aborted)
			{
				HandleException(StatusCode.DisconnectByServerReasonUnknown);
			}
		}
		Disconnect();
	}

	public override bool Disconnect()
	{
		if (clientWebSocket != null && clientWebSocket.State == WebSocketState.CloseReceived)
		{
			try
			{
				clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "CloseAsync due to state CloseReceived", CancellationToken.None);
			}
			catch (Exception ex)
			{
				if (ReportDebugOfLevel(DebugLevel.ALL))
				{
					EnqueueDebugReturn(DebugLevel.ALL, "Caught exception in clientWebSocket.CloseAsync(): " + ex);
				}
			}
			base.State = PhotonSocketState.Disconnected;
			return true;
		}
		if (clientWebSocket != null && clientWebSocket.State != WebSocketState.Closed && clientWebSocket.State != WebSocketState.CloseSent)
		{
			base.State = PhotonSocketState.Disconnecting;
			try
			{
				clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "ws close", CancellationToken.None);
			}
			catch (Exception ex2)
			{
				if (ReportDebugOfLevel(DebugLevel.ALL))
				{
					EnqueueDebugReturn(DebugLevel.ALL, "Caught exception in clientWebSocket.CloseOutputAsync(): " + ex2);
				}
			}
		}
		base.State = PhotonSocketState.Disconnected;
		return true;
	}

	public override PhotonSocketError Send(byte[] data, int length)
	{
		if (clientWebSocket != null && clientWebSocket.State != WebSocketState.Open && base.State != PhotonSocketState.Disconnecting && base.State != 0)
		{
			if (clientWebSocket.State == WebSocketState.CloseReceived)
			{
				HandleException(StatusCode.DisconnectByServerLogic);
				return PhotonSocketError.Exception;
			}
			if (clientWebSocket.State == WebSocketState.Aborted)
			{
				HandleException(StatusCode.DisconnectByServerReasonUnknown);
				return PhotonSocketError.Exception;
			}
		}
		if (clientWebSocket == null)
		{
			if (base.State == PhotonSocketState.Disconnecting || base.State == PhotonSocketState.Disconnected)
			{
				return PhotonSocketError.Skipped;
			}
			if (ReportDebugOfLevel(DebugLevel.ERROR))
			{
				EnqueueDebugReturn(DebugLevel.ERROR, $"PhotonClientWebSocket.Send() failed: this.clientWebSocket is null. this.State: {base.State}");
			}
			return PhotonSocketError.Exception;
		}
		if (sendTask != null && !sendTask.IsCompleted && !sendTask.Wait(5))
		{
			return PhotonSocketError.Busy;
		}
		sendTask = clientWebSocket.SendAsync(new ArraySegment<byte>(data, 0, length), WebSocketMessageType.Binary, endOfMessage: true, CancellationToken.None);
		if (sendTask != null && !sendTask.IsCompleted && !sendTask.Wait(5))
		{
			return PhotonSocketError.PendingSend;
		}
		sendTask = null;
		return PhotonSocketError.Success;
	}

	public override PhotonSocketError Receive(out byte[] data)
	{
		throw new NotImplementedException();
	}
}
