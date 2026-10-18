namespace UnityEngine.XR.OpenXR.Features.Interactions;

public static class EyeTrackingUsages
{
	public static InputFeatureUsage<Vector3> gazePosition = new InputFeatureUsage<Vector3>("gazePosition");

	public static InputFeatureUsage<Quaternion> gazeRotation = new InputFeatureUsage<Quaternion>("gazeRotation");
}
