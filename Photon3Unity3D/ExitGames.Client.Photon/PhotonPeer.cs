using System;
using System.Collections.Generic;
using System.Diagnostics;
using ExitGames.Client.Photon.Encryption;
using Photon.SocketServer.Security;

namespace ExitGames.Client.Photon;

public class PhotonPeer
{
	[Obsolete("Check QueuedOutgoingCommands and QueuedIncomingCommands on demand instead.")]
	public int WarningSize;

	[Obsolete("Where dynamic linking is available, this library will attempt to load it and fallback to a managed implementation. This value is always true.")]
	public const bool NativeDatagramEncrypt = true;

	[Obsolete("Use the ITrafficRecorder to capture all traffic instead.")]
	public int CommandLogSize;

	public const bool NoSocket = false;

	public const bool DebugBuild = true;

	public const int NativeEncryptorApiVersion = 2;

	public TargetFrameworks TargetFramework = TargetFrameworks.NetStandard20;

	public static bool NoNativeCallbacks;

	public bool RemoveAppIdFromWebSocketPath;

	public byte ClientSdkId = 15;

	private static string clientVersion;

	[Obsolete("A Native Socket implementation is no longer part of this DLL but delivered in a separate add-on. This value always returns false.")]
	public static readonly bool NativeSocketLibAvailable = false;

	[Obsolete("Native Payload Encryption is no longer part of this DLL but delivered in a separate add-on. This value always returns false.")]
	public static readonly bool NativePayloadEncryptionLibAvailable = false;

	[Obsolete("Native Datagram Encryption is no longer part of this DLL but delivered in a separate add-on. This value always returns false.")]
	public static readonly bool NativeDatagramEncryptionLibAvailable = false;

	internal bool UseInitV3;

	public Dictionary<ConnectionProtocol, Type> SocketImplementationConfig;

	public DebugLevel DebugOut = DebugLevel.ERROR;

	private bool reuseEventInstance;

	private bool useByteArraySlicePoolForEvents = false;

	private bool wrapIncomingStructs = false;

	public bool SendInCreationOrder = true;

	public int SendWindowSize = 50;

	public ITrafficRecorder TrafficRecorder;

	private byte quickResendAttempts;

	public byte ChannelCount = 2;

	public bool EnableEncryptedFlag = false;

	private bool crcEnabled;

	public int SentCountAllowance = 7;

	public int InitialResendTimeMax = 400;

	public int TimePingInterval = 1000;

	public bool PingUsedAsInit = false;

	private int disconnectTimeout = 10000;

	public static int OutgoingStreamBufferSize = 1200;

	private int mtu = 1200;

	public static bool AsyncKeyExchange = false;

	internal bool RandomizeSequenceNumbers;

	internal byte[] RandomizedSequenceNumbers;

	internal bool GcmDatagramEncryption;

	private Stopwatch trafficStatsStopwatch;

	private bool trafficStatsEnabled = false;

	internal PeerBase peerBase;

	private readonly object SendOutgoingLockObject = new object();

	private readonly object DispatchLockObject = new object();

	private readonly object EnqueueLock = new object();

	private Type payloadEncryptorType;

	protected internal byte[] PayloadEncryptionSecret;

	private Type encryptorType;

	protected internal IPhotonEncryptor Encryptor;

	[Obsolete("See remarks.")]
	public int CommandBufferSize { get; set; }

	[Obsolete("See remarks.")]
	public int LimitOfUnreliableCommands { get; set; }

	[Obsolete("Returns SupportClass.GetTickCount(). Should be replaced by a StopWatch or the per peer PhotonPeer.ClientTime.")]
	public int LocalTimeInMilliSeconds => SupportClass.GetTickCount();

	protected internal byte ClientSdkIdShifted => (byte)((uint)(ClientSdkId << 1) | 0u);

	[Obsolete("The static string Version should be preferred.")]
	public string ClientVersion
	{
		get
		{
			if (string.IsNullOrEmpty(clientVersion))
			{
				clientVersion = $"{ExitGames.Client.Photon.Version.clientVersion[0]}.{ExitGames.Client.Photon.Version.clientVersion[1]}.{ExitGames.Client.Photon.Version.clientVersion[2]}.{ExitGames.Client.Photon.Version.clientVersion[3]}";
			}
			return clientVersion;
		}
	}

