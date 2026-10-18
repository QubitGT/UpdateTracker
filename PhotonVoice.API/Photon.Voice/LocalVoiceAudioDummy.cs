using System;

namespace Photon.Voice;

public class LocalVoiceAudioDummy : LocalVoice, ILocalVoiceAudio
{
	private AudioUtil.VoiceDetectorDummy voiceDetector;

	private AudioUtil.LevelMeterDummy levelMeter;

	public static LocalVoiceAudioDummy Dummy = new LocalVoiceAudioDummy();

	public AudioUtil.IVoiceDetector VoiceDetector => voiceDetector;

	public AudioUtil.ILevelMeter LevelMeter => levelMeter;

	public bool VoiceDetectorCalibrating => false;

	public void VoiceDetectorCalibrate(int durationMs, Action<float> onCalibrated = null)
	{
	}

	public LocalVoiceAudioDummy()
	{
		voiceDetector = new AudioUtil.VoiceDetectorDummy();
		levelMeter = new AudioUtil.LevelMeterDummy();
	}
}
