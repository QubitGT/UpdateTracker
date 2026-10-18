#define DEBUG
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Fusion;

public static class Maths
{
	[StructLayout(LayoutKind.Explicit)]
	private struct FastAbs2
	{
		[FieldOffset(0)]
		public uint uint32;

		[FieldOffset(0)]
		public float single;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct FastAbs
	{
		public const uint Mask = 2147483647u;

		[FieldOffset(0)]
		public uint UInt32;

		[FieldOffset(0)]
		public float Single;
	}

	private const float ENRANGE = 1.4142133f;

	private const float UNRANGE = 0.7071069f;

	private const uint HALF_ENCODED = 512u;

	private const float ENCODER = 724.0772f;

	private const float DECODER = 0.0013810681f;

	private const uint MASK10BITS = 1023u;

	private static byte[] _debruijnTable32 = new byte[32]
	{
		0, 9, 1, 10, 13, 21, 2, 29, 11, 14,
		16, 18, 22, 25, 3, 30, 8, 12, 20, 28,
		15, 17, 24, 7, 19, 27, 23, 6, 26, 5,
		4, 31
	};

	private static byte[] _debruijnTable64 = new byte[64]
	{
		63, 0, 58, 1, 59, 47, 53, 2, 60, 39,
		48, 27, 54, 33, 42, 3, 61, 51, 37, 40,
		49, 18, 28, 20, 55, 30, 34, 11, 43, 14,
		22, 4, 62, 57, 46, 52, 38, 26, 32, 41,
		50, 36, 17, 19, 29, 10, 13, 21, 56, 45,
		25, 31, 35, 16, 9, 12, 44, 24, 15, 8,
		23, 7, 6, 5
	};

	private static readonly int[] DeBruijnLookupLong = new int[128]
	{
		0, 48, -1, -1, 31, -1, 15, 51, -1, 63,
		5, -1, -1, -1, 19, -1, 23, 28, -1, -1,
		-1, 40, 36, 46, -1, 13, -1, -1, -1, 34,
		-1, 58, -1, 60, 2, 43, 55, -1, -1, -1,
		50, 62, 4, -1, 18, 27, -1, 39, 45, -1,
		-1, 33, 57, -1, 1, 54, -1, 49, -1, 17,
		-1, -1, 32, -1, 53, -1, 16, -1, -1, 52,
		-1, -1, -1, 64, 6, 7, 8, -1, 9, -1,
		-1, -1, 20, 10, -1, -1, 24, -1, 29, -1,
		-1, 21, -1, 11, -1, -1, 41, -1, 25, 37,
		-1, 47, -1, 30, 14, -1, -1, -1, -1, 22,
		-1, -1, 35, 12, -1, -1, -1, 59, 42, -1,
		-1, 61, 3, 26, 38, 44, -1, 56
	};

	public static uint QuaternionCompress(Quaternion rot)
	{
		FastAbs2 fastAbs = default(FastAbs2);
		fastAbs.single = rot.x;
		fastAbs.uint32 &= 2147483647u;
		float single = fastAbs.single;
		fastAbs.single = rot.y;
		fastAbs.uint32 &= 2147483647u;
		float single2 = fastAbs.single;
		fastAbs.single = rot.z;
		fastAbs.uint32 &= 2147483647u;
		float single3 = fastAbs.single;
		fastAbs.single = rot.w;
		fastAbs.uint32 &= 2147483647u;
		float single4 = fastAbs.single;
		int num = ((!(single > single2)) ? 1 : 0);
		int num2 = ((single3 > single4) ? 2 : 3);
		int num3 = ((((num == 0) ? single : single2) > ((num2 == 2) ? single3 : single4)) ? num : num2);
		float num4;
		float num5;
		float num6;
		float num7;
		switch (num3)
		{
		case 0:
			num4 = rot.y;
			num5 = rot.z;
			num6 = rot.w;
			num7 = rot.x;
			break;
		case 1:
			num4 = rot.x;
			num5 = rot.z;
			num6 = rot.w;
			num7 = rot.y;
			break;
		case 2:
			num4 = rot.x;
			num5 = rot.y;
			num6 = rot.w;
			num7 = rot.z;
			break;
		default:
			num4 = rot.x;
			num5 = rot.y;
			num6 = rot.z;
			num7 = rot.w;
			break;
		}
		if (num7 > 0f)
		{
			return (uint)(num4 * 724.0772f + 512f) | ((uint)(num5 * 724.0772f + 512f) << 10) | ((uint)(num6 * 724.0772f + 512f) << 20) | (uint)(num3 << 30);
		}
		return (uint)((0f - num4) * 724.0772f + 512f) | ((uint)((0f - num5) * 724.0772f + 512f) << 10) | ((uint)((0f - num6) * 724.0772f + 512f) << 20) | (uint)(num3 << 30);
	}

