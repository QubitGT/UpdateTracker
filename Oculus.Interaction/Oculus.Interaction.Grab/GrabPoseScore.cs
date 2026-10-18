using UnityEngine;

namespace Oculus.Interaction.Grab;

public struct GrabPoseScore
{
	private float _translationScore;

	private float _rotationScore;

	private PoseMeasureParameters _measureParameters;

	public static readonly GrabPoseScore Max = new GrabPoseScore(float.PositiveInfinity, float.PositiveInfinity, PoseMeasureParameters.DEFAULT);

	public GrabPoseScore(float translationScore, float rotationScore, PoseMeasureParameters measureParameters)
	{
		_translationScore = translationScore;
		_rotationScore = rotationScore;
		_measureParameters = measureParameters;
	}

	public GrabPoseScore(Vector3 fromPoint, Vector3 toPoint, bool isInside = false)
	{
		_translationScore = PositionalScore(in fromPoint, in toPoint);
		_rotationScore = 0f;
		_measureParameters = PoseMeasureParameters.DEFAULT;
		if (isInside)
		{
			_translationScore = 0f - Mathf.Abs(_translationScore);
		}
	}

	public GrabPoseScore(in Pose poseA, in Pose poseB, in Pose offset, PoseMeasureParameters measureParameters)
	{
		Pose pose = PoseUtils.Multiply(in poseA, in offset);
		Pose pose2 = PoseUtils.Multiply(in poseB, in offset);
		_translationScore = PositionalScore(in pose.position, in pose2.position);
		_rotationScore = RotationalScore(in pose.rotation, in pose2.rotation);
		_measureParameters = measureParameters;
	}

	public bool IsValid()
	{
		if (_translationScore != float.PositiveInfinity)
		{
			return _rotationScore != float.PositiveInfinity;
		}
		return false;
	}

	private float Score(float maxDistance)
	{
		return Mathf.Lerp(_translationScore, _rotationScore * maxDistance, _measureParameters.PositionRotationWeight);
	}

	private static float PositionalScore(in Vector3 from, in Vector3 to)
	{
		return (from - to).sqrMagnitude;
	}

	private static float RotationalScore(in Quaternion from, in Quaternion to)
	{
		float num = Vector3.Dot(from * Vector3.forward, to * Vector3.forward) * 0.5f + 0.5f;
		float num2 = Vector3.Dot(from * Vector3.up, to * Vector3.up) * 0.5f + 0.5f;
		return 1f - num * num2;
	}

	public static GrabPoseScore Lerp(in GrabPoseScore from, in GrabPoseScore to, float t)
	{
		return new GrabPoseScore(Mathf.Lerp(from._translationScore, to._translationScore, t), Mathf.Lerp(from._rotationScore, to._rotationScore, t), PoseMeasureParameters.Lerp(in from._measureParameters, in to._measureParameters, t));
	}

	public bool IsBetterThan(GrabPoseScore referenceScore)
	{
		if (_translationScore == float.PositiveInfinity)
		{
			return false;
		}
		if (referenceScore._translationScore == float.PositiveInfinity)
		{
			return true;
		}
		float maxDistance = Mathf.Max(_translationScore, referenceScore._translationScore);
		float num = Score(maxDistance);
		float num2 = referenceScore.Score(maxDistance);
		if ((!(num < 0f) || !(num2 > 0f)) && (!(num < 0f) || !(num2 < 0f) || !(num > num2)))
		{
			if (num > 0f && num2 > 0f)
			{
				return num < num2;
			}
			return false;
		}
		return true;
	}
}
