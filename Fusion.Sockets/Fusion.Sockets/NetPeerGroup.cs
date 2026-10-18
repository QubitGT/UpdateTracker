#define TRACE
#define DEBUG
using System;
using System.Threading;

namespace Fusion.Sockets;

public struct NetPeerGroup
{
	private const double RELIABLE_SEND_INTERVAL = 0.05;

	private unsafe NetPeer* _peer;

	private short _group;

	private Timer _clock;

	private NetConfig _config;

	private uint _counter;

	private IntPtr _sendHead;

	private IntPtr _recvHead;

	private NetBitBufferStack _recvStack;

	private unsafe NetBitBufferBlock* _sendBlock;

	private unsafe NetConnectionMap* _connectionsMap;

	internal double ReliableSendInterval;

	public double Time => _clock.ElapsedInSeconds;

	public int Group => _group;

	public unsafe int ConnectionCount => _connectionsMap->Count;

	internal unsafe static void Dispose(NetPeerGroup* g, INetPeerGroupCallbacks callbacks)
	{
		if (g != null)
		{
			NetConnectionMap.Dispose(ref g->_connectionsMap, callbacks);
			NetBitBufferBlock.Dispose(ref g->_sendBlock);
			NetBitBufferStack.Dispose(ref g->_recvStack);
		}
	}

	public unsafe static NetConnection* GetConnection(NetPeerGroup* g, NetConnectionId id)
	{
		return g->_connectionsMap->Find(id);
	}

	public unsafe static NetConnection* GetConnectionByIndex(NetPeerGroup* g, int index)
	{
		return g->_connectionsMap->FindByIndex(index);
	}

	public unsafe static bool TryGetConnectionByIndex(NetPeerGroup* g, int index, out NetConnection* connection)
	{
		return g->_connectionsMap->TryFindByIndex(index, out connection);
	}

	public unsafe static NetConnectionMap.Iterator ConnectionIterator(NetPeerGroup* g)
	{
		return new NetConnectionMap.Iterator(g->_connectionsMap);
	}

	public unsafe static void Connect(NetPeerGroup* g, NetAddress address, byte[] token, byte[] uniqueId = null)
	{
		NetConnection* ptr = AllocateConnection(g, address, token, uniqueId);
		if (ptr == null)
		{
			InternalLogStreams.LogError?.Log("No free connection slots");
			return;
		}
		ChangeConnectionStatus(g, null, ptr, NetConnectionStatus.Connecting);
		SendCommandConnect(g, null, ptr);
	}

	public unsafe static void Connect(NetPeerGroup* g, string ip, ushort port, byte[] token, byte[] uniqueId = null)
	{
		Connect(g, NetAddress.CreateFromIpPort(ip, port), token, uniqueId);
	}

	public unsafe static void Disconnect(NetPeerGroup* g, NetConnection* c, byte[] token)
	{
		if (g != null && (c->Status == NetConnectionStatus.Connected || c->Status == NetConnectionStatus.Connecting))
		{
			SendCommand(g, c, NetCommandDisconnect.Create(NetDisconnectReason.Requested, token));
			DisconnectInternal(g, c, NetDisconnectReason.Requested);
		}
	}

	internal unsafe static void DisconnectInternal(NetPeerGroup* g, NetConnection* c, NetDisconnectReason reason = NetDisconnectReason.ByRemote, byte[] token = null)
	{
		if (g == null || (c->Status != NetConnectionStatus.Connected && c->Status != NetConnectionStatus.Connecting))
		{
			return;
		}
		c->StateDisconnected = default(NetConnection.StateDisconnectedData);
		c->StateDisconnected.Reason = reason;
		if (token != null)
		{
			int num = (c->DisconnectTokenLength = Math.Min(128, token.Length));
			c->DisconnectToken = Native.MallocAndClearArray<byte>(num);
			for (int i = 0; i < num; i++)
			{
				c->DisconnectToken[i] = token[i];
			}
		}
		else
		{
			c->DisconnectToken = null;
			c->DisconnectTokenLength = 0;
		}
		ChangeConnectionStatus(g, null, c, NetConnectionStatus.Disconnected);
	}

	public unsafe static void Update(NetPeerGroup* g, INetPeerGroupCallbacks cb)
	{
		if (g != null)
		{
			Receive(g, cb);
			Assert.Check(g->_recvStack.Count == 0);
			UpdateConnections(g, cb);
			IntPtr intPtr;
			do
			{
				intPtr = Volatile.Read(ref g->_recvHead);
			}
			while (Interlocked.CompareExchange(ref g->_recvHead, IntPtr.Zero, intPtr) != intPtr);
			if (intPtr != IntPtr.Zero)
			{
				g->_recvStack.PushFromHead((NetBitBuffer*)(void*)intPtr);
				Receive(g, cb);
			}
		}
	}

