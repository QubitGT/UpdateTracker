using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.Hands;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

[AddComponentMenu("XR/Debug/Simulated Device Lifecycle Manager", 11)]
[DefaultExecutionOrder(-29995)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation.SimulatedDeviceLifecycleManager.html")]
public class SimulatedDeviceLifecycleManager : MonoBehaviour
{
	public enum DeviceMode
	{
		Controller,
		Hand,
		None
	}

	[SerializeField]
	private bool m_RemoveOtherHMDDevices = true;

	[SerializeField]
	private bool m_HandTrackingCapability = true;

	private DeviceMode m_DeviceMode;

	private XRSimulatedHMD m_HMDDevice;

	private XRSimulatedController m_LeftControllerDevice;

	private XRSimulatedController m_RightControllerDevice;

	private bool m_OnInputDeviceChangeSubscribed;

	private bool m_DeviceModeDirty;

	private bool m_StartedDeviceModeChange;

	public bool removeOtherHMDDevices
	{
		get
		{
			return m_RemoveOtherHMDDevices;
		}
		set
		{
			m_RemoveOtherHMDDevices = value;
		}
	}

	public bool handTrackingCapability
	{
		get
		{
			return m_HandTrackingCapability;
		}
		set
		{
			m_HandTrackingCapability = value;
		}
	}

	public DeviceMode deviceMode => m_DeviceMode;

	public static SimulatedDeviceLifecycleManager instance { get; private set; }

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Debug.LogWarning($"Another instance of Simulated Device Lifecycle Manager already exists ({instance}), destroying {base.gameObject}.", this);
			Object.Destroy(base.gameObject);
			return;
		}
		InitializeHandSubsystem();
	}

	protected virtual void OnEnable()
	{
		if (m_RemoveOtherHMDDevices)
		{
			UnityEngine.InputSystem.InputDevice[] array = UnityEngine.InputSystem.InputSystem.devices.ToArray();
			foreach (UnityEngine.InputSystem.InputDevice inputDevice in array)
			{
				if (inputDevice is XRHMD && !(inputDevice is XRSimulatedHMD))
				{
					UnityEngine.InputSystem.InputSystem.RemoveDevice(inputDevice);
				}
			}
			UnityEngine.InputSystem.InputSystem.onDeviceChange += OnInputDeviceChange;
			m_OnInputDeviceChangeSubscribed = true;
		}
		AddDevices();
	}

	protected virtual void OnDisable()
	{
		if (m_OnInputDeviceChangeSubscribed)
		{
			UnityEngine.InputSystem.InputSystem.onDeviceChange -= OnInputDeviceChange;
			m_OnInputDeviceChangeSubscribed = false;
		}
		RemoveDevices();
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void Update()
	{
	}

	internal void ApplyHMDState(XRSimulatedHMDState state)
	{
		if (m_HMDDevice != null && m_HMDDevice.added)
		{
			InputState.Change(m_HMDDevice, state);
		}
	}

	internal void ApplyControllerState(XRSimulatedControllerState leftControllerState, XRSimulatedControllerState rightControllerState)
	{
		if (m_LeftControllerDevice != null && m_LeftControllerDevice.added)
		{
			InputState.Change(m_LeftControllerDevice, leftControllerState);
		}
		if (m_RightControllerDevice != null && m_RightControllerDevice.added)
		{
			InputState.Change(m_RightControllerDevice, rightControllerState);
		}
	}

	internal void ApplyHandState(XRSimulatedHandState leftHandState, XRSimulatedHandState rightHandState)
	{
	}

	internal void SwitchDeviceMode()
	{
	}

	internal virtual void AddDevices()
	{
		if (m_HMDDevice == null)
		{
			InputDeviceDescription inputDeviceDescription = default(InputDeviceDescription);
			inputDeviceDescription.product = "XRSimulatedHMD";
			inputDeviceDescription.capabilities = new XRDeviceDescriptor
			{
				characteristics = XRInputTrackingAggregator.Characteristics.hmd
			}.ToJson();
			InputDeviceDescription description = inputDeviceDescription;
			m_HMDDevice = UnityEngine.InputSystem.InputSystem.AddDevice(description) as XRSimulatedHMD;
			if (m_HMDDevice == null)
			{
				Debug.LogError("Failed to create XRSimulatedHMD.", this);
			}
		}
		else
		{
			UnityEngine.InputSystem.InputSystem.AddDevice(m_HMDDevice);
		}
		if (m_DeviceMode == DeviceMode.Controller)
		{
			AddControllerDevices();
		}
	}

	internal virtual void RemoveDevices()
	{
		if (m_HMDDevice != null && m_HMDDevice.added)
		{
			UnityEngine.InputSystem.InputSystem.RemoveDevice(m_HMDDevice);
		}
		RemoveControllerDevices();
	}

	private void AddControllerDevices()
	{
		if (m_LeftControllerDevice == null)
		{
			InputDeviceDescription inputDeviceDescription = default(InputDeviceDescription);
			inputDeviceDescription.product = "XRSimulatedController";
			inputDeviceDescription.capabilities = new XRDeviceDescriptor
			{
				deviceName = string.Format("{0} - {1}", "XRSimulatedController", UnityEngine.InputSystem.CommonUsages.LeftHand),
				characteristics = XRInputTrackingAggregator.Characteristics.leftController
			}.ToJson();
			InputDeviceDescription description = inputDeviceDescription;
			m_LeftControllerDevice = UnityEngine.InputSystem.InputSystem.AddDevice(description) as XRSimulatedController;
			if (m_LeftControllerDevice != null)
			{
				UnityEngine.InputSystem.InputSystem.SetDeviceUsage(m_LeftControllerDevice, UnityEngine.InputSystem.CommonUsages.LeftHand);
			}
			else
			{
				Debug.LogError(string.Format("Failed to create {0} for {1}.", "XRSimulatedController", UnityEngine.InputSystem.CommonUsages.LeftHand), this);
			}
		}
		else
		{
			UnityEngine.InputSystem.InputSystem.AddDevice(m_LeftControllerDevice);
		}
		if (m_RightControllerDevice == null)
		{
			InputDeviceDescription inputDeviceDescription = default(InputDeviceDescription);
			inputDeviceDescription.product = "XRSimulatedController";
			inputDeviceDescription.capabilities = new XRDeviceDescriptor
			{
				deviceName = string.Format("{0} - {1}", "XRSimulatedController", UnityEngine.InputSystem.CommonUsages.RightHand),
				characteristics = XRInputTrackingAggregator.Characteristics.rightController
			}.ToJson();
			InputDeviceDescription description2 = inputDeviceDescription;
			m_RightControllerDevice = UnityEngine.InputSystem.InputSystem.AddDevice(description2) as XRSimulatedController;
			if (m_RightControllerDevice != null)
			{
				UnityEngine.InputSystem.InputSystem.SetDeviceUsage(m_RightControllerDevice, UnityEngine.InputSystem.CommonUsages.RightHand);
			}
			else
			{
				Debug.LogError(string.Format("Failed to create {0} for {1}.", "XRSimulatedController", UnityEngine.InputSystem.CommonUsages.RightHand), this);
			}
		}
		else
		{
			UnityEngine.InputSystem.InputSystem.AddDevice(m_RightControllerDevice);
		}
	}

	private void RemoveControllerDevices()
	{
		if (m_LeftControllerDevice != null && m_LeftControllerDevice.added)
		{
			UnityEngine.InputSystem.InputSystem.RemoveDevice(m_LeftControllerDevice);
		}
		if (m_RightControllerDevice != null && m_RightControllerDevice.added)
		{
			UnityEngine.InputSystem.InputSystem.RemoveDevice(m_RightControllerDevice);
		}
	}

	private void OnInputDeviceChange(UnityEngine.InputSystem.InputDevice device, InputDeviceChange change)
	{
		if (m_RemoveOtherHMDDevices && change == InputDeviceChange.Added && device is XRHMD && !(device is XRSimulatedHMD))
		{
			UnityEngine.InputSystem.InputSystem.RemoveDevice(device);
		}
	}

	private void InitializeHandSubsystem()
	{
	}

	private static DeviceMode Negate(DeviceMode mode)
	{
		return mode switch
		{
			DeviceMode.Controller => DeviceMode.Hand, 
			DeviceMode.Hand => DeviceMode.Controller, 
			_ => DeviceMode.Controller, 
		};
	}
}
