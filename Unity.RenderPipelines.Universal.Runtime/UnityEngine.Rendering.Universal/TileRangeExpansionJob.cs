using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

[BurstCompile(FloatMode = FloatMode.Fast, DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct TileRangeExpansionJob : IJobFor
{
	[ReadOnly]
	public NativeArray<InclusiveRange> tileRanges;

	[NativeDisableParallelForRestriction]
	public NativeArray<uint> tileMasks;

	public int rangesPerItem;

	public int itemsPerTile;

	public int wordsPerTile;

	public int2 tileResolution;

	public void Execute(int jobIndex)
	{
		int num = jobIndex % tileResolution.y;
		int num2 = jobIndex / tileResolution.y;
		int num3 = 0;
		NativeArray<short> nativeArray = new NativeArray<short>(itemsPerTile, Allocator.Temp);
		NativeArray<InclusiveRange> nativeArray2 = new NativeArray<InclusiveRange>(itemsPerTile, Allocator.Temp);
		for (int i = 0; i < itemsPerTile; i++)
		{
			InclusiveRange value = tileRanges[num2 * rangesPerItem * itemsPerTile + i * rangesPerItem + 1 + num];
			if (!value.isEmpty)
			{
				nativeArray[num3] = (short)i;
				nativeArray2[num3] = value;
				num3++;
			}
		}
		int num4 = num2 * wordsPerTile * tileResolution.x * tileResolution.y + num * wordsPerTile * tileResolution.x;
		for (int j = 0; j < tileResolution.x; j++)
		{
			int num5 = num4 + j * wordsPerTile;
			for (int k = 0; k < num3; k++)
			{
				int num6 = nativeArray[k];
				int num7 = num6 / 32;
				uint num8 = (uint)(1 << num6 % 32);
				if (nativeArray2[k].Contains((short)j))
				{
					tileMasks[num5 + num7] |= num8;
				}
			}
		}
		nativeArray.Dispose();
		nativeArray2.Dispose();
	}
}