	internal unsafe static void Initialize(short groupIndex, NetPeerGroup* g, NetPeer* p, NetConfig config)
	{
		*g = default(NetPeerGroup);
		g->_config = config;
		g->_peer = p;
		g->_group = groupIndex;
		g->_clock = Timer.StartNew();
		g->_sendBlock = NetBitBufferBlock.Create(config.PacketSize);
		g->_recvStack = NetBitBufferStack.Create(1024);
		int connectionsPerGroup = g->_config.ConnectionsPerGroup;
		NetConfig* config2 = &g->_config;
		g->_connectionsMap = NetConnectionMap.Allocate(connectionsPerGroup, groupIndex, in config2);
		g->ReliableSendInterval = 0.05;
	}

	internal unsafe static IntPtr PopSendHead(NetPeerGroup* g)
	{
		IntPtr intPtr;
		do
		{
			intPtr = Volatile.Read(ref g->_sendHead);
		}
		while (Interlocked.CompareExchange(ref g->_sendHead, IntPtr.Zero, intPtr) != intPtr);
		return intPtr;
	}

	internal unsafe static void PushOnRecvHead(NetPeerGroup* g, NetBitBuffer* b)
	{
		IntPtr intPtr;
		do
		{
			intPtr = Volatile.Read(ref g->_recvHead);
			b->Next = (NetBitBuffer*)(void*)intPtr;
		}
		while (Interlocked.CompareExchange(ref g->_recvHead, (IntPtr)b, intPtr) != intPtr);
	}

	private unsafe static void UpdateConnections(NetPeerGroup* g, INetPeerGroupCallbacks cb)
	{
		int countUsed = g->_connectionsMap->CountUsed;
		NetConnection* connectionsBuffer = g->_connectionsMap->ConnectionsBuffer;
		for (int i = 0; i < countUsed; i++)
		{
			NetConnection* ptr = connectionsBuffer + i;
			if (ptr->MapState == NetConnectionMap.EntryState.Used)
			{
				switch (ptr->Status)
				{
				case NetConnectionStatus.Connecting:
					UpdateConnecting(g, cb, ptr);
					break;
				case NetConnectionStatus.Connected:
					UpdateConnected(g, cb, ptr);
					break;
				case NetConnectionStatus.Disconnected:
					UpdateDisconnected(g, cb, ptr);
					break;
				case NetConnectionStatus.Shutdown:
					UpdateShutdown(g, cb, ptr);
					break;
				}
			}
		}
	}

	public unsafe static void SendReliable(NetPeerGroup* g, NetConnection* c, ReliableId rid, byte* data, int dataLength)
	{
		Assert.Check(sizeof(ReliableId) == 48);
		Assert.Check(sizeof(ReliableHeader) == 64);
		Assert.Check(c->Status == NetConnectionStatus.Connected);
		Assert.Check(data);
		Assert.Check(dataLength >= 0);
		int num = dataLength;
		while (dataLength > 0)
		{
			int num2 = Math.Min(dataLength, 1088);
			ReliableHeader* ptr = (ReliableHeader*)Native.MallocAndClear(64 + num2);
			rid.Sequence = c->ReliableBuffer.NextSendSequence();
			rid.SliceLength = num2;
			rid.TotalLength = ((rid.TotalLength < num) ? num : rid.TotalLength);
			ptr->Id = rid;
			Native.MemCpy((byte*)ptr + 64, data, num2);
			data += num2;
			dataLength -= num2;
			c->ReliableSendList.AddLast(ptr);
		}
	}

	public unsafe static void ChangeConnectionAddressDuringConnecting(NetPeerGroup* g, NetConnection* c, NetAddress newAddress)
	{
		Assert.Check(c->Status == NetConnectionStatus.Connecting);
		InternalLogStreams.LogTraceNetwork?.Log($"Changing address for connection ({c->LocalId}:{(IntPtr)c}) from {c->Address} to {newAddress} during connecting phase");
		NetAddress address = c->Address;
		g->_connectionsMap->Remap(address, newAddress);
		Assert.Check(c->Address.Equals(newAddress));
		NetPeer.RemapAddress(g->_peer, address, newAddress);
	}

