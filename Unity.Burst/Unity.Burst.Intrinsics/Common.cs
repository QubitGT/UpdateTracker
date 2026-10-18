namespace Unity.Burst.Intrinsics;

public static class Common
{
	public static void Pause()
	{
	}

	public static ulong umul128(ulong x, ulong y, out ulong high)
	{
		ulong num = (uint)x;
		ulong num2 = x >> 32;
		ulong num3 = (uint)y;
		ulong num4 = y >> 32;
		ulong num5 = num2 * num4;
		ulong num6 = num2 * num3;
		ulong num7 = num4 * num;
		ulong num8 = num * num3;
		ulong num9 = (uint)num6;
		ulong num10 = num8 >> 32;
		ulong num11 = num6 >> 32;
		high = num5 + num11 + (num10 + num9 + num7 >> 32);
		return x * y;
	}
}
