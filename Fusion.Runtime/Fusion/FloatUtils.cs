using System.Runtime.CompilerServices;

namespace Fusion;

public static class FloatUtils
{
	public const int DEFAULT_ACCURACY = 1024;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe static int Compress(float f, int accuracy = 1024)
	{
		return Maths.ZigZagEncode((int)(f * (float)accuracy + (0.5f - (float)(*(uint*)(&f) >> 31))));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Decompress(int value, float accuracy = 1024f)
	{
		return (float)Maths.ZigZagDecode(value) / accuracy;
	}
}
