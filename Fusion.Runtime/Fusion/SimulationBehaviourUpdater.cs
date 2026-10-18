#define ENABLE_PROFILER
#define TRACE
#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Fusion.Statistics;

namespace Fusion;

internal class SimulationBehaviourUpdater
{
	internal class BehaviourList : ILogDumpable
	{
		public Type Type;

		public int ExecutionOrder;

		public SimulationModes Modes;

		public SimulationStages Stages;

		public Topologies Topologies;

		public SimulationBehaviour Head;

		public SimulationBehaviour Tail;

		public int LockCount;

		public List<SimulationBehaviour> PendingRemovals;

		public BehaviourStatisticsManager BehaviourStats = new BehaviourStatisticsManager();

		public void AddAfter(SimulationBehaviour item, SimulationBehaviour after)
		{
			Assert.Check(IsInList(after));
			Assert.Check(!IsInList(item));
			Assert.Check((item.Flags & SimulationBehaviourRuntimeFlags.PendingRemoval) == 0);
			if (BehaviourUtils.IsSame(after, Tail))
			{
				AddLast(item);
			}
			else
			{
				Assert.Check(BehaviourUtils.IsNotNull(after.Next));
				item.Next = after.Next;
				item.Prev = after;
				after.Next.Prev = item;
				after.Next = item;
			}
			Assert.Check(IsInList(after));
			Assert.Check(IsInList(item));
		}

		public void AddFirst(SimulationBehaviour item)
		{
			Assert.Check(!IsInList(item));
			item.Next = Head;
			item.Prev = null;
			if (BehaviourUtils.IsNotNull(Head))
			{
				Head.Prev = item;
				Head = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
		}

		public void AddLast(SimulationBehaviour item)
		{
			Assert.Check(BehaviourUtils.IsNull(item.Prev));
			Assert.Check(BehaviourUtils.IsNull(item.Next));
			Assert.Check(!IsInList(item));
			Assert.Check((item.Flags & SimulationBehaviourRuntimeFlags.PendingRemoval) == 0);
			item.Next = null;
			item.Prev = Tail;
			if (BehaviourUtils.IsNotNull(Tail))
			{
				Tail.Next = item;
				Tail = item;
			}
			else
			{
				Head = item;
				Tail = item;
			}
		}

		public void RemoveAllPending()
		{
			Assert.Check(LockCount == 0);
			if (PendingRemovals == null || PendingRemovals.Count == 0)
			{
				return;
			}
			foreach (SimulationBehaviour pendingRemoval in PendingRemovals)
			{
				Remove(pendingRemoval);
			}
			PendingRemovals.Clear();
		}

		public void PendingRemove(SimulationBehaviour item)
		{
			Assert.Check(IsInList(item));
			Assert.Check(LockCount > 0);
			if (PendingRemovals == null)
			{
				PendingRemovals = new List<SimulationBehaviour>();
			}
			PendingRemovals.Add(item);
		}

		public void Remove(SimulationBehaviour item)
		{
			if (!IsInList(item))
			{
				InternalLogStreams.LogTraceObject?.Warn(item, $"Not in list {BehaviourUtils.GetName(item)}");
				return;
			}
			if (BehaviourUtils.IsNotNull(item.Prev))
			{
				item.Prev.Next = item.Next;
			}
			if (BehaviourUtils.IsNotNull(item.Next))
			{
				item.Next.Prev = item.Prev;
			}
			if (BehaviourUtils.IsSame(item, Tail))
			{
				Tail = item.Prev;
			}
			if (BehaviourUtils.IsSame(item, Head))
			{
				Head = item.Next;
			}
			item.Prev = null;
			item.Next = null;
			item.Flags &= ~SimulationBehaviourRuntimeFlags.PendingRemoval;
		}

		public bool IsInList(SimulationBehaviour item)
		{
			SimulationBehaviour simulationBehaviour = Head;
			while (BehaviourUtils.IsNotNull(simulationBehaviour))
			{
				if (BehaviourUtils.IsSame(simulationBehaviour, item))
				{
					return true;
				}
				simulationBehaviour = simulationBehaviour.Next;
			}
			return false;
		}

		void ILogDumpable.Dump(StringBuilder builder)
		{
			builder.Append("[Type: ").Append(Type.Name).Append(", List: ");
			SimulationBehaviour simulationBehaviour = Head;
			while (!BehaviourUtils.IsNull(simulationBehaviour))
			{
				if (!BehaviourUtils.IsSame(simulationBehaviour, Head))
				{
					builder.Append("->");
				}
				if (!simulationBehaviour.CanReceiveRenderCallback)
				{
					builder.Append("[x]");
				}
				builder.Append(BehaviourUtils.GetName(simulationBehaviour));
				simulationBehaviour = simulationBehaviour.Next;
			}
			builder.Append("]");
		}
	}

