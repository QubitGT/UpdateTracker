using System;
using System.Runtime.InteropServices;
using POpusCodec.Enums;
using Photon.Voice;

namespace POpusCodec;

public class OpusDecoder<T> : IDisposable
{
	private const bool UseInbandFEC = true;

	private bool TisFloat;

	private int sizeofT;

	private IntPtr _handle = IntPtr.Zero;

	private const int MaxFrameSize = 5760;

	private int _channelCount;

	private static readonly T[] EmptyBuffer = new T[0];

	private Bandwidth? _previousPacketBandwidth;

	private T[] buffer;

	private FrameBuffer prevPacketData;

	private bool prevPacketInvalid;

	public Bandwidth? PreviousPacketBandwidth => _previousPacketBandwidth;

	public OpusDecoder(SamplingRate outputSamplingRateHz, Channels numChannels)
	{
		TisFloat = default(T) is float;
		sizeofT = Marshal.SizeOf(default(T));
		if (outputSamplingRateHz != SamplingRate.Sampling08000 && outputSamplingRateHz != SamplingRate.Sampling12000 && outputSamplingRateHz != SamplingRate.Sampling16000 && outputSamplingRateHz != SamplingRate.Sampling24000 && outputSamplingRateHz != SamplingRate.Sampling48000)
		{
			throw new ArgumentOutOfRangeException("outputSamplingRateHz", "Must use one of the pre-defined sampling rates (" + outputSamplingRateHz.ToString() + ")");
		}
		if (numChannels != Channels.Mono && numChannels != Channels.Stereo)
		{
			throw new ArgumentOutOfRangeException("numChannels", "Must be Mono or Stereo");
		}
		_channelCount = (int)numChannels;
		_handle = Wrapper.opus_decoder_create(outputSamplingRateHz, numChannels);
		if (_handle == IntPtr.Zero)
		{
			throw new OpusException(OpusStatusCode.AllocFail, "Memory was not allocated for the encoder");
		}
	}

	public T[] DecodePacket(ref FrameBuffer packetData)
	{
		if (buffer == null && packetData.Array == null)
		{
			return EmptyBuffer;
		}
		int num = 0;
		if (buffer == null)
		{
			buffer = new T[5760 * _channelCount];
		}
		bool flag = packetData.Array == null || Wrapper.opus_packet_get_bandwidth(packetData.Ptr) == -4;
		bool flag2 = false;
		if (prevPacketInvalid)
		{
			num = ((!flag) ? (TisFloat ? Wrapper.opus_decode(_handle, packetData, buffer as float[], 1, _channelCount) : Wrapper.opus_decode(_handle, packetData, buffer as short[], 1, _channelCount)) : (TisFloat ? Wrapper.opus_decode(_handle, default(FrameBuffer), buffer as float[], 0, _channelCount) : Wrapper.opus_decode(_handle, default(FrameBuffer), buffer as short[], 0, _channelCount)));
		}
		else if (prevPacketData.Array != null)
		{
			num = (TisFloat ? Wrapper.opus_decode(_handle, prevPacketData, buffer as float[], 0, _channelCount) : Wrapper.opus_decode(_handle, prevPacketData, buffer as short[], 0, _channelCount));
			flag2 = true;
		}
		prevPacketData.Release();
		prevPacketData = packetData;
		packetData.Retain();
		prevPacketInvalid = flag;
		if (num == 0)
		{
			return EmptyBuffer;
		}
		if (buffer.Length != num * _channelCount)
		{
			if (!flag2)
			{
				return EmptyBuffer;
			}
			T[] src = buffer;
			buffer = new T[num * _channelCount];
			Buffer.BlockCopy(src, 0, buffer, 0, num * sizeofT);
		}
		return buffer;
	}

	public T[] DecodeEndOfStream()
	{
		int num = 0;
		if (!prevPacketInvalid)
		{
			if (buffer == null)
			{
				buffer = new T[5760 * _channelCount];
			}
			if (prevPacketData.Array != null)
			{
				num = (TisFloat ? Wrapper.opus_decode(_handle, prevPacketData, buffer as float[], 1, _channelCount) : Wrapper.opus_decode(_handle, prevPacketData, buffer as short[], 1, _channelCount));
			}
			prevPacketData.Release();
			prevPacketData = default(FrameBuffer);
			prevPacketInvalid = false;
			if (num == 0)
			{
				return EmptyBuffer;
			}
			if (buffer.Length != num * _channelCount)
			{
				T[] src = buffer;
				buffer = new T[num * _channelCount];
				Buffer.BlockCopy(src, 0, buffer, 0, num * sizeofT);
			}
			return buffer;
		}
		prevPacketData.Release();
		prevPacketData = default(FrameBuffer);
		prevPacketInvalid = false;
		return EmptyBuffer;
	}

	public void Dispose()
	{
		prevPacketData.Release();
		if (_handle != IntPtr.Zero)
		{
			Wrapper.opus_decoder_destroy(_handle);
			_handle = IntPtr.Zero;
		}
	}
}
