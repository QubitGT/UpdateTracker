namespace Meta.XR;

internal enum DepthRaycastResult
{
	Success,
	HitPointOccluded,
	NotReady,
	RayOutsideOfDepthCameraFrustum,
	RayOccluded,
	NoHit
}