	private unsafe static void SendCommandConnect(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c)
	{
		Assert.Check(c->Status == NetConnectionStatus.Connecting);
		if (c->StateConnecting.Attempts == g->_config.ConnectAttempts)
		{
			Assert.Check(cb != null);
			NetAddress address = c->Address;
			ChangeConnectionStatus(g, cb, c, NetConnectionStatus.Shutdown);
			cb.OnConnectionFailed(address, NetConnectFailedReason.Timeout);
		}
		else
		{
			cb?.OnConnectionAttempt(c, c->StateConnecting.Attempts, g->_config.ConnectAttempts);
			c->StateConnecting.Attempts++;
			c->StateConnecting.AttemptTimeout = g->_clock.ElapsedInSeconds + g->_config.ConnectInterval;
			SendCommand(g, c, NetCommandConnect.Create(c->LocalId, c->ConnectionToken, c->ConnectionTokenLength, c->UniqueId));
			InternalLogStreams.LogDebug?.Log($"Connection Attempt: {*c} [{c->StateConnecting.Attempts}/{g->_config.ConnectAttempts}]");
		}
	}

	private unsafe static void UpdateConnecting(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c)
	{
		if (c->StateConnecting.AttemptTimeout < g->_clock.ElapsedInSeconds)
		{
			SendCommandConnect(g, cb, c);
		}
	}

	private unsafe static void UpdateConnected(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c)
	{
		if (c->RecvTime + g->_config.ConnectionTimeout < g->_clock.ElapsedInSeconds)
		{
			DisconnectInternal(g, c, NetDisconnectReason.Timeout);
			return;
		}
		if (c->ReliableSendTimer.IsRunning && c->ReliableSendTimer.Peek() >= g->ReliableSendInterval && c->ReliableSendList.Count > 0 && GetNotifyDataBuffer(g, c, out var b))
		{
			c->ReliableSendTimer.Consume();
			ReliableHeader* memory = c->ReliableSendList.RemoveHead();
			byte* data = ReliableHeader.GetData(memory);
			b->WriteBytesAligned(&memory->Id, 48);
			Native.MemCpy(b->GetDataPointer(), data, memory->Id.SliceLength);
			b->OffsetBits += memory->Id.SliceLength * 8;
			b->PacketType = NetPacketType.NotifyReliableData;
			if (!SendNotifyDataBuffer(g, c, b, memory))
			{
				Native.Free(ref memory);
				c->ReliableSendList.Dispose();
			}
		}
		if (((c->NotifyRecvUnackedCount > 0 && c->NotifyRecvUnackedCount >= g->_config.Notify.AckForceCount) || c->NotifySendTime + g->_config.Notify.AckForceTimeout < g->_clock.ElapsedInSeconds) && GetConnectionSendBuffer(g, c, out var b2))
		{
			c->NotifyRecvUnackedCount = 0;
			c->NotifySendTime = g->_clock.ElapsedInSeconds;
			*(NetNotifyHeader*)b2->Data = NetNotifyHeader.CreateAcks(c->NotifyRecvSequence, c->NotifyRecvMask);
			b2->OffsetBits = 112;
			Send(g, c, b2);
		}
	}

	private unsafe static void UpdateDisconnected(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c)
	{
		if (c->StateDisconnected.SentDisconnectCommand == 0)
		{
			c->StateDisconnected.SentDisconnectCommand = (SendCommand(g, c, NetCommandDisconnect.Create(c->StateDisconnected.Reason, c->DisconnectToken, c->DisconnectTokenLength)) ? 1 : 0);
		}
		if (c->StateDisconnected.CallbackInvoked == 0)
		{
			c->StateDisconnected.CallbackInvoked = 1;
			cb.OnDisconnected(c, c->StateDisconnected.Reason);
		}
		if (c->StateDisconnected.SentDisconnectCommand == 1 && c->StateDisconnected.CallbackInvoked == 1)
		{
			ChangeConnectionStatus(g, cb, c, NetConnectionStatus.Shutdown);
		}
	}

	private unsafe static void UpdateShutdown(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c)
	{
		if (c->StateShutdown.Unmapped == 1)
		{
			if (c->StateShutdown.Timeout < g->_clock.ElapsedInSeconds)
			{
				ReleaseConnection(g, cb, c);
			}
		}
		else if (c->StateShutdown.Unmapped == 0)
		{
			QueueAddressUnmap(g, c);
		}
	}

	private unsafe static void SendUnconnected(NetPeerGroup* g, NetBitBuffer* b)
	{
		IntPtr sendHead;
		do
		{
			sendHead = g->_sendHead;
			b->Next = (NetBitBuffer*)(void*)sendHead;
		}
		while (Interlocked.CompareExchange(ref g->_sendHead, (IntPtr)b, sendHead) != sendHead);
	}

	private unsafe static void Send(NetPeerGroup* g, NetConnection* c, NetBitBuffer* b)
	{
		Assert.Check(!c->Address.Equals(default(NetAddress)));
		if (b->PacketType == NetPacketType.NotifyData)
		{
			c->NotifyRecvUnackedCount = 0;
		}
		c->SendTime = g->_clock.ElapsedInSeconds;
		IntPtr sendHead;
		do
		{
			sendHead = g->_sendHead;
			b->Next = (NetBitBuffer*)(void*)sendHead;
		}
		while (Interlocked.CompareExchange(ref g->_sendHead, (IntPtr)b, sendHead) != sendHead);
	}

