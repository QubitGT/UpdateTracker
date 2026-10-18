#define TRACE
#define DEBUG
using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace Fusion;

[AddComponentMenu("Fusion/Network Object")]
[DisallowMultipleComponent]
[HelpURL("https://doc.photonengine.com/fusion/current/manual/network-object")]
[ScriptHelp(Url = "https://doc.photonengine.com/fusion/current/manual/network-object", BackColor = ScriptHeaderBackColor.Orange)]
public class NetworkObject : Behaviour
{
	public delegate bool ReplicateToDelegate(NetworkObject networkObject, PlayerRef player);

	public delegate PriorityLevel PriorityLevelDelegate(NetworkObject networkObject, PlayerRef player);

	internal enum ObjectInterestModes
	{
		AreaOfInterest,
		Global,
		Explicit
	}

	[NonSerialized]
	internal unsafe int* Ptr;

	[NonSerialized]
	public bool IsResume;

	private NetworkRunner _runner;

	internal NetworkObjectMeta Meta;

	[HideInInspector]
	[SerializeField]
	public uint SortKey;

	[NonSerialized]
	[Obsolete("not used anymore, use interest management instead")]
	public ReplicateToDelegate ReplicateTo;

	[NonSerialized]
	public PriorityLevelDelegate PriorityCallback;

	[InlineHelp]
	[SerializeField]
	[FormerlySerializedAs("AoiMode")]
	internal ObjectInterestModes ObjectInterest = ObjectInterestModes.Global;

	[InlineHelp]
	public NetworkObjectFlags Flags = NetworkObjectFlags.DestroyWhenStateAuthorityLeaves;

	[NonSerialized]
	internal NetworkObjectRuntimeFlags RuntimeFlags;

	[NonSerialized]
	public NetworkObjectTypeId NetworkTypeId;

	[InlineHelp]
	public NetworkObject[] NestedObjects;

	[InlineHelp]
	public NetworkBehaviour[] NetworkedBehaviours;

	private RenderSource _renderSource;

	public bool ForceRemoteRenderTimeframe = false;

