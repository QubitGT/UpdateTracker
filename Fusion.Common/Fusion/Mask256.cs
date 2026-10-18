using System;
using UnityEngine;

namespace Fusion;

[Serializable]
public struct Mask256 : IEquatable<Mask256>
{
	[SerializeField]
	private unsafe fixed long values[4];

	public unsafe long this[int i]
	{
		get
		{
			return values[i];
		}
		set
		{
			values[i] = value;
		}
	}

	public unsafe void Clear()
	{
		values[0] = 0L;
		values[1] = 0L;
		values[2] = 0L;
		values[3] = 0L;
	}

	public unsafe void SetBit(int bitIndex, bool set)
	{
		if (set)
		{
			if (bitIndex < 64)
			{
				values[0] |= 1L << bitIndex;
			}
			else if (bitIndex < 128)
			{
				ref long reference = ref values[1];
				reference |= 1L << bitIndex - 64;
			}
			else if (bitIndex < 192)
			{
				ref long reference2 = ref values[2];
				reference2 |= 1L << bitIndex - 128;
			}
			else if (bitIndex < 256)
			{
				ref long reference3 = ref values[3];
				reference3 |= 1L << bitIndex - 192;
			}
		}
		else if (bitIndex < 64)
		{
			values[0] &= ~(1L << bitIndex);
		}
		else if (bitIndex < 128)
		{
			ref long reference4 = ref values[1];
			reference4 &= ~(1L << bitIndex - 64);
		}
		else if (bitIndex < 192)
		{
			ref long reference5 = ref values[2];
			reference5 &= ~(1L << bitIndex - 128);
		}
		else if (bitIndex < 256)
		{
			ref long reference6 = ref values[3];
			reference6 &= ~(1L << bitIndex - 192);
		}
	}

	public unsafe bool GetBit(int bitIndex)
	{
		if (bitIndex < 64)
		{
			return (values[0] & (1L << bitIndex)) != 0;
		}
		if (bitIndex < 128)
		{
			return (values[1] & (1L << bitIndex - 64)) != 0;
		}
		if (bitIndex < 192)
		{
			return (values[2] & (1L << bitIndex - 128)) != 0;
		}
		if (bitIndex < 256)
		{
			return (values[3] & (1L << bitIndex - 192)) != 0;
		}
		return false;
	}

	public unsafe Mask256(long a, long b = 0L, long c = 0L, long d = 0L)
	{
		this = default(Mask256);
		values[0] = a;
		values[1] = b;
		values[2] = c;
		values[3] = d;
	}

	public unsafe static implicit operator long(Mask256 mask)
	{
		return mask.values[0];
	}

	public static implicit operator Mask256(long value)
	{
		return new Mask256(value, 0L, 0L, 0L);
	}

	public unsafe static Mask256 operator &(Mask256 a, Mask256 b)
	{
		return new Mask256(a.values[0] & b.values[0], a.values[1] & b.values[1], a.values[2] & b.values[2], a.values[3] & b.values[3]);
	}

	public unsafe static Mask256 operator |(Mask256 a, Mask256 b)
	{
		return new Mask256(a.values[0] | b.values[0], a.values[1] | b.values[1], a.values[2] | b.values[2], a.values[3] | b.values[3]);
	}

	public unsafe static Mask256 operator ~(Mask256 a)
	{
		return new Mask256(~a.values[0], ~a.values[1], ~a.values[2], ~a.values[3]);
	}

	public override bool Equals(object obj)
	{
		return obj is Mask256 other && Equals(other);
	}

	public unsafe override int GetHashCode()
	{
		return values[0].GetHashCode() ^ values[1].GetHashCode() ^ values[2].GetHashCode() ^ values[3].GetHashCode();
	}

	public unsafe bool Equals(Mask256 other)
	{
		return values[0] == other.values[0] && values[1] == other.values[1] && values[2] == other.values[2] && values[3] == other.values[3];
	}

	public unsafe bool IsNothing()
	{
		return values[0] == 0L && values[1] == 0L && values[2] == 0L && values[3] == 0;
	}

	public unsafe override string ToString()
	{
		return $"{values[0]}:{values[1]}:{values[2]}:{values[3]}";
	}
}
