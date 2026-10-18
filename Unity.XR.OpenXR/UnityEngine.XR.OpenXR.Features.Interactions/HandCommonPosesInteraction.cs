using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class HandCommonPosesInteraction : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Hand Interaction Poses (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" }, isGenericTypeOfDevice = true)]
	public class HandInteractionPoses : OpenXRDevice
	{
		[Preserve]
		[InputControl(offset = 0u, aliases = new string[] { "device", "gripPose" }, usage = "Device")]
		public UnityEngine.InputSystem.XR.PoseControl devicePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u, alias = "aimPose", usage = "Pointer")]
		public UnityEngine.InputSystem.XR.PoseControl pointer { get; private set; }

		[Preserve]
		[InputControl(offset = 0u)]
		public UnityEngine.InputSystem.XR.PoseControl pokePose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u)]
		public UnityEngine.InputSystem.XR.PoseControl pinchPose { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			devicePose = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("devicePose");
			pointer = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("pointer");
			pokePose = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("pokePose");
			pinchPose = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("pinchPose");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.handinteractionposes";

	public const string profile = "/interaction_profiles/unity/hand_interaction_poses";

	public const string grip = "/input/grip/pose";

	public const string aim = "/input/aim/pose";

	public const string poke = "/input/poke_ext/pose";

	public const string pinch = "/input/pinch_ext/pose";

	private const string kDeviceLocalizedName = "Hand Interaction Poses OpenXR";

	public const string extensionString = "XR_EXT_hand_interaction";

	internal override bool IsAdditive => true;

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
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(HandInteractionPoses), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Hand Interaction Poses OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("HandInteractionPoses");
	}

	protected override InteractionProfileType GetInteractionProfileType()
	{
		if (!typeof(HandInteractionPoses).IsSubclassOf(typeof(XRController)))
		{
			return InteractionProfileType.Device;
		}
		return InteractionProfileType.XRController;
	}

	protected override string GetDeviceLayoutName()
	{
		return "HandInteractionPoses";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "handinteractionposes",
			localizedName = "Hand Interaction Poses OpenXR",
			desiredInteractionProfile = "/interaction_profiles/unity/hand_interaction_poses",
			manufacturer = "",
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
					name = "devicePose",
					localizedName = "Device Pose",
					type = ActionType.Pose,
					usages = new List<string> { "Device" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/grip/pose",
							interactionProfileName = "/interaction_profiles/unity/hand_interaction_poses"
						}
					},
					isAdditive = true
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
							interactionProfileName = "/interaction_profiles/unity/hand_interaction_poses"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "PokePose",
					localizedName = "Poke Pose",
					type = ActionType.Pose,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/poke_ext/pose",
							interactionProfileName = "/interaction_profiles/unity/hand_interaction_poses"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "PinchPose",
					localizedName = "Pinch Pose",
					type = ActionType.Pose,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/pinch_ext/pose",
							interactionProfileName = "/interaction_profiles/unity/hand_interaction_poses"
						}
					},
					isAdditive = true
				}
			}
		};
		AddActionMap(map);
	}

	internal override void AddAdditiveActions(List<ActionMapConfig> actionMaps, ActionMapConfig additiveMap)
	{
		foreach (ActionMapConfig actionMap in actionMaps)
		{
			if (!actionMap.deviceInfos.Where((DeviceConfig d) => d.userPath != null && (string.CompareOrdinal(d.userPath, "/user/hand/left") == 0 || string.CompareOrdinal(d.userPath, "/user/hand/right") == 0)).Any())
			{
				continue;
			}
			foreach (ActionConfig additiveAction in additiveMap.actions.Where((ActionConfig a) => a.isAdditive))
			{
				bool flag = false;
				foreach (ActionConfig item in actionMap.actions.Where((ActionConfig m) => m.type == ActionType.Pose).Distinct().ToList())
				{
					if (item.bindings.Where((ActionBinding b) => b.interactionPath != null && string.CompareOrdinal(b.interactionPath, additiveAction.bindings[0].interactionPath) == 0).Any())
					{
						item.isAdditive = true;
						flag = true;
					}
				}
				if (!flag)
				{
					actionMap.actions.Add(additiveAction);
				}
			}
		}
	}
}