	private unsafe static bool GetConnectionSendBuffer(NetPeerGroup* g, NetConnection* c, out NetBitBuffer* b)
	{
		if (g->_sendBlock->TryAcquire(out b))
		{
			b->Group = g->_group;
			b->Address = c->Address;
			return true;
		}
		return false;
	}

	public unsafe static bool SendUnconnectedData(NetPeerGroup* g, NetAddress address, void* data, int dataLength)
	{
		if (g->_sendBlock->TryAcquire(out var ptr))
		{
			*(sbyte*)ptr->Data = 5;
			ptr->Group = 0;
			ptr->OffsetBits = 8;
			ptr->Address = address;
			ptr->WriteBytesAligned(data, dataLength);
			SendUnconnected(g, ptr);
			return true;
		}
		return false;
	}

	public unsafe static bool GetUnreliableDataBuffer(NetPeerGroup* g, NetConnection* c, out NetBitBuffer* b)
	{
		if (c->Status == NetConnectionStatus.Connected && GetConnectionSendBuffer(g, c, out b))
		{
			*(NetUnreliableHeader*)b->Data = NetUnreliableHeader.Create();
			b->OffsetBits = 8;
			return true;
		}
		b = null;
		return false;
	}

	public unsafe static bool SendUnreliableDataBuffer(NetPeerGroup* g, NetConnection* c, NetBitBuffer* b)
	{
		Assert.Check(b->PacketType == NetPacketType.UnreliableData);
		if (c->Status != NetConnectionStatus.Connected)
		{
			NetBitBuffer.Release(b);
			return false;
		}
		Send(g, c, b);
		return true;
	}

	public unsafe static bool GetNotifyDataBuffer(NetPeerGroup* g, NetConnection* c, out NetBitBuffer* b)
	{
		if (c->Status == NetConnectionStatus.Connected && !c->NotifySendWindow.IsFull && GetConnectionSendBuffer(g, c, out b))
		{
			NetNotifyHeader netNotifyHeader = default(NetNotifyHeader);
			netNotifyHeader.PacketType = NetPacketType.NotifyData;
			*(NetNotifyHeader*)b->Data = netNotifyHeader;
			b->OffsetBits = 112;
			return true;
		}
		b = null;
		return false;
	}

	public unsafe static bool SendNotifyDataBuffer(NetPeerGroup* g, NetConnection* c, NetBitBuffer* b, void* userData)
	{
		Assert.Check(b->PacketType == NetPacketType.NotifyData || b->PacketType == NetPacketType.NotifyReliableData);
		if (c->Status != NetConnectionStatus.Connected)
		{
			NetBitBuffer.Release(b);
			return false;
		}
		if (c->NotifySendWindow.IsFull)
		{
			DisconnectInternal(g, c, NetDisconnectReason.SendWindowFull);
			NetBitBuffer.Release(b);
			return false;
		}
		InternalLogStreams.LogTraceNetwork?.Log($"{LogUtils.GetDump(b)} Send:{Maths.BytesRequiredForBits(b->OffsetBits)}");
		NetNotifyHeader netNotifyHeader = NetNotifyHeader.CreateData(NetConnection.NextNotifySendSequence(c), c->NotifyRecvSequence, c->NotifyRecvMask);
		if (b->PacketType == NetPacketType.NotifyReliableData)
		{
			netNotifyHeader.PacketType = NetPacketType.NotifyReliableData;
		}
		Native.MemCpy(b->Data, &netNotifyHeader, 14);
		NetSendEnvelope envelope = default(NetSendEnvelope);
		envelope.Sequence = netNotifyHeader.Sequence;
		envelope.UserData = userData;
		envelope.SendTime = g->_clock.ElapsedInSeconds;
		envelope.PacketType = b->PacketType;
		c->NotifySendWindow.Push(envelope);
		c->NotifySendTime = envelope.SendTime;
		Send(g, c, b);
		return true;
	}

	private unsafe static void Receive(NetPeerGroup* g, INetPeerGroupCallbacks cb)
	{
		NetBitBuffer* ptr = null;
		while (g->_recvStack.TryPop(&ptr))
		{
			InternalLogStreams.LogTraceNetwork?.Log($"Receive:{ptr->LengthBytes}");
			try
			{
				NetConnection* ptr2 = g->_connectionsMap->Find(ptr->Address);
				if (ptr2 == null)
				{
					HandlePacketUnconnected(g, cb, ptr);
				}
				else
				{
					HandlePacket(g, cb, ptr2, ptr);
				}
			}
			finally
			{
				NetBitBuffer.Release(ptr);
			}
		}
	}

