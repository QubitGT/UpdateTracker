namespace Liv.Lck.Encoding;

internal struct EncoderSessionData
{
	public ulong EncodedVideoFrames { get; set; }

	public ulong EncodedAudioSamplesPerChannel { get; set; }

	public float CaptureTimeSeconds { get; set; }
}
