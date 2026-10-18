using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(4)]
public struct NetworkRNG : INetworkStruct
{
	internal const double FP_32_32_ToUnitDoubleInclusive = 2.3283064370807974E-10;

	internal const double FP_32_32_ToUnitDoubleExclusive = 2.3283064365386963E-10;

	internal const float FP_8_24_ToUnitSingleInclusive = 5.960465E-08f;

	internal const float FP_8_24_ToUnitSingleExclusive = 5.9604645E-08f;

	public const int SIZE = 16;

	public const uint MAX = uint.MaxValue;

	[FieldOffset(0)]
	private ulong _state;

	[FieldOffset(8)]
	private ulong _inc;

	public NetworkRNG Peek
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double Next()
	{
		return (double)NextUInt32Internal() * 2.3283064370807974E-10;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public double NextExclusive()
	{
		return (double)NextUInt32Internal() * 2.3283064365386963E-10;
	}

	public float NextSingle()
	{
		return (float)(NextUInt32Internal() >> 8) * 5.960465E-08f;
	}

	public float NextSingleExclusive()
	{
		return (float)(NextUInt32Internal() >> 8) * 5.9604645E-08f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int NextInt32()
	{
		return (int)NextUInt32Internal();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint NextUInt32()
	{
		return NextUInt32Internal();
	}

	private uint NextUnbiasedUInt32(uint max)
	{
		uint num = (0 - max) % max;
		uint num2;
		do
		{
			num2 = NextUInt32Internal();
		}
		while (num2 < num);
		return num2 % max;
	}

	public NetworkRNG(int seed)
	{
		ulong x = (ulong)seed;
		_state = NextSplitMix64(ref x);
		_inc = NextSplitMix64(ref x);
	}

	private uint NextUInt32Internal()
	{
		ulong state = _state;
		_state = state * 6364136223846793005L + (_inc | 1);
		uint num = (uint)(((state >> 18) ^ state) >> 27);
		int num2 = (int)(state >> 59);
		return (num >> num2) | (num << (-num2 & 0x1F));
	}

	public override string ToString()
	{
		return string.Format("{0}:{1}, {2}:{3}", "_state", _state, "_inc", _inc);
	}

	private static ulong NextSplitMix64(ref ulong x)
	{
		ulong num = (x += 11400714819323198485uL);
		num = (num ^ (num >> 30)) * 13787848793156543929uL;
		num = (num ^ (num >> 27)) * 10723151780598845931uL;
		return num ^ (num >> 31);
	}

	public double RangeInclusive(double minInclusive, double maxInclusive)
	{
		if (minInclusive > maxInclusive)
		{
			double num = minInclusive;
			minInclusive = maxInclusive;
			maxInclusive = num;
		}
		return minInclusive + Next() * (maxInclusive - minInclusive);
	}

	public float RangeInclusive(float minInclusive, float maxInclusive)
	{
		if (minInclusive > maxInclusive)
		{
			float num = minInclusive;
			minInclusive = maxInclusive;
			maxInclusive = num;
		}
		return minInclusive + NextSingle() * (maxInclusive - minInclusive);
	}

	public int RangeExclusive(int minInclusive, int maxExclusive)
	{
		if (minInclusive == maxExclusive)
		{
			return minInclusive;
		}
		if (minInclusive > maxExclusive)
		{
			int num = minInclusive;
			minInclusive = maxExclusive;
			maxExclusive = num;
		}
		uint max = (uint)(maxExclusive - minInclusive);
		uint num2 = NextUnbiasedUInt32(max);
		return (int)(minInclusive + num2);
	}

	public int RangeInclusive(int minInclusive, int maxInclusive)
	{
		if (minInclusive > maxInclusive)
		{
			int num = minInclusive;
			minInclusive = maxInclusive;
			maxInclusive = num;
		}
		uint num2 = (uint)(maxInclusive - minInclusive + 1);
		if (num2 == 0)
		{
			return (int)NextUInt32Internal();
		}
		uint num3 = NextUnbiasedUInt32(num2);
		return (int)(minInclusive + num3);
	}

	public uint RangeExclusive(uint minInclusive, uint maxExclusive)
	{
		if (minInclusive == maxExclusive)
		{
			return minInclusive;
		}
		if (minInclusive > maxExclusive)
		{
			uint num = minInclusive;
			minInclusive = maxExclusive;
			maxExclusive = num;
		}
		uint max = maxExclusive - minInclusive;
		uint num2 = NextUnbiasedUInt32(max);
		return minInclusive + num2;
	}

	public uint RangeInclusive(uint minInclusive, uint maxInclusive)
	{
		if (minInclusive > maxInclusive)
		{
			uint num = minInclusive;
			minInclusive = maxInclusive;
			maxInclusive = num;
		}
		uint num2 = maxInclusive - minInclusive + 1;
		if (num2 == 0)
		{
			return NextUInt32Internal();
		}
		uint num3 = NextUnbiasedUInt32(num2);
		return minInclusive + num3;
	}
}
