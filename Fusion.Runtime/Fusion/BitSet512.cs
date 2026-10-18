#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(16)]
public struct BitSet512 : INetworkStruct, IEquatable<BitSet512>, IEnumerable<int>, IEnumerable
{
	public struct Iterator
	{
		private int _bit;

		public BitSet512 _set;

		public Iterator(BitSet512 set)
		{
			_set = set;
			_bit = -1;
		}

		public unsafe bool Next(out int index)
		{
			_bit++;
			while (true)
			{
				if (_bit >= 512)
				{
					index = -1;
					return false;
				}
				ulong num = _set.Bits[_bit / 64];
				int num2 = _bit % 64;
				if ((num & (ulong)(1L << num2)) != 0)
				{
					break;
				}
				if (num == 0)
				{
					_bit += 64;
					continue;
				}
				if (*(uint*)((byte*)(&num) + (nint)(num2 / 32) * (nint)4) == 0)
				{
					_bit += 32;
					continue;
				}
				if (*(ushort*)((byte*)(&num) + (nint)(num2 / 16) * (nint)2) == 0)
				{
					_bit += 16;
					continue;
				}
				int num3 = _bit / 64;
				while (++_bit / 64 == num3)
				{
					if ((num & (ulong)(1L << _bit % 64)) != 0)
					{
						index = _bit;
						return true;
					}
				}
			}
			index = _bit;
			return true;
		}
	}

	public struct Enumerator : IEnumerator<int>, IEnumerator, IDisposable
	{
		private unsafe ulong* _bits;

		private int _bit;

		public int Current => _bit;

		object IEnumerator.Current => Current;

		public unsafe Enumerator(ulong* bits)
		{
			_bits = bits;
			_bit = -1;
		}

		public void Reset()
		{
			_bit = -1;
		}

		public unsafe bool MoveNext()
		{
			_bit++;
			while (true)
			{
				if (_bit >= 512)
				{
					return false;
				}
				ulong num = _bits[_bit / 64];
				int num2 = _bit % 64;
				if ((num & (ulong)(1L << num2)) != 0)
				{
					break;
				}
				if (num == 0)
				{
					_bit += 64;
					continue;
				}
				if (*(uint*)((byte*)(&num) + (nint)(num2 / 32) * (nint)4) == 0)
				{
					_bit += 32;
					continue;
				}
				if (*(ushort*)((byte*)(&num) + (nint)(num2 / 16) * (nint)2) == 0)
				{
					_bit += 16;
					continue;
				}
				int num3 = _bit / 64;
				while (++_bit / 64 == num3)
				{
					if ((num & (ulong)(1L << _bit % 64)) != 0)
					{
						return true;
					}
				}
			}
			return true;
		}

		public unsafe void Dispose()
		{
			_bits = null;
			_bit = -1;
		}
	}

	public const int SIZE = 64;

	public const int CAPACITY = 512;

	[FieldOffset(0)]
	public unsafe fixed ulong Bits[8];

	public int Length => 512;

