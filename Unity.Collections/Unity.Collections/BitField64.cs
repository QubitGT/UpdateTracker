using System;
using System.Diagnostics;
using Unity.Mathematics;

namespace Unity.Collections;

[DebuggerTypeProxy(typeof(BitField64DebugView))]
[GenerateTestsForBurstCompatibility]
public struct BitField64
{
	public ulong Value;

	public BitField64(ulong initialValue = 0uL)
	{
		Value = initialValue;
	}

	public void Clear()
	{
		Value = 0uL;
	}

	public void SetBits(int pos, bool value)
	{
		Value = Bitwise.SetBits(Value, pos, 1uL, value);
	}

	public void SetBits(int pos, bool value, int numBits = 1)
	{
		ulong mask = ulong.MaxValue >> 64 - numBits;
		Value = Bitwise.SetBits(Value, pos, mask, value);
	}

	public ulong GetBits(int pos, int numBits = 1)
	{
		ulong mask = ulong.MaxValue >> 64 - numBits;
		return Bitwise.ExtractBits(Value, pos, mask);
	}

	public bool IsSet(int pos)
	{
		return GetBits(pos) != 0;
	}

	public bool TestNone(int pos, int numBits = 1)
	{
		return GetBits(pos, numBits) == 0;
	}

	public bool TestAny(int pos, int numBits = 1)
	{
		return GetBits(pos, numBits) != 0;
	}

	public bool TestAll(int pos, int numBits = 1)
	{
		ulong num = ulong.MaxValue >> 64 - numBits;
		return num == Bitwise.ExtractBits(Value, pos, num);
	}

	public int CountBits()
	{
		return math.countbits(Value);
	}

	public int CountLeadingZeros()
	{
		return math.lzcnt(Value);
	}

	public int CountTrailingZeros()
	{
		return math.tzcnt(Value);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckArgs(int pos, int numBits)
	{
		if (pos > 63 || numBits == 0 || numBits > 64 || pos + numBits > 64)
		{
			throw new ArgumentException($"BitField32 invalid arguments: pos {pos} (must be 0-63), numBits {numBits} (must be 1-64).");
		}
	}
}
