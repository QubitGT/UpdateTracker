using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Meta.Voice.Audio.Decoding;

[Preserve]
public class AudioDecoderPcm : IAudioDecoder
{
	internal delegate float PcmDecodeDelegate(byte[] buffer, int bufferOffset);

	public readonly AudioDecoderPcmType PcmType;

	private readonly int _byteCount;

	private readonly PcmDecodeDelegate _decoder;

	private int _overflowOffset;

	private readonly byte[] _overflow;

	private readonly float[] _samples;

	public bool WillDecodeInBackground => true;

	[Preserve]
	public AudioDecoderPcm(AudioDecoderPcmType pcmType, int sampleBufferLength = 720)
	{
		PcmType = pcmType;
		_byteCount = GetByteCount(PcmType);
		_overflow = new byte[_byteCount];
		_samples = new float[sampleBufferLength];
		_decoder = GetPcmDecoder(PcmType);
	}

	public virtual void Decode(byte[] buffer, int bufferOffset, int bufferLength, AudioSampleDecodeDelegate onSamplesDecoded)
	{
		if (_overflowOffset > 0)
		{
			int num = Mathf.Min(_byteCount - _overflowOffset, bufferLength);
			Array.Copy(buffer, bufferOffset, _overflow, _overflowOffset, num);
			_samples[0] = _decoder(_overflow, 0);
			onSamplesDecoded?.Invoke(_samples, 0, 1);
			bufferOffset += num;
			bufferLength -= num;
			_overflowOffset = 0;
		}
		while (bufferLength >= _byteCount)
		{
			int num2 = Mathf.Min(Mathf.FloorToInt((float)bufferLength / (float)_byteCount), _samples.Length);
			for (int i = 0; i < num2; i++)
			{
				_samples[i] = _decoder(buffer, bufferOffset + i * _byteCount);
			}
			onSamplesDecoded?.Invoke(_samples, 0, num2);
			num2 *= _byteCount;
			bufferOffset += num2;
			bufferLength -= num2;
		}
		if (bufferLength > 0)
		{
			Array.Copy(buffer, bufferOffset, _overflow, _overflowOffset, bufferLength);
			_overflowOffset += bufferLength;
		}
	}

	public static int GetByteCount(AudioDecoderPcmType pcmType)
	{
		switch (pcmType)
		{
		case AudioDecoderPcmType.Int16:
		case AudioDecoderPcmType.UInt16:
			return 2;
		case AudioDecoderPcmType.Int32:
		case AudioDecoderPcmType.UInt32:
			return 4;
		case AudioDecoderPcmType.Int64:
		case AudioDecoderPcmType.UInt64:
			return 8;
		default:
			return 0;
		}
	}

	public static long GetTotalSamplesPcm(long contentLength, AudioDecoderPcmType pcmType = AudioDecoderPcmType.Int16)
	{
		return contentLength / GetByteCount(pcmType);
	}

	public static float[] DecodePcm(byte[] rawData, AudioDecoderPcmType pcmType = AudioDecoderPcmType.Int16)
	{
		PcmDecodeDelegate pcmDecoder = GetPcmDecoder(pcmType);
		float[] array = new float[(int)GetTotalSamplesPcm(rawData.Length, pcmType)];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = pcmDecoder(rawData, i * 2);
		}
		return array;
	}

	internal static PcmDecodeDelegate GetPcmDecoder(AudioDecoderPcmType pcmType)
	{
		return pcmType switch
		{
			AudioDecoderPcmType.Int16 => DecodeSample_Pcm16, 
			AudioDecoderPcmType.Int32 => DecodeSample_Pcm32, 
			AudioDecoderPcmType.Int64 => DecodeSample_Pcm64, 
			AudioDecoderPcmType.UInt16 => DecodeSample_PcmU16, 
			AudioDecoderPcmType.UInt32 => DecodeSample_PcmU32, 
			AudioDecoderPcmType.UInt64 => DecodeSample_PcmU64, 
			_ => DecodeSample_Pcm16, 
		};
	}

	public static float DecodeSample_Pcm16(byte[] rawData, int index)
	{
		return (float)BitConverter.ToInt16(rawData, index) / 32767f;
	}

	public static float DecodeSample_Pcm32(byte[] rawData, int index)
	{
		return (float)BitConverter.ToInt32(rawData, index) / 2.1474836E+09f;
	}

	public static float DecodeSample_Pcm64(byte[] rawData, int index)
	{
		return (float)((double)BitConverter.ToInt64(rawData, index) / 9.223372036854776E+18);
	}

	public static float DecodeSample_PcmU16(byte[] rawData, int index)
	{
		return (float)(int)BitConverter.ToUInt16(rawData, index) / 65535f;
	}

	public static float DecodeSample_PcmU32(byte[] rawData, int index)
	{
		return (float)BitConverter.ToUInt32(rawData, index) / 4.2949673E+09f;
	}

	public static float DecodeSample_PcmU64(byte[] rawData, int index)
	{
		return (float)((double)BitConverter.ToUInt64(rawData, index) / 1.8446744073709552E+19);
	}
}
