using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Unity.Collections;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[GenerateTestsForBurstCompatibility]
internal struct Bitwise
{
	internal static int AlignDown(int value, int alignPow2)
	{
		return value & ~(alignPow2 - 1);
	}

	internal static int AlignUp(int value, int alignPow2)
	{
		return AlignDown(value + alignPow2 - 1, alignPow2);
	}

	internal static int FromBool(bool value)
	{
		if (!value)
		{
			return 0;
		}
		return 1;
	}

	internal static uint ExtractBits(uint input, int pos, uint mask)
	{
		return (input >> pos) & mask;
	}

	internal static uint ReplaceBits(uint input, int pos, uint mask, uint value)
	{
		uint num = (value & mask) << pos;
		uint num2 = input & ~(mask << pos);
		return num | num2;
	}

	internal static uint SetBits(uint input, int pos, uint mask, bool value)
	{
		return ReplaceBits(input, pos, mask, (uint)(-FromBool(value)));
	}

	internal static ulong ExtractBits(ulong input, int pos, ulong mask)
	{
		return (input >> pos) & mask;
	}

	internal static ulong ReplaceBits(ulong input, int pos, ulong mask, ulong value)
	{
		ulong num = (value & mask) << pos;
		ulong num2 = input & ~(mask << pos);
		return num | num2;
	}

	internal static ulong SetBits(ulong input, int pos, ulong mask, bool value)
	{
		return ReplaceBits(input, pos, mask, (ulong)(-FromBool(value)));
	}

	internal static int lzcnt(byte value)
	{
		return math.lzcnt((uint)value) - 24;
	}

	internal static int tzcnt(byte value)
	{
		return math.min(8, math.tzcnt((uint)value));
	}

	internal static int lzcnt(ushort value)
	{
		return math.lzcnt((uint)value) - 16;
	}

	internal static int tzcnt(ushort value)
	{
		return math.min(16, math.tzcnt((uint)value));
	}

	private unsafe static int FindUlong(ulong* ptr, int beginBit, int endBit, int numBits)
	{
		_ = numBits + 63 >> 6;
		int num = 64;
		int i = beginBit / num;
		for (int num2 = AlignUp(endBit, num) / num; i < num2; i++)
		{
			if (ptr[i] != 0L)
			{
				continue;
			}
			int num3 = i * num;
			int num4 = math.min(num3 + num, endBit) - num3;
			if (num3 != beginBit)
			{
				ulong x = ptr[num3 / num - 1];
				int num5 = math.max(num3 - math.lzcnt(x), beginBit);
				num4 += num3 - num5;
				num3 = num5;
			}
			for (i++; i < num2; i++)
			{
				if (num4 >= numBits)
				{
					return num3;
				}
				ulong num6 = ptr[i];
				int num7 = i * num;
				num4 += math.min(num7 + math.tzcnt(num6), endBit) - num7;
				if (num6 != 0L)
				{
					break;
				}
			}
			if (num4 >= numBits)
			{
				return num3;
			}
		}
		return endBit;
	}

	private unsafe static int FindUint(ulong* ptr, int beginBit, int endBit, int numBits)
	{
		_ = numBits + 31 >> 5;
		int num = 32;
		int i = beginBit / num;
		for (int num2 = AlignUp(endBit, num) / num; i < num2; i++)
		{
			if (*(uint*)((byte*)ptr + (nint)i * (nint)4) != 0)
			{
				continue;
			}
			int num3 = i * num;
			int num4 = math.min(num3 + num, endBit) - num3;
			if (num3 != beginBit)
			{
				uint x = *(uint*)((byte*)ptr + (nint)(num3 / num - 1) * (nint)4);
				int num5 = math.max(num3 - math.lzcnt(x), beginBit);
				num4 += num3 - num5;
				num3 = num5;
			}
			for (i++; i < num2; i++)
			{
				if (num4 >= numBits)
				{
					return num3;
				}
				uint num6 = *(uint*)((byte*)ptr + (nint)i * (nint)4);
				int num7 = i * num;
				num4 += math.min(num7 + math.tzcnt(num6), endBit) - num7;
				if (num6 != 0)
				{
					break;
				}
			}
			if (num4 >= numBits)
			{
				return num3;
			}
		}
		return endBit;
	}

