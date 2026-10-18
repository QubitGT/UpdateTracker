using System;

namespace Unity.XR.CoreUtils;

public static class GuidUtil
{
	public static Guid Compose(ulong low, ulong high)
	{
		return new Guid((uint)(low & 0xFFFFFFFFu), (ushort)((low & 0xFFFF00000000L) >> 32), (ushort)((low & 0xFFFF000000000000uL) >> 48), (byte)(high & 0xFF), (byte)((high & 0xFF00) >> 8), (byte)((high & 0xFF0000) >> 16), (byte)((high & 0xFF000000u) >> 24), (byte)((high & 0xFF00000000L) >> 32), (byte)((high & 0xFF0000000000L) >> 40), (byte)((high & 0xFF000000000000L) >> 48), (byte)((high & 0xFF00000000000000uL) >> 56));
	}
}
