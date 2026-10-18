using UnityEngine;

namespace Meta.XR.ImmersiveDebugger;

public static class CustomIntegrationConfig
{
	public delegate Camera GetCameraDelegate();

	public delegate Transform GetLeftControllerTransformDelegate();

	public delegate Transform GetRightControllerTransformDelegate();

	public static event GetCameraDelegate GetCameraHandler;

	public static void SetupAllConfig(ICustomIntegrationConfig customConfig)
	{
		GetCameraHandler += customConfig.GetCamera;
	}

	public static void ClearAllConfig(ICustomIntegrationConfig customConfig)
	{
		GetCameraHandler -= customConfig.GetCamera;
	}

	public static Camera GetCamera()
	{
		return CustomIntegrationConfig.GetCameraHandler?.Invoke();
	}
}
