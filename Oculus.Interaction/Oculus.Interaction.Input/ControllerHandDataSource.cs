using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction.Input;

public class ControllerHandDataSource : DataSource<HandDataAsset>
{
	[SerializeField]
	private DataSource<ControllerDataAsset> _controllerSource;

	[SerializeField]
	private Transform _root;

	[SerializeField]
	private Transform _openXRRoot;

	[SerializeField]
	private bool _rootIsLocal = true;

	[SerializeField]
	[FormerlySerializedAs("_bones")]
	[FormerlySerializedAs("_joints")]
	private Transform[] _jointTransforms;

	[SerializeField]
	private Transform[] _openXRJointTransforms;

	private HandDataSourceConfig _config;

	private readonly HandDataAsset _handDataAsset = new HandDataAsset();

	public Transform Root
	{
		get
		{
			return _openXRRoot;
		}
		set
		{
			_openXRRoot = value;
		}
	}

	public bool RootIsLocal
	{
		get
		{
			return _rootIsLocal;
		}
		set
		{
			_rootIsLocal = value;
		}
	}

	public Transform[] Joints => _openXRJointTransforms;

	protected override HandDataAsset DataAsset => _handDataAsset;

	private HandDataSourceConfig Config
	{
		get
		{
			if (_config == null)
			{
				_config = new HandDataSourceConfig();
			}
			return _config;
		}
	}

	protected virtual void Awake()
	{
		if (_root != null)
		{
			_root.gameObject.SetActive(value: false);
		}
		if (_openXRRoot != null)
		{
			_openXRRoot.gameObject.SetActive(value: true);
		}
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		UpdateConfig();
		this.EndStart(ref _started);
	}

	private void UpdateConfig()
	{
		ControllerDataSourceConfig config = _controllerSource.GetData().Config;
		Config.Handedness = config.Handedness;
		Config.TrackingToWorldTransformer = config.TrackingToWorldTransformer;
		Config.HandSkeleton = HandSkeleton.FromJoints(Joints);
	}

	protected override void UpdateData()
	{
		ControllerDataAsset data = _controllerSource.GetData();
		_handDataAsset.Config = Config;
		_handDataAsset.IsDataValid = data.IsDataValid;
		_handDataAsset.IsConnected = data.IsConnected;
		if (!_handDataAsset.IsConnected || !base.isActiveAndEnabled)
		{
			_handDataAsset.IsTracked = false;
			_handDataAsset.RootPoseOrigin = PoseOrigin.None;
			_handDataAsset.PointerPoseOrigin = PoseOrigin.None;
			_handDataAsset.IsHighConfidence = false;
			for (int i = 0; i < 5; i++)
			{
				_handDataAsset.IsFingerPinching[i] = false;
				_handDataAsset.IsFingerHighConfidence[i] = false;
			}
			return;
		}
		_handDataAsset.IsTracked = data.IsTracked;
		_handDataAsset.IsHighConfidence = true;
		_handDataAsset.IsDominantHand = data.IsDominantHand;
		float trigger = data.Input.Trigger;
		float grip = data.Input.Grip;
		bool triggerButton = data.Input.TriggerButton;
		bool gripButton = data.Input.GripButton;
		_handDataAsset.IsFingerHighConfidence[0] = true;
		_handDataAsset.IsFingerPinching[0] = triggerButton || gripButton;
		_handDataAsset.FingerPinchStrength[0] = Mathf.Max(trigger, grip);
		_handDataAsset.IsFingerHighConfidence[1] = true;
		_handDataAsset.IsFingerPinching[1] = triggerButton;
		_handDataAsset.FingerPinchStrength[1] = trigger;
		_handDataAsset.IsFingerHighConfidence[2] = true;
		_handDataAsset.IsFingerPinching[2] = gripButton;
		_handDataAsset.FingerPinchStrength[2] = grip;
		_handDataAsset.IsFingerHighConfidence[3] = true;
		_handDataAsset.IsFingerPinching[3] = false;
		_handDataAsset.FingerPinchStrength[3] = 0f;
		_handDataAsset.IsFingerHighConfidence[4] = true;
		_handDataAsset.IsFingerPinching[4] = false;
		_handDataAsset.FingerPinchStrength[4] = 0f;
		_handDataAsset.PointerPoseOrigin = PoseOrigin.FilteredTrackedPose;
		_handDataAsset.PointerPose = data.PointerPose;
		for (int j = 0; j < Joints.Length; j++)
		{
			_handDataAsset.Joints[j] = Joints[j].localRotation;
			_handDataAsset.JointPoses[j] = Root.Delta(Joints[j]);
		}
		if (_rootIsLocal)
		{
			Pose b = Root.GetPose(Space.Self);
			Pose a = data.RootPose;
			PoseUtils.Multiply(in a, in b, ref _handDataAsset.Root);
			_handDataAsset.HandScale = Root.localScale.x;
		}
		else
		{
			_handDataAsset.Root = Root.GetPose();
			_handDataAsset.HandScale = Root.lossyScale.x;
		}
		_handDataAsset.RootPoseOrigin = PoseOrigin.FilteredTrackedPose;
	}

	public void InjectAllControllerHandDataSource(UpdateModeFlags updateMode, IDataSource updateAfter, DataSource<ControllerDataAsset> controllerSource, Transform[] jointTransforms)
	{
		InjectAllDataSource(updateMode, updateAfter);
		InjectControllerSource(controllerSource);
		InjectJointTransforms(jointTransforms);
	}

	public void InjectControllerSource(DataSource<ControllerDataAsset> controllerSource)
	{
		_controllerSource = controllerSource;
	}

	[Obsolete("Use InjectJointTransforms instead")]
	public void InjectBones(Transform[] joints)
	{
		InjectJointTransforms(joints);
	}

	public void InjectJointTransforms(Transform[] jointTransforms)
	{
		_openXRJointTransforms = jointTransforms;
	}
}
