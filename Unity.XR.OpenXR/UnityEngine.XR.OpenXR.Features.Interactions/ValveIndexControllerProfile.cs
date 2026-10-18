using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class ValveIndexControllerProfile : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Index Controller (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class ValveIndexController : XRControllerWithRumble
	{
		[Preserve]
		[InputControl(alias = "systemButton", usage = "MenuButton")]
		public ButtonControl system { get; private set; }

		[Preserve]
		[InputControl(usage = "MenuTouch")]
		public ButtonControl systemTouched { get; private set; }

		[Preserve]
		[InputControl(usage = "PrimaryButton")]
		public ButtonControl primaryButton { get; private set; }

		[Preserve]
		[InputControl(usage = "PrimaryTouch")]
		public ButtonControl primaryTouched { get; private set; }

		[Preserve]
		[InputControl(usage = "SecondaryButton")]
		public ButtonControl secondaryButton { get; private set; }

		[Preserve]
		[InputControl(usage = "SecondaryTouch")]
		public ButtonControl secondaryTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripAxis", "squeeze" }, usage = "Grip")]
		public AxisControl grip { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripButton", "squeezeClicked" }, usage = "GripButton")]
		public ButtonControl gripPressed { get; private set; }

		[Preserve]
		[InputControl(alias = "squeezeForce", usage = "GripForce")]
		public AxisControl gripForce { get; private set; }

		[Preserve]
		[InputControl(usage = "Trigger")]
		public AxisControl trigger { get; private set; }

		[Preserve]
		[InputControl(usage = "TriggerButton")]
		public ButtonControl triggerPressed { get; private set; }

		[Preserve]
		[InputControl(usage = "TriggerTouch")]
		public ButtonControl triggerTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "joystick", "Primary2DAxis" }, usage = "Primary2DAxis")]
		public Vector2Control thumbstick { get; private set; }

		[Preserve]
		[InputControl(alias = "joystickClicked", usage = "Primary2DAxisClick")]
		public ButtonControl thumbstickClicked { get; private set; }

		[Preserve]
		[InputControl(alias = "joystickTouched", usage = "Primary2DAxisTouch")]
		public ButtonControl thumbstickTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "touchpad", "Secondary2DAxis" }, usage = "Secondary2DAxis")]
		public Vector2Control trackpad { get; private set; }

		[Preserve]
		[InputControl(alias = "touchpadTouched", usage = "Secondary2DAxisTouch")]
		public ButtonControl trackpadTouched { get; private set; }

		[Preserve]
		[InputControl(alias = "touchpadForce", usage = "Secondary2DAxisForce")]
		public AxisControl trackpadForce { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "device", "gripPose" }, usage = "Device")]
		public UnityEngine.InputSystem.XR.PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, alias = "aimPose", usage = "Pointer")]
		public UnityEngine.InputSystem.XR.PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 53u)]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 56u)]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 60u, alias = "gripPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 72u, alias = "gripOrientation")]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 120u)]
		public Vector3Control pointerPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 132u, alias = "pointerOrientation")]
		public QuaternionControl pointerRotation { get; private set; }

		[Preserve]
		[InputControl(usage = "Haptic")]
		public HapticControl haptic { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			system = GetChildControl<ButtonControl>("system");
			systemTouched = GetChildControl<ButtonControl>("systemTouched");
			primaryButton = GetChildControl<ButtonControl>("primaryButton");
			primaryTouched = GetChildControl<ButtonControl>("primaryTouched");
			secondaryButton = GetChildControl<ButtonControl>("secondaryButton");
			secondaryTouched = GetChildControl<ButtonControl>("secondaryTouched");
			grip = GetChildControl<AxisControl>("grip");
			gripPressed = GetChildControl<ButtonControl>("gripPressed");
			gripForce = GetChildControl<AxisControl>("gripForce");
			trigger = GetChildControl<AxisControl>("trigger");
			triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
			triggerTouched = GetChildControl<ButtonControl>("triggerTouched");
			thumbstick = GetChildControl<StickControl>("thumbstick");
			thumbstickClicked = GetChildControl<ButtonControl>("thumbstickClicked");
			thumbstickTouched = GetChildControl<ButtonControl>("thumbstickTouched");
			trackpad = GetChildControl<StickControl>("trackpad");
			trackpadTouched = GetChildControl<ButtonControl>("trackpadTouched");
			trackpadForce = GetChildControl<AxisControl>("trackpadForce");
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

	public const string featureId = "com.unity.openxr.feature.input.valveindex";

	public const string profile = "/interaction_profiles/valve/index_controller";

	public const string system = "/input/system/click";

	public const string systemTouch = "/input/system/touch";

	public const string buttonA = "/input/a/click";

	public const string buttonATouch = "/input/a/touch";

	public const string buttonB = "/input/b/click";

	public const string buttonBTouch = "/input/b/touch";

	public const string squeeze = "/input/squeeze/value";

	public const string squeezeForce = "/input/squeeze/force";

	public const string triggerClick = "/input/trigger/click";

	public const string trigger = "/input/trigger/value";

	public const string triggerTouch = "/input/trigger/touch";

	public const string thumbstick = "/input/thumbstick";

	public const string thumbstickClick = "/input/thumbstick/click";

	public const string thumbstickTouch = "/input/thumbstick/touch";

	public const string trackpad = "/input/trackpad";

	public const string trackpadForce = "/input/trackpad/force";

	public const string trackpadTouch = "/input/trackpad/touch";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string haptic = "/output/haptic";

	private const string kDeviceLocalizedName = "Index Controller OpenXR";

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(ValveIndexController), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Index Controller OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("ValveIndexController");
	}

	protected override string GetDeviceLayoutName()
	{
		return "ValveIndexController";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "valveindexcontroller",
			localizedName = "Index Controller OpenXR",
			desiredInteractionProfile = "/interaction_profiles/valve/index_controller",
			manufacturer = "Valve",
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
					name = "system",
					localizedName = "System",
					type = ActionType.Binary,
					usages = new List<string> { "MenuButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/system/click",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "systemTouched",
					localizedName = "System Touched",
					type = ActionType.Binary,
					usages = new List<string> { "MenuTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/system/touch",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
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
							interactionPath = "/input/a/click",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "primaryTouched",
					localizedName = "Primary Touched",
					type = ActionType.Binary,
					usages = new List<string> { "PrimaryTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/a/touch",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionPath = "/input/b/click",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "secondaryTouched",
					localizedName = "Secondary Touched",
					type = ActionType.Binary,
					usages = new List<string> { "SecondaryTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/b/touch",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "gripForce",
					localizedName = "Grip Force",
					type = ActionType.Axis1D,
					usages = new List<string> { "GripForce" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/squeeze/force",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "triggerPressed",
					localizedName = "Triggger Pressed",
					type = ActionType.Binary,
					usages = new List<string> { "TriggerButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trigger/click",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "triggerTouched",
					localizedName = "Trigger Touched",
					type = ActionType.Binary,
					usages = new List<string> { "TriggerTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trigger/touch",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "thumbstickTouched",
					localizedName = "Thumbstick Touched",
					type = ActionType.Binary,
					usages = new List<string> { "Primary2DAxisTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick/touch",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "trackpad",
					localizedName = "Trackpad",
					type = ActionType.Axis2D,
					usages = new List<string> { "Secondary2DAxis" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "trackpadForce",
					localizedName = "Trackpad Force",
					type = ActionType.Axis1D,
					usages = new List<string> { "Secondary2DAxisForce" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/force",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "trackpadTouched",
					localizedName = "Trackpad Touched",
					type = ActionType.Binary,
					usages = new List<string> { "Secondary2DAxisTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/touch",
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionProfileName = "/interaction_profiles/valve/index_controller"
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
							interactionProfileName = "/interaction_profiles/valve/index_controller"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
