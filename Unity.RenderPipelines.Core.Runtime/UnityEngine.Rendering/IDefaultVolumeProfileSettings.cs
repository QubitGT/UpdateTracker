namespace UnityEngine.Rendering;

public interface IDefaultVolumeProfileSettings : IRenderPipelineGraphicsSettings
{
	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;

	VolumeProfile volumeProfile { get; set; }
}
