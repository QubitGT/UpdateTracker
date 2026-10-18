using System.Runtime.InteropServices;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
internal struct NetUnreliableHeader
{
	public const int SIZE = 1;

	public const int SIZE_IN_BITS = 8;

	[FieldOffset(0)]
	public NetPacketType PacketType;

	public static NetUnreliableHeader Create()
	{
		NetUnreliableHeader result = default(NetUnreliableHeader);
		result.PacketType = NetPacketType.UnreliableData;
		return result;
	}
}
