using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class FromHandPrefabDataSource : DataSource<HandDataAsset>
{
	private readonly HandDataAsset _handDataAsset = new HandDataAsset();

	[SerializeField]
	private Handedness _handedness;

	[SerializeField]
	private bool _hidePrefabOnStart = true;

	[HideInInspector]
	[SerializeField]
	private List<Transform> _jointTransforms = new List<Transform>();

	[HideInInspector]
	[SerializeField]
	private List<Transform> _jointTransformsOpenXR = new List<Transform>();

	[SerializeField]
	[Interface(typeof(IHandSkeletonProvider), new Type[] { })]
	private UnityEngine.Object _handSkeletonProvider;

	private IHandSkeletonProvider HandSkeletonProvider;

	[SerializeField]
	[Interface(typeof(ITrackingToWorldTransformer), new Type[] { })]
	[Optional]
	private UnityEngine.Object _trackingToWorldTransformer;

	private ITrackingToWorldTransformer TrackingToWorldTransformer;

	protected override HandDataAsset DataAsset => _handDataAsset;

	public Handedness Handedness => _handedness;

	public List<Transform> JointTransforms => _jointTransformsOpenXR;

	protected virtual void Awake()
	{
		HandSkeletonProvider = _handSkeletonProvider as IHandSkeletonProvider;
		if (_trackingToWorldTransformer != null)
		{
			TrackingToWorldTransformer = _trackingToWorldTransformer as ITrackingToWorldTransformer;
		}
		_handDataAsset.Config.Handedness = _handedness;
	}

	protected override void Start()
	{
		base.Start();
		HandDataSourceConfig config = _handDataAsset.Config;
		config.TrackingToWorldTransformer = TrackingToWorldTransformer;
		config.HandSkeleton = HandSkeletonProvider[_handedness];
		if (_hidePrefabOnStart)
		{
			Renderer[] componentsInChildren = base.gameObject.GetComponentsInChildren<Renderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}
	}

	protected override void UpdateData()
	{
		_handDataAsset.IsDataValid = true;
		_handDataAsset.IsConnected = true;
		_handDataAsset.IsTracked = true;
		_handDataAsset.IsHighConfidence = true;
		_handDataAsset.RootPoseOrigin = PoseOrigin.SyntheticPose;
		_handDataAsset.Root = base.transform.GetPose();
		_handDataAsset.HandScale = 1f;
		for (int i = 0; i < 26; i++)
		{
			Transform transform = JointTransforms[i];
			if (transform == null)
			{
				_handDataAsset.Joints[i] = Quaternion.identity;
				_handDataAsset.JointPoses[i] = new Pose(Vector3.zero, Quaternion.identity);
				continue;
			}
			_handDataAsset.Joints[i] = transform.transform.localRotation;
			Pose to = transform.transform.GetPose();
			to = base.transform.Delta(in to);
			_handDataAsset.JointPoses[i] = to;
		}
	}

	public Transform GetTransformFor(HandJointId jointId)
	{
		return JointTransforms[(int)jointId];
	}
}
