using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace UnityEngine.XR.OpenXR.Input;

public static class OpenXRInput
{
	[StructLayout(LayoutKind.Explicit)]
	private struct SerializedGuid
	{
		[FieldOffset(0)]
		public Guid guid;

		[FieldOffset(0)]
		public ulong ulong1;

		[FieldOffset(8)]
		public ulong ulong2;
	}

	internal struct SerializedBinding
	{
		public ulong actionId;

		public string path;
	}

	[Flags]
	public enum InputSourceNameFlags
	{
		UserPath = 1,
		InteractionProfile = 2,
		Component = 4,
		All = 7
	}

	[StructLayout(LayoutKind.Explicit, Size = 12)]
	private struct GetInternalDeviceIdCommand : IInputDeviceCommandInfo
	{
		private const int k_BaseCommandSizeSize = 8;

		private const int k_Size = 12;

		[FieldOffset(0)]
		private InputDeviceCommand baseCommand;

		[FieldOffset(8)]
		public readonly uint deviceId;

		private static FourCC Type => new FourCC('X', 'R', 'D', 'I');

		public FourCC typeStatic => Type;

		public static GetInternalDeviceIdCommand Create()
		{
			GetInternalDeviceIdCommand result = default(GetInternalDeviceIdCommand);
			result.baseCommand = new InputDeviceCommand(Type, 12);
			return result;
		}
	}

	private static readonly Dictionary<string, OpenXRInteractionFeature.ActionType> ExpectedControlTypeToActionType = new Dictionary<string, OpenXRInteractionFeature.ActionType>
	{
		["Digital"] = OpenXRInteractionFeature.ActionType.Binary,
		["Button"] = OpenXRInteractionFeature.ActionType.Binary,
		["Axis"] = OpenXRInteractionFeature.ActionType.Axis1D,
		["Integer"] = OpenXRInteractionFeature.ActionType.Axis1D,
		["Analog"] = OpenXRInteractionFeature.ActionType.Axis1D,
		["Vector2"] = OpenXRInteractionFeature.ActionType.Axis2D,
		["Dpad"] = OpenXRInteractionFeature.ActionType.Axis2D,
		["Stick"] = OpenXRInteractionFeature.ActionType.Axis2D,
		["Pose"] = OpenXRInteractionFeature.ActionType.Pose,
		["Vector3"] = OpenXRInteractionFeature.ActionType.Pose,
		["Quaternion"] = OpenXRInteractionFeature.ActionType.Pose,
		["Haptic"] = OpenXRInteractionFeature.ActionType.Vibrate
	};

	private const string s_devicePoseActionName = "devicepose";

	private const string s_pointerActionName = "pointer";

	private static readonly Dictionary<string, string> kVirtualControlMap = new Dictionary<string, string>
	{
		["deviceposition"] = "devicepose",
		["devicerotation"] = "devicepose",
		["trackingstate"] = "devicepose",
		["istracked"] = "devicepose",
		["pointerposition"] = "pointer",
		["pointerrotation"] = "pointer"
	};

	private const string Library = "UnityOpenXR";

	internal static void RegisterLayouts()
	{
		UnityEngine.InputSystem.InputSystem.RegisterLayout<HapticControl>("Haptic");
		UnityEngine.InputSystem.InputSystem.RegisterLayout<OpenXRDevice>();
		UnityEngine.InputSystem.InputSystem.RegisterLayout<OpenXRHmd>(null, default(InputDeviceMatcher).WithInterface("^(XRInput)").WithProduct("Head Tracking - OpenXR").WithManufacturer("OpenXR"));
		OpenXRInteractionFeature.RegisterLayouts();
	}

	private static bool ValidateActionMapConfig(OpenXRInteractionFeature interactionFeature, OpenXRInteractionFeature.ActionMapConfig actionMapConfig)
	{
		bool result = true;
		if (actionMapConfig.deviceInfos == null || actionMapConfig.deviceInfos.Count == 0)
		{
			Debug.LogError($"ActionMapConfig contains no `deviceInfos` in InteractionFeature '{interactionFeature.GetType()}'");
			result = false;
		}
		if (actionMapConfig.actions == null || actionMapConfig.actions.Count == 0)
		{
			Debug.LogError($"ActionMapConfig contains no `actions` in InteractionFeature '{interactionFeature.GetType()}'");
			result = false;
		}
		return result;
	}

