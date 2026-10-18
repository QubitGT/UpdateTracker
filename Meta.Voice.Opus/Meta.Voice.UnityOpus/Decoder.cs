using System;
using UnityEngine;

namespace Meta.Voice.UnityOpus;

public class Decoder : IDisposable
{
	public const int maximumPacketDuration = 5760;

	private IntPtr decoder;

	private readonly NumChannels channels;

	private readonly float[] softclipMem;

	private bool disposedValue;

	public Decoder(SamplingFrequency samplingFrequency, NumChannels channels)
	{
		this.channels = channels;
		decoder = Library.OpusDecoderCreate(samplingFrequency, channels, out var error);
		if (error != 0)
		{
			Debug.LogError("[UnityOpus] Failed to create Decoder. Error code is " + error);
			decoder = IntPtr.Zero;
		}
		softclipMem = new float[(int)channels];
	}

	public int Decode(byte[] data, int dataLength, float[] pcm, int decodeFec = 0)
	{
		if (decoder == IntPtr.Zero)
		{
			return 0;
		}
		int num = Library.OpusDecodeFloat(decoder, data, dataLength, pcm, pcm.Length / (int)channels, decodeFec);
		Library.OpusPcmSoftClip(pcm, num / (int)channels, channels, softclipMem);
		return num;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue && !(decoder == IntPtr.Zero))
		{
			Library.OpusDecoderDestroy(decoder);
			decoder = IntPtr.Zero;
			disposedValue = true;
		}
	}

	~Decoder()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
