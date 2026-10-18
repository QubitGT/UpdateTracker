using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class SteamFrameControllerProfile : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Steam Frame Controller Controller (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class SteamFrameController : XRControllerWithRumble
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
		[InputControl(aliases = new string[] { "GripButtonTouched", "squeezeTouched" }, usage = "GripButtonTouch")]
		public ButtonControl gripTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "menuButton", "viewButton" }, usages = new string[] { "MenuButton", "ViewButton" })]
		public ButtonControl menu { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "menuButtonTouched", "viewButtonTouched" }, usages = new string[] { "MenuButtonTouch", "ViewButtonTouch" })]
		public ButtonControl menuTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "bumperButton" }, usage = "BumperButton")]
		public ButtonControl bumper { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "bumperButtonTouched" }, usage = "BumperButtonTouch")]
		public ButtonControl bumperTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "buttonTop", "buttonY", "buttonDpadUp" }, usages = new string[] { "FaceButtonTop", "YButton", "DpadUpButton" })]
		public ButtonControl faceButtonTop { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "buttonOutside", "buttonB", "buttonDpadLeft" }, usages = new string[] { "FaceButtonOutside", "BButton", "DpadLeftButton" })]
		public ButtonControl faceButtonOutside { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "buttonBottom", "buttonA", "buttonDpadDown" }, usages = new string[] { "PrimaryButton", "FaceButtonBottom", "AButton", "DpadDownButton" })]
		public ButtonControl faceButtonBottom { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "buttonInside", "buttonX", "buttonDpadRight" }, usages = new string[] { "SecondaryButton", "FaceButtonInside", "XButton", "DpadRightButton" })]
		public ButtonControl faceButtonInside { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "buttonTopTouched", "buttonYTouched", "buttonDpadUpTouched" }, usages = new string[] { "FaceButtonTopTouch", "YButtonTouch", "DpadUpButtonTouch" })]
		public ButtonControl faceButtonTopTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "buttonOutsideTouched", "buttonBTouched", "buttonDpadLeftTouched" }, usages = new string[] { "FaceButtonOutsideTouch", "BButtonTouch", "DpadLeftButtonTouch" })]
		public ButtonControl faceButtonOutsideTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "buttonBottomTouched", "buttonATouched", "buttonDpadDownTouched" }, usages = new string[] { "PrimaryButtonTouch", "FaceButtonBottomTouch", "AButtonTouch", "DpadDownButtonTouch" })]
		public ButtonControl faceButtonBottomTouched { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "buttonInsideTouched", "buttonXTouched", "buttonDpadRightTouched" }, usages = new string[] { "SecondaryButtonTouch", "FaceButtonInsideTouch", "XButtonTouch", "DpadRightButtonTouch" })]
		public ButtonControl faceButtonInsideTouched { get; private set; }

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
		[InputControl(offset = 40u, noisy = true, alias = "gripPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 52u, noisy = true, alias = "gripOrientation")]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 100u)]
		public Vector3Control pointerPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 112u, alias = "pointerOrientation")]
		public QuaternionControl pointerRotation { get; private set; }

		[Preserve]
		[InputControl(usage = "Haptic")]
		public HapticControl haptic { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			thumbstick = GetChildControl<StickControl>("thumbstick");
			trigger = GetChildControl<AxisControl>("trigger");
			triggerPressed = GetChildControl<ButtonControl>("triggerPressed");
			triggerTouched = GetChildControl<ButtonControl>("triggerTouched");
			grip = GetChildControl<AxisControl>("grip");
			gripPressed = GetChildControl<ButtonControl>("gripPressed");
			gripTouched = GetChildControl<ButtonControl>("gripTouched");
			bumper = GetChildControl<ButtonControl>("bumper");
			bumperTouched = GetChildControl<ButtonControl>("bumperTouched");
			faceButtonTop = GetChildControl<ButtonControl>("faceButtonTop");
			faceButtonOutside = GetChildControl<ButtonControl>("faceButtonOutside");
			faceButtonBottom = GetChildControl<ButtonControl>("faceButtonBottom");
			faceButtonInside = GetChildControl<ButtonControl>("faceButtonInside");
			faceButtonTopTouched = GetChildControl<ButtonControl>("faceButtonTopTouched");
			faceButtonOutsideTouched = GetChildControl<ButtonControl>("faceButtonOutsideTouched");
			faceButtonBottomTouched = GetChildControl<ButtonControl>("faceButtonBottomTouched");
			faceButtonInsideTouched = GetChildControl<ButtonControl>("faceButtonInsideTouched");
			menu = GetChildControl<ButtonControl>("menu");
			menuTouched = GetChildControl<ButtonControl>("menuTouched");
			thumbstickClicked = GetChildControl<ButtonControl>("thumbstickClicked");
			thumbstickTouched = GetChildControl<ButtonControl>("thumbstickTouched");
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

	public const string featureId = "com.unity.openxr.feature.input.frame_controller";

	public const string profile = "/interaction_profiles/valve/frame_controller_valve";

	public const string buttonDpadUp = "/input/dpad_up/click";

	public const string buttonDpadUpTouch = "/input/dpad_up/touch";

	public const string buttonDpadRight = "/input/dpad_right/click";

	public const string buttonDpadRightTouch = "/input/dpad_right/touch";

	public const string buttonDpadDown = "/input/dpad_down/click";

	public const string buttonDpadDownTouch = "/input/dpad_down/touch";

	public const string buttonDpadLeft = "/input/dpad_left/click";

	public const string buttonDpadLeftTouch = "/input/dpad_left/touch";

	public const string buttonView = "/input/view/click";

	public const string buttonViewTouch = "/input/view/touch";

	public const string buttonA = "/input/a/click";

	public const string buttonATouch = "/input/a/touch";

	public const string buttonB = "/input/b/click";

	public const string buttonBTouch = "/input/b/touch";

	public const string buttonX = "/input/x/click";

	public const string buttonXTouch = "/input/x/touch";

	public const string buttonY = "/input/y/click";

	public const string buttonYTouch = "/input/y/touch";

	public const string menu = "/input/menu/click";

	public const string menuTouch = "/input/menu/touch";

	public const string view = "/input/view/click";

	public const string viewTouch = "/input/view/touch";

	public const string system = "/input/system/click";

	public const string systemTouch = "/input/system/touch";

	public const string squeezeValue = "/input/squeeze/value";

	public const string squeezeClick = "/input/squeeze/click";

	public const string squeezeTouch = "/input/squeeze/touch";

	public const string bumperClick = "/input/bumper/click";

	public const string bumperTouch = "/input/bumper/touch";

	public const string trigger = "/input/trigger/value";

	public const string triggerTouch = "/input/trigger/touch";

	public const string thumbstick = "/input/thumbstick";

	public const string thumbstickClick = "/input/thumbstick/click";

	public const string thumbstickTouch = "/input/thumbstick/touch";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string haptic = "/output/haptic";

	private const string kDeviceLocalizedName = "Steam Frame Controller OpenXR";

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(SteamFrameController), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Steam Frame Controller OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("SteamFrameController");
	}

	protected override string GetDeviceLayoutName()
	{
		return "SteamFrameController";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "steamframecontroller",
			localizedName = "Steam Frame Controller OpenXR",
			desiredInteractionProfile = "/interaction_profiles/valve/frame_controller_valve",
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
					name = "thumbstick",
					localizedName = "Thumbstick",
					type = ActionType.Axis2D,
					usages = new List<string> { "Primary2DAxis" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
						}
					}
				},
				new ActionConfig
				{
					name = "gripTouched",
					localizedName = "Grip Touched",
					type = ActionType.Binary,
					usages = new List<string> { "GripButtonTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/squeeze/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
						}
					}
				},
				new ActionConfig
				{
					name = "menu",
					localizedName = "Menu",
					type = ActionType.Binary,
					usages = new List<string> { "MenuButton", "ViewButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/menu/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/view/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "menuTouched",
					localizedName = "Menu Touched",
					type = ActionType.Binary,
					usages = new List<string> { "MenuButtonTouch", "ViewButtonTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/menu/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/view/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "faceButtonTop",
					localizedName = "Face Button Top",
					type = ActionType.Binary,
					usages = new List<string> { "FaceButtonTop", "YButton", "DpadUpButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/y/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/dpad_up/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "faceButtonTopTouched",
					localizedName = "Face Button Top Touched",
					type = ActionType.Binary,
					usages = new List<string> { "FaceButtonTopTouch", "YButtonTouch", "DpadUpButtonTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/y/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/dpad_up/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "faceButtonOutside",
					localizedName = "Face Button Outside",
					type = ActionType.Binary,
					usages = new List<string> { "FaceButtonOutside", "BButton", "DpadLeftButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/b/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/dpad_left/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "faceButtonOutsideTouched",
					localizedName = "Face Button Outside Touched",
					type = ActionType.Binary,
					usages = new List<string> { "FaceButtonOutsideTouch", "BButtonTouch", "DpadLeftButtonTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/b/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/dpad_left/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "faceButtonBottom",
					localizedName = "Face Button Bottom",
					type = ActionType.Binary,
					usages = new List<string> { "PrimaryButton", "FaceButtonBottom", "AButton", "DpadDownButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/a/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/dpad_down/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "faceButtonBottomTouched",
					localizedName = "Face Button Bottom Touched",
					type = ActionType.Binary,
					usages = new List<string> { "PrimaryButtonTouch", "FaceButtonBottomTouch", "AButtonTouch", "DpadDownButtonTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/a/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/dpad_down/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "faceButtonInside",
					localizedName = "Face Button Inside",
					type = ActionType.Binary,
					usages = new List<string> { "SecondaryButton", "FaceButtonInside", "XButton", "DpadRightButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/x/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/dpad_right/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
						}
					}
				},
				new ActionConfig
				{
					name = "faceButtonInsideTouched",
					localizedName = "Face Button Inside Touched",
					type = ActionType.Binary,
					usages = new List<string> { "SecondaryButtonTouch", "FaceButtonInsideTouch", "XButtonTouch", "DpadRightButtonTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/x/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/right" }
						},
						new ActionBinding
						{
							interactionPath = "/input/dpad_right/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve",
							userPaths = new List<string> { "/user/hand/left" }
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
						}
					}
				},
				new ActionConfig
				{
					name = "bumperButton",
					localizedName = "Bumper Button",
					type = ActionType.Binary,
					usages = new List<string> { "BumperButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/bumper/click",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
						}
					}
				},
				new ActionConfig
				{
					name = "bumperTouched",
					localizedName = "Bumper Touched",
					type = ActionType.Binary,
					usages = new List<string> { "BumperTouch" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/bumper/touch",
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
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
							interactionProfileName = "/interaction_profiles/valve/frame_controller_valve"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
