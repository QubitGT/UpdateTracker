using System;
using UnityEngine;

namespace Oculus.Interaction.Throw;

public class RANSACVelocity
{
	protected struct TimedPose
	{
		public float time;

		public Pose pose;

		public TimedPose(float time, Pose pose)
		{
			this.time = time;
			this.pose = pose;
		}
	}

	private bool _highConfidenceStreak;

	private float _lastProcessedTime;

	private float _maxSyntheticSpeed = 5f;

	private const float _minSyntheticSpeed = 0.0001f;

	private RandomSampleConsensus<Vector3> _ransac;

	private RingBuffer<TimedPose> _poses;

	public float MaxSyntheticSpeed
	{
		get
		{
			return _maxSyntheticSpeed;
		}
		set
		{
			_maxSyntheticSpeed = Mathf.Max(0.0001f, value);
		}
	}

	[Obsolete("The minHighConfidenceSamples parameter will be ignored. Use the constructor without it")]
	public RANSACVelocity(int samplesCount = 10, int samplesDeadZone = 2, int minHighConfidenceSamples = 2)
		: this(samplesCount, samplesDeadZone)
	{
	}

	public RANSACVelocity(int samplesCount = 10, int samplesDeadZone = 2)
	{
		_poses = new RingBuffer<TimedPose>(samplesCount);
		_ransac = new RandomSampleConsensus<Vector3>(samplesCount, samplesDeadZone);
	}

	public void Initialize()
	{
		_poses.Clear();
		_highConfidenceStreak = false;
	}

	public void Process(Pose pose, float time, bool isHighConfidence = true)
	{
		if (_poses.Count > 0 && _poses.Peek().time == time)
		{
			return;
		}
		if (!isHighConfidence)
		{
			_highConfidenceStreak = false;
		}
		else
		{
			if (!_highConfidenceStreak && _poses.Count > 0)
			{
				TimedPose item = _poses.Peek();
				_poses.Clear();
				float num = Vector3.Distance(pose.position, item.pose.position);
				float num2 = time - _lastProcessedTime;
				if (Mathf.Approximately(num2, 0f) || num / num2 > _maxSyntheticSpeed)
				{
					item.time = time - num / _maxSyntheticSpeed;
				}
				else
				{
					item.time = _lastProcessedTime;
				}
				_poses.Add(item);
			}
			_highConfidenceStreak = true;
			TimedPose item2 = new TimedPose(time, pose);
			_poses.Add(item2);
		}
		_lastProcessedTime = time;
	}

	public void GetVelocities(out Vector3 velocity, out Vector3 torque)
	{
		if (_poses.Count >= 2)
		{
			velocity = _ransac.FindOptimalModel(CalculateVelocityFromSamples, ScoreDistance, _poses.Count);
			torque = _ransac.FindOptimalModel(CalculateTorqueFromSamples, ScoreAngularDistance, _poses.Count);
		}
		else
		{
			velocity = Vector3.zero;
			torque = Vector3.zero;
		}
	}

	private Vector3 CalculateVelocityFromSamples(int idx1, int idx2)
	{
		GetSortedTimePoses(idx1, idx2, out var older, out var younger);
		float num = younger.time - older.time;
		return PositionOffset(younger.pose, older.pose) / num;
	}

	private Vector3 CalculateTorqueFromSamples(int idx1, int idx2)
	{
		GetSortedTimePoses(idx1, idx2, out var older, out var younger);
		return GetTorque(older, younger);
	}

	protected virtual Vector3 PositionOffset(Pose youngerPose, Pose olderPose)
	{
		return youngerPose.position - olderPose.position;
	}

	private float ScoreDistance(Vector3 distance, Vector3[,] distances)
	{
		float num = 0f;
		for (int i = 0; i < _poses.Count; i++)
		{
			for (int j = i + 1; j < _poses.Count; j++)
			{
				num += (distance - distances[i, j]).sqrMagnitude;
			}
		}
		return num;
	}

	protected void GetSortedTimePoses(int idx1, int idx2, out TimedPose older, out TimedPose younger)
	{
		int index = idx1;
		int index2 = idx2;
		if (idx2 > idx1)
		{
			index = idx2;
			index2 = idx1;
		}
		older = _poses[index2];
		younger = _poses[index];
	}

	private float ScoreAngularDistance(Vector3 angularDistance, Vector3[,] angularDistances)
	{
		float num = 0f;
		Quaternion a = Quaternion.Euler(angularDistance);
		for (int i = 0; i < _poses.Count; i++)
		{
			for (int j = i + 1; j < _poses.Count; j++)
			{
				Quaternion b = Quaternion.Euler(angularDistances[i, j]);
				num += Mathf.Abs(Quaternion.Dot(a, b));
			}
		}
		return num;
	}

	protected static Vector3 GetTorque(TimedPose older, TimedPose younger)
	{
		float num = younger.time - older.time;
		Quaternion rotation = older.pose.rotation;
		Quaternion rotation2 = younger.pose.rotation;
		if (Quaternion.Dot(rotation, rotation2) < 0f)
		{
			rotation2.x = 0f - rotation2.x;
			rotation2.y = 0f - rotation2.y;
			rotation2.z = 0f - rotation2.z;
			rotation2.w = 0f - rotation2.w;
		}
		(rotation2 * Quaternion.Inverse(rotation)).ToAngleAxis(out var angle, out var axis);
		angle = angle * (MathF.PI / 180f) / num;
		return axis * angle;
	}
}
