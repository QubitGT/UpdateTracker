using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

namespace Meta.XR.BuildingBlocks;

internal static class Telemetry
{
	public static OVRTelemetryMarker AddBlockInfo(this OVRTelemetryMarker marker, BuildingBlock block)
	{
		return marker.AddAnnotation("BlockId", block.BlockId).AddAnnotation("InstanceId", block.InstanceId).AddAnnotation("BlockName", block.gameObject.name)
			.AddAnnotation("Version", block.Version.ToString())
			.AddBlockVariantInfo(block);
	}

	private static OVRTelemetryMarker AddBlockVariantInfo(this OVRTelemetryMarker marker, BuildingBlock block)
	{
		if (block.InstallationRoutineCheckpoint == null || string.IsNullOrEmpty(block.InstallationRoutineCheckpoint.InstallationRoutineId))
		{
			return marker;
		}
		return marker.AddAnnotation("InstallationRoutineId", block.InstallationRoutineCheckpoint.InstallationRoutineId).AddInstallationRoutineInfo(block.InstallationRoutineCheckpoint);
	}

	private static OVRTelemetryMarker AddInstallationRoutineInfo(this OVRTelemetryMarker marker, InstallationRoutineCheckpoint checkpoint)
	{
		if (checkpoint == null)
		{
			return marker;
		}
		List<string> list;
		using (new OVRObjectPool.ListScope<string>(out list))
		{
			foreach (VariantCheckpoint installationVariant in checkpoint.InstallationVariants)
			{
				if (installationVariant != null)
				{
					list.Add(installationVariant.MemberName + ":" + installationVariant.Value);
				}
			}
			if (list.Count > 0)
			{
				marker.AddAnnotation("InstallationRoutineData", string.Join(',', list));
			}
		}
		return marker;
	}

	public static OVRTelemetryMarker AddSceneInfo(this OVRTelemetryMarker marker, Scene scene)
	{
		long num = 0L;
		if (File.Exists(scene.path))
		{
			num = new FileInfo(scene.path).Length;
		}
		return marker.AddAnnotation("SceneSizeInB", num.ToString());
	}
}
