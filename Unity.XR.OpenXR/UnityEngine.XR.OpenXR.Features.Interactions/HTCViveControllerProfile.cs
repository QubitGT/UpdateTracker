using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class HTCViveControllerProfile : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "HTC Vive Controller (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class ViveController : XRControllerWithRumble
	{
		[Preserve]
		[InputControl(aliases = new string[] { "Secondary", "selectbutton" }, usage = "SystemButton")]
		public ButtonControl select { get; private set; }

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
		[InputControl(alias = "triggeraxis", usage = "Trigger")]
		public AxisControl trigger { get; private set; }

		[Preserve]
		[InputControl(alias = "triggerbutton", usage = "TriggerButton")]
		public ButtonControl triggerPressed { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "Primary2DAxis", "touchpadaxes", "touchpad" }, usage = "Primary2DAxis")]
		public Vector2Control trackpad { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "joystickorpadpressed", "touchpadpressed" }, usage = "Primary2DAxisClick")]
		public ButtonControl trackpadClicked { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "joystickorpadtouched", "touchpadtouched" }, usage = "Primary2DAxisTouch")]
		public ButtonControl trackpadTouched { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "device", "gripPose" }, usage = "Device")]
		public UnityEngine.InputSystem.XR.PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, alias = "aimPose", usage = "Pointer")]
		public UnityEngine.InputSystem.XR.PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 26u)]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 28u)]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 32u, alias = "gripPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 44u, alias = "gripOrientation")]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 92u)]
		public Vector3Control pointerPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 104u, alias = "pointerOrientation")]
		public QuaternionControl pointerRotation { get; private set; }

		[Preserve]
		[InputControl(usage = "Haptic")]
		public HapticControl haptic { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			select = GetChildControl<ButtonControl>("select");
			grip = GetChildControl<AxisControl>("grip");
			gripPressed = GetChildControl<ButtonControl>("gripPressed");
			menu = GetChildControl<ButtonControl>("menu");
			trigger = GetChildControl<AxisControl>("trigger");
			triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
			trackpad = GetChildControl<StickControl>("trackpad");
			trackpadClicked = GetChildControl<ButtonControl>("trackpadClicked");
			trackpadTouched = GetChildControl<ButtonControl>("trackpadTouched");
			pointer = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("pointer");
			pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
			pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
			devicePose = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("devicePose");
			isTracked = GetChildControl<ButtonControl>("isTracked");
			trackingState = GetChildControl<IntegerControl>("trackingState");
			devicePosition = GetChildControl<Vector3Control>("devicePosition");
			deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
			haptic = GetChildControl<HapticControl>("haptic");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.htcvive";

	public const string profile = "/interaction_profiles/htc/vive_controller";

	public const string system = "/input/system/click";

	public const string squeeze = "/input/squeeze/click";

	public const string menu = "/input/menu/click";

	public const string trigger = "/input/trigger/value";

	public const string triggerClick = "/input/trigger/click";

	public const string trackpad = "/input/trackpad";

	public const string trackpadClick = "/input/trackpad/click";

	public const string trackpadTouch = "/input/trackpad/touch";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string haptic = "/output/haptic";

	private const string kDeviceLocalizedName = "HTC Vive Controller OpenXR";

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(ViveController), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("HTC Vive Controller OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("ViveController");
	}

	protected override string GetDeviceLayoutName()
	{
		return "ViveController";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "htcvivecontroller",
			localizedName = "HTC Vive Controller OpenXR",
			desiredInteractionProfile = "/interaction_profiles/htc/vive_controller",
			manufacturer = "HTC",
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
					name = "grip",
					localizedName = "Grip",
					type = ActionType.Axis1D,
					usages = new List<string> { "Grip" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/squeeze/click",
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
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
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
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
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "select",
					localizedName = "Select",
					type = ActionType.Binary,
					usages = new List<string> { "SystemButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/system/click",
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
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
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
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
							interactionPath = "/input/trigger/click",
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "trackpad",
					localizedName = "Trackpad",
					type = ActionType.Axis2D,
					usages = new List<string> { "Primary2DAxis" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad",
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "trackpadTouched",
					localizedName = "Trackpad Touched",
					type = ActionType.Binary,
					usages = new List<string> { "Primary2DAxisTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/touch",
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
						}
					}
				},
				new ActionConfig
				{
					name = "trackpadClicked",
					localizedName = "Trackpad Clicked",
					type = ActionType.Binary,
					usages = new List<string> { "Primary2DAxisClick" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/click",
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
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
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
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
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
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
							interactionProfileName = "/interaction_profiles/htc/vive_controller"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
