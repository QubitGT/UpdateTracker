using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal static class SpaceFillingCurves
{
	private static uint Part1By1(uint x)
	{
		x &= 0xFFFFu;
		x = (x ^ (x << 8)) & 0xFF00FFu;
		x = (x ^ (x << 4)) & 0xF0F0F0Fu;
		x = (x ^ (x << 2)) & 0x33333333u;
		x = (x ^ (x << 1)) & 0x55555555u;
		return x;
	}

	private static uint Compact1By1(uint x)
	{
		x &= 0x55555555u;
		x = (x ^ (x >> 1)) & 0x33333333u;
		x = (x ^ (x >> 2)) & 0xF0F0F0Fu;
		x = (x ^ (x >> 4)) & 0xFF00FFu;
		x = (x ^ (x >> 8)) & 0xFFFFu;
		return x;
	}

	public static uint EncodeMorton2D(uint2 coord)
	{
		return (Part1By1(coord.y) << 1) + Part1By1(coord.x);
	}

	public static uint2 DecodeMorton2D(uint code)
	{
		return math.uint2(Compact1By1(code), Compact1By1(code >> 1));
	}
}
