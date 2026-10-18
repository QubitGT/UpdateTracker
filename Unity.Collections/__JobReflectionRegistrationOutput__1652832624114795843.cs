using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

[DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__1652832624114795843
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobExtensions.EarlyJobInit<CollectionHelper.DummyJob>();
			IJobExtensions.EarlyJobInit<NativeBitArrayDisposeJob>();
			IJobExtensions.EarlyJobInit<NativeHashMapDisposeJob>();
			IJobExtensions.EarlyJobInit<NativeListDisposeJob>();
			IJobExtensions.EarlyJobInit<NativeQueueDisposeJob>();
			IJobExtensions.EarlyJobInit<NativeReferenceDisposeJob>();
			IJobExtensions.EarlyJobInit<NativeRingQueueDisposeJob>();
			IJobExtensions.EarlyJobInit<NativeStream.ConstructJobList>();
			IJobExtensions.EarlyJobInit<NativeStream.ConstructJob>();
			IJobExtensions.EarlyJobInit<NativeStreamDisposeJob>();
			IJobExtensions.EarlyJobInit<NativeTextDisposeJob>();
			IJobExtensions.EarlyJobInit<UnsafeQueueDisposeJob>();
			IJobExtensions.EarlyJobInit<UnsafeDisposeJob>();
			IJobExtensions.EarlyJobInit<UnsafeParallelHashMapDataDisposeJob>();
			IJobExtensions.EarlyJobInit<UnsafeParallelHashMapDisposeJob>();
			IJobExtensions.EarlyJobInit<UnsafeStream.DisposeJob>();
			IJobExtensions.EarlyJobInit<UnsafeStream.ConstructJobList>();
			IJobExtensions.EarlyJobInit<UnsafeStream.ConstructJob>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
