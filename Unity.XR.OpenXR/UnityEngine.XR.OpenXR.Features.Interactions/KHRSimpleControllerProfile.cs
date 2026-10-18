using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class KHRSimpleControllerProfile : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Khronos Simple Controller (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class KHRSimpleController : XRControllerWithRumble
	{
		[Preserve]
		[InputControl(aliases = new string[] { "Secondary", "selectbutton" }, usage = "PrimaryButton")]
		public ButtonControl select { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "Primary", "menubutton" }, usage = "MenuButton")]
		public ButtonControl menu { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "device", "gripPose" }, usage = "Device")]
		public UnityEngine.InputSystem.XR.PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, alias = "aimPose", usage = "Pointer")]
		public UnityEngine.InputSystem.XR.PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 2u)]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 4u)]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 8u, alias = "gripPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 20u, alias = "gripOrientation")]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 68u)]
		public Vector3Control pointerPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 80u, alias = "pointerOrientation")]
		public QuaternionControl pointerRotation { get; private set; }

		[Preserve]
		[InputControl(usage = "Haptic")]
		public HapticControl haptic { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			menu = GetChildControl<ButtonControl>("menu");
			select = GetChildControl<ButtonControl>("select");
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

	public const string featureId = "com.unity.openxr.feature.input.khrsimpleprofile";

	public const string profile = "/interaction_profiles/khr/simple_controller";

	public const string select = "/input/select/click";

	public const string menu = "/input/menu/click";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string haptic = "/output/haptic";

	private const string kDeviceLocalizedName = "KHR Simple Controller OpenXR";

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(KHRSimpleController), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("KHR Simple Controller OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout(typeof(KHRSimpleController).Name);
	}

	protected override string GetDeviceLayoutName()
	{
		return "KHRSimpleController";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "khrsimplecontroller",
			localizedName = "KHR Simple Controller OpenXR",
			desiredInteractionProfile = "/interaction_profiles/khr/simple_controller",
			manufacturer = "Khronos",
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
					name = "select",
					localizedName = "Select",
					type = ActionType.Binary,
					usages = new List<string> { "PrimaryButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/select/click",
							interactionProfileName = "/interaction_profiles/khr/simple_controller"
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
							interactionProfileName = "/interaction_profiles/khr/simple_controller"
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
							interactionProfileName = "/interaction_profiles/khr/simple_controller"
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
							interactionProfileName = "/interaction_profiles/khr/simple_controller"
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
							interactionProfileName = "/interaction_profiles/khr/simple_controller"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