	private unsafe static void HandlePacketUnconnected(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetBitBuffer* b)
	{
		if (b->PacketType == NetPacketType.Command)
		{
			InternalLogStreams.LogDebug?.Log($"Handle Packet Unconnected from {b->Address}");
			NetCommandHeader* data = (NetCommandHeader*)b->Data;
			if (data->Command != NetCommands.Connect)
			{
				return;
			}
			NetCommandConnect data2 = *(NetCommandConnect*)b->Data;
			byte[] tokenDataAsArray = NetCommandConnect.GetTokenDataAsArray(data2);
			byte[] uniqueIdAsArray = NetCommandConnect.GetUniqueIdAsArray(data2);
			switch (cb.OnConnectionRequest(b->Address, tokenDataAsArray, uniqueIdAsArray))
			{
			case OnConnectionRequestReply.Ok:
			{
				NetConnection* ptr = AllocateConnection(g, b->Address, tokenDataAsArray, uniqueIdAsArray);
				if (ptr != null)
				{
					HandlePacketCommand(g, cb, ptr, b);
				}
				break;
			}
			case OnConnectionRequestReply.Refuse:
				if (!SendCommandUnconnected(g, b->Address, NetCommandRefused.Create(NetConnectFailedReason.ServerRefused)))
				{
					InternalLogStreams.LogDebug?.Error("Sending Refused Connection Failed");
				}
				break;
			case OnConnectionRequestReply.Waiting:
				break;
			}
		}
		else if (b->PacketType == NetPacketType.Unconnected)
		{
			cb.OnUnconnectedData(b);
		}
	}

	public unsafe static double GetConnectionIdleTime(NetPeerGroup* g, NetConnection* c)
	{
		return g->Time - c->RecvTime;
	}

	private unsafe static void HandlePacket(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetBitBuffer* b)
	{
		c->RecvTime = g->_clock.ElapsedInSeconds;
		switch (b->PacketType)
		{
		case NetPacketType.Command:
			if (c->Status == NetConnectionStatus.Connecting || c->Status == NetConnectionStatus.Connected)
			{
				HandlePacketCommand(g, cb, c, b);
			}
			break;
		case NetPacketType.NotifyData:
		case NetPacketType.NotifyReliableData:
			if (c->Status == NetConnectionStatus.Connected)
			{
				HandlePacketNotifyData(g, cb, c, b);
			}
			break;
		case NetPacketType.NotifyAcks:
			if (c->Status == NetConnectionStatus.Connected)
			{
				HandlePacketNotifyAcks(g, cb, c, b);
			}
			break;
		case NetPacketType.UnreliableData:
			if (c->Status == NetConnectionStatus.Connected)
			{
				HandlePacketUnreliableData(g, cb, c, b);
			}
			break;
		case NetPacketType.Unconnected:
			break;
		default:
			InternalLogStreams.LogError?.Log($"Invalid Packet Type {b->PacketType}");
			break;
		}
	}

	private unsafe static void HandlePacketNotifyAcks(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetBitBuffer* b)
	{
		if (b->LengthBytes >= 14)
		{
			c->NotifyRecvTime = g->_clock.ElapsedInSeconds;
			HandlePacketAcks(g, cb, c, *(NetNotifyHeader*)b->Data);
		}
	}

