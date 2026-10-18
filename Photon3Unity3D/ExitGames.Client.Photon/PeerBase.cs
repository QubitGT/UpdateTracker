using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using ExitGames.Client.Photon.Encryption;
using Photon.SocketServer.Security;

namespace ExitGames.Client.Photon;

public abstract class PeerBase
{
	internal delegate void MyAction();

	private static class GpBinaryV3Parameters
	{
		public const byte CustomObject = 0;

		public const byte ExtraPlatformParams = 1;
	}

	internal PhotonPeer photonPeer;

	public IProtocol SerializationProtocol;

	internal ConnectionProtocol usedTransportProtocol;

	internal IPhotonSocket PhotonSocket;

	internal ConnectionStateValue peerConnectionState;

	internal int ByteCountLastOperation;

	internal int ByteCountCurrentDispatch;

	internal NCommand CommandInCurrentDispatch;

	internal int packetLossByCrc;

	internal int packetLossByChallenge;

	internal readonly Queue<MyAction> ActionQueue = new Queue<MyAction>();

	internal short peerID = -1;

	internal int serverTimeOffset;

	internal bool serverTimeOffsetIsAvailable;

	internal int roundTripTime;

	internal int roundTripTimeVariance;

	internal int lastRoundTripTime;

	internal int lowestRoundTripTime;

	internal int highestRoundTripTimeVariance;

	internal int timestampOfLastReceive;

	internal static short peerCount;

	internal long bytesOut;

	internal long bytesIn;

	internal object PhotonToken;

	internal object CustomInitData;

	public string AppId;

	internal EventData reusableEventData;

	private Stopwatch watch = Stopwatch.StartNew();

	internal int timeoutInt;

	internal int timeLastAckReceive;

	internal int longestSentCall;

	internal int timeLastSendAck;

	internal int timeLastSendOutgoing;

	internal bool ApplicationIsInitialized;

	internal bool isEncryptionAvailable;

	internal int outgoingCommandsInStream = 0;

	protected internal static Queue<StreamBuffer> MessageBufferPool = new Queue<StreamBuffer>(32);

	internal byte[] messageHeader;

	internal ICryptoProvider CryptoProvider;

	private readonly Random lagRandomizer = new Random();

	internal readonly LinkedList<SimulationItem> NetSimListOutgoing = new LinkedList<SimulationItem>();

	internal readonly LinkedList<SimulationItem> NetSimListIncoming = new LinkedList<SimulationItem>();

	private readonly NetworkSimulationSet networkSimulationSettings = new NetworkSimulationSet();

	internal int TrafficPackageHeaderSize;

	public string ServerAddress { get; internal set; }

	public string ProxyServerAddress { get; internal set; }

	internal IPhotonPeerListener Listener => photonPeer.Listener;

	public DebugLevel debugOut => photonPeer.DebugOut;

	internal int DisconnectTimeout => photonPeer.DisconnectTimeout;

	internal int timePingInterval => photonPeer.TimePingInterval;

	internal byte ChannelCount => photonPeer.ChannelCount;

	internal long BytesOut => bytesOut;

	internal long BytesIn => bytesIn;

	internal abstract int QueuedIncomingCommandsCount { get; }

	internal abstract int QueuedOutgoingCommandsCount { get; }

	internal virtual int SentReliableCommandsCount => 0;

	public virtual string PeerID => ((ushort)peerID).ToString();

	internal int timeInt => (int)watch.ElapsedMilliseconds;

	internal static int outgoingStreamBufferSize => PhotonPeer.OutgoingStreamBufferSize;

	internal int mtu => photonPeer.MaximumTransferUnit;

	protected internal bool IsIpv6 => PhotonSocket != null && PhotonSocket.AddressResolvedAsIpv6;

	public NetworkSimulationSet NetworkSimulationSettings => networkSimulationSettings;

	internal bool TrafficStatsEnabled => photonPeer.TrafficStatsEnabled;

	internal TrafficStats TrafficStatsIncoming => photonPeer.TrafficStatsIncoming;

