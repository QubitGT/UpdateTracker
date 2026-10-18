namespace Fusion.Sockets;

public enum NetDisconnectReason : byte
{
	Unknown = 1,
	Timeout,
	Requested,
	SequenceOutOfBounds,
	SendWindowFull,
	ByRemote
}
