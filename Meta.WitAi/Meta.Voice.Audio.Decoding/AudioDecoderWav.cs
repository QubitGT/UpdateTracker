using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Meta.Voice.Audio.Decoding;

[Preserve]
public class AudioDecoderWav : AudioDecoderPcm
{
	private int _subChunkOffset;

	private readonly byte[] _subChunkHeader = new byte[8];

	private bool _subChunkIsData;

	private int _subChunkLength = 12;

	private static readonly byte[] DataDescriptor = new byte[4] { 100, 97, 116, 97 };

	[Preserve]
	public AudioDecoderWav(int sampleBufferLength = 720)
		: base(AudioDecoderPcmType.Int16, sampleBufferLength)
	{
	}

	public override void Decode(byte[] buffer, int bufferOffset, int bufferLength, AudioSampleDecodeDelegate onSamplesDecoded)
	{
		while (bufferLength > 0)
		{
			if (_subChunkLength == 0)
			{
				int num = DecodeSubChunkHeader(buffer, bufferOffset, bufferLength);
				bufferOffset += num;
				bufferLength -= num;
				continue;
			}
			int num2 = Mathf.Min(_subChunkLength - _subChunkOffset, bufferLength);
			if (_subChunkIsData)
			{
				base.Decode(buffer, bufferOffset, num2, onSamplesDecoded);
			}
			_subChunkOffset += num2;
			bufferOffset += num2;
			bufferLength -= num2;
			if (_subChunkOffset >= _subChunkLength)
			{
				_subChunkOffset = 0;
				_subChunkLength = 0;
			}
		}
	}

	private int DecodeSubChunkHeader(byte[] buffer, int bufferOffset, int bufferLength)
	{
		int num = Mathf.Min(_subChunkHeader.Length - _subChunkOffset, bufferLength);
		Array.Copy(buffer, bufferOffset, _subChunkHeader, _subChunkOffset, num);
		_subChunkOffset += num;
		if (_subChunkOffset >= _subChunkHeader.Length)
		{
			_subChunkOffset = 0;
			_subChunkIsData = SubArrayEquals(_subChunkHeader, 0, DataDescriptor, 0, DataDescriptor.Length);
			_subChunkLength = (int)BitConverter.ToUInt32(_subChunkHeader, 4);
		}
		return num;
	}

	private static bool SubArrayEquals<T>(T[] array1, int offset1, T[] array2, int offset2, int length)
	{
		if (array1 == null || array2 == null || array1.Length < offset1 + length || array2.Length < offset2 + length)
		{
			return false;
		}
		for (int i = 0; i < length; i++)
		{
			if (!array1[offset1 + i].Equals(array2[offset2 + i]))
			{
				return false;
			}
		}
		return true;
	}
}
