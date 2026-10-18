#define TRACE
#define DEBUG
using System;
using System.Runtime.CompilerServices;

namespace Fusion.Sockets;

public struct NetPeer
{
	public const int DEFAULT_HEADERS = 144;

	public const int MAX_MTU_BYTES_TOTAL = 1280;

	public const int MAX_MTU_BYTES_PAYLOAD = 1136;

	public const int MAX_MTU_BITS_PAYLOAD = 9088;

	public const int MAX_PACKET_BYTES_PAYLOAD = 44880;

	public const int MAX_PACKET_BYTES_TOTAL = 51200;

	internal const int FRAG_MAX_COUNT = 40;

	internal const byte FRAG_END_BIT = 128;

	private const int STATE_RUNNING = 0;

	private const int STATE_SHUTDOWN = 2;

	private volatile int _state;

	private NetConfig _config;

	private Timer _recvTimer;

	private unsafe byte* _fragmentBuffer;

	internal NetSocket _socket;

	private NetAddress _address;

	private NetBitBufferStack _sendStack;

	private unsafe NetPeerGroup* _groups;

	private unsafe NetPeerGroupMap* _groupsMap;

	private unsafe int* _groupsAssigned;

	private unsafe NetCommandRefused* _refusedCommand;

	private unsafe NetBitBuffer* _recv;

	private unsafe NetBitBufferBlock* _recvBlock;

	private Timer _delayedClock;

	private NetDelayedPacketList _delayedPackets;

	public NetAddress Address => _address;

	public NetConfig Config => _config;

	public int GroupCount => _config.ConnectionGroups;

	public bool IsShutdown => _state == 2;

	public unsafe static NetConfig* GetConfigPointer(NetPeer* p)
	{
		if (p->_state == 2)
		{
			return null;
		}
		return &p->_config;
	}

	public unsafe static NetPeerGroup* GetGroup(NetPeer* p, int index)
	{
		if (p->_state == 2)
		{
			return null;
		}
		Assert.Check((uint)index < (uint)p->_config.ConnectionGroups);
		return p->_groups + index;
	}

	public unsafe static void Update(NetPeer* p, INetSocket socket, Random rng)
	{
		bool flag = false;
		Update(p, socket, &flag, rng);
	}

	public unsafe static void Update(NetPeer* p, INetSocket socket, bool* work, Random rng)
	{
		if (p->_state != 2)
		{
			if (p->_state != 0)
			{
				InternalLogStreams.LogError?.Log("Can't call Update on NetPeer which is running or has been running on a thread");
				return;
			}
			RecvInternal(p, socket, work, rng);
			SendInternal(p, socket, work);
		}
	}

	public unsafe static void Recv(NetPeer* p, INetSocket socket, Random rng)
	{
		bool flag = false;
		Recv(p, socket, &flag, rng);
	}

	public unsafe static void Recv(NetPeer* p, INetSocket socket, bool* work, Random rng)
	{
		if (p->_state != 2)
		{
			if (p->_state != 0)
			{
				InternalLogStreams.LogError?.Log("Can't call Update on NetPeer which is running or has been running on a thread");
			}
			else
			{
				RecvInternal(p, socket, work, rng);
			}
		}
	}

	public unsafe static void RemapAddress(NetPeer* p, NetAddress oldAddress, NetAddress newAddress)
	{
		int num = p->_groupsMap->Remove(oldAddress);
		Assert.Check(num >= 0);
		p->_groupsMap->Insert(newAddress, 0);
	}

	public unsafe static void Send(NetPeer* p, INetSocket socket)
	{
		if (p->_state != 2)
		{
			bool flag = false;
			Send(p, socket, &flag);
		}
	}

	public unsafe static void Send(NetPeer* p, INetSocket socket, bool* work)
	{
		if (p->_state != 2)
		{
			if (p->_state != 0)
			{
				InternalLogStreams.LogError?.Log("Can't call Update on NetPeer which is running or has been running on a thread");
			}
			else
			{
				SendInternal(p, socket, work);
			}
		}
	}

	public unsafe static NetPeer* Initialize(NetConfig config, INetSocket socket)
	{
		NetPeer* ptr = Native.MallocAndClear<NetPeer>();
		Initialize(ptr, config, socket);
		return ptr;
	}

