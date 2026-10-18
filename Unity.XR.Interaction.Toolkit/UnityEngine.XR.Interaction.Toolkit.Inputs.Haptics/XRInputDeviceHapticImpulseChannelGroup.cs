using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class XRInputDeviceHapticImpulseChannelGroup : IXRHapticImpulseChannelGroup
{
	private InputDevice m_Device;

	private readonly List<IXRHapticImpulseChannel> m_Channels = new List<IXRHapticImpulseChannel>();

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

	public void Initialize(InputDevice device)
	{
		if (m_Device == device)
		{
			return;
		}
		m_Device = device;
		m_Channels.Clear();
		if (!device.isValid)
		{
			return;
		}
		if (!device.TryGetHapticCapabilities(out var capabilities))
		{
			Debug.LogWarning($"Failed to get haptic capabilities of {device}");
			return;
		}
		if (!capabilities.supportsImpulse)
		{
			Debug.LogWarning($"{device} does not support haptic impulse.");
			return;
		}
		int numChannels = (int)capabilities.numChannels;
		for (int i = 0; i < numChannels; i++)
		{
			m_Channels.Add(new XRInputDeviceHapticImpulseChannel
			{
				motorChannel = i,
				device = device
			});
		}
	}
}
