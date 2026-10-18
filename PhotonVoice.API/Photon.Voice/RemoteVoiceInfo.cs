namespace Photon.Voice;

public class RemoteVoiceInfo
{
	public VoiceInfo Info { get; private set; }

	public int ChannelId { get; private set; }

	public int PlayerId { get; private set; }

	public byte VoiceId { get; private set; }

	internal RemoteVoiceInfo(int channelId, int playerId, byte voiceId, VoiceInfo info)
	{
		ChannelId = channelId;
		PlayerId = playerId;
		VoiceId = voiceId;
		Info = info;
	}
}
