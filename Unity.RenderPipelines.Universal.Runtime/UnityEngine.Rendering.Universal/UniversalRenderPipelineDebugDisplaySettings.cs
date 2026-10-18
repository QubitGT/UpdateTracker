using System;

namespace UnityEngine.Rendering.Universal;

public class UniversalRenderPipelineDebugDisplaySettings : DebugDisplaySettings<UniversalRenderPipelineDebugDisplaySettings>
{
	private DebugDisplaySettingsCommon commonSettings { get; set; }

	public DebugDisplaySettingsMaterial materialSettings { get; private set; }

	public DebugDisplaySettingsRendering renderingSettings { get; private set; }

	public DebugDisplaySettingsLighting lightingSettings { get; private set; }

	public DebugDisplaySettingsVolume volumeSettings { get; private set; }

	internal DebugDisplaySettingsStats<URPProfileId> displayStats { get; private set; }

	internal DebugDisplayGPUResidentDrawer gpuResidentDrawerSettings { get; private set; }

	public override bool IsPostProcessingAllowed
	{
		get
		{
			DebugPostProcessingMode postProcessingDebugMode = renderingSettings.postProcessingDebugMode;
			switch (postProcessingDebugMode)
			{
			case DebugPostProcessingMode.Disabled:
				return false;
			case DebugPostProcessingMode.Auto:
			{
				bool flag = true;
				{
					foreach (IDebugDisplaySettingsData setting in m_Settings)
					{
						flag &= setting.IsPostProcessingAllowed;
					}
					return flag;
				}
			}
			case DebugPostProcessingMode.Enabled:
				return true;
			default:
				throw new ArgumentOutOfRangeException("debugPostProcessingMode", $"Invalid post-processing state {postProcessingDebugMode}");
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		displayStats = Add(new DebugDisplaySettingsStats<URPProfileId>(new UniversalRenderPipelineDebugDisplayStats()));
		materialSettings = Add(new DebugDisplaySettingsMaterial());
		lightingSettings = Add(new DebugDisplaySettingsLighting());
		renderingSettings = Add(new DebugDisplaySettingsRendering());
		volumeSettings = Add(new DebugDisplaySettingsVolume());
		commonSettings = Add(new DebugDisplaySettingsCommon());
		gpuResidentDrawerSettings = Add(new DebugDisplayGPUResidentDrawer());
		Texture.streamingTextureDiscardUnusedMips = false;
	}

	internal void UpdateDisplayStats()
	{
		if (displayStats != null)
		{
			displayStats.debugDisplayStats.Update();
		}
	}

	internal void UpdateMaterials()
	{
		if (renderingSettings.mipInfoMode != 0)
		{
			Texture.SetStreamingTextureMaterialDebugProperties((renderingSettings.canAggregateData && renderingSettings.showInfoForAllSlots) ? (-1) : renderingSettings.mipDebugMaterialTextureSlot);
		}
	}
}
