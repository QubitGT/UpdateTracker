using System;
using System.Linq;
using Oculus.Interaction.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;

namespace Oculus.Interaction.UnityXR;

public class FromUnityXRControllerDataSource : DataSource<ControllerDataAsset>
{
	[Header("Shared Configuration")]
	[SerializeField]
	private Handedness _handedness;

	[SerializeField]
	[Interface(typeof(ITrackingToWorldTransformer), new Type[] { })]
	private UnityEngine.Object _trackingToWorldTransformer;

	private ITrackingToWorldTransformer TrackingToWorldTransformer;

	private static string ControllerActionMap = "{\n            \"maps\": [\n                {\n                    \"name\": \"XRController\",\n                    \"actions\": [\n                        {\n                            \"name\": \"PrimaryButton\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/primaryButton\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"PrimaryTouch\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/primaryTouched\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"SecondaryButton\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/secondaryButton\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"SecondaryTouch\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/secondaryTouched\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"GripButton\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/gripPressed\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"TriggerButton\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/triggerPressed\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"TriggerTouch\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/triggerTouched\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"MenuButton\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/menu\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"Primary2DAxisClick\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/thumbstickClicked\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"Primary2DAxisTouch\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/thumbstickTouched\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"Thumbrest\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/thumbrestTouched\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"Trigger\",\n                            \"expectedControlLayout\": \"Axis1D\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/trigger\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"Grip\",\n                            \"expectedControlLayout\": \"Axis1D\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/grip\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"Primary2DAxis\",\n                            \"expectedControlLayout\": \"Axis2D\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/thumbstick\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"Secondary2DAxis\",\n                            \"expectedControlLayout\": \"Axis2D\",\n                            \"bindings\": [\n                                {\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"RootPose\",\n                            \"expectedControlLayout\": \"Pose\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/devicePose\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"PointerPose\",\n                            \"expectedControlLayout\": \"Pose\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<XRController>{LeftHand}/pointer\"\n                                }\n                            ]\n                        }\n                    ]\n                }\n            ]}";

	[SerializeField]
	private InputActionMap _leftHandControllerBindings = InputActionMap.FromJson(ControllerActionMap).FirstOrDefault();

	[SerializeField]
	private InputActionMap _rightHandControllerBindings = InputActionMap.FromJson(ControllerActionMap.Replace("{LeftHand}", "{RightHand}")).FirstOrDefault();

	private readonly ControllerDataAsset _dataAsset = new ControllerDataAsset();

	private readonly ControllerDataSourceConfig _config = new ControllerDataSourceConfig();

	private static readonly Quaternion OpenXRToOVRLeftRotTipInverted = Quaternion.Inverse(Quaternion.AngleAxis(90f, Vector3.forward));

	private static readonly Quaternion OpenXRToOVRRightRotTipInverted = Quaternion.Inverse(Quaternion.AngleAxis(180f, Vector3.right) * Quaternion.AngleAxis(-90f, Vector3.forward));

	private static readonly Func<int> DefaultFrameCountProvider = () => Time.frameCount;

	private Func<int> _frameCountProvider = DefaultFrameCountProvider;

	private int _lastRequiredUpdate;

	protected override ControllerDataAsset DataAsset => _dataAsset;

	public void SetTimeFrameCountProvider(Func<int> frameCountProvider)
	{
		if (frameCountProvider == null)
		{
			frameCountProvider = DefaultFrameCountProvider;
		}
		_frameCountProvider = frameCountProvider;
	}