	internal static void AttachActionSets()
	{
		List<OpenXRInteractionFeature.ActionMapConfig> list = new List<OpenXRInteractionFeature.ActionMapConfig>();
		List<OpenXRInteractionFeature.ActionMapConfig> list2 = new List<OpenXRInteractionFeature.ActionMapConfig>();
		foreach (OpenXRInteractionFeature item in from f in OpenXRSettings.Instance.features.OfType<OpenXRInteractionFeature>()
			where f.enabled && !f.IsAdditive
			select f)
		{
			int count = list.Count;
			item.CreateActionMaps(list);
			for (int num = list.Count - 1; num >= count; num--)
			{
				if (!ValidateActionMapConfig(item, list[num]))
				{
					list.RemoveAt(num);
				}
			}
		}
		if (!RegisterDevices(list, isAdditive: false))
		{
			return;
		}
		foreach (OpenXRInteractionFeature item2 in from f in OpenXRSettings.Instance.features.OfType<OpenXRInteractionFeature>()
			where f.enabled && f.IsAdditive
			select f)
		{
			item2.CreateActionMaps(list2);
			item2.AddAdditiveActions(list, list2[list2.Count - 1]);
		}
		Dictionary<string, List<SerializedBinding>> dictionary = new Dictionary<string, List<SerializedBinding>>();
		if (!CreateActions(list, dictionary))
		{
			return;
		}
		if (list2.Count > 0)
		{
			RegisterDevices(list2, isAdditive: true);
			CreateActions(list2, dictionary);
		}
		SetDpadBindingCustomValues();
		foreach (KeyValuePair<string, List<SerializedBinding>> item3 in dictionary)
		{
			if (!Internal_SuggestBindings(item3.Key, item3.Value.ToArray(), (uint)item3.Value.Count))
			{
				OpenXRRuntime.LogLastError();
			}
		}
		if (!Internal_AttachActionSets())
		{
			OpenXRRuntime.LogLastError();
		}
	}

	private static bool RegisterDevices(List<OpenXRInteractionFeature.ActionMapConfig> actionMaps, bool isAdditive)
	{
		foreach (OpenXRInteractionFeature.ActionMapConfig actionMap in actionMaps)
		{
			foreach (OpenXRInteractionFeature.DeviceConfig deviceInfo in actionMap.deviceInfos)
			{
				string name = ((actionMap.desiredInteractionProfile == null) ? UserPathToDeviceName(deviceInfo.userPath) : actionMap.localizedName);
				if (Internal_RegisterDeviceDefinition(deviceInfo.userPath, actionMap.desiredInteractionProfile, isAdditive, (uint)deviceInfo.characteristics, name, actionMap.manufacturer, actionMap.serialNumber) == 0L)
				{
					OpenXRRuntime.LogLastError();
					return false;
				}
			}
		}
		return true;
	}

	private static bool CreateActions(List<OpenXRInteractionFeature.ActionMapConfig> actionMaps, Dictionary<string, List<SerializedBinding>> interactionProfiles)
	{
		foreach (OpenXRInteractionFeature.ActionMapConfig actionMap in actionMaps)
		{
			string localizedName = SanitizeStringForOpenXRPath(actionMap.localizedName);
			ulong num = Internal_CreateActionSet(SanitizeStringForOpenXRPath(actionMap.name), localizedName, default(SerializedGuid));
			if (num == 0L)
			{
				OpenXRRuntime.LogLastError();
				return false;
			}
			List<string> list = actionMap.deviceInfos.Select((OpenXRInteractionFeature.DeviceConfig d) => d.userPath).ToList();
			foreach (OpenXRInteractionFeature.ActionConfig action in actionMap.actions)
			{
				string[] array = action.bindings.Where((OpenXRInteractionFeature.ActionBinding b) => b.userPaths != null).SelectMany((OpenXRInteractionFeature.ActionBinding b) => b.userPaths).Distinct()
					.ToList()
					.Union(list)
					.ToArray();
				ulong num2 = Internal_CreateAction(num, SanitizeStringForOpenXRPath(action.name), action.localizedName, (uint)action.type, default(SerializedGuid), array, (uint)array.Length, action.isAdditive, action.usages?.ToArray(), (uint)(action.usages?.Count ?? 0));
				if (num2 == 0L)
				{
					OpenXRRuntime.LogLastError();
					return false;
				}
				foreach (OpenXRInteractionFeature.ActionBinding binding in action.bindings)
				{
					foreach (string item in binding.userPaths ?? list)
					{
						string key = (action.isAdditive ? actionMap.desiredInteractionProfile : (binding.interactionProfileName ?? actionMap.desiredInteractionProfile));
						if (!interactionProfiles.TryGetValue(key, out var value))
						{
							value = (interactionProfiles[key] = new List<SerializedBinding>());
						}
						value.Add(new SerializedBinding
						{
							actionId = num2,
							path = item + binding.interactionPath
						});
					}
				}
			}
		}
		return true;
	}