	public static Quaternion QuaternionDecompress(uint buffer)
	{
		int num = (int)(0x3FF & buffer);
		int num2 = (int)(0x3FF & (buffer >> 10));
		int num3 = (int)(0x3FF & (buffer >> 20));
		int num4 = (int)(buffer >> 30);
		float num5 = (float)((long)num - 512L) * 0.0013810681f;
		float num6 = (float)((long)num2 - 512L) * 0.0013810681f;
		float num7 = (float)((long)num3 - 512L) * 0.0013810681f;
		float num8 = (float)Math.Sqrt(1.0 - (double)(num5 * num5 + num6 * num6 + num7 * num7));
		return num4 switch
		{
			0 => new Quaternion(num8, num5, num6, num7), 
			1 => new Quaternion(num5, num8, num6, num7), 
			2 => new Quaternion(num5, num6, num8, num7), 
			_ => new Quaternion(num5, num6, num7, num8), 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int SizeOfBits<T>() where T : unmanaged
	{
		return sizeof(T) * 8;
	}

	public static int BytesRequiredForBits(int b)
	{
		return b + 7 >> 3;
	}

	public static int IntsRequiredForBits(int b)
	{
		return b + 31 >> 5;
	}

	public static short BytesRequiredForBits(short b)
	{
		return (short)(b + 7 >> 3);
	}

	public unsafe static string PrintBits(byte* data, int count)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[lo ");
		for (int i = 0; i < count; i++)
		{
			byte b = data[i];
			for (int j = 0; j < 8; j++)
			{
				stringBuilder.Append(((b & (1 << j)) == 1 << j) ? '1' : '0');
			}
			if (i + 1 < count)
			{
				stringBuilder.Append(" ");
			}
		}
		stringBuilder.Append(" hi]");
		return stringBuilder.ToString();
	}

	public static int BitsRequiredForNumber(int n)
	{
		for (int num = 31; num >= 0; num--)
		{
			int num2 = 1 << num;
			if ((n & num2) == num2)
			{
				return num + 1;
			}
		}
		return 0;
	}

	public static int FloorToInt(double value)
	{
		return (int)Math.Floor(value);
	}

	public static int CeilToInt(double value)
	{
		return (int)Math.Ceiling(value);
	}

	public static int CountUsedBitsMinOne(uint value)
	{
		Assert.Check(value != 0);
		int num = 0;
		do
		{
			num++;
			value >>= 1;
		}
		while (value != 0);
		return num;
	}

	public static int BitsRequiredForNumber(uint n)
	{
		for (int num = 31; num >= 0; num--)
		{
			int num2 = 1 << num;
			if ((n & num2) == num2)
			{
				return num + 1;
			}
		}
		return 0;
	}

	public static uint NextPowerOfTwo(uint v)
	{
		v--;
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		v++;
		return v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int CountSetBits(ulong x)
	{
		x -= (x >> 1) & 0x5555555555555555L;
		x = (x & 0x3333333333333333L) + ((x >> 2) & 0x3333333333333333L);
		x = (x + (x >> 4)) & 0xF0F0F0F0F0F0F0FuL;
		return (int)(x * 72340172838076673L >> 56);
	}

	public static double MillisecondsToSeconds(double seconds)
	{
		return seconds / 1000.0;
	}

	public static long SecondsToMilliseconds(double seconds)
	{
		return (long)(seconds * 1000.0);
	}

	public static long SecondsToMicroseconds(double seconds)
	{
		return (long)(seconds * 1000000.0);
	}

	public static double MicrosecondsToSeconds(long microseconds)
	{
		return (double)microseconds / 1000000.0;
	}

	public static long MillisecondsToMicroseconds(long milliseconds)
	{
		return milliseconds * 1000;
	}

	public static double CosineInterpolate(double a, double b, double t)
	{
		double num = (1.0 - Math.Cos(t * Math.PI)) * 0.5;
		return a * (1.0 - num) + b * num;
	}

	public static byte ClampToByte(int v)
	{
		if (v < 0)
		{
			return 0;
		}
		if (v > 255)
		{
			return byte.MaxValue;
		}
		return (byte)v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ZigZagEncode(int i)
	{
		return (i >> 31) ^ (i << 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ZigZagDecode(int i)
	{
		return (i >> 1) ^ -(i & 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long ZigZagEncode(long i)
	{
		return (i >> 63) ^ (i << 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long ZigZagDecode(long i)
	{
		return (i >> 1) ^ -(i & 1);
	}

	public static int Clamp(int v, int min, int max)
	{
		if (v < min)
		{
			return min;
		}
		if (v > max)
		{
			return max;
		}
		return v;
	}

	public static uint Clamp(uint v, uint min, uint max)
	{
		if (v < min)
		{
			return min;
		}
		if (v > max)
		{
			return max;
		}
		return v;
	}

	public static double Clamp(double v, double min, double max)
	{
		if (v < min)
		{
			return min;
		}
		if (v > max)
		{
			return max;
		}
		return v;
	}

	public static float Clamp(float v, float min, float max)
	{
		if (v < min)
		{
			return min;
		}
		if (v > max)
		{
			return max;
		}
		return v;
	}

	public static double Clamp01(double v)
	{
		if (v < 0.0)
		{
			return 0.0;
		}
		if (v > 1.0)
		{
			return 1.0;
		}
		return v;
	}

	public static float Clamp01(float v)
	{
		if (v < 0f)
		{
			return 0f;
		}
		if (v > 1f)
		{
			return 1f;
		}
		return v;
	}

	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * Clamp01(t);
	}

	public static double Lerp(double a, double b, double t)
	{
		return a + (b - a) * Clamp01(t);
	}

	public static uint Min(uint v, uint max)
	{
		if (v > max)
		{
			return max;
		}
		return v;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BitScanReverse(int v)
	{
		return BitScanReverse((uint)v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BitScanReverse(uint v)
	{
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		return _debruijnTable32[v * 130329821 >> 27];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BitScanReverse(long v)
	{
		return BitScanReverse((ulong)v);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int BitScanReverse(ulong v)
	{
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		v |= v >> 32;
		return DeBruijnLookupLong[v * 7783611145303519083L >> 57];
	}
}
