using System;
using Fusion.Sockets;

namespace Fusion;

[Serializable]
public class NetworkConfiguration
{
	[Flags]
	public enum ReliableDataTransfers
	{
		ClientToServer = 1,
		ClientToClientWithServerProxy = 2
	}

	[InlineHelp]
	[Unit(Units.Seconds)]
	public double ConnectionTimeout = 10.0;

	[InlineHelp]
	[Unit(Units.Seconds)]
	public double ConnectionShutdownTime = 1.0;

	[InlineHelp]
	public ReliableDataTransfers ReliableDataTransferModes = ReliableDataTransfers.ClientToServer | ReliableDataTransfers.ClientToClientWithServerProxy;

	public int SocketSendBufferSize => 256;

	public int SocketRecvBufferSize => 256;

	public int ConnectAttempts => 10;

	public double ConnectInterval => 0.5;

	public double ConnectionDefaultRtt => 0.1;

	public double ConnectionPingInterval => 1.0;

	public NetworkConfiguration Init()
	{
		return (NetworkConfiguration)MemberwiseClone();
	}

	internal NetConfig ToNetConfig(NetAddress address)
	{
		NetConfig defaults = NetConfig.Defaults;
		defaults.SocketSendBuffer = SocketSendBufferSize * 1024;
		defaults.SocketRecvBuffer = SocketRecvBufferSize * 1024;
		defaults.ConnectAttempts = ConnectAttempts;
		defaults.ConnectInterval = ConnectInterval;
		defaults.ConnectionDefaultRtt = ConnectionDefaultRtt;
		defaults.ConnectionTimeout = ConnectionTimeout;
		defaults.ConnectionPingInterval = ConnectionPingInterval;
		defaults.ConnectionShutdownTime = ConnectionShutdownTime;
		defaults.Address = address;
		return defaults;
	}
}