	public static string Version
	{
		get
		{
			if (string.IsNullOrEmpty(clientVersion))
			{
				clientVersion = $"{ExitGames.Client.Photon.Version.clientVersion[0]}.{ExitGames.Client.Photon.Version.clientVersion[1]}.{ExitGames.Client.Photon.Version.clientVersion[2]}.{ExitGames.Client.Photon.Version.clientVersion[3]}";
			}
			return clientVersion;
		}
	}

	public SerializationProtocol SerializationProtocolType { get; set; }

	public Type SocketImplementation { get; internal set; }

	public int SocketErrorCode => (peerBase != null && peerBase.PhotonSocket != null) ? peerBase.PhotonSocket.SocketErrorCode : 0;

	public IPhotonPeerListener Listener { get; protected set; }

	public bool ReuseEventInstance
	{
		get
		{
			return reuseEventInstance;
		}
		set
		{
			lock (DispatchLockObject)
			{
				reuseEventInstance = value;
				if (!value)
				{
					peerBase.reusableEventData = null;
				}
			}
		}
	}

	public bool UseByteArraySlicePoolForEvents
	{
		get
		{
			return useByteArraySlicePoolForEvents;
		}
		set
		{
			useByteArraySlicePoolForEvents = value;
		}
	}

	public bool WrapIncomingStructs
	{
		get
		{
			return wrapIncomingStructs;
		}
		set
		{
			wrapIncomingStructs = value;
		}
	}

	public ByteArraySlicePool ByteArraySlicePool => peerBase.SerializationProtocol.ByteArraySlicePool;

	[Obsolete("Use SendWindowSize instead.")]
	public int SequenceDeltaLimitSends
	{
		get
		{
			return SendWindowSize;
		}
		set
		{
			SendWindowSize = value;
		}
	}

	public long BytesIn => peerBase.BytesIn;

	public long BytesOut => peerBase.BytesOut;

	public int ByteCountCurrentDispatch => peerBase.ByteCountCurrentDispatch;

	public string CommandInfoCurrentDispatch => (peerBase.CommandInCurrentDispatch != null) ? peerBase.CommandInCurrentDispatch.ToString() : string.Empty;

	public int ByteCountLastOperation => peerBase.ByteCountLastOperation;

	public bool EnableServerTracing { get; set; }

	public byte QuickResendAttempts
	{
		get
		{
			return quickResendAttempts;
		}
		set
		{
			quickResendAttempts = value;
			if (quickResendAttempts > 4)
			{
				quickResendAttempts = 4;
			}
		}
	}

	public PeerStateValue PeerState
	{
		get
		{
			if (peerBase.peerConnectionState == ConnectionStateValue.Connected && !peerBase.ApplicationIsInitialized)
			{
				return PeerStateValue.InitializingApplication;
			}
			return (PeerStateValue)peerBase.peerConnectionState;
		}
	}

	public string PeerID => peerBase.PeerID;

	public int QueuedIncomingCommands => peerBase.QueuedIncomingCommandsCount;

	public int QueuedOutgoingCommands => peerBase.QueuedOutgoingCommandsCount;

	public bool CrcEnabled
	{
		get
		{
			return crcEnabled;
		}
		set
		{
			if (crcEnabled != value)
			{
				if (peerBase.peerConnectionState != 0)
				{
					throw new Exception("CrcEnabled can only be set while disconnected.");
				}
				crcEnabled = value;
			}
		}
	}

	public int PacketLossByCrc => peerBase.packetLossByCrc;

	public int PacketLossByChallenge => peerBase.packetLossByChallenge;

	public int SentReliableCommandsCount => peerBase.SentReliableCommandsCount;

	public int ResentReliableCommands => (UsedProtocol == ConnectionProtocol.Udp) ? ((EnetPeer)peerBase).reliableCommandsRepeated : 0;

	public int DisconnectTimeout
	{
		get
		{
			return disconnectTimeout;
		}
		set
		{
			if (value < 0)
			{
				disconnectTimeout = 10000;
			}
			disconnectTimeout = value;
		}
	}

	public int ServerTimeInMilliSeconds => peerBase.serverTimeOffsetIsAvailable ? (peerBase.serverTimeOffset + ConnectionTime) : 0;

