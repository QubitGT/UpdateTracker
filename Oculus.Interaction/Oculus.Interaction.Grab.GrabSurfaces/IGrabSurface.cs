using System;
using UnityEngine;

namespace Oculus.Interaction.Grab.GrabSurfaces;

public interface IGrabSurface
{
	[Obsolete("Use CalculateBestPoseAtSurface with offset instead")]
	GrabPoseScore CalculateBestPoseAtSurface(in Pose targetPose, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo);

	GrabPoseScore CalculateBestPoseAtSurface(in Pose targetPose, in Pose offset, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo);

	bool CalculateBestPoseAtSurface(Ray targetRay, out Pose bestPose, Transform relativeTo);

	Pose MirrorPose(in Pose gripPose, Transform relativeTo);

	IGrabSurface CreateMirroredSurface(GameObject gameObject);

	IGrabSurface CreateDuplicatedSurface(GameObject gameObject);
}
