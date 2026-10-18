namespace Photon.Voice;

public class LocalVoiceAudioShort : LocalVoiceAudio<short>
{
	internal LocalVoiceAudioShort(VoiceClient voiceClient, IEncoder encoder, byte id, VoiceInfo voiceInfo, IAudioDesc audioSourceDesc, int channelId)
		: base(voiceClient, encoder, id, voiceInfo, audioSourceDesc, channelId)
	{
		levelMeter = new AudioUtil.LevelMeterShort(info.SamplingRate, info.Channels);
		voiceDetector = new AudioUtil.VoiceDetectorShort(info.SamplingRate, info.Channels);
		initBuiltinProcessors();
	}
}
