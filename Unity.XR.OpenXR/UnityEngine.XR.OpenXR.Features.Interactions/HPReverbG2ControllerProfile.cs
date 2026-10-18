using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class HPReverbG2ControllerProfile : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "HP Reverb G2 Controller (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class ReverbG2Controller : XRControllerWithRumble
	{
		[Preserve]
		[InputControl(aliases = new string[] { "A", "X", "buttonA", "buttonX" }, usage = "PrimaryButton")]
		public ButtonControl primaryButton { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "B", "Y", "buttonB", "buttonY" }, usage = "SecondaryButton")]
		public ButtonControl secondaryButton { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "Primary", "menubutton" }, usage = "MenuButton")]
		public ButtonControl menu { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripAxis", "squeeze" }, usage = "Grip")]
		public AxisControl grip { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripButton", "squeezeClicked" }, usage = "GripButton")]
		public ButtonControl gripPressed { get; private set; }

		[Preserve]
		[InputControl(usage = "Trigger")]
		public AxisControl trigger { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "indexButton", "indexTouched", "triggerbutton" }, usage = "TriggerButton")]
		public ButtonControl triggerPressed { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "Primary2DAxis", "Joystick" }, usage = "Primary2DAxis")]
		public Vector2Control thumbstick { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "JoystickOrPadPressed", "thumbstickClick", "joystickClicked" }, usage = "Primary2DAxisClick")]
		public ButtonControl thumbstickClicked { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "device", "gripPose" }, usage = "Device")]
		public UnityEngine.InputSystem.XR.PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, alias = "aimPose", usage = "Pointer")]
		public UnityEngine.InputSystem.XR.PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 29u)]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 32u)]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 36u, alias = "gripPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 48u, alias = "gripOrientation")]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 96u)]
		public Vector3Control pointerPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 108u, alias = "pointerOrientation")]
		public QuaternionControl pointerRotation { get; private set; }

		[Preserve]
		[InputControl(usage = "Haptic")]
		public HapticControl haptic { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			primaryButton = GetChildControl<ButtonControl>("primaryButton");
			secondaryButton = GetChildControl<ButtonControl>("secondaryButton");
			menu = GetChildControl<ButtonControl>("menu");
			grip = GetChildControl<AxisControl>("grip");
			gripPressed = GetChildControl<ButtonControl>("gripPressed");
			trigger = GetChildControl<AxisControl>("trigger");
			triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
			thumbstick = GetChildControl<StickControl>("thumbstick");
			thumbstickClicked = GetChildControl<ButtonControl>("thumbstickClicked");
			devicePose = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("devicePose");
			pointer = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("pointer");
			isTracked = GetChildControl<ButtonControl>("isTracked");
			trackingState = GetChildControl<IntegerControl>("trackingState");
			devicePosition = GetChildControl<Vector3Control>("devicePosition");
			deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
			pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
			pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
			haptic = GetChildControl<HapticControl>("haptic");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.hpreverb";

	public const string profile = "/interaction_profiles/hp/mixed_reality_controller";

	public const string buttonX = "/input/x/click";

	public const string buttonY = "/input/y/click";

	public const string buttonA = "/input/a/click";

	public const string buttonB = "/input/b/click";

	public const string menu = "/input/menu/click";

	public const string squeeze = "/input/squeeze/value";

	public const string trigger = "/input/trigger/value";

	public const string thumbstick = "/input/thumbstick";

	public const string thumbstickClick = "/input/thumbstick/click";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string haptic = "/output/haptic";

	private const string kDeviceLocalizedName = "HP Reverb G2 Controller OpenXR";

	protected internal override bool OnInstanceCreate(ulong instance)
	{
		if (!OpenXRRuntime.IsExtensionEnabled("XR_EXT_hp_mixed_reality_controller"))
		{
			return false;
		}
		return base.OnInstanceCreate(instance);
	}

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(ReverbG2Controller), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("HP Reverb G2 Controller OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("ReverbG2Controller");
	}

	protected override string GetDeviceLayoutName()
	{
		return "ReverbG2Controller";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "hpreverbg2controller",
			localizedName = "HP Reverb G2 Controller OpenXR",
			desiredInteractionProfile = "/interaction_profiles/hp/mixed_reality_controller",
			manufacturer = "HP",
			serialNumber = "",
			deviceInfos = new List<DeviceConfig>
			{
				new DeviceConfig
				{
					characteristics = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left),
					userPath = "/user/hand/left"
				},
				new DeviceConfig
				{
					characteristics = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right),
					userPath = "/user/hand/right"
				}
			},
			actions = new List<ActionConfig>
			{
				new ActionConfig
				{
					name = "primaryButton",
					localizedName = "Primary Button",
					type = ActionType.Binary,
					usages = new List<string> { "PrimaryButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/x/click",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller",
							userPaths = new List<string> { "/user/hand/left" }
						},
						new ActionBinding
						{
							interactionPath = "/input/a/click",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller",
							userPaths = new List<string> { "/user/hand/right" }
						}
					}
				},
				new ActionConfig
				{
					name = "secondaryButton",
					localizedName = "Secondary Button",
					type = ActionType.Binary,
					usages = new List<string> { "SecondaryButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/y/click",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller",
							userPaths = new List<string> { "/user/hand/left" }
						},
						new ActionBinding
						{
							interactionPath = "/input/b/click",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller",
							userPaths = new List<string> { "/user/hand/right" }
						}
					}
				},
				new ActionConfig
				{
					name = "menu",
					localizedName = "Menu",
					type = ActionType.Binary,
					usages = new List<string> { "MenuButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/menu/click",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "grip",
					localizedName = "Grip",
					type = ActionType.Axis1D,
					usages = new List<string> { "Grip" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/squeeze/value",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "gripPressed",
					localizedName = "Grip Pressed",
					type = ActionType.Binary,
					usages = new List<string> { "GripButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/squeeze/value",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "trigger",
					localizedName = "Trigger",
					type = ActionType.Axis1D,
					usages = new List<string> { "Trigger" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trigger/value",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "triggerPressed",
					localizedName = "Trigger Pressed",
					type = ActionType.Binary,
					usages = new List<string> { "TriggerButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trigger/value",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "thumbstick",
					localizedName = "Thumbstick",
					type = ActionType.Axis2D,
					usages = new List<string> { "Primary2DAxis" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "thumbstickClicked",
					localizedName = "Thumbstick Clicked",
					type = ActionType.Binary,
					usages = new List<string> { "Primary2DAxisClick" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick/click",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "devicePose",
					localizedName = "Device Pose",
					type = ActionType.Pose,
					usages = new List<string> { "Device" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/grip/pose",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
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
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "haptic",
					localizedName = "Haptic Output",
					type = ActionType.Vibrate,
					usages = new List<string> { "Haptic" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/output/haptic",
							interactionProfileName = "/interaction_profiles/hp/mixed_reality_controller"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
