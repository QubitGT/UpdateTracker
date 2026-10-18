namespace Fusion.Sockets;

internal enum NetPacketType : byte
{
	Command = 1,
	UnreliableData,
	NotifyData,
	NotifyAcks,
	Unconnected,
	MtuDiscoveryReq,
	MtuDiscoveryRep,
	NotifyReliableData
}
