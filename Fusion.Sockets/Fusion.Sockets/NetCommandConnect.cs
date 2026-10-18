#define DEBUG
using System.Runtime.InteropServices;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
internal struct NetCommandConnect
{
	public const int TOKEN_MAX_LENGTH_BYTES = 128;

	public const int UNIQUE_ID_LENGTH_BYTES = 8;

	public const int SIZE_BYTES = 152;

	public const int SIZE_BITS = 1216;

	[FieldOffset(0)]
	public NetCommandHeader Header;

	[FieldOffset(4)]
	public int TokenLength;

	[FieldOffset(8)]
	public NetConnectionId ConnectionId;

	[FieldOffset(16)]
	public unsafe fixed byte TokenData[128];

	[FieldOffset(144)]
	public unsafe fixed byte UniqueId[8];

	public static int ClampTokenLength(int tokenLength)
	{
		if (tokenLength < 0)
		{
			InternalLogStreams.LogWarn?.Log("Connection token length can't be negative");
		}
		if (tokenLength > 128)
		{
			InternalLogStreams.LogWarn?.Log($"Connection token length to large, truncated to {128} bytes.");
		}
		return Maths.Clamp(tokenLength, 0, 128);
	}

	public unsafe static byte[] GetTokenDataAsArray(NetCommandConnect command)
	{
		int num = ClampTokenLength(command.TokenLength);
		if (num == 0)
		{
			return null;
		}
		byte[] array = new byte[num];
		fixed (byte* destination = array)
		{
			Native.MemCpy(destination, command.TokenData, num);
		}
		return array;
	}

	public unsafe static byte[] GetUniqueIdAsArray(NetCommandConnect command)
	{
		byte[] array = new byte[8];
		fixed (byte* destination = array)
		{
			Native.MemCpy(destination, command.UniqueId, 8);
		}
		return array;
	}

	public unsafe static NetCommandConnect Create(NetConnectionId id, byte* token = null, int tokenLength = 0, byte* uniqueId = null)
	{
		tokenLength = ClampTokenLength(tokenLength);
		NetCommandConnect result = default(NetCommandConnect);
		result.Header = NetCommands.Connect;
		result.ConnectionId = id;
		result.TokenLength = tokenLength;
		if (result.TokenLength > 0)
		{
			Assert.Check(token != null);
			Native.MemCpy(result.TokenData, token, result.TokenLength);
		}
		if (uniqueId != null)
		{
			Native.MemCpy(result.UniqueId, uniqueId, 8);
		}
		return result;
	}
}
