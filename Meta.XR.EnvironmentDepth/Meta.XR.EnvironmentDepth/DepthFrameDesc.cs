using UnityEngine;

namespace Meta.XR.EnvironmentDepth;

internal struct DepthFrameDesc
{
	internal Vector3 createPoseLocation;

	internal Quaternion createPoseRotation;

	internal float fovLeftAngleTangent;

	internal float fovRightAngleTangent;

	internal float fovTopAngleTangent;

	internal float fovDownAngleTangent;

	internal float nearZ;

	internal float farZ;
}
