using System;

namespace UnityEngine.Rendering.Universal;

[Obsolete("This is not longer supported Please use DebugDisplaySettingsVolume. #from(6000.2)", false)]
public class UniversalRenderPipelineVolumeDebugSettings : VolumeDebugSettings<UniversalAdditionalCameraData>
{
	public override VolumeStack selectedCameraVolumeStack
	{
		get
		{
			if (base.selectedCamera == null)
			{
				return null;
			}
			UniversalAdditionalCameraData component = base.selectedCamera.GetComponent<UniversalAdditionalCameraData>();
			if (component == null)
			{
				return null;
			}
			VolumeStack volumeStack = component.volumeStack;
			if (volumeStack != null)
			{
				return volumeStack;
			}
			return VolumeManager.instance.stack;
		}
	}

	public override LayerMask selectedCameraLayerMask
	{
		get
		{
			if (base.selectedCamera != null && base.selectedCamera.TryGetComponent<UniversalAdditionalCameraData>(out var component))
			{
				return component.volumeLayerMask;
			}
			return 1;
		}
	}

	public override Vector3 selectedCameraPosition
	{
		get
		{
			if (!(base.selectedCamera != null))
			{
				return Vector3.zero;
			}
			return base.selectedCamera.transform.position;
		}
	}

	[Obsolete("This property is obsolete and kept only for not breaking user code. VolumeDebugSettings will use current pipeline when it needs to gather volume component types and paths. #from(23.2)", false)]
	public override Type targetRenderPipeline => typeof(UniversalRenderPipeline);
}
