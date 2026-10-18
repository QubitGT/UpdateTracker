#define DEBUG
using System;

namespace Fusion.Sockets;

public struct NetConnectionMap
{
	public enum EntryState
	{
		None,
		Free,
		Used
	}

	private struct UniqueIdMapping
	{
		public long UniqueId;

		public short Index;
	}

	public struct Iterator
	{
		private unsafe NetConnectionMap* _map;

		private int _index;

		private int _count;

		public unsafe NetConnection* Current => IsValid ? (_map->Connections + _index) : null;

		public bool IsValid => _index >= 0 && _index < _count;

		public unsafe Iterator(NetConnectionMap* map)
		{
			_map = map;
			_index = -1;
			_count = (int)_map->UsedCount;
		}

		public unsafe bool Next()
		{
			while (++_index < _count)
			{
				if (_map->Connections[_index].MapState == EntryState.Used && _map->Connections[_index].Status < NetConnectionStatus.Disconnected)
				{
					return true;
				}
			}
			return false;
		}
	}

	private unsafe NetConnection** Buckets;

	private unsafe NetConnection* FreeHead;

	internal unsafe NetConnection* Connections;

	private unsafe UniqueIdMapping* UniqueIdHashes;

	private short Group;

	private ulong UsedCount;

	private ulong FreeCount;

	private ulong IdsCount;

	private ulong CapacityAllocated;

	internal ulong CapacityUsable;

	public int Count => (int)(UsedCount - FreeCount);

	public int CountUsed => (int)UsedCount;

	public unsafe NetConnection* ConnectionsBuffer => Connections;

	public bool Full => UsedCount == CapacityAllocated;

	public unsafe static void Dispose(ref NetConnectionMap* map, INetPeerGroupCallbacks callbacks)
	{
		if (map == null)
		{
			return;
		}
		for (int i = 0; i < (int)map->CapacityUsable; i++)
		{
			NetConnection* ptr = map->Connections + i;
			while (ptr->NotifySendWindow.Count > 0)
			{
				NetSendEnvelope envelope = ptr->NotifySendWindow.Peek();
				ptr->NotifySendWindow.Pop();
				callbacks.OnNotifyDispose(ref envelope);
			}
			ptr->NotifySendWindow.Dispose();
			Native.Free(ref ptr->NotifyRecvFragmentBuffer);
			Native.Free(ref ptr->ConnectionToken);
			ptr->ConnectionTokenLength = 0;
			Native.Free(ref ptr->UniqueId);
			ptr->UniqueIdHash = 0L;
			ptr->ReliableSendList.Dispose();
			ptr->ReliableBuffer.Dispose();
		}
		Native.Free(ref map);
	}

	public unsafe static NetConnectionMap* Allocate(int capacity, short groupIndex, in NetConfig* config)
	{
		Assert.Check(capacity >= 0);
		int nextPrime = Primes.GetNextPrime(capacity);
		int num = Native.RoundToMaxAlignment(sizeof(NetConnectionMap));
		int num2 = Native.RoundToMaxAlignment(sizeof(NetConnection*) * nextPrime);
		int num3 = Native.RoundToMaxAlignment(sizeof(NetConnection) * nextPrime);
		int num4 = Native.RoundToMaxAlignment(sizeof(UniqueIdMapping) * nextPrime);
		byte* ptr = (byte*)Native.MallocAndClear(num + num2 + num3 + num4);
		NetConnectionMap* ptr2 = (NetConnectionMap*)ptr;
		ptr2->Buckets = (NetConnection**)(ptr + num);
		ptr2->Connections = (NetConnection*)(ptr + num + num2);
		ptr2->UniqueIdHashes = (UniqueIdMapping*)(ptr + num + num2 + num3);
		ptr2->Group = groupIndex;
		ptr2->UsedCount = 0uL;
		ptr2->FreeCount = 0uL;
		ptr2->IdsCount = 0uL;
		ptr2->CapacityAllocated = (ulong)nextPrime;
		ptr2->CapacityUsable = (ulong)capacity;
		for (int i = 0; i < nextPrime; i++)
		{
			ptr2->UniqueIdHashes[i] = default(UniqueIdMapping);
		}
		for (short num5 = 0; num5 < capacity; num5++)
		{
			NetConnection.Initialize(ptr2->Connections + num5, groupIndex, num5, config);
		}
		return ptr2;
	}

