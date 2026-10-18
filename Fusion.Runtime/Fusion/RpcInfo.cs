namespace Fusion;

public struct RpcInfo
{
	public Tick Tick;

	public PlayerRef Source;

	public RpcChannel Channel;

	public bool IsInvokeLocal;

	public static RpcInfo FromLocal(NetworkRunner runner, RpcChannel channel, RpcHostMode hostMode)
	{
		RpcInfo rpcInfo = default(RpcInfo);
		rpcInfo.Tick = runner.Simulation.Tick;
		rpcInfo.Source = runner.Simulation.LocalPlayer;
		rpcInfo.IsInvokeLocal = true;
		rpcInfo.Channel = channel;
		RpcInfo result = rpcInfo;
		if (hostMode == RpcHostMode.SourceIsServer && runner.Simulation.IsHostPlayer(result.Source) && !runner.IsSinglePlayer)
		{
			result.Source = default(PlayerRef);
		}
		return result;
	}

	public unsafe static RpcInfo FromMessage(NetworkRunner runner, SimulationMessage* message, RpcHostMode hostMode)
	{
		RpcInfo rpcInfo = default(RpcInfo);
		rpcInfo.Tick = message->Tick;
		rpcInfo.Source = message->Source;
		rpcInfo.IsInvokeLocal = false;
		rpcInfo.Channel = (message->GetFlag(8) ? RpcChannel.Unreliable : RpcChannel.Reliable);
		RpcInfo result = rpcInfo;
		if (message->Source.IsNone && hostMode == RpcHostMode.SourceIsHostPlayer && runner.Simulation.TryGetHostPlayer(out var player))
		{
			result.Source = player;
		}
		return result;
	}

	public override string ToString()
	{
		return string.Format("[RpcInfo: {0}={1}, {2}={3}, {4}={5}, {6}={7}]", "Tick", Tick, "Source", Source, "IsInvokeLocal", IsInvokeLocal, "Channel", Channel);
	}
}
