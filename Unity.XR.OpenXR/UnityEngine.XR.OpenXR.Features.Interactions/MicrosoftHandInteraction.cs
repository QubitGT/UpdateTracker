using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class MicrosoftHandInteraction : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Hololens Hand (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class HoloLensHand : XRController
	{
		[Preserve]
		[InputControl(usage = "PrimaryAxis")]
		public AxisControl select { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "Primary", "selectbutton" }, usages = new string[] { "PrimaryButton" })]
		public ButtonControl selectPressed { get; private set; }

		[Preserve]
		[InputControl(alias = "Secondary", usage = "Grip")]
		public AxisControl squeeze { get; private set; }

		[Preserve]
		[InputControl(aliases = new string[] { "GripButton", "squeezeClicked" }, usages = new string[] { "GripButton" })]
		public ButtonControl squeezePressed { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, alias = "device", usage = "Device")]
		public PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, usage = "Pointer")]
		public PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 132u)]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 136u)]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 20u, alias = "gripPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 32u, alias = "gripOrientation")]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 80u)]
		public Vector3Control pointerPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 92u, alias = "pointerOrientation")]
		public QuaternionControl pointerRotation { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			select = GetChildControl<AxisControl>("select");
			selectPressed = GetChildControl<ButtonControl>("selectPressed");
			squeeze = GetChildControl<AxisControl>("squeeze");
			squeezePressed = GetChildControl<ButtonControl>("squeezePressed");
			devicePose = GetChildControl<PoseControl>("devicePose");
			pointer = GetChildControl<PoseControl>("pointer");
			isTracked = GetChildControl<ButtonControl>("isTracked");
			trackingState = GetChildControl<IntegerControl>("trackingState");
			devicePosition = GetChildControl<Vector3Control>("devicePosition");
			deviceRotation = GetChildControl<QuaternionControl>("deviceRotation");
			pointerPosition = GetChildControl<Vector3Control>("pointerPosition");
			pointerRotation = GetChildControl<QuaternionControl>("pointerRotation");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.handtracking";

	public const string extensionString = "XR_MSFT_hand_interaction";

	public const string profile = "/interaction_profiles/microsoft/hand_interaction";

	public const string select = "/input/select/value";

	public const string squeeze = "/input/squeeze/value";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	private const string kDeviceLocalizedName = "HoloLens Hand OpenXR";

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(HoloLensHand), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("HoloLens Hand OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("HoloLensHand");
	}

	protected override string GetDeviceLayoutName()
	{
		return "HoloLensHand";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "microsofthandinteraction",
			localizedName = "HoloLens Hand OpenXR",
			desiredInteractionProfile = "/interaction_profiles/microsoft/hand_interaction",
			manufacturer = "Microsoft",
			serialNumber = "",
			deviceInfos = new List<DeviceConfig>
			{
				new DeviceConfig
				{
					characteristics = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left),
					userPath = "/user/hand/left"
				},
				new DeviceConfig
				{
					characteristics = (InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.HandTracking | InputDeviceCharacteristics.TrackedDevice | InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right),
					userPath = "/user/hand/right"
				}
			},
			actions = new List<ActionConfig>
			{
				new ActionConfig
				{
					name = "select",
					localizedName = "Select",
					type = ActionType.Axis1D,
					usages = new List<string> { "PrimaryAxis" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/select/value",
							interactionProfileName = "/interaction_profiles/microsoft/hand_interaction"
						}
					}
				},
				new ActionConfig
				{
					name = "selectPressed",
					localizedName = "Select Pressed",
					type = ActionType.Binary,
					usages = new List<string> { "PrimaryButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/select/value",
							interactionProfileName = "/interaction_profiles/microsoft/hand_interaction"
						}
					}
				},
				new ActionConfig
				{
					name = "squeeze",
					localizedName = "Squeeze",
					type = ActionType.Axis1D,
					usages = new List<string> { "Grip" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/squeeze/value",
							interactionProfileName = "/interaction_profiles/microsoft/hand_interaction"
						}
					}
				},
				new ActionConfig
				{
					name = "squeezePressed",
					localizedName = "Squeeze Pressed",
					type = ActionType.Binary,
					usages = new List<string> { "GripButton" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/squeeze/value",
							interactionProfileName = "/interaction_profiles/microsoft/hand_interaction"
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
							interactionProfileName = "/interaction_profiles/microsoft/hand_interaction"
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
							interactionProfileName = "/interaction_profiles/microsoft/hand_interaction"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
