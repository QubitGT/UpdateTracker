using POpusCodec.Enums;

namespace Photon.Voice;

public struct VoiceInfo
{
	public Codec Codec { get; set; }

	public int SamplingRate { get; set; }

	public int Channels { get; set; }

	public int FrameDurationUs { get; set; }

	public int Bitrate { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public int FPS { get; set; }

	public int KeyFrameInt { get; set; }

	public object UserData { get; set; }

	public int FrameDurationSamples => (int)((long)SamplingRate * (long)FrameDurationUs / 1000000);

	public int FrameSize => FrameDurationSamples * Channels;

	public static VoiceInfo CreateAudioOpus(SamplingRate samplingRate, int channels, OpusCodec.FrameDuration frameDurationUs, int bitrate, object userdata = null)
	{
		VoiceInfo result = default(VoiceInfo);
		result.Codec = Codec.AudioOpus;
		result.SamplingRate = (int)samplingRate;
		result.Channels = channels;
		result.FrameDurationUs = (int)frameDurationUs;
		result.Bitrate = bitrate;
		result.UserData = userdata;
		return result;
	}

	public static VoiceInfo CreateAudio(Codec codec, int samplingRate, int channels, int frameDurationUs, object userdata = null)
	{
		VoiceInfo result = default(VoiceInfo);
		result.Codec = codec;
		result.SamplingRate = samplingRate;
		result.Channels = channels;
		result.FrameDurationUs = frameDurationUs;
		result.UserData = userdata;
		return result;
	}

	public override string ToString()
	{
		return "c=" + Codec.ToString() + " f=" + SamplingRate + " ch=" + Channels + " d=" + FrameDurationUs + " s=" + FrameSize + " b=" + Bitrate + " w=" + Width + " h=" + Height + " fps=" + FPS + " kfi=" + KeyFrameInt + " ud=" + UserData;
	}
}
