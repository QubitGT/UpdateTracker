using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UnityEngine.Rendering;

[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
internal struct PrefixSumDrawInstancesJob : IJob
{
	[ReadOnly]
	public NativeParallelHashMap<RangeKey, int> rangeHash;

	public NativeList<DrawRange> drawRanges;

	public NativeList<DrawBatch> drawBatches;

	public NativeArray<int> drawBatchIndices;

	public void Execute()
	{
		int num = 0;
		for (int i = 0; i < drawRanges.Length; i++)
		{
			ref DrawRange reference = ref drawRanges.ElementAt(i);
			reference.drawOffset = num;
			num += reference.drawCount;
		}
		NativeArray<int> nativeArray = new NativeArray<int>(drawRanges.Length, Allocator.Temp);
		for (int j = 0; j < drawBatches.Length; j++)
		{
			ref DrawBatch reference2 = ref drawBatches.ElementAt(j);
			if (rangeHash.TryGetValue(reference2.key.range, out var item))
			{
				ref DrawRange reference3 = ref drawRanges.ElementAt(item);
				drawBatchIndices[reference3.drawOffset + nativeArray[item]] = j;
				nativeArray[item]++;
			}
		}
		int num2 = 0;
		for (int k = 0; k < drawBatchIndices.Length; k++)
		{
			int index = drawBatchIndices[k];
			ref DrawBatch reference4 = ref drawBatches.ElementAt(index);
			reference4.instanceOffset = num2;
			num2 += reference4.instanceCount;
		}
		nativeArray.Dispose();
	}
}
