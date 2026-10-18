using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Jobs;

public static class IJobFilterExtensions
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct JobFilterProducer<T> where T : struct, IJobFilter
	{
		public struct JobWrapper
		{
			[NativeDisableParallelForRestriction]
			public NativeList<int> outputIndices;

			public int appendCount;

			public T JobData;
		}

		public delegate void ExecuteJobFunction(ref JobWrapper jobWrapper, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

		internal static readonly SharedStatic<IntPtr> jobReflectionData = SharedStatic<IntPtr>.GetOrCreate<JobFilterProducer<T>>();

		[BurstDiscard]
		internal static void Initialize()
		{
			if (jobReflectionData.Data == IntPtr.Zero)
			{
				jobReflectionData.Data = JobsUtility.CreateJobReflectionData(typeof(JobWrapper), typeof(T), new ExecuteJobFunction(Execute));
			}
		}

		public static void Execute(ref JobWrapper jobWrapper, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
		{
			if (jobWrapper.appendCount == -1)
			{
				ExecuteFilter(ref jobWrapper, bufferRangePatchData);
			}
			else
			{
				ExecuteAppend(ref jobWrapper, bufferRangePatchData);
			}
		}

		public unsafe static void ExecuteAppend(ref JobWrapper jobWrapper, IntPtr bufferRangePatchData)
		{
			int length = jobWrapper.outputIndices.Length;
			jobWrapper.outputIndices.Capacity = math.max(jobWrapper.appendCount + length, jobWrapper.outputIndices.Capacity);
			int* unsafePtr = jobWrapper.outputIndices.GetUnsafePtr();
			int num = length;
			for (int i = 0; i != jobWrapper.appendCount; i++)
			{
				if (jobWrapper.JobData.Execute(i))
				{
					unsafePtr[num] = i;
					num++;
				}
			}
			jobWrapper.outputIndices.ResizeUninitialized(num);
		}

		public unsafe static void ExecuteFilter(ref JobWrapper jobWrapper, IntPtr bufferRangePatchData)
		{
			int* unsafePtr = jobWrapper.outputIndices.GetUnsafePtr();
			int length = jobWrapper.outputIndices.Length;
			int num = 0;
			for (int i = 0; i != length; i++)
			{
				int num2 = unsafePtr[i];
				if (jobWrapper.JobData.Execute(num2))
				{
					unsafePtr[num] = num2;
					num++;
				}
			}
			jobWrapper.outputIndices.ResizeUninitialized(num);
		}
	}

	public static void EarlyJobInit<T>() where T : struct, IJobFilter
	{
		JobFilterProducer<T>.Initialize();
	}

	private static IntPtr GetReflectionData<T>() where T : struct, IJobFilter
	{
		JobFilterProducer<T>.Initialize();
		return JobFilterProducer<T>.jobReflectionData.Data;
	}

	public static JobHandle ScheduleAppend<T>(this T jobData, NativeList<int> indices, int arrayLength, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobFilter
	{
		return ScheduleAppendByRef(ref jobData, indices, arrayLength, dependsOn);
	}

	public static JobHandle ScheduleFilter<T>(this T jobData, NativeList<int> indices, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobFilter
	{
		return ScheduleFilterByRef(ref jobData, indices, dependsOn);
	}

	public static void RunAppend<T>(this T jobData, NativeList<int> indices, int arrayLength) where T : struct, IJobFilter
	{
		RunAppendByRef(ref jobData, indices, arrayLength);
	}

	public static void RunFilter<T>(this T jobData, NativeList<int> indices) where T : struct, IJobFilter
	{
		RunFilterByRef(ref jobData, indices);
	}

	public unsafe static JobHandle ScheduleAppendByRef<T>(this ref T jobData, NativeList<int> indices, int arrayLength, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobFilter
	{
		JobFilterProducer<T>.JobWrapper jobWrapper = default(JobFilterProducer<T>.JobWrapper);
		jobWrapper.JobData = jobData;
		jobWrapper.outputIndices = indices;
		jobWrapper.appendCount = arrayLength;
		JobFilterProducer<T>.JobWrapper output = jobWrapper;
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), GetReflectionData<T>(), dependsOn, ScheduleMode.Single);
		return JobsUtility.Schedule(ref parameters);
	}

	public unsafe static JobHandle ScheduleFilterByRef<T>(this ref T jobData, NativeList<int> indices, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobFilter
	{
		JobFilterProducer<T>.JobWrapper jobWrapper = default(JobFilterProducer<T>.JobWrapper);
		jobWrapper.JobData = jobData;
		jobWrapper.outputIndices = indices;
		jobWrapper.appendCount = -1;
		JobFilterProducer<T>.JobWrapper output = jobWrapper;
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), GetReflectionData<T>(), dependsOn, ScheduleMode.Single);
		return JobsUtility.Schedule(ref parameters);
	}

	public unsafe static void RunAppendByRef<T>(this ref T jobData, NativeList<int> indices, int arrayLength) where T : struct, IJobFilter
	{
		JobFilterProducer<T>.JobWrapper jobWrapper = default(JobFilterProducer<T>.JobWrapper);
		jobWrapper.JobData = jobData;
		jobWrapper.outputIndices = indices;
		jobWrapper.appendCount = arrayLength;
		JobFilterProducer<T>.JobWrapper output = jobWrapper;
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), GetReflectionData<T>(), default(JobHandle), ScheduleMode.Run);
		JobsUtility.Schedule(ref parameters);
	}

	public unsafe static void RunFilterByRef<T>(this ref T jobData, NativeList<int> indices) where T : struct, IJobFilter
	{
		JobFilterProducer<T>.JobWrapper jobWrapper = default(JobFilterProducer<T>.JobWrapper);
		jobWrapper.JobData = jobData;
		jobWrapper.outputIndices = indices;
		jobWrapper.appendCount = -1;
		JobFilterProducer<T>.JobWrapper output = jobWrapper;
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref output), GetReflectionData<T>(), default(JobHandle), ScheduleMode.Run);
		JobsUtility.Schedule(ref parameters);
	}
}
