using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class MetaQuestTouchPlusControllerProfile : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Meta Quest Touch Plus Controller(OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class QuestTouchPlusController : XRControllerWithRumble
	{
		[Preserve]
		[InputControl(aliases = new string[] { "Primary2DAxis", "Joystick" }, usage = "Primary2DAxis")]
		public Vector2Control thumbstick { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripAxis", "squeeze" }, usage = "Grip")]
		public AxisControl grip { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripButton", "squeezeClicked" }, usage = "GripButton")]
		public ButtonControl gripPressed { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "Primary", "menuButton", "systemButton" }, usage = "MenuButton")]
		public ButtonControl menu { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "A", "X", "buttonA", "buttonX" }, usage = "PrimaryButton")]
		public ButtonControl primaryButton { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "ATouched", "XTouched", "ATouch", "XTouch", "buttonATouched", "buttonXTouched" }, usage = "PrimaryTouch")]
		public ButtonControl primaryTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "B", "Y", "buttonB", "buttonY" }, usage = "SecondaryButton")]
		public ButtonControl secondaryButton { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "BTouched", "YTouched", "BTouch", "YTouch", "buttonBTouched", "buttonYTouched" }, usage = "SecondaryTouch")]
		public ButtonControl secondaryTouched { get; private set; }

		[Preserve]
		[InputControl(usage = "Trigger")]
		public AxisControl trigger { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "indexButton", "indexTouched", "triggerbutton" }, usage = "TriggerButton")]
		public ButtonControl triggerPressed { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "indexTouch", "indexNearTouched" }, usage = "TriggerTouch")]
		public ButtonControl triggerTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "JoystickOrPadPressed", "thumbstickClick", "joystickClicked" }, usage = "Primary2DAxisClick")]
		public ButtonControl thumbstickClicked { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "JoystickOrPadTouched", "thumbstickTouch", "joystickTouched" }, usage = "Primary2DAxisTouch")]
		public ButtonControl thumbstickTouched { get; private set; }

		[Preserve]
		[InputControl(usage = "ThumbrestTouch")]
		public ButtonControl thumbrestTouched { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "device", "gripPose" }, usage = "Device")]
		public UnityEngine.InputSystem.XR.PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, alias = "aimPose", usage = "Pointer")]
		public UnityEngine.InputSystem.XR.PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 28u, usage = "IsTracked")]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 32u, usage = "TrackingState")]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 36u, noisy = true, alias = "gripPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 48u, noisy = true, alias = "gripOrientation")]
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

		[Preserve]
		[InputControl(usage = "TriggerForce")]
		public AxisControl triggerForce { get; private set; }

		[Preserve]
		[InputControl(usage = "TriggerCurl")]
		public AxisControl triggerCurl { get; private set; }

		[Preserve]
		[InputControl(usage = "TriggerSlide")]
		public AxisControl triggerSlide { get; private set; }

		[Preserve]
		[InputControl(usage = "TriggerProximity")]
		public ButtonControl triggerProximity { get; private set; }

		[Preserve]
		[InputControl(usage = "ThumbProximity")]
		public ButtonControl thumbProximity { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			thumbstick = GetChildControl<StickControl>("thumbstick");
			trigger = GetChildControl<AxisControl>("trigger");
			triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
			triggerTouched = GetChildControl<ButtonControl>("triggerTouched");
			grip = GetChildControl<AxisControl>("grip");
			gripPressed = GetChildControl<ButtonControl>("gripPressed");
			menu = GetChildControl<ButtonControl>("menu");
			primaryButton = GetChildControl<ButtonControl>("primaryButton");
			primaryTouched = GetChildControl<ButtonControl>("primaryTouched");
			secondaryButton = GetChildControl<ButtonControl>("secondaryButton");
			secondaryTouched = GetChildControl<ButtonControl>("secondaryTouched");
			thumbstickClicked = GetChildControl<ButtonControl>("thumbstickClicked");
			thumbstickTouched = GetChildControl<ButtonControl>("thumbstickTouched");
			thumbrestTouched = GetChildControl<ButtonControl>("thumbrestTouched");
			devicePose = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("devicePose");
			pointer = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("pointer");
			isTracked = GetChildControl<ButtonControl>("isTracked");
			trackingState = GetChildControl<IntegerControl>("trackingState");
			devicePosition = GetChildControl<Vector3Control>("devicePosition");
			deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
			pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
			pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
			haptic = GetChildControl<HapticControl>("haptic");
			triggerForce = GetChildControl<AxisControl>("triggerForce");
			triggerCurl = GetChildControl<AxisControl>("triggerCurl");
			triggerSlide = GetChildControl<AxisControl>("triggerSlide");
			triggerProximity = GetChildControl<ButtonControl>("triggerProximity");
			thumbProximity = GetChildControl<ButtonControl>("thumbProximity");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.metaquestplus";

	public const string profile = "/interaction_profiles/meta/touch_controller_plus";

	public const string buttonX = "/input/x/click";

	public const string buttonXTouch = "/input/x/touch";

	public const string buttonY = "/input/y/click";

	public const string buttonYTouch = "/input/y/touch";

	public const string menu = "/input/menu/click";

	public const string buttonA = "/input/a/click";

	public const string buttonATouch = "/input/a/touch";

	public const string buttonB = "/input/b/click";

	public const string buttonBTouch = "/input/b/touch";

	public const string system = "/input/system/click";

	public const string squeeze = "/input/squeeze/value";

	public const string trigger = "/input/trigger/value";

	public const string triggerTouch = "/input/trigger/touch";

	public const string thumbstick = "/input/thumbstick";

	public const string thumbstickClick = "/input/thumbstick/click";

	public const string thumbstickTouch = "/input/thumbstick/touch";

	public const string thumbrest = "/input/thumbrest/touch";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string haptic = "/output/haptic";

	public const string triggerForce = "/input/trigger/force";

	public const string triggerCurl = "/input/trigger/curl_meta";

	public const string triggerSlide = "/input/trigger/slide_meta";

	public const string triggerProximity = "/input/trigger/proximity_meta";

	public const string thumbProximity = "/input/thumb_meta/proximity_meta";

	private const string kDeviceLocalizedName = "Meta Quest Touch Plus Controller OpenXR";

	protected internal override bool OnInstanceCreate(ulong instance)
	{
		if (!OpenXRRuntime.IsExtensionEnabled("XR_META_touch_controller_plus"))
		{
			return false;
		}
		return base.OnInstanceCreate(instance);
	}

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(QuestTouchPlusController), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Meta Quest Touch Plus Controller OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("QuestTouchPlusController");
	}

	protected override string GetDeviceLayoutName()
	{
		return "QuestTouchPlusController";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "questtouchpluscontroller",
			localizedName = "Meta Quest Touch Plus Controller OpenXR",
			desiredInteractionProfile = "/interaction_profiles/meta/touch_controller_plus",
			manufacturer = "Oculus",
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
					name = "thumbstick",
					localizedName = "Thumbstick",
					type = ActionType.Axis2D,
					usages = new List<string> { "Primary2DAxis" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/left" }
						},
						new ActionBinding
						{
							interactionPath = "/input/system/click",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/right" }
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
							interactionPath = "/input/x/click",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/left" }
						},
						new ActionBinding
						{
							interactionPath = "/input/a/click",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/right" }
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
							interactionPath = "/input/x/touch",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/left" }
						},
						new ActionBinding
						{
							interactionPath = "/input/a/touch",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/left" }
						},
						new ActionBinding
						{
							interactionPath = "/input/b/click",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/right" }
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
							interactionPath = "/input/y/touch",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/left" }
						},
						new ActionBinding
						{
							interactionPath = "/input/b/touch",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus",
							userPaths = new List<string> { "/user/hand/right" }
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
						}
					}
				},
				new ActionConfig
				{
					name = "thumbrestTouched",
					localizedName = "Thumbrest Touched",
					type = ActionType.Binary,
					usages = new List<string> { "ThumbrestTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbrest/touch",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
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
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
						}
					}
				},
				new ActionConfig
				{
					name = "triggerForce",
					localizedName = "Trigger Force",
					type = ActionType.Axis1D,
					usages = new List<string> { "TriggerForce" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trigger/force",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
						}
					}
				},
				new ActionConfig
				{
					name = "triggerCurl",
					localizedName = "Trigger Curl",
					type = ActionType.Axis1D,
					usages = new List<string> { "TriggerCurl" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trigger/curl_meta",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
						}
					}
				},
				new ActionConfig
				{
					name = "triggerSlide",
					localizedName = "Trigger Slide",
					type = ActionType.Axis1D,
					usages = new List<string> { "TriggerSlide" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trigger/slide_meta",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
						}
					}
				},
				new ActionConfig
				{
					name = "triggerProximity",
					localizedName = "Trigger Proximity",
					type = ActionType.Binary,
					usages = new List<string> { "TriggerProximity" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trigger/proximity_meta",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
						}
					}
				},
				new ActionConfig
				{
					name = "thumbProximity",
					localizedName = "Thumb Proximity",
					type = ActionType.Binary,
					usages = new List<string> { "ThumbProximity" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumb_meta/proximity_meta",
							interactionProfileName = "/interaction_profiles/meta/touch_controller_plus"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
