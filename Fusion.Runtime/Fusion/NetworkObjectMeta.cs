#define DEBUG
using System;
using System.Runtime.CompilerServices;

namespace Fusion;

public class NetworkObjectMeta
{
	internal struct List
	{
		public int Count;

		public NetworkObjectMeta Head;

		public NetworkObjectMeta Tail;

		public static NetworkObjectMeta Next(NetworkObjectMeta item)
		{
			return item._next;
		}

		public void AddFirst(NetworkObjectMeta item)
		{
			Assert.Check(!IsInList(item));
			item._next = Head;
			item._prev = null;
			if (Head != null)
			{
				Head._prev = item;
				Head = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
			Count++;
		}

		public void AddLast(NetworkObjectMeta item)
		{
			Assert.Check(!IsInList(item));
			item._next = null;
			item._prev = Tail;
			if (Tail != null)
			{
				Tail._next = item;
				Tail = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
			Count++;
		}

		public void AddBefore(NetworkObjectMeta item, NetworkObjectMeta before)
		{
			Assert.Check(Count > 0);
			Assert.Check(IsInList(before));
			Assert.Check(!IsInList(item));
			if (before == Head)
			{
				AddFirst(item);
			}
			else
			{
				Assert.Check(Count > 1);
				Assert.Check(before._prev != null);
				item._next = before;
				item._prev = before._prev;
				before._prev._next = item;
				before._prev = item;
				Count++;
			}
			Assert.Check(IsInList(before));
			Assert.Check(IsInList(item));
		}

		public void AddAfter(NetworkObjectMeta item, NetworkObjectMeta after)
		{
			Assert.Check(Count > 0);
			Assert.Check(IsInList(after));
			Assert.Check(!IsInList(item));
			if (after == Tail)
			{
				AddLast(item);
			}
			else
			{
				Assert.Check(Count > 1);
				Assert.Check(after._next != null);
				item._next = after._next;
				item._prev = after;
				after._next._prev = item;
				after._next = item;
				Count++;
			}
			Assert.Check(IsInList(after));
			Assert.Check(IsInList(item));
		}

		public NetworkObjectMeta RemoveHead()
		{
			Assert.Check(Count > 0);
			Assert.Check(Head != null);
			Assert.Check(IsInList(Head));
			NetworkObjectMeta head = Head;
			Remove(head);
			return head;
		}

		public void Remove(NetworkObjectMeta item)
		{
			if (IsInList(item))
			{
				if (item._prev != null)
				{
					item._prev._next = item._next;
				}
				if (item._next != null)
				{
					item._next._prev = item._prev;
				}
				if (item == Tail)
				{
					Tail = item._prev;
				}
				if (item == Head)
				{
					Head = item._next;
				}
				item._prev = null;
				item._next = null;
				Count--;
			}
		}

		private bool IsInList(NetworkObjectMeta item)
		{
			for (NetworkObjectMeta networkObjectMeta = Head; networkObjectMeta != null; networkObjectMeta = networkObjectMeta._next)
			{
				if (networkObjectMeta == item)
				{
					return true;
				}
			}
			return false;
		}

		public List RemoveAll()
		{
			List result = this;
			Head = null;
			Tail = null;
			Count = 0;
			return result;
		}

		public void Concat(List other)
		{
			if (other.Count != 0)
			{
				if (Count == 0)
				{
					Count = other.Count;
					Head = other.Head;
					Tail = other.Tail;
					return;
				}
				Assert.Check(!IsInList(other.Head));
				Assert.Check(Tail != null);
				Assert.Check(Tail._next == null);
				Assert.Check(other.Head != null);
				Assert.Check(other.Head._prev == null);
				Tail._next = other.Head;
				other.Head._prev = Tail;
				Tail = other.Tail;
				Count += other.Count;
			}
		}
	}

	internal struct ListMigration
	{
		public int Count;

		public NetworkObjectMeta Head;

		public NetworkObjectMeta Tail;

		public static NetworkObjectMeta Next(NetworkObjectMeta item)
		{
			return item._nextMigration;
		}

		public void AddFirst(NetworkObjectMeta item)
		{
			Assert.Check(!IsInList(item));
			item._nextMigration = Head;
			item._prevMigration = null;
			if (Head != null)
			{
				Head._prevMigration = item;
				Head = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
			Count++;
		}

		public void AddLast(NetworkObjectMeta item)
		{
			Assert.Check(!IsInList(item));
			item._nextMigration = null;
			item._prevMigration = Tail;
			if (Tail != null)
			{
				Tail._nextMigration = item;
				Tail = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
			Count++;
		}

		public void AddBefore(NetworkObjectMeta item, NetworkObjectMeta before)
		{
			Assert.Check(Count > 0);
			Assert.Check(IsInList(before));
			Assert.Check(!IsInList(item));
			if (before == Head)
			{
				AddFirst(item);
			}
			else
			{
				Assert.Check(Count > 1);
				Assert.Check(before._prevMigration != null);
				item._nextMigration = before;
				item._prevMigration = before._prevMigration;
				before._prevMigration._nextMigration = item;
				before._prevMigration = item;
				Count++;
			}
			Assert.Check(IsInList(before));
			Assert.Check(IsInList(item));
		}

		public void AddAfter(NetworkObjectMeta item, NetworkObjectMeta after)
		{
			Assert.Check(Count > 0);
			Assert.Check(IsInList(after));
			Assert.Check(!IsInList(item));
			if (after == Tail)
			{
				AddLast(item);
			}
			else
			{
				Assert.Check(Count > 1);
				Assert.Check(after._nextMigration != null);
				item._nextMigration = after._nextMigration;
				item._prevMigration = after;
				after._nextMigration._prevMigration = item;
				after._nextMigration = item;
				Count++;
			}
			Assert.Check(IsInList(after));
			Assert.Check(IsInList(item));
		}

		public NetworkObjectMeta RemoveHead()
		{
			Assert.Check(Count > 0);
			Assert.Check(Head != null);
			Assert.Check(IsInList(Head));
			NetworkObjectMeta head = Head;
			Remove(head);
			return head;
		}

		public void Remove(NetworkObjectMeta item)
		{
			Assert.Check(IsInList(item));
			if (item._prevMigration != null)
			{
				item._prevMigration._nextMigration = item._nextMigration;
			}
			if (item._nextMigration != null)
			{
				item._nextMigration._prevMigration = item._prevMigration;
			}
			if (item == Tail)
			{
				Tail = item._prevMigration;
			}
			if (item == Head)
			{
				Head = item._nextMigration;
			}
			item._prevMigration = null;
			item._nextMigration = null;
			Count--;
		}

		private bool IsInList(NetworkObjectMeta item)
		{
			for (NetworkObjectMeta networkObjectMeta = Head; networkObjectMeta != null; networkObjectMeta = networkObjectMeta._nextMigration)
			{
				if (networkObjectMeta == item)
				{
					return true;
				}
			}
			return false;
		}
	}

	private static NetworkBufferSerializerInfo[] _serializersStatic;

	private static NetworkBufferSerializerInfo[] _serializersNone;

	private Allocator _allocator;

	private unsafe int* _changes;

	private Simulation _simulation;

	private NetworkObjectHeaderSnapshot _shadow;

	private NetworkObjectHeaderSnapshot _render;

	private NetworkObjectHeaderSnapshot _previous;

	private NetworkObjectHeaderSnapshot _migration;

	private NetworkObjectHeaderSnapshotList _snapshots;

	private NetworkObjectHeaderSnapshot[] _snapshotsByIndex;

	private Tick? _snapshotsByIndexLatest;

	private Timeline _timeline;

	private NetworkObjectMeta _prev;

	private NetworkObjectMeta _next;

	private NetworkObjectMeta _prevMigration;

	private NetworkObjectMeta _nextMigration;

	internal Tick ScannedTick;

	internal Tick ChangedTick;

	internal int AreaOfInterestCell;

	internal NetworkObjectMetaFlags LocalFlags;

	internal NetworkObjectHeader.PlayerUniqueData PlayerData;

	private unsafe int* _ptr;

	internal NetworkObject Instance;

	internal short WordCount;

	internal short BehaviourCount;

	private NetworkObjectHeaderFlags _flags;

	internal const int PRIORITY_IDLE = -32768;

	internal const int PRIORITY_LEVEL_PLAYER = 0;

	internal const int PRIORITY_LEVEL_HIGH = 1;

	internal const int PRIORITY_LEVEL_MED = 2;

	internal const int PRIORITY_LEVEL_LOW = 3;

	internal const int PRIORITY_LEVEL_LOWEST = 4;

	internal const int PRIORITY_LEVEL_COUNT = 5;

	internal NetworkBufferSerializerInfo[] Serializers
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return GetSerializers(HasMainTRSP);
		}
	}

	internal Timeline Timeline
	{
		get
		{
			Assert.Check(_simulation.HasRuntimeConfig);
			if (_timeline == null)
			{
				_timeline = new Timeline(_simulation.TickRate);
			}
			return _timeline;
		}
	}

	internal unsafe ref NetworkObjectHeader Header
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref *(NetworkObjectHeader*)_ptr;
		}
	}