	public unsafe NetConnection* Remap(NetAddress oldAddress, NetAddress newAddress)
	{
		ulong num = NetAddress.Hash64(oldAddress);
		ulong num2 = NetAddress.Hash64(newAddress);
		ulong num3 = num % CapacityAllocated;
		NetConnection* ptr = Buckets[num3];
		NetConnection* ptr2 = default(NetConnection*);
		ulong num4 = num2 % CapacityAllocated;
		while (ptr != null)
		{
			if (ptr->MapHash == num && ptr->Address.Block0 == oldAddress.Block0 && ptr->Address.Block1 == oldAddress.Block1 && ptr->Address.Block2 == oldAddress.Block2)
			{
				Assert.Check(ptr->MapState == EntryState.Used);
				if (ptr2 == null)
				{
					Buckets[num3] = ptr->MapNext;
				}
				else
				{
					ptr2->MapNext = ptr->MapNext;
				}
				ptr->Address = newAddress;
				ptr->MapHash = num2;
				ptr->MapNext = Buckets[num4];
				Buckets[num4] = ptr;
				return ptr;
			}
			ptr2 = ptr;
			ptr = ptr->MapNext;
		}
		Assert.AlwaysFail($"Remap failed from {oldAddress} to {newAddress}");
		return null;
	}

	public unsafe bool Remove(NetAddress address)
	{
		ulong num = NetAddress.Hash64(address);
		ulong num2 = num % CapacityAllocated;
		NetConnection* ptr = Buckets[num2];
		NetConnection* ptr2 = default(NetConnection*);
		while (ptr != null)
		{
			if (ptr->MapHash == num && ptr->Address.Block0 == address.Block0 && ptr->Address.Block1 == address.Block1 && ptr->Address.Block2 == address.Block2)
			{
				if (ptr2 == null)
				{
					Buckets[num2] = ptr->MapNext;
				}
				else
				{
					ptr2->MapNext = ptr->MapNext;
				}
				Assert.Check(ptr->MapState == EntryState.Used);
				RemoveUniqueId(ptr->UniqueIdHash);
				NetConnection.Reset(ptr);
				ptr->MapNext = FreeHead;
				ptr->MapState = EntryState.Free;
				FreeHead = ptr;
				FreeCount++;
				return true;
			}
			ptr2 = ptr;
			ptr = ptr->MapNext;
		}
		return false;
	}

	public unsafe NetConnection* Insert(NetAddress address, byte[] uniqueId)
	{
		Assert.Check(Find(address) == null);
		Assert.Check(!address.Equals(default(NetAddress)));
		long num = BitConverter.ToInt64(uniqueId, 0);
		if (ContainsUniqueId(num, out var groupIndex))
		{
			NetAddress address2 = FindByIndex(groupIndex)->Address;
			NetConnection* ptr = Remap(address2, address);
			StoreUniqueId(num, ptr->LocalId.GroupIndex);
			InternalLogStreams.LogDebug?.Log($"UniqueId ({num}) already used. Update connection address from {address2} to {address}");
			return null;
		}
		ulong num2 = NetAddress.Hash64(address);
		ulong num3 = num2 % CapacityAllocated;
		NetConnection* ptr2;
		if (FreeHead != null)
		{
			Assert.Check(FreeCount != 0);
			ptr2 = FreeHead;
			FreeHead = ptr2->MapNext;
			FreeCount--;
			Assert.Check(ptr2->MapState == EntryState.Free);
		}
		else
		{
			if (UsedCount == CapacityUsable)
			{
				return null;
			}
			ptr2 = Connections + UsedCount++;
			Assert.Check(ptr2->MapState == EntryState.None);
			Assert.Check(ptr2->MapNext == null);
		}
		Assert.Check(ptr2 == Connections + ptr2->LocalId.GroupIndex);
		ptr2->Address = address;
		ptr2->MapHash = num2;
		ptr2->MapState = EntryState.Used;
		ptr2->MapNext = Buckets[num3];
		if (ptr2->UniqueId == null)
		{
			ptr2->UniqueId = (byte*)Native.MallocAndClear(uniqueId.Length);
		}
		ptr2->UniqueIdHash = num;
		fixed (byte* source = uniqueId)
		{
			Native.MemCpy(ptr2->UniqueId, source, 8);
		}
		StoreUniqueId(num, ptr2->LocalId.GroupIndex);
		Buckets[num3] = ptr2;
		return ptr2;
	}

