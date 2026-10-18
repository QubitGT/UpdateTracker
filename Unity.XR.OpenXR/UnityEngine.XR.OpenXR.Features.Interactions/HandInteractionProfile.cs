using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class HandInteractionProfile : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Hand Interaction (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class HandInteraction : XRController
	{
		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "device", "gripPose" }, usage = "Device")]
		public PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, alias = "aimPose", usage = "Pointer")]
		public PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, usage = "Poke")]
		public PoseControl pokePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, usage = "Pinch")]
		public PoseControl pinchPose { get; private set; }

		[Preserve]
		[InputControl(usage = "PinchValue")]
		public AxisControl pinchValue { get; private set; }

		[Preserve]
		[InputControl(usage = "PinchTouched")]
		public ButtonControl pinchTouched { get; private set; }

		[Preserve]
		[InputControl(usage = "PinchReady")]
		public ButtonControl pinchReady { get; private set; }

		[Preserve]
		[InputControl(usage = "PointerActivateValue")]
		public AxisControl pointerActivateValue { get; private set; }

		[Preserve]
		[InputControl(usage = "PointerActivated")]
		public ButtonControl pointerActivated { get; private set; }

		[Preserve]
		[InputControl(usage = "PointerActivateReady")]
		public ButtonControl pointerActivateReady { get; private set; }

		[Preserve]
		[InputControl(usage = "GraspValue")]
		public AxisControl graspValue { get; private set; }

		[Preserve]
		[InputControl(usage = "GraspFirm")]
		public ButtonControl graspFirm { get; private set; }

		[Preserve]
		[InputControl(usage = "GraspReady")]
		public ButtonControl graspReady { get; private set; }

		[Preserve]
		[InputControl(offset = 2u)]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 4u)]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 8u, noisy = true, alias = "gripPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 20u, noisy = true, alias = "gripRotation")]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 68u, noisy = true)]
		public Vector3Control pointerPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 80u, noisy = true)]
		public QuaternionControl pointerRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 128u, noisy = true)]
		public Vector3Control pokePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 140u, noisy = true)]
		public QuaternionControl pokeRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 188u, noisy = true)]
		public Vector3Control pinchPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 200u, noisy = true)]
		public QuaternionControl pinchRotation { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			devicePose = GetChildControl<PoseControl>("devicePose");
			pointer = GetChildControl<PoseControl>("pointer");
			pokePose = GetChildControl<PoseControl>("pokePose");
			pinchPose = GetChildControl<PoseControl>("pinchPose");
			pinchValue = GetChildControl<AxisControl>("pinchValue");
			pinchTouched = GetChildControl<ButtonControl>("pinchTouched");
			pinchReady = GetChildControl<ButtonControl>("pinchReady");
			pointerActivateValue = GetChildControl<AxisControl>("pointerActivateValue");
			pointerActivated = GetChildControl<ButtonControl>("pointerActivated");
			pointerActivateReady = GetChildControl<ButtonControl>("pointerActivateReady");
			graspValue = GetChildControl<AxisControl>("graspValue");
			graspFirm = GetChildControl<ButtonControl>("graspFirm");
			graspReady = GetChildControl<ButtonControl>("graspReady");
			isTracked = GetChildControl<ButtonControl>("isTracked");
			trackingState = GetChildControl<IntegerControl>("trackingState");
			devicePosition = GetChildControl<Vector3Control>("devicePosition");
			deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
			pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
			pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
			pokePosition = GetChildControl<Vector3Control>("pokePosition");
			pokeRotation = GetChildControl<QuaternionControl>("pokeRotation");
			pinchPosition = GetChildControl<Vector3Control>("pinchPosition");
			pinchRotation = GetChildControl<QuaternionControl>("pinchRotation");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.handinteraction";

	public const string profile = "/interaction_profiles/ext/hand_interaction_ext";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string poke = "/input/poke_ext/pose";

	public const string pinch = "/input/pinch_ext/pose";

	public const string pinchValue = "/input/pinch_ext/value";

	public const string pinchReady = "/input/pinch_ext/ready_ext";

	public const string pointerActivateValue = "/input/aim_activate_ext/value";

	public const string pointerActivateReady = "/input/aim_activate_ext/ready_ext";

	public const string graspValue = "/input/grasp_ext/value";

	public const string graspReady = "/input/grasp_ext/ready_ext";

	private const string kDeviceLocalizedName = "Hand Interaction OpenXR";

	public const string extensionString = "XR_EXT_hand_interaction";

	protected internal override bool OnInstanceCreate(ulong instance)
	{
		if (!OpenXRRuntime.IsExtensionEnabled("XR_EXT_hand_interaction"))
		{
			return false;
		}
		return base.OnInstanceCreate(instance);
	}

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(HandInteraction), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Hand Interaction OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("HandInteraction");
	}

	protected override string GetDeviceLayoutName()
	{
		return "HandInteraction";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "handinteraction",
			localizedName = "Hand Interaction OpenXR",
			desiredInteractionProfile = "/interaction_profiles/ext/hand_interaction_ext",
			manufacturer = "",
			serialNumber = "",
			deviceInfos = new List<DeviceConfig>
			{
				new DeviceConfig
				{
					characteristics = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Left),
					userPath = "/user/hand/left"
				},
				new DeviceConfig
				{
					characteristics = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Right),
					userPath = "/user/hand/right"
				}
			},
			actions = new List<ActionConfig>
			{
				new ActionConfig
				{
					name = "devicePose",
					localizedName = "Grip Pose",
					type = ActionType.Pose,
					usages = new List<string> { "Device" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/grip/pose",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "pointer",
					localizedName = "Pointer Pose",
					type = ActionType.Pose,
					usages = new List<string> { "Pointer" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/aim/pose",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "PokePose",
					localizedName = "Poke Pose",
					type = ActionType.Pose,
					usages = new List<string> { "Poke" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/poke_ext/pose",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "PinchPose",
					localizedName = "Pinch Pose",
					type = ActionType.Pose,
					usages = new List<string> { "Pinch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/pinch_ext/pose",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "PinchValue",
					localizedName = "Pinch Value",
					type = ActionType.Axis1D,
					usages = new List<string> { "PinchValue" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/pinch_ext/value",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "PinchTouched",
					localizedName = "Pinch Touched",
					type = ActionType.Binary,
					usages = new List<string> { "PinchTouched" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/pinch_ext/value",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "PinchReady",
					localizedName = "Pinch Ready",
					type = ActionType.Binary,
					usages = new List<string> { "PinchReady" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/pinch_ext/ready_ext",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "PointerActivateValue",
					localizedName = "Pointer Activate Value",
					type = ActionType.Axis1D,
					usages = new List<string> { "PointerActivateValue" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/aim_activate_ext/value",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "PointerActivated",
					localizedName = "Pointer Activated",
					type = ActionType.Binary,
					usages = new List<string> { "PointerActivated" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/aim_activate_ext/value",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "PointerActivateReady",
					localizedName = "Pointer Activate Ready",
					type = ActionType.Binary,
					usages = new List<string> { "PointerActivateReady" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/aim_activate_ext/ready_ext",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "GraspValue",
					localizedName = "Grasp Value",
					type = ActionType.Axis1D,
					usages = new List<string> { "GraspValue" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/grasp_ext/value",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "GraspFirm",
					localizedName = "Grasp Firm",
					type = ActionType.Binary,
					usages = new List<string> { "GraspFirm" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/grasp_ext/value",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				},
				new ActionConfig
				{
					name = "GraspReady",
					localizedName = "Grasp Ready",
					type = ActionType.Binary,
					usages = new List<string> { "GraspReady" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/grasp_ext/ready_ext",
							interactionProfileName = "/interaction_profiles/ext/hand_interaction_ext"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