	private readonly Dictionary<Type, BehaviourList> _byTypeLookup;

	private readonly Dictionary<Type, (SimulationBehaviour[], Type[])> _byTypeHierarchy;

	private readonly List<BehaviourList> _inOrderList;

	private readonly Dictionary<Type, List<BehaviourList>> _inOrderByInterfaceList;

	private readonly HashSet<Type> _behavioursChecked;

	private readonly NetworkProjectConfig _config;

	private static Type[] CallbackInterfacesDefualts => new Type[19]
	{
		typeof(IAfterRender),
		typeof(IBeforeTick),
		typeof(IAfterTick),
		typeof(IBeforeAllTicks),
		typeof(IAfterAllTicks),
		typeof(IBeforeSimulation),
		typeof(IBeforeHitboxRegistration),
		typeof(IPlayerJoined),
		typeof(IPlayerLeft),
		typeof(IBeforeUpdate),
		typeof(IAfterUpdate),
		typeof(ISceneLoadDone),
		typeof(ISceneLoadStart),
		typeof(IAfterClientPredictionReset),
		typeof(IBeforeClientPredictionReset),
		typeof(IBeforeCopyPreviousState),
		typeof(IBeforeUpdateRemotePrefabs),
		typeof(IAfterUpdateRemotePrefabs),
		typeof(IAfterHostMigration)
	};

	public SimulationBehaviourUpdater(NetworkProjectConfig config)
	{
		_byTypeLookup = new Dictionary<Type, BehaviourList>();
		_byTypeHierarchy = new Dictionary<Type, (SimulationBehaviour[], Type[])>();
		_inOrderList = new List<BehaviourList>();
		_inOrderByInterfaceList = new Dictionary<Type, List<BehaviourList>>();
		_behavioursChecked = new HashSet<Type>();
		_config = config;
	}

