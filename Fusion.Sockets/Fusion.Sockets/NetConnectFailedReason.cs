namespace Fusion.Sockets;

public enum NetConnectFailedReason : byte
{
	Timeout = 1,
	ServerFull,
	ServerRefused
}
