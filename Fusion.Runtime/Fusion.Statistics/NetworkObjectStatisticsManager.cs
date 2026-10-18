#define DEBUG
using System.Collections.Generic;
using System.Diagnostics;

namespace Fusion.Statistics;

public class NetworkObjectStatisticsManager
{
	private HashSet<NetworkId> _monitoredNetworkObjects;

	private Dictionary<NetworkId, NetworkObjectStatisticsSnapshot> _pendingSnapshots;

	private Dictionary<NetworkId, NetworkObjectStatisticsSnapshot> _completedSnapshots;

	private Stack<NetworkObjectStatisticsSnapshot> _free;

	internal NetworkObjectStatisticsManager()
	{
		_monitoredNetworkObjects = new HashSet<NetworkId>();
		_completedSnapshots = new Dictionary<NetworkId, NetworkObjectStatisticsSnapshot>();
		_pendingSnapshots = new Dictionary<NetworkId, NetworkObjectStatisticsSnapshot>();
		_free = new Stack<NetworkObjectStatisticsSnapshot>();
	}

	private NetworkObjectStatisticsSnapshot GetNewStatisticsObject()
	{
		return (_free.Count > 0) ? _free.Pop() : new NetworkObjectStatisticsSnapshot();
	}

	public void MonitorNetworkObjectStatistics(NetworkId id, bool monitor)
	{
		if (monitor)
		{
			_monitoredNetworkObjects.Add(id);
		}
		else
		{
			_monitoredNetworkObjects.Remove(id);
		}
	}

	public void ClearMonitoredNetworkObjects()
	{
		_monitoredNetworkObjects.Clear();
	}

	private bool IsObjectMonitored(NetworkId id, Dictionary<NetworkId, NetworkObjectStatisticsSnapshot> source, out NetworkObjectStatisticsSnapshot snapshot)
	{
		snapshot = null;
		if (!_monitoredNetworkObjects.Contains(id))
		{
			return false;
		}
		if (!source.TryGetValue(id, out snapshot))
		{
			snapshot = GetNewStatisticsObject();
			snapshot.Reset();
			source.Add(id, snapshot);
		}
		return true;
	}

	public bool GetNetworkObjectStatistics(NetworkId id, out NetworkObjectStatisticsSnapshot objectStatisticsSnapshot)
	{
		if (IsObjectMonitored(id, _completedSnapshots, out objectStatisticsSnapshot))
		{
			return true;
		}
		objectStatisticsSnapshot = null;
		return false;
	}

	[Conditional("DEBUG")]
	internal void AddToNetworkObjectInBandwidth(NetworkId id, float value, bool overrideValue = false)
	{
		if (IsObjectMonitored(id, _pendingSnapshots, out var snapshot))
		{
			snapshot.AddToInBandwidthStat(value, overrideValue);
		}
	}

	[Conditional("DEBUG")]
	internal void AddToNetworkObjectOutBandwidth(NetworkId id, float value, bool overrideValue = false)
	{
		if (IsObjectMonitored(id, _pendingSnapshots, out var snapshot))
		{
			snapshot.AddToOutBandwidthStat(value, overrideValue);
		}
	}

	[Conditional("DEBUG")]
	internal void AddToNetworkObjectInPackets(NetworkId id, int value, bool overrideValue = false)
	{
		if (IsObjectMonitored(id, _pendingSnapshots, out var snapshot))
		{
			snapshot.AddToInPacketsStat(value, overrideValue);
		}
	}

	[Conditional("DEBUG")]
	internal void AddToNetworkObjectOutPackets(NetworkId id, int value, bool overrideValue = false)
	{
		if (IsObjectMonitored(id, _pendingSnapshots, out var snapshot))
		{
			snapshot.AddToOutPacketsStat(value, overrideValue);
		}
	}

	[Conditional("DEBUG")]
	internal void CollectStatistics()
	{
		foreach (NetworkObjectStatisticsSnapshot value in _completedSnapshots.Values)
		{
			value.Reset();
			_free.Push(value);
		}
		_completedSnapshots.Clear();
		foreach (KeyValuePair<NetworkId, NetworkObjectStatisticsSnapshot> pendingSnapshot in _pendingSnapshots)
		{
			_completedSnapshots.Add(pendingSnapshot.Key, pendingSnapshot.Value);
		}
		_pendingSnapshots.Clear();
	}
}
