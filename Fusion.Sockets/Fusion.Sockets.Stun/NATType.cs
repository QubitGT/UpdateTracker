namespace Fusion.Sockets.Stun;

public enum NATType : byte
{
	Invalid = 0,
	UdpBlocked = 1,
	OpenInternet = 2,
	FullCone = 4,
	Symmetric = 8
}