	public bool this[int index]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return IsSet(index);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			if (value)
			{
				Set(index);
			}
			else
			{
				Clear(index);
			}
		}
	}

	public Iterator GetIterator()
	{
		return new Iterator(this);
	}

	public unsafe static BitSet512 FromArray(ulong[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (8 != values.Length)
		{
			throw new ArgumentException("Array needs to be of length 8", "values");
		}
		BitSet512 result = default(BitSet512);
		for (int i = 0; i < 8; i++)
		{
			result.Bits[i] = values[i];
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Set(int bit)
	{
		Assert.Check(bit >= 0 && bit < 512);
		ref ulong reference = ref Bits[bit / 64];
		reference |= (ulong)(1L << bit % 64);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Clear(int bit)
	{
		Assert.Check(bit >= 0 && bit < 512);
		ref ulong reference = ref Bits[bit / 64];
		reference &= (ulong)(~(1L << bit % 64));
	}

	public unsafe void And(BitSet512 other)
	{
		Bits[0] &= other.Bits[0];
		ref ulong reference = ref Bits[1];
		reference &= other.Bits[1];
		ref ulong reference2 = ref Bits[2];
		reference2 &= other.Bits[2];
		ref ulong reference3 = ref Bits[3];
		reference3 &= other.Bits[3];
		ref ulong reference4 = ref Bits[4];
		reference4 &= other.Bits[4];
		ref ulong reference5 = ref Bits[5];
		reference5 &= other.Bits[5];
		ref ulong reference6 = ref Bits[6];
		reference6 &= other.Bits[6];
		ref ulong reference7 = ref Bits[7];
		reference7 &= other.Bits[7];
	}

	public unsafe void Or(BitSet512 other)
	{
		Bits[0] |= other.Bits[0];
		ref ulong reference = ref Bits[1];
		reference |= other.Bits[1];
		ref ulong reference2 = ref Bits[2];
		reference2 |= other.Bits[2];
		ref ulong reference3 = ref Bits[3];
		reference3 |= other.Bits[3];
		ref ulong reference4 = ref Bits[4];
		reference4 |= other.Bits[4];
		ref ulong reference5 = ref Bits[5];
		reference5 |= other.Bits[5];
		ref ulong reference6 = ref Bits[6];
		reference6 |= other.Bits[6];
		ref ulong reference7 = ref Bits[7];
		reference7 |= other.Bits[7];
	}

	public unsafe void Xor(BitSet512 other)
	{
		Bits[0] ^= other.Bits[0];
		ref ulong reference = ref Bits[1];
		reference ^= other.Bits[1];
		ref ulong reference2 = ref Bits[2];
		reference2 ^= other.Bits[2];
		ref ulong reference3 = ref Bits[3];
		reference3 ^= other.Bits[3];
		ref ulong reference4 = ref Bits[4];
		reference4 ^= other.Bits[4];
		ref ulong reference5 = ref Bits[5];
		reference5 ^= other.Bits[5];
		ref ulong reference6 = ref Bits[6];
		reference6 ^= other.Bits[6];
		ref ulong reference7 = ref Bits[7];
		reference7 ^= other.Bits[7];
	}

	public unsafe void AndNot(BitSet512 other)
	{
		Bits[0] &= ~other.Bits[0];
		ref ulong reference = ref Bits[1];
		reference &= ~other.Bits[1];
		ref ulong reference2 = ref Bits[2];
		reference2 &= ~other.Bits[2];
		ref ulong reference3 = ref Bits[3];
		reference3 &= ~other.Bits[3];
		ref ulong reference4 = ref Bits[4];
		reference4 &= ~other.Bits[4];
		ref ulong reference5 = ref Bits[5];
		reference5 &= ~other.Bits[5];
		ref ulong reference6 = ref Bits[6];
		reference6 &= ~other.Bits[6];
		ref ulong reference7 = ref Bits[7];
		reference7 &= ~other.Bits[7];
	}

	public unsafe void Not()
	{
		ref ulong fixedElementField = ref Bits[0];
		fixedElementField = ~Bits[0];
		Bits[1] = ~Bits[1];
		Bits[2] = ~Bits[2];
		Bits[3] = ~Bits[3];
		Bits[4] = ~Bits[4];
		Bits[5] = ~Bits[5];
		Bits[6] = ~Bits[6];
		Bits[7] = ~Bits[7];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void ClearAll()
	{
		Bits[0] = 0uL;
		Bits[1] = 0uL;
		Bits[2] = 0uL;
		Bits[3] = 0uL;
		Bits[4] = 0uL;
		Bits[5] = 0uL;
		Bits[6] = 0uL;
		Bits[7] = 0uL;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool IsSet(int bit)
	{
		return (Bits[bit / 64] & (ulong)(1L << bit % 64)) != 0;
	}

	public unsafe int GetSetCount()
	{
		return Maths.CountSetBits(Bits[0]) + Maths.CountSetBits(Bits[1]) + Maths.CountSetBits(Bits[2]) + Maths.CountSetBits(Bits[3]) + Maths.CountSetBits(Bits[4]) + Maths.CountSetBits(Bits[5]) + Maths.CountSetBits(Bits[6]) + Maths.CountSetBits(Bits[7]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool Any()
	{
		return Bits[0] != 0L || Bits[1] != 0L || Bits[2] != 0L || Bits[3] != 0L || Bits[4] != 0L || Bits[5] != 0L || Bits[6] != 0L || Bits[7] != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool Empty()
	{
		return Bits[0] == 0L && Bits[1] == 0L && Bits[2] == 0L && Bits[3] == 0L && Bits[4] == 0L && Bits[5] == 0L && Bits[6] == 0L && Bits[7] == 0;
	}

	public unsafe override int GetHashCode()
	{
		fixed (ulong* ptr = Bits)
		{
			return HashCodeUtilities.GetArrayHashCode(ptr, 8, 43);
		}
	}

	public override bool Equals(object obj)
	{
		return obj is BitSet512 other && Equals(other);
	}

	public unsafe bool Equals(BitSet512 other)
	{
		return Bits[0] == other.Bits[0] && Bits[1] == other.Bits[1] && Bits[2] == other.Bits[2] && Bits[3] == other.Bits[3] && Bits[4] == other.Bits[4] && Bits[5] == other.Bits[5] && Bits[6] == other.Bits[6] && Bits[7] == other.Bits[7];
	}

	public unsafe Enumerator GetEnumerator()
	{
		fixed (ulong* bits = Bits)
		{
			return new Enumerator(bits);
		}
	}

	IEnumerator<int> IEnumerable<int>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public static bool operator ==(BitSet512 a, BitSet512 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(BitSet512 a, BitSet512 b)
	{
		return !a.Equals(b);
	}
}
