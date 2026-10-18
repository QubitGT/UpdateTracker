using System;
using System.Collections.Generic;

namespace UnityEngine.XR.OpenXR.Features;

[Serializable]
public abstract class OpenXRInteractionFeature : OpenXRFeature
{
	[Serializable]
	protected internal enum ActionType
	{
		Binary,
		Axis1D,
		Axis2D,
		Pose,
		Vibrate,
		Count
	}

	[Serializable]
	protected internal class ActionBinding
	{
		public string interactionProfileName;

		public string interactionPath;

		public List<string> userPaths;
	}

	[Serializable]
	protected internal class ActionConfig
	{
		public string name;

		public ActionType type;

		public string localizedName;

		public List<ActionBinding> bindings;

		public List<string> usages;

		public bool isAdditive;
	}

	protected internal class DeviceConfig
	{
		public InputDeviceCharacteristics characteristics;

		public string userPath;
	}

	[Serializable]
	protected internal class ActionMapConfig
	{
		public string name;

		public string localizedName;

		public List<DeviceConfig> deviceInfos;

		public List<ActionConfig> actions;

		public string desiredInteractionProfile;

		public string manufacturer;

		public string serialNumber;
	}

	public static class UserPaths
	{
		public const string leftHand = "/user/hand/left";

		public const string rightHand = "/user/hand/right";

		public const string head = "/user/head";

		public const string gamepad = "/user/gamepad";

		public const string treadmill = "/user/treadmill";
	}

	public enum InteractionProfileType
	{
		Device,
		XRController
	}

	private static List<ActionMapConfig> m_CreatedActionMaps = null;

	private static Dictionary<InteractionProfileType, Dictionary<string, bool>> m_InteractionProfileEnabledMaps = new Dictionary<InteractionProfileType, Dictionary<string, bool>>();

	internal virtual bool IsAdditive => false;

	protected virtual void RegisterDeviceLayout()
	{
	}

	protected virtual void UnregisterDeviceLayout()
	{
	}

	protected virtual void RegisterActionMapsWithRuntime()
	{
	}

	protected internal override bool OnInstanceCreate(ulong xrSession)
	{
		RegisterDeviceLayout();
		return true;
	}

	protected virtual InteractionProfileType GetInteractionProfileType()
	{
		return InteractionProfileType.XRController;
	}

	protected virtual string GetDeviceLayoutName()
	{
		return "";
	}

	internal void CreateActionMaps(List<ActionMapConfig> configs)
	{
		m_CreatedActionMaps = configs;
		RegisterActionMapsWithRuntime();
		m_CreatedActionMaps = null;
	}

	protected void AddActionMap(ActionMapConfig map)
	{
		if (map == null)
		{
			throw new ArgumentNullException("map");
		}
		if (m_CreatedActionMaps == null)
		{
			throw new InvalidOperationException("ActionMap must be added from within the RegisterActionMapsWithRuntime method");
		}
		m_CreatedActionMaps.Add(map);
	}

	internal virtual void AddAdditiveActions(List<ActionMapConfig> actionMaps, ActionMapConfig additiveMap)
	{
	}

	protected internal override void OnEnabledChange()
	{
		base.OnEnabledChange();
	}

	internal static void RegisterLayouts()
	{
		OpenXRFeature[] features = OpenXRSettings.Instance.GetFeatures<OpenXRInteractionFeature>();
		foreach (OpenXRFeature openXRFeature in features)
		{
			if (openXRFeature.enabled)
			{
				((OpenXRInteractionFeature)openXRFeature).RegisterDeviceLayout();
			}
		}
	}
}
