using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Jobs;

public static class IJobParallelForBatchExtensions
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct JobParallelForBatchProducer<T> where T : struct, IJobParallelForBatch
	{
		internal delegate void ExecuteJobFunction(ref T jobData, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex);

		internal static readonly SharedStatic<IntPtr> jobReflectionData = SharedStatic<IntPtr>.GetOrCreate<JobParallelForBatchProducer<T>>();

		[BurstDiscard]
		internal static void Initialize()
		{
			if (jobReflectionData.Data == IntPtr.Zero)
			{
				jobReflectionData.Data = JobsUtility.CreateJobReflectionData(typeof(T), new ExecuteJobFunction(Execute));
			}
		}

		public static void Execute(ref T jobData, IntPtr additionalPtr, IntPtr bufferRangePatchData, ref JobRanges ranges, int jobIndex)
		{
			int beginIndex;
			int endIndex;
			while (JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out beginIndex, out endIndex))
			{
				jobData.Execute(beginIndex, endIndex - beginIndex);
			}
		}
	}

	public static void EarlyJobInit<T>() where T : struct, IJobParallelForBatch
	{
		JobParallelForBatchProducer<T>.Initialize();
	}

	private static IntPtr GetReflectionData<T>() where T : struct, IJobParallelForBatch
	{
		JobParallelForBatchProducer<T>.Initialize();
		return JobParallelForBatchProducer<T>.jobReflectionData.Data;
	}

	public unsafe static JobHandle Schedule<T>(this T jobData, int arrayLength, int indicesPerJobCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForBatch
	{
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), dependsOn, ScheduleMode.Single);
		return JobsUtility.ScheduleParallelFor(ref parameters, arrayLength, indicesPerJobCount);
	}

	public unsafe static JobHandle ScheduleByRef<T>(this ref T jobData, int arrayLength, int indicesPerJobCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForBatch
	{
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), dependsOn, ScheduleMode.Single);
		return JobsUtility.ScheduleParallelFor(ref parameters, arrayLength, indicesPerJobCount);
	}

	public unsafe static JobHandle ScheduleParallel<T>(this T jobData, int arrayLength, int indicesPerJobCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForBatch
	{
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), dependsOn, ScheduleMode.Batched);
		return JobsUtility.ScheduleParallelFor(ref parameters, arrayLength, indicesPerJobCount);
	}

	public unsafe static JobHandle ScheduleParallelByRef<T>(this ref T jobData, int arrayLength, int indicesPerJobCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForBatch
	{
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), dependsOn, ScheduleMode.Batched);
		return JobsUtility.ScheduleParallelFor(ref parameters, arrayLength, indicesPerJobCount);
	}

	public static JobHandle ScheduleBatch<T>(this T jobData, int arrayLength, int indicesPerJobCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForBatch
	{
		return ScheduleParallel(jobData, arrayLength, indicesPerJobCount, dependsOn);
	}

	public static JobHandle ScheduleBatchByRef<T>(this ref T jobData, int arrayLength, int indicesPerJobCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobParallelForBatch
	{
		return ScheduleParallelByRef(ref jobData, arrayLength, indicesPerJobCount, dependsOn);
	}

	public unsafe static void Run<T>(this T jobData, int arrayLength, int indicesPerJobCount) where T : struct, IJobParallelForBatch
	{
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), default(JobHandle), ScheduleMode.Run);
		JobsUtility.ScheduleParallelFor(ref parameters, arrayLength, arrayLength);
	}

	public unsafe static void RunByRef<T>(this ref T jobData, int arrayLength, int indicesPerJobCount) where T : struct, IJobParallelForBatch
	{
		JobsUtility.JobScheduleParameters parameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData), GetReflectionData<T>(), default(JobHandle), ScheduleMode.Run);
		JobsUtility.ScheduleParallelFor(ref parameters, arrayLength, arrayLength);
	}

	public static void RunBatch<T>(this T jobData, int arrayLength) where T : struct, IJobParallelForBatch
	{
		jobData.Run(arrayLength, arrayLength);
	}

	public static void RunBatchByRef<T>(this ref T jobData, int arrayLength) where T : struct, IJobParallelForBatch
	{
		RunByRef(ref jobData, arrayLength, arrayLength);
	}
}