	private void Awake()
	{
		TrackingToWorldTransformer = _trackingToWorldTransformer as ITrackingToWorldTransformer;
		UpdateConfig();
		InputActionMap inputActionMap = ((_handedness == Handedness.Left) ? _leftHandControllerBindings : _rightHandControllerBindings);
		_dataAsset.IsDominantHand = _handedness != Handedness.Left;
		string[] names = Enum.GetNames(typeof(ControllerButtonUsage));
		foreach (string text in names)
		{
			if (!(text == ControllerButtonUsage.None.ToString()))
			{
				ControllerButtonUsage usage = Enum.Parse<ControllerButtonUsage>(text);
				inputActionMap[text].started += delegate
				{
					_dataAsset.Input.SetButton(usage, value: true);
				};
				inputActionMap[text].canceled += delegate
				{
					_dataAsset.Input.SetButton(usage, value: false);
				};
			}
		}
		names = Enum.GetNames(typeof(ControllerAxis1DUsage));
		foreach (string usageName2 in names)
		{
			if (!(usageName2 == ControllerAxis1DUsage.None.ToString()))
			{
				inputActionMap[usageName2].performed += delegate(InputAction.CallbackContext context)
				{
					_dataAsset.Input.SetAxis1D(Enum.Parse<ControllerAxis1DUsage>(usageName2), context.ReadValue<float>());
				};
				inputActionMap[usageName2].canceled += delegate
				{
					_dataAsset.Input.SetAxis1D(Enum.Parse<ControllerAxis1DUsage>(usageName2), 0f);
				};
			}
		}
		names = Enum.GetNames(typeof(ControllerAxis2DUsage));
		foreach (string usageName in names)
		{
			if (!(usageName == ControllerAxis2DUsage.None.ToString()))
			{
				inputActionMap[usageName].performed += delegate(InputAction.CallbackContext context)
				{
					_dataAsset.Input.SetAxis2D(Enum.Parse<ControllerAxis2DUsage>(usageName), context.ReadValue<Vector2>());
				};
				inputActionMap[usageName].canceled += delegate
				{
					_dataAsset.Input.SetAxis2D(Enum.Parse<ControllerAxis2DUsage>(usageName), Vector2.zero);
				};
			}
		}
		inputActionMap["RootPose"].performed += delegate(InputAction.CallbackContext context)
		{
			PoseState poseState2 = context.ReadValue<PoseState>();
			_dataAsset.RootPose = new Pose(poseState2.position, poseState2.rotation);
			_dataAsset.RootPose = FlipZ(_dataAsset.RootPose);
			_dataAsset.RootPose.rotation *= ((_dataAsset.Config.Handedness == Handedness.Left) ? OpenXRToOVRLeftRotTipInverted : OpenXRToOVRRightRotTipInverted);
			_dataAsset.RootPose = FlipZ(_dataAsset.RootPose);
			_dataAsset.RootPoseOrigin = PoseOrigin.RawTrackedPose;
			_dataAsset.IsTracked = poseState2.trackingState.HasFlag(InputTrackingState.Position) && poseState2.trackingState.HasFlag(InputTrackingState.Rotation);
			_dataAsset.IsDataValid = _dataAsset.IsTracked;
			_dataAsset.IsConnected = _dataAsset.IsTracked;
			int num = _frameCountProvider();
			if (_lastRequiredUpdate != num)
			{
				_lastRequiredUpdate = num;
				MarkInputDataRequiresUpdate();
			}
		};
		inputActionMap["RootPose"].canceled += delegate
		{
			_dataAsset.RootPoseOrigin = PoseOrigin.None;
		};
		inputActionMap["PointerPose"].performed += delegate(InputAction.CallbackContext context)
		{
			PoseState poseState = context.ReadValue<PoseState>();
			_dataAsset.PointerPose = new Pose(poseState.position, poseState.rotation);
			_dataAsset.PointerPoseOrigin = PoseOrigin.RawTrackedPose;
		};
		inputActionMap["PointerPose"].canceled += delegate
		{
			_dataAsset.PointerPoseOrigin = PoseOrigin.None;
		};
		inputActionMap.Enable();
	}

	private static Quaternion FlipZ(Quaternion q)
	{
		Quaternion result = default(Quaternion);
		result.x = 0f - q.x;
		result.y = 0f - q.y;
		result.z = q.z;
		result.w = q.w;
		return result;
	}

	public static Pose FlipZ(Pose p)
	{
		p.rotation = FlipZ(p.rotation);
		p.position = new Vector3
		{
			x = p.position.x,
			y = p.position.y,
			z = 0f - p.position.z
		};
		return p;
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

	protected override void UpdateData()
	{
	}

	private void UpdateConfig()
	{
		_config.Handedness = _handedness;
		_config.TrackingToWorldTransformer = TrackingToWorldTransformer;
		_dataAsset.Config = _config;
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
