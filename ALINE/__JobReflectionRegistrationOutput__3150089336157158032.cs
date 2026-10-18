using System;
using Drawing;
using Drawing.Examples;
using Unity.Jobs;
using UnityEngine;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__3150089336157158032
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobExtensions.EarlyJobInit<GeometryBuilderJob>();
			IJobExtensions.EarlyJobInit<PersistentFilterJob>();
			IJobExtensions.EarlyJobInit<StreamSplitter>();
			IJobExtensions.EarlyJobInit<BurstExample.DrawingJob>();
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
