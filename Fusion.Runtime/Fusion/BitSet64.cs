#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(2)]
public struct BitSet64 : INetworkStruct, IEquatable<BitSet64>, IEnumerable<int>, IEnumerable
{
	public struct Iterator
	{
		private int _bit;

		public BitSet64 _set;

		public Iterator(BitSet64 set)
		{
			_set = set;
			_bit = -1;
		}

		public unsafe bool Next(out int index)
		{
			_bit++;
			while (true)
			{
				if (_bit >= 64)
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
				if (_bit >= 64)
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

	public const int SIZE = 8;

	public const int CAPACITY = 64;

	[FieldOffset(0)]
	public unsafe fixed ulong Bits[1];

	public int Length => 64;

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

	public unsafe static BitSet64 FromValue(ulong value)
	{
		if (value > 9223372036854775808uL)
		{
			throw new ArgumentOutOfRangeException("value");
		}
		BitSet64 result = default(BitSet64);
		result.Bits[0] = value;
		return result;
	}

	public unsafe static BitSet64 FromArray(ulong[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (1 != values.Length)
		{
			throw new ArgumentException("Array needs to be of length 1", "values");
		}
		BitSet64 result = default(BitSet64);
		for (int i = 0; i < 1; i++)
		{
			result.Bits[i] = values[i];
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Set(int bit)
	{
		Assert.Check(bit >= 0 && bit < 64);
		ref ulong reference = ref Bits[bit / 64];
		reference |= (ulong)(1L << bit % 64);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Clear(int bit)
	{
		Assert.Check(bit >= 0 && bit < 64);
		ref ulong reference = ref Bits[bit / 64];
		reference &= (ulong)(~(1L << bit % 64));
	}

	public unsafe void And(BitSet64 other)
	{
		Bits[0] &= other.Bits[0];
	}

	public unsafe void Or(BitSet64 other)
	{
		Bits[0] |= other.Bits[0];
	}

	public unsafe void Xor(BitSet64 other)
	{
		Bits[0] ^= other.Bits[0];
	}

	public unsafe void AndNot(BitSet64 other)
	{
		Bits[0] &= ~other.Bits[0];
	}

	public unsafe void Not()
	{
		ref ulong fixedElementField = ref Bits[0];
		fixedElementField = ~Bits[0];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void ClearAll()
	{
		Bits[0] = 0uL;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool IsSet(int bit)
	{
		return (Bits[bit / 64] & (ulong)(1L << bit % 64)) != 0;
	}

	public unsafe int GetSetCount()
	{
		return Maths.CountSetBits(Bits[0]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool Any()
	{
		return Bits[0] != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool Empty()
	{
		return Bits[0] == 0;
	}

	public unsafe override int GetHashCode()
	{
		fixed (ulong* ptr = Bits)
		{
			return HashCodeUtilities.GetArrayHashCode(ptr, 1, 43);
		}
	}

	public override bool Equals(object obj)
	{
		return obj is BitSet64 other && Equals(other);
	}

	public unsafe bool Equals(BitSet64 other)
	{
		return Bits[0] == other.Bits[0];
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

	public static bool operator ==(BitSet64 a, BitSet64 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(BitSet64 a, BitSet64 b)
	{
		return !a.Equals(b);
	}
}
