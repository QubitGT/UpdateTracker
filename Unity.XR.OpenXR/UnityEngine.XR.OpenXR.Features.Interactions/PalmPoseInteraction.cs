using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class PalmPoseInteraction : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Palm Pose (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class PalmPose : XRController
	{
		[Preserve]
		[InputControl(offset = 0u)]
		public PoseControl palmPose { get; private set; }

		[Preserve]
		[InputControl(offset = 0u)]
		public new ButtonControl isTracked { get; private set; }

		[Preserve]
		[InputControl(offset = 4u)]
		public new IntegerControl trackingState { get; private set; }

		[Preserve]
		[InputControl(offset = 8u, noisy = true, alias = "palmPosition")]
		public new Vector3Control devicePosition { get; private set; }

		[Preserve]
		[InputControl(offset = 20u, noisy = true, alias = "palmRotation")]
		public new QuaternionControl deviceRotation { get; private set; }

		[Preserve]
		[InputControl(offset = 8u, noisy = true)]
		public Vector3Control palmPosition { get; private set; }

		[Preserve]
		[InputControl(offset = 20u, noisy = true)]
		public QuaternionControl palmRotation { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			palmPose = GetChildControl<PoseControl>("palmPose");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.palmpose";

	public const string palmPose = "/input/palm_ext/pose";

	public const string gripSurfacePose = "/input/grip_surface/pose";

	public const string profile = "/interaction_profiles/ext/palmpose";

	private const string kDeviceLocalizedName = "Palm Pose Interaction OpenXR";

	public const string extensionString = "XR_EXT_palm_pose";

	internal override bool IsAdditive => true;

	protected internal override bool OnInstanceCreate(ulong instance)
	{
		if (!OpenXRRuntime.IsExtensionEnabled("XR_EXT_palm_pose"))
		{
			return false;
		}
		return base.OnInstanceCreate(instance);
	}

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(PalmPose), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Palm Pose Interaction OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("PalmPose");
	}

	protected override string GetDeviceLayoutName()
	{
		return "PalmPose";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "palmposeinteraction",
			localizedName = "Palm Pose Interaction OpenXR",
			desiredInteractionProfile = "/interaction_profiles/ext/palmpose",
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
					name = "palmpose",
					localizedName = "Palm Pose",
					type = ActionType.Pose,
					bindings = AddBindingBasedOnRuntimeAPIVersion(),
					isAdditive = true
				}
			}
		};
		AddActionMap(map);
	}

	internal List<ActionBinding> AddBindingBasedOnRuntimeAPIVersion()
	{
		if (OpenXRRuntime.isRuntimeAPIVersionGreaterThan1_1())
		{
			return new List<ActionBinding>
			{
				new ActionBinding
				{
					interactionPath = "/input/grip_surface/pose",
					interactionProfileName = "/interaction_profiles/ext/palmpose"
				}
			};
		}
		return new List<ActionBinding>
		{
			new ActionBinding
			{
				interactionPath = "/input/palm_ext/pose",
				interactionProfileName = "/interaction_profiles/ext/palmpose"
			}
		};
	}

	internal override void AddAdditiveActions(List<ActionMapConfig> actionMaps, ActionMapConfig additiveMap)
	{
		foreach (ActionMapConfig actionMap in actionMaps)
		{
			if (!actionMap.deviceInfos.Where((DeviceConfig d) => d.userPath != null && (string.CompareOrdinal(d.userPath, "/user/hand/left") == 0 || string.CompareOrdinal(d.userPath, "/user/hand/right") == 0)).Any())
			{
				continue;
			}
			foreach (ActionConfig item in additiveMap.actions.Where((ActionConfig a) => a.isAdditive))
			{
				actionMap.actions.Add(item);
			}
		}
	}
}
