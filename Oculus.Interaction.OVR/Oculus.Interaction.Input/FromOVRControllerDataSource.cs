using System;
using Meta.XR.Util;
using UnityEngine;

namespace Oculus.Interaction.Input;

[Feature(Feature.Interaction)]
public class FromOVRControllerDataSource : DataSource<ControllerDataAsset>
{
	[Header("OVR Data Source")]
	[SerializeField]
	[Interface(typeof(IOVRCameraRigRef), new Type[] { })]
	private UnityEngine.Object _cameraRigRef;

	[SerializeField]
	private bool _processLateUpdates;

	[Header("Shared Configuration")]
	[SerializeField]
	private Handedness _handedness;

	[SerializeField]
	[Interface(typeof(ITrackingToWorldTransformer), new Type[] { })]
	private UnityEngine.Object _trackingToWorldTransformer;

	private ITrackingToWorldTransformer TrackingToWorldTransformer;

	private readonly ControllerDataAsset _controllerDataAsset = new ControllerDataAsset();

	private OVRInput.Controller _ovrController;

	private ControllerDataSourceConfig _config;

	private OVRPointerPoseSelector _pointerPoseSelector;

	private static readonly IUsage[] ControllerUsageMappings = new IUsage[13]
	{
		new UsageButtonMapping(ControllerButtonUsage.PrimaryButton, OVRInput.Button.One),
		new UsageTouchMapping(ControllerButtonUsage.PrimaryTouch, OVRInput.Touch.One),
		new UsageButtonMapping(ControllerButtonUsage.SecondaryButton, OVRInput.Button.Two),
		new UsageTouchMapping(ControllerButtonUsage.SecondaryTouch, OVRInput.Touch.Two),
		new UsageButtonMapping(ControllerButtonUsage.GripButton, OVRInput.Button.PrimaryHandTrigger),
		new UsageButtonMapping(ControllerButtonUsage.TriggerButton, OVRInput.Button.PrimaryIndexTrigger),
		new UsageButtonMapping(ControllerButtonUsage.MenuButton, OVRInput.Button.Start),
		new UsageButtonMapping(ControllerButtonUsage.Primary2DAxisClick, OVRInput.Button.PrimaryThumbstick),
		new UsageTouchMapping(ControllerButtonUsage.Primary2DAxisTouch, OVRInput.Touch.PrimaryThumbstick),
		new UsageTouchMapping(ControllerButtonUsage.Thumbrest, OVRInput.Touch.PrimaryThumbRest),
		new UsageAxis1DMapping(ControllerAxis1DUsage.Trigger, OVRInput.Axis1D.PrimaryIndexTrigger),
		new UsageAxis1DMapping(ControllerAxis1DUsage.Grip, OVRInput.Axis1D.PrimaryHandTrigger),
		new UsageAxis2DMapping(ControllerAxis2DUsage.Primary2DAxis, OVRInput.Axis2D.PrimaryThumbstick)
	};

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

	private ControllerDataSourceConfig Config
	{
		get
		{
			if (_config != null)
			{
				return _config;
			}
			_config = new ControllerDataSourceConfig
			{
				Handedness = _handedness
			};
			return _config;
		}
	}

	protected override ControllerDataAsset DataAsset => _controllerDataAsset;

	protected void Awake()
	{
		TrackingToWorldTransformer = _trackingToWorldTransformer as ITrackingToWorldTransformer;
		CameraRigRef = _cameraRigRef as IOVRCameraRigRef;
		UpdateConfig();
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		if (_handedness == Handedness.Left)
		{
			_ovrController = OVRInput.Controller.LTouch;
		}
		else
		{
			_ovrController = OVRInput.Controller.RTouch;
		}
		_pointerPoseSelector = new OVRPointerPoseSelector(_handedness);
		UpdateConfig();
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

	private void UpdateConfig()
	{
		Config.Handedness = _handedness;
		Config.TrackingToWorldTransformer = TrackingToWorldTransformer;
	}

	protected override void UpdateData()
	{
		_controllerDataAsset.Config = Config;
		_controllerDataAsset.IsDataValid = true;
		_controllerDataAsset.IsConnected = (OVRInput.GetConnectedControllers() & _ovrController) > OVRInput.Controller.None;
		if (!_controllerDataAsset.IsConnected || !base.isActiveAndEnabled)
		{
			_controllerDataAsset.IsConnected = false;
			_controllerDataAsset.IsTracked = false;
			_controllerDataAsset.Input = default(ControllerInput);
			_controllerDataAsset.RootPoseOrigin = PoseOrigin.None;
			return;
		}
		_controllerDataAsset.IsTracked = true;
		OVRInput.Handedness dominantHand = OVRInput.GetDominantHand();
		_controllerDataAsset.IsDominantHand = (dominantHand == OVRInput.Handedness.LeftHanded && _handedness == Handedness.Left) || (dominantHand == OVRInput.Handedness.RightHanded && _handedness == Handedness.Right);
		_controllerDataAsset.Input.Clear();
		OVRInput.Controller ovrController = _ovrController;
		IUsage[] controllerUsageMappings = ControllerUsageMappings;
		for (int i = 0; i < controllerUsageMappings.Length; i++)
		{
			controllerUsageMappings[i].Apply(_controllerDataAsset, ovrController);
		}
		_controllerDataAsset.RootPose = new Pose(OVRInput.GetLocalControllerPosition(_ovrController), OVRInput.GetLocalControllerRotation(_ovrController));
		_controllerDataAsset.RootPoseOrigin = PoseOrigin.RawTrackedPose;
		Matrix4x4 matrix4x = Matrix4x4.TRS(_controllerDataAsset.RootPose.position, _controllerDataAsset.RootPose.rotation, Vector3.one);
		_controllerDataAsset.PointerPose = new Pose(matrix4x.MultiplyPoint3x4(_pointerPoseSelector.LocalPointerPose.position), _controllerDataAsset.RootPose.rotation * _pointerPoseSelector.LocalPointerPose.rotation);
		_controllerDataAsset.PointerPoseOrigin = PoseOrigin.RawTrackedPose;
	}

	public void InjectAllFromOVRControllerDataSource(UpdateModeFlags updateMode, IDataSource updateAfter, Handedness handedness, ITrackingToWorldTransformer trackingToWorldTransformer)
	{
		InjectAllDataSource(updateMode, updateAfter);
		InjectHandedness(handedness);
		InjectTrackingToWorldTransformer(trackingToWorldTransformer);
	}

	public void InjectHandedness(Handedness handedness)
	{
		_handedness = handedness;
	}

	public void InjectTrackingToWorldTransformer(ITrackingToWorldTransformer trackingToWorldTransformer)
	{
		_trackingToWorldTransformer = trackingToWorldTransformer as UnityEngine.Object;
		TrackingToWorldTransformer = trackingToWorldTransformer;
	}
}