	public unsafe NetworkId Id
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			NetworkObjectHeader* ptr = (NetworkObjectHeader*)Ptr;
			return (ptr != null) ? ptr->Id : default(NetworkId);
		}
	}

	public NetworkRunner Runner
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _runner;
		}
	}

	public Tick LastReceiveTick
	{
		get
		{
			NetworkObjectMeta meta = Meta;
			return (meta != null && meta.HasSnapshots) ? Meta.SnapshotLatest.Tick : default(Tick);
		}
	}

	public string Name => Id.ToString() + (BehaviourUtils.IsAlive(this) ? ("(" + base.name + ")") : "");

	internal Simulation Simulation => BehaviourUtils.IsAlive(Runner) ? Runner.Simulation : null;

	public bool IsValid => BehaviourUtils.IsAlive(Runner) && Runner.Exists(this);

	public bool IsInSimulation => (RuntimeFlags & NetworkObjectRuntimeFlags.InSimulation) == NetworkObjectRuntimeFlags.InSimulation;

	internal unsafe ref NetworkObjectHeader Header
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ref *(NetworkObjectHeader*)Ptr;
		}
	}

	internal unsafe Span<int> Data
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Ptr != null) ? new Span<int>(Ptr + 20, Header.WordCount - 20) : default(Span<int>);
		}
	}

	internal ReadOnlySpan<int> BehaviourChangedTickArray => (Meta != null) ? Meta.BehaviourChangedTickArray : default(Span<int>);

	public bool HasInputAuthority => Simulation?.IsLocalSimulationInputAuthority(ref Header) ?? false;

	public bool HasStateAuthority => Simulation?.IsLocalSimulationStateAuthority(ref Header) ?? false;

	public bool IsProxy => Simulation != null && !Simulation.IsLocalSimulationInputAuthority(ref Header) && !Simulation.IsLocalSimulationStateAuthority(ref Header);

	public bool IsNested => (RuntimeFlags & NetworkObjectRuntimeFlags.IsNested) == NetworkObjectRuntimeFlags.IsNested;

	public unsafe NetworkObject NestingRoot
	{
		get
		{
			if (!IsNested || Runner == null || Ptr == null)
			{
				return null;
			}
			return Runner.FindObject(Header.NestingRoot);
		}
	}

	public RenderTimeframe RenderTimeframe
	{
		get
		{
			if (ForceRemoteRenderTimeframe)
			{
				return RenderTimeframe.Remote;
			}
			int result;
			if (!IsInSimulation)
			{
				Simulation simulation = Simulation;
				if (simulation == null || !simulation.IsLocalSimulationStateAuthority(ref Header))
				{
					result = 1;
					goto IL_0036;
				}
			}
			result = 0;
			goto IL_0036;
			IL_0036:
			return (RenderTimeframe)result;
		}
	}

	public RenderSource RenderSource
	{
		get
		{
			return _renderSource;
		}
		set
		{
			_renderSource = value;
		}
	}

	public float RenderTime
	{
		get
		{
			if (!BehaviourUtils.IsAlive(Runner))
			{
				return 0f;
			}
			if (RenderTimeframe == RenderTimeframe.Local)
			{
				return Runner.LocalRenderTime;
			}
			return Runner.RemoteRenderTime;
		}
	}

	public unsafe PlayerRef InputAuthority => (Ptr == null) ? PlayerRef.None : Header.InputAuthority;

	public unsafe PlayerRef StateAuthority => (Ptr == null) ? PlayerRef.None : Runner.Simulation.GetStateAuthority(Header.StateAuthority);

	public bool IsSpawnable
	{
		get
		{
			return !Flags.IsIgnored();
		}
		set
		{
			Flags.SetIgnored(!value);
		}
	}

	internal void PrepareBehaviourOrder()
	{
		if (NetworkedBehaviours == null || NetworkedBehaviours.Length == 0)
		{
			RuntimeFlags &= ~NetworkObjectRuntimeFlags.HasMainNetworkTRSP;
			return;
		}
		bool flag = false;
		for (int i = 0; i < NetworkedBehaviours.Length; i++)
		{
			if (NetworkedBehaviours[i] is NetworkTRSP networkTRSP)
			{
				if ((object)NetworkedBehaviours[i].gameObject == base.gameObject && !flag)
				{
					networkTRSP.IsMainTRSP = true;
					flag = true;
					Assert.Check(NetworkedBehaviours[i].ObjectIndex == i);
					RuntimeFlags |= NetworkObjectRuntimeFlags.HasMainNetworkTRSP;
					NetworkBehaviour networkBehaviour = NetworkedBehaviours[0];
					NetworkedBehaviours[0] = NetworkedBehaviours[i];
					NetworkedBehaviours[i] = networkBehaviour;
					NetworkedBehaviours[0].ObjectIndex = 0;
					NetworkedBehaviours[i].ObjectIndex = i;
				}
				else
				{
					networkTRSP.IsMainTRSP = false;
				}
			}
		}
		if (!flag)
		{
			RuntimeFlags &= ~NetworkObjectRuntimeFlags.HasMainNetworkTRSP;
		}
	}

	protected virtual void Awake()
	{
		Assert.Check(!RuntimeFlags.CheckFlag(NetworkObjectRuntimeFlags.HadAwake));
		RuntimeFlags |= NetworkObjectRuntimeFlags.HadAwake;
		DebugAwake();
		if (RuntimeFlags.CheckFlag(NetworkObjectRuntimeFlags.NotAwakeWhenAttaching) && Id.IsValid)
		{
			if (BehaviourUtils.IsAlive(Runner))
			{
				Runner.AttachActivatedByUser(this);
			}
			else
			{
				InternalLogStreams.LogDebug?.Warn(this, "Expected to be activated while the runner is active");
			}
		}
	}

	protected virtual void OnDestroy()
	{
		OnDestroyInternal();
		DebugOnDestroy(wasActive: true);
	}

	internal void OnDestroyNeverActive()
	{
		Assert.Check(!RuntimeFlags.CheckFlag(NetworkObjectRuntimeFlags.HadAwake), "Object was not supposed to be activated {0}", LogUtils.GetDump(this));
		Assert.Check(RuntimeFlags.CheckFlag(NetworkObjectRuntimeFlags.NotAwakeWhenAttaching), "Expected to have the flag {0}", LogUtils.GetDump(this));
		OnDestroyInternal();
		Assert.Check((RuntimeFlags & NetworkObjectRuntimeFlags.Spawned) == 0, "Never should have become active");
		DebugOnDestroy(wasActive: false);
	}

	private unsafe void OnDestroyInternal()
	{
		if (BehaviourUtils.IsAlive(this))
		{
			RuntimeFlags |= NetworkObjectRuntimeFlags.IsDestroyed;
			if (BehaviourUtils.IsAlive(Runner))
			{
				bool flag = Ptr != null && Runner.Simulation != null && (HasStateAuthority || (StateAuthority == PlayerRef.None && Runner.IsSharedModeMasterClient));
				Runner.Destroy(this, NetworkObjectDestroyFlags.DestroyedByEngine | (flag ? NetworkObjectDestroyFlags.DestroyState : NetworkObjectDestroyFlags.None));
			}
			else if (BehaviourUtils.IsNotNull(Runner) && Id.IsValid)
			{
				InternalLogStreams.LogDebug?.Warn(this, "Runner has been destroyed, but the object has not been despawned.");
			}
			Ptr = null;
		}
	}

	internal unsafe void ResetNetworkState()
	{
		MakeUnowned();
		Ptr = default(int*);
		NetworkTypeId = default(NetworkObjectTypeId);
		RuntimeFlags &= ~NetworkObjectRuntimeFlags.ClearMask;
	}

	internal unsafe void Defaults()
	{
		Assert.Check(Ptr != null);
		Header.InputAuthority = default(PlayerRef);
		Header.StateAuthority = default(PlayerRef);
	}

	public static int GetWordCount(NetworkObject obj)
	{
		if (BehaviourUtils.IsAlive(obj))
		{
			int num = NetworkStructUtils.GetWordCount<NetworkObjectHeader>();
			for (int i = 0; i < obj.NetworkedBehaviours.Length; i++)
			{
				if (BehaviourUtils.IsAlive(obj.NetworkedBehaviours[i]))
				{
					num += NetworkBehaviourUtils.GetWordCount(obj.NetworkedBehaviours[i]);
					continue;
				}
				throw new Exception("Found missing NetworkBehaviour reference in NetworkBehaviours[] list on " + obj.Name + ". Re-baking of object required. Please check prefab or scene object and make sure NetworkBehaviour list is up to date.");
			}
			return num + obj.NetworkedBehaviours.Length;
		}
		return 0;
	}

	public int GetLocalAuthorityMask()
	{
		return Simulation?.GetLocalAuthorityMask(ref Header) ?? 0;
	}

	public void AssignInputAuthority(PlayerRef player)
	{
		Assert.Check(BehaviourUtils.IsAlive(Runner));
		Assert.Check(Runner.Exists(this));
		if (Runner.Topology != Topologies.ClientServer && (!HasStateAuthority || !(Runner.LocalPlayer == player)))
		{
			return;
		}
		PlayerRef inputAuthority = Header.InputAuthority;
		Header.InputAuthority = player;
		if ((Simulation.Config.ReplicationFeatures & NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement) == NetworkProjectConfig.ReplicationFeatures.SchedulingAndInterestManagement)
		{
			if (inputAuthority.IsRealPlayer && Simulation.TryGetSimulationConnectionForPlayer(inputAuthority, out var sc))
			{
				sc.RemoveAlwaysInterested(Meta);
			}
			if (player.IsRealPlayer && Simulation.TryGetSimulationConnectionForPlayer(player, out var sc2))
			{
				sc2.AddAlwaysInterested(Meta);
			}
		}
	}

	public void RequestStateAuthority()
	{
		Assert.Check(BehaviourUtils.IsAlive(Runner));
		Assert.Check(Runner.Exists(this));
		if (Runner.IsClient && !HasStateAuthority)
		{
			Runner.Simulation.RequestStateAuthority(Id, wants: true);
		}
	}

	public void ReleaseStateAuthority()
	{
		Assert.Check(BehaviourUtils.IsAlive(Runner));
		Assert.Check(Runner.Exists(this));
		if (Runner.IsClient && HasStateAuthority)
		{
			Runner.Simulation.RequestStateAuthority(Id, wants: false);
		}
	}

	public void RemoveInputAuthority()
	{
		AssignInputAuthority(default(PlayerRef));
	}

	public static implicit operator NetworkId(NetworkObject obj)
	{
		return BehaviourUtils.IsNull(obj) ? default(NetworkId) : obj.Id;
	}

	public void SetPlayerAlwaysInterested(PlayerRef player, bool alwaysInterested)
	{
		if (HasStateAuthority)
		{
			Runner.Simulation.SetPlayerAlwaysInterested(player, this, alwaysInterested);
		}
	}

	public unsafe void CopyStateFrom(NetworkObject source)
	{
		Assert.Check(source.Id.IsValid, "Invalid NetworkId from source NetworkObject");
		Assert.Check(source.Id.Equals(Id), "NetworkObjects must have the same NetworkIds");
		Assert.Check(Ptr != null);
		Assert.Check(source.Ptr != null);
		Assert.Check(Header.Type.Equals(source.Header.Type), "NetworkObjects must be of the same type");
		Native.MemCpy(Data, source.Data);
		for (int i = 0; i < NestedObjects.Length; i++)
		{
			NestedObjects[i].CopyStateFrom(source.NestedObjects[i]);
		}
	}

	public unsafe void CopyStateFrom(NetworkObjectHeaderPtr source)
	{
		Assert.Check(Ptr != null);
		Assert.Check(Header.Type.Equals(source.Ptr->Type), "NetworkObjects must be of the same type");
		Native.MemCpy(Data, source.Data);
	}

	[Obsolete("Use NetworkWrap(NetworkObject) instead")]
	public static NetworkId NetworkWrap(NetworkRunner runner, NetworkObject obj)
	{
		return NetworkWrap(obj);
	}

	[NetworkSerializeMethod]
	public static NetworkId NetworkWrap(NetworkObject obj)
	{
		if (BehaviourUtils.IsNotAlive(obj))
		{
			return default(NetworkId);
		}
		return obj.Id;
	}

	[NetworkDeserializeMethod]
	public static void NetworkUnwrap(NetworkRunner runner, NetworkId wrapper, ref NetworkObject result)
	{
		if (!wrapper.IsValid)
		{
			result = null;
		}
		else if (!runner.TryFindObject(wrapper, out result))
		{
			Assert.Check(BehaviourUtils.IsNotAlive(result));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void MakeOwned(NetworkRunner runner)
	{
		Assert.Check(_runner == null, "Already owned {0}", _runner);
		_runner = runner;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void MakeUnowned()
	{
		_runner = null;
	}

	private void DebugAwake()
	{
		InternalLogStreams.LogTraceObject?.Log(this, $"Awake ({RuntimeFlags})");
		if ((RuntimeFlags & NetworkObjectRuntimeFlags.Spawned) != 0)
		{
			InternalLogStreams.LogError?.Log(this, "Spawned before Awake");
		}
	}

	private void DebugOnDestroy(bool wasActive)
	{
		InternalLogStreams.LogTraceObject?.Log(this, $"OnDestroy ({RuntimeFlags})");
		if (wasActive)
		{
			if ((RuntimeFlags & NetworkObjectRuntimeFlags.Spawned) != 0)
			{
				InternalLogStreams.LogError?.Log(this, "Not despawned before OnDestroy");
			}
		}
		else if ((RuntimeFlags & NetworkObjectRuntimeFlags.HadAwake) != 0)
		{
			InternalLogStreams.LogError?.Log(this, "Should not have been awoken");
		}
	}

	protected internal override void GetDumpString(StringBuilder builder)
	{
		builder.Append("[");
		builder.Append(base.DebugNameThreadSafe);
		if (Id.IsValid)
		{
			builder.Append(" ");
			builder.Append(Id.ToString());
		}
		int length = builder.Length;
		if (NetworkRunner.TryGetPrettyRunnerName(builder, Runner))
		{
			builder.Insert(length, "@");
		}
		builder.Append("]");
	}
}
