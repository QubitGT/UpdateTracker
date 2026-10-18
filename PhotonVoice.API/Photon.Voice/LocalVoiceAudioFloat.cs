namespace Photon.Voice;

public class LocalVoiceAudioFloat : LocalVoiceAudio<float>
{
	internal LocalVoiceAudioFloat(VoiceClient voiceClient, IEncoder encoder, byte id, VoiceInfo voiceInfo, IAudioDesc audioSourceDesc, int channelId)
		: base(voiceClient, encoder, id, voiceInfo, audioSourceDesc, channelId)
	{
		levelMeter = new AudioUtil.LevelMeterFloat(info.SamplingRate, info.Channels);
		voiceDetector = new AudioUtil.VoiceDetectorFloat(info.SamplingRate, info.Channels);
		initBuiltinProcessors();
	}
}
