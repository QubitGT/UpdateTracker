using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Oculus.Interaction.Input.UnityXR;

public class FromUnityXRHandDataSource : FromOpenXRHandDataSource
{
	[Header("Shared Configuration")]
	[SerializeField]
	private Handedness _handedness;

	[SerializeField]
	[Interface(typeof(ITrackingToWorldTransformer), new Type[] { })]
	private UnityEngine.Object _trackingToWorldTransformer;

	private ITrackingToWorldTransformer TrackingToWorldTransformer;

	private static string _metaAimHandActionMap = "{\n            \"maps\": [\n                {\n                    \"name\": \"MetaAimHand\",\n                    \"actions\": [\n                        {\n                            \"name\": \"aimFlags\",\n                            \"expectedControlLayout\": \"Integer\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<MetaAimHand>{LeftHand}/aimFlags\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"pinchStrengthIndex\",\n                            \"expectedControlLayout\": \"Axis\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<MetaAimHand>{LeftHand}/pinchStrengthIndex\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"pinchStrengthMiddle\",\n                            \"expectedControlLayout\": \"Axis\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<MetaAimHand>{LeftHand}/pinchStrengthMiddle\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"pinchStrengthRing\",\n                            \"expectedControlLayout\": \"Axis\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<MetaAimHand>{LeftHand}/pinchStrengthRing\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"pinchStrengthLittle\",\n                            \"expectedControlLayout\": \"Axis\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<MetaAimHand>{LeftHand}/pinchStrengthLittle\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"devicePosition\",\n                            \"expectedControlLayout\": \"Vector3\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<MetaAimHand>{LeftHand}/devicePosition\"\n                                }\n                            ]\n                        },\n                        {\n                            \"name\": \"deviceRotation\",\n                            \"expectedControlLayout\": \"Quaternion\",\n                            \"bindings\": [\n                                {\n                                    \"path\":\"<MetaAimHand>{LeftHand}/deviceRotation\"\n                                }\n                            ]\n                        }\n                    ]\n                }\n            ]}";

	[SerializeField]
	private InputActionMap _metaAimHandBindingsLeft = InputActionMap.FromJson(_metaAimHandActionMap).FirstOrDefault();

	[SerializeField]
	private InputActionMap _metaAimHandBindingsRight = InputActionMap.FromJson(_metaAimHandActionMap.Replace("{LeftHand}", "{RightHand}")).FirstOrDefault();

	private HandDataSourceConfig _config;

	private InputAction _metaAimFlags;

	private InputAction _pinchStrengthIndex;

	private InputAction _pinchStrengthMiddle;

	private InputAction _pinchStrengthRing;

	private InputAction _pinchStrengthLittle;

	private InputAction _devicePosition;

	private InputAction _deviceRotation;

	private InputActionMap MetaAimHandBindings
	{
		get
		{
			if (_handedness != 0)
			{
				return _metaAimHandBindingsRight;
			}
			return _metaAimHandBindingsLeft;
		}
	}

	private HandDataSourceConfig Config
	{
		get
		{
			if (_config != null)
			{
				return _config;
			}
			_config = new HandDataSourceConfig
			{
				Handedness = _handedness
			};
			return _config;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		TrackingToWorldTransformer = _trackingToWorldTransformer as ITrackingToWorldTransformer;
		UpdateConfig();
	}

	protected override void Start()
	{
		base.Start();
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		UpdateConfig();
		InputActionMap metaAimHandBindings = MetaAimHandBindings;
		_metaAimFlags = metaAimHandBindings["aimFlags"];
		_pinchStrengthIndex = metaAimHandBindings["pinchStrengthIndex"];
		_pinchStrengthMiddle = metaAimHandBindings["pinchStrengthMiddle"];
		_pinchStrengthRing = metaAimHandBindings["pinchStrengthRing"];
		_pinchStrengthLittle = metaAimHandBindings["pinchStrengthLittle"];
		_devicePosition = metaAimHandBindings["devicePosition"];
		_deviceRotation = metaAimHandBindings["deviceRotation"];
		this.EndStart(ref _started);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		MetaAimHandBindings.Enable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		MetaAimHandBindings.Disable();
	}

	private void UpdateConfig()
	{
		Config.TrackingToWorldTransformer = TrackingToWorldTransformer;
		Config.HandSkeleton = ((_handedness == Handedness.Left) ? HandSkeleton.DefaultLeftSkeleton : HandSkeleton.DefaultRightSkeleton);
		_dataAsset.Config = Config;
	}

	public void InjectTrackingToWorldTransformer(ITrackingToWorldTransformer trackingToWorldTransformer)
	{
		_trackingToWorldTransformer = trackingToWorldTransformer as UnityEngine.Object;
		TrackingToWorldTransformer = trackingToWorldTransformer;
		UpdateConfig();
	}
}
