using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CSCore;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public class WaveFormatExtensible : WaveFormat
{
	internal const int WaveFormatExtensibleExtraSize = 22;

	private short _samplesUnion;

	private ChannelMask _channelMask;

	private Guid _subFormat;

	public int ValidBitsPerSample
	{
		get
		{
			return _samplesUnion;
		}
		protected internal set
		{
			_samplesUnion = (short)value;
		}
	}

	public int SamplesPerBlock
	{
		get
		{
			return _samplesUnion;
		}
		protected internal set
		{
			_samplesUnion = (short)value;
		}
	}

	public ChannelMask ChannelMask
	{
		get
		{
			return _channelMask;
		}
		protected internal set
		{
			_channelMask = value;
		}
	}

	public Guid SubFormat
	{
		get
		{
			return _subFormat;
		}
		protected internal set
		{
			_subFormat = value;
		}
	}

	public static Guid SubTypeFromWaveFormat(WaveFormat waveFormat)
	{
		if (waveFormat == null)
		{
			throw new ArgumentNullException("waveFormat");
		}
		if (waveFormat is WaveFormatExtensible)
		{
			return ((WaveFormatExtensible)waveFormat).SubFormat;
		}
		return AudioSubTypes.SubTypeFromEncoding(waveFormat.WaveFormatTag);
	}

	internal WaveFormatExtensible()
	{
	}

	public WaveFormatExtensible(int sampleRate, int bits, int channels, Guid subFormat)
		: base(sampleRate, bits, channels, AudioEncoding.Extensible, 22)
	{
		_samplesUnion = (short)bits;
		_subFormat = SubTypeFromWaveFormat(this);
		int num = 0;
		for (int i = 0; i < channels; i++)
		{
			num |= 1 << i;
		}
		_channelMask = (ChannelMask)num;
		_subFormat = subFormat;
	}

	public WaveFormatExtensible(int sampleRate, int bits, int channels, Guid subFormat, ChannelMask channelMask)
		: this(sampleRate, bits, channels, subFormat)
	{
		Array values = Enum.GetValues(typeof(ChannelMask));
		int num = 0;
		for (int i = 0; i < values.Length; i++)
		{
			if ((channelMask & (ChannelMask)values.GetValue(i)) == (ChannelMask)values.GetValue(i))
			{
				num++;
			}
		}
		if (channels != num)
		{
			throw new ArgumentException("Channels has to equal the set flags in the channelmask.");
		}
		_channelMask = channelMask;
	}

	public WaveFormat ToWaveFormat()
	{
		return new WaveFormat(SampleRate, BitsPerSample, Channels, AudioSubTypes.EncodingFromSubType(SubFormat));
	}

	public override object Clone()
	{
		return MemberwiseClone();
	}

	internal override void SetWaveFormatTagInternal(AudioEncoding waveFormatTag)
	{
		SubFormat = AudioSubTypes.SubTypeFromEncoding(waveFormatTag);
	}

	[DebuggerStepThrough]
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(base.ToString());
		stringBuilder.Append("|SubFormat: " + SubFormat.ToString());
		stringBuilder.Append("|ChannelMask: " + ChannelMask);
		return stringBuilder.ToString();
	}
}
