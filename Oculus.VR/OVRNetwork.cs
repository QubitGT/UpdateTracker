using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

public class OVRNetwork
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct FrameHeader
	{
		public uint protocolIdentifier;

		public int payloadType;

		public int payloadLength;

		public const int StructSize = 12;

		public byte[] ToBytes()
		{
			int num = Marshal.SizeOf(this);
			byte[] array = new byte[num];
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			Marshal.StructureToPtr(this, intPtr, fDeleteOld: true);
			Marshal.Copy(intPtr, array, 0, num);
			Marshal.FreeHGlobal(intPtr);
			return array;
		}

		public static FrameHeader FromBytes(byte[] arr)
		{
			FrameHeader frameHeader = default(FrameHeader);
			int num = Marshal.SizeOf(frameHeader);
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			Marshal.Copy(arr, 0, intPtr, num);
			frameHeader = (FrameHeader)Marshal.PtrToStructure(intPtr, frameHeader.GetType());
			Marshal.FreeHGlobal(intPtr);
			return frameHeader;
		}
	}

	public class OVRNetworkTcpServer
	{
		public TcpListener tcpListener;

		private readonly object clientsLock = new object();

		public readonly List<TcpClient> clients = new List<TcpClient>();

		public void StartListening(int listeningPort)
		{
			if (tcpListener != null)
			{
				Debug.LogWarning("[OVRNetworkTcpServer] tcpListener is not null");
				return;
			}
			IPAddress any = IPAddress.Any;
			tcpListener = new TcpListener(any, listeningPort);
			try
			{
				tcpListener.Start();
				Debug.LogFormat("TcpListener started. Local endpoint: {0}", tcpListener.LocalEndpoint.ToString());
			}
			catch (SocketException ex)
			{
				Debug.LogWarningFormat("[OVRNetworkTcpServer] Unsable to start TcpListener. Socket exception: {0}", ex.Message);
				Debug.LogWarning("It could be caused by multiple instances listening at the same port, or the port is forwarded to the Android device through ADB");
				Debug.LogWarning("If the port is forwarded through ADB, use the Android Tools in Tools/Oculus/System Metrics Profiler to kill the server");
				tcpListener = null;
			}
			if (tcpListener == null)
			{
				return;
			}
			Debug.LogFormat("[OVRNetworkTcpServer] Start Listening on port {0}", listeningPort);
			try
			{
				tcpListener.BeginAcceptTcpClient(DoAcceptTcpClientCallback, tcpListener);
			}
			catch (Exception ex2)
			{
				Debug.LogWarningFormat("[OVRNetworkTcpServer] can't accept new client: {0}", ex2.Message);
			}
		}

		public void StopListening()
		{
			if (tcpListener == null)
			{
				Debug.LogWarning("[OVRNetworkTcpServer] tcpListener is null");
				return;
			}
			lock (clientsLock)
			{
				clients.Clear();
			}
			tcpListener.Stop();
			tcpListener = null;
			Debug.Log("[OVRNetworkTcpServer] Stopped listening");
		}

		private void DoAcceptTcpClientCallback(IAsyncResult ar)
		{
			TcpListener tcpListener = ar.AsyncState as TcpListener;
			try
			{
				TcpClient item = tcpListener.EndAcceptTcpClient(ar);
				lock (clientsLock)
				{
					clients.Add(item);
					Debug.Log("[OVRNetworkTcpServer] client added");
				}
				try
				{
					this.tcpListener.BeginAcceptTcpClient(DoAcceptTcpClientCallback, this.tcpListener);
				}
				catch (Exception ex)
				{
					Debug.LogWarningFormat("[OVRNetworkTcpServer] can't accept new client: {0}", ex.Message);
				}
			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex3)
			{
				Debug.LogWarningFormat("[OVRNetworkTcpServer] EndAcceptTcpClient failed: {0}", ex3.Message);
			}
		}

		public bool HasConnectedClient()
		{
			lock (clientsLock)
			{
				foreach (TcpClient client in clients)
				{
					if (client.Connected)
					{
						return true;
					}
				}
			}
			return false;
		}

		public void Broadcast(int payloadType, byte[] payload)
		{
			if (payload.Length > 65524)
			{
				Debug.LogWarningFormat("[OVRNetworkTcpServer] drop payload because it's too long: {0} bytes", payload.Length);
			}
			FrameHeader frameHeader = default(FrameHeader);
			frameHeader.protocolIdentifier = 1384359787u;
			frameHeader.payloadType = payloadType;
			frameHeader.payloadLength = payload.Length;
			byte[] array = frameHeader.ToBytes();
			byte[] array2 = new byte[array.Length + payload.Length];
			array.CopyTo(array2, 0);
			payload.CopyTo(array2, array.Length);
			lock (clientsLock)
			{
				foreach (TcpClient client in clients)
				{
					if (client.Connected)
					{
						try
						{
							client.GetStream().BeginWrite(array2, 0, array2.Length, DoWriteDataCallback, client.GetStream());
						}
						catch (SocketException ex)
						{
							Debug.LogWarningFormat("[OVRNetworkTcpServer] close client because of socket error: {0}", ex.Message);
							client.GetStream().Close();
							client.Close();
						}
					}
				}
			}
		}

		private void DoWriteDataCallback(IAsyncResult ar)
		{
			(ar.AsyncState as NetworkStream).EndWrite(ar);
		}
	}

	public class OVRNetworkTcpClient
	{
		public enum ConnectionState
		{
			Disconnected,
			Connected,
			Connecting
		}

		public Action connectionStateChangedCallback;

		public Action<int, byte[], int, int> payloadReceivedCallback;

		private TcpClient tcpClient;

		private byte[][] receivedBuffers = new byte[2][]
		{
			new byte[65536],
			new byte[65536]
		};

		private int receivedBufferIndex;

		private int receivedBufferDataSize;

		private ManualResetEvent readyReceiveDataEvent = new ManualResetEvent(initialState: true);

		public ConnectionState connectionState
		{
			get
			{
				if (tcpClient == null)
				{
					return ConnectionState.Disconnected;
				}
				if (tcpClient.Connected)
				{
					return ConnectionState.Connected;
				}
				return ConnectionState.Connecting;
			}
		}

		public bool Connected => connectionState == ConnectionState.Connected;

		public void Connect(int listeningPort)
		{
			if (tcpClient == null)
			{
				receivedBufferIndex = 0;
				receivedBufferDataSize = 0;
				readyReceiveDataEvent.Set();
				string host = "127.0.0.1";
				tcpClient = new TcpClient(AddressFamily.InterNetwork);
				tcpClient.BeginConnect(host, listeningPort, ConnectCallback, tcpClient);
				if (connectionStateChangedCallback != null)
				{
					connectionStateChangedCallback();
				}
			}
			else
			{
				Debug.LogWarning("[OVRNetworkTcpClient] already connected");
			}
		}

		private void ConnectCallback(IAsyncResult ar)
		{
			try
			{
				TcpClient tcpClient = ar.AsyncState as TcpClient;
				tcpClient.EndConnect(ar);
				Debug.LogFormat("[OVRNetworkTcpClient] connected to {0}", tcpClient.ToString());
			}
			catch (Exception ex)
			{
				Debug.LogWarningFormat("[OVRNetworkTcpClient] connect error {0}", ex.Message);
			}
			if (connectionStateChangedCallback != null)
			{
				connectionStateChangedCallback();
			}
		}

		public void Disconnect()
		{
			if (tcpClient != null)
			{
				if (!readyReceiveDataEvent.WaitOne(5))
				{
					Debug.LogWarning("[OVRNetworkTcpClient] readyReceiveDataEvent not signaled. data receiving timeout?");
				}
				Debug.Log("[OVRNetworkTcpClient] close tcpClient");
				try
				{
					tcpClient.GetStream().Close();
					tcpClient.Close();
				}
				catch (Exception ex)
				{
					Debug.LogWarning("[OVRNetworkTcpClient] " + ex.Message);
				}
				tcpClient = null;
				if (connectionStateChangedCallback != null)
				{
					connectionStateChangedCallback();
				}
			}
			else
			{
				Debug.LogWarning("[OVRNetworkTcpClient] not connected");
			}
		}

		public void Tick()
		{
			if (tcpClient != null && tcpClient.Connected && readyReceiveDataEvent.WaitOne(TimeSpan.Zero) && tcpClient.GetStream().DataAvailable)
			{
				if (receivedBufferDataSize >= 65536)
				{
					Debug.LogWarning("[OVRNetworkTcpClient] receive buffer overflow. It should not happen since we have the constraint on message size");
					Disconnect();
				}
				else
				{
					readyReceiveDataEvent.Reset();
					int count = 65536 - receivedBufferDataSize;
					tcpClient.GetStream().BeginRead(receivedBuffers[receivedBufferIndex], receivedBufferDataSize, count, OnReadDataCallback, tcpClient.GetStream());
				}
			}
		}

		private void OnReadDataCallback(IAsyncResult ar)
		{
			NetworkStream networkStream = ar.AsyncState as NetworkStream;
			try
			{
				int num = networkStream.EndRead(ar);
				receivedBufferDataSize += num;
				while (receivedBufferDataSize >= 12)
				{
					FrameHeader frameHeader = FrameHeader.FromBytes(receivedBuffers[receivedBufferIndex]);
					if (frameHeader.protocolIdentifier != 1384359787)
					{
						Debug.LogWarning("[OVRNetworkTcpClient] header mismatch");
						Disconnect();
						return;
					}
					if (frameHeader.payloadLength < 0 || frameHeader.payloadLength > 65524)
					{
						Debug.LogWarningFormat("[OVRNetworkTcpClient] Sanity check failed. PayloadLength %d", frameHeader.payloadLength);
						Disconnect();
						return;
					}
					if (receivedBufferDataSize >= 12 + frameHeader.payloadLength)
					{
						if (payloadReceivedCallback != null)
						{
							payloadReceivedCallback(frameHeader.payloadType, receivedBuffers[receivedBufferIndex], 12, frameHeader.payloadLength);
						}
						int num2 = 1 - receivedBufferIndex;
						int num3 = receivedBufferDataSize - (12 + frameHeader.payloadLength);
						if (num3 > 0)
						{
							Array.Copy(receivedBuffers[receivedBufferIndex], 12 + frameHeader.payloadLength, receivedBuffers[num2], 0, num3);
						}
						receivedBufferIndex = num2;
						receivedBufferDataSize = num3;
					}
				}
				readyReceiveDataEvent.Set();
			}
			catch (SocketException ex)
			{
				Debug.LogErrorFormat("[OVRNetworkTcpClient] OnReadDataCallback: socket error: {0}", ex.Message);
				Disconnect();
			}
		}
	}

	public const int MaxBufferLength = 65536;

	public const int MaxPayloadLength = 65524;

	public const uint FrameHeaderMagicIdentifier = 1384359787u;
}
