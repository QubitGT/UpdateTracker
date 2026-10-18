using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class MicrosoftMotionControllerProfile : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Windows MR Controller (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class WMRSpatialController : XRControllerWithRumble
	{
		[Preserve]
		[InputControl(aliases = new string[] { "Primary2DAxis", "thumbstickaxes", "thumbstick" }, usage = "Primary2DAxis")]
		public Vector2Control joystick { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "Secondary2DAxis", "touchpadaxes", "trackpad" }, usage = "Secondary2DAxis")]
		public Vector2Control touchpad { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripAxis", "squeeze" }, usage = "Grip")]
		public AxisControl grip { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripButton", "squeezeClicked" }, usage = "GripButton")]
		public ButtonControl gripPressed { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "Primary", "menubutton" }, usage = "MenuButton")]
		public ButtonControl menu { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "triggeraxis" }, usage = "Trigger")]
		public AxisControl trigger { get; private set; }

		[Preserve]
		[InputControl(alias = "triggerbutton", usage = "TriggerButton")]
		public ButtonControl triggerPressed { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "joystickClicked", "thumbstickpressed" }, usage = "Primary2DAxisClick")]
		public ButtonControl joystickClicked { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "joystickorpadpressed", "touchpadpressed", "trackpadClicked" }, usage = "Secondary2DAxisClick")]
		public ButtonControl touchpadClicked { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "joystickorpadtouched", "touchpadtouched", "trackpadTouched" }, usage = "Secondary2DAxisTouch")]
		public ButtonControl touchpadTouched { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "device", "gripPose" }, usage = "Device")]
		public UnityEngine.InputSystem.XR.PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "aimPose" }, usage = "Pointer")]
		public UnityEngine.InputSystem.XR.PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 32u)]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 36u)]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 40u, aliases = new string[] { "gripPosition" })]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 52u, aliases = new string[] { "gripOrientation" })]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 100u)]
		public Vector3Control pointerPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 112u, aliases = new string[] { "pointerOrientation" })]
		public QuaternionControl pointerRotation { get; private set; }

		[Preserve]
		[InputControl(usage = "Haptic")]
		public HapticControl haptic { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			joystick = GetChildControl<StickControl>("joystick");
			trigger = GetChildControl<AxisControl>("trigger");
			touchpad = GetChildControl<StickControl>("touchpad");
			grip = GetChildControl<AxisControl>("grip");
			gripPressed = GetChildControl<ButtonControl>("gripPressed");
			menu = GetChildControl<ButtonControl>("menu");
			joystickClicked = GetChildControl<ButtonControl>("joystickClicked");
			triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
			touchpadClicked = GetChildControl<ButtonControl>("touchpadClicked");
			touchpadTouched = GetChildControl<ButtonControl>("touchPadTouched");
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

	public const string featureId = "com.unity.openxr.feature.input.microsoftmotioncontroller";

	public const string profile = "/interaction_profiles/microsoft/motion_controller";

	public const string menu = "/input/menu/click";

	public const string squeeze = "/input/squeeze/click";

	public const string trigger = "/input/trigger/value";

	public const string thumbstick = "/input/thumbstick";

	public const string thumbstickClick = "/input/thumbstick/click";

	public const string trackpad = "/input/trackpad";

	public const string trackpadClick = "/input/trackpad/click";

	public const string trackpadTouch = "/input/trackpad/touch";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string haptic = "/output/haptic";

	private const string kDeviceLocalizedName = "Windows MR Controller OpenXR";

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(WMRSpatialController), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Windows MR Controller OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("WMRSpatialController");
	}

	protected override string GetDeviceLayoutName()
	{
		return "WMRSpatialController";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "microsoftmotioncontroller",
			localizedName = "Windows MR Controller OpenXR",
			desiredInteractionProfile = "/interaction_profiles/microsoft/motion_controller",
			manufacturer = "Microsoft",
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
					name = "joystick",
					localizedName = "Joystick",
					type = ActionType.Axis2D,
					usages = new List<string> { "Primary2DAxis" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick",
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "touchpad",
					localizedName = "Touchpad",
					type = ActionType.Axis2D,
					usages = new List<string> { "Secondary2DAxis" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad",
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
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
							interactionPath = "/input/squeeze/click",
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
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
							interactionPath = "/input/squeeze/click",
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
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
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
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
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
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
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "joystickClicked",
					localizedName = "JoystickClicked",
					type = ActionType.Binary,
					usages = new List<string> { "Primary2DAxisClick" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick/click",
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "touchpadClicked",
					localizedName = "Touchpad Clicked",
					type = ActionType.Binary,
					usages = new List<string> { "Secondary2DAxisClick" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/click",
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "touchpadTouched",
					localizedName = "Touchpad Touched",
					type = ActionType.Binary,
					usages = new List<string> { "Secondary2DAxisTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/touch",
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
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
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
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
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
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
							interactionProfileName = "/interaction_profiles/microsoft/motion_controller"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