	private static void SetDpadBindingCustomValues()
	{
		DPadInteraction feature = OpenXRSettings.Instance.GetFeature<DPadInteraction>();
		if (feature != null && feature.enabled)
		{
			Internal_SetDpadBindingCustomValues(isLeft: true, feature.forceThresholdLeft, feature.forceThresholdReleaseLeft, feature.centerRegionLeft, feature.wedgeAngleLeft, feature.isStickyLeft);
			Internal_SetDpadBindingCustomValues(isLeft: false, feature.forceThresholdRight, feature.forceThresholdReleaseRight, feature.centerRegionRight, feature.wedgeAngleRight, feature.isStickyRight);
		}
	}

	private static char SanitizeCharForOpenXRPath(char c)
	{
		if (char.IsLower(c) || char.IsDigit(c))
		{
			return c;
		}
		if (char.IsUpper(c))
		{
			return char.ToLower(c);
		}
		if (c == '-' || c == '.' || c == '_' || c == '/')
		{
			return c;
		}
		return '\0';
	}

	private static string SanitizeStringForOpenXRPath(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return "";
		}
		int i;
		for (i = 0; i < input.Length && SanitizeCharForOpenXRPath(input[i]) == input[i]; i++)
		{
		}
		if (i == input.Length)
		{
			return input;
		}
		StringBuilder stringBuilder = new StringBuilder(input, 0, i, input.Length);
		for (; i < input.Length; i++)
		{
			char c = SanitizeCharForOpenXRPath(input[i]);
			if (c != 0)
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private static string GetActionHandleName(InputControl control)
	{
		InputControl inputControl = control;
		while (inputControl.parent != null && inputControl.parent.parent != null)
		{
			inputControl = inputControl.parent;
		}
		string text = SanitizeStringForOpenXRPath(inputControl.name);
		if (kVirtualControlMap.TryGetValue(text, out var value))
		{
			return value;
		}
		return text;
	}

	public static void SendHapticImpulse(InputActionReference actionRef, float amplitude, float duration, UnityEngine.InputSystem.InputDevice inputDevice = null)
	{
		SendHapticImpulse(actionRef, amplitude, 0f, duration, inputDevice);
	}

	public static void SendHapticImpulse(InputActionReference actionRef, float amplitude, float frequency, float duration, UnityEngine.InputSystem.InputDevice inputDevice = null)
	{
		SendHapticImpulse(actionRef.action, amplitude, frequency, duration, inputDevice);
	}

	public static void SendHapticImpulse(InputAction action, float amplitude, float duration, UnityEngine.InputSystem.InputDevice inputDevice = null)
	{
		SendHapticImpulse(action, amplitude, 0f, duration, inputDevice);
	}

	public static void SendHapticImpulse(InputAction action, float amplitude, float frequency, float duration, UnityEngine.InputSystem.InputDevice inputDevice = null)
	{
		if (action != null)
		{
			ulong actionHandle = GetActionHandle(action, inputDevice);
			if (actionHandle != 0L)
			{
				amplitude = Mathf.Clamp(amplitude, 0f, 1f);
				duration = Mathf.Max(duration, 0f);
				Internal_SendHapticImpulse(GetDeviceId(inputDevice), actionHandle, amplitude, frequency, duration);
			}
		}
	}

	public static void StopHaptics(InputActionReference actionRef, UnityEngine.InputSystem.InputDevice inputDevice = null)
	{
		if (!(actionRef == null))
		{
			StopHaptics(actionRef.action, inputDevice);
		}
	}

	public static void SendHapticImpulse(InputDevice device, float amplitude, float frequency, float duration)
	{
		if (device.isValid)
		{
			Internal_SendHapticImpulseNoISX(GetDeviceId(device), amplitude, frequency, duration);
		}
	}

	public static void StopHapticImpulse(InputDevice device)
	{
		if (device.isValid)
		{
			Internal_StopHapticsNoISX(GetDeviceId(device));
		}
	}

	public static void StopHaptics(InputAction inputAction, UnityEngine.InputSystem.InputDevice inputDevice = null)
	{
		if (inputAction != null)
		{
			ulong actionHandle = GetActionHandle(inputAction, inputDevice);
			if (actionHandle != 0L)
			{
				Internal_StopHaptics(GetDeviceId(inputDevice), actionHandle);
			}
		}
	}

	public static bool TryGetInputSourceName(InputAction inputAction, int index, out string name, InputSourceNameFlags flags = InputSourceNameFlags.All, UnityEngine.InputSystem.InputDevice inputDevice = null)
	{
		name = "";
		if (index < 0)
		{
			return false;
		}
		ulong actionHandle = GetActionHandle(inputAction, inputDevice);
		if (actionHandle == 0L)
		{
			return false;
		}
		return Internal_TryGetInputSourceName(GetDeviceId(inputDevice), actionHandle, (uint)index, (uint)flags, out name);
	}

	public static bool GetActionIsActive(InputAction inputAction)
	{
		if (inputAction != null && inputAction.controls.Count > 0 && inputAction.controls[0].device != null)
		{
			for (int i = 0; i < inputAction.controls.Count; i++)
			{
				uint deviceId = GetDeviceId(inputAction.controls[i].device);
				if (deviceId != 0)
				{
					string actionHandleName = GetActionHandleName(inputAction.controls[i]);
					if (Internal_GetActionIsActive(deviceId, actionHandleName))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool GetActionIsActive(InputDevice device, InputFeatureUsage usage)
	{
		return GetActionIsActive(device, usage.name);
	}

	public static bool GetActionIsActive(InputDevice device, string usageName)
	{
		uint deviceId = GetDeviceId(device);
		if (deviceId == 0)
		{
			return false;
		}
		return Internal_GetActionIsActiveNoISX(deviceId, usageName);
	}

	public static bool TrySetControllerLateLatchAction(InputAction inputAction)
	{
		if (inputAction == null || inputAction.controls.Count != 1)
		{
			return false;
		}
		if (inputAction.controls[0].device == null)
		{
			return false;
		}
		uint deviceId = GetDeviceId(inputAction.controls[0].device);
		if (deviceId == 0)
		{
			return false;
		}
		ulong actionHandle = GetActionHandle(inputAction);
		if (actionHandle == 0L)
		{
			return false;
		}
		return Internal_TrySetControllerLateLatchAction(deviceId, actionHandle);
	}

	public static bool TrySetControllerLateLatchAction(InputDevice device, InputFeatureUsage usage)
	{
		return TrySetControllerLateLatchAction(device, usage.name);
	}

	public static bool TrySetControllerLateLatchAction(InputDevice device, string usageName)
	{
		uint deviceId = GetDeviceId(device);
		if (deviceId == 0)
		{
			return false;
		}
		ulong actionHandle = GetActionHandle(device, usageName);
		if (actionHandle == 0L)
		{
			return false;
		}
		return Internal_TrySetControllerLateLatchAction(deviceId, actionHandle);
	}

	public static ulong GetActionHandle(InputDevice device, InputFeatureUsage usage)
	{
		return GetActionHandle(device, usage.name);
	}

	public static ulong GetActionHandle(InputDevice device, string usageName)
	{
		uint deviceId = GetDeviceId(device);
		if (deviceId == 0)
		{
			return 0uL;
		}
		return Internal_GetActionIdNoISX(deviceId, usageName);
	}

	public static ulong GetActionHandle(InputAction inputAction, UnityEngine.InputSystem.InputDevice inputDevice = null)
	{
		if (inputAction == null || inputAction.controls.Count == 0)
		{
			return 0uL;
		}
		foreach (InputControl control in inputAction.controls)
		{
			if ((inputDevice != null && control.device != inputDevice) || control.device == null)
			{
				continue;
			}
			uint deviceId = GetDeviceId(control.device);
			if (deviceId != 0)
			{
				string actionHandleName = GetActionHandleName(control);
				ulong num = Internal_GetActionId(deviceId, actionHandleName);
				if (num != 0L)
				{
					return num;
				}
			}
		}
		return 0uL;
	}

	private static uint GetDeviceId(UnityEngine.InputSystem.InputDevice inputDevice)
	{
		if (inputDevice == null)
		{
			return 0u;
		}
		GetInternalDeviceIdCommand command = GetInternalDeviceIdCommand.Create();
		if (inputDevice.ExecuteCommand(ref command) != 0L)
		{
			return command.deviceId;
		}
		return 0u;
	}

	private static uint GetDeviceId(InputDevice inputDevice)
	{
		return Internal_GetDeviceId(inputDevice.characteristics, inputDevice.name);
	}

	private static string UserPathToDeviceName(string userPath)
	{
		string[] array = userPath.Split('/', '_');
		StringBuilder stringBuilder = new StringBuilder("OXR");
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.Length != 0)
			{
				string text2 = SanitizeStringForOpenXRPath(text);
				stringBuilder.Append(char.ToUpper(text2[0]));
				stringBuilder.Append(text2.Substring(1));
			}
		}
		return stringBuilder.ToString();
	}

	[DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "OpenXRInputProvider_SetDpadBindingCustomValues")]
	private static extern void Internal_SetDpadBindingCustomValues([MarshalAs(UnmanagedType.I1)] bool isLeft, float forceThreshold, float forceThresholdReleased, float centerRegion, float wedgeAngle, [MarshalAs(UnmanagedType.I1)] bool isSticky);

	[DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "OpenXRInputProvider_SendHapticImpulse")]
	private static extern void Internal_SendHapticImpulse(uint deviceId, ulong actionId, float amplitude, float frequency, float duration);

	[DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "OpenXRInputProvider_SendHapticImpulseNoISX")]
	private static extern void Internal_SendHapticImpulseNoISX(uint deviceId, float amplitude, float frequency, float duration);

