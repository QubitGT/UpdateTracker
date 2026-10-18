using System;
using UnityEngine;

namespace Meta.Voice.UnityOpus;

public class Encoder : IDisposable
{
	private int bitrate;

	private int complexity;

	private OpusSignal signal;

	private IntPtr encoder;

	private NumChannels channels;

	private bool disposedValue;

	public int Bitrate
	{
		get
		{
			return bitrate;
		}
		set
		{
			Library.OpusEncoderSetBitrate(encoder, value);
			bitrate = value;
		}
	}

	public int Complexity
	{
		get
		{
			return complexity;
		}
		set
		{
			Library.OpusEncoderSetComplexity(encoder, value);
			complexity = value;
		}
	}

	public OpusSignal Signal
	{
		get
		{
			return signal;
		}
		set
		{
			Library.OpusEncoderSetSignal(encoder, value);
			signal = value;
		}
	}

	public Encoder(SamplingFrequency samplingFrequency, NumChannels channels, OpusApplication application)
	{
		this.channels = channels;
		encoder = Library.OpusEncoderCreate(samplingFrequency, channels, application, out var error);
		if (error != 0)
		{
			Debug.LogError("[UnityOpus] Failed to init encoder. Error code: " + error);
			encoder = IntPtr.Zero;
		}
	}

	public int Encode(float[] pcm, byte[] output)
	{
		if (encoder == IntPtr.Zero)
		{
			return 0;
		}
		return Library.OpusEncodeFloat(encoder, pcm, pcm.Length / (int)channels, output, output.Length);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue && !(encoder == IntPtr.Zero))
		{
			Library.OpusEncoderDestroy(encoder);
			encoder = IntPtr.Zero;
			disposedValue = true;
		}
	}

	~Encoder()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