	public NetworkObjectHeaderFlags Flags
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _flags;
		}
	}

	internal bool HasMainTRSP
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Flags.CheckFlag(NetworkObjectHeaderFlags.HasMainNetworkTRSP);
		}
	}

	internal unsafe ref NetworkTRSPData MainTRSPData
	{
		get
		{
			Assert.Check(HasMainTRSP);
			return ref *(NetworkTRSPData*)(_ptr + 20);
		}
	}

	internal unsafe Span<int> Raw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Span<int>(_ptr, WordCount);
		}
	}

	internal Span<int> Data
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			Span<int> raw = Raw;
			return raw.Slice(20, raw.Length - 20);
		}
	}

	internal Span<int> BehaviourChangedTickArray
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			Span<int> raw = Raw;
			int num = WordCount - BehaviourCount;
			return raw.Slice(num, raw.Length - num);
		}
	}

	internal bool HasSnapshots
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _snapshots.Latest != null;
		}
	}

	internal NetworkObjectHeaderSnapshotRef SnapshotLatest
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new NetworkObjectHeaderSnapshotRef(_snapshots.Latest);
		}
	}

	internal bool IsStruct
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & NetworkObjectHeaderFlags.Struct) == NetworkObjectHeaderFlags.Struct;
		}
	}

	internal bool IsObject
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & NetworkObjectHeaderFlags.Struct) == 0;
		}
	}

	public NetworkObjectTypeId Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Header.Type;
		}
	}

	public NetworkId Id
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Header.Id;
		}
	}

	public NetworkId NestingRoot
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Header.NestingRoot;
		}
	}

	public NetworkObjectNestingKey NestingKey
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Header.NestingKey;
		}
	}

	internal ref PlayerRef StateAuthority
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref Header.StateAuthority;
		}
	}

	public ref PlayerRef InputAuthority
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref Header.InputAuthority;
		}
	}

	internal NetworkObjectHeaderSnapshotRef Shadow
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _shadow ?? (_shadow = GetFirstShadowSnapshot());
		}
	}

	internal NetworkObjectHeaderSnapshotRef Render
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _render ?? (_render = GetSnapshot(copyState: false));
		}
	}

	internal NetworkObjectHeaderSnapshotRef Previous
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _previous ?? (_previous = GetSnapshot(copyState: true));
		}
	}

	internal NetworkObjectHeaderSnapshotRef Migration
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _migration ?? (_migration = GetSnapshot(copyState: false));
		}
	}

	internal unsafe Span<int> ChangesSpan
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new Span<int>(Changes, WordCount);
		}
	}

	internal unsafe int* Changes
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if (_changes == null)
			{
				_changes = Allocator.AllocAndClearArray<int>(_allocator, WordCount);
			}
			return _changes;
		}
	}

	static NetworkObjectMeta()
	{
		_serializersNone = Array.Empty<NetworkBufferSerializerInfo>();
		_serializersStatic = new NetworkBufferSerializerInfo[29];
		_serializersStatic[22].Serializer = NetworkTransformSerializer.Instance;
		_serializersStatic[22].Offset = 0;
		_serializersStatic[23].Serializer = NetworkTransformSerializer.Instance;
		_serializersStatic[23].Offset = 1;
		_serializersStatic[24].Serializer = NetworkTransformSerializer.Instance;
		_serializersStatic[24].Offset = 2;
		_serializersStatic[25].Serializer = NetworkTransformSerializer.Instance;
		_serializersStatic[25].Offset = 3;
		_serializersStatic[26].Serializer = NetworkTransformSerializer.Instance;
		_serializersStatic[26].Offset = 4;
		_serializersStatic[27].Serializer = NetworkTransformSerializer.Instance;
		_serializersStatic[27].Offset = 5;
		_serializersStatic[28].Serializer = NetworkTransformSerializer.Instance;
		_serializersStatic[28].Offset = 6;
	}

	internal static NetworkBufferSerializerInfo[] GetSerializers(bool main)
	{
		return main ? _serializersStatic : _serializersNone;
	}

	internal Span<int> GetBehaviourChangedTickArray(NetworkObjectHeaderSnapshotRef snapshot)
	{
		Span<int> raw = snapshot.Raw;
		int num = WordCount - BehaviourCount;
		return raw.Slice(num, raw.Length - num);
	}

	internal Tick GetMaxBehaviourChangedTick()
	{
		Tick tick = 0;
		Span<int> behaviourChangedTickArray = BehaviourChangedTickArray;
		for (int i = 0; i < behaviourChangedTickArray.Length; i++)
		{
			Tick tick2 = behaviourChangedTickArray[i];
			tick = Math.Max(tick, tick2);
		}
		return tick;
	}

	internal Tick GetMaxBehaviourChangedTick(NetworkObjectHeaderSnapshotRef snapshot)
	{
		Tick tick = 0;
		Span<int> behaviourChangedTickArray = GetBehaviourChangedTickArray(snapshot);
		for (int i = 0; i < behaviourChangedTickArray.Length; i++)
		{
			Tick tick2 = behaviourChangedTickArray[i];
			tick = Math.Max(tick, tick2);
		}
		return tick;
	}

	internal unsafe T* GetDataAs<T>() where T : unmanaged
	{
		return (T*)(_ptr + 20);
	}

	private NetworkObjectHeaderSnapshot GetFirstShadowSnapshot()
	{
		NetworkObjectHeaderSnapshot snapshot = GetSnapshot(copyState: false);
		new NetworkObjectHeaderSnapshotRef(snapshot).Header.StateAuthority = PlayerRef.Invalid;
		return snapshot;
	}

	internal NetworkObjectMeta(Simulation simulation, Allocator allocator)
	{
		_allocator = allocator;
		_simulation = simulation;
	}

	private NetworkObjectHeaderSnapshot GetSnapshot(bool copyState)
	{
		NetworkObjectHeaderSnapshot snapshot = _simulation.GetSnapshot();
		snapshot.Init(this, copyState);
		return snapshot;
	}

	internal unsafe void Release(Allocator objectAllocator)
	{
		Header = default(NetworkObjectHeader);
		Allocator.Free(objectAllocator, ref _ptr);
		if ((bool)Instance)
		{
			Instance.Meta = null;
		}
		_prev = null;
		_next = null;
		Instance = null;
		PlayerData = default(NetworkObjectHeader.PlayerUniqueData);
		AreaOfInterestCell = 0;
		LocalFlags = NetworkObjectMetaFlags.None;
		ScannedTick = default(Tick);
		ChangedTick = default(Tick);
		_simulation.SnapshotRelease(ref _shadow);
		_simulation.SnapshotRelease(ref _render);
		_simulation.SnapshotRelease(ref _previous);
		_simulation.SnapshotRelease(ref _migration);
		Allocator.Free(_allocator, ref _changes);
		if (_snapshotsByIndex != null)
		{
			Array.Clear(_snapshotsByIndex, 0, _snapshotsByIndex.Length);
		}
		while (_snapshots.Count > 0)
		{
			_simulation.SnapshotRelease(_snapshots.RemoveLatest());
		}
		_snapshots = default(NetworkObjectHeaderSnapshotList);
		_timeline?.Clear();
	}

	internal NetworkObjectHeaderSnapshotRef NextSnapshot(Tick tick)
	{
		if (_snapshots.Count == 0)
		{
			_snapshots.AddFirst(GetSnapshot(copyState: true));
		}
		else if (_simulation.HasRuntimeConfig && _snapshots.Count >= _simulation.TickRate)
		{
			NetworkObjectHeaderSnapshot networkObjectHeaderSnapshot = _snapshots.RemoveOldest();
			networkObjectHeaderSnapshot.CopyFrom(_snapshots.Latest);
			_snapshots.AddFirst(networkObjectHeaderSnapshot);
		}
		else
		{
			_snapshots.AddFirst(_snapshots.Latest.Clone(_simulation));
		}
		_snapshots.Latest.Tick = tick;
		if (_snapshotsByIndex == null)
		{
			_snapshotsByIndex = new NetworkObjectHeaderSnapshot[64];
		}
		Array.Clear(_snapshotsByIndex, 0, _snapshotsByIndex.Length);
		_snapshotsByIndexLatest = _snapshots.Latest.Tick;
		NetworkObjectHeaderSnapshot networkObjectHeaderSnapshot2 = _snapshots.Latest;
		while (networkObjectHeaderSnapshot2 != null)
		{
			int num = (int)_snapshotsByIndexLatest.Value - (int)networkObjectHeaderSnapshot2.Tick;
			if (num >= _snapshotsByIndex.Length)
			{
				break;
			}
			if (num < 0)
			{
				NetworkObjectHeaderSnapshot networkObjectHeaderSnapshot3 = networkObjectHeaderSnapshot2;
				networkObjectHeaderSnapshot2 = networkObjectHeaderSnapshot2.Next;
				_snapshots.Remove(networkObjectHeaderSnapshot3);
				networkObjectHeaderSnapshot3.Release();
			}
			else
			{
				_snapshotsByIndex[num] = networkObjectHeaderSnapshot2;
				networkObjectHeaderSnapshot2 = networkObjectHeaderSnapshot2.Next;
			}
		}
		return new NetworkObjectHeaderSnapshotRef(_snapshots.Latest);
	}

	internal void AddLatestSnapshotToTimeline()
	{
		if (_simulation.IsClient && _simulation.HasRuntimeConfig && HasSnapshots)
		{
			Tick tick = SnapshotLatest.Tick;
			Tick maxBehaviourChangedTick = GetMaxBehaviourChangedTick(SnapshotLatest);
			Timeline.AddPoint(new TimelinePoint(tick, maxBehaviourChangedTick, _simulation.TickDeltaDouble), _simulation.TickDeltaDouble);
		}
	}

	internal NetworkObjectHeaderSnapshot FindSnapshot(Tick tick)
	{
		if (_snapshotsByIndexLatest.HasValue)
		{
			Assert.Check(_snapshotsByIndex);
			int num = (int)_snapshotsByIndexLatest.Value - (int)tick;
			if ((uint)num < (uint)_snapshotsByIndex.Length)
			{
				return _snapshotsByIndex[num];
			}
		}
		for (NetworkObjectHeaderSnapshot networkObjectHeaderSnapshot = _snapshots.Latest; networkObjectHeaderSnapshot != null; networkObjectHeaderSnapshot = networkObjectHeaderSnapshot.Next)
		{
			if (networkObjectHeaderSnapshot.Tick == tick)
			{
				return networkObjectHeaderSnapshot;
			}
		}
		return null;
	}

	internal bool TryFindSnapshot(Tick tick, out NetworkObjectHeaderSnapshot snapshot)
	{
		return (snapshot = FindSnapshot(tick)) != null;
	}

	internal unsafe void Init(int* words, short wordCount, short behaviourCount, NetworkObjectHeaderFlags flags)
	{
		Assert.Check(_ptr == null);
		Assert.Check(BehaviourUtils.IsNull(Instance));
		_ptr = words;
		WordCount = wordCount;
		BehaviourCount = behaviourCount;
		_flags = flags;
		Assert.Check(Header.WordCount == wordCount);
		Assert.Check(Header.BehaviourCount == behaviourCount);
		Assert.Check(Header.Flags == flags);
	}

	internal unsafe int* GetBehaviourPtr(NetworkBehaviour behaviour)
	{
		return _ptr + behaviour.WordOffset;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int EncodePriorityLevel(int level)
	{
		return Maths.Clamp(level, 0, 4) + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int DecodePriorityLevel(int level)
	{
		return level - 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsIdle(int level)
	{
		return level < -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static bool IsActive(int level)
	{
		return level >= 1 && level <= 5;
	}

	internal int GetPriority(PlayerRef player)
	{
		if (_simulation.Topology == Topologies.ClientServer)
		{
			if (Header.InputAuthority == player && player.IsRealPlayer)
			{
				return EncodePriorityLevel(0);
			}
			if (BehaviourUtils.IsAlive(Instance) && Instance.PriorityCallback != null)
			{
				try
				{
					return EncodePriorityLevel((int)(Instance.PriorityCallback(Instance, player) - 1));
				}
				catch (Exception error)
				{
					InternalLogStreams.LogException?.Log(error);
				}
			}
		}
		return EncodePriorityLevel(4);
	}

	internal unsafe void LinkInstance(NetworkObject instance)
	{
		Assert.Check(BehaviourUtils.IsNotNull(instance));
		Assert.Check(BehaviourUtils.IsNull(Instance));
		Assert.Check(instance.Ptr == null);
		Assert.Check(_ptr != null);
		instance.Ptr = _ptr;
		instance.Meta = this;
		Instance = instance;
	}

	internal void UnlinkInstance(NetworkObject instance)
	{
		Assert.Check((object)Instance == instance);
		Assert.Check(this == instance.Meta);
		Instance = null;
		instance.Meta = null;
	}
}
