using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class DPadInteraction : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "D-Pad Binding (OpenXR)", commonUsages = new string[] { "LeftHand", "RightHand" })]
	public class DPad : XRController
	{
		[Preserve]
		[InputControl]
		public ButtonControl thumbstickDpadUp { get; private set; }

		[Preserve]
		[InputControl]
		public ButtonControl thumbstickDpadDown { get; private set; }

		[Preserve]
		[InputControl]
		public ButtonControl thumbstickDpadLeft { get; private set; }

		[Preserve]
		[InputControl]
		public ButtonControl thumbstickDpadRight { get; private set; }

		[Preserve]
		[InputControl]
		public ButtonControl trackpadDpadUp { get; private set; }

		[Preserve]
		[InputControl]
		public ButtonControl trackpadDpadDown { get; private set; }

		[Preserve]
		[InputControl]
		public ButtonControl trackpadDpadLeft { get; private set; }

		[Preserve]
		[InputControl]
		public ButtonControl trackpadDpadRight { get; private set; }

		[Preserve]
		[InputControl]
		public ButtonControl trackpadDpadCenter { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			thumbstickDpadUp = GetChildControl<ButtonControl>("thumbstickDpadUp");
			thumbstickDpadDown = GetChildControl<ButtonControl>("thumbstickDpadDown");
			thumbstickDpadLeft = GetChildControl<ButtonControl>("thumbstickDpadLeft");
			thumbstickDpadRight = GetChildControl<ButtonControl>("thumbstickDpadRight");
			trackpadDpadUp = GetChildControl<ButtonControl>("trackpadDpadUp");
			trackpadDpadDown = GetChildControl<ButtonControl>("trackpadDpadDown");
			trackpadDpadLeft = GetChildControl<ButtonControl>("trackpadDpadLeft");
			trackpadDpadRight = GetChildControl<ButtonControl>("trackpadDpadRight");
			trackpadDpadCenter = GetChildControl<ButtonControl>("trackpadDpadCenter");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.dpadinteraction";

	public float forceThresholdLeft = 0.5f;

	public float forceThresholdReleaseLeft = 0.4f;

	public float centerRegionLeft = 0.5f;

	public float wedgeAngleLeft = MathF.PI / 2f;

	public bool isStickyLeft;

	public float forceThresholdRight = 0.5f;

	public float forceThresholdReleaseRight = 0.4f;

	public float centerRegionRight = 0.5f;

	public float wedgeAngleRight = MathF.PI / 2f;

	public bool isStickyRight;

	public const string thumbstickDpadUp = "/input/thumbstick/dpad_up";

	public const string thumbstickDpadDown = "/input/thumbstick/dpad_down";

	public const string thumbstickDpadLeft = "/input/thumbstick/dpad_left";

	public const string thumbstickDpadRight = "/input/thumbstick/dpad_right";

	public const string trackpadDpadUp = "/input/trackpad/dpad_up";

	public const string trackpadDpadDown = "/input/trackpad/dpad_down";

	public const string trackpadDpadLeft = "/input/trackpad/dpad_left";

	public const string trackpadDpadRight = "/input/trackpad/dpad_right";

	public const string trackpadDpadCenter = "/input/trackpad/dpad_center";

	public const string profile = "/interaction_profiles/unity/dpad";

	private const string kDeviceLocalizedName = "DPad Interaction OpenXR";

	public string[] extensionStrings = new string[2] { "XR_KHR_binding_modification", "XR_EXT_dpad_binding" };

	internal override bool IsAdditive => true;

	protected internal override bool OnInstanceCreate(ulong instance)
	{
		string[] array = extensionStrings;
		for (int i = 0; i < array.Length; i++)
		{
			if (!OpenXRRuntime.IsExtensionEnabled(array[i]))
			{
				return false;
			}
		}
		return base.OnInstanceCreate(instance);
	}

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(DPad), null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("DPad Interaction OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("DPad");
	}

	protected override string GetDeviceLayoutName()
	{
		return "DPad";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "dpadinteraction",
			localizedName = "DPad Interaction OpenXR",
			desiredInteractionProfile = "/interaction_profiles/unity/dpad",
			manufacturer = "",
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
					name = "thumbstickDpadUp",
					localizedName = " Thumbstick Dpad Up",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick/dpad_up",
							interactionProfileName = "/interaction_profiles/unity/dpad"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "thumbstickDpadDown",
					localizedName = "Thumbstick Dpad Down",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick/dpad_down",
							interactionProfileName = "/interaction_profiles/unity/dpad"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "thumbstickDpadLeft",
					localizedName = "Thumbstick Dpad Left",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick/dpad_left",
							interactionProfileName = "/interaction_profiles/unity/dpad"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "thumbstickDpadRight",
					localizedName = "Thumbstick Dpad Right",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/thumbstick/dpad_right",
							interactionProfileName = "/interaction_profiles/unity/dpad"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "trackpadDpadUp",
					localizedName = "Trackpad Dpad Up",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/dpad_up",
							interactionProfileName = "/interaction_profiles/unity/dpad"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "trackpadDpadDown",
					localizedName = "Trackpad Dpad Down",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/dpad_down",
							interactionProfileName = "/interaction_profiles/unity/dpad"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "trackpadDpadLeft",
					localizedName = "Trackpad Dpad Left",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/dpad_left",
							interactionProfileName = "/interaction_profiles/unity/dpad"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "trackpadDpadRight",
					localizedName = "Trackpad Dpad Right",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/dpad_right",
							interactionProfileName = "/interaction_profiles/unity/dpad"
						}
					},
					isAdditive = true
				},
				new ActionConfig
				{
					name = "trackpadDpadCenter",
					localizedName = "Trackpad Dpad Center",
					type = ActionType.Binary,
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/trackpad/dpad_center",
							interactionProfileName = "/interaction_profiles/unity/dpad"
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
			bool flag = false;
			bool flag2 = false;
			foreach (ActionConfig action in actionMap.actions)
			{
				if (!flag && action.bindings.FirstOrDefault((ActionBinding b) => b.interactionPath.Contains("trackpad")) != null)
				{
					flag = true;
				}
				if (!flag2 && action.bindings.FirstOrDefault((ActionBinding b) => b.interactionPath.Contains("thumbstick")) != null)
				{
					flag2 = true;
				}
			}
			foreach (ActionConfig item in additiveMap.actions.Where((ActionConfig a) => a.isAdditive))
			{
				if ((flag && item.name.StartsWith("trackpad")) || (flag2 && item.name.StartsWith("thumbstick")))
				{
					actionMap.actions.Add(item);
				}
			}
		}
	}
}
