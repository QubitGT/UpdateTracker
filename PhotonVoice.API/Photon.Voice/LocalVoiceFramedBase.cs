namespace Photon.Voice;

public class LocalVoiceFramedBase : LocalVoice
{
	public int FrameSize { get; private set; }

	internal LocalVoiceFramedBase(VoiceClient voiceClient, IEncoder encoder, byte id, VoiceInfo voiceInfo, int channelId, int frameSize)
		: base(voiceClient, encoder, id, voiceInfo, channelId)
	{
		FrameSize = frameSize;
	}
}
