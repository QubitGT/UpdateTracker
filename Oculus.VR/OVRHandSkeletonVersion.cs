using UnityEngine;

public enum OVRHandSkeletonVersion
{
	[InspectorName(null)]
	Uninitialized = -1,
	[InspectorName("OVR Hand Skeleton")]
	OVR,
	[InspectorName("OpenXR Hand Skeleton")]
	OpenXR
}
