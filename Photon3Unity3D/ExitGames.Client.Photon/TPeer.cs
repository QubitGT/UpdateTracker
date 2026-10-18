using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon;

internal class TPeer : PeerBase
{
	internal const int TCP_HEADER_BYTES = 7;

	internal const int MSG_HEADER_BYTES = 2;

	public const int ALL_HEADER_BYTES = 9;

	private Queue<StreamBuffer> incomingList = new Queue<StreamBuffer>(32);

	internal List<StreamBuffer> outgoingStream;

	private int lastPingActivity;

	private readonly byte[] pingRequest = new byte[5] { 240, 0, 0, 0, 0 };

	private readonly ParameterDictionary pingParamDict = new ParameterDictionary();

	internal static readonly byte[] tcpFramedMessageHead = new byte[9] { 251, 0, 0, 0, 0, 0, 0, 243, 2 };

	internal static readonly byte[] tcpMsgHead = new byte[2] { 243, 2 };

	protected internal bool DoFraming = true;

	private bool waitForInitResponse = true;

	internal override int QueuedIncomingCommandsCount => incomingList.Count;

	internal override int QueuedOutgoingCommandsCount => outgoingCommandsInStream;

	internal TPeer()
	{
		TrafficPackageHeaderSize = 0;
	}

	internal override bool IsTransportEncrypted()
	{
		return usedTransportProtocol == ConnectionProtocol.WebSocketSecure;
	}

	internal override void Reset()
	{
		base.Reset();
		if (photonPeer.PayloadEncryptionSecret != null && usedTransportProtocol != ConnectionProtocol.WebSocketSecure)
		{
			InitEncryption(photonPeer.PayloadEncryptionSecret);
		}
		incomingList = new Queue<StreamBuffer>(32);
		timestampOfLastReceive = base.timeInt;
		lastPingActivity = base.timeInt;
		waitForInitResponse = true;
	}

	internal override bool Connect(string serverAddress, string proxyServerAddress, string appID, object photonToken)
	{
		outgoingStream = new List<StreamBuffer>();
		messageHeader = (DoFraming ? tcpFramedMessageHead : tcpMsgHead);
		if (usedTransportProtocol == ConnectionProtocol.WebSocket || usedTransportProtocol == ConnectionProtocol.WebSocketSecure)
		{
			PhotonSocket.ConnectAddress = PrepareWebSocketUrl(serverAddress, appID, photonToken);
		}
		if (PhotonSocket.Connect())
		{
			peerConnectionState = ConnectionStateValue.Connecting;
			return true;
		}
		return false;
	}

	public override void OnConnect()
	{
		lastPingActivity = base.timeInt;
		if (DoFraming || PhotonToken != null)
		{
			waitForInitResponse = true;
			byte[] data = WriteInitRequest();
			EnqueueInit(data);
		}
		else
		{
			waitForInitResponse = false;
		}
		SendOutgoingCommands();
	}