	internal TrafficStats TrafficStatsOutgoing => photonPeer.TrafficStatsOutgoing;

	internal TrafficStatsGameLevel TrafficStatsGameLevel => photonPeer.TrafficStatsGameLevel;

	protected PeerBase()
	{
		networkSimulationSettings.peerBase = this;
		peerCount++;
	}

	public static StreamBuffer MessageBufferPoolGet()
	{
		lock (MessageBufferPool)
		{
			if (MessageBufferPool.Count > 0)
			{
				return MessageBufferPool.Dequeue();
			}
			return new StreamBuffer(75);
		}
	}

	public static void MessageBufferPoolPut(StreamBuffer buff)
	{
		buff.Position = 0;
		buff.SetLength(0L);
		lock (MessageBufferPool)
		{
			MessageBufferPool.Enqueue(buff);
		}
	}

	internal virtual void Reset()
	{
		SerializationProtocol = SerializationProtocolFactory.Create(photonPeer.SerializationProtocolType);
		photonPeer.InitializeTrafficStats();
		ByteCountLastOperation = 0;
		ByteCountCurrentDispatch = 0;
		bytesIn = 0L;
		bytesOut = 0L;
		packetLossByCrc = 0;
		packetLossByChallenge = 0;
		networkSimulationSettings.LostPackagesIn = 0;
		networkSimulationSettings.LostPackagesOut = 0;
		lock (NetSimListOutgoing)
		{
			NetSimListOutgoing.Clear();
		}
		lock (NetSimListIncoming)
		{
			NetSimListIncoming.Clear();
		}
		lock (ActionQueue)
		{
			ActionQueue.Clear();
		}
		peerConnectionState = ConnectionStateValue.Disconnected;
		watch.Reset();
		watch.Start();
		isEncryptionAvailable = false;
		ApplicationIsInitialized = false;
		CryptoProvider = null;
		roundTripTime = 200;
		roundTripTimeVariance = 5;
		serverTimeOffsetIsAvailable = false;
		serverTimeOffset = 0;
	}

	internal abstract bool Connect(string serverAddress, string proxyServerAddress, string appID, object photonToken);

