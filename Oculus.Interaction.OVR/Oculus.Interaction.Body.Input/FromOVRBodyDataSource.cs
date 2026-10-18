using System;
using Meta.XR.Util;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Body.Input;

[Feature(Feature.Interaction)]
public class FromOVRBodyDataSource : DataSource<BodyDataAsset>
{
	[Header("OVR Data Source")]
	[SerializeField]
	[Interface(typeof(OVRSkeleton.IOVRSkeletonDataProvider), new Type[] { })]
	private UnityEngine.Object _dataProvider;

	private OVRSkeleton.IOVRSkeletonDataProvider DataProvider;

	[SerializeField]
	[Interface(typeof(IOVRCameraRigRef), new Type[] { })]
	private UnityEngine.Object _cameraRigRef;

	private IOVRCameraRigRef CameraRigRef;

	[SerializeField]
	private bool _processLateUpdates;

	private readonly BodyDataAsset _bodyDataAsset = new BodyDataAsset();

	private OVRSkeletonMapping _mapping;

	protected override BodyDataAsset DataAsset => _bodyDataAsset;

	private static OVRPlugin.BodyJointSet GetJointSet(OVRSkeleton.IOVRSkeletonDataProvider provider)
	{
		return provider.GetSkeletonType() switch
		{
			OVRSkeleton.SkeletonType.Body => OVRPlugin.BodyJointSet.UpperBody, 
			OVRSkeleton.SkeletonType.FullBody => OVRPlugin.BodyJointSet.FullBody, 
			_ => OVRPlugin.BodyJointSet.None, 
		};
	}

	protected void Awake()
	{
		CameraRigRef = _cameraRigRef as IOVRCameraRigRef;
		DataProvider = _dataProvider as OVRSkeleton.IOVRSkeletonDataProvider;
	}

	protected override void Start()
	{
		base.Start();
		_mapping = new OVRSkeletonMapping(GetJointSet(DataProvider));
		_bodyDataAsset.SkeletonMapping = _mapping;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (_started)
		{
			CameraRigRef.WhenInputDataDirtied += HandleInputDataDirtied;
		}
	}

	protected override void OnDisable()
	{
		if (_started)
		{
			CameraRigRef.WhenInputDataDirtied -= HandleInputDataDirtied;
		}
		base.OnDisable();
	}

	private void HandleInputDataDirtied(bool isLateUpdate)
	{
		if (!isLateUpdate || _processLateUpdates)
		{
			MarkInputDataRequiresUpdate();
		}
	}

	protected override void UpdateData()
	{
		OVRSkeleton.SkeletonPoseData skeletonPoseData = DataProvider.GetSkeletonPoseData();
		if (!skeletonPoseData.IsDataValid)
		{
			return;
		}
		_bodyDataAsset.SkeletonMapping = _mapping;
		_bodyDataAsset.IsDataHighConfidence = skeletonPoseData.IsDataHighConfidence;
		_bodyDataAsset.IsDataValid = skeletonPoseData.IsDataValid;
		_bodyDataAsset.SkeletonChangedCount = skeletonPoseData.SkeletonChangedCount;
		_bodyDataAsset.RootScale = skeletonPoseData.RootScale;
		_bodyDataAsset.Root = new Pose
		{
			position = skeletonPoseData.RootPose.Position.FromFlippedZVector3f(),
			rotation = skeletonPoseData.RootPose.Orientation.FromFlippedZQuatf()
		};
		foreach (BodyJointId joint in _mapping.Joints)
		{
			Pose to = default(Pose);
			Pose pose;
			if (_mapping.TryGetSourceJointId(joint, out var sourceJointId))
			{
				int num = (int)sourceJointId;
				pose = default(Pose);
				pose.rotation = (float.IsNaN(skeletonPoseData.BoneRotations[num].w) ? default(Quaternion) : skeletonPoseData.BoneRotations[num].FromFlippedZQuatf());
				pose.position = skeletonPoseData.BoneTranslations[num].FromFlippedZVector3f();
				to = pose;
			}
			Pose[] jointPoses = _bodyDataAsset.JointPoses;
			pose = _bodyDataAsset.Root;
			jointPoses[(int)joint] = PoseUtils.Delta(in pose, in to);
		}
	}
}
