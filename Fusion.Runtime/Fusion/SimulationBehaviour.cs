#define TRACE
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Fusion;

[ScriptHelp(BackColor = ScriptHeaderBackColor.Green)]
[HelpURL("https://doc.photonengine.com/fusion/current/manual/network-object#simulationbehaviour")]
public abstract class SimulationBehaviour : Behaviour
{
	[NonSerialized]
	internal SimulationBehaviour Prev;

	[NonSerialized]
	internal SimulationBehaviour Next;

	[NonSerialized]
	internal SimulationBehaviourRuntimeFlags Flags = SimulationBehaviourRuntimeFlags.IsUnityDisabled;

	private NetworkRunner _runner;

	private NetworkObject _object;

	public bool CanReceiveRenderCallback
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & (SimulationBehaviourRuntimeFlags.PendingRemoval | SimulationBehaviourRuntimeFlags.IsUnityDestroyed | SimulationBehaviourRuntimeFlags.IsUnityDisabled)) == 0;
		}
	}

	public bool CanReceiveSimulationCallback
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (Flags & (SimulationBehaviourRuntimeFlags.PendingRemoval | SimulationBehaviourRuntimeFlags.IsUnityDestroyed | SimulationBehaviourRuntimeFlags.IsUnityDisabled)) == 0 && (Flags & (SimulationBehaviourRuntimeFlags.IsGlobal | SimulationBehaviourRuntimeFlags.InSimulation)) != 0;
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

	public NetworkObject Object
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _object;
		}
	}

	public virtual void FixedUpdateNetwork()
	{
	}

	internal virtual void PreRender()
	{
	}

	public virtual void Render()
	{
	}

	private void OnDestroy()
	{
		NetworkBehaviourUtils.InternalOnDestroy(this);
	}

	private void OnEnable()
	{
		NetworkBehaviourUtils.InternalOnEnable(this);
	}

	private void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void MakeOwned(NetworkRunner runner, NetworkObject obj)
	{
		_runner = runner;
		_object = obj;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void MakeUnowned()
	{
		_runner = null;
		_object = null;
	}

	[Conditional("DEBUG")]
	internal void DebugNotifySpawned()
	{
		InternalLogStreams.LogTraceObject?.Log(this, "Spawned");
	}

	[Conditional("DEBUG")]
	internal void DebugNotifyDespawned()
	{
		InternalLogStreams.LogTraceObject?.Log(this, "Despawned");
	}

	protected internal override void GetDumpString(StringBuilder builder)
	{
		builder.Append("[");
		builder.Append(base.DebugNameThreadSafe);
		if (this is NetworkBehaviour { Id: { IsValid: not false } } networkBehaviour)
		{
			builder.Append(" ");
			builder.Append(networkBehaviour.Id.ToString());
		}
		int length = builder.Length;
		if (NetworkRunner.TryGetPrettyRunnerName(builder, Runner))
		{
			builder.Insert(length, "@");
		}
		builder.Append("]");
	}
}