	public unsafe NetConnection* FindByIndex(int index)
	{
		if (index >= 0 && index < (int)CapacityUsable)
		{
			return Connections + index;
		}
		throw new IndexOutOfRangeException();
	}

	public unsafe bool TryFindByIndex(int index, out NetConnection* connection)
	{
		if (index >= 0 && index < (int)CapacityUsable)
		{
			connection = Connections + index;
			return true;
		}
		connection = null;
		return false;
	}

	public unsafe NetConnection* Find(NetConnectionId id)
	{
		Assert.Check(Group == id.Group);
		NetConnection* ptr = Connections + id.GroupIndex;
		if (ptr->LocalId.Raw == id.Raw)
		{
			return ptr;
		}
		return null;
	}

	public unsafe NetConnection* Find(NetAddress address)
	{
		ulong num = NetAddress.Hash64(address);
		ulong num2 = num % CapacityAllocated;
		for (NetConnection* ptr = Buckets[num2]; ptr != null; ptr = ptr->MapNext)
		{
			if (ptr->MapHash == num && ptr->Address.Block0 == address.Block0 && ptr->Address.Block1 == address.Block1 && ptr->Address.Block2 == address.Block2)
			{
				return ptr;
			}
		}
		return null;
	}

	private unsafe bool ContainsUniqueId(long value, out short groupIndex)
	{
		groupIndex = -1;
		ulong num = FindInsertionIndex(value);
		if (num < IdsCount && UniqueIdHashes[num].UniqueId == value)
		{
			groupIndex = UniqueIdHashes[num].Index;
			return true;
		}
		return false;
	}

	private unsafe void StoreUniqueId(long value, short groupIndex)
	{
		ulong num = FindInsertionIndex(value);
		if (UniqueIdHashes[num].UniqueId == value)
		{
			UniqueIdHashes[num].Index = groupIndex;
			return;
		}
		Native.MemMove(UniqueIdHashes + num + 1, UniqueIdHashes + num, (int)(IdsCount - num) * sizeof(UniqueIdMapping));
		UniqueIdHashes[num].UniqueId = value;
		UniqueIdHashes[num].Index = groupIndex;
		Assert.Check(IdsCount + 1 <= CapacityUsable, "Unique Ids count exceeds capacity");
		IdsCount++;
	}

	private unsafe bool RemoveUniqueId(long value)
	{
		ulong num = FindInsertionIndex(value);
		if (num >= IdsCount || UniqueIdHashes[num].UniqueId != value)
		{
			return false;
		}
		Native.MemMove(UniqueIdHashes + num, UniqueIdHashes + num + 1, (int)(IdsCount - num - 1) * sizeof(UniqueIdMapping));
		Assert.Check(IdsCount != 0, "Unique Ids count is already 0");
		IdsCount--;
		return true;
	}

	private unsafe ulong FindInsertionIndex(long value)
	{
		ulong num = 0uL;
		ulong num2 = IdsCount;
		while (num < num2)
		{
			ulong num3 = (num + num2) / 2;
			if (UniqueIdHashes[num3].UniqueId < value)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3;
			}
		}
		return num;
	}
}
