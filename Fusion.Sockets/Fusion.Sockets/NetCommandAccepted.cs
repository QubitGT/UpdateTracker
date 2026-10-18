using System.Runtime.InteropServices;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
internal struct NetCommandAccepted
{
	[FieldOffset(0)]
	public NetCommandHeader Header;

	[FieldOffset(8)]
	public NetConnectionId AcceptedLocalId;

	[FieldOffset(16)]
	public NetConnectionId AcceptedRemoteId;

	[FieldOffset(24)]
	public uint Counter;

	public static NetCommandAccepted Create(NetConnectionId localId, NetConnectionId remoteId, uint counter)
	{
		NetCommandAccepted result = default(NetCommandAccepted);
		result.Header = NetCommands.Accepted;
		result.AcceptedLocalId = localId;
		result.AcceptedRemoteId = remoteId;
		result.Counter = counter;
		return result;
	}
}
