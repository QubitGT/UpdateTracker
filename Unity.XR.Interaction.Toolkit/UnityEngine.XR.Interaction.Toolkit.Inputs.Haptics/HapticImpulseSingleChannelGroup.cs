namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public class HapticImpulseSingleChannelGroup : IXRHapticImpulseChannelGroup
{
	public int channelCount => 1;

	public IXRHapticImpulseChannel impulseChannel { get; }

	public HapticImpulseSingleChannelGroup(IXRHapticImpulseChannel channel)
	{
		impulseChannel = channel;
	}

	public IXRHapticImpulseChannel GetChannel(int channel = 0)
	{
		if (channel < 0)
		{
			Debug.LogError("Haptic channel can't be negative.");
			return null;
		}
		return impulseChannel;
	}
}
