using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.XR;
using UnityEngine.Scripting;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.OpenXR.Features.Interactions;

public class EyeGazeInteraction : OpenXRInteractionFeature
{
	[Preserve]
	[InputControlLayout(displayName = "Eye Gaze (OpenXR)", isGenericTypeOfDevice = true)]
	public class EyeGazeDevice : OpenXRDevice
	{
		[Preserve]
		[InputControl(offset = 0u, usages = new string[] { "Device", "gaze" })]
		public UnityEngine.InputSystem.XR.PoseControl pose { get; private set; }

		protected override void FinishSetup()
		{
			base.FinishSetup();
			pose = GetChildControl<UnityEngine.InputSystem.XR.PoseControl>("pose");
		}
	}

	public const string featureId = "com.unity.openxr.feature.input.eyetracking";

	private const string userPath = "/user/eyes_ext";

	private const string profile = "/interaction_profiles/ext/eye_gaze_interaction";

	private const string pose = "/input/gaze_ext/pose";

	private const string kDeviceLocalizedName = "Eye Tracking OpenXR";

	public const string extensionString = "XR_EXT_eye_gaze_interaction";

	private const string layoutName = "EyeGaze";

	protected internal override bool OnInstanceCreate(ulong instance)
	{
		if (!OpenXRRuntime.IsExtensionEnabled("XR_EXT_eye_gaze_interaction"))
		{
			return false;
		}
		return base.OnInstanceCreate(instance);
	}

	protected override void RegisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout(typeof(EyeGazeDevice), "EyeGaze", default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Eye Tracking OpenXR"));
	}

	protected override void UnregisterDeviceLayout()
	{
		UnityEngine.InputSystem.InputSystem.RemoveLayout("EyeGaze");
	}

	protected override InteractionProfileType GetInteractionProfileType()
	{
		if (!typeof(EyeGazeDevice).IsSubclassOf(typeof(XRController)))
		{
			return InteractionProfileType.Device;
		}
		return InteractionProfileType.XRController;
	}

	protected override string GetDeviceLayoutName()
	{
		return "EyeGaze";
	}

	protected override void RegisterActionMapsWithRuntime()
	{
		ActionMapConfig map = new ActionMapConfig
		{
			name = "eyegaze",
			localizedName = "Eye Tracking OpenXR",
			desiredInteractionProfile = "/interaction_profiles/ext/eye_gaze_interaction",
			manufacturer = "",
			serialNumber = "",
			deviceInfos = new List<DeviceConfig>
			{
				new DeviceConfig
				{
					characteristics = (InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.EyeTracking | InputDeviceCharacteristics.TrackedDevice),
					userPath = "/user/eyes_ext"
				}
			},
			actions = new List<ActionConfig>
			{
				new ActionConfig
				{
					name = "pose",
					localizedName = "Pose",
					type = ActionType.Pose,
					usages = new List<string> { "Device", "gaze" },
					bindings = new List<ActionBinding>
					{
						new ActionBinding
						{
							interactionPath = "/input/gaze_ext/pose",
							interactionProfileName = "/interaction_profiles/ext/eye_gaze_interaction"
						}
					}
				}
			}
		};
		AddActionMap(map);
	}
}
