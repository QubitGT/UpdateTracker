using System.Runtime.InteropServices;

namespace NanoSockets;

[StructLayout(LayoutKind.Explicit)]
public struct Socket
{
	[FieldOffset(0)]
	public long handle;

	public bool IsCreated => handle > 0;

	public static implicit operator long(Socket socket)
	{
		return socket.handle;
	}

	public static implicit operator Socket(long handle)
	{
		Socket result = default(Socket);
		result.handle = handle;
		return result;
	}
}
