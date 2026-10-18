namespace UnityEngine.XR.Interaction.Toolkit;

public static class XRInteractionUpdateOrder
{
	public enum UpdatePhase
	{
		Fixed,
		Dynamic,
		Late,
		OnBeforeRender
	}

	public const int k_XRInputDeviceButtonReader = -31000;

	public const int k_ScreenSpaceRayPoseDriver = -31000;

	public const int k_ScreenSpaceSelectInput = -30050;

	public const int k_ControllerRecorder = -30000;

	public const int k_SimulatedDeviceLifecycleManager = -29995;

	public const int k_SimulatedHandExpressionManager = -29994;

	public const int k_InteractionSimulator = -29991;

	public const int k_DeviceSimulator = -29991;

	public const int k_Controllers = -29990;

	public const int k_TransformStabilizer = -29985;

	public const int k_GazeAssistance = -29980;

	public const int k_LocomotionProviders = -210;

	public const int k_TwoHandedGrabMoveProviders = -209;

	public const int k_GravityProvider = -207;

	public const int k_XRBodyTransformer = -205;

	public const int k_UIInputModule = -200;

	public const int k_XRUIToolkitManager = -200;

	public const int k_InteractionManager = -105;

	public const int k_InteractionGroups = -100;

	public const int k_Interactors = -99;

	public const int k_InteractableSnapVolume = -99;

	public const int k_Interactables = -98;

	public const int k_LineVisual = 100;

	public const int k_BeforeRenderGazeAssistance = 95;

	public const int k_BeforeRenderOrder = 100;

	public const int k_BeforeRenderLineVisual = 101;
}
