namespace UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

public interface IXRHapticImpulseChannelGroup
{
	int channelCount { get; }

	IXRHapticImpulseChannel GetChannel(int channel = 0);
}
