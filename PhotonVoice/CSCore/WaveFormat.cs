using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CSCore;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public class WaveFormat : ICloneable, IEquatable<WaveFormat>
{
	private AudioEncoding _encoding;

	private short _channels;

	private int _sampleRate;

	private int _bytesPerSecond;

	private short _blockAlign;

	private short _bitsPerSample;

	private short _extraSize;

	public virtual int Channels
	{
		get
		{
			return _channels;
		}
		protected internal set
		{
			_channels = (short)value;
			UpdateProperties();
		}
	}

	public virtual int SampleRate
	{
		get
		{
			return _sampleRate;
		}
		protected internal set
		{
			_sampleRate = value;
			UpdateProperties();
		}
	}

	public virtual int BytesPerSecond
	{
		get
		{
			return _bytesPerSecond;
		}
		protected internal set
		{
			_bytesPerSecond = value;
		}
	}

	public virtual int BlockAlign
	{
		get
		{
			return _blockAlign;
		}
		protected internal set
		{
			_blockAlign = (short)value;
		}
	}

	public virtual int BitsPerSample
	{
		get
		{
			return _bitsPerSample;
		}
		protected internal set
		{
			_bitsPerSample = (short)value;
			UpdateProperties();
		}
	}

	public virtual int ExtraSize
	{
		get
		{
			return _extraSize;
		}
		protected internal set
		{
			_extraSize = (short)value;
		}
	}

	public virtual int BytesPerSample => BitsPerSample / 8;

	public virtual int BytesPerBlock => BytesPerSample * Channels;

	public virtual AudioEncoding WaveFormatTag
	{
		get
		{
			return _encoding;
		}
		protected internal set
		{
			_encoding = value;
		}
	}

	public WaveFormat()
		: this(44100, 16, 2)
	{
	}

	public WaveFormat(int sampleRate, int bits, int channels)
		: this(sampleRate, bits, channels, AudioEncoding.Pcm)
	{
	}

	public WaveFormat(int sampleRate, int bits, int channels, AudioEncoding encoding)
		: this(sampleRate, bits, channels, encoding, 0)
	{
	}

	public WaveFormat(int sampleRate, int bits, int channels, AudioEncoding encoding, int extraSize)
	{
		if (sampleRate < 1)
		{
			throw new ArgumentOutOfRangeException("sampleRate");
		}
		if (bits < 0)
		{
			throw new ArgumentOutOfRangeException("bits");
		}
		if (channels < 1)
		{
			throw new ArgumentOutOfRangeException("channels", "Number of channels has to be bigger than 0.");
		}
		_sampleRate = sampleRate;
		_bitsPerSample = (short)bits;
		_channels = (short)channels;
		_encoding = encoding;
		_extraSize = (short)extraSize;
		UpdateProperties();
	}

	public long MillisecondsToBytes(double milliseconds)
	{
		long num = (long)((double)BytesPerSecond / 1000.0 * milliseconds);
		return num - num % BlockAlign;
	}

	public double BytesToMilliseconds(long bytes)
	{
		bytes -= bytes % BlockAlign;
		return (double)bytes / (double)BytesPerSecond * 1000.0;
	}

	public virtual bool Equals(WaveFormat other)
	{
		if (Channels == other.Channels && SampleRate == other.SampleRate && BytesPerSecond == other.BytesPerSecond && BlockAlign == other.BlockAlign && BitsPerSample == other.BitsPerSample && ExtraSize == other.ExtraSize)
		{
			return WaveFormatTag == other.WaveFormatTag;
		}
		return false;
	}

	public override string ToString()
	{
		return GetInformation().ToString();
	}

	public virtual object Clone()
	{
		return MemberwiseClone();
	}

	internal virtual void SetWaveFormatTagInternal(AudioEncoding waveFormatTag)
	{
		WaveFormatTag = waveFormatTag;
	}

	internal virtual void SetBitsPerSampleAndFormatProperties(int bitsPerSample)
	{
		BitsPerSample = bitsPerSample;
		UpdateProperties();
	}

	protected internal virtual void UpdateProperties()
	{
		BlockAlign = BitsPerSample / 8 * Channels;
		BytesPerSecond = BlockAlign * SampleRate;
	}

	[DebuggerStepThrough]
	private StringBuilder GetInformation()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("ChannelsAvailable: " + Channels);
		stringBuilder.Append("|SampleRate: " + SampleRate);
		stringBuilder.Append("|Bps: " + BytesPerSecond);
		stringBuilder.Append("|BlockAlign: " + BlockAlign);
		stringBuilder.Append("|BitsPerSample: " + BitsPerSample);
		stringBuilder.Append("|Encoding: " + _encoding);
		return stringBuilder;
	}
}
