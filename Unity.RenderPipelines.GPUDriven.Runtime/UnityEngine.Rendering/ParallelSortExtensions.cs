using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UnityEngine.Rendering;

internal static class ParallelSortExtensions
{
	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal struct RadixSortBucketCountJob : IJobFor
	{
		[ReadOnly]
		public int radix;

		[ReadOnly]
		public int jobsCount;

		[ReadOnly]
		public int batchSize;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> array;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> buckets;

		public void Execute(int index)
		{
			int num = index * batchSize;
			int num2 = math.min(num + batchSize, array.Length);
			int num3 = index * 256;
			for (int i = num; i < num2; i++)
			{
				int num4 = (array[i] >> radix * 8) & 0xFF;
				buckets[num3 + num4]++;
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal struct RadixSortBatchPrefixSumJob : IJobFor
	{
		[ReadOnly]
		public int radix;

		[ReadOnly]
		public int jobsCount;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> array;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> counter;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> indicesSum;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> buckets;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> indices;

		private unsafe static int AtomicIncrement(NativeArray<int> counter)
		{
			return Interlocked.Increment(ref UnsafeUtility.AsRef<int>(counter.GetUnsafePtr()));
		}

		private int JobIndexPrefixSum(int sum, int i)
		{
			for (int j = 0; j < jobsCount; j++)
			{
				int index = i + j * 256;
				indices[index] = sum;
				sum += buckets[index];
				buckets[index] = 0;
			}
			return sum;
		}

		public void Execute(int index)
		{
			int num = index * 16;
			int num2 = num + 16;
			int num3 = 0;
			for (int i = num; i < num2; i++)
			{
				num3 = JobIndexPrefixSum(num3, i);
			}
			indicesSum[index] = num3;
			if (AtomicIncrement(counter) != 16)
			{
				return;
			}
			int num4 = 0;
			if (radix < 3)
			{
				for (int j = 0; j < 16; j++)
				{
					int num5 = indicesSum[j];
					indicesSum[j] = num4;
					num4 += num5;
				}
			}
			else
			{
				for (int k = 8; k < 16; k++)
				{
					int num6 = indicesSum[k];
					indicesSum[k] = num4;
					num4 += num6;
				}
				for (int l = 0; l < 8; l++)
				{
					int num7 = indicesSum[l];
					indicesSum[l] = num4;
					num4 += num7;
				}
			}
			counter[0] = 0;
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal struct RadixSortPrefixSumJob : IJobFor
	{
		[ReadOnly]
		public int jobsCount;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> indicesSum;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> indices;

		public void Execute(int index)
		{
			int num = index * 16;
			int num2 = num + 16;
			int num3 = indicesSum[index];
			for (int i = 0; i < jobsCount; i++)
			{
				for (int j = num; j < num2; j++)
				{
					indices[i * 256 + j] += num3;
				}
			}
		}
	}

	[BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
	internal struct RadixSortBucketSortJob : IJobFor
	{
		[ReadOnly]
		public int radix;

		[ReadOnly]
		public int batchSize;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> array;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> indices;

		[NativeDisableContainerSafetyRestriction]
		[NoAlias]
		public NativeArray<int> arraySorted;

		public void Execute(int index)
		{
			int num = index * batchSize;
			int num2 = math.min(num + batchSize, array.Length);
			int num3 = index * 256;
			for (int i = num; i < num2; i++)
			{
				int num4 = array[i];
				int num5 = (num4 >> radix * 8) & 0xFF;
				int index2 = indices[num3 + num5]++;
				arraySorted[index2] = num4;
			}
		}
	}

	private const int kMinRadixSortArraySize = 2048;

	private const int kMinRadixSortBatchSize = 256;

	internal static JobHandle ParallelSort(this NativeArray<int> array)
	{
		if (array.Length <= 1)
		{
			return default(JobHandle);
		}
		JobHandle jobHandle = default(JobHandle);
		if (array.Length >= 2048)
		{
			int num = Mathf.Max(JobsUtility.JobWorkerCount + 1, 1);
			int num2 = Mathf.Max(256, Mathf.CeilToInt((float)array.Length / (float)num));
			int num3 = Mathf.CeilToInt((float)array.Length / (float)num2);
			NativeArray<int> nativeArray = new NativeArray<int>(array.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> counter = new NativeArray<int>(1, Allocator.TempJob);
			NativeArray<int> buckets = new NativeArray<int>(num3 * 256, Allocator.TempJob);
			NativeArray<int> indices = new NativeArray<int>(num3 * 256, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> indicesSum = new NativeArray<int>(16, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			NativeArray<int> a2 = array;
			NativeArray<int> b2 = nativeArray;
			for (int i = 0; i < 4; i++)
			{
				RadixSortBucketCountJob radixSortBucketCountJob = default(RadixSortBucketCountJob);
				radixSortBucketCountJob.radix = i;
				radixSortBucketCountJob.jobsCount = num3;
				radixSortBucketCountJob.batchSize = num2;
				radixSortBucketCountJob.buckets = buckets;
				radixSortBucketCountJob.array = a2;
				RadixSortBucketCountJob jobData = radixSortBucketCountJob;
				RadixSortBatchPrefixSumJob radixSortBatchPrefixSumJob = default(RadixSortBatchPrefixSumJob);
				radixSortBatchPrefixSumJob.radix = i;
				radixSortBatchPrefixSumJob.jobsCount = num3;
				radixSortBatchPrefixSumJob.array = a2;
				radixSortBatchPrefixSumJob.counter = counter;
				radixSortBatchPrefixSumJob.buckets = buckets;
				radixSortBatchPrefixSumJob.indices = indices;
				radixSortBatchPrefixSumJob.indicesSum = indicesSum;
				RadixSortBatchPrefixSumJob jobData2 = radixSortBatchPrefixSumJob;
				RadixSortPrefixSumJob radixSortPrefixSumJob = default(RadixSortPrefixSumJob);
				radixSortPrefixSumJob.jobsCount = num3;
				radixSortPrefixSumJob.indices = indices;
				radixSortPrefixSumJob.indicesSum = indicesSum;
				RadixSortPrefixSumJob jobData3 = radixSortPrefixSumJob;
				RadixSortBucketSortJob radixSortBucketSortJob = default(RadixSortBucketSortJob);
				radixSortBucketSortJob.radix = i;
				radixSortBucketSortJob.batchSize = num2;
				radixSortBucketSortJob.indices = indices;
				radixSortBucketSortJob.array = a2;
				radixSortBucketSortJob.arraySorted = b2;
				RadixSortBucketSortJob jobData4 = radixSortBucketSortJob;
				jobHandle = IJobForExtensions.ScheduleParallel(jobData, num3, 1, jobHandle);
				jobHandle = IJobForExtensions.ScheduleParallel(jobData2, 16, 1, jobHandle);
				jobHandle = IJobForExtensions.ScheduleParallel(jobData3, 16, 1, jobHandle);
				jobHandle = IJobForExtensions.ScheduleParallel(jobData4, num3, 1, jobHandle);
				JobHandle.ScheduleBatchedJobs();
				Swap(ref a2, ref b2);
			}
			nativeArray.Dispose(jobHandle);
			counter.Dispose(jobHandle);
			buckets.Dispose(jobHandle);
			indices.Dispose(jobHandle);
			indicesSum.Dispose(jobHandle);
		}
		else
		{
			jobHandle = array.SortJob().Schedule();
		}
		return jobHandle;
		static void Swap(ref NativeArray<int> a, ref NativeArray<int> b)
		{
			NativeArray<int> nativeArray2 = a;
			a = b;
			b = nativeArray2;
		}
	}
}
