using System;
using Meta.Voice.UnityOpus;
using UnityEngine;
using UnityEngine.Scripting;

namespace Meta.Voice.Audio.Decoding;

[Preserve]
public class AudioDecoderOpus : IAudioDecoder
{
	private readonly Decoder _decoder;

	private readonly byte[] _frameBuffer;

	private readonly float[] _opusBuffer;

	private const int _headerLength = 8;

	private const int _frameMax = 240;

	private int _frameLength;

	private bool _validHeader;

	private int _frameOffset;

	public bool WillDecodeInBackground { get; set; } = true;


	public AudioDecoderOpus(int channels, int samplerate)
	{
		_decoder = new Decoder((SamplingFrequency)samplerate, (NumChannels)channels);
		_frameBuffer = new byte[240];
		_opusBuffer = new float[5760 * channels];
	}

	public void Decode(byte[] buffer, int bufferOffset, int bufferLength, AudioSampleDecodeDelegate onSamplesDecoded)
	{
		while (bufferLength > 0)
		{
			if (!_validHeader)
			{
				int num = DecodeFrameHeader(buffer, bufferOffset, bufferLength);
				bufferOffset += num;
				bufferLength -= num;
				continue;
			}
			int num2 = Mathf.Min(_frameLength - _frameOffset, bufferLength);
			if (num2 == 0)
			{
				_validHeader = false;
				_frameOffset = 0;
				continue;
			}
			Array.Copy(buffer, bufferOffset, _frameBuffer, _frameOffset, num2);
			_frameOffset += num2;
			bufferOffset += num2;
			bufferLength -= num2;
			if (_frameOffset == _frameLength)
			{
				int length = _decoder.Decode(_frameBuffer, _frameLength, _opusBuffer);
				onSamplesDecoded?.Invoke(_opusBuffer, 0, length);
				_validHeader = false;
				_frameOffset = 0;
			}
		}
	}

	private int DecodeFrameHeader(byte[] buffer, int bufferOffset, int bufferLength)
	{
		int num = Mathf.Min(8 - _frameOffset, bufferLength);
		Array.Copy(buffer, bufferOffset, _frameBuffer, _frameOffset, num);
		_frameOffset += num;
		if (_frameOffset < 8)
		{
			return num;
		}
		_frameOffset = 0;
		Array.Reverse(_frameBuffer, 0, 4);
		_frameLength = BitConverter.ToInt32(_frameBuffer, 0);
		if (_frameLength == 0)
		{
			throw new Exception("Invalid zero-length opus frame");
		}
		if (_frameLength > 240)
		{
			throw new Exception($"Frame size ({_frameLength}) exceeded max frame size ({240})");
		}
		_validHeader = true;
		return num;
	}
}