	private unsafe static int FindUshort(ulong* ptr, int beginBit, int endBit, int numBits)
	{
		_ = numBits + 15 >> 4;
		int num = 16;
		int i = beginBit / num;
		for (int num2 = AlignUp(endBit, num) / num; i < num2; i++)
		{
			if (*(ushort*)((byte*)ptr + (nint)i * (nint)2) != 0)
			{
				continue;
			}
			int num3 = i * num;
			int num4 = math.min(num3 + num, endBit) - num3;
			if (num3 != beginBit)
			{
				ushort value = *(ushort*)((byte*)ptr + (nint)(num3 / num - 1) * (nint)2);
				int num5 = math.max(num3 - lzcnt(value), beginBit);
				num4 += num3 - num5;
				num3 = num5;
			}
			for (i++; i < num2; i++)
			{
				if (num4 >= numBits)
				{
					return num3;
				}
				ushort num6 = *(ushort*)((byte*)ptr + (nint)i * (nint)2);
				int num7 = i * num;
				num4 += math.min(num7 + tzcnt(num6), endBit) - num7;
				if (num6 != 0)
				{
					break;
				}
			}
			if (num4 >= numBits)
			{
				return num3;
			}
		}
		return endBit;
	}

	private unsafe static int FindByte(ulong* ptr, int beginBit, int endBit, int numBits)
	{
		_ = numBits + 7 >> 3;
		int num = 8;
		int i = beginBit / num;
		for (int num2 = AlignUp(endBit, num) / num; i < num2; i++)
		{
			if (((byte*)ptr)[i] != 0)
			{
				continue;
			}
			int num3 = i * num;
			int num4 = math.min(num3 + num, endBit) - num3;
			if (num3 != beginBit)
			{
				byte value = ((byte*)ptr)[num3 / num - 1];
				int num5 = math.max(num3 - lzcnt(value), beginBit);
				num4 += num3 - num5;
				num3 = num5;
			}
			for (i++; i < num2; i++)
			{
				if (num4 >= numBits)
				{
					return num3;
				}
				byte b = ((byte*)ptr)[i];
				int num6 = i * num;
				num4 += math.min(num6 + tzcnt(b), endBit) - num6;
				if (b != 0)
				{
					break;
				}
			}
			if (num4 >= numBits)
			{
				return num3;
			}
		}
		return endBit;
	}

	private unsafe static int FindUpto14bits(ulong* ptr, int beginBit, int endBit, int numBits)
	{
		byte b = (byte)((uint)beginBit & 7u);
		byte b2 = (byte)(~(255 << (int)b));
		int num = 0;
		int num2 = beginBit / 8;
		int num3 = AlignUp(endBit, 8) / 8;
		for (int i = num2; i < num3; i++)
		{
			byte b3 = ((byte*)ptr)[i];
			b3 = (byte)(b3 | ((i == num2) ? b2 : 0));
			if (b3 != byte.MaxValue)
			{
				int num4 = i * 8;
				int num5 = math.min(num4 + tzcnt(b3), endBit) - num4;
				if (num + num5 >= numBits)
				{
					return num4 - num;
				}
				num = lzcnt(b3);
				int num6 = num4 + 8;
				int num7 = math.max(num6 - num, beginBit);
				num = math.min(num6, endBit) - num7;
				if (num >= numBits)
				{
					return num7;
				}
			}
		}
		return endBit;
	}

	private unsafe static int FindUpto6bits(ulong* ptr, int beginBit, int endBit, int numBits)
	{
		byte b = (byte)(~(255 << (beginBit & 7)));
		byte b2 = (byte)(~(255 >> ((8 - (endBit & 7)) & 7)));
		int num = 1 << numBits - 1;
		int num2 = beginBit / 8;
		int num3 = AlignUp(endBit, 8) / 8;
		for (int i = num2; i < num3; i++)
		{
			byte b3 = ((byte*)ptr)[i];
			b3 = (byte)(b3 | ((i == num2) ? b : 0));
			b3 = (byte)(b3 | ((i == num3 - 1) ? b2 : 0));
			if (b3 == byte.MaxValue)
			{
				continue;
			}
			int num4 = i * 8;
			int num5 = num4 + 7;
			while (num4 < num5)
			{
				int num6 = tzcnt((byte)(b3 ^ 0xFFu));
				b3 = (byte)(b3 >> num6);
				num4 += num6;
				if ((b3 & num) == 0)
				{
					return num4;
				}
				b3 >>= 1;
				num4++;
			}
		}
		return endBit;
	}

	internal unsafe static int FindWithBeginEnd(ulong* ptr, int beginBit, int endBit, int numBits)
	{
		int num;
		if (numBits >= 127)
		{
			num = FindUlong(ptr, beginBit, endBit, numBits);
			if (num != endBit)
			{
				return num;
			}
		}
		if (numBits >= 63)
		{
			num = FindUint(ptr, beginBit, endBit, numBits);
			if (num != endBit)
			{
				return num;
			}
		}
		if (numBits >= 128)
		{
			return int.MaxValue;
		}
		if (numBits >= 31)
		{
			num = FindUshort(ptr, beginBit, endBit, numBits);
			if (num != endBit)
			{
				return num;
			}
		}
		if (numBits >= 64)
		{
			return int.MaxValue;
		}
		num = FindByte(ptr, beginBit, endBit, numBits);
		if (num != endBit)
		{
			return num;
		}
		if (numBits < 15)
		{
			num = FindUpto14bits(ptr, beginBit, endBit, numBits);
			if (num != endBit)
			{
				return num;
			}
			if (numBits < 7)
			{
				num = FindUpto6bits(ptr, beginBit, endBit, numBits);
				if (num != endBit)
				{
					return num;
				}
			}
		}
		return int.MaxValue;
	}

