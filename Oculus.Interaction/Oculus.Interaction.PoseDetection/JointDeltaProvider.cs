using System;
using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.PoseDetection;

public class JointDeltaProvider : MonoBehaviour, IJointDeltaProvider
{
	private class PoseData
	{
		public bool IsValid;

		public Pose Pose = Pose.identity;
	}

	[SerializeField]
	[Interface(typeof(IHand), new Type[] { })]
	private UnityEngine.Object _hand;

	private IHand Hand;

	private Dictionary<HandJointId, PoseData[]> _poseDataCache = new Dictionary<HandJointId, PoseData[]>();

	private HashSet<HandJointId> _trackedJoints = new HashSet<HandJointId>();

	private Dictionary<int, List<HandJointId>> _requestors = new Dictionary<int, List<HandJointId>>();

	private int CurDataIndex;

	private int _lastUpdateDataVersion;

	protected bool _started;

	private int PrevDataIndex => 1 - CurDataIndex;

	public bool GetPositionDelta(HandJointId joint, out Vector3 delta)
	{
		UpdateData();
		PoseData poseData = _poseDataCache[joint][PrevDataIndex];
		PoseData poseData2 = _poseDataCache[joint][CurDataIndex];
		if (!poseData.IsValid || !poseData2.IsValid)
		{
			delta = Vector3.zero;
			return false;
		}
		delta = poseData2.Pose.position - poseData.Pose.position;
		return true;
	}

	public bool GetRotationDelta(HandJointId joint, out Quaternion delta)
	{
		UpdateData();
		PoseData poseData = _poseDataCache[joint][PrevDataIndex];
		PoseData poseData2 = _poseDataCache[joint][CurDataIndex];
		if (!poseData.IsValid || !poseData2.IsValid)
		{
			delta = Quaternion.identity;
			return false;
		}
		delta = poseData2.Pose.rotation * Quaternion.Inverse(poseData.Pose.rotation);
		return true;
	}

	public bool GetPrevJointPose(HandJointId joint, out Pose pose)
	{
		UpdateData();
		PoseData poseData = _poseDataCache[joint][PrevDataIndex];
		pose = poseData.Pose;
		return poseData.IsValid;
	}

	public void RegisterConfig(JointDeltaConfig config)
	{
		_requestors.ContainsKey(config.InstanceID);
		_requestors.Add(config.InstanceID, new List<HandJointId>(config.JointIDs));
		foreach (HandJointId jointID in config.JointIDs)
		{
			if (!_poseDataCache.ContainsKey(jointID))
			{
				_poseDataCache.Add(jointID, new PoseData[2]
				{
					new PoseData(),
					new PoseData()
				});
				PoseData poseData = _poseDataCache[jointID][CurDataIndex];
				poseData.IsValid = Hand.GetJointPose(jointID, out poseData.Pose);
			}
		}
	}

	public void UnRegisterConfig(JointDeltaConfig config)
	{
		_requestors.Remove(config.InstanceID);
	}

	protected virtual void Awake()
	{
		Hand = _hand as IHand;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated += UpdateData;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			Hand.WhenHandUpdated -= UpdateData;
		}
	}

	private void UpdateData()
	{
		if (Hand.CurrentDataVersion <= _lastUpdateDataVersion)
		{
			return;
		}
		_lastUpdateDataVersion = Hand.CurrentDataVersion;
		CurDataIndex = 1 - CurDataIndex;
		_trackedJoints.Clear();
		foreach (int key in _requestors.Keys)
		{
			IList<HandJointId> other = _requestors[key];
			_trackedJoints.UnionWithNonAlloc(other);
		}
		foreach (HandJointId key2 in _poseDataCache.Keys)
		{
			PoseData poseData = _poseDataCache[key2][CurDataIndex];
			poseData.IsValid = _trackedJoints.Contains(key2) && Hand.GetJointPose(key2, out poseData.Pose);
		}
	}
}