	public unsafe static void Initialize(NetPeer* p, NetConfig config, INetSocket socket)
	{
		config.MaxConnections = Maths.Clamp(config.MaxConnections, 1, 2048);
		socket.Initialize(config);
		p->_config = config;
		p->_state = 0;
		p->_recvTimer = default(Timer);
		p->_fragmentBuffer = (byte*)Native.MallocAndClear(1280);
		p->_refusedCommand = Native.MallocAndClear<NetCommandRefused>();
		p->_delayedClock = Timer.StartNew();
		p->_delayedPackets = default(NetDelayedPacketList);
		p->_sendStack = NetBitBufferStack.Create(2048);
		p->_recvBlock = NetBitBufferBlock.Create(config.PacketSize);
		p->_socket = socket.Create(config);
		p->_groupsMap = NetPeerGroupMap.Allocate(config.MaxConnections);
		p->_groups = Native.MallocAndClearArray<NetPeerGroup>(config.ConnectionGroups);
		p->_groupsAssigned = Native.MallocAndClearArray<int>(config.ConnectionGroups);
		for (short num = 0; num < config.ConnectionGroups; num++)
		{
			NetPeerGroup.Initialize(num, p->_groups + num, p, config);
		}
		p->_address = socket.Bind(p->_socket, p->_config);
		InternalLogStreams.LogTraceNetwork?.Log($"socket bound to {p->_address}");
	}

	public unsafe static void Destroy(NetPeer* p, INetSocket socket, INetPeerGroupCallbacks callbacks)
	{
		if (p->_state == 0)
		{
			p->_state = 2;
			DestroySocket(p, socket, callbacks);
		}
	}

	private unsafe static void DestroySocket(NetPeer* p, INetSocket socket, INetPeerGroupCallbacks callbacks)
	{
		if (p != null && p->_socket.IsCreated)
		{
			NetBitBufferStack.Dispose(ref p->_sendStack);
			p->_delayedPackets.Dispose();
			for (int i = 0; i < p->GroupCount; i++)
			{
				NetPeerGroup.Dispose(p->_groups + i, callbacks);
			}
			NetBitBuffer.ReleaseRef(ref p->_recv);
			NetBitBufferBlock.Dispose(ref p->_recvBlock);
			NetPeerGroupMap.Dispose(ref p->_groupsMap);
			Native.Free(ref p->_groupsAssigned);
			Native.Free(ref p->_refusedCommand);
			Native.Free(ref p->_fragmentBuffer);
			Native.Free(ref p->_groups);
			socket.Destroy(p->_socket);
			p->_socket = default(NetSocket);
			Native.Free(ref p);
		}
	}

	private unsafe static short FindGroupWithLeastAssignedAddresses(NetPeer* p)
	{
		short result = -1;
		int num = p->_config.ConnectionsPerGroup;
		for (short num2 = 0; num2 < p->_config.ConnectionGroups; num2++)
		{
			if (p->_groupsAssigned[num2] < num)
			{
				result = num2;
				num = p->_groupsAssigned[num2];
			}
		}
		return result;
	}

