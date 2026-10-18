using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

[BurstCompile(FloatMode = FloatMode.Fast, DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct ZBinningJob : IJobFor
{
	public const int batchSize = 128;

	public const int headerLength = 2;

	[NativeDisableParallelForRestriction]
	public NativeArray<uint> bins;

	[ReadOnly]
	public NativeArray<float2> minMaxZs;

	public float zBinScale;

	public float zBinOffset;

	public int binCount;

	public int wordsPerTile;

	public int lightCount;

	public int reflectionProbeCount;

	public int batchCount;

	public int viewCount;

	public bool isOrthographic;

	private static uint EncodeHeader(uint min, uint max)
	{
		return (min & 0xFFFFu) | ((max & 0xFFFF) << 16);
	}

	private static (uint, uint) DecodeHeader(uint zBin)
	{
		return (zBin & 0xFFFFu, (zBin >> 16) & 0xFFFFu);
	}

	public void Execute(int jobIndex)
	{
		int num = jobIndex % batchCount;
		int num2 = jobIndex / batchCount;
		int num3 = 128 * num;
		int num4 = math.min(num3 + 128, binCount) - 1;
		int num5 = num2 * binCount;
		uint value = EncodeHeader(65535u, 0u);
		for (int i = num3; i <= num4; i++)
		{
			bins[(num5 + i) * (2 + wordsPerTile)] = value;
			bins[(num5 + i) * (2 + wordsPerTile) + 1] = value;
		}
		FillZBins(num3, num4, 0, lightCount, 0, num2 * lightCount, num5);
		FillZBins(num3, num4, lightCount, lightCount + reflectionProbeCount, 1, lightCount * (viewCount - 1) + num2 * reflectionProbeCount, num5);
	}

	private void FillZBins(int binStart, int binEnd, int itemStart, int itemEnd, int headerIndex, int itemOffset, int binOffset)
	{
		for (int i = itemStart; i < itemEnd; i++)
		{
			float2 @float = minMaxZs[itemOffset + i];
			int num = math.max((int)((isOrthographic ? @float.x : math.log2(@float.x)) * zBinScale + zBinOffset), binStart);
			int num2 = math.min((int)((isOrthographic ? @float.y : math.log2(@float.y)) * zBinScale + zBinOffset), binEnd);
			int num3 = i / 32;
			uint num4 = (uint)(1 << i % 32);
			for (int j = num; j <= num2; j++)
			{
				int num5 = (binOffset + j) * (2 + wordsPerTile);
				(uint, uint) tuple = DecodeHeader(bins[num5 + headerIndex]);
				uint item = tuple.Item1;
				uint item2 = tuple.Item2;
				item = math.min(item, (uint)i);
				item2 = math.max(item2, (uint)i);
				bins[num5 + headerIndex] = EncodeHeader(item, item2);
				bins[num5 + 2 + num3] |= num4;
			}
		}
	}
}
