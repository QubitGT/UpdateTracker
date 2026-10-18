using System;

namespace Unity.XR.CoreUtils;

public static class GuidExtensions
{
	public static void Decompose(this Guid guid, out ulong low, out ulong high)
	{
		byte[] value = guid.ToByteArray();
		low = BitConverter.ToUInt64(value, 0);
		high = BitConverter.ToUInt64(value, 8);
	}
}