	private string GetHttpKeyValueString(Dictionary<string, string> dic)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item in dic)
		{
			stringBuilder.Append(item.Key).Append("=").Append(item.Value)
				.Append("&");
		}
		return stringBuilder.ToString();
	}

	internal byte[] WriteInitRequest()
	{
		if (PhotonSocket == null || !PhotonSocket.Connected)
		{
			EnqueueDebugReturn(DebugLevel.WARNING, "The peer attempts to prepare an Init-Request but the socket is not connected!?");
		}
		if (photonPeer.UseInitV3)
		{
			return WriteInitV3();
		}
		if (PhotonToken == null)
		{
			byte[] array = new byte[41];
			byte[] clientVersion = Version.clientVersion;
			array[0] = 243;
			array[1] = 0;
			array[2] = SerializationProtocol.VersionBytes[0];
			array[3] = SerializationProtocol.VersionBytes[1];
			array[4] = photonPeer.ClientSdkIdShifted;
			array[5] = (byte)((byte)(clientVersion[0] << 4) | clientVersion[1]);
			array[6] = clientVersion[2];
			array[7] = clientVersion[3];
			array[8] = 0;
			if (string.IsNullOrEmpty(AppId))
			{
				AppId = "LoadBalancing";
			}
			for (int i = 0; i < 32; i++)
			{
				array[i + 9] = (byte)((i < AppId.Length) ? ((byte)AppId[i]) : 0);
			}
			if (IsIpv6)
			{
				array[5] |= 128;
			}
			else
			{
				array[5] &= 127;
			}
			return array;
		}
		if (PhotonToken != null)
		{
			byte[] array2 = null;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["init"] = null;
			dictionary["app"] = AppId;
			dictionary["clientversion"] = photonPeer.ClientVersion;
			dictionary["protocol"] = SerializationProtocol.ProtocolType;
			dictionary["sid"] = photonPeer.ClientSdkIdShifted.ToString();
			byte[] array3 = null;
			int num = 0;
			if (PhotonToken != null)
			{
				array3 = SerializationProtocol.Serialize(PhotonToken);
				num += array3.Length;
			}
			string text = GetHttpKeyValueString(dictionary);
			if (IsIpv6)
			{
				text += "&IPv6";
			}
			string text2 = $"POST /?{text} HTTP/1.1\r\nHost: {ServerAddress}\r\nContent-Length: {num}\r\n\r\n";
			array2 = new byte[text2.Length + num];
			if (array3 != null)
			{
				Buffer.BlockCopy(array3, 0, array2, text2.Length, array3.Length);
			}
			Buffer.BlockCopy(Encoding.UTF8.GetBytes(text2), 0, array2, 0, text2.Length);
			return array2;
		}
		return null;
	}

	private byte[] WriteInitV3()
	{
		StreamBuffer streamBuffer = new StreamBuffer();
		streamBuffer.WriteByte(245);
		InitV3Flags initV3Flags = InitV3Flags.NoFlags;
		if (IsIpv6)
		{
			initV3Flags |= InitV3Flags.IPv6Flag;
		}
		IPhotonEncryptor encryptor = photonPeer.Encryptor;
		if (encryptor != null)
		{
			initV3Flags |= InitV3Flags.EncryptionFlag;
		}
		streamBuffer.WriteBytes((byte)((int)initV3Flags >> 8), (byte)initV3Flags);
		switch (SerializationProtocol.VersionBytes[1])
		{
		case 6:
			streamBuffer.WriteByte(16);
			break;
		case 8:
			streamBuffer.WriteByte(18);
			break;
		default:
			throw new Exception("Unknown protocol version: " + SerializationProtocol.VersionBytes[1]);
		}
		streamBuffer.Write(Version.clientVersion, 0, 4);
		streamBuffer.WriteByte(photonPeer.ClientSdkIdShifted);
		streamBuffer.WriteByte(0);
		if (string.IsNullOrEmpty(AppId))
		{
			AppId = "Master";
		}
		byte[] bytes = Encoding.UTF8.GetBytes(AppId);
		int num = bytes.Length;
		if (num > 255)
		{
			throw new Exception("AppId is too long. Limited by 255 symbols.");
		}
		streamBuffer.WriteByte((byte)num);
		streamBuffer.Write(bytes, 0, bytes.Length);
		if (PhotonToken is byte[] array)
		{
			num = array.Length;
			streamBuffer.WriteBytes((byte)(num >> 8), (byte)num);
			streamBuffer.Write(array, 0, num);
		}
		else
		{
			streamBuffer.WriteBytes(0, 0);
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>();
		if (CustomInitData != null)
		{
			dictionary.Add(0, CustomInitData);
		}
		if (encryptor != null)
		{
			throw new NotImplementedException("InitV3 with encryption is not implemented yet.");
		}
		SerializationProtocol.Serialize(streamBuffer, dictionary, setType: true);
		return streamBuffer.ToArray();
	}

	internal string PrepareWebSocketUrl(string serverAddress, string appId, object photonToken)
	{
		StringBuilder stringBuilder = new StringBuilder(1024);
		stringBuilder.Append(serverAddress);
		stringBuilder.AppendFormat("/?libversion={0}&sid={1}", photonPeer.ClientVersion, photonPeer.ClientSdkIdShifted);
		if (!photonPeer.RemoveAppIdFromWebSocketPath)
		{
			stringBuilder.AppendFormat("&app={0}", appId);
		}
		if (IsIpv6)
		{
			stringBuilder.Append("&IPv6");
		}
		if (photonToken != null)
		{
			stringBuilder.Append("&xInit=");
		}
		return stringBuilder.ToString();
	}

	public abstract void OnConnect();

	internal void InitCallback()
	{
		if (peerConnectionState == ConnectionStateValue.Connecting)
		{
			peerConnectionState = ConnectionStateValue.Connected;
		}
		ApplicationIsInitialized = true;
		FetchServerTimestamp();
		Listener.OnStatusChanged(StatusCode.Connect);
	}

	internal abstract void Disconnect();

	internal abstract void StopConnection();

	internal abstract void FetchServerTimestamp();

	internal abstract bool IsTransportEncrypted();

	internal abstract bool EnqueuePhotonMessage(StreamBuffer opBytes, SendOptions sendParams);

	internal StreamBuffer SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, EgMessageType messageType, bool encrypt)
	{
		bool flag = encrypt && !IsTransportEncrypted();
		StreamBuffer streamBuffer = MessageBufferPoolGet();
		streamBuffer.SetLength(0L);
		if (!flag)
		{
			streamBuffer.Write(messageHeader, 0, messageHeader.Length);
		}
		SerializationProtocol.SerializeOperationRequest(streamBuffer, opCode, parameters, setType: false);
		if (flag)
		{
			byte[] array = CryptoProvider.Encrypt(streamBuffer.GetBuffer(), 0, streamBuffer.Length);
			streamBuffer.SetLength(0L);
			streamBuffer.Write(messageHeader, 0, messageHeader.Length);
			streamBuffer.Write(array, 0, array.Length);
		}
		byte[] buffer = streamBuffer.GetBuffer();
		if (messageType != EgMessageType.Operation)
		{
			buffer[messageHeader.Length - 1] = (byte)messageType;
		}
		if (flag || (encrypt && photonPeer.EnableEncryptedFlag))
		{
			buffer[messageHeader.Length - 1] = (byte)(buffer[messageHeader.Length - 1] | 0x80u);
		}
		return streamBuffer;
	}

	internal StreamBuffer SerializeOperationToMessage(byte opCode, ParameterDictionary parameters, EgMessageType messageType, bool encrypt)
	{
		bool flag = encrypt && !IsTransportEncrypted();
		StreamBuffer streamBuffer = MessageBufferPoolGet();
		streamBuffer.SetLength(0L);
		if (!flag)
		{
			streamBuffer.Write(messageHeader, 0, messageHeader.Length);
		}
		SerializationProtocol.SerializeOperationRequest(streamBuffer, opCode, parameters, setType: false);
		if (flag)
		{
			byte[] array = CryptoProvider.Encrypt(streamBuffer.GetBuffer(), 0, streamBuffer.Length);
			streamBuffer.SetLength(0L);
			streamBuffer.Write(messageHeader, 0, messageHeader.Length);
			streamBuffer.Write(array, 0, array.Length);
		}
		byte[] buffer = streamBuffer.GetBuffer();
		if (messageType != EgMessageType.Operation)
		{
			buffer[messageHeader.Length - 1] = (byte)messageType;
		}
		if (flag || (encrypt && photonPeer.EnableEncryptedFlag))
		{
			buffer[messageHeader.Length - 1] = (byte)(buffer[messageHeader.Length - 1] | 0x80u);
		}
		return streamBuffer;
	}

	internal StreamBuffer SerializeMessageToMessage(object message, bool encrypt)
	{
		bool flag = encrypt && !IsTransportEncrypted();
		StreamBuffer streamBuffer = MessageBufferPoolGet();
		streamBuffer.SetLength(0L);
		if (!flag)
		{
			streamBuffer.Write(messageHeader, 0, messageHeader.Length);
		}
		bool flag2 = message is byte[];
		if (flag2)
		{
			byte[] array = message as byte[];
			streamBuffer.Write(array, 0, array.Length);
		}
		else
		{
			SerializationProtocol.SerializeMessage(streamBuffer, message);
		}
		if (flag)
		{
			byte[] array2 = CryptoProvider.Encrypt(streamBuffer.GetBuffer(), 0, streamBuffer.Length);
			streamBuffer.SetLength(0L);
			streamBuffer.Write(messageHeader, 0, messageHeader.Length);
			streamBuffer.Write(array2, 0, array2.Length);
		}
		byte[] buffer = streamBuffer.GetBuffer();
		buffer[messageHeader.Length - 1] = (byte)(flag2 ? 9 : 8);
		if (flag || (encrypt && photonPeer.EnableEncryptedFlag))
		{
			buffer[messageHeader.Length - 1] = (byte)(buffer[messageHeader.Length - 1] | 0x80u);
		}
		return streamBuffer;
	}

	internal abstract bool SendOutgoingCommands();

	internal virtual bool SendAcksOnly()
	{
		return false;
	}

	internal abstract void ReceiveIncomingCommands(byte[] inBuff, int dataLength);

	internal abstract bool DispatchIncomingCommands();

	internal virtual bool DeserializeMessageAndCallback(StreamBuffer stream)
	{
		if (stream.Length < 2)
		{
			if ((int)debugOut >= 1)
			{
				Listener.DebugReturn(DebugLevel.ERROR, "Incoming UDP data too short! " + stream.Length);
			}
			return false;
		}
		byte b = stream.ReadByte();
		if (b != 243 && b != 253)
		{
			if ((int)debugOut >= 1)
			{
				Listener.DebugReturn(DebugLevel.ALL, "No regular operation UDP message: " + b);
			}
			return false;
		}
		byte b2 = stream.ReadByte();
		byte b3 = (byte)(b2 & 0x7Fu);
		bool flag = (b2 & 0x80) > 0;
		if (b3 != 1)
		{
			try
			{
				if (flag)
				{
					byte[] buf = CryptoProvider.Decrypt(stream.GetBuffer(), 2, stream.Length - 2);
					stream = new StreamBuffer(buf);
				}
				else
				{
					stream.Seek(2L, SeekOrigin.Begin);
				}
			}
			catch (Exception ex)
			{
				if ((int)debugOut >= 1)
				{
					Listener.DebugReturn(DebugLevel.ERROR, "msgType: " + b3 + " exception: " + ex.ToString());
				}
				SupportClass.WriteStackTrace(ex);
				return false;
			}
		}
		int num = 0;
		IProtocol.DeserializationFlags flags = (photonPeer.UseByteArraySlicePoolForEvents ? IProtocol.DeserializationFlags.AllowPooledByteArray : IProtocol.DeserializationFlags.None) | (photonPeer.WrapIncomingStructs ? IProtocol.DeserializationFlags.WrapIncomingStructs : IProtocol.DeserializationFlags.None);
		switch (b3)
		{
		case 3:
		{
			OperationResponse operationResponse = null;
			try
			{
				operationResponse = SerializationProtocol.DeserializeOperationResponse(stream, flags);
			}
			catch (Exception ex5)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Deserialization failed for Operation Response. " + ex5);
				return false;
			}
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.CountResult(ByteCountCurrentDispatch);
				num = timeInt;
			}
			Listener.OnOperationResponse(operationResponse);
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.TimeForResponseCallback(operationResponse.OperationCode, timeInt - num);
			}
			break;
		}
		case 4:
		{
			EventData eventData = null;
			try
			{
				eventData = SerializationProtocol.DeserializeEventData(stream, reusableEventData, flags);
			}
			catch (Exception ex4)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Deserialization failed for Event. " + ex4);
				return false;
			}
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.CountEvent(ByteCountCurrentDispatch);
				num = timeInt;
			}
			Listener.OnEvent(eventData);
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.TimeForEventCallback(eventData.Code, timeInt - num);
			}
			if (photonPeer.ReuseEventInstance)
			{
				reusableEventData = eventData;
			}
			break;
		}
		case 5:
			try
			{
				DisconnectMessage dm = SerializationProtocol.DeserializeDisconnectMessage(stream);
				photonPeer.OnDisconnectMessageCall(dm);
			}
			catch (Exception ex3)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Deserialization failed for disconnect message. " + ex3);
				return false;
			}
			break;
		case 1:
			InitCallback();
			break;
		case 7:
		{
			OperationResponse operationResponse;
			try
			{
				operationResponse = SerializationProtocol.DeserializeOperationResponse(stream);
			}
			catch (Exception ex2)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Deserialization failed for internal Operation Response. " + ex2);
				return false;
			}
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.CountResult(ByteCountCurrentDispatch);
				num = timeInt;
			}
			if (operationResponse.OperationCode == PhotonCodes.InitEncryption)
			{
				DeriveSharedKey(operationResponse);
			}
			else if (operationResponse.OperationCode == PhotonCodes.Ping)
			{
				if (peerConnectionState == ConnectionStateValue.Connecting && (usedTransportProtocol == ConnectionProtocol.WebSocket || usedTransportProtocol == ConnectionProtocol.WebSocketSecure))
				{
					photonPeer.PingUsedAsInit = true;
					InitCallback();
				}
				if (this is TPeer tPeer)
				{
					tPeer.ReadPingResult(operationResponse);
				}
			}
			else
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "Received unknown internal operation. " + operationResponse.ToStringFull());
			}
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.TimeForResponseCallback(operationResponse.OperationCode, timeInt - num);
			}
			break;
		}
		case 8:
		{
			object obj = SerializationProtocol.DeserializeMessage(stream);
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.CountEvent(ByteCountCurrentDispatch);
				num = timeInt;
			}
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.TimeForMessageCallback(timeInt - num);
			}
			break;
		}
		case 9:
		{
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.CountEvent(ByteCountCurrentDispatch);
				num = timeInt;
			}
			byte[] array = stream.ToArrayFromPos();
			if (TrafficStatsEnabled)
			{
				TrafficStatsGameLevel.TimeForRawMessageCallback(timeInt - num);
			}
			break;
		}
		default:
			EnqueueDebugReturn(DebugLevel.ERROR, "unexpected msgType " + b3);
			break;
		}
		return true;
	}

	internal void UpdateRoundTripTimeAndVariance(int lastRoundtripTime)
	{
		if (lastRoundtripTime >= 0)
		{
			roundTripTimeVariance -= roundTripTimeVariance / 4;
			if (lastRoundtripTime >= roundTripTime)
			{
				roundTripTime += (lastRoundtripTime - roundTripTime) / 8;
				roundTripTimeVariance += (lastRoundtripTime - roundTripTime) / 4;
			}
			else
			{
				roundTripTime += (lastRoundtripTime - roundTripTime) / 8;
				roundTripTimeVariance -= (lastRoundtripTime - roundTripTime) / 4;
			}
			if (roundTripTime < lowestRoundTripTime)
			{
				lowestRoundTripTime = roundTripTime;
			}
			if (roundTripTimeVariance > highestRoundTripTimeVariance)
			{
				highestRoundTripTimeVariance = roundTripTimeVariance;
			}
		}
	}

	internal bool ExchangeKeysForEncryption(object lockObject)
	{
		isEncryptionAvailable = false;
		if (CryptoProvider != null)
		{
			CryptoProvider.Dispose();
			CryptoProvider = null;
		}
		if (photonPeer.PayloadEncryptorType != null)
		{
			try
			{
				CryptoProvider = (ICryptoProvider)Activator.CreateInstance(photonPeer.PayloadEncryptorType);
				if (CryptoProvider == null)
				{
					Listener.DebugReturn(DebugLevel.WARNING, "Payload encryptor creation by type failed, Activator.CreateInstance() returned null for: " + photonPeer.PayloadEncryptorType);
				}
			}
			catch (Exception ex)
			{
				Listener.DebugReturn(DebugLevel.WARNING, "Payload encryptor creation by type failed: " + ex);
			}
		}
		if (CryptoProvider == null)
		{
			CryptoProvider = new DiffieHellmanCryptoProvider();
		}
		Dictionary<byte, object> dictionary = new Dictionary<byte, object>(1);
		dictionary[PhotonCodes.ClientKey] = CryptoProvider.PublicKey;
		SendOptions sendOptions;
		if (lockObject != null)
		{
			lock (lockObject)
			{
				sendOptions = default(SendOptions);
				sendOptions.Channel = 0;
				sendOptions.Encrypt = false;
				sendOptions.DeliveryMode = DeliveryMode.Reliable;
				SendOptions sendParams = sendOptions;
				StreamBuffer opBytes = SerializeOperationToMessage(PhotonCodes.InitEncryption, dictionary, EgMessageType.InternalOperationRequest, sendParams.Encrypt);
				return EnqueuePhotonMessage(opBytes, sendParams);
			}
		}
		sendOptions = default(SendOptions);
		sendOptions.Channel = 0;
		sendOptions.Encrypt = false;
		sendOptions.DeliveryMode = DeliveryMode.Reliable;
		SendOptions sendParams2 = sendOptions;
		StreamBuffer opBytes2 = SerializeOperationToMessage(PhotonCodes.InitEncryption, dictionary, EgMessageType.InternalOperationRequest, sendParams2.Encrypt);
		return EnqueuePhotonMessage(opBytes2, sendParams2);
	}

	internal void DeriveSharedKey(OperationResponse operationResponse)
	{
		if (operationResponse.ReturnCode != 0)
		{
			EnqueueDebugReturn(DebugLevel.ERROR, "Establishing encryption keys failed. " + operationResponse.ToStringFull());
			EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
			return;
		}
		byte[] array = (byte[])operationResponse.Parameters[PhotonCodes.ServerKey];
		if (array == null || array.Length == 0)
		{
			EnqueueDebugReturn(DebugLevel.ERROR, "Establishing encryption keys failed. Server's public key is null or empty. " + operationResponse.ToStringFull());
			EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
		}
		else
		{
			CryptoProvider.DeriveSharedKey(array);
			isEncryptionAvailable = true;
			EnqueueStatusCallback(StatusCode.EncryptionEstablished);
		}
	}

	internal virtual void InitEncryption(byte[] secret)
	{
		if (photonPeer.PayloadEncryptorType != null)
		{
			try
			{
				CryptoProvider = (ICryptoProvider)Activator.CreateInstance(photonPeer.PayloadEncryptorType, secret);
				if (CryptoProvider == null)
				{
					Listener.DebugReturn(DebugLevel.WARNING, "Payload encryptor creation by type failed, Activator.CreateInstance() returned null for: " + photonPeer.PayloadEncryptorType);
				}
				else
				{
					isEncryptionAvailable = true;
				}
			}
			catch (Exception ex)
			{
				Listener.DebugReturn(DebugLevel.WARNING, "Payload encryptor creation by type failed: " + ex);
			}
		}
		if (CryptoProvider == null)
		{
			CryptoProvider = new DiffieHellmanCryptoProvider(secret);
			isEncryptionAvailable = true;
		}
	}

	internal void EnqueueActionForDispatch(MyAction action)
	{
		lock (ActionQueue)
		{
			ActionQueue.Enqueue(action);
		}
	}

	internal void EnqueueDebugReturn(DebugLevel level, string debugReturn)
	{
		lock (ActionQueue)
		{
			ActionQueue.Enqueue(delegate
			{
				Listener.DebugReturn(level, debugReturn);
			});
		}
	}

	internal void EnqueueStatusCallback(StatusCode statusValue)
	{
		lock (ActionQueue)
		{
			ActionQueue.Enqueue(delegate
			{
				Listener.OnStatusChanged(statusValue);
			});
		}
	}

	internal void SendNetworkSimulated(byte[] dataToSend)
	{
		if (!NetworkSimulationSettings.IsSimulationEnabled)
		{
			throw new NotImplementedException("SendNetworkSimulated was called, despite NetworkSimulationSettings.IsSimulationEnabled == false.");
		}
		if (usedTransportProtocol == ConnectionProtocol.Udp && NetworkSimulationSettings.OutgoingLossPercentage > 0 && lagRandomizer.Next(101) < NetworkSimulationSettings.OutgoingLossPercentage)
		{
			networkSimulationSettings.LostPackagesOut++;
			return;
		}
		int num = ((networkSimulationSettings.OutgoingJitter > 0) ? (lagRandomizer.Next(networkSimulationSettings.OutgoingJitter * 2) - networkSimulationSettings.OutgoingJitter) : 0);
		int num2 = networkSimulationSettings.OutgoingLag + num;
		int num3 = timeInt + num2;
		SimulationItem value = new SimulationItem
		{
			DelayedData = dataToSend,
			TimeToExecute = num3,
			Delay = num2
		};
		lock (NetSimListOutgoing)
		{
			if (NetSimListOutgoing.Count == 0 || usedTransportProtocol == ConnectionProtocol.Tcp)
			{
				NetSimListOutgoing.AddLast(value);
				return;
			}
			LinkedListNode<SimulationItem> linkedListNode = NetSimListOutgoing.First;
			while (linkedListNode != null && linkedListNode.Value.TimeToExecute < num3)
			{
				linkedListNode = linkedListNode.Next;
			}
			if (linkedListNode == null)
			{
				NetSimListOutgoing.AddLast(value);
			}
			else
			{
				NetSimListOutgoing.AddBefore(linkedListNode, value);
			}
		}
	}

	internal void ReceiveNetworkSimulated(byte[] dataReceived)
	{
		if (!networkSimulationSettings.IsSimulationEnabled)
		{
			throw new NotImplementedException("ReceiveNetworkSimulated was called, despite NetworkSimulationSettings.IsSimulationEnabled == false.");
		}
		if (usedTransportProtocol == ConnectionProtocol.Udp && networkSimulationSettings.IncomingLossPercentage > 0 && lagRandomizer.Next(101) < networkSimulationSettings.IncomingLossPercentage)
		{
			networkSimulationSettings.LostPackagesIn++;
			return;
		}
		int num = ((networkSimulationSettings.IncomingJitter > 0) ? (lagRandomizer.Next(networkSimulationSettings.IncomingJitter * 2) - networkSimulationSettings.IncomingJitter) : 0);
		int num2 = networkSimulationSettings.IncomingLag + num;
		int num3 = timeInt + num2;
		SimulationItem value = new SimulationItem
		{
			DelayedData = dataReceived,
			TimeToExecute = num3,
			Delay = num2
		};
		lock (NetSimListIncoming)
		{
			if (NetSimListIncoming.Count == 0 || usedTransportProtocol == ConnectionProtocol.Tcp)
			{
				NetSimListIncoming.AddLast(value);
				return;
			}
			LinkedListNode<SimulationItem> linkedListNode = NetSimListIncoming.First;
			while (linkedListNode != null && linkedListNode.Value.TimeToExecute < num3)
			{
				linkedListNode = linkedListNode.Next;
			}
			if (linkedListNode == null)
			{
				NetSimListIncoming.AddLast(value);
			}
			else
			{
				NetSimListIncoming.AddBefore(linkedListNode, value);
			}
		}
	}

	protected internal void NetworkSimRun()
	{
		while (true)
		{
			bool flag = false;
			lock (networkSimulationSettings.NetSimManualResetEvent)
			{
				flag = networkSimulationSettings.IsSimulationEnabled;
			}
			if (!flag)
			{
				networkSimulationSettings.NetSimManualResetEvent.WaitOne();
				continue;
			}
			lock (NetSimListIncoming)
			{
				SimulationItem simulationItem = null;
				while (NetSimListIncoming.First != null)
				{
					simulationItem = NetSimListIncoming.First.Value;
					if (simulationItem.stopw.ElapsedMilliseconds < simulationItem.Delay)
					{
						break;
					}
					ReceiveIncomingCommands(simulationItem.DelayedData, simulationItem.DelayedData.Length);
					NetSimListIncoming.RemoveFirst();
				}
			}
			lock (NetSimListOutgoing)
			{
				SimulationItem simulationItem2 = null;
				while (NetSimListOutgoing.First != null)
				{
					simulationItem2 = NetSimListOutgoing.First.Value;
					if (simulationItem2.stopw.ElapsedMilliseconds < simulationItem2.Delay)
					{
						break;
					}
					if (PhotonSocket != null && PhotonSocket.Connected)
					{
						PhotonSocket.Send(simulationItem2.DelayedData, simulationItem2.DelayedData.Length);
					}
					NetSimListOutgoing.RemoveFirst();
				}
			}
			Thread.Sleep(0);
		}
	}
}
