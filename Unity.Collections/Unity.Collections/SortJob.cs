using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(NativeSortExtension.DefaultComparer<int>)
})]
public struct SortJob<T, U> where T : unmanaged where U : IComparer<T>
{
	[BurstCompile]
	public struct SegmentSort : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe T* Data;

		internal U Comp;

		internal int Length;

		internal int SegmentWidth;

		public unsafe void Execute(int index)
		{
			int num = index * SegmentWidth;
			int length = ((Length - num < SegmentWidth) ? (Length - num) : SegmentWidth);
			NativeSortExtension.Sort(Data + num, length, Comp);
		}
	}

	[BurstCompile]
	public struct SegmentSortMerge : IJob
	{
		[NativeDisableUnsafePtrRestriction]
		internal unsafe T* Data;

		internal U Comp;

		internal int Length;

		internal int SegmentWidth;

		public unsafe void Execute()
		{
			int num = (Length + (SegmentWidth - 1)) / SegmentWidth;
			int* ptr = stackalloc int[num];
			T* ptr2 = (T*)Memory.Unmanaged.Allocate(UnsafeUtility.SizeOf<T>() * Length, 16, Allocator.Temp);
			for (int i = 0; i < Length; i++)
			{
				int num2 = -1;
				T val = default(T);
				for (int j = 0; j < num; j++)
				{
					int num3 = j * SegmentWidth;
					int num4 = ptr[j];
					int num5 = ((Length - num3 < SegmentWidth) ? (Length - num3) : SegmentWidth);
					if (num4 != num5)
					{
						T val2 = Data[num3 + num4];
						if (num2 == -1 || Comp.Compare(val2, val) <= 0)
						{
							val = val2;
							num2 = j;
						}
					}
				}
				ptr[num2]++;
				ptr2[i] = val;
			}
			UnsafeUtility.MemCpy(Data, ptr2, UnsafeUtility.SizeOf<T>() * Length);
		}
	}

	public unsafe T* Data;

	public U Comp;

	public int Length;

	public unsafe JobHandle Schedule(JobHandle inputDeps = default(JobHandle))
	{
		if (Length == 0)
		{
			return inputDeps;
		}
		int num = (Length + 1023) / 1024;
		int threadIndexCount = JobsUtility.ThreadIndexCount;
		int num2 = math.max(1, threadIndexCount);
		int innerloopBatchCount = num / num2;
		SegmentSort jobData = default(SegmentSort);
		jobData.Data = Data;
		jobData.Comp = Comp;
		jobData.Length = Length;
		jobData.SegmentWidth = 1024;
		JobHandle dependsOn = IJobParallelForExtensions.Schedule(jobData, num, innerloopBatchCount, inputDeps);
		SegmentSortMerge jobData2 = default(SegmentSortMerge);
		jobData2.Data = Data;
		jobData2.Comp = Comp;
		jobData2.Length = Length;
		jobData2.SegmentWidth = 1024;
		return jobData2.Schedule(dependsOn);
	}
}
