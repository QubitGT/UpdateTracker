using System;

namespace Photon.Voice;

public abstract class LocalVoiceAudio<T> : LocalVoiceFramed<T>, ILocalVoiceAudio
{
	protected AudioUtil.VoiceDetector<T> voiceDetector;

	protected AudioUtil.VoiceDetectorCalibration<T> voiceDetectorCalibration;

	protected AudioUtil.LevelMeter<T> levelMeter;

	protected int channels;

	protected bool resampleSource;

	public virtual AudioUtil.IVoiceDetector VoiceDetector => voiceDetector;

	public virtual AudioUtil.ILevelMeter LevelMeter => levelMeter;

	public bool VoiceDetectorCalibrating => voiceDetectorCalibration.IsCalibrating;

	public static LocalVoiceAudio<T> Create(VoiceClient voiceClient, byte voiceId, IEncoder encoder, VoiceInfo voiceInfo, IAudioDesc audioSourceDesc, int channelId)
	{
		if (typeof(T) == typeof(float))
		{
			return new LocalVoiceAudioFloat(voiceClient, encoder, voiceId, voiceInfo, audioSourceDesc, channelId) as LocalVoiceAudio<T>;
		}
		if (typeof(T) == typeof(short))
		{
			return new LocalVoiceAudioShort(voiceClient, encoder, voiceId, voiceInfo, audioSourceDesc, channelId) as LocalVoiceAudio<T>;
		}
		throw new UnsupportedSampleTypeException(typeof(T));
	}

	public void VoiceDetectorCalibrate(int durationMs, Action<float> onCalibrated = null)
	{
		voiceDetectorCalibration.Calibrate(durationMs, onCalibrated);
	}

	internal LocalVoiceAudio(VoiceClient voiceClient, IEncoder encoder, byte id, VoiceInfo voiceInfo, IAudioDesc audioSourceDesc, int channelId)
		: base(voiceClient, encoder, id, voiceInfo, channelId, (voiceInfo.SamplingRate != 0) ? (voiceInfo.FrameSize * audioSourceDesc.SamplingRate / voiceInfo.SamplingRate) : voiceInfo.FrameSize)
	{
		channels = voiceInfo.Channels;
		if (audioSourceDesc.SamplingRate != voiceInfo.SamplingRate)
		{
			resampleSource = true;
			base.voiceClient.logger.LogWarning("[PV] Local voice #" + base.id + " audio source frequency " + audioSourceDesc.SamplingRate + " and encoder sampling rate " + voiceInfo.SamplingRate + " do not match. Resampling will occur before encoding.");
		}
	}

	protected void initBuiltinProcessors()
	{
		if (resampleSource)
		{
			AddPostProcessor(new AudioUtil.Resampler<T>(info.FrameSize, channels));
		}
		voiceDetectorCalibration = new AudioUtil.VoiceDetectorCalibration<T>(voiceDetector, levelMeter, info.SamplingRate, channels);
		AddPostProcessor(levelMeter, voiceDetectorCalibration, voiceDetector);
	}
}