	private unsafe static void HandlePacketNotifyData(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetBitBuffer* b)
	{
		if (b->LengthBytes <= 14)
		{
			return;
		}
		NetNotifyHeader header = default(NetNotifyHeader);
		Native.MemCpy(&header, b->Data, 14);
		int num = c->NotifySendSequencer.Distance(header.Sequence, c->NotifyRecvSequence);
		if (num > g->_config.Notify.SequenceBounds || num < -g->_config.Notify.SequenceBounds)
		{
			DisconnectInternal(g, c, NetDisconnectReason.SequenceOutOfBounds);
		}
		else
		{
			if (num < 0)
			{
				return;
			}
			if (header.Fragment != 0)
			{
				if (num == 0)
				{
					return;
				}
				Assert.Check(b->PacketType == NetPacketType.NotifyData);
				int num2 = header.Fragment & -129;
				if (num2 == 1)
				{
					InternalLogStreams.LogTraceNetwork?.Log("frag-recv-start");
					InternalLogStreams.LogTraceNetwork?.Log($"frag-recv:{num2} seq:{header.Sequence} size:{b->LengthBytes}");
					c->NotifyRecvFragment = num2;
					c->NotifyRecvFragmentBufferLength = 0;
					c->NotifyRecvFragmentSequenceDistance = num;
					Native.MemCpy(c->NotifyRecvFragmentBuffer, b->Data, b->LengthBytes);
					c->NotifyRecvFragmentBufferLength = b->LengthBytes;
				}
				else if (num2 > 1 && num == c->NotifyRecvFragmentSequenceDistance && c->NotifyRecvFragment + 1 == num2)
				{
					InternalLogStreams.LogTraceNetwork?.Log($"frag-recv:{num2} seq:{header.Sequence} size:{b->LengthBytes}");
					c->NotifyRecvFragment = num2;
					int num3 = b->LengthBytes - 14;
					if (c->NotifyRecvFragmentBufferLength + num3 > 51200)
					{
						InternalLogStreams.LogError?.Log("Fragment buffer overflow");
						c->NotifyRecvFragment = 0;
						c->NotifyRecvFragmentBufferLength = 0;
						c->NotifyRecvFragmentSequenceDistance = 0;
						return;
					}
					Native.MemCpy(c->NotifyRecvFragmentBuffer + c->NotifyRecvFragmentBufferLength, (byte*)b->Data + 14, num3);
					c->NotifyRecvFragmentBufferLength += num3;
					if ((header.Fragment & 0x80) == 128)
					{
						HandlePacketNotifyData_Part2(header, num, g, cb, c, b);
						InternalLogStreams.LogTraceNetwork?.Log("frag-reassembled");
						NetBitBuffer netBitBuffer = default(NetBitBuffer);
						netBitBuffer.LengthBytes = c->NotifyRecvFragmentBufferLength;
						netBitBuffer.OffsetBits = 112;
						netBitBuffer.Data = (ulong*)c->NotifyRecvFragmentBuffer;
						netBitBuffer.Address = b->Address;
						netBitBuffer.Group = (short)g->Group;
						cb.OnNotifyData(c, &netBitBuffer);
						c->NotifyRecvFragment = 0;
						c->NotifyRecvFragmentBufferLength = 0;
						c->NotifyRecvFragmentSequenceDistance = 0;
					}
				}
				else
				{
					c->NotifyRecvFragment = 0;
					c->NotifyRecvFragmentBufferLength = 0;
					c->NotifyRecvFragmentSequenceDistance = 0;
				}
			}
			else
			{
				if (num <= 0)
				{
					return;
				}
				c->NotifyRecvFragment = 0;
				c->NotifyRecvFragmentBufferLength = 0;
				c->NotifyRecvFragmentSequenceDistance = 0;
				HandlePacketNotifyData_Part2(header, num, g, cb, c, b);
				if (b->PacketType == NetPacketType.NotifyReliableData)
				{
					if (c->ReliableBuffer.Receive(b, out var rid))
					{
						byte* data = b->GetDataPointer();
						cb.OnReliableData(c, rid, data);
						void* root;
						while (c->ReliableBuffer.LateReceive(out root, out rid, out data))
						{
							cb.OnReliableData(c, rid, data);
							c->ReliableBuffer.LateFree(ref root);
						}
					}
				}
				else
				{
					cb.OnNotifyData(c, b);
				}
			}
		}
	}

	private unsafe static void HandlePacketNotifyData_Part2(NetNotifyHeader header, int sequenceDistance, NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetBitBuffer* b)
	{
		c->NotifyRecvSequence = header.Sequence;
		if (sequenceDistance >= g->_config.Notify.AckMaskBits)
		{
			InternalLogStreams.LogTraceNetwork?.Warn("Huge loss. Clear Ack Mask.");
			c->NotifyRecvMask = 1uL;
		}
		else
		{
			c->NotifyRecvMask = (c->NotifyRecvMask << sequenceDistance) | 1;
		}
		c->NotifyRecvTime = g->_clock.ElapsedInSeconds;
		c->NotifyRecvUnackedCount++;
		HandlePacketAcks(g, cb, c, header);
		b->OffsetBits = 112;
	}

	private unsafe static void HandlePacketAcks(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetNotifyHeader h)
	{
		int num = 0;
		while (c->NotifySendWindow.Count > 0)
		{
			NetSendEnvelope envelope = c->NotifySendWindow.Peek();
			int num2 = c->NotifySendSequencer.Distance(envelope.Sequence, h.AckSequence);
			if (num2 > 0)
			{
				break;
			}
			if (num2 == 0)
			{
				c->Rtt = Math.Max(0.0, g->_clock.ElapsedInSeconds - envelope.SendTime);
			}
			num++;
			c->NotifyRecvAckTime = g->_clock.ElapsedInSeconds;
			c->NotifySendWindow.Pop();
			if (num2 <= -g->_config.Notify.AckMaskBits || (h.AckMask & (ulong)(1L << -num2)) == 0)
			{
				if (envelope.PacketType == NetPacketType.NotifyReliableData)
				{
					ReliableHeader* item = envelope.TakeUserData<ReliableHeader>();
					c->ReliableSendList.AddFirst(item);
				}
				else
				{
					cb.OnNotifyLost(c, ref envelope);
				}
			}
			else if (envelope.PacketType == NetPacketType.NotifyReliableData)
			{
				ReliableHeader* memory = envelope.TakeUserData<ReliableHeader>();
				Native.Free(ref memory);
			}
			else
			{
				cb.OnNotifyDelivered(c, ref envelope);
			}
			Assert.Always(envelope.UserData == null, (IntPtr)envelope.UserData);
		}
		if (num == 0)
		{
			c->NotifyRecvAckOutdatedCount++;
		}
	}

