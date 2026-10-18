using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility(RequiredUnityDefine = "UNITY_2020_2_OR_NEWER", GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(NativeSortExtension.DefaultComparer<int>)
})]
public struct SortJobDefer<T, U> where T : unmanaged where U : IComparer<T>
{
	[BurstCompile]
	public struct SegmentSort : IJobParallelForDefer
	{
		[ReadOnly]
		internal NativeList<T> DataRO;

		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeList<T>* Data;

		internal U Comp;

		internal int SegmentWidth;

		public unsafe void Execute(int index)
		{
			int num = index * SegmentWidth;
			int length = ((Data->Length - num < SegmentWidth) ? (Data->Length - num) : SegmentWidth);
			NativeSortExtension.Sort(Data->Ptr + num, length, Comp);
		}
	}

	[BurstCompile]
	public struct SegmentSortMerge : IJob
	{
		[NativeDisableUnsafePtrRestriction]
		internal NativeList<T> Data;

		internal U Comp;

		internal int SegmentWidth;

		public unsafe void Execute()
		{
			int length = Data.Length;
			T* unsafePtr = Data.GetUnsafePtr();
			int num = (length + (SegmentWidth - 1)) / SegmentWidth;
			int* ptr = stackalloc int[num];
			T* ptr2 = (T*)Memory.Unmanaged.Allocate(UnsafeUtility.SizeOf<T>() * length, 16, Allocator.Temp);
			for (int i = 0; i < length; i++)
			{
				int num2 = -1;
				T val = default(T);
				for (int j = 0; j < num; j++)
				{
					int num3 = j * SegmentWidth;
					int num4 = ptr[j];
					int num5 = ((length - num3 < SegmentWidth) ? (length - num3) : SegmentWidth);
					if (num4 != num5)
					{
						T val2 = unsafePtr[num3 + num4];
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
			UnsafeUtility.MemCpy(unsafePtr, ptr2, UnsafeUtility.SizeOf<T>() * length);
		}
	}

	public NativeList<T> Data;

	public U Comp;

	public unsafe JobHandle Schedule(JobHandle inputDeps = default(JobHandle))
	{
		SegmentSort segmentSort = default(SegmentSort);
		segmentSort.DataRO = Data;
		segmentSort.Data = Data.m_ListData;
		segmentSort.Comp = Comp;
		segmentSort.SegmentWidth = 1024;
		SegmentSort jobData = segmentSort;
		JobHandle dependsOn = IJobParallelForDeferExtensions.ScheduleByRef(ref jobData, Data, 1024, inputDeps);
		SegmentSortMerge jobData2 = default(SegmentSortMerge);
		jobData2.Data = Data;
		jobData2.Comp = Comp;
		jobData2.SegmentWidth = 1024;
		return jobData2.Schedule(dependsOn);
	}
}
