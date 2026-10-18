using System;
using System.Runtime.InteropServices;

namespace Meta.Voice.UnityOpus;

public class Library
{
	public const int maximumPacketDuration = 5760;

	private const string dllName = "UnityOpus";

	[DllImport("UnityOpus")]
	public static extern IntPtr OpusEncoderCreate(SamplingFrequency samplingFrequency, NumChannels channels, OpusApplication application, out ErrorCode error);

	[DllImport("UnityOpus")]
	public static extern int OpusEncode(IntPtr encoder, short[] pcm, int frameSize, byte[] data, int maxDataBytes);

	[DllImport("UnityOpus")]
	public static extern int OpusEncodeFloat(IntPtr encoder, float[] pcm, int frameSize, byte[] data, int maxDataBytes);

	[DllImport("UnityOpus")]
	public static extern void OpusEncoderDestroy(IntPtr encoder);

	[DllImport("UnityOpus")]
	public static extern int OpusEncoderSetBitrate(IntPtr encoder, int bitrate);

	[DllImport("UnityOpus")]
	public static extern int OpusEncoderSetComplexity(IntPtr encoder, int complexity);

	[DllImport("UnityOpus")]
	public static extern int OpusEncoderSetSignal(IntPtr encoder, OpusSignal signal);

	[DllImport("UnityOpus")]
	public static extern IntPtr OpusDecoderCreate(SamplingFrequency samplingFrequency, NumChannels channels, out ErrorCode error);

	[DllImport("UnityOpus")]
	public static extern int OpusDecode(IntPtr decoder, byte[] data, int len, short[] pcm, int frameSize, int decodeFec);

	[DllImport("UnityOpus")]
	public static extern int OpusDecodeFloat(IntPtr decoder, byte[] data, int len, float[] pcm, int frameSize, int decodeFec);

	[DllImport("UnityOpus")]
	public static extern void OpusDecoderDestroy(IntPtr decoder);

	[DllImport("UnityOpus")]
	public static extern void OpusPcmSoftClip(float[] pcm, int frameSize, NumChannels channels, float[] softclipMem);
}
