using System;
using Meta.XR.Util;
using UnityEngine;
using UnityEngine.XR;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class FromOVRHmdDataSource : DataSource<HmdDataAsset>
{
	[Header("OVR Data Source")]
	[SerializeField]
	[Interface(typeof(IOVRCameraRigRef), new Type[] { })]
	private UnityEngine.Object _cameraRigRef;

	[SerializeField]
	private bool _processLateUpdates;

	[SerializeField]
	[Tooltip("If true, uses OVRManager.headPoseRelativeOffset rather than sensor data for HMD pose.")]
	private bool _useOvrManagerEmulatedPose;

	[Header("Shared Configuration")]
	[SerializeField]
	[Interface(typeof(ITrackingToWorldTransformer), new Type[] { })]
	private UnityEngine.Object _trackingToWorldTransformer;

	private ITrackingToWorldTransformer TrackingToWorldTransformer;

	private HmdDataAsset _hmdDataAsset = new HmdDataAsset();

	private HmdDataSourceConfig _config;

	public IOVRCameraRigRef CameraRigRef { get; private set; }

	public bool ProcessLateUpdates
	{
		get
		{
			return _processLateUpdates;
		}
		set
		{
			_processLateUpdates = value;
		}
	}

	private HmdDataSourceConfig Config
	{
		get
		{
			if (_config != null)
			{
				return _config;
			}
			_config = new HmdDataSourceConfig
			{
				TrackingToWorldTransformer = TrackingToWorldTransformer
			};
			return _config;
		}
	}

	protected override HmdDataAsset DataAsset => _hmdDataAsset;

	protected void Awake()
	{
		CameraRigRef = _cameraRigRef as IOVRCameraRigRef;
		TrackingToWorldTransformer = _trackingToWorldTransformer as ITrackingToWorldTransformer;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
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
		MarkInputDataRequiresUpdate();
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
		_hmdDataAsset.Config = Config;
		bool flag = OVRNodeStateProperties.IsHmdPresent() && base.isActiveAndEnabled;
		ref Pose root = ref _hmdDataAsset.Root;
		if (_useOvrManagerEmulatedPose)
		{
			Quaternion rotation = Quaternion.Euler(0f - OVRManager.instance.headPoseRelativeOffsetRotation.x, 0f - OVRManager.instance.headPoseRelativeOffsetRotation.y, OVRManager.instance.headPoseRelativeOffsetRotation.z);
			root.rotation = rotation;
			root.position = OVRManager.instance.headPoseRelativeOffsetTranslation;
			flag = true;
		}
		else
		{
			Pose pose = Pose.identity;
			if (_hmdDataAsset.IsTracked)
			{
				pose = _hmdDataAsset.Root;
			}
			if (flag)
			{
				if (!OVRNodeStateProperties.GetNodeStatePropertyVector3(XRNode.CenterEye, NodeStatePropertyType.Position, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out root.position))
				{
					root.position = pose.position;
				}
				if (!OVRNodeStateProperties.GetNodeStatePropertyQuaternion(XRNode.CenterEye, NodeStatePropertyType.Orientation, OVRPlugin.Node.EyeCenter, OVRPlugin.Step.Render, out root.rotation))
				{
					root.rotation = pose.rotation;
				}
			}
			else
			{
				root = pose;
			}
		}
		_hmdDataAsset.IsTracked = flag;
		_hmdDataAsset.FrameId = Time.frameCount;
	}

	public void InjectAllFromOVRHmdDataSource(UpdateModeFlags updateMode, IDataSource updateAfter, bool useOvrManagerEmulatedPose, ITrackingToWorldTransformer trackingToWorldTransformer)
	{
		InjectAllDataSource(updateMode, updateAfter);
		InjectUseOvrManagerEmulatedPose(useOvrManagerEmulatedPose);
		InjectTrackingToWorldTransformer(trackingToWorldTransformer);
	}

	public void InjectUseOvrManagerEmulatedPose(bool useOvrManagerEmulatedPose)
	{
		_useOvrManagerEmulatedPose = useOvrManagerEmulatedPose;
	}

	public void InjectTrackingToWorldTransformer(ITrackingToWorldTransformer trackingToWorldTransformer)
	{
		_trackingToWorldTransformer = trackingToWorldTransformer as UnityEngine.Object;
		TrackingToWorldTransformer = trackingToWorldTransformer;
	}
}