	internal override void Disconnect()
	{
		if (peerConnectionState != 0 && peerConnectionState != ConnectionStateValue.Disconnecting)
		{
			if ((int)base.debugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, "TPeer.Disconnect()");
			}
			StopConnection();
		}
	}

	internal override void StopConnection()
	{
		peerConnectionState = ConnectionStateValue.Disconnecting;
		if (PhotonSocket != null)
		{
			PhotonSocket.Disconnect();
		}
		lock (incomingList)
		{
			incomingList.Clear();
		}
		peerConnectionState = ConnectionStateValue.Disconnected;
		EnqueueStatusCallback(StatusCode.Disconnect);
	}

	internal override void FetchServerTimestamp()
	{
		SendPing();
		serverTimeOffsetIsAvailable = false;
	}

	private void EnqueueInit(byte[] data)
	{
		StreamBuffer streamBuffer = new StreamBuffer(data.Length + 32);
		byte[] array = new byte[7] { 251, 0, 0, 0, 0, 0, 1 };
		int targetOffset = 1;
		Protocol.Serialize(data.Length + array.Length, array, ref targetOffset);
		streamBuffer.Write(array, 0, array.Length);
		streamBuffer.Write(data, 0, data.Length);
		if (base.TrafficStatsEnabled)
		{
			base.TrafficStatsOutgoing.CountControlCommand(streamBuffer.Length);
		}
		EnqueueMessageAsPayload(DeliveryMode.Reliable, streamBuffer, 0);
	}

	internal override bool DispatchIncomingCommands()
	{
		if (peerConnectionState == ConnectionStateValue.Connected && base.timeInt - timestampOfLastReceive > base.DisconnectTimeout)
		{
			EnqueueStatusCallback(StatusCode.TimeoutDisconnect);
			EnqueueActionForDispatch(Disconnect);
		}
		while (true)
		{
			MyAction myAction;
			lock (ActionQueue)
			{
				if (ActionQueue.Count <= 0)
				{
					break;
				}
				myAction = ActionQueue.Dequeue();
				goto IL_0097;
			}
			IL_0097:
			myAction();
		}
		StreamBuffer streamBuffer;
		lock (incomingList)
		{
			if (incomingList.Count <= 0)
			{
				return false;
			}
			streamBuffer = incomingList.Dequeue();
		}
		ByteCountCurrentDispatch = streamBuffer.Length + 3;
		bool result = DeserializeMessageAndCallback(streamBuffer);
		PeerBase.MessageBufferPoolPut(streamBuffer);
		return result;
	}

	internal override bool SendOutgoingCommands()
	{
		if (peerConnectionState == ConnectionStateValue.Disconnected)
		{
			return false;
		}
		SendPing();
		if (!PhotonSocket.Connected)
		{
			return false;
		}
		timeLastSendOutgoing = base.timeInt;
		lock (outgoingStream)
		{
			int num = 0;
			int num2 = 0;
			PhotonSocketError photonSocketError = PhotonSocketError.Success;
			for (int i = 0; i < outgoingStream.Count; i++)
			{
				StreamBuffer streamBuffer = outgoingStream[i];
				photonSocketError = SendData(streamBuffer.GetBuffer(), streamBuffer.Length);
				if (photonSocketError == PhotonSocketError.Busy)
				{
					break;
				}
				num2 += streamBuffer.Length;
				num++;
				if (photonSocketError != PhotonSocketError.PendingSend)
				{
					PeerBase.MessageBufferPoolPut(streamBuffer);
				}
				if (num2 >= base.mtu || photonSocketError == PhotonSocketError.PendingSend)
				{
					break;
				}
			}
			outgoingStream.RemoveRange(0, num);
			outgoingCommandsInStream -= num;
			if (photonSocketError == PhotonSocketError.Busy || photonSocketError == PhotonSocketError.PendingSend)
			{
				return false;
			}
			return outgoingStream.Count > 0;
		}
	}

	internal override bool SendAcksOnly()
	{
		if (peerConnectionState == ConnectionStateValue.Disconnected)
		{
			return false;
		}
		SendPing();
		return false;
	}

	internal override bool EnqueuePhotonMessage(StreamBuffer opBytes, SendOptions sendParams)
	{
		return EnqueueMessageAsPayload(sendParams.DeliveryMode, opBytes, sendParams.Channel);
	}

	internal bool EnqueueMessageAsPayload(DeliveryMode deliveryMode, StreamBuffer opMessage, byte channelId)
	{
		if (opMessage == null)
		{
			return false;
		}
		if (DoFraming)
		{
			byte[] buffer = opMessage.GetBuffer();
			int targetOffset = 1;
			Protocol.Serialize(opMessage.Length, buffer, ref targetOffset);
			buffer[5] = channelId;
			switch (deliveryMode)
			{
			case DeliveryMode.Unreliable:
				buffer[6] = 0;
				break;
			case DeliveryMode.Reliable:
				buffer[6] = 1;
				break;
			case DeliveryMode.UnreliableUnsequenced:
				buffer[6] = 2;
				break;
			case DeliveryMode.ReliableUnsequenced:
				buffer[6] = 3;
				break;
			default:
				throw new ArgumentOutOfRangeException("DeliveryMode", deliveryMode, null);
			}
		}
		lock (outgoingStream)
		{
			outgoingStream.Add(opMessage);
			outgoingCommandsInStream++;
		}
		int num = (ByteCountLastOperation = opMessage.Length);
		if (base.TrafficStatsEnabled)
		{
			switch (deliveryMode)
			{
			case DeliveryMode.Unreliable:
			case DeliveryMode.UnreliableUnsequenced:
				base.TrafficStatsOutgoing.CountUnreliableOpCommand(num);
				break;
			case DeliveryMode.Reliable:
			case DeliveryMode.ReliableUnsequenced:
				base.TrafficStatsOutgoing.CountReliableOpCommand(num);
				break;
			}
			base.TrafficStatsGameLevel.CountOperation(num);
		}
		return true;
	}

	internal void SendPing()
	{
		if (base.timeInt - lastPingActivity < base.timePingInterval || (peerConnectionState != ConnectionStateValue.Connected && (peerConnectionState != ConnectionStateValue.Connecting || waitForInitResponse)))
		{
			return;
		}
		int num = (lastPingActivity = base.timeInt);
		StreamBuffer streamBuffer;
		if (!DoFraming)
		{
			lock (pingParamDict)
			{
				pingParamDict[1] = num;
				streamBuffer = SerializeOperationToMessage(PhotonCodes.Ping, pingParamDict, EgMessageType.InternalOperationRequest, encrypt: false);
			}
		}
		else
		{
			int targetOffset = 1;
			Protocol.Serialize(num, pingRequest, ref targetOffset);
			streamBuffer = PeerBase.MessageBufferPoolGet();
			streamBuffer.Write(pingRequest, 0, pingRequest.Length);
		}
		if (base.TrafficStatsEnabled)
		{
			base.TrafficStatsOutgoing.CountControlCommand(streamBuffer.Length);
		}
		if (SendData(streamBuffer.GetBuffer(), streamBuffer.Length) == PhotonSocketError.Success)
		{
			PeerBase.MessageBufferPoolPut(streamBuffer);
		}
	}

	internal PhotonSocketError SendData(byte[] data, int length)
	{
		PhotonSocketError result = PhotonSocketError.Success;
		try
		{
			bytesOut += length;
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsOutgoing.TotalPacketCount++;
				base.TrafficStatsOutgoing.TotalCommandsInPackets++;
			}
			if (base.NetworkSimulationSettings.IsSimulationEnabled)
			{
				byte[] array = new byte[length];
				Buffer.BlockCopy(data, 0, array, 0, length);
				SendNetworkSimulated(array);
			}
			else
			{
				int num = base.timeInt;
				result = PhotonSocket.Send(data, length);
				int num2 = base.timeInt - num;
				if (num2 > longestSentCall)
				{
					longestSentCall = num2;
				}
			}
		}
		catch (Exception ex)
		{
			if ((int)base.debugOut >= 1)
			{
				base.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
			}
			SupportClass.WriteStackTrace(ex);
		}
		return result;
	}

	internal override void ReceiveIncomingCommands(byte[] inbuff, int dataLength)
	{
		if (inbuff == null)
		{
			if ((int)base.debugOut >= 1)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "checkAndQueueIncomingCommands() inBuff: null");
			}
			return;
		}
		timestampOfLastReceive = base.timeInt;
		bytesIn += dataLength + 7;
		if (base.TrafficStatsEnabled)
		{
			base.TrafficStatsIncoming.TotalPacketCount++;
			base.TrafficStatsIncoming.TotalCommandsInPackets++;
		}
		if (inbuff[0] == 243)
		{
			byte b = (byte)(inbuff[1] & 0x7Fu);
			byte b2 = inbuff[2];
			if (b != 7 || b2 != PhotonCodes.Ping)
			{
				StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
				streamBuffer.Write(inbuff, 0, dataLength);
				streamBuffer.Position = 0;
				lock (incomingList)
				{
					incomingList.Enqueue(streamBuffer);
					return;
				}
			}
			DeserializeMessageAndCallback(new StreamBuffer(inbuff));
		}
		else if (inbuff[0] == 240)
		{
			base.TrafficStatsIncoming.CountControlCommand(dataLength);
			ReadPingResult(inbuff);
		}
		else if ((int)base.debugOut >= 1 && dataLength > 0)
		{
			EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() MagicNumber should be 0xF0 or 0xF3. Is: " + inbuff[0] + " dataLength: " + dataLength);
		}
	}

	private void ReadPingResult(byte[] inbuff)
	{
		int value = 0;
		int value2 = 0;
		int offset = 1;
		Protocol.Deserialize(out value, inbuff, ref offset);
		Protocol.Deserialize(out value2, inbuff, ref offset);
		lastRoundTripTime = base.timeInt - value2;
		if (!serverTimeOffsetIsAvailable)
		{
			roundTripTime = lastRoundTripTime;
		}
		UpdateRoundTripTimeAndVariance(lastRoundTripTime);
		if (!serverTimeOffsetIsAvailable)
		{
			serverTimeOffset = value + (lastRoundTripTime >> 1) - base.timeInt;
			serverTimeOffsetIsAvailable = true;
		}
	}

	protected internal void ReadPingResult(OperationResponse operationResponse)
	{
		int num = (int)operationResponse.Parameters[2];
		int num2 = (int)operationResponse.Parameters[1];
		lastRoundTripTime = base.timeInt - num2;
		if (!serverTimeOffsetIsAvailable)
		{
			roundTripTime = lastRoundTripTime;
		}
		UpdateRoundTripTimeAndVariance(lastRoundTripTime);
		if (!serverTimeOffsetIsAvailable)
		{
			serverTimeOffset = num + (lastRoundTripTime >> 1) - base.timeInt;
			serverTimeOffsetIsAvailable = true;
		}
	}
}