	[Obsolete("The PhotonPeer will no longer use this delegate. It uses a Stopwatch in all cases. You can access PhotonPeer.ConnectionTime.")]
	public SupportClass.IntegerMillisecondsDelegate LocalMsTimestampDelegate
	{
		set
		{
			if (PeerState != 0)
			{
				throw new Exception("LocalMsTimestampDelegate only settable while disconnected. State: " + PeerState);
			}
			SupportClass.IntegerMilliseconds = value;
		}
	}

	public int ConnectionTime => peerBase.timeInt;

	public int LastSendAckTime => peerBase.timeLastSendAck;

	public int LastSendOutgoingTime => peerBase.timeLastSendOutgoing;

	public int LongestSentCall
	{
		get
		{
			return peerBase.longestSentCall;
		}
		set
		{
			peerBase.longestSentCall = value;
		}
	}

	public int RoundTripTime => peerBase.roundTripTime;

	public int RoundTripTimeVariance => peerBase.roundTripTimeVariance;

	public int LastRoundTripTime => peerBase.lastRoundTripTime;

	public int TimestampOfLastSocketReceive => peerBase.timestampOfLastReceive;

	public string ServerAddress => peerBase.ServerAddress;

	public string ServerIpAddress => IPhotonSocket.ServerIpAddress;

	public ConnectionProtocol UsedProtocol => peerBase.usedTransportProtocol;

	public ConnectionProtocol TransportProtocol { get; set; }

	public virtual bool IsSimulationEnabled
	{
		get
		{
			return NetworkSimulationSettings.IsSimulationEnabled;
		}
		set
		{
			if (value == NetworkSimulationSettings.IsSimulationEnabled)
			{
				return;
			}
			lock (SendOutgoingLockObject)
			{
				NetworkSimulationSettings.IsSimulationEnabled = value;
			}
		}
	}

	public NetworkSimulationSet NetworkSimulationSettings => peerBase.NetworkSimulationSettings;

	public int MaximumTransferUnit
	{
		get
		{
			return mtu;
		}
		set
		{
			if (PeerState != 0)
			{
				throw new Exception("MaximumTransferUnit is only settable while disconnected. State: " + PeerState);
			}
			if (value < 576)
			{
				value = 576;
			}
			mtu = value;
		}
	}

	public bool IsEncryptionAvailable => peerBase.isEncryptionAvailable;

	[Obsolete("Internally not used anymore. Call SendAcksOnly() instead.")]
	public bool IsSendingOnlyAcks { get; set; }

	public TrafficStats TrafficStatsIncoming { get; internal set; }

	public TrafficStats TrafficStatsOutgoing { get; internal set; }

	public TrafficStatsGameLevel TrafficStatsGameLevel { get; internal set; }

	public long TrafficStatsElapsedMs => (trafficStatsStopwatch != null) ? trafficStatsStopwatch.ElapsedMilliseconds : 0;

	public bool TrafficStatsEnabled
	{
		get
		{
			return trafficStatsEnabled;
		}
		set
		{
			if (trafficStatsEnabled == value)
			{
				return;
			}
			trafficStatsEnabled = value;
			if (trafficStatsEnabled)
			{
				if (trafficStatsStopwatch == null)
				{
					InitializeTrafficStats();
				}
				trafficStatsStopwatch.Start();
			}
			else if (trafficStatsStopwatch != null)
			{
				trafficStatsStopwatch.Stop();
			}
		}
	}

	public Type PayloadEncryptorType
	{
		get
		{
			return payloadEncryptorType;
		}
		set
		{
			bool flag = false;
			if (value == null || typeof(ICryptoProvider).IsAssignableFrom(value))
			{
				payloadEncryptorType = value;
			}
			else
			{
				Listener.DebugReturn(DebugLevel.WARNING, "Failed to set the EncryptorType. Type must implement IPhotonEncryptor.");
			}
		}
	}

	public Type EncryptorType
	{
		get
		{
			return encryptorType;
		}
		set
		{
			bool flag = false;
			if (value == null || typeof(IPhotonEncryptor).IsAssignableFrom(value))
			{
				encryptorType = value;
			}
			else
			{
				Listener.DebugReturn(DebugLevel.WARNING, "Failed to set the EncryptorType. Type must implement IPhotonEncryptor.");
			}
		}
	}

	public int CountDiscarded { get; set; }

	public int DeltaUnreliableNumber { get; set; }

	public event Action<DisconnectMessage> OnDisconnectMessage;

