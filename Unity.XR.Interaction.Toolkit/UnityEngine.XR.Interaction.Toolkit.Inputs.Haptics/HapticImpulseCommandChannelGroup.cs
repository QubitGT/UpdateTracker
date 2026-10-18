using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR.Haptics;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticImpulseCommandChannelGroup : IXRHapticImpulseChannelGroup
{
	private readonly List<IXRHapticImpulseChannel> m_Channels = new List<IXRHapticImpulseChannel>();

	private UnityEngine.InputSystem.InputDevice m_Device;

	public int channelCount => m_Channels.Count;

	public IXRHapticImpulseChannel GetChannel(int channel = 0)
	{
		if (channel < 0)
		{
			Debug.LogError("Haptic channel can't be negative.");
			return null;
		}
		if (channel >= m_Channels.Count)
		{
			return null;
		}
		return m_Channels[channel];
	}

	public void Initialize(UnityEngine.InputSystem.InputDevice device)
	{
		if (m_Device == device)
		{
			return;
		}
		m_Device = device;
		m_Channels.Clear();
		if (device != null)
		{
			GetHapticCapabilitiesCommand command = GetHapticCapabilitiesCommand.Create();
			long num = device.ExecuteCommand(ref command);
			int num2;
			if (num < 0)
			{
				Debug.LogWarning($"Failed to get haptic capabilities of {device}, error code {num}. Continuing assuming a single haptic channel.");
				num2 = 1;
			}
			else
			{
				num2 = (int)command.numChannels;
			}
			for (int i = 0; i < num2; i++)
			{
				m_Channels.Add(new HapticImpulseCommandChannel
				{
					motorChannel = i,
					device = device
				});
			}
		}
	}
}
