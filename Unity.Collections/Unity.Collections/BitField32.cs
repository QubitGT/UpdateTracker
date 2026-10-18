using System;
using System.Diagnostics;
using Unity.Mathematics;

namespace Unity.Collections;

[DebuggerTypeProxy(typeof(BitField32DebugView))]
[GenerateTestsForBurstCompatibility]
public struct BitField32
{
	public uint Value;

	public BitField32(uint initialValue = 0u)
	{
		Value = initialValue;
	}

	public void Clear()
	{
		Value = 0u;
	}

	public void SetBits(int pos, bool value)
	{
		Value = Bitwise.SetBits(Value, pos, 1u, value);
	}

	public void SetBits(int pos, bool value, int numBits)
	{
		uint mask = uint.MaxValue >> 32 - numBits;
		Value = Bitwise.SetBits(Value, pos, mask, value);
	}

	public uint GetBits(int pos, int numBits = 1)
	{
		uint mask = uint.MaxValue >> 32 - numBits;
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
		uint num = uint.MaxValue >> 32 - numBits;
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
		if (pos > 31 || numBits == 0 || numBits > 32 || pos + numBits > 32)
		{
			throw new ArgumentException($"BitField32 invalid arguments: pos {pos} (must be 0-31), numBits {numBits} (must be 1-32).");
		}
	}
}