	[Obsolete("Use the ITrafficRecorder to capture all traffic instead.")]
	public string CommandLogToString()
	{
		return string.Empty;
	}

	public static void MessageBufferPoolTrim(int countOfBuffers)
	{
		lock (PeerBase.MessageBufferPool)
		{
			if (countOfBuffers <= 0)
			{
				PeerBase.MessageBufferPool.Clear();
			}
			else if (countOfBuffers < PeerBase.MessageBufferPool.Count)
			{
				while (PeerBase.MessageBufferPool.Count > countOfBuffers)
				{
					PeerBase.MessageBufferPool.Dequeue();
				}
				PeerBase.MessageBufferPool.TrimExcess();
			}
		}
	}

	public static int MessageBufferPoolSize()
	{
		return PeerBase.MessageBufferPool.Count;
	}

	public void TrafficStatsReset()
	{
		TrafficStatsEnabled = false;
		InitializeTrafficStats();
		TrafficStatsEnabled = true;
	}

	internal void InitializeTrafficStats()
	{
		if (trafficStatsStopwatch == null)
		{
			trafficStatsStopwatch = new Stopwatch();
		}
		else
		{
			trafficStatsStopwatch.Reset();
		}
		TrafficStatsIncoming = new TrafficStats(peerBase.TrafficPackageHeaderSize);
		TrafficStatsOutgoing = new TrafficStats(peerBase.TrafficPackageHeaderSize);
		TrafficStatsGameLevel = new TrafficStatsGameLevel(trafficStatsStopwatch);
		if (trafficStatsEnabled)
		{
			trafficStatsStopwatch.Start();
		}
	}

	public string VitalStatsToString(bool all)
	{
		string text = "";
		if (TrafficStatsGameLevel != null)
		{
			text = TrafficStatsGameLevel.ToStringVitalStats();
		}
		if (!all)
		{
			return string.Format("Rtt(variance): {0}({1}). Since receive: {2}ms. Longest send: {5}ms. Stats elapsed: {4}sec.\n{3}", RoundTripTime, RoundTripTimeVariance, ConnectionTime - TimestampOfLastSocketReceive, text, TrafficStatsElapsedMs / 1000, LongestSentCall);
		}
		return string.Format("Rtt(variance): {0}({1}). Since receive: {2}ms. Longest send: {7}ms. Stats elapsed: {6}sec.\n{3}\n{4}\n{5}", RoundTripTime, RoundTripTimeVariance, ConnectionTime - TimestampOfLastSocketReceive, text, TrafficStatsIncoming, TrafficStatsOutgoing, TrafficStatsElapsedMs / 1000, LongestSentCall);
	}

	public PhotonPeer(ConnectionProtocol protocolType)
	{
		TransportProtocol = protocolType;
		SocketImplementationConfig = new Dictionary<ConnectionProtocol, Type>(5);
		SocketImplementationConfig[ConnectionProtocol.Udp] = typeof(SocketUdp);
		SocketImplementationConfig[ConnectionProtocol.Tcp] = typeof(SocketTcp);
		SocketImplementationConfig[ConnectionProtocol.WebSocket] = typeof(PhotonClientWebSocket);
		SocketImplementationConfig[ConnectionProtocol.WebSocketSecure] = typeof(PhotonClientWebSocket);
		CreatePeerBase();
	}

