using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace ExitGames.Client.Photon;

internal class EnetPeer : PeerBase
{
	private const int CRC_LENGTH = 4;

	private const int EncryptedDataGramHeaderSize = 7;

	private const int EncryptedHeaderSize = 5;

	private const int QUICK_RESEND_QUEUELIMIT = 25;

	internal NCommandPool nCommandPool = new NCommandPool();

	private List<NCommand> sentReliableCommands = new List<NCommand>();

	private int sendWindowUpdateRequiredBackValue = 0;

	private StreamBuffer outgoingAcknowledgementsPool;

	internal const int UnsequencedWindowSize = 128;

	internal readonly int[] unsequencedWindow = new int[4];

	internal int outgoingUnsequencedGroupNumber;

	internal int incomingUnsequencedGroupNumber;

	private byte udpCommandCount;

	private byte[] udpBuffer;

	private int udpBufferIndex;

	private byte[] bufferForEncryption;

	private int commandBufferSize = 100;

	internal int challenge;

	internal int reliableCommandsRepeated;

	internal int reliableCommandsSent;

	internal int serverSentTime;

	internal static readonly byte[] udpHeader0xF3 = new byte[2] { 243, 2 };

	protected bool datagramEncryptedConnection;

	private EnetChannel[] channelArray = new EnetChannel[0];

	private const byte ControlChannelNumber = byte.MaxValue;

	protected internal const short PeerIdForConnect = -1;

	protected internal const short PeerIdForConnectTrace = -2;

	private Queue<int> commandsToRemove = new Queue<int>();

	private int fragmentLength = 0;

	private int fragmentLengthDatagramEncrypt = 0;

	private int fragmentLengthMtuValue = 0;

	private Queue<NCommand> CommandQueue = new Queue<NCommand>();

	private readonly HashSet<byte> channelsToUpdateLowestSent = new HashSet<byte>();

	private int[] lowestSentSequenceNumber;

	internal override int QueuedIncomingCommandsCount
	{
		get
		{
			int num = 0;
			lock (channelArray)
			{
				for (int i = 0; i < channelArray.Length; i++)
				{
					EnetChannel enetChannel = channelArray[i];
					num += enetChannel.incomingReliableCommandsList.Count;
					num += enetChannel.incomingUnreliableCommandsList.Count;
				}
			}
			return num;
		}
	}

	internal override int QueuedOutgoingCommandsCount
	{
		get
		{
			int num = 0;
			lock (channelArray)
			{
				for (int i = 0; i < channelArray.Length; i++)
				{
					EnetChannel enetChannel = channelArray[i];
					lock (enetChannel)
					{
						num += enetChannel.outgoingReliableCommandsList.Count;
						num += enetChannel.outgoingUnreliableCommandsList.Count;
					}
				}
			}
			return num;
		}
	}

	internal override int SentReliableCommandsCount => sentReliableCommands.Count;

	private bool sendWindowUpdateRequired
	{
		get
		{
			return Interlocked.CompareExchange(ref sendWindowUpdateRequiredBackValue, 1, 1) == 1;
		}
		set
		{
			if (value)
			{
				Interlocked.CompareExchange(ref sendWindowUpdateRequiredBackValue, 1, 0);
			}
			else
			{
				Interlocked.CompareExchange(ref sendWindowUpdateRequiredBackValue, 0, 1);
			}
		}
	}

	internal EnetPeer()
	{
		TrafficPackageHeaderSize = 12;
		messageHeader = udpHeader0xF3;
	}

	internal override bool IsTransportEncrypted()
	{
		return datagramEncryptedConnection;
	}

	internal override void Reset()
	{
		base.Reset();
		if (photonPeer.PayloadEncryptionSecret != null && usedTransportProtocol == ConnectionProtocol.Udp)
		{
			InitEncryption(photonPeer.PayloadEncryptionSecret);
		}
		if (photonPeer.Encryptor != null)
		{
			isEncryptionAvailable = true;
		}
		peerID = (short)(photonPeer.EnableServerTracing ? (-2) : (-1));
		challenge = SupportClass.ThreadSafeRandom.Next();
		if (udpBuffer == null || udpBuffer.Length != base.mtu)
		{
			udpBuffer = new byte[base.mtu];
		}
		reliableCommandsSent = 0;
		reliableCommandsRepeated = 0;
		timeoutInt = 0;
		outgoingUnsequencedGroupNumber = 0;
		incomingUnsequencedGroupNumber = 0;
		for (int i = 0; i < unsequencedWindow.Length; i++)
		{
			unsequencedWindow[i] = 0;
		}
		lock (channelArray)
		{
			EnetChannel[] array = channelArray;
			if (array.Length != base.ChannelCount + 1)
			{
				array = new EnetChannel[base.ChannelCount + 1];
			}
			for (byte b = 0; b < base.ChannelCount; b++)
			{
				array[b] = new EnetChannel(b, commandBufferSize);
			}
			array[base.ChannelCount] = new EnetChannel(byte.MaxValue, commandBufferSize);
			channelArray = array;
		}
		lock (sentReliableCommands)
		{
			sentReliableCommands.Clear();
		}
		outgoingAcknowledgementsPool = new StreamBuffer();
	}

	internal void ApplyRandomizedSequenceNumbers()
	{
		if (!photonPeer.RandomizeSequenceNumbers)
		{
			return;
		}
		lock (channelArray)
		{
			for (int i = 0; i < channelArray.Length; i++)
			{
				EnetChannel enetChannel = channelArray[i];
				int num = photonPeer.RandomizedSequenceNumbers[i % photonPeer.RandomizedSequenceNumbers.Length];
				if (photonPeer.GcmDatagramEncryption)
				{
					enetChannel.incomingReliableSequenceNumber += num;
					enetChannel.outgoingReliableSequenceNumber += num;
					enetChannel.highestReceivedAck += num;
					enetChannel.outgoingReliableUnsequencedNumber += num;
				}
				else
				{
					enetChannel.incomingReliableSequenceNumber = num;
					enetChannel.outgoingReliableSequenceNumber = num;
					enetChannel.highestReceivedAck = num;
					enetChannel.outgoingReliableUnsequencedNumber = num;
				}
			}
		}
	}

	private EnetChannel GetChannel(byte channelNumber)
	{
		return (channelNumber == byte.MaxValue) ? channelArray[channelArray.Length - 1] : channelArray[channelNumber];
	}

