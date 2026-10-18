namespace UnityEngine.Rendering.Universal;

public static class CameraExtensions
{
	public static UniversalAdditionalCameraData GetUniversalAdditionalCameraData(this Camera camera)
	{
		GameObject gameObject = camera.gameObject;
		if (!gameObject.TryGetComponent<UniversalAdditionalCameraData>(out var component))
		{
			return gameObject.AddComponent<UniversalAdditionalCameraData>();
		}
		return component;
	}

	public static VolumeFrameworkUpdateMode GetVolumeFrameworkUpdateMode(this Camera camera)
	{
		return camera.GetUniversalAdditionalCameraData().volumeFrameworkUpdateMode;
	}

	public static void SetVolumeFrameworkUpdateMode(this Camera camera, VolumeFrameworkUpdateMode mode)
	{
		UniversalAdditionalCameraData universalAdditionalCameraData = camera.GetUniversalAdditionalCameraData();
		if (universalAdditionalCameraData.volumeFrameworkUpdateMode != mode)
		{
			bool requiresVolumeFrameworkUpdate = universalAdditionalCameraData.requiresVolumeFrameworkUpdate;
			universalAdditionalCameraData.volumeFrameworkUpdateMode = mode;
			if (requiresVolumeFrameworkUpdate && !universalAdditionalCameraData.requiresVolumeFrameworkUpdate)
			{
				camera.UpdateVolumeStack(universalAdditionalCameraData);
			}
		}
	}

	public static void UpdateVolumeStack(this Camera camera)
	{
		UniversalAdditionalCameraData universalAdditionalCameraData = camera.GetUniversalAdditionalCameraData();
		camera.UpdateVolumeStack(universalAdditionalCameraData);
	}

	public static void UpdateVolumeStack(this Camera camera, UniversalAdditionalCameraData cameraData)
	{
		if (!VolumeManager.instance.isInitialized)
		{
			Debug.LogError("UpdateVolumeStack must not be called before VolumeManager.instance.Initialize. If you tries calling this from Awake or Start, try instead to use the RenderPipelineManager.activeRenderPipelineCreated callback to be sure your render pipeline is fully initialized before calling this.");
		}
		else if (!cameraData.requiresVolumeFrameworkUpdate)
		{
			if (cameraData.volumeStack == null)
			{
				cameraData.GetOrCreateVolumeStack();
			}
			camera.GetVolumeLayerMaskAndTrigger(cameraData, out var layerMask, out var trigger);
			VolumeManager.instance.Update(cameraData.volumeStack, trigger, layerMask);
		}
	}

	public static void DestroyVolumeStack(this Camera camera)
	{
		UniversalAdditionalCameraData universalAdditionalCameraData = camera.GetUniversalAdditionalCameraData();
		camera.DestroyVolumeStack(universalAdditionalCameraData);
	}

	public static void DestroyVolumeStack(this Camera camera, UniversalAdditionalCameraData cameraData)
	{
		if (!(cameraData == null) && cameraData.volumeStack != null)
		{
			cameraData.volumeStack = null;
		}
	}

	internal static void GetVolumeLayerMaskAndTrigger(this Camera camera, UniversalAdditionalCameraData cameraData, out LayerMask layerMask, out Transform trigger)
	{
		layerMask = 1;
		trigger = camera.transform;
		if (cameraData != null)
		{
			layerMask = cameraData.volumeLayerMask;
			trigger = ((cameraData.volumeTrigger != null) ? cameraData.volumeTrigger : trigger);
		}
		else if (camera.cameraType == CameraType.SceneView)
		{
			Camera main = Camera.main;
			UniversalAdditionalCameraData component = null;
			if (main != null && main.TryGetComponent<UniversalAdditionalCameraData>(out component))
			{
				layerMask = component.volumeLayerMask;
			}
			trigger = ((component != null && component.volumeTrigger != null) ? component.volumeTrigger : trigger);
		}
	}
}