	public PhotonPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType)
		: this(protocolType)
	{
		Listener = listener;
	}

	public virtual bool Connect(string serverAddress, string appId, object photonToken = null, object customInitData = null)
	{
		return Connect(serverAddress, null, appId, photonToken, customInitData);
	}

	public virtual bool Connect(string serverAddress, string proxyServerAddress, string appId, object photonToken, object customInitData = null)
	{
		lock (DispatchLockObject)
		{
			lock (SendOutgoingLockObject)
			{
				if (peerBase != null && peerBase.peerConnectionState != 0)
				{
					Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting.");
					return false;
				}
				if (photonToken == null)
				{
					Encryptor = null;
					RandomizedSequenceNumbers = null;
					RandomizeSequenceNumbers = false;
					GcmDatagramEncryption = false;
				}
				CreatePeerBase();
				peerBase.Reset();
				PingUsedAsInit = false;
				peerBase.ServerAddress = serverAddress;
				peerBase.ProxyServerAddress = proxyServerAddress;
				peerBase.AppId = appId;
				peerBase.PhotonToken = photonToken;
				peerBase.CustomInitData = customInitData;
				Type value = null;
				if (!SocketImplementationConfig.TryGetValue(TransportProtocol, out value))
				{
					peerBase.EnqueueDebugReturn(DebugLevel.ERROR, "Connect failed. SocketImplementationConfig is not set for protocol " + TransportProtocol.ToString() + ": " + SupportClass.DictionaryToString(SocketImplementationConfig, includeTypes: false));
					return false;
				}
				SocketImplementation = value;
				try
				{
					peerBase.PhotonSocket = (IPhotonSocket)Activator.CreateInstance(SocketImplementation, peerBase);
				}
				catch (Exception ex)
				{
					Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed to create a IPhotonSocket instance for " + TransportProtocol.ToString() + ". SocketImplementationConfig: " + SupportClass.DictionaryToString(SocketImplementationConfig, includeTypes: false) + " Exception: " + ex);
					return false;
				}
				return peerBase.Connect(serverAddress, proxyServerAddress, appId, photonToken);
			}
		}
	}

	private void CreatePeerBase()
	{
		ConnectionProtocol transportProtocol = TransportProtocol;
		ConnectionProtocol connectionProtocol = transportProtocol;
		if (connectionProtocol == ConnectionProtocol.Tcp || connectionProtocol - 4 <= ConnectionProtocol.Tcp)
		{
			TPeer tPeer = peerBase as TPeer;
			if (tPeer == null)
			{
				tPeer = (TPeer)(peerBase = new TPeer());
			}
			tPeer.DoFraming = TransportProtocol == ConnectionProtocol.Tcp;
		}
		else if (!(peerBase is EnetPeer))
		{
			peerBase = new EnetPeer();
		}
		peerBase.photonPeer = this;
		peerBase.usedTransportProtocol = TransportProtocol;
	}

	public virtual void Disconnect()
	{
		lock (DispatchLockObject)
		{
			lock (SendOutgoingLockObject)
			{
				peerBase.Disconnect();
			}
		}
	}

	internal void OnDisconnectMessageCall(DisconnectMessage dm)
	{
		if (this.OnDisconnectMessage != null)
		{
			this.OnDisconnectMessage(dm);
		}
	}

	public virtual void StopThread()
	{
		lock (DispatchLockObject)
		{
			lock (SendOutgoingLockObject)
			{
				peerBase.StopConnection();
			}
		}
	}

	public virtual void FetchServerTimestamp()
	{
		peerBase.FetchServerTimestamp();
	}

	public bool EstablishEncryption()
	{
		if (AsyncKeyExchange)
		{
			SupportClass.StartBackgroundCalls(delegate
			{
				peerBase.ExchangeKeysForEncryption(SendOutgoingLockObject);
				return false;
			});
			return true;
		}
		return peerBase.ExchangeKeysForEncryption(SendOutgoingLockObject);
	}

	public bool InitDatagramEncryption(byte[] encryptionSecret, byte[] hmacSecret, bool randomizedSequenceNumbers = false, bool chainingModeGCM = false)
	{
		if (EncryptorType != null)
		{
			try
			{
				Encryptor = (IPhotonEncryptor)Activator.CreateInstance(EncryptorType);
				if (Encryptor == null)
				{
					Listener.DebugReturn(DebugLevel.WARNING, "Datagram encryptor creation by type failed, Activator.CreateInstance() returned null");
				}
			}
			catch (Exception ex)
			{
				Listener.DebugReturn(DebugLevel.WARNING, "Datagram encryptor creation by type failed: " + ex);
			}
		}
		if (Encryptor == null)
		{
			Encryptor = new EncryptorNet();
		}
		if (Encryptor == null)
		{
			throw new NullReferenceException("Can not init datagram encryption. No suitable encryptor found or provided.");
		}
		Listener.DebugReturn(DebugLevel.INFO, "Datagram encryptor of type " + Encryptor.GetType()?.ToString() + " created. Api version: " + 2);
		Listener.DebugReturn(DebugLevel.INFO, "Datagram encryptor initialization: GCM = " + chainingModeGCM + ", random seq num = " + randomizedSequenceNumbers);
		Encryptor.Init(encryptionSecret, hmacSecret, null, chainingModeGCM, mtu);
		if (randomizedSequenceNumbers)
		{
			RandomizedSequenceNumbers = encryptionSecret;
			RandomizeSequenceNumbers = true;
			GcmDatagramEncryption = chainingModeGCM;
		}
		return true;
	}

	public void InitPayloadEncryption(byte[] secret)
	{
		PayloadEncryptionSecret = secret;
	}

	public virtual void Service()
	{
		while (DispatchIncomingCommands())
		{
		}
		while (SendOutgoingCommands())
		{
		}
	}

	public virtual bool SendOutgoingCommands()
	{
		if (TrafficStatsEnabled)
		{
			TrafficStatsGameLevel.SendOutgoingCommandsCalled();
		}
		lock (SendOutgoingLockObject)
		{
			return peerBase.SendOutgoingCommands();
		}
	}

	public virtual bool SendAcksOnly()
	{
		if (TrafficStatsEnabled)
		{
			TrafficStatsGameLevel.SendOutgoingCommandsCalled();
		}
		lock (SendOutgoingLockObject)
		{
			return peerBase.SendAcksOnly();
		}
	}

	public virtual bool DispatchIncomingCommands()
	{
		if (TrafficStatsEnabled)
		{
			TrafficStatsGameLevel.DispatchIncomingCommandsCalled();
		}
		lock (DispatchLockObject)
		{
			peerBase.ByteCountCurrentDispatch = 0;
			return peerBase.DispatchIncomingCommands();
		}
	}

	public virtual bool SendOperation(byte operationCode, Dictionary<byte, object> operationParameters, SendOptions sendOptions)
	{
		if (sendOptions.Encrypt && !IsEncryptionAvailable && peerBase.usedTransportProtocol != ConnectionProtocol.WebSocketSecure)
		{
			throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
		}
		if (peerBase.peerConnectionState != ConnectionStateValue.Connected)
		{
			if ((int)DebugOut >= 1)
			{
				Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: " + operationCode + " Not connected. PeerState: " + peerBase.peerConnectionState);
			}
			Listener.OnStatusChanged(StatusCode.SendError);
			return false;
		}
		if (sendOptions.Channel >= ChannelCount)
		{
			if ((int)DebugOut >= 1)
			{
				Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Selected channel (" + sendOptions.Channel + ")>= channelCount (" + ChannelCount + ").");
			}
			Listener.OnStatusChanged(StatusCode.SendError);
			return false;
		}
		lock (EnqueueLock)
		{
			StreamBuffer opBytes = peerBase.SerializeOperationToMessage(operationCode, operationParameters, EgMessageType.Operation, sendOptions.Encrypt);
			return peerBase.EnqueuePhotonMessage(opBytes, sendOptions);
		}
	}

	public virtual bool SendOperation(byte operationCode, ParameterDictionary operationParameters, SendOptions sendOptions)
	{
		if (sendOptions.Encrypt && !IsEncryptionAvailable && peerBase.usedTransportProtocol != ConnectionProtocol.WebSocketSecure)
		{
			throw new ArgumentException("Can't use encryption yet. Exchange keys first.");
		}
		if (peerBase.peerConnectionState != ConnectionStateValue.Connected)
		{
			if ((int)DebugOut >= 1)
			{
				Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: " + operationCode + " Not connected. PeerState: " + peerBase.peerConnectionState);
			}
			Listener.OnStatusChanged(StatusCode.SendError);
			return false;
		}
		if (sendOptions.Channel >= ChannelCount)
		{
			if ((int)DebugOut >= 1)
			{
				Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Selected channel (" + sendOptions.Channel + ")>= channelCount (" + ChannelCount + ").");
			}
			Listener.OnStatusChanged(StatusCode.SendError);
			return false;
		}
		lock (EnqueueLock)
		{
			StreamBuffer opBytes = peerBase.SerializeOperationToMessage(operationCode, operationParameters, EgMessageType.Operation, sendOptions.Encrypt);
			return peerBase.EnqueuePhotonMessage(opBytes, sendOptions);
		}
	}

	public static bool RegisterType(Type customType, byte code, SerializeMethod serializeMethod, DeserializeMethod constructor)
	{
		return Protocol.TryRegisterType(customType, code, serializeMethod, constructor);
	}

	public static bool RegisterType(Type customType, byte code, SerializeStreamMethod serializeMethod, DeserializeStreamMethod constructor)
	{
		return Protocol.TryRegisterType(customType, code, serializeMethod, constructor);
	}
}
