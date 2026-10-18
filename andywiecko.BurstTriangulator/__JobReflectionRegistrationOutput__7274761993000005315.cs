using System;
using Unity.Jobs;
using UnityEngine;
using andywiecko.BurstTriangulator;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__7274761993000005315
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobExtensions.EarlyJobInit<Triangulator.ValidateInputPositionsJob>();
			IJobExtensions.EarlyJobInit<Triangulator.PCATransformationJob>();
			IJobExtensions.EarlyJobInit<Triangulator.PCATransformationHolesJob>();
			IJobParallelForDeferExtensions.EarlyJobInit<Triangulator.PCAInverseTransformationJob>();
			IJobExtensions.EarlyJobInit<Triangulator.InitialLocalTransformationJob>();
			IJobExtensions.EarlyJobInit<Triangulator.CalculateLocalHoleSeedsJob>();
			IJobParallelForDeferExtensions.EarlyJobInit<Triangulator.CalculateLocalPositionsJob>();
			IJobParallelForDeferExtensions.EarlyJobInit<Triangulator.LocalToWorldTransformationJob>();
			IJobExtensions.EarlyJobInit<Triangulator.ClearDataJob>();
			IJobExtensions.EarlyJobInit<Triangulator.DelaunayTriangulationJob>();
			IJobExtensions.EarlyJobInit<Triangulator.ValidateInputConstraintEdges>();
			IJobExtensions.EarlyJobInit<Triangulator.ConstrainEdgesJob>();
			IJobExtensions.EarlyJobInit<Triangulator.RefineMeshJob>();
			IJobExtensions.EarlyJobInit<Triangulator.PlantingSeedsJob<Triangulator.PlantBoundary>>();
			IJobExtensions.EarlyJobInit<Triangulator.PlantingSeedsJob<Triangulator.PlantBoundaryAndHoles>>();
			IJobExtensions.EarlyJobInit<Triangulator.PlantingSeedsJob<Triangulator.PlantHoles>>();
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