	private unsafe static void HandlePacketUnreliableData(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetBitBuffer* b)
	{
		b->OffsetBits = 8;
		cb.OnUnreliableData(c, b);
	}

	private unsafe static void HandlePacketCommand(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetBitBuffer* b)
	{
		NetCommandHeader* data = (NetCommandHeader*)b->Data;
		InternalLogStreams.LogTraceNetwork?.Log($"command {c->Address} <= {data->Command}");
		switch (data->Command)
		{
		case NetCommands.Connect:
			HandleCommandConnect(g, cb, c, *(NetCommandConnect*)b->Data);
			break;
		case NetCommands.Refused:
			HandleCommandRefused(g, cb, c, *(NetCommandRefused*)b->Data);
			break;
		case NetCommands.Accepted:
			HandleCommandAccepted(g, cb, c, *(NetCommandAccepted*)b->Data);
			break;
		case NetCommands.Disconnect:
			HandleCommandDisconnect(g, cb, c, *(NetCommandDisconnect*)b->Data);
			break;
		default:
			InternalLogStreams.LogError?.Log($"Invalid Command Type {data->Command}");
			break;
		}
	}

	private unsafe static void HandleCommandRefused(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetCommandRefused cmd)
	{
		Assert.Check(c->Status == NetConnectionStatus.Connecting);
		try
		{
			cb.OnConnectionFailed(c->Address, cmd.Reason);
		}
		catch (Exception error)
		{
			InternalLogStreams.LogException?.Log(error);
		}
		ChangeConnectionStatus(g, cb, c, NetConnectionStatus.Shutdown);
	}

	private unsafe static void HandleCommandDisconnect(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetCommandDisconnect cmd)
	{
		if (c->Status != NetConnectionStatus.Connected)
		{
			InternalLogStreams.LogError?.Log(string.Format("received {0} with connection status {1}", "NetCommandDisconnect", c->Status));
		}
		else
		{
			DisconnectInternal(g, c, cmd.Reason);
		}
	}

	private unsafe static void HandleCommandConnect(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetCommandConnect cmd)
	{
		switch (c->Status)
		{
		case NetConnectionStatus.Created:
			c->RemoteId = cmd.ConnectionId;
			c->Counter = ++g->_counter;
			ChangeConnectionStatus(g, cb, c, NetConnectionStatus.Connected);
			SendCommand(g, c, NetCommandAccepted.Create(c->LocalId, c->RemoteId, c->Counter));
			cb.OnConnected(c);
			break;
		case NetConnectionStatus.Connected:
			SendCommand(g, c, NetCommandAccepted.Create(c->LocalId, c->RemoteId, c->Counter));
			break;
		default:
			InternalLogStreams.LogError?.Log(string.Format("received {0} with connection status {1}", "NetCommandConnect", c->Status));
			break;
		}
	}

	private unsafe static void HandleCommandAccepted(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetCommandAccepted cmd)
	{
		NetConnectionStatus status = c->Status;
		NetConnectionStatus netConnectionStatus = status;
		if (netConnectionStatus == NetConnectionStatus.Connecting)
		{
			Assert.Check(c->LocalId.Equals(cmd.AcceptedRemoteId));
			c->RemoteId = cmd.AcceptedLocalId;
			c->Counter = cmd.Counter;
			ChangeConnectionStatus(g, cb, c, NetConnectionStatus.Connected);
			cb.OnConnected(c);
		}
		else
		{
			InternalLogStreams.LogTraceNetwork?.Error(string.Format("received {0} with connection status {1}", "NetCommandAccepted", c->Status));
		}
	}

	private unsafe static bool SendCommand<T>(NetPeerGroup* g, NetConnection* c, T cmd) where T : unmanaged
	{
		if (GetConnectionSendBuffer(g, c, out var b))
		{
			*(T*)b->Data = cmd;
			b->OffsetBits = Maths.SizeOfBits<T>();
			Send(g, c, b);
			InternalLogStreams.LogTraceNetwork?.Log($"command {c->Address} => {((NetCommandHeader*)b->Data)->Command}");
			return true;
		}
		return false;
	}

