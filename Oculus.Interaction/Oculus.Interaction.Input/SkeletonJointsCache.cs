using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public abstract class SkeletonJointsCache
{
	private const int ULONG_BITS = 64;

	protected Pose[] _originalPoses;

	protected Pose[] _posesFromRoot;

	protected Pose[] _localPoses;

	protected Pose[] _worldPoses;

	private ulong[] _dirtyJointsFromRoot;

	private ulong[] _dirtyLocalJoints;

	private ulong[] _dirtyWorldJoints;

	private Matrix4x4 _scale;

	private Pose _rootPose;

	private Pose _worldRoot;

	private readonly int _numJoints;

	private readonly int _dirtyArraySize;

	public int LocalDataVersion { get; private set; } = -1;


	protected abstract bool TryGetParent(int joint, out int parent);

	public SkeletonJointsCache(int numJoints)
	{
		LocalDataVersion = -1;
		_numJoints = numJoints;
		_originalPoses = new Pose[numJoints];
		_posesFromRoot = new Pose[numJoints];
		_localPoses = new Pose[numJoints];
		_worldPoses = new Pose[numJoints];
		_dirtyArraySize = 1 + numJoints / 64;
		_dirtyJointsFromRoot = new ulong[_dirtyArraySize];
		_dirtyLocalJoints = new ulong[_dirtyArraySize];
		_dirtyWorldJoints = new ulong[_dirtyArraySize];
	}

	public void Update(int dataVersion, Pose rootPose, Pose[] jointPoses, float scale, Transform trackingSpace = null)
	{
		LocalDataVersion = dataVersion;
		for (int i = 0; i < _dirtyArraySize; i++)
		{
			_dirtyJointsFromRoot[i] = ulong.MaxValue;
			_dirtyLocalJoints[i] = ulong.MaxValue;
			_dirtyWorldJoints[i] = ulong.MaxValue;
		}
		_scale = Matrix4x4.Scale(Vector3.one * scale);
		_rootPose = rootPose;
		_worldRoot = _rootPose;
		if (trackingSpace != null)
		{
			_scale *= Matrix4x4.Scale(trackingSpace.lossyScale);
			_worldRoot.position = trackingSpace.TransformPoint(_rootPose.position);
			_worldRoot.rotation = trackingSpace.rotation * _rootPose.rotation;
		}
		Array.Copy(jointPoses, _originalPoses, _numJoints);
	}

	public Pose GetLocalJointPose(int jointId)
	{
		UpdateLocalJointPose(jointId);
		return _localPoses[jointId];
	}

	public Pose GetJointPoseFromRoot(int jointId)
	{
		UpdateJointPoseFromRoot(jointId);
		return _posesFromRoot[jointId];
	}

	public Pose GetWorldJointPose(int jointId)
	{
		UpdateWorldJointPose(jointId);
		return _worldPoses[jointId];
	}

	public Pose GetWorldRootPose()
	{
		return _worldRoot;
	}

	private void UpdateJointPoseFromRoot(int jointId)
	{
		if (CheckJointDirty(jointId, _dirtyJointsFromRoot))
		{
			_posesFromRoot[jointId] = _originalPoses[jointId];
			SetJointClean(jointId, _dirtyJointsFromRoot);
		}
	}

	private void UpdateLocalJointPose(int jointId)
	{
		if (CheckJointDirty(jointId, _dirtyLocalJoints))
		{
			if (TryGetParent(jointId, out var parent))
			{
				Pose pose = _originalPoses[jointId];
				Pose pose2 = _originalPoses[parent];
				Vector3 position = Quaternion.Inverse(pose2.rotation) * (pose.position - pose2.position);
				Quaternion rotation = Quaternion.Inverse(pose2.rotation) * pose.rotation;
				_localPoses[jointId] = new Pose(position, rotation);
			}
			else
			{
				_localPoses[jointId] = Pose.identity;
			}
			SetJointClean(jointId, _dirtyLocalJoints);
		}
	}

	private void UpdateWorldJointPose(int jointId)
	{
		if (CheckJointDirty(jointId, _dirtyWorldJoints))
		{
			Pose a = GetJointPoseFromRoot(jointId);
			a.position = _scale * a.position;
			Pose b = GetWorldRootPose();
			a.Postmultiply(in b);
			_worldPoses[jointId] = a;
			SetJointClean(jointId, _dirtyWorldJoints);
		}
	}

	protected void UpdateAllWorldPoses()
	{
		for (int i = 0; i < _numJoints; i++)
		{
			UpdateWorldJointPose(i);
		}
	}

	protected void UpdateAllLocalPoses()
	{
		for (int i = 0; i < _numJoints; i++)
		{
			UpdateLocalJointPose(i);
		}
	}

	protected void UpdateAllPosesFromRoot()
	{
		for (int i = 0; i < _numJoints; i++)
		{
			UpdateJointPoseFromRoot(i);
		}
	}

	private bool CheckJointDirty(int jointId, ulong[] dirtyFlags)
	{
		int num = jointId / 64;
		int num2 = jointId % 64;
		return (dirtyFlags[num] & (ulong)(1L << num2)) != 0;
	}

	private void SetJointClean(int jointId, ulong[] dirtyFlags)
	{
		int num = jointId / 64;
		int num2 = jointId % 64;
		dirtyFlags[num] &= (ulong)(~(1L << num2));
	}
}
