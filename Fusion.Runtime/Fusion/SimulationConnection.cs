#define DEBUG
using System;
using System.Collections.Generic;
using Fusion.Sockets;

namespace Fusion;

internal class SimulationConnection
{
	public const int INTEGRATOR_HISTORY_MULT = 10;

	private Simulation _simulation;

	private Dictionary<NetworkId, NetworkObjectConnectionData> _objects;

	private Queue<NetworkId> _objectsDestroyed;

	public PlayerRef Player;

	public bool AreaOfInterestHasBeenUpdated = false;

	public HashSet<int> AreaOfInterestCells = new HashSet<int>();

	public ulong MessagesInSequence;

	public ulong MessagesOutSequence;

	public SimulationMessageList MessagesIn;

	public SimulationMessageList MessagesOut;

	internal double LastSend;

	internal int ActiveStructsVersion = 0;

	internal int ActiveStructsIndex = 0;

	internal List<NetworkObjectConnectionData> ActiveStructs = new List<NetworkObjectConnectionData>();

	internal TimeSeries _packetRecvDelta;

	internal Timer _packetRecvDeltaTimer;

	internal SimulationInput.Buffer _inputs;

	internal TimeSeries _clientOffset;

	internal Tick _latestTickReceived;

	internal Tick _latestTickAcknowledged;

	internal NetworkObjectPriorityList ObjectPriorityList;

	internal unsafe NetConnection* Connection;

	internal NetConnectionId ConnectionId;

	internal HashSet<NetworkId> PendingDeleteMainTRSP = new HashSet<NetworkId>(NetworkId.Comparer);

	public unsafe int ConnectionIndex => Connection->LocalId.GroupIndex;

	public int DestroysPending => _objectsDestroyed.Count;

	public static implicit operator PlayerRef(SimulationConnection c)
	{
		Assert.Check(c.Player.IsRealPlayer);
		return c.Player;
	}

	internal SimulationConnection(Simulation simulation)
	{
		_simulation = simulation;
		int clientSend = TickRate.Resolve(simulation.Config.TickRateSelection).ClientSend;
		clientSend = Math.Max(clientSend, 5);
		_inputs = new SimulationInput.Buffer(simulation.ProjectConfig);
		_clientOffset = new TimeSeries(clientSend);
		_latestTickReceived = default(Tick);
		_packetRecvDelta = new TimeSeries(clientSend);
		_packetRecvDeltaTimer = default(Timer);
		_objects = new Dictionary<NetworkId, NetworkObjectConnectionData>(NetworkId.Comparer);
		_objectsDestroyed = new Queue<NetworkId>();
		ObjectPriorityList = new NetworkObjectPriorityList();
		ObjectPriorityList.Player = Player;
	}

	public int GetPriority(NetworkObjectMeta meta)
	{
		return meta.GetPriority(Player);
	}

	public bool TryGetObjectData(NetworkId id, out NetworkObjectConnectionData data)
	{
		return (data = GetObjectData(id, create: false)) != null;
	}

