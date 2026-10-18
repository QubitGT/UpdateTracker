#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fusion;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(4)]
public struct BitSet128 : INetworkStruct, IEquatable<BitSet128>, IEnumerable<int>, IEnumerable
{
	public struct Iterator
	{
		private int _bit;

		public BitSet128 _set;

		public Iterator(BitSet128 set)
		{
			_set = set;
			_bit = -1;
		}

		public unsafe bool Next(out int index)
		{
			_bit++;
			while (true)
			{
				if (_bit >= 128)
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
				if (_bit >= 128)
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

	public const int SIZE = 16;

	public const int CAPACITY = 128;

	[FieldOffset(0)]
	public unsafe fixed ulong Bits[2];

	public int Length => 128;

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

	public unsafe static BitSet128 FromArray(ulong[] values)
	{
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (2 != values.Length)
		{
			throw new ArgumentException("Array needs to be of length 2", "values");
		}
		BitSet128 result = default(BitSet128);
		for (int i = 0; i < 2; i++)
		{
			result.Bits[i] = values[i];
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Set(int bit)
	{
		Assert.Check(bit >= 0 && bit < 128);
		ref ulong reference = ref Bits[bit / 64];
		reference |= (ulong)(1L << bit % 64);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void Clear(int bit)
	{
		Assert.Check(bit >= 0 && bit < 128);
		ref ulong reference = ref Bits[bit / 64];
		reference &= (ulong)(~(1L << bit % 64));
	}

	public unsafe void And(BitSet128 other)
	{
		Bits[0] &= other.Bits[0];
		ref ulong reference = ref Bits[1];
		reference &= other.Bits[1];
	}

	public unsafe void Or(BitSet128 other)
	{
		Bits[0] |= other.Bits[0];
		ref ulong reference = ref Bits[1];
		reference |= other.Bits[1];
	}

	public unsafe void Xor(BitSet128 other)
	{
		Bits[0] ^= other.Bits[0];
		ref ulong reference = ref Bits[1];
		reference ^= other.Bits[1];
	}

	public unsafe void AndNot(BitSet128 other)
	{
		Bits[0] &= ~other.Bits[0];
		ref ulong reference = ref Bits[1];
		reference &= ~other.Bits[1];
	}

	public unsafe void Not()
	{
		ref ulong fixedElementField = ref Bits[0];
		fixedElementField = ~Bits[0];
		Bits[1] = ~Bits[1];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe void ClearAll()
	{
		Bits[0] = 0uL;
		Bits[1] = 0uL;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool IsSet(int bit)
	{
		return (Bits[bit / 64] & (ulong)(1L << bit % 64)) != 0;
	}

	public unsafe int GetSetCount()
	{
		return Maths.CountSetBits(Bits[0]) + Maths.CountSetBits(Bits[1]);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool Any()
	{
		return Bits[0] != 0L || Bits[1] != 0;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe bool Empty()
	{
		return Bits[0] == 0L && Bits[1] == 0;
	}

	public unsafe override int GetHashCode()
	{
		fixed (ulong* ptr = Bits)
		{
			return HashCodeUtilities.GetArrayHashCode(ptr, 2, 43);
		}
	}

	public override bool Equals(object obj)
	{
		return obj is BitSet128 other && Equals(other);
	}

	public unsafe bool Equals(BitSet128 other)
	{
		return Bits[0] == other.Bits[0] && Bits[1] == other.Bits[1];
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

	public static bool operator ==(BitSet128 a, BitSet128 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(BitSet128 a, BitSet128 b)
	{
		return !a.Equals(b);
	}
}