	private unsafe static bool SendCommandUnconnected<T>(NetPeerGroup* g, NetAddress address, T cmd) where T : unmanaged
	{
		if (g->_sendBlock->TryAcquire(out var ptr))
		{
			*(T*)ptr->Data = cmd;
			ptr->Group = g->_group;
			ptr->Address = address;
			ptr->OffsetBits = Maths.SizeOfBits<T>();
			SendUnconnected(g, ptr);
			InternalLogStreams.LogTraceNetwork?.Log($"command {address} => {((NetCommandHeader*)ptr->Data)->Command}");
			return true;
		}
		return false;
	}

	private unsafe static void QueueAddressUnmap(NetPeerGroup* g, NetConnection* c)
	{
		Assert.Check(c->Status == NetConnectionStatus.Shutdown);
		Assert.Check(c->StateShutdown.Unmapped == 0);
		if (c->StateShutdown.Unmapped == 0 && GetConnectionSendBuffer(g, c, out var b))
		{
			InternalLogStreams.LogTraceNetwork?.Log($"Sending Unmap For: {c->Address}");
			b->Group = -1;
			b->OffsetBits = 0;
			Send(g, c, b);
			c->StateShutdown.Unmapped = 1;
		}
	}

	private unsafe static void ChangeConnectionStatus(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c, NetConnectionStatus status)
	{
		if (c->Status == status)
		{
			return;
		}
		InternalLogStreams.LogTraceNetwork?.Log($"{c->Address} status changed from {c->Status} to {status}");
		c->Status = status;
		if (status == NetConnectionStatus.Shutdown)
		{
			c->StateShutdown.Unmapped = 0;
			c->StateShutdown.Timeout = g->_clock.ElapsedInSeconds + g->_config.ConnectionShutdownTime;
			while (c->NotifySendWindow.Count > 0)
			{
				NetSendEnvelope envelope = c->NotifySendWindow.Peek();
				c->NotifySendWindow.Pop();
				cb.OnNotifyDispose(ref envelope);
				Assert.Always(envelope.UserData == null, (IntPtr)envelope.UserData);
			}
			QueueAddressUnmap(g, c);
		}
	}

	private unsafe static void ReleaseConnection(NetPeerGroup* g, INetPeerGroupCallbacks cb, NetConnection* c)
	{
		Assert.Check(g->_connectionsMap->Find(c->Address) == c);
		g->_connectionsMap->Remove(c->Address);
	}

	private unsafe static NetConnection* AllocateConnection(NetPeerGroup* g, NetAddress address, byte[] token, byte[] uniqueId)
	{
		Assert.Check(uniqueId != null, "UniqueId is required");
		NetConnection* ptr = g->_connectionsMap->Insert(address, uniqueId);
		if (ptr == null)
		{
			return null;
		}
		Assert.Check(ptr->RecvTime == 0.0, "c->RecvTime == 0");
		Assert.Check(ptr->SendTime == 0.0, "c->SendTime == 0");
		Assert.Check(ptr->Rtt == 0.0, "c->Rtt == 0");
		Assert.Check(ptr->NotifySendSequencer.Sequence == 0, "c->NotifySendSequencer.Sequence == 0");
		Assert.Check(ptr->NotifySendWindow.Head == 0, "c->NotifySendWindow.Head == 0");
		Assert.Check(ptr->NotifySendWindow.Tail == 0, "c->NotifySendWindow.Tail == 0");
		Assert.Check(ptr->NotifySendWindow.Count == 0, "c->NotifySendWindow.Count == 0");
		Assert.Check(ptr->NotifyRecvTime == 0.0, "c->NotifyRecvTime == 0");
		Assert.Check(ptr->NotifyRecvMask == 0, "c->NotifyRecvMask == 0");
		Assert.Check(ptr->NotifyRecvSequence == 0, "c->NotifyRecvSequence == 0");
		Assert.Check(ptr->NotifyRecvUnackedCount == 0, "c->NotifyRecvUnackedCount == 0");
		ptr->RecvTime = g->_clock.ElapsedInSeconds;
		ptr->SendTime = ptr->RecvTime;
		ptr->Rtt = g->_config.ConnectionDefaultRtt;
		ptr->Status = NetConnectionStatus.Created;
		if (token != null)
		{
			ptr->ConnectionTokenLength = NetCommandConnect.ClampTokenLength(token.Length);
			ptr->ConnectionToken = (byte*)Native.MallocAndClear(token.Length);
			fixed (byte* source = token)
			{
				Native.MemCpy(ptr->ConnectionToken, source, ptr->ConnectionTokenLength);
			}
		}
		return ptr;
	}
}
