using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Fusion;

public class FusionUnityLogger : FusionUnityLoggerBase
{
	public bool LogActiveRunnerTick;

	public FusionUnityLogger(Thread mainThread, bool isDarkMode)
		: base(mainThread, isDarkMode)
	{
	}

	protected override (string, Object) CreateMessage(in LogContext context)
	{
		bool isMainThread;
		StringBuilder threadSafeStringBuilder = GetThreadSafeStringBuilder(out isMainThread);
		Object @object = context.Source?.GetUnityObject();
		try
		{
			AppendPrefix(threadSafeStringBuilder, context.Flags, context.Prefix);
			int length = threadSafeStringBuilder.Length;
			if (@object != null)
			{
				if (@object is NetworkRunner runner)
				{
					TryAppendRunnerPrefix(threadSafeStringBuilder, runner);
				}
				else if (@object is NetworkObject networkObject)
				{
					TryAppendNetworkObjectPrefix(threadSafeStringBuilder, networkObject);
				}
				else if (@object is SimulationBehaviour simulationBehaviour)
				{
					TryAppendSimulationBehaviourPrefix(threadSafeStringBuilder, simulationBehaviour);
				}
				else
				{
					AppendNameThreadSafe(threadSafeStringBuilder, @object);
				}
			}
			if (LogActiveRunnerTick)
			{
				List<NetworkRunner>.Enumerator instancesEnumerator = NetworkRunner.GetInstancesEnumerator();
				while (instancesEnumerator.MoveNext())
				{
					NetworkRunner current = instancesEnumerator.Current;
					if (!(current == null) && current.IsSimulationUpdating)
					{
						threadSafeStringBuilder.Append(string.Format("[Tick {0}{1}{2}] ", (int)current.Tick, current.IsFirstTick ? "F" : "", (current.Stage == (SimulationStages)0) ? "" : $" {current.Stage}"));
					}
				}
			}
			if (threadSafeStringBuilder.Length > length)
			{
				threadSafeStringBuilder.Append(": ");
			}
			threadSafeStringBuilder.Append(context.Message);
			return (threadSafeStringBuilder.ToString(), isMainThread ? @object : null);
		}
		finally
		{
			threadSafeStringBuilder.Clear();
		}
	}

	private bool TryAppendRunnerPrefix(StringBuilder builder, NetworkRunner runner)
	{
		if ((object)runner == null)
		{
			return false;
		}
		NetworkProjectConfig config = runner.Config;
		if (config == null || config.PeerMode != NetworkProjectConfig.PeerModes.Multiple)
		{
			return false;
		}
		AppendNameThreadSafe(builder, runner);
		PlayerRef localPlayer = runner.LocalPlayer;
		if (localPlayer.IsRealPlayer)
		{
			builder.Append("[P").Append(localPlayer.PlayerId).Append("]");
		}
		else
		{
			builder.Append("[P-]");
		}
		return true;
	}

	private bool TryAppendNetworkObjectPrefix(StringBuilder builder, NetworkObject networkObject)
	{
		if ((object)networkObject == null)
		{
			return false;
		}
		AppendNameThreadSafe(builder, networkObject);
		if (networkObject.Id.IsValid)
		{
			builder.Append(" ");
			builder.Append(networkObject.Id.ToString());
		}
		int length = builder.Length;
		if (TryAppendRunnerPrefix(builder, networkObject.Runner))
		{
			builder.Insert(length, '@');
		}
		return true;
	}

	private bool TryAppendSimulationBehaviourPrefix(StringBuilder builder, SimulationBehaviour simulationBehaviour)
	{
		if ((object)simulationBehaviour == null)
		{
			return false;
		}
		AppendNameThreadSafe(builder, simulationBehaviour);
		if (simulationBehaviour is NetworkBehaviour { Id: { IsValid: not false } } networkBehaviour)
		{
			builder.Append(" ");
			builder.Append(networkBehaviour.Id.ToString());
		}
		int length = builder.Length;
		if (TryAppendRunnerPrefix(builder, simulationBehaviour.Runner))
		{
			builder.Insert(length, '@');
		}
		return true;
	}
}
