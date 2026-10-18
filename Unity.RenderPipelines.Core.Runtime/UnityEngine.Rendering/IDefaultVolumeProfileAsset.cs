namespace UnityEngine.Rendering;

public interface IDefaultVolumeProfileAsset : IRenderPipelineGraphicsSettings
{
	VolumeProfile defaultVolumeProfile { get; set; }
}