	[DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "OpenXRInputProvider_StopHaptics")]
	private static extern void Internal_StopHaptics(uint deviceId, ulong actionId);

	[DllImport("UnityOpenXR", CallingConvention = CallingConvention.Cdecl, EntryPoint = "OpenXRInputProvider_StopHapticsNoISX")]
	private static extern void Internal_StopHapticsNoISX(uint deviceId);

	[DllImport("UnityOpenXR", EntryPoint = "OpenXRInputProvider_GetActionIdByControl")]
	private static extern ulong Internal_GetActionId(uint deviceId, string name);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_GetActionIdByUsageName")]
	private static extern ulong Internal_GetActionIdNoISX(uint deviceId, string usageName);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_TryGetInputSourceName")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_TryGetInputSourceNamePtr(uint deviceId, ulong actionId, uint index, uint flags, out IntPtr outName);

	internal static bool Internal_TryGetInputSourceName(uint deviceId, ulong actionId, uint index, uint flags, out string outName)
	{
		if (!Internal_TryGetInputSourceNamePtr(deviceId, actionId, index, flags, out var outName2))
		{
			outName = "";
			return false;
		}
		outName = Marshal.PtrToStringAnsi(outName2);
		return true;
	}

	[DllImport("UnityOpenXR", EntryPoint = "OpenXRInputProvider_TrySetControllerLateLatchAction")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_TrySetControllerLateLatchAction(uint deviceId, ulong actionId);

	[DllImport("UnityOpenXR", EntryPoint = "OpenXRInputProvider_GetActionIsActive")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetActionIsActive(uint deviceId, string name);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_GetActionIsActiveNoISX")]
	[return: MarshalAs(UnmanagedType.U1)]
	private static extern bool Internal_GetActionIsActiveNoISX(uint deviceId, string name);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_RegisterDeviceDefinition")]
	private static extern ulong Internal_RegisterDeviceDefinition(string userPath, string interactionProfile, [MarshalAs(UnmanagedType.I1)] bool isAdditive, uint characteristics, string name, string manufacturer, string serialNumber);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_CreateActionSet")]
	private static extern ulong Internal_CreateActionSet(string name, string localizedName, SerializedGuid guid);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_CreateAction")]
	private static extern ulong Internal_CreateAction(ulong actionSetId, string name, string localizedName, uint actionType, SerializedGuid guid, string[] userPaths, uint userPathCount, [MarshalAs(UnmanagedType.I1)] bool isAdditive, string[] usages, uint usageCount);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_SuggestBindings")]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static extern bool Internal_SuggestBindings(string interactionProfile, SerializedBinding[] serializedBindings, uint serializedBindingCount);

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_AttachActionSets")]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static extern bool Internal_AttachActionSets();

	[DllImport("UnityOpenXR", CharSet = CharSet.Ansi, EntryPoint = "OpenXRInputProvider_GetDeviceId")]
	private static extern uint Internal_GetDeviceId(InputDeviceCharacteristics characteristics, string name);
}
