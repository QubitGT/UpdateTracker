using System.Runtime.InteropServices;
using NanoSockets;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
public struct NetSocket
{
	[FieldOffset(0)]
	public long Handle;

	[FieldOffset(0)]
	public Socket NativeSocket;

	public bool IsCreated => NativeSocket.IsCreated;
}
