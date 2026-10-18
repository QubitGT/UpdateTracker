namespace Meta.XR;

public enum EnvironmentRaycastHitStatus
{
	Hit,
	HitPointOccluded,
	NotReady,
	HitPointOutsideOfCameraFrustum,
	RayOccluded,
	NoHit,
	NotSupported
}
