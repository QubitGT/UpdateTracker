using System;
using System.Runtime.InteropServices;

namespace Fusion.Sockets;

[StructLayout(LayoutKind.Explicit)]
internal struct NetCommandDisconnect
{
	public const int TOKEN_MAX_LENGTH_BYTES = 128;

	[FieldOffset(0)]
	public NetCommandHeader Header;

	[FieldOffset(2)]
	public NetDisconnectReason Reason;

	[FieldOffset(4)]
	public int TokenLength;

	[FieldOffset(8)]
	public unsafe fixed byte TokenData[128];

	public unsafe static NetCommandDisconnect Create(NetDisconnectReason reason, byte[] token)
	{
		int num = Math.Min(128, (token?.Length).GetValueOrDefault());
		NetCommandDisconnect netCommandDisconnect = default(NetCommandDisconnect);
		netCommandDisconnect.Header = NetCommands.Disconnect;
		netCommandDisconnect.Reason = reason;
		netCommandDisconnect.TokenLength = num;
		NetCommandDisconnect result = netCommandDisconnect;
		for (int i = 0; i < num; i++)
		{
			result.TokenData[i] = token[i];
		}
		return result;
	}

	public unsafe static NetCommandDisconnect Create(NetDisconnectReason reason, byte* token, int tokenLength)
	{
		tokenLength = Math.Min(128, tokenLength);
		NetCommandDisconnect netCommandDisconnect = default(NetCommandDisconnect);
		netCommandDisconnect.Header = NetCommands.Disconnect;
		netCommandDisconnect.Reason = reason;
		netCommandDisconnect.TokenLength = tokenLength;
		NetCommandDisconnect result = netCommandDisconnect;
		for (int i = 0; i < tokenLength; i++)
		{
			result.TokenData[i] = token[i];
		}
		return result;
	}
}
