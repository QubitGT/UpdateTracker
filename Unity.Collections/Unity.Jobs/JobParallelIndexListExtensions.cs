using System;
using Unity.Collections;

namespace Unity.Jobs;

[Obsolete("'JobParallelIndexListExtensions' has been deprecated; Use 'IJobFilterExtensions' instead.", false)]
public static class JobParallelIndexListExtensions
{
	[Obsolete("The signature for 'ScheduleAppend' has changed. 'innerloopBatchCount' is no longer part of this API.", false)]
	public static JobHandle ScheduleAppend<T>(this T jobData, NativeList<int> indices, int arrayLength, int innerloopBatchCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobFilter
	{
		return jobData.ScheduleAppend(indices, arrayLength, dependsOn);
	}

	[Obsolete("The signature for 'ScheduleFilter' has changed. 'innerloopBatchCount' is no longer part of this API.")]
	public static JobHandle ScheduleFilter<T>(this T jobData, NativeList<int> indices, int innerloopBatchCount, JobHandle dependsOn = default(JobHandle)) where T : struct, IJobFilter
	{
		return jobData.ScheduleFilter(indices, dependsOn);
	}
}