	public NetworkObjectConnectionData GetObjectData(NetworkId id, bool create, bool allowFail = false)
	{
		if (!_objects.TryGetValue(id, out var value) && create)
		{
			if (!_simulation.TryGetMeta(id, out var meta))
			{
				if (allowFail)
				{
					return null;
				}
				throw new InvalidOperationException($"tried to get connection object data for {id} but it does not exist");
			}
			value = new NetworkObjectConnectionData();
			value.Id = id;
			value.Filter = ulong.MaxValue;
			value.MainTRSP = meta.HasMainTRSP;
			if ((bool)meta.Instance && meta.Instance.NetworkedBehaviours != null)
			{
				NetworkBehaviour[] networkedBehaviours = meta.Instance.NetworkedBehaviours;
				for (int i = 0; i < networkedBehaviours.Length; i++)
				{
					if (!networkedBehaviours[i].DefaultReplicated)
					{
						value.Filter &= (ulong)(~(1L << networkedBehaviours[i].ObjectIndex));
					}
				}
			}
			_objects.Add(id, value);
			if (!meta.IsStruct && _simulation.Config.SchedulingEnabled)
			{
				ObjectPriorityList.SetActive(value, meta);
				if (_simulation.IsClient || !_simulation.Config.AreaOfInterestEnabled)
				{
					value.SetPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.InAreaOfInterest, _simulation);
				}
				if ((meta.Flags & NetworkObjectHeaderFlags.GlobalObjectInterest) == NetworkObjectHeaderFlags.GlobalObjectInterest)
				{
					value.SetPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.ForceInterest, _simulation);
					try
					{
						_simulation.Callbacks.ObjectEnterAOI(Player, id);
					}
					catch (Exception error)
					{
						InternalLogStreams.LogException?.Log(error);
					}
				}
			}
		}
		return value;
	}

	public bool DestroyedNextId(out NetworkId id)
	{
		if (_objectsDestroyed.Count > 0)
		{
			id = _objectsDestroyed.Dequeue();
			if (_objects.TryGetValue(id, out var value))
			{
				Assert.Check(value.Status == NetworkObjectConnectionDataStatus.DestroyUnconfirmed);
				value.Status = NetworkObjectConnectionDataStatus.DestroyPending;
			}
			return true;
		}
		id = default(NetworkId);
		return false;
	}

	public void ObjectData_Remove(NetworkId id)
	{
		if (_objects.TryGetValue(id, out var _))
		{
			_objects.Remove(id);
			PendingDeleteMainTRSP.Remove(id);
		}
	}

	public void ObjectData_Destroyed(NetworkId id, bool force = false)
	{
		if (_objects.TryGetValue(id, out var value))
		{
			if (value.Status != NetworkObjectConnectionDataStatus.DestroyUnconfirmed)
			{
				value.Status = NetworkObjectConnectionDataStatus.DestroyUnconfirmed;
				_objectsDestroyed.Enqueue(id);
				if (value.MainTRSP)
				{
					PendingDeleteMainTRSP.Add(id);
				}
			}
		}
		else if (force)
		{
			_objectsDestroyed.Enqueue(id);
		}
	}

	public bool? ObjectData_IsCreateUnconfirmed(NetworkId id)
	{
		NetworkObjectConnectionData objectData = GetObjectData(id, create: false);
		if (objectData == null)
		{
			return null;
		}
		return objectData.Status == NetworkObjectConnectionDataStatus.CreatedUnconfirmed;
	}

	public bool? ObjectData_IsDestroyUnconfirmed(NetworkId id)
	{
		NetworkObjectConnectionData objectData = GetObjectData(id, create: false);
		if (objectData == null)
		{
			return null;
		}
		return objectData.Status == NetworkObjectConnectionDataStatus.DestroyUnconfirmed;
	}

	public void Free(Simulation simulation)
	{
		simulation.FreeMessages(ref MessagesIn);
		simulation.FreeMessages(ref MessagesOut);
		LastSend = 0.0;
	}

	public void PacketReceiveDelta()
	{
		if (!_packetRecvDeltaTimer.IsRunning)
		{
			_packetRecvDeltaTimer = Timer.StartNew();
			Assert.Check(_packetRecvDelta.IsEmpty);
			if (_simulation.HasRuntimeConfig)
			{
				double value = ((!_simulation.IsClient) ? _simulation.RuntimeConfig.TickRate.ClientSendDelta : _simulation.RuntimeConfig.TickRate.ServerSendDelta);
				_packetRecvDelta.Add(value);
			}
		}
		else
		{
			double value = _packetRecvDeltaTimer.ElapsedInSeconds;
			if (!(value < 0.001))
			{
				_packetRecvDelta.Add(value);
				_packetRecvDeltaTimer.Restart();
			}
		}
	}

	public void ResetTimeFeedback()
	{
		_clientOffset.Clear();
		_packetRecvDelta.Clear();
		_packetRecvDeltaTimer.Reset();
	}

	public void InputReceiveDelta(Tick tick, double receive, double expected)
	{
		if (!(tick <= _latestTickReceived))
		{
			_latestTickReceived = tick;
			_clientOffset.Add(expected - receive);
		}
	}

	public void SetActive(NetworkObjectConnectionData data, NetworkObjectMeta meta)
	{
		ObjectPriorityList.SetActive(data, meta);
	}

	public void SetIdle(NetworkObjectConnectionData data)
	{
		ObjectPriorityList.SetIdle(data);
	}

	public void AddAlwaysInterested(NetworkObjectMeta meta)
	{
		if (meta == null || ((bool)meta.Instance && meta.Instance.ObjectInterest == NetworkObject.ObjectInterestModes.Global))
		{
			return;
		}
		Assert.Check((_simulation.Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement);
		NetworkObjectConnectionData objectData = GetObjectData(meta.Id, create: true);
		if ((objectData.UniqueData.Flags & NetworkObjectHeaderPlayerDataFlags.ForceInterest) != NetworkObjectHeaderPlayerDataFlags.ForceInterest)
		{
			if ((objectData.UniqueData.Flags & NetworkObjectHeaderPlayerDataFlags.AllInterestFlags) == 0)
			{
				_simulation.Callbacks.ObjectEnterAOI(Player, meta.Id);
			}
			SetActive(objectData, meta);
			objectData.SetPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.ForceInterest, _simulation);
		}
	}

	public void RemoveAlwaysInterested(NetworkObjectMeta meta)
	{
		if (meta == null)
		{
			return;
		}
		NetworkObject instance = meta.Instance;
		if ((object)instance != null && instance.ObjectInterest == NetworkObject.ObjectInterestModes.Global)
		{
			return;
		}
		Assert.Check((_simulation.Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement);
		NetworkObjectConnectionData objectData = GetObjectData(meta.Id, create: false);
		if (objectData != null && objectData.UniqueData.Flags.HasFlag(NetworkObjectHeaderPlayerDataFlags.ForceInterest))
		{
			if ((objectData.UniqueData.Flags & NetworkObjectHeaderPlayerDataFlags.AllInterestFlags) == NetworkObjectHeaderPlayerDataFlags.ForceInterest)
			{
				_simulation.Callbacks.ObjectExitAOI(Player, meta.Id);
			}
			objectData.ClearPlayerDataFlag(NetworkObjectHeaderPlayerDataFlags.ForceInterest, _simulation);
		}
	}
}