	internal override bool Connect(string ipport, string proxyServerAddress, string appID, object photonToken)
	{
		if (PhotonSocket.Connect())
		{
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsOutgoing.ControlCommandBytes += 44;
				base.TrafficStatsOutgoing.ControlCommandCount++;
			}
			peerConnectionState = ConnectionStateValue.Connecting;
			return true;
		}
		return false;
	}

	public override void OnConnect()
	{
		QueueOutgoingReliableCommand(nCommandPool.Acquire(this, 2, null, byte.MaxValue));
	}

	internal override void Disconnect()
	{
		if (peerConnectionState == ConnectionStateValue.Disconnected || peerConnectionState == ConnectionStateValue.Disconnecting)
		{
			return;
		}
		if (sentReliableCommands != null)
		{
			lock (sentReliableCommands)
			{
				sentReliableCommands.Clear();
			}
		}
		lock (channelArray)
		{
			EnetChannel[] array = channelArray;
			foreach (EnetChannel enetChannel in array)
			{
				enetChannel.clearAll();
			}
		}
		bool isSimulationEnabled = base.NetworkSimulationSettings.IsSimulationEnabled;
		base.NetworkSimulationSettings.IsSimulationEnabled = false;
		NCommand nCommand = nCommandPool.Acquire(this, 4, null, byte.MaxValue);
		peerConnectionState = ConnectionStateValue.Disconnecting;
		QueueOutgoingReliableCommand(nCommand);
		SendOutgoingCommands();
		if (base.TrafficStatsEnabled)
		{
			base.TrafficStatsOutgoing.CountControlCommand(nCommand.Size);
		}
		base.NetworkSimulationSettings.IsSimulationEnabled = isSimulationEnabled;
		PhotonSocket.Disconnect();
		peerConnectionState = ConnectionStateValue.Disconnected;
		EnqueueStatusCallback(StatusCode.Disconnect);
		lock (udpBuffer)
		{
			datagramEncryptedConnection = false;
		}
	}

	internal override void StopConnection()
	{
		if (PhotonSocket != null)
		{
			PhotonSocket.Disconnect();
		}
		peerConnectionState = ConnectionStateValue.Disconnected;
		if (base.Listener != null)
		{
			base.Listener.OnStatusChanged(StatusCode.Disconnect);
		}
	}

	internal override void FetchServerTimestamp()
	{
		if (peerConnectionState != ConnectionStateValue.Connected || !ApplicationIsInitialized)
		{
			if ((int)base.debugOut >= 3)
			{
				EnqueueDebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + peerConnectionState);
			}
		}
		else
		{
			CreateAndEnqueueCommand(12, null, byte.MaxValue);
		}
	}

	internal override bool DispatchIncomingCommands()
	{
		int count = CommandQueue.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				lock (CommandQueue)
				{
					NCommand command = CommandQueue.Dequeue();
					ExecuteCommand(command);
				}
			}
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
				goto IL_00ba;
			}
			IL_00ba:
			myAction();
		}
		NCommand val = null;
		lock (channelArray)
		{
			for (int j = 0; j < channelArray.Length; j++)
			{
				EnetChannel enetChannel = channelArray[j];
				if (enetChannel.incomingUnsequencedCommandsList.Count > 0)
				{
					val = enetChannel.incomingUnsequencedCommandsList.Dequeue();
					break;
				}
				if (enetChannel.incomingUnreliableCommandsList.Count > 0)
				{
					int num = int.MaxValue;
					foreach (int key2 in enetChannel.incomingUnreliableCommandsList.Keys)
					{
						NCommand nCommand = enetChannel.incomingUnreliableCommandsList[key2];
						if (key2 < enetChannel.incomingUnreliableSequenceNumber || nCommand.reliableSequenceNumber < enetChannel.incomingReliableSequenceNumber)
						{
							photonPeer.CountDiscarded++;
							commandsToRemove.Enqueue(key2);
						}
						else if (key2 < num && nCommand.reliableSequenceNumber <= enetChannel.incomingReliableSequenceNumber)
						{
							num = key2;
						}
					}
					NonAllocDictionary<int, NCommand> incomingUnreliableCommandsList = enetChannel.incomingUnreliableCommandsList;
					while (commandsToRemove.Count > 0)
					{
						int key = commandsToRemove.Dequeue();
						NCommand nCommand2 = incomingUnreliableCommandsList[key];
						incomingUnreliableCommandsList.Remove(key);
						nCommand2.FreePayload();
						nCommand2.Release();
					}
					if (num < int.MaxValue)
					{
						photonPeer.DeltaUnreliableNumber = num - enetChannel.incomingUnreliableSequenceNumber;
						val = enetChannel.incomingUnreliableCommandsList[num];
					}
					if (val != null)
					{
						enetChannel.incomingUnreliableCommandsList.Remove(val.unreliableSequenceNumber);
						enetChannel.incomingUnreliableSequenceNumber = val.unreliableSequenceNumber;
						break;
					}
				}
				if (val != null || enetChannel.incomingReliableCommandsList.Count <= 0)
				{
					continue;
				}
				enetChannel.incomingReliableCommandsList.TryGetValue(enetChannel.incomingReliableSequenceNumber + 1, out val);
				if (val != null)
				{
					if (val.commandType != 8)
					{
						enetChannel.incomingReliableSequenceNumber = val.reliableSequenceNumber;
						enetChannel.incomingReliableCommandsList.Remove(val.reliableSequenceNumber);
					}
					else if (val.fragmentsRemaining > 0)
					{
						val = null;
					}
					else
					{
						enetChannel.incomingReliableSequenceNumber = val.reliableSequenceNumber + val.fragmentCount - 1;
						enetChannel.incomingReliableCommandsList.Remove(val.reliableSequenceNumber);
					}
					break;
				}
			}
		}
		if (val != null && val.Payload != null)
		{
			ByteCountCurrentDispatch = val.Size;
			CommandInCurrentDispatch = val;
			bool flag = DeserializeMessageAndCallback(val.Payload);
			CommandInCurrentDispatch = null;
			val.FreePayload();
			val.Release();
			return true;
		}
		return false;
	}

	private int GetFragmentLength()
	{
		if (fragmentLength == 0 || base.mtu != fragmentLengthMtuValue)
		{
			fragmentLengthMtuValue = base.mtu;
			fragmentLength = base.mtu - 12 - 36;
			fragmentLengthDatagramEncrypt = ((photonPeer.Encryptor != null) ? photonPeer.Encryptor.CalculateFragmentLength() : 0);
		}
		return datagramEncryptedConnection ? fragmentLengthDatagramEncrypt : fragmentLength;
	}

	private int CalculatePacketSize(int inSize)
	{
		if (datagramEncryptedConnection)
		{
			return photonPeer.Encryptor.CalculateEncryptedSize(inSize + 7);
		}
		return inSize;
	}

	private int CalculateInitialOffset()
	{
		if (datagramEncryptedConnection)
		{
			return 5;
		}
		int num = 12;
		if (photonPeer.CrcEnabled)
		{
			num += 4;
		}
		return num;
	}

	internal override bool SendAcksOnly()
	{
		if (peerConnectionState == ConnectionStateValue.Disconnected)
		{
			return false;
		}
		if (PhotonSocket == null || !PhotonSocket.Connected)
		{
			return false;
		}
		lock (udpBuffer)
		{
			int num = 0;
			udpBufferIndex = CalculateInitialOffset();
			udpCommandCount = 0;
			lock (outgoingAcknowledgementsPool)
			{
				num = SerializeAckToBuffer();
				timeLastSendAck = base.timeInt;
			}
			if (base.timeInt > timeoutInt && sentReliableCommands.Count > 0)
			{
				int num2 = base.timeInt + 100;
				lock (sentReliableCommands)
				{
					int num3 = 0;
					for (int i = 0; i < sentReliableCommands.Count; i++)
					{
						NCommand nCommand = sentReliableCommands[i];
						int num4 = nCommand.commandSentTime + nCommand.roundTripTimeout;
						if (base.timeInt > num4)
						{
							bool flag = SerializeCommandToBuffer(nCommand, commandIsInSentQueue: true);
							if (flag)
							{
								if ((int)base.debugOut >= 5)
								{
									base.Listener.DebugReturn(DebugLevel.ALL, $"Resending: {nCommand}. now: {base.timeInt} rtt/var: {roundTripTime}/{roundTripTimeVariance} last recv: {base.timeInt - timestampOfLastReceive} didFit: {flag}");
								}
								reliableCommandsRepeated++;
							}
							else
							{
								num3++;
								num2 = timeoutInt;
								if (base.mtu - udpBufferIndex < 80)
								{
									break;
								}
							}
						}
						else if (num4 < num2)
						{
							num2 = num4;
						}
					}
					num += num3;
					timeoutInt = num2;
				}
			}
			if (udpCommandCount <= 0)
			{
				return false;
			}
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsOutgoing.TotalPacketCount++;
				base.TrafficStatsOutgoing.TotalCommandsInPackets += udpCommandCount;
			}
			SendData(udpBuffer, udpBufferIndex);
			return num > 0;
		}
	}

	internal override bool SendOutgoingCommands()
	{
		if (peerConnectionState == ConnectionStateValue.Disconnected)
		{
			return false;
		}
		if (!PhotonSocket.Connected)
		{
			return false;
		}
		lock (udpBuffer)
		{
			int num = 0;
			udpBufferIndex = CalculateInitialOffset();
			udpCommandCount = 0;
			timeLastSendOutgoing = base.timeInt;
			lock (outgoingAcknowledgementsPool)
			{
				if (outgoingAcknowledgementsPool.Length > 0)
				{
					num = SerializeAckToBuffer();
					timeLastSendAck = base.timeInt;
				}
			}
			if (base.timeInt > timeoutInt && sentReliableCommands.Count > 0)
			{
				int num2 = base.timeInt + 100;
				lock (sentReliableCommands)
				{
					int num3 = 0;
					for (int i = 0; i < sentReliableCommands.Count; i++)
					{
						NCommand nCommand = sentReliableCommands[i];
						int num4 = nCommand.commandSentTime + nCommand.roundTripTimeout;
						if (base.timeInt > num4)
						{
							if (nCommand.commandSentCount > photonPeer.SentCountAllowance || base.timeInt > nCommand.timeoutTime)
							{
								if ((int)base.debugOut >= 2)
								{
									base.Listener.DebugReturn(DebugLevel.WARNING, $"Timeout-disconnect! Command: {nCommand} now: {base.timeInt} challenge: {Convert.ToString(challenge, 16)}");
									if ((int)base.debugOut >= 3)
									{
										base.Listener.DebugReturn(DebugLevel.INFO, $"QueuedOutgoing: {QueuedOutgoingCommandsCount} channel.LowestUnAckd: {GetChannel(nCommand.commandChannelID).lowestUnacknowledgedSequenceNumber} sentReliableCommands: {sentReliableCommands.Count}");
									}
								}
								peerConnectionState = ConnectionStateValue.Zombie;
								EnqueueStatusCallback(StatusCode.TimeoutDisconnect);
								Disconnect();
								nCommand.Release();
								return false;
							}
							if (SerializeCommandToBuffer(nCommand, commandIsInSentQueue: true))
							{
								if ((int)base.debugOut >= 5)
								{
									base.Listener.DebugReturn(DebugLevel.ALL, $"Resending: {nCommand}. now: {base.timeInt} rtt/var: {roundTripTime}/{roundTripTimeVariance} last recv: {base.timeInt - timestampOfLastReceive}");
								}
								reliableCommandsRepeated++;
							}
							else
							{
								num3++;
								num2 = timeoutInt;
								if (base.mtu - udpBufferIndex < 80)
								{
									break;
								}
							}
						}
						else if (num4 < num2)
						{
							num2 = num4;
						}
					}
					num += num3;
					timeoutInt = num2;
				}
			}
			if (peerConnectionState == ConnectionStateValue.Connected && base.timePingInterval > 0 && sentReliableCommands.Count == 0 && base.timeInt - timeLastAckReceive > base.timePingInterval && CalculatePacketSize(udpBufferIndex + 12) <= base.mtu)
			{
				NCommand nCommand2 = nCommandPool.Acquire(this, 5, null, byte.MaxValue);
				QueueOutgoingReliableCommand(nCommand2);
				if (base.TrafficStatsEnabled)
				{
					base.TrafficStatsOutgoing.CountControlCommand(nCommand2.Size);
				}
			}
			if (sendWindowUpdateRequired)
			{
				UpdateSendWindow();
			}
			lock (channelArray)
			{
				for (int j = 0; j < channelArray.Length; j++)
				{
					EnetChannel enetChannel = channelArray[j];
					lock (enetChannel)
					{
						int channelSequenceLimit = enetChannel.lowestUnacknowledgedSequenceNumber + photonPeer.SendWindowSize;
						num += SerializeToBuffer(enetChannel.outgoingReliableCommandsList, channelSequenceLimit);
						num += SerializeToBuffer(enetChannel.outgoingUnreliableCommandsList, channelSequenceLimit);
					}
				}
			}
			if (udpCommandCount <= 0)
			{
				return false;
			}
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsOutgoing.TotalPacketCount++;
				base.TrafficStatsOutgoing.TotalCommandsInPackets += udpCommandCount;
			}
			SendData(udpBuffer, udpBufferIndex);
			return num > 0;
		}
	}

	private void UpdateSendWindow()
	{
		sendWindowUpdateRequired = false;
		if (photonPeer.SendWindowSize <= 0)
		{
			return;
		}
		if (sentReliableCommands.Count == 0)
		{
			lock (channelArray)
			{
				for (int i = 0; i < channelArray.Length; i++)
				{
					EnetChannel enetChannel = channelArray[i];
					enetChannel.reliableCommandsInFlight = 0;
					enetChannel.lowestUnacknowledgedSequenceNumber = enetChannel.highestReceivedAck + 1;
				}
				return;
			}
		}
		channelsToUpdateLowestSent.Clear();
		lock (channelArray)
		{
			for (int j = 0; j < channelArray.Length; j++)
			{
				EnetChannel enetChannel2 = channelArray[j];
				if (enetChannel2.ChannelNumber != byte.MaxValue && enetChannel2.reliableCommandsInFlight > 0)
				{
					channelsToUpdateLowestSent.Add(enetChannel2.ChannelNumber);
				}
			}
		}
		if (lowestSentSequenceNumber == null || lowestSentSequenceNumber.Length != channelArray.Length)
		{
			lowestSentSequenceNumber = new int[channelArray.Length];
		}
		else
		{
			for (int k = 0; k < lowestSentSequenceNumber.Length; k++)
			{
				lowestSentSequenceNumber[k] = 0;
			}
		}
		lock (sentReliableCommands)
		{
			for (int l = 0; l < sentReliableCommands.Count; l++)
			{
				NCommand nCommand = sentReliableCommands[l];
				if (nCommand.IsFlaggedUnsequenced || nCommand.commandChannelID == byte.MaxValue)
				{
					continue;
				}
				int commandChannelID = nCommand.commandChannelID;
				if (channelsToUpdateLowestSent.Contains(nCommand.commandChannelID))
				{
					if (lowestSentSequenceNumber[commandChannelID] == 0)
					{
						lowestSentSequenceNumber[commandChannelID] = nCommand.reliableSequenceNumber;
					}
					channelsToUpdateLowestSent.Remove(nCommand.commandChannelID);
					if (channelsToUpdateLowestSent.Count == 0)
					{
						break;
					}
				}
			}
		}
		lock (channelArray)
		{
			for (int m = 0; m < channelArray.Length; m++)
			{
				EnetChannel enetChannel3 = channelArray[m];
				enetChannel3.lowestUnacknowledgedSequenceNumber = ((lowestSentSequenceNumber[m] > 0) ? lowestSentSequenceNumber[m] : (enetChannel3.highestReceivedAck + 1));
			}
		}
	}

	internal override bool EnqueuePhotonMessage(StreamBuffer opBytes, SendOptions sendParams)
	{
		byte commandType = 7;
		if (sendParams.DeliveryMode == DeliveryMode.UnreliableUnsequenced)
		{
			commandType = 11;
		}
		else if (sendParams.DeliveryMode == DeliveryMode.ReliableUnsequenced)
		{
			commandType = 14;
		}
		else if (sendParams.DeliveryMode == DeliveryMode.Reliable)
		{
			commandType = 6;
		}
		return CreateAndEnqueueCommand(commandType, opBytes, sendParams.Channel);
	}

	internal bool CreateAndEnqueueCommand(byte commandType, StreamBuffer payload, byte channelNumber)
	{
		EnetChannel channel = GetChannel(channelNumber);
		ByteCountLastOperation = 0;
		int num = GetFragmentLength();
		if (num == 0)
		{
			num = 1000;
			EnqueueDebugReturn(DebugLevel.WARNING, "Value of currentFragmentSize should not be 0. Corrected to 1000.");
		}
		if (payload == null || payload.Length <= num)
		{
			NCommand nCommand = nCommandPool.Acquire(this, commandType, payload, channel.ChannelNumber);
			if (nCommand.IsFlaggedReliable)
			{
				QueueOutgoingReliableCommand(nCommand);
				ByteCountLastOperation = nCommand.Size;
				if (base.TrafficStatsEnabled)
				{
					base.TrafficStatsOutgoing.CountReliableOpCommand(nCommand.Size);
					base.TrafficStatsGameLevel.CountOperation(nCommand.Size);
				}
			}
			else
			{
				QueueOutgoingUnreliableCommand(nCommand);
				ByteCountLastOperation = nCommand.Size;
				if (base.TrafficStatsEnabled)
				{
					base.TrafficStatsOutgoing.CountUnreliableOpCommand(nCommand.Size);
					base.TrafficStatsGameLevel.CountOperation(nCommand.Size);
				}
			}
		}
		else
		{
			bool flag = commandType == 14 || commandType == 11;
			int fragmentCount = (payload.Length + num - 1) / num;
			int startSequenceNumber = (flag ? channel.outgoingReliableUnsequencedNumber : channel.outgoingReliableSequenceNumber) + 1;
			byte[] buffer = payload.GetBuffer();
			int num2 = 0;
			for (int i = 0; i < payload.Length; i += num)
			{
				if (payload.Length - i < num)
				{
					num = payload.Length - i;
				}
				StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
				streamBuffer.Write(buffer, i, num);
				NCommand nCommand2 = nCommandPool.Acquire(this, (byte)(flag ? 15 : 8), streamBuffer, channel.ChannelNumber);
				nCommand2.fragmentNumber = num2;
				nCommand2.startSequenceNumber = startSequenceNumber;
				nCommand2.fragmentCount = fragmentCount;
				nCommand2.totalLength = payload.Length;
				nCommand2.fragmentOffset = i;
				QueueOutgoingReliableCommand(nCommand2);
				ByteCountLastOperation += nCommand2.Size;
				if (base.TrafficStatsEnabled)
				{
					base.TrafficStatsOutgoing.CountFragmentOpCommand(nCommand2.Size);
					base.TrafficStatsGameLevel.CountOperation(nCommand2.Size);
				}
				num2++;
			}
			PeerBase.MessageBufferPoolPut(payload);
		}
		return true;
	}

	internal int SerializeAckToBuffer()
	{
		outgoingAcknowledgementsPool.Seek(0L, SeekOrigin.Begin);
		while (outgoingAcknowledgementsPool.Position + 20 <= outgoingAcknowledgementsPool.Length)
		{
			if (CalculatePacketSize(udpBufferIndex + 20) > base.mtu)
			{
				if ((int)base.debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "UDP package is full. Commands in Package: " + udpCommandCount + ". bytes left in queue: " + outgoingAcknowledgementsPool.Position);
				}
				break;
			}
			int offset;
			byte[] bufferAndAdvance = outgoingAcknowledgementsPool.GetBufferAndAdvance(20, out offset);
			Buffer.BlockCopy(bufferAndAdvance, offset, udpBuffer, udpBufferIndex, 20);
			udpBufferIndex += 20;
			udpCommandCount++;
		}
		outgoingAcknowledgementsPool.Compact();
		outgoingAcknowledgementsPool.Position = outgoingAcknowledgementsPool.Length;
		return outgoingAcknowledgementsPool.Length / 20;
	}

	internal int SerializeToBuffer(Queue<NCommand> commandList, int channelSequenceLimit)
	{
		while (commandList.Count > 0)
		{
			NCommand nCommand = commandList.Peek();
			if (nCommand == null)
			{
				commandList.Dequeue();
				continue;
			}
			if (nCommand.IsFlaggedReliable && nCommand.commandChannelID != byte.MaxValue && photonPeer.SendWindowSize > 0 && nCommand.reliableSequenceNumber >= channelSequenceLimit)
			{
				return 0;
			}
			if (SerializeCommandToBuffer(nCommand))
			{
				commandList.Dequeue();
				continue;
			}
			if ((int)base.debugOut >= 5)
			{
				base.Listener.DebugReturn(DebugLevel.ALL, "UDP package is full. Commands in Package: " + udpCommandCount + " commandList.Count: " + commandList.Count);
			}
			break;
		}
		return commandList.Count;
	}

	private bool SerializeCommandToBuffer(NCommand command, bool commandIsInSentQueue = false)
	{
		if (command == null)
		{
			return true;
		}
		if (CalculatePacketSize(udpBufferIndex + command.Size) > base.mtu)
		{
			return false;
		}
		command.SerializeHeader(udpBuffer, ref udpBufferIndex);
		if (command.SizeOfPayload > 0)
		{
			Buffer.BlockCopy(command.Serialize(), 0, udpBuffer, udpBufferIndex, command.SizeOfPayload);
			udpBufferIndex += command.SizeOfPayload;
		}
		udpCommandCount++;
		if (command.IsFlaggedReliable)
		{
			QueueSentCommand(command, commandIsInSentQueue);
		}
		else
		{
			command.FreePayload();
			command.Release();
		}
		return true;
	}

	internal void SendData(byte[] data, int length)
	{
		try
		{
			if (datagramEncryptedConnection)
			{
				SendDataEncrypted(data, length);
				return;
			}
			int targetOffset = 0;
			Protocol.Serialize(peerID, data, ref targetOffset);
			data[2] = (byte)(photonPeer.CrcEnabled ? 204 : 0);
			data[3] = udpCommandCount;
			targetOffset = 4;
			Protocol.Serialize(base.timeInt, data, ref targetOffset);
			Protocol.Serialize(challenge, data, ref targetOffset);
			if (photonPeer.CrcEnabled)
			{
				Protocol.Serialize(0, data, ref targetOffset);
				uint value = SupportClass.CalculateCrc(data, length);
				targetOffset -= 4;
				Protocol.Serialize((int)value, data, ref targetOffset);
			}
			SendToSocket(data, length);
		}
		catch (Exception ex)
		{
			if ((int)base.debugOut >= 1)
			{
				base.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
			}
			SupportClass.WriteStackTrace(ex);
		}
	}

	private void SendToSocket(byte[] data, int length)
	{
		bytesOut += length;
		ITrafficRecorder trafficRecorder = photonPeer.TrafficRecorder;
		if (trafficRecorder != null && trafficRecorder.Enabled)
		{
			trafficRecorder.Record(data, length, incoming: false, peerID, PhotonSocket);
		}
		if (base.NetworkSimulationSettings.IsSimulationEnabled)
		{
			byte[] array = new byte[length];
			Buffer.BlockCopy(data, 0, array, 0, length);
			SendNetworkSimulated(array);
			return;
		}
		int num = base.timeInt;
		PhotonSocket.Send(data, length);
		int num2 = base.timeInt - num;
		if (num2 > longestSentCall)
		{
			longestSentCall = num2;
		}
	}

	private void SendDataEncrypted(byte[] data, int length)
	{
		if (bufferForEncryption == null || bufferForEncryption.Length != base.mtu)
		{
			bufferForEncryption = new byte[base.mtu];
		}
		byte[] array = bufferForEncryption;
		int targetOffset = 0;
		Protocol.Serialize(peerID, array, ref targetOffset);
		array[2] = 1;
		targetOffset++;
		Protocol.Serialize(challenge, array, ref targetOffset);
		data[0] = udpCommandCount;
		int targetOffset2 = 1;
		Protocol.Serialize(base.timeInt, data, ref targetOffset2);
		int outSize = array.Length - targetOffset;
		photonPeer.Encryptor.Encrypt2(data, length, array, array, targetOffset, ref outSize);
		SendToSocket(array, outSize + targetOffset);
	}

	internal void QueueSentCommand(NCommand command, bool commandIsAlreadyInSentQueue = false)
	{
		command.commandSentTime = base.timeInt;
		if (command.roundTripTimeout == 0)
		{
			command.roundTripTimeout = Math.Min(roundTripTime + 4 * roundTripTimeVariance, photonPeer.InitialResendTimeMax);
			command.timeoutTime = base.timeInt + base.DisconnectTimeout;
			reliableCommandsSent++;
		}
		else if (command.commandSentCount > photonPeer.QuickResendAttempts || sentReliableCommands.Count >= 25)
		{
			command.roundTripTimeout *= 2;
		}
		command.commandSentCount++;
		int num = command.commandSentTime + command.roundTripTimeout;
		if (num < timeoutInt)
		{
			timeoutInt = num;
		}
		if (!commandIsAlreadyInSentQueue)
		{
			EnetChannel channel = GetChannel(command.commandChannelID);
			channel.reliableCommandsInFlight++;
			lock (sentReliableCommands)
			{
				sentReliableCommands.Add(command);
			}
		}
	}

	internal void QueueOutgoingReliableCommand(NCommand command)
	{
		EnetChannel channel = GetChannel(command.commandChannelID);
		lock (channel)
		{
			if (command.reliableSequenceNumber == 0)
			{
				if (command.IsFlaggedUnsequenced)
				{
					command.reliableSequenceNumber = ++channel.outgoingReliableUnsequencedNumber;
				}
				else
				{
					command.reliableSequenceNumber = ++channel.outgoingReliableSequenceNumber;
				}
			}
			channel.outgoingReliableCommandsList.Enqueue(command);
		}
	}

	internal void QueueOutgoingUnreliableCommand(NCommand command)
	{
		EnetChannel channel = GetChannel(command.commandChannelID);
		lock (channel)
		{
			if (command.IsFlaggedUnsequenced)
			{
				command.reliableSequenceNumber = 0;
				command.unsequencedGroupNumber = ++outgoingUnsequencedGroupNumber;
			}
			else
			{
				command.reliableSequenceNumber = channel.outgoingReliableSequenceNumber;
				command.unreliableSequenceNumber = ++channel.outgoingUnreliableSequenceNumber;
			}
			if (!photonPeer.SendInCreationOrder)
			{
				channel.outgoingUnreliableCommandsList.Enqueue(command);
			}
			else
			{
				channel.outgoingReliableCommandsList.Enqueue(command);
			}
		}
	}

	internal void QueueOutgoingAcknowledgement(NCommand readCommand, int sendTime)
	{
		lock (outgoingAcknowledgementsPool)
		{
			int offset;
			byte[] bufferAndAdvance = outgoingAcknowledgementsPool.GetBufferAndAdvance(20, out offset);
			NCommand.CreateAck(bufferAndAdvance, offset, readCommand, sendTime);
		}
	}

	internal override void ReceiveIncomingCommands(byte[] inBuff, int inDataLength)
	{
		timestampOfLastReceive = base.timeInt;
		if (peerConnectionState == ConnectionStateValue.Disconnected)
		{
			return;
		}
		try
		{
			int offset = 0;
			Protocol.Deserialize(out short _, inBuff, ref offset);
			byte b = inBuff[offset++];
			int value2;
			byte b2;
			if (b == 1)
			{
				if (photonPeer.Encryptor == null)
				{
					return;
				}
				Protocol.Deserialize(out value2, inBuff, ref offset);
				if (value2 != challenge)
				{
					packetLossByChallenge++;
					return;
				}
				inBuff = photonPeer.Encryptor.Decrypt2(inBuff, offset, inDataLength - offset, inBuff, out var _);
				if (!datagramEncryptedConnection)
				{
					lock (udpBuffer)
					{
						datagramEncryptedConnection = true;
						fragmentLength = 0;
					}
				}
				offset = 0;
				b2 = inBuff[offset++];
				Protocol.Deserialize(out serverSentTime, inBuff, ref offset);
				bytesIn += inDataLength;
			}
			else
			{
				if (datagramEncryptedConnection)
				{
					if ((int)base.debugOut >= 2)
					{
						EnqueueDebugReturn(DebugLevel.WARNING, "Ignored received package. Connection requires Datagram Encryption but received unencrypted datagram.");
					}
					return;
				}
				b2 = inBuff[offset++];
				Protocol.Deserialize(out serverSentTime, inBuff, ref offset);
				Protocol.Deserialize(out value2, inBuff, ref offset);
				if (value2 != challenge)
				{
					packetLossByChallenge++;
					if (peerConnectionState != 0 && (int)base.debugOut >= 5)
					{
						EnqueueDebugReturn(DebugLevel.ALL, "Ignored received package due to wrong challenge. Received:" + value2 + " local: " + challenge);
					}
					return;
				}
				if (b == 204)
				{
					Protocol.Deserialize(out int value3, inBuff, ref offset);
					bytesIn += 4L;
					offset -= 4;
					Protocol.Serialize(0, inBuff, ref offset);
					uint num = SupportClass.CalculateCrc(inBuff, inDataLength);
					if (value3 != (int)num)
					{
						packetLossByCrc++;
						if (peerConnectionState != 0 && (int)base.debugOut >= 3)
						{
							EnqueueDebugReturn(DebugLevel.INFO, $"Ignored package due to wrong CRC. Incoming:  {(uint)value3:X} Local: {num:X}");
						}
						return;
					}
				}
				bytesIn += 12L;
			}
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsIncoming.TotalPacketCount++;
				base.TrafficStatsIncoming.TotalCommandsInPackets += b2;
			}
			if (b2 > commandBufferSize || b2 <= 0)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, "too many/few incoming commands in package: " + b2 + " > " + commandBufferSize);
			}
			bool flag = false;
			for (int i = 0; i < b2; i++)
			{
				NCommand nCommand = nCommandPool.Acquire(this, inBuff, ref offset);
				if (nCommand.commandType == 1 || nCommand.commandType == 16)
				{
					ExecuteCommand(nCommand);
					flag = true;
				}
				else
				{
					lock (CommandQueue)
					{
						CommandQueue.Enqueue(nCommand);
					}
				}
				if (nCommand.IsFlaggedReliable)
				{
					QueueOutgoingAcknowledgement(nCommand, serverSentTime);
					if (base.TrafficStatsEnabled)
					{
						base.TrafficStatsIncoming.TimestampOfLastReliableCommand = base.timeInt;
						base.TrafficStatsOutgoing.CountControlCommand(20);
					}
				}
			}
			if (flag)
			{
				sendWindowUpdateRequired = true;
			}
		}
		catch (Exception ex)
		{
			if ((int)base.debugOut >= 1)
			{
				EnqueueDebugReturn(DebugLevel.ERROR, $"Exception while reading commands from incoming data: {ex}");
			}
			SupportClass.WriteStackTrace(ex);
		}
	}

	internal void ExecuteCommand(NCommand command)
	{
		bool flag = false;
		switch (command.commandType)
		{
		case 2:
		case 5:
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsIncoming.CountControlCommand(command.Size);
			}
			command.Release();
			break;
		case 4:
		{
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsIncoming.CountControlCommand(command.Size);
			}
			StatusCode statusValue = StatusCode.DisconnectByServerReasonUnknown;
			if (command.reservedByte == 1)
			{
				statusValue = StatusCode.DisconnectByServerLogic;
			}
			else if (command.reservedByte == 2)
			{
				statusValue = StatusCode.DisconnectByServerTimeout;
			}
			else if (command.reservedByte == 3)
			{
				statusValue = StatusCode.DisconnectByServerUserLimit;
			}
			if ((int)base.debugOut >= 3)
			{
				base.Listener.DebugReturn(DebugLevel.INFO, "Server " + base.ServerAddress + " sent disconnect. PeerId: " + (ushort)peerID + " RTT/Variance:" + roundTripTime + "/" + roundTripTimeVariance + " reason byte: " + command.reservedByte + " peerConnectionState: " + peerConnectionState);
			}
			if (peerConnectionState != 0 && peerConnectionState != ConnectionStateValue.Disconnecting)
			{
				EnqueueStatusCallback(statusValue);
				Disconnect();
			}
			command.Release();
			break;
		}
		case 1:
		case 16:
		{
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsIncoming.TimestampOfLastAck = base.timeInt;
				base.TrafficStatsIncoming.CountControlCommand(command.Size);
			}
			timeLastAckReceive = base.timeInt;
			lastRoundTripTime = base.timeInt - command.ackReceivedSentTime;
			if (lastRoundTripTime < 0 || lastRoundTripTime > 10000)
			{
				if ((int)base.debugOut >= 3)
				{
					EnqueueDebugReturn(DebugLevel.INFO, "Measured lastRoundtripTime is suspicious: " + lastRoundTripTime + " for command: " + command);
				}
				lastRoundTripTime = roundTripTime * 4;
			}
			NCommand nCommand = RemoveSentReliableCommand(command.ackReceivedReliableSequenceNumber, command.commandChannelID, command.commandType == 16);
			command.Release();
			if (nCommand == null)
			{
				break;
			}
			nCommand.FreePayload();
			EnetChannel channel2 = GetChannel(nCommand.commandChannelID);
			lock (channel2)
			{
				if (nCommand.reliableSequenceNumber > channel2.highestReceivedAck)
				{
					channel2.highestReceivedAck = nCommand.reliableSequenceNumber;
				}
				channel2.reliableCommandsInFlight--;
			}
			if (nCommand.commandType == 12)
			{
				if (lastRoundTripTime <= roundTripTime)
				{
					serverTimeOffset = serverSentTime + (lastRoundTripTime >> 1) - base.timeInt;
					serverTimeOffsetIsAvailable = true;
				}
				else
				{
					FetchServerTimestamp();
				}
			}
			else
			{
				UpdateRoundTripTimeAndVariance(lastRoundTripTime);
				if (nCommand.commandType == 4 && peerConnectionState == ConnectionStateValue.Disconnecting)
				{
					if ((int)base.debugOut >= 3)
					{
						EnqueueDebugReturn(DebugLevel.INFO, "Received ACK for previously sent Disconnect command.");
					}
					EnqueueActionForDispatch(delegate
					{
						PhotonSocket.Disconnect();
					});
				}
				else if (nCommand.commandType == 2 && lastRoundTripTime >= 0)
				{
					if (lastRoundTripTime <= 15)
					{
						roundTripTime = 15;
						roundTripTimeVariance = 5;
					}
					else
					{
						roundTripTime = lastRoundTripTime;
					}
				}
			}
			nCommand.Release();
			break;
		}
		case 6:
		case 7:
		case 11:
		case 14:
			if (base.TrafficStatsEnabled)
			{
				if (command.IsFlaggedReliable)
				{
					base.TrafficStatsIncoming.CountReliableOpCommand(command.Size);
				}
				else
				{
					base.TrafficStatsIncoming.CountUnreliableOpCommand(command.Size);
				}
			}
			if (peerConnectionState != ConnectionStateValue.Connected || !QueueIncomingCommand(command))
			{
				command.Release();
			}
			break;
		case 8:
		case 15:
		{
			if (peerConnectionState != ConnectionStateValue.Connected)
			{
				command.Release();
				break;
			}
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsIncoming.CountFragmentOpCommand(command.Size);
			}
			if (command.fragmentNumber > command.fragmentCount || command.fragmentOffset >= command.totalLength || command.fragmentOffset + command.Payload.Length > command.totalLength)
			{
				if ((int)base.debugOut >= 1)
				{
					base.Listener.DebugReturn(DebugLevel.ERROR, "Received fragment has bad size: " + command);
				}
				command.Release();
				break;
			}
			bool flag2 = command.commandType == 8;
			EnetChannel channel = GetChannel(command.commandChannelID);
			NCommand fragment = null;
			bool flag3 = channel.TryGetFragment(command.startSequenceNumber, flag2, out fragment);
			if (flag3 && fragment.fragmentsRemaining <= 0)
			{
				command.Release();
				break;
			}
			if (!QueueIncomingCommand(command))
			{
				command.Release();
				break;
			}
			if (command.reliableSequenceNumber != command.startSequenceNumber)
			{
				if (flag3)
				{
					fragment.fragmentsRemaining--;
				}
			}
			else
			{
				fragment = command;
				fragment.fragmentsRemaining--;
				NCommand fragment2 = null;
				int num = command.startSequenceNumber + 1;
				while (fragment.fragmentsRemaining > 0 && num < fragment.startSequenceNumber + fragment.fragmentCount)
				{
					if (channel.TryGetFragment(num++, flag2, out fragment2))
					{
						fragment.fragmentsRemaining--;
					}
				}
			}
			if (fragment == null || fragment.fragmentsRemaining > 0)
			{
				break;
			}
			StreamBuffer streamBuffer = PeerBase.MessageBufferPoolGet();
			streamBuffer.Position = 0;
			streamBuffer.SetCapacityMinimum(fragment.totalLength);
			byte[] buffer = streamBuffer.GetBuffer();
			for (int i = fragment.startSequenceNumber; i < fragment.startSequenceNumber + fragment.fragmentCount; i++)
			{
				if (channel.TryGetFragment(i, flag2, out var fragment3))
				{
					Buffer.BlockCopy(fragment3.Payload.GetBuffer(), 0, buffer, fragment3.fragmentOffset, fragment3.Payload.Length);
					fragment3.FreePayload();
					channel.RemoveFragment(fragment3.reliableSequenceNumber, flag2);
					if (fragment3.fragmentNumber > 0)
					{
						fragment3.Release();
					}
					continue;
				}
				throw new Exception("startCommand.fragmentsRemaining was 0 but not all fragments were found to be combined!");
			}
			streamBuffer.SetLength(fragment.totalLength);
			fragment.Payload = streamBuffer;
			fragment.Size = 12 * fragment.fragmentCount + fragment.totalLength;
			if (flag2)
			{
				channel.incomingReliableCommandsList.Add(fragment.startSequenceNumber, fragment);
			}
			else
			{
				channel.incomingUnsequencedCommandsList.Enqueue(fragment);
			}
			break;
		}
		case 3:
			if (base.TrafficStatsEnabled)
			{
				base.TrafficStatsIncoming.CountControlCommand(command.Size);
			}
			if (peerConnectionState == ConnectionStateValue.Connecting)
			{
				byte[] buf = WriteInitRequest();
				CreateAndEnqueueCommand(6, new StreamBuffer(buf), 0);
				if (photonPeer.RandomizeSequenceNumbers)
				{
					ApplyRandomizedSequenceNumbers();
				}
				peerConnectionState = ConnectionStateValue.Connected;
			}
			command.Release();
			break;
		case 9:
		case 10:
		case 12:
		case 13:
			break;
		}
	}

	internal bool QueueIncomingCommand(NCommand command)
	{
		EnetChannel channel = GetChannel(command.commandChannelID);
		if (channel == null)
		{
			if ((int)base.debugOut >= 1)
			{
				base.Listener.DebugReturn(DebugLevel.ERROR, "Received command for non-existing channel: " + command.commandChannelID);
			}
			return false;
		}
		if (command.IsFlaggedReliable)
		{
			if (command.IsFlaggedUnsequenced)
			{
				return channel.QueueIncomingReliableUnsequenced(command);
			}
			if (command.reliableSequenceNumber <= channel.incomingReliableSequenceNumber)
			{
				if ((int)base.debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "incoming command " + command?.ToString() + " is old (not saving it). Dispatched incomingReliableSequenceNumber: " + channel.incomingReliableSequenceNumber);
				}
				return false;
			}
			if (channel.ContainsReliableSequenceNumber(command.reliableSequenceNumber))
			{
				if ((int)base.debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "Info: command was received before! Old/New: " + channel.FetchReliableSequenceNumber(command.reliableSequenceNumber)?.ToString() + "/" + command?.ToString() + " inReliableSeq#: " + channel.incomingReliableSequenceNumber);
				}
				return false;
			}
			channel.incomingReliableCommandsList.Add(command.reliableSequenceNumber, command);
			return true;
		}
		if (command.commandFlags == 0)
		{
			if (command.reliableSequenceNumber < channel.incomingReliableSequenceNumber)
			{
				photonPeer.CountDiscarded++;
				if ((int)base.debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "incoming reliable-seq# < Dispatched-rel-seq#. not saved.");
				}
				return false;
			}
			if (command.unreliableSequenceNumber <= channel.incomingUnreliableSequenceNumber)
			{
				photonPeer.CountDiscarded++;
				if ((int)base.debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "incoming unreliable-seq# < Dispatched-unrel-seq#. not saved.");
				}
				return false;
			}
			if (channel.ContainsUnreliableSequenceNumber(command.unreliableSequenceNumber))
			{
				if ((int)base.debugOut >= 5)
				{
					base.Listener.DebugReturn(DebugLevel.ALL, "command was received before! Old/New: " + channel.incomingUnreliableCommandsList[command.unreliableSequenceNumber]?.ToString() + "/" + command);
				}
				return false;
			}
			channel.incomingUnreliableCommandsList.Add(command.unreliableSequenceNumber, command);
			return true;
		}
		if (command.commandFlags == 2)
		{
			int unsequencedGroupNumber = command.unsequencedGroupNumber;
			int num = command.unsequencedGroupNumber % 128;
			if (unsequencedGroupNumber >= incomingUnsequencedGroupNumber + 128)
			{
				incomingUnsequencedGroupNumber = unsequencedGroupNumber - num;
				for (int i = 0; i < unsequencedWindow.Length; i++)
				{
					unsequencedWindow[i] = 0;
				}
			}
			else if (unsequencedGroupNumber < incomingUnsequencedGroupNumber || (unsequencedWindow[num / 32] & (1 << num % 32)) != 0)
			{
				return false;
			}
			unsequencedWindow[num / 32] |= 1 << num % 32;
			channel.incomingUnsequencedCommandsList.Enqueue(command);
			return true;
		}
		return false;
	}

	internal NCommand RemoveSentReliableCommand(int ackReceivedReliableSequenceNumber, int ackReceivedChannel, bool isUnsequenced)
	{
		NCommand nCommand = null;
		lock (sentReliableCommands)
		{
			foreach (NCommand sentReliableCommand in sentReliableCommands)
			{
				if (sentReliableCommand != null && sentReliableCommand.reliableSequenceNumber == ackReceivedReliableSequenceNumber && sentReliableCommand.commandChannelID == ackReceivedChannel && sentReliableCommand.IsFlaggedUnsequenced == isUnsequenced)
				{
					nCommand = sentReliableCommand;
					break;
				}
			}
			if (nCommand != null)
			{
				sentReliableCommands.Remove(nCommand);
			}
			else if ((int)base.debugOut >= 5 && peerConnectionState != ConnectionStateValue.Connected && peerConnectionState != ConnectionStateValue.Disconnecting)
			{
				EnqueueDebugReturn(DebugLevel.ALL, $"No sent command for ACK (Ch: {ackReceivedReliableSequenceNumber} Sq#: {ackReceivedChannel}). PeerState: {peerConnectionState}.");
			}
		}
		return nCommand;
	}

	internal string CommandListToString(NCommand[] list)
	{
		if ((int)base.debugOut < 5)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < list.Length; i++)
		{
			stringBuilder.Append(i + "=");
			stringBuilder.Append(list[i]);
			stringBuilder.Append(" # ");
		}
		return stringBuilder.ToString();
	}
}
