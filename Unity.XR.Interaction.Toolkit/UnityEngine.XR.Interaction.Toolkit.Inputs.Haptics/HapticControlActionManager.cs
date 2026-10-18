using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.OpenXR;
using UnityEngine.XR.OpenXR.Input;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticControlActionManager
{
	private readonly HapticImpulseCommandChannelGroup m_DeviceChannelGroup;

	private readonly OpenXRHapticImpulseChannel m_OpenXRChannel;

	private readonly HapticImpulseSingleChannelGroup m_OpenXRChannelGroup;

	public HapticControlActionManager()
	{
		m_DeviceChannelGroup = new HapticImpulseCommandChannelGroup();
		m_OpenXRChannel = new OpenXRHapticImpulseChannel();
		m_OpenXRChannelGroup = new HapticImpulseSingleChannelGroup(m_OpenXRChannel);
	}

	public IXRHapticImpulseChannelGroup GetChannelGroup(InputAction action)
	{
		if (action == null)
		{
			return null;
		}
		InputControl activeControl = action.activeControl;
		if (activeControl == null)
		{
			ReadOnlyArray<InputControl> controls = action.controls;
			if (controls.Count > 0 && controls[0] is HapticControl hapticControl)
			{
				m_OpenXRChannel.hapticAction = action;
				m_OpenXRChannel.device = hapticControl.device;
				return m_OpenXRChannelGroup;
			}
			return null;
		}
		m_DeviceChannelGroup.Initialize(activeControl.device);
		return m_DeviceChannelGroup;
	}
}
