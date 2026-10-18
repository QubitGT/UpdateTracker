using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;

namespace Unity.XR.OpenVR;

internal static class InputLayoutLoader
{
	static InputLayoutLoader()
	{
		RegisterInputLayouts();
	}

	public static void RegisterInputLayouts()
	{
		InputSystem.RegisterLayout<XRHMD>("OpenVRHMD", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("^(OpenVR Headset)|^(Vive Pro)"));
		InputSystem.RegisterLayout<XRController>("OpenVRControllerWMR", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("^(OpenVR Controller\\(WindowsMR)"));
		InputSystem.RegisterLayout<XRController>("ViveWand", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("HTC").WithProduct("^(OpenVR Controller\\(((Vive Controller)|(VIVE Controller)))"));
		InputSystem.RegisterLayout<XRController>("OpenVRViveCosmosController", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("HTC").WithProduct("^(OpenVR Controller\\(((VIVE Cosmos Controller)|(Vive Cosmos Controller)|(vive_cosmos_controller)))"));
		InputSystem.RegisterLayout<XRController>("OpenVRControllerIndex", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("Valve").WithProduct("^(OpenVR Controller\\(Knuckles)"));
		InputSystem.RegisterLayout<XRController>("OpenVROculusTouchController", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("Oculus").WithProduct("^(OpenVR Controller\\(Oculus)"));
		InputSystem.RegisterLayout<XRController>("HandedViveTracker", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("HTC").WithProduct("^(OpenVR Controller\\(((Vive Tracker)|(VIVE Tracker)).+ - ((Left)|(Right)))"));
		InputSystem.RegisterLayout<XRController>("ViveTracker", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("HTC").WithProduct("^(OpenVR Controller\\(((Vive Tracker)|(VIVE Tracker)).+\\)(?! - Left| - Right))"));
		InputSystem.RegisterLayout<XRController>("ViveTracker", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("HTC").WithProduct("^(OpenVR Tracked Device\\(((Vive Tracker)|(VIVE Tracker)).+\\)(?! - Left| - Right))"));
		InputSystem.RegisterLayout<XRController>("LogitechStylus", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("Logitech").WithProduct("(OpenVR Controller\\(.+stylus)"));
		InputSystem.RegisterLayout<TrackedDevice>("ViveLighthouse", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("HTC").WithProduct("^(OpenVR Tracking Reference\\()"));
		InputSystem.RegisterLayout<TrackedDevice>("ValveLighthouse", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithManufacturer("Valve Corporation").WithProduct("^(OpenVR Tracking Reference\\()"));
	}
}