	private unsafe static void RecvInternal(NetPeer* p, INetSocket socket, bool* work, Random rng)
	{
		p->_recvTimer.Restart();
		RecvDelayed(p, socket, work, rng);
		if (RecvExpired(p))
		{
			return;
		}
		int num;
		while (RecvBufferAvailable(p) && (num = socket.Receive(p->_socket, &p->_recv->Address, (byte*)p->_recv->Data, p->_config.PacketSize)) > 0)
		{
			InternalLogStreams.LogTraceNetwork?.Log($"recv {p->_recv->Address} <= {num} bytes");
			*work = true;
			p->_recv->LengthBytes = num;
			if (p->_config.Simulation.LossNotifySequencesLength > 0 && p->_recv->PacketType == NetPacketType.NotifyData)
			{
				Assert.Check(p->_config.Simulation.LossNotifySequences != null);
				ushort sequence = ((NetNotifyHeader*)p->_recv->Data)->Sequence;
				bool flag = false;
				for (int i = 0; i < p->_config.Simulation.LossNotifySequencesLength; i++)
				{
					if (p->_config.Simulation.LossNotifySequences[i] == sequence)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			NetConfigSimulationOscillator lossOscillator = p->_config.Simulation.LossOscillator;
			if (lossOscillator.Min > 0.0 && lossOscillator.Min <= lossOscillator.Max)
			{
				double curveValue = lossOscillator.GetCurveValue(rng, p->_delayedClock.ElapsedInSeconds);
				if (rng.NextDouble() <= curveValue)
				{
					continue;
				}
			}
			NetConfigSimulationOscillator delayOscillator = p->_config.Simulation.DelayOscillator;
			if (delayOscillator.Min > 0.0 && delayOscillator.Min <= delayOscillator.Max)
			{
				NetDelayedPacket* ptr = NetDelayedPacket.Create(p->_recv->LengthBytes);
				Native.MemCpy(ptr->Data, p->_recv->Data, p->_recv->LengthBytes);
				ptr->Address = p->_recv->Address;
				double curveValue2 = delayOscillator.GetCurveValue(rng, p->_delayedClock.ElapsedInSeconds);
				ptr->DeliveryTime = p->_delayedClock.ElapsedInSeconds + curveValue2;
				if (curveValue2 > 0.0)
				{
					p->_delayedPackets.AddLast(ptr);
					continue;
				}
			}
			RecvBufferPushToGroup(p, socket, rng);
			if (RecvExpired(p))
			{
				break;
			}
		}
	}

	private unsafe static void RecvBufferPushToGroup(NetPeer* p, INetSocket socket, Random rng)
	{
		Assert.Check(p->_recv != null);
		Assert.Check(!p->_recv->Address.Equals(default(NetAddress)));
		short num = p->_groupsMap->Find(p->_recv->Address);
		if (num == -1)
		{
			NetCommandHeader data = *(NetCommandHeader*)p->_recv->Data;
			if (data.PacketType != NetPacketType.Command || data.Command != NetCommands.Connect)
			{
				return;
			}
			num = FindGroupWithLeastAssignedAddresses(p);
			if (num == -1)
			{
				*p->_refusedCommand = NetCommandRefused.Create(NetConnectFailedReason.ServerFull);
				socket.Send(p->_socket, &p->_recv->Address, (byte*)p->_refusedCommand, 3);
				return;
			}
			Assert.Check(p->_groupsAssigned[num] >= 0 && p->_groupsAssigned[num] < p->_config.ConnectionsPerGroup);
			if (!p->_groupsMap->Insert(p->_recv->Address, num))
			{
				return;
			}
			p->_groupsAssigned[num]++;
		}
		Assert.Check(num >= 0 && num <= p->_config.ConnectionGroups);
		if (p->_config.Simulation.DuplicateChance > 0.0 && rng.NextDouble() <= p->_config.Simulation.DuplicateChance && p->_recvBlock->TryAcquire(out var ptr))
		{
			ptr->Address = p->_recv->Address;
			ptr->LengthBytes = p->_recv->LengthBytes;
			Native.MemCpy(ptr->Data, p->_recv->Data, p->_recv->LengthBytes);
			NetPeerGroup.PushOnRecvHead(p->_groups + num, ptr);
		}
		NetPeerGroup.PushOnRecvHead(p->_groups + num, p->_recv);
		p->_recv = null;
	}

	private unsafe static void RecvDelayed(NetPeer* p, INetSocket socket, bool* work, Random rng)
	{
		while (p->_delayedPackets.Count > 0 && p->_delayedPackets.Head->DeliveryTime < p->_delayedClock.ElapsedInSeconds && RecvBufferAvailable(p) && !RecvExpired(p))
		{
			*work = true;
			NetDelayedPacket* memory = p->_delayedPackets.RemoveHead();
			Native.MemCpy(p->_recv->Data, memory->Data, memory->DataLength);
			p->_recv->Address = memory->Address;
			p->_recv->LengthBytes = memory->DataLength;
			RecvBufferPushToGroup(p, socket, rng);
			Native.Free(ref memory);
		}
	}

	private unsafe static void SendInternal(NetPeer* p, INetSocket socket, bool* work)
	{
		SendFromStack(p, socket, work);
		Assert.Check(p->_sendStack.Count == 0);
		for (int i = 0; i < p->_config.ConnectionGroups; i++)
		{
			IntPtr intPtr = NetPeerGroup.PopSendHead(p->_groups + i);
			if (!(intPtr == IntPtr.Zero))
			{
				*work = true;
				p->_sendStack.PushFromHead((NetBitBuffer*)(void*)intPtr);
			}
		}
		SendFromStack(p, socket, work);
	}

	private unsafe static void SendFromStack(NetPeer* p, INetSocket socket, bool* work)
	{
		NetBitBuffer* ptr = null;
		while (p->_sendStack.TryPop(&ptr))
		{
			*work = true;
			Assert.Check(!ptr->Address.Equals(default(NetAddress)));
			if (ptr->PacketType == NetPacketType.Command)
			{
				NetCommandHeader* data = (NetCommandHeader*)ptr->Data;
				if (data->Command == NetCommands.Connect)
				{
					short num = p->_groupsMap->Find(ptr->Address);
					if (num == -1)
					{
						if (!p->_groupsMap->Insert(ptr->Address, ptr->Group))
						{
							NetBitBuffer.Release(ptr);
							continue;
						}
						p->_groupsAssigned[ptr->Group]++;
					}
				}
			}
			if (ptr->PacketType != NetPacketType.Unconnected)
			{
				Assert.Check((uint)p->_groupsMap->Find(ptr->Address) < (uint)p->_config.ConnectionGroups);
			}
			if (ptr->Group == -1)
			{
				Assert.Check(ptr->OffsetBits == 0);
				int num2 = p->_groupsMap->Remove(ptr->Address);
				socket.DeleteEncryptionKey(ptr->Address);
				InternalLogStreams.LogTraceNetwork?.Log($"{ptr->Address} unmapped from {num2}");
				Assert.Check((uint)num2 < (uint)p->_config.ConnectionGroups);
				p->_groupsAssigned[num2]--;
				Assert.Check(p->_groupsAssigned[num2] >= 0);
				NetBitBuffer.Release(ptr);
				continue;
			}
			int num3 = Maths.BytesRequiredForBits(ptr->OffsetBits);
			if (ptr->PacketType == NetPacketType.NotifyData && num3 > 1280)
			{
				InternalLogStreams.LogTraceNetwork?.Log($"send {ptr->Address} => {num3} bytes [FRAGMENTED, MTU:{1280}]");
				NetNotifyHeader netNotifyHeader = default(NetNotifyHeader);
				Native.MemCpy(&netNotifyHeader, ptr->Data, 14);
				byte* ptr2 = (byte*)ptr->Data + 14;
				int num4 = num3 - 14;
				byte b = 1;
				InternalLogStreams.LogTraceNetwork?.Log("frag-send-start");
				while (num4 > 0)
				{
					Assert.Check(b >= 1 && b <= 40, "Max amount of fragments reached {0}, remaining data: {1}", 40, num4);
					int num5 = Math.Min(1122, num4);
					num4 -= num5;
					Assert.Check(num4 >= 0);
					netNotifyHeader.Fragment = b;
					if (num4 == 0)
					{
						netNotifyHeader.Fragment |= 128;
					}
					InternalLogStreams.LogTraceNetwork?.Log($"frag-send:{b} seq:{netNotifyHeader.Sequence} size:{num5} last:{(netNotifyHeader.Fragment & 0x80) == 128}");
					Native.MemCpy(p->_fragmentBuffer, &netNotifyHeader, 14);
					Native.MemCpy(p->_fragmentBuffer + 14, ptr2, num5);
					ptr2 += num5;
					b++;
					socket.Send(p->_socket, &ptr->Address, p->_fragmentBuffer, num5 + 14, reliable: true);
				}
				InternalLogStreams.LogTraceNetwork?.Log("frag-send-end");
			}
			else
			{
				socket.Send(p->_socket, &ptr->Address, (byte*)ptr->Data, num3);
			}
			if (ptr->PacketType == NetPacketType.Command)
			{
				NetCommandHeader* data2 = (NetCommandHeader*)ptr->Data;
				if (data2->Command == NetCommands.Refused && p->_groupsMap->Find(ptr->Address) != -1)
				{
					int num6 = p->_groupsMap->Remove(ptr->Address);
					InternalLogStreams.LogTraceNetwork?.Log($"{ptr->Address} unmapped from {num6} because it was refused.");
					Assert.Check((uint)num6 < (uint)p->_config.ConnectionGroups);
					p->_groupsAssigned[num6]--;
					Assert.Check(p->_groupsAssigned[num6] >= 0);
				}
			}
			NetBitBuffer.Release(ptr);
		}
		Assert.Check(p->_sendStack.Count == 0);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static bool RecvBufferAvailable(NetPeer* p)
	{
		if (p->_recv == null)
		{
			p->_recv = p->_recvBlock->TryAcquire();
		}
		if (p->_recv != null)
		{
			p->_recv->Address = default(NetAddress);
		}
		return p->_recv != null;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private unsafe static bool RecvExpired(NetPeer* p)
	{
		return p->_recvTimer.ElapsedInMilliseconds > p->_config.OperationExpireTime;
	}
}
