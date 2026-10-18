using UnityEngine;

namespace Oculus.Interaction.Grab;

public static class GrabPoseHelper
{
	public delegate Pose PoseCalculator(in Pose desiredPose, Transform relativeTo);

	public static GrabPoseScore CalculateBestPoseAtSurface(in Pose desiredPose, in Pose offset, out Pose bestPose, in PoseMeasureParameters scoringModifier, Transform relativeTo, PoseCalculator minimalTranslationPoseCalculator, PoseCalculator minimalRotationPoseCalculator)
	{
		if (scoringModifier.PositionRotationWeight == 1f)
		{
			bestPose = minimalRotationPoseCalculator(in desiredPose, relativeTo);
			return new GrabPoseScore(in desiredPose, in bestPose, in offset, scoringModifier);
		}
		if (scoringModifier.PositionRotationWeight == 0f)
		{
			bestPose = minimalTranslationPoseCalculator(in desiredPose, relativeTo);
			return new GrabPoseScore(in desiredPose, in bestPose, in offset, scoringModifier);
		}
		Pose poseB = minimalTranslationPoseCalculator(in desiredPose, relativeTo);
		Pose poseA = minimalRotationPoseCalculator(in desiredPose, relativeTo);
		bestPose = SelectBestPose(in poseA, in poseB, in desiredPose, in offset, scoringModifier, out var bestScore);
		return bestScore;
	}

	public static Pose SelectBestPose(in Pose poseA, in Pose poseB, in Pose reference, in Pose offset, PoseMeasureParameters scoringModifier, out GrabPoseScore bestScore)
	{
		GrabPoseScore grabPoseScore = new GrabPoseScore(in reference, in poseA, in offset, scoringModifier);
		GrabPoseScore grabPoseScore2 = new GrabPoseScore(in reference, in poseB, in offset, scoringModifier);
		if (grabPoseScore.IsBetterThan(grabPoseScore2))
		{
			bestScore = grabPoseScore;
			return poseA;
		}
		bestScore = grabPoseScore2;
		return poseB;
	}

	public static GrabPoseScore CollidersScore(Vector3 position, Collider[] colliders, out Vector3 hitPoint)
	{
		GrabPoseScore grabPoseScore = GrabPoseScore.Max;
		hitPoint = position;
		foreach (Collider collider in colliders)
		{
			bool flag = Collisions.IsPointWithinCollider(position, collider);
			Vector3 vector = (flag ? collider.bounds.center : collider.ClosestPoint(position));
			GrabPoseScore grabPoseScore2 = new GrabPoseScore(position, vector, flag);
			if (grabPoseScore2.IsBetterThan(grabPoseScore))
			{
				hitPoint = (flag ? position : vector);
				grabPoseScore = grabPoseScore2;
			}
		}
		return grabPoseScore;
	}
}