	private static List<Type> Scanlibrary()
	{
		List<Type> list = new List<Type>();
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.GetCustomAttribute<NetworkAssemblyIgnoreAttribute>() != null)
			{
				continue;
			}
			try
			{
				Type[] types = assembly.GetTypes();
				Type[] array = types;
				foreach (Type type in array)
				{
					if (typeof(SimulationBehaviour).IsAssignableFrom(type) && !list.Contains(type))
					{
						list.Add(type);
					}
				}
			}
			catch (Exception)
			{
				InternalLogStreams.LogDebug?.Warn("Error while loading types from Assembly: " + assembly.FullName + ". Ignore.");
			}
		}
		return list;
	}

	private static (SimulationModes, SimulationStages, Topologies) GetSimulationFlags(Type type)
	{
		while (typeof(SimulationBehaviour).IsAssignableFrom(type))
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(SimulationBehaviourAttribute), inherit: true);
			if (customAttributes.Length != 0)
			{
				SimulationBehaviourAttribute simulationBehaviourAttribute = (SimulationBehaviourAttribute)customAttributes[0];
				return ((simulationBehaviourAttribute.Modes == (SimulationModes)0) ? (SimulationModes.Server | SimulationModes.Host | SimulationModes.Client) : simulationBehaviourAttribute.Modes, (simulationBehaviourAttribute.Stages == (SimulationStages)0) ? (SimulationStages.Forward | SimulationStages.Resimulate) : simulationBehaviourAttribute.Stages, (simulationBehaviourAttribute.Topologies == (Topologies)0) ? (Topologies.ClientServer | Topologies.Shared) : simulationBehaviourAttribute.Topologies);
			}
			type = type.BaseType;
		}
		return (SimulationModes.Server | SimulationModes.Host | SimulationModes.Client, SimulationStages.Forward | SimulationStages.Resimulate, Topologies.ClientServer | Topologies.Shared);
	}

	private int GetExecutionOrder(Type type)
	{
		while (type != typeof(object))
		{
			int? executionOrder = _config.GetExecutionOrder(type);
			if (executionOrder.HasValue)
			{
				return executionOrder.Value;
			}
			type = type.BaseType;
		}
		return 0;
	}

	public void BuildTypeOrder(Type[] customCallbackInterfaces)
	{
		if (customCallbackInterfaces != null)
		{
			Assert.Always(customCallbackInterfaces.All((Type x) => x.IsInterface), "All types provided as custom callback interfaces must be interfaces.");
		}
		else
		{
			customCallbackInterfaces = new Type[0];
		}
		_inOrderList.Clear();
		_byTypeLookup.Clear();
		foreach (Type item in Scanlibrary())
		{
			AddType(item, GetSimulationFlags(item));
		}
		_inOrderList.Sort((BehaviourList a, BehaviourList b) => a.ExecutionOrder.CompareTo(b.ExecutionOrder));
		foreach (Type item2 in CallbackInterfacesDefualts.Concat(customCallbackInterfaces))
		{
			List<BehaviourList> list = new List<BehaviourList>();
			for (int i = 0; i < _inOrderList.Count; i++)
			{
				BehaviourList behaviourList = _inOrderList[i];
				if (item2.IsAssignableFrom(behaviourList.Type))
				{
					list.Add(behaviourList);
				}
			}
			_inOrderByInterfaceList.Add(item2, list);
		}
	}

	public void InvokeRender()
	{
		int count = _inOrderList.Count;
		for (int i = 0; i < count; i++)
		{
			try
			{
				BehaviourList behaviourList = _inOrderList[i];
				SimulationBehaviour simulationBehaviour = behaviourList.Head;
				int num = 0;
				Timer timer = Timer.StartNew();
				while (BehaviourUtils.IsNotNull(simulationBehaviour))
				{
					if (simulationBehaviour.CanReceiveRenderCallback)
					{
						simulationBehaviour.PreRender();
						simulationBehaviour.Render();
						num++;
					}
					simulationBehaviour = simulationBehaviour.Next;
				}
				timer.Stop();
				behaviourList.BehaviourStats.PendingSnapshot.AccumulateRenderExecutionCount(num);
				behaviourList.BehaviourStats.PendingSnapshot.AccumulateRenderExecutionTime(timer.ElapsedInMilliseconds);
			}
			catch (Exception error)
			{
				InternalLogStreams.LogException?.Log(error);
			}
		}
	}

	public int GetCallbackCount(Type type)
	{
		return _inOrderByInterfaceList[type].Count;
	}

	public SimulationBehaviourListScope GetCallbackHead(Type type, int index, out SimulationBehaviour head)
	{
		BehaviourList behaviourList = _inOrderByInterfaceList[type][index];
		head = behaviourList.Head;
		return new SimulationBehaviourListScope(behaviourList);
	}

	public void GetAllSimulationBehaviours(List<SimulationBehaviour> allSb)
	{
		Type typeFromHandle = typeof(NetworkBehaviour);
		for (int i = 0; i < _inOrderList.Count; i++)
		{
			BehaviourList behaviourList = _inOrderList[i];
			SimulationBehaviour simulationBehaviour = behaviourList.Head;
			while (BehaviourUtils.IsNotNull(simulationBehaviour))
			{
				if (!typeFromHandle.IsInstanceOfType(simulationBehaviour))
				{
					allSb.Add(simulationBehaviour);
				}
				simulationBehaviour = simulationBehaviour.Next;
			}
		}
	}

	public void InvokeFixedUpdateNetwork(SimulationStages stage, SimulationModes mode, Topologies topology)
	{
		EngineProfiler.Begin("SimulationBehaviourUpdater.InvokeFixedUpdateNetwork");
		int count = _inOrderList.Count;
		for (int i = 0; i < count; i++)
		{
			try
			{
				BehaviourList behaviourList = _inOrderList[i];
				Timer timer = Timer.StartNew();
				int num = 0;
				if ((behaviourList.Modes & mode) != mode || (behaviourList.Stages & stage) != stage || (behaviourList.Topologies & topology) != topology)
				{
					continue;
				}
				SimulationBehaviour simulationBehaviour = behaviourList.Head;
				Assert.Check(behaviourList.LockCount == 0);
				behaviourList.LockCount++;
				try
				{
					while (BehaviourUtils.IsNotNull(simulationBehaviour))
					{
						SimulationBehaviour next = simulationBehaviour.Next;
						if ((simulationBehaviour.Flags & SimulationBehaviourRuntimeFlags.SkipNextUpdate) != 0)
						{
							simulationBehaviour.Flags &= ~SimulationBehaviourRuntimeFlags.SkipNextUpdate;
						}
						else if (simulationBehaviour.CanReceiveSimulationCallback)
						{
							simulationBehaviour.FixedUpdateNetwork();
							num++;
						}
						simulationBehaviour = next;
					}
				}
				finally
				{
					if (--behaviourList.LockCount == 0)
					{
						behaviourList.RemoveAllPending();
					}
					timer.Stop();
					behaviourList.BehaviourStats.PendingSnapshot.AccumulateFixedUpdateNetworkExecutionCount(num);
					behaviourList.BehaviourStats.PendingSnapshot.AccumulateFixedUpdateNetworkExecutionTime(timer.ElapsedInMilliseconds);
				}
			}
			catch (Exception error)
			{
				InternalLogStreams.LogException?.Log(error);
			}
		}
		EngineProfiler.End();
	}

	public void AddObject(NetworkRunner runner, NetworkObject obj, bool skipFirstCall, bool isInSimulation)
	{
		Assert.Check(BehaviourUtils.IsSameNotNull(obj.Runner, runner));
		Assert.Check((obj.RuntimeFlags & NetworkObjectRuntimeFlags.InSimulation) == 0);
		if (isInSimulation)
		{
			obj.RuntimeFlags |= NetworkObjectRuntimeFlags.InSimulation;
		}
		for (int i = 0; i < obj.NetworkedBehaviours.Length; i++)
		{
			Assert.Check(BehaviourUtils.IsSame(obj.NetworkedBehaviours[i].Object, obj));
			Assert.Check(BehaviourUtils.IsSame(obj.NetworkedBehaviours[i].Runner, runner));
			AddBehaviour(obj.NetworkedBehaviours[i], skipFirstCall);
			if (!isInSimulation)
			{
				continue;
			}
			obj.NetworkedBehaviours[i].Flags |= SimulationBehaviourRuntimeFlags.InSimulation;
			if (obj.NetworkedBehaviours[i] is ISimulationEnter simulationEnter)
			{
				try
				{
					simulationEnter.SimulationEnter();
				}
				catch (Exception error)
				{
					InternalLogStreams.LogException?.Log(error);
				}
			}
		}
	}

	public void AddBehaviour(SimulationBehaviour behaviour, bool skipFirstCall)
	{
		CheckSimulationBehaviourForNetworkedAttribute(behaviour.GetType());
		if (skipFirstCall)
		{
			behaviour.Flags |= SimulationBehaviourRuntimeFlags.SkipNextUpdate;
		}
		else
		{
			behaviour.Flags &= ~SimulationBehaviourRuntimeFlags.SkipNextUpdate;
		}
		BehaviourList behaviourList = FindList(behaviour.GetType());
		if (behaviourList.IsInList(behaviour))
		{
			InternalLogStreams.LogTraceObject?.Warn(behaviour, $"Not added {BehaviourUtils.GetName(behaviour)}: already on the list {LogUtils.GetDump(behaviourList)}");
			return;
		}
		SimulationBehaviour simulationBehaviour = behaviourList.Head;
		if (BehaviourUtils.IsNotNull(behaviour.Object))
		{
			while (BehaviourUtils.IsNotNull(simulationBehaviour) && BehaviourUtils.IsNull(simulationBehaviour.Object))
			{
				simulationBehaviour = simulationBehaviour.Next;
			}
			while (BehaviourUtils.IsNotNull(simulationBehaviour) && BehaviourUtils.IsNotNull(simulationBehaviour.Next))
			{
				if (simulationBehaviour.Object.Id == behaviour.Object.Id)
				{
					behaviourList.AddAfter(behaviour, simulationBehaviour);
					return;
				}
				if (behaviourList.PendingRemovals != null && behaviourList.PendingRemovals.Contains(simulationBehaviour.Next))
				{
					simulationBehaviour = simulationBehaviour.Next.Next;
					continue;
				}
				if (simulationBehaviour.Object.Id.Raw < behaviour.Object.Id.Raw && behaviour.Object.Id.Raw <= simulationBehaviour.Next.Object.Id.Raw)
				{
					behaviourList.AddAfter(behaviour, simulationBehaviour);
					return;
				}
				simulationBehaviour = simulationBehaviour.Next;
			}
			if (BehaviourUtils.IsNull(simulationBehaviour) || BehaviourUtils.IsNull(simulationBehaviour.Next))
			{
				behaviourList.AddLast(behaviour);
			}
			return;
		}
		string name = behaviour.GetType().Name;
		while (BehaviourUtils.IsNotNull(simulationBehaviour) && BehaviourUtils.IsNotNull(simulationBehaviour.Next) && !BehaviourUtils.IsNotNull(simulationBehaviour.Object))
		{
			if (BehaviourUtils.IsNotNull(simulationBehaviour.Next.Object))
			{
				behaviourList.AddAfter(behaviour, simulationBehaviour);
				return;
			}
			int num = string.CompareOrdinal(simulationBehaviour.GetType().Name, name);
			int num2 = string.CompareOrdinal(name, simulationBehaviour.Next.GetType().Name);
			if (num < 0 && num2 <= 0)
			{
				behaviourList.AddAfter(behaviour, simulationBehaviour);
				return;
			}
			simulationBehaviour = simulationBehaviour.Next;
		}
		if (BehaviourUtils.IsNull(simulationBehaviour) || BehaviourUtils.IsNull(simulationBehaviour.Next))
		{
			behaviourList.AddFirst(behaviour);
		}
	}

	private void CheckSimulationBehaviourForNetworkedAttribute(Type type)
	{
		if (_behavioursChecked.Contains(type))
		{
			return;
		}
		_behavioursChecked.Add(type);
		if (typeof(NetworkBehaviour).IsAssignableFrom(type))
		{
			return;
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (PropertyInfo propertyInfo in properties)
		{
			Attribute customAttribute = propertyInfo.GetCustomAttribute(typeof(NetworkedAttribute));
			if (customAttribute != null)
			{
				InternalLogStreams.LogError?.Log("[Networked] attribute found on property " + propertyInfo.Name + " on " + type.FullName + ". [Networked] properties are only supported on types inheriting from NetworkBehaviour.");
			}
		}
		MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (MethodInfo methodInfo in methods)
		{
			Attribute customAttribute2 = methodInfo.GetCustomAttribute(typeof(RpcAttribute));
			if (customAttribute2 != null)
			{
				InternalLogStreams.LogError?.Log("[Rpc] attribute found on method " + methodInfo.Name + " on " + type.FullName + ". [Rpc] methods are only supported on types inheriting from NetworkBehaviour.");
			}
		}
	}

	public void RemoveBehaviour(SimulationBehaviour behaviour)
	{
		BehaviourList behaviourList = FindList(behaviour.GetType());
		Assert.Check((behaviour.Flags & SimulationBehaviourRuntimeFlags.PendingRemoval) == 0);
		if (behaviourList.LockCount > 0)
		{
			InternalLogStreams.LogTraceObject?.Log(behaviour, $"Pending removal of {BehaviourUtils.GetName(behaviour)} {LogUtils.GetDump(behaviourList)}");
			behaviour.Flags |= SimulationBehaviourRuntimeFlags.PendingRemoval;
			behaviourList.PendingRemove(behaviour);
		}
		else
		{
			InternalLogStreams.LogTraceObject?.Log(behaviour, $"Removing {BehaviourUtils.GetName(behaviour)} {LogUtils.GetDump(behaviourList)}");
			behaviourList.Remove(behaviour);
		}
	}

	public SimulationBehaviour[] GetTypeHeads(Type type)
	{
		if (!_byTypeHierarchy.TryGetValue(type, out var value))
		{
			List<Type> list = new List<Type>();
			for (int i = 0; i < _inOrderList.Count; i++)
			{
				if (type.IsAssignableFrom(_inOrderList[i].Type))
				{
					list.Add(_inOrderList[i].Type);
				}
			}
			Dictionary<Type, (SimulationBehaviour[], Type[])> byTypeHierarchy = _byTypeHierarchy;
			value = (new SimulationBehaviour[list.Count], list.ToArray());
			byTypeHierarchy.Add(type, value);
		}
		var (array, array2) = value;
		Assert.Check(array.Length == array2.Length);
		for (int j = 0; j < array2.Length; j++)
		{
			array[j] = FindList(array2[j]).Head;
		}
		return array;
	}

	private void AddType(Type type, (SimulationModes, SimulationStages, Topologies) attr)
	{
		if (!type.IsAbstract)
		{
			if (typeof(NetworkBehaviour).IsAssignableFrom(type))
			{
				NetworkBehaviourUtils.RegisterRpcInvokeDelegates(type);
				NetworkBehaviourUtils.RegisterMetaData(type);
			}
			else if (typeof(SimulationBehaviour).IsAssignableFrom(type))
			{
				NetworkBehaviourUtils.RegisterRpcInvokeDelegates(type);
			}
		}
		(SimulationModes, SimulationStages, Topologies) tuple = attr;
		SimulationModes item = tuple.Item1;
		SimulationStages item2 = tuple.Item2;
		Topologies item3 = tuple.Item3;
		BehaviourList behaviourList = new BehaviourList();
		behaviourList.Type = type;
		behaviourList.Modes = item;
		behaviourList.Stages = item2;
		behaviourList.Topologies = item3;
		behaviourList.ExecutionOrder = GetExecutionOrder(type);
		_byTypeLookup.Add(type, behaviourList);
		_inOrderList.Add(behaviourList);
	}

	private BehaviourList FindList(Type type)
	{
		if (_byTypeLookup.TryGetValue(type, out var value))
		{
			return value;
		}
		Type key = type;
		while (typeof(SimulationBehaviour).IsAssignableFrom(type))
		{
			if (_byTypeLookup.TryGetValue(type, out value))
			{
				_byTypeLookup.Add(key, value);
				return value;
			}
			type = type.BaseType;
		}
		throw new InvalidOperationException(string.Format("{0} or any of its base-classes found in _byTypeLookup: {1}", type, string.Join(", ", _byTypeLookup.Select((KeyValuePair<Type, BehaviourList> x) => x.Key.ToString()).ToString())));
	}

	[Conditional("DEBUG")]
	public void FinishBehaviourStatisticsPendingSnapshot()
	{
		foreach (BehaviourList inOrder in _inOrderList)
		{
			inOrder.BehaviourStats.FinishPendingSnapshot();
		}
	}

	public bool TryGetBehaviourStatisticsSnapshot(Type behaviourType, out BehaviourStatisticsSnapshot behaviourStatisticsSnapshot)
	{
		if (_byTypeLookup.TryGetValue(behaviourType, out var value))
		{
			behaviourStatisticsSnapshot = value.BehaviourStats.CompletedSnapshot;
			return true;
		}
		behaviourStatisticsSnapshot = null;
		return false;
	}
}
