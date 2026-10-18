using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

[AddComponentMenu("XR/XR Input Modality Manager", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.XRInputModalityManager.html")]
public class XRInputModalityManager : MonoBehaviour
{
	public enum InputMode
	{
		None,
		TrackedHand,
		MotionController
	}

	private class TrackedDeviceMonitor
	{
		private readonly List<int> m_MonitoredDevices = new List<int>();

		private bool m_Subscribed;

		public event Action<TrackedDevice> trackingAcquired;

		public void AddDevice(TrackedDevice device)
		{
			if (!m_MonitoredDevices.Contains(device.deviceId))
			{
				m_MonitoredDevices.Add(device.deviceId);
				Subscribe();
			}
		}

		public void RemoveDevice(TrackedDevice device)
		{
			if (m_MonitoredDevices.Remove(device.deviceId) && m_MonitoredDevices.Count == 0)
			{
				Unsubscribe();
			}
		}

		public void ClearAllDevices()
		{
			if (m_MonitoredDevices.Count > 0)
			{
				m_MonitoredDevices.Clear();
				Unsubscribe();
			}
		}

		private void Subscribe()
		{
			if (!m_Subscribed && m_MonitoredDevices.Count > 0)
			{
				UnityEngine.InputSystem.InputSystem.onAfterUpdate += OnAfterInputUpdate;
				m_Subscribed = true;
			}
		}

		private void Unsubscribe()
		{
			if (m_Subscribed)
			{
				UnityEngine.InputSystem.InputSystem.onAfterUpdate -= OnAfterInputUpdate;
				m_Subscribed = false;
			}
		}

		private void OnAfterInputUpdate()
		{
			for (int i = 0; i < m_MonitoredDevices.Count; i++)
			{
				if (UnityEngine.InputSystem.InputSystem.GetDeviceById(m_MonitoredDevices[i]) is TrackedDevice trackedDevice && IsTracked(trackedDevice))
				{
					m_MonitoredDevices.RemoveAt(i);
					i--;
					this.trackingAcquired?.Invoke(trackedDevice);
				}
			}
			if (m_MonitoredDevices.Count == 0)
			{
				Unsubscribe();
			}
		}
	}

	private class InputDeviceMonitor
	{
		private readonly List<InputDevice> m_MonitoredDevices = new List<InputDevice>();

		private bool m_Subscribed;

		public event Action<InputDevice> trackingAcquired;

		public void AddDevice(InputDevice device)
		{
			if (!m_MonitoredDevices.Contains(device))
			{
				m_MonitoredDevices.Add(device);
				Subscribe();
			}
		}

		public void RemoveDevice(InputDevice device)
		{
			if (m_MonitoredDevices.Remove(device) && m_MonitoredDevices.Count == 0)
			{
				Unsubscribe();
			}
		}

		public void ClearAllDevices()
		{
			if (m_MonitoredDevices.Count > 0)
			{
				m_MonitoredDevices.Clear();
				Unsubscribe();
			}
		}

		private void Subscribe()
		{
			if (!m_Subscribed && m_MonitoredDevices.Count > 0)
			{
				InputTracking.trackingAcquired += OnTrackingAcquired;
				m_Subscribed = true;
			}
		}

		private void Unsubscribe()
		{
			if (m_Subscribed)
			{
				InputTracking.trackingAcquired -= OnTrackingAcquired;
				m_Subscribed = false;
			}
		}

		private void OnTrackingAcquired(XRNodeState nodeState)
		{
			for (int i = 0; i < m_MonitoredDevices.Count; i++)
			{
				InputDevice inputDevice = m_MonitoredDevices[i];
				if (IsTracked(inputDevice))
				{
					m_MonitoredDevices.RemoveAt(i);
					i--;
					this.trackingAcquired?.Invoke(inputDevice);
				}
			}
			if (m_MonitoredDevices.Count == 0)
			{
				Unsubscribe();
			}
		}
	}

	[HideInInspector]
	[SerializeField]
	[Tooltip("GameObject representing the left hand group of interactors. Will toggle on when using hand tracking and off when using motion controllers.")]
	private GameObject m_LeftHand;

	[HideInInspector]
	[SerializeField]
	[Tooltip("GameObject representing the right hand group of interactors. Will toggle on when using hand tracking and off when using motion controllers.")]
	private GameObject m_RightHand;

	[Header("Motion Controllers")]
	[SerializeField]
	[Tooltip("GameObject representing the left motion controller group of interactors. Will toggle on when using motion controllers and off when using hand tracking.")]
	private GameObject m_LeftController;

	[SerializeField]
	[Tooltip("GameObject representing the right motion controller group of interactors. Will toggle on when using motion controllers and off when using hand tracking.")]
	private GameObject m_RightController;

	[HideInInspector]
	[SerializeField]
	private UnityEvent m_TrackedHandModeStarted;

	[HideInInspector]
	[SerializeField]
	private UnityEvent m_TrackedHandModeEnded;

	[Header("Events")]
	[SerializeField]
	private UnityEvent m_MotionControllerModeStarted;

	[SerializeField]
	private UnityEvent m_MotionControllerModeEnded;

	private readonly TrackedDeviceMonitor m_TrackedDeviceMonitor = new TrackedDeviceMonitor();

	private readonly InputDeviceMonitor m_InputDeviceMonitor = new InputDeviceMonitor();

	private static BindableEnum<InputMode> s_CurrentInputMode = new BindableEnum<InputMode>(InputMode.None);

	private InputMode m_LeftInputMode;

	private InputMode m_RightInputMode;

	public GameObject leftHand
	{
		get
		{
			return m_LeftHand;
		}
		set
		{
			m_LeftHand = value;
		}
	}

	public GameObject rightHand
	{
		get
		{
			return m_RightHand;
		}
		set
		{
			m_RightHand = value;
		}
	}

	public GameObject leftController
	{
		get
		{
			return m_LeftController;
		}
		set
		{
			m_LeftController = value;
		}
	}

	public GameObject rightController
	{
		get
		{
			return m_RightController;
		}
		set
		{
			m_RightController = value;
		}
	}

	public UnityEvent trackedHandModeStarted
	{
		get
		{
			return m_TrackedHandModeStarted;
		}
		set
		{
			m_TrackedHandModeStarted = value;
		}
	}

	public UnityEvent trackedHandModeEnded
	{
		get
		{
			return m_TrackedHandModeEnded;
		}
		set
		{
			m_TrackedHandModeEnded = value;
		}
	}

	public UnityEvent motionControllerModeStarted
	{
		get
		{
			return m_MotionControllerModeStarted;
		}
		set
		{
			m_MotionControllerModeStarted = value;
		}
	}

	public UnityEvent motionControllerModeEnded
	{
		get
		{
			return m_MotionControllerModeEnded;
		}
		set
		{
			m_MotionControllerModeEnded = value;
		}
	}

	public static IReadOnlyBindableVariable<InputMode> currentInputMode => s_CurrentInputMode;

	internal InputMode leftInputMode => m_LeftInputMode;

	internal InputMode rightInputMode => m_RightInputMode;

	internal static List<XRInputModalityManager> activeModalityManagers { get; } = new List<XRInputModalityManager>();


	internal event Action<XRInputModalityManager, InputMode> leftInputModeChanged;

	internal event Action<XRInputModalityManager, InputMode> rightInputModeChanged;

	internal static event Action<XRInputModalityManager, bool> activeModalityManagersChanged;

	protected void OnEnable()
	{
		if (m_LeftHand != null || m_RightHand != null)
		{
			Debug.LogWarning("Script requires XR Hands (com.unity.xr.hands) package to switch to hand tracking groups. Install using Window > Package Manager or click Fix on the related issue in Edit > Project Settings > XR Plug-in Management > Project Validation.", this);
		}
		SubscribeHandSubsystem();
		UnityEngine.InputSystem.InputSystem.onDeviceChange += OnDeviceChange;
		InputDevices.deviceConnected += OnDeviceConnected;
		InputDevices.deviceDisconnected += OnDeviceDisconnected;
		InputDevices.deviceConfigChanged += OnDeviceConfigChanged;
		m_TrackedDeviceMonitor.trackingAcquired += OnControllerTrackingAcquired;
		m_InputDeviceMonitor.trackingAcquired += OnControllerTrackingAcquired;
		UpdateLeftMode();
		UpdateRightMode();
		activeModalityManagers.Add(this);
		XRInputModalityManager.activeModalityManagersChanged?.Invoke(this, arg2: true);
	}

	protected void OnDisable()
	{
		UnsubscribeHandSubsystem();
		UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnDeviceChange;
		InputDevices.deviceConnected -= OnDeviceConnected;
		InputDevices.deviceDisconnected -= OnDeviceDisconnected;
		InputDevices.deviceConfigChanged -= OnDeviceConfigChanged;
		if (m_TrackedDeviceMonitor != null)
		{
			m_TrackedDeviceMonitor.trackingAcquired -= OnControllerTrackingAcquired;
			m_TrackedDeviceMonitor.ClearAllDevices();
		}
		if (m_InputDeviceMonitor != null)
		{
			m_InputDeviceMonitor.trackingAcquired -= OnControllerTrackingAcquired;
			m_InputDeviceMonitor.ClearAllDevices();
		}
		activeModalityManagers.Remove(this);
		XRInputModalityManager.activeModalityManagersChanged?.Invoke(this, arg2: false);
	}

	protected void Update()
	{
	}

	private void SubscribeHandSubsystem()
	{
	}

	private void UnsubscribeHandSubsystem()
	{
	}

	private void LogMissingHandSubsystem()
	{
	}

	private void SetLeftMode(InputMode inputMode)
	{
		SafeSetActive(m_LeftHand, inputMode == InputMode.TrackedHand);
		SafeSetActive(m_LeftController, inputMode == InputMode.MotionController);
		InputMode inputMode2 = m_LeftInputMode;
		m_LeftInputMode = inputMode;
		if (inputMode2 != inputMode)
		{
			OnModeChanged(inputMode2, inputMode, m_RightInputMode);
			this.leftInputModeChanged?.Invoke(this, inputMode);
		}
	}

	private void SetRightMode(InputMode inputMode)
	{
		SafeSetActive(m_RightHand, inputMode == InputMode.TrackedHand);
		SafeSetActive(m_RightController, inputMode == InputMode.MotionController);
		InputMode inputMode2 = m_RightInputMode;
		m_RightInputMode = inputMode;
		if (inputMode2 != inputMode)
		{
			OnModeChanged(inputMode2, inputMode, m_LeftInputMode);
			this.rightInputModeChanged?.Invoke(this, inputMode);
		}
	}

	private void OnModeChanged(InputMode oldInputMode, InputMode newInputMode, InputMode otherHandInputMode)
	{
		if (otherHandInputMode != InputMode.TrackedHand && oldInputMode == InputMode.TrackedHand)
		{
			m_TrackedHandModeEnded?.Invoke();
		}
		else if (otherHandInputMode != InputMode.MotionController && oldInputMode == InputMode.MotionController)
		{
			m_MotionControllerModeEnded?.Invoke();
		}
		if (otherHandInputMode != InputMode.TrackedHand && newInputMode == InputMode.TrackedHand)
		{
			m_TrackedHandModeStarted?.Invoke();
		}
		else if (otherHandInputMode != InputMode.MotionController && newInputMode == InputMode.MotionController)
		{
			m_MotionControllerModeStarted?.Invoke();
		}
		s_CurrentInputMode.Value = newInputMode;
	}

	private static void SafeSetActive(GameObject gameObject, bool active)
	{
		if (gameObject != null && gameObject.activeSelf != active)
		{
			gameObject.SetActive(active);
		}
	}

	private bool GetLeftHandIsTracked()
	{
		return false;
	}

	private bool GetRightHandIsTracked()
	{
		return false;
	}

	private void UpdateLeftMode()
	{
		if (TryGetControllerDevice(UnityEngine.InputSystem.CommonUsages.LeftHand, out var controllerDevice))
		{
			UpdateLeftMode(controllerDevice);
			return;
		}
		if (XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(XRInputTrackingAggregator.Characteristics.leftController, out var inputDevice))
		{
			UpdateMode(inputDevice, SetLeftMode);
			return;
		}
		if (XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(XRInputTrackingAggregator.Characteristics.leftHandInteraction, out inputDevice) || XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(XRInputTrackingAggregator.Characteristics.leftMicrosoftHandInteraction, out inputDevice))
		{
			if (GetLeftHandIsTracked())
			{
				SetLeftMode(InputMode.TrackedHand);
			}
			else
			{
				UpdateMode(inputDevice, SetLeftMode);
			}
		}
		InputMode leftMode = (GetLeftHandIsTracked() ? InputMode.TrackedHand : InputMode.None);
		SetLeftMode(leftMode);
	}

	private void UpdateRightMode()
	{
		if (TryGetControllerDevice(UnityEngine.InputSystem.CommonUsages.RightHand, out var controllerDevice))
		{
			UpdateRightMode(controllerDevice);
			return;
		}
		if (XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(XRInputTrackingAggregator.Characteristics.rightController, out var inputDevice))
		{
			UpdateMode(inputDevice, SetRightMode);
			return;
		}
		if (XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(XRInputTrackingAggregator.Characteristics.rightHandInteraction, out inputDevice) || XRInputTrackingAggregator.TryGetDeviceWithExactCharacteristics(XRInputTrackingAggregator.Characteristics.rightMicrosoftHandInteraction, out inputDevice))
		{
			if (GetRightHandIsTracked())
			{
				SetRightMode(InputMode.TrackedHand);
			}
			else
			{
				UpdateMode(inputDevice, SetRightMode);
			}
		}
		InputMode rightMode = (GetRightHandIsTracked() ? InputMode.TrackedHand : InputMode.None);
		SetRightMode(rightMode);
	}

	private void UpdateLeftMode(UnityEngine.InputSystem.XR.XRController controllerDevice)
	{
		if (IsHandInteractionXRControllerType(controllerDevice))
		{
			if (GetLeftHandIsTracked())
			{
				SetLeftMode(InputMode.TrackedHand);
			}
			else
			{
				UpdateMode(controllerDevice, SetLeftMode);
			}
		}
		else
		{
			UpdateMode(controllerDevice, SetLeftMode);
		}
	}

	private void UpdateRightMode(UnityEngine.InputSystem.XR.XRController controllerDevice)
	{
		if (IsHandInteractionXRControllerType(controllerDevice))
		{
			if (GetRightHandIsTracked())
			{
				SetRightMode(InputMode.TrackedHand);
			}
			else
			{
				UpdateMode(controllerDevice, SetRightMode);
			}
		}
		else
		{
			UpdateMode(controllerDevice, SetRightMode);
		}
	}

	private void UpdateMode(UnityEngine.InputSystem.XR.XRController controllerDevice, Action<InputMode> setModeMethod)
	{
		if (controllerDevice == null)
		{
			setModeMethod(InputMode.None);
			return;
		}
		if (IsTracked(controllerDevice))
		{
			setModeMethod(InputMode.MotionController);
			return;
		}
		setModeMethod(InputMode.None);
		m_TrackedDeviceMonitor.AddDevice(controllerDevice);
	}

	private void UpdateMode(InputDevice controllerDevice, Action<InputMode> setModeMethod)
	{
		if (!controllerDevice.isValid)
		{
			setModeMethod(InputMode.None);
			return;
		}
		if (IsTracked(controllerDevice))
		{
			setModeMethod(InputMode.MotionController);
			return;
		}
		setModeMethod(InputMode.None);
		m_InputDeviceMonitor.AddDevice(controllerDevice);
	}

	private static bool TryGetControllerDevice(InternedString usage, out UnityEngine.InputSystem.XR.XRController controllerDevice)
	{
		controllerDevice = null;
		double num = -1.0;
		ReadOnlyArray<UnityEngine.InputSystem.InputDevice> devices = UnityEngine.InputSystem.InputSystem.devices;
		for (int i = 0; i < devices.Count; i++)
		{
			if (devices[i] is UnityEngine.InputSystem.XR.XRController xRController && !ShouldIgnoreXRControllerType(xRController) && xRController.usages.Contains(usage) && (controllerDevice == null || xRController.lastUpdateTime > num))
			{
				controllerDevice = xRController;
				num = xRController.lastUpdateTime;
			}
		}
		return controllerDevice != null;
	}

	private static bool ShouldIgnoreXRControllerType(UnityEngine.InputSystem.XR.XRController device)
	{
		if (device is DPadInteraction.DPad || device is PalmPoseInteraction.PalmPose)
		{
			return true;
		}
		return false;
	}

	private static bool IsHandInteractionXRControllerType(UnityEngine.InputSystem.XR.XRController device)
	{
		if (device is HandInteractionProfile.HandInteraction || device is MicrosoftHandInteraction.HoloLensHand)
		{
			return true;
		}
		return false;
	}

	private static bool IsTracked(TrackedDevice device)
	{
		if (device.isTracked.isPressed)
		{
			return true;
		}
		return (device.trackingState.value & 3) == 3;
	}

	private static bool IsTracked(InputDevice device)
	{
		if (device.TryGetFeatureValue(CommonUsages.isTracked, out var value) && value)
		{
			return true;
		}
		if (device.TryGetFeatureValue(CommonUsages.trackingState, out var value2))
		{
			return (value2 & (InputTrackingState.Position | InputTrackingState.Rotation)) == (InputTrackingState.Position | InputTrackingState.Rotation);
		}
		return false;
	}

	private void OnDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
	{
		if (!(device is UnityEngine.InputSystem.XR.XRController xRController) || ShouldIgnoreXRControllerType(xRController))
		{
			return;
		}
		switch (change)
		{
		case InputDeviceChange.Added:
		case InputDeviceChange.Reconnected:
		case InputDeviceChange.Enabled:
		case InputDeviceChange.UsageChanged:
			if (device.added)
			{
				ReadOnlyArray<InternedString> usages2 = device.usages;
				if (usages2.Contains(UnityEngine.InputSystem.CommonUsages.LeftHand))
				{
					UpdateLeftMode(xRController);
				}
				else if (usages2.Contains(UnityEngine.InputSystem.CommonUsages.RightHand))
				{
					UpdateRightMode(xRController);
				}
			}
			break;
		case InputDeviceChange.Removed:
		case InputDeviceChange.Disconnected:
		case InputDeviceChange.Disabled:
		{
			m_TrackedDeviceMonitor.RemoveDevice(xRController);
			ReadOnlyArray<InternedString> usages = device.usages;
			if (usages.Contains(UnityEngine.InputSystem.CommonUsages.LeftHand))
			{
				InputMode leftMode = (GetLeftHandIsTracked() ? InputMode.TrackedHand : InputMode.None);
				SetLeftMode(leftMode);
			}
			else if (usages.Contains(UnityEngine.InputSystem.CommonUsages.RightHand))
			{
				InputMode rightMode = (GetRightHandIsTracked() ? InputMode.TrackedHand : InputMode.None);
				SetRightMode(rightMode);
			}
			break;
		}
		}
	}

	private void OnDeviceConnected(InputDevice device)
	{
		InputDeviceCharacteristics characteristics = device.characteristics;
		if (characteristics == XRInputTrackingAggregator.Characteristics.leftHandInteraction || characteristics == XRInputTrackingAggregator.Characteristics.leftMicrosoftHandInteraction)
		{
			if (GetLeftHandIsTracked())
			{
				SetLeftMode(InputMode.TrackedHand);
			}
			else
			{
				UpdateMode(device, SetLeftMode);
			}
		}
		else if (characteristics == XRInputTrackingAggregator.Characteristics.rightHandInteraction || characteristics == XRInputTrackingAggregator.Characteristics.rightMicrosoftHandInteraction)
		{
			if (GetRightHandIsTracked())
			{
				SetRightMode(InputMode.TrackedHand);
			}
			else
			{
				UpdateMode(device, SetRightMode);
			}
		}
		else if (characteristics == XRInputTrackingAggregator.Characteristics.leftController)
		{
			UpdateMode(device, SetLeftMode);
		}
		else if (characteristics == XRInputTrackingAggregator.Characteristics.rightController)
		{
			UpdateMode(device, SetRightMode);
		}
	}

	private void OnDeviceDisconnected(InputDevice device)
	{
		m_InputDeviceMonitor.RemoveDevice(device);
		InputDeviceCharacteristics characteristics = device.characteristics;
		if (characteristics == XRInputTrackingAggregator.Characteristics.leftController || characteristics == XRInputTrackingAggregator.Characteristics.leftHandInteraction || characteristics == XRInputTrackingAggregator.Characteristics.leftMicrosoftHandInteraction)
		{
			InputMode leftMode = (GetLeftHandIsTracked() ? InputMode.TrackedHand : InputMode.None);
			SetLeftMode(leftMode);
		}
		else if (characteristics == XRInputTrackingAggregator.Characteristics.rightController || characteristics == XRInputTrackingAggregator.Characteristics.rightHandInteraction || characteristics == XRInputTrackingAggregator.Characteristics.rightMicrosoftHandInteraction)
		{
			InputMode rightMode = (GetRightHandIsTracked() ? InputMode.TrackedHand : InputMode.None);
			SetRightMode(rightMode);
		}
	}

	private void OnDeviceConfigChanged(InputDevice device)
	{
		OnDeviceConnected(device);
	}

	private void OnControllerTrackingAcquired(TrackedDevice device)
	{
		if (device is UnityEngine.InputSystem.XR.XRController)
		{
			ReadOnlyArray<InternedString> usages = device.usages;
			if (m_LeftInputMode == InputMode.None && usages.Contains(UnityEngine.InputSystem.CommonUsages.LeftHand))
			{
				SetLeftMode(InputMode.MotionController);
			}
			else if (m_RightInputMode == InputMode.None && usages.Contains(UnityEngine.InputSystem.CommonUsages.RightHand))
			{
				SetRightMode(InputMode.MotionController);
			}
		}
	}

	private void OnControllerTrackingAcquired(InputDevice device)
	{
		InputDeviceCharacteristics characteristics = device.characteristics;
		if (m_LeftInputMode == InputMode.None && characteristics == XRInputTrackingAggregator.Characteristics.leftController)
		{
			SetLeftMode(InputMode.MotionController);
		}
		else if (m_RightInputMode == InputMode.None && characteristics == XRInputTrackingAggregator.Characteristics.rightController)
		{
			SetRightMode(InputMode.MotionController);
		}
	}
}
