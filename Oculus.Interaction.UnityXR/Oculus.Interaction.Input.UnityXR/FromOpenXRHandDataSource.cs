using System;
using Oculus.Interaction.GrabAPI;
using UnityEngine;

namespace Oculus.Interaction.Input.UnityXR;

public abstract class FromOpenXRHandDataSource : DataSource<HandDataAsset>
{
	private static readonly float DefaultSkeletonIndexMagnitude = HandSkeleton.DefaultLeftSkeleton[8].pose.position.magnitude;

	private const float PressThreshold = 0.8f;

	private static readonly Vector3 TrackedRemoteAimOffset = new Vector3(0f, 0f, -0.055f);

	[SerializeField]
	[Interface(typeof(IHmd), new Type[] { })]
	private UnityEngine.Object _hmdData;

	private IHmd HmdData;

	protected readonly HandDataAsset _dataAsset = new HandDataAsset();

	protected bool _shouldMockHandTrackingAim;

	private PinchGrabAPI _fingerGrabAPI;

	protected override HandDataAsset DataAsset => _dataAsset;

	protected virtual void Awake()
	{
		HmdData = _hmdData as IHmd;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected override void UpdateData()
	{
		for (int i = 0; i < 26; i++)
		{
			int num = (int)HandJointUtils.JointParentList[i];
			_dataAsset.Joints[i] = ((num < 0) ? Quaternion.identity : (Quaternion.Inverse(_dataAsset.JointPoses[num].rotation) * _dataAsset.JointPoses[i].rotation));
		}
		UpdateHandScale(_dataAsset.JointPoses[7].position, _dataAsset.JointPoses[8].position);
		if (_dataAsset.IsDataValidAndConnected && _shouldMockHandTrackingAim)
		{
			PopulateMockHandTrackingAim(_dataAsset.JointPoses[0]);
		}
	}

	private void UpdateHandScale(Vector3 indexProximal, Vector3 indexIntermediate)
	{
		float num = Vector3.Distance(indexProximal, indexIntermediate);
		_dataAsset.HandScale = num / DefaultSkeletonIndexMagnitude;
		float num2 = 1f / _dataAsset.HandScale;
		for (int i = 0; i < 26; i++)
		{
			_dataAsset.JointPoses[i].position *= num2;
		}
	}

	private void PopulateMockHandTrackingAim(Pose xrPalmPose)
	{
		_dataAsset.PointerPose = xrPalmPose.GetTransformedBy(new Pose(TrackedRemoteAimOffset, Quaternion.identity));
		_dataAsset.PointerPoseOrigin = PoseOrigin.SyntheticPose;
		_dataAsset.IsDominantHand = _dataAsset.Config.Handedness == Handedness.Right;
		Pose[] jointPoses = _dataAsset.JointPoses;
		if (_fingerGrabAPI == null)
		{
			_fingerGrabAPI = new PinchGrabAPI(HmdData);
		}
		_fingerGrabAPI.Update(jointPoses, _dataAsset.Config.Handedness, _dataAsset.Root, _dataAsset.HandScale);
		PopulateMockHandTrackingAimFinger(HandFinger.Index);
		PopulateMockHandTrackingAimFinger(HandFinger.Middle);
		PopulateMockHandTrackingAimFinger(HandFinger.Ring);
		PopulateMockHandTrackingAimFinger(HandFinger.Pinky);
	}

	private void PopulateMockHandTrackingAimFinger(HandFinger finger)
	{
		_dataAsset.FingerPinchStrength[(int)finger] = _fingerGrabAPI.GetFingerGrabScore(finger);
		_dataAsset.IsFingerPinching[(int)finger] = _dataAsset.FingerPinchStrength[(int)finger] > 0.8f;
	}
}