	internal unsafe static int Find(ulong* ptr, int pos, int count, int numBits)
	{
		return FindWithBeginEnd(ptr, pos, pos + count, numBits);
	}

	internal unsafe static bool TestNone(ulong* ptr, int length, int pos, int numBits = 1)
	{
		int num = math.min(pos + numBits, length);
		int num2 = pos >> 6;
		int num3 = pos & 0x3F;
		int num4 = num - 1 >> 6;
		int num5 = num & 0x3F;
		ulong num6 = (ulong)(-1L << num3);
		ulong num7 = ulong.MaxValue >> 64 - num5;
		if (num2 == num4)
		{
			ulong num8 = num6 & num7;
			return (ptr[num2] & num8) == 0;
		}
		if ((ptr[num2] & num6) != 0L)
		{
			return false;
		}
		for (int i = num2 + 1; i < num4; i++)
		{
			if (ptr[i] != 0L)
			{
				return false;
			}
		}
		return (ptr[num4] & num7) == 0;
	}

	internal unsafe static bool TestAny(ulong* ptr, int length, int pos, int numBits = 1)
	{
		int num = math.min(pos + numBits, length);
		int num2 = pos >> 6;
		int num3 = pos & 0x3F;
		int num4 = num - 1 >> 6;
		int num5 = num & 0x3F;
		ulong num6 = (ulong)(-1L << num3);
		ulong num7 = ulong.MaxValue >> 64 - num5;
		if (num2 == num4)
		{
			ulong num8 = num6 & num7;
			return (ptr[num2] & num8) != 0;
		}
		if ((ptr[num2] & num6) != 0L)
		{
			return true;
		}
		for (int i = num2 + 1; i < num4; i++)
		{
			if (ptr[i] != 0L)
			{
				return true;
			}
		}
		return (ptr[num4] & num7) != 0;
	}

	internal unsafe static bool TestAll(ulong* ptr, int length, int pos, int numBits = 1)
	{
		int num = math.min(pos + numBits, length);
		int num2 = pos >> 6;
		int num3 = pos & 0x3F;
		int num4 = num - 1 >> 6;
		int num5 = num & 0x3F;
		ulong num6 = (ulong)(-1L << num3);
		ulong num7 = ulong.MaxValue >> 64 - num5;
		if (num2 == num4)
		{
			ulong num8 = num6 & num7;
			return num8 == (ptr[num2] & num8);
		}
		if (num6 != (ptr[num2] & num6))
		{
			return false;
		}
		for (int i = num2 + 1; i < num4; i++)
		{
			if (ulong.MaxValue != ptr[i])
			{
				return false;
			}
		}
		return num7 == (ptr[num4] & num7);
	}

	internal unsafe static int CountBits(ulong* ptr, int length, int pos, int numBits = 1)
	{
		int num = math.min(pos + numBits, length);
		int num2 = pos >> 6;
		int num3 = pos & 0x3F;
		int num4 = num - 1 >> 6;
		int num5 = num & 0x3F;
		ulong num6 = (ulong)(-1L << num3);
		ulong num7 = ulong.MaxValue >> 64 - num5;
		if (num2 == num4)
		{
			ulong num8 = num6 & num7;
			return math.countbits(ptr[num2] & num8);
		}
		int num9 = math.countbits(ptr[num2] & num6);
		for (int i = num2 + 1; i < num4; i++)
		{
			num9 += math.countbits(ptr[i]);
		}
		return num9 + math.countbits(ptr[num4] & num7);
	}

	internal unsafe static bool IsSet(ulong* ptr, int pos)
	{
		int num = pos >> 6;
		int num2 = pos & 0x3F;
		ulong num3 = (ulong)(1L << num2);
		return (ptr[num] & num3) != 0;
	}

	internal unsafe static ulong GetBits(ulong* ptr, int length, int pos, int numBits = 1)
	{
		int num = pos >> 6;
		int num2 = pos & 0x3F;
		if (num2 + numBits <= 64)
		{
			ulong mask = ulong.MaxValue >> 64 - numBits;
			return ExtractBits(ptr[num], num2, mask);
		}
		int num3 = math.min(pos + numBits, length);
		int num4 = num3 - 1 >> 6;
		int num5 = num3 & 0x3F;
		ulong mask2 = ulong.MaxValue >> num2;
		ulong num6 = ExtractBits(ptr[num], num2, mask2);
		ulong mask3 = ulong.MaxValue >> 64 - num5;
		return (ExtractBits(ptr[num4], 0, mask3) << 64 - num2) | num6;
	}
}
