using System;
using System.IO;
using System.Text;
using Lib.Wit.Runtime.Utilities.Logging;
using Meta.Voice.Logging;
using Meta.Voice.NLayer;
using UnityEngine;
using UnityEngine.Scripting;

namespace Meta.Voice.Audio.Decoding;

[Preserve]
internal class AudioDecoderMp3Frame : IMpegFrame, ILogSource
{
	private readonly byte[] _dataBuffer = new byte[192];

	private int _dataOffset;

	private const int HeaderLength = 4;

	private readonly float[] _sampleBuffer = new float[576];

	private readonly MpegFrameDecoder _decoder = new MpegFrameDecoder();

	private int _readOffset;

	private ulong _bitBucket;

	private int _bitsRead;

	private uint _frameIndex;

	private static readonly int[][][] _bitRateTable = new int[2][][]
	{
		new int[3][]
		{
			new int[15]
			{
				0, 32, 64, 96, 128, 160, 192, 224, 256, 288,
				320, 352, 384, 416, 448
			},
			new int[15]
			{
				0, 32, 48, 56, 64, 80, 96, 112, 128, 160,
				192, 224, 256, 320, 384
			},
			new int[15]
			{
				0, 32, 40, 48, 56, 64, 80, 96, 112, 128,
				160, 192, 224, 256, 320
			}
		},
		new int[3][]
		{
			new int[15]
			{
				0, 32, 48, 56, 64, 80, 96, 112, 128, 144,
				160, 176, 192, 224, 256
			},
			new int[15]
			{
				0, 8, 16, 24, 32, 40, 48, 56, 64, 80,
				96, 112, 128, 144, 160
			},
			new int[15]
			{
				0, 8, 16, 24, 32, 40, 48, 56, 64, 80,
				96, 112, 128, 144, 160
			}
		}
	};

	private const int frameSyncMask = 2047;

	public IVLogger Logger { get; } = LoggerRegistry.Instance.GetLogger(LogCategory.Audio);


	public bool IsHeaderDecoded => _dataOffset >= 4;

	public MpegVersion Version { get; private set; }

	public MpegLayer Layer { get; private set; }

	public MpegChannelMode ChannelMode { get; private set; }

	public int ChannelModeExtension { get; private set; }

	public int BitRateIndex { get; private set; }

	public int BitRate { get; private set; }

	public int SampleRateIndex { get; private set; }

	public int SampleRate { get; private set; }

	public bool IsCopyrighted { get; private set; }

	public bool HasCrc { get; private set; }

	public bool IsCorrupted { get; private set; }

	public int FrameLength { get; private set; }

	public int SampleCount { get; private set; }

	private void Clear()
	{
		_dataOffset = 0;
		Reset();
	}

	public void Reset()
	{
		_readOffset = 4 + (HasCrc ? 2 : 0);
		_bitBucket = 0uL;
		_bitsRead = 0;
	}

	public int Decode(byte[] buffer, int bufferOffset, int bufferLength, AudioSampleDecodeDelegate onSamplesDecoded)
	{
		int num = 0;
		if (!IsHeaderDecoded)
		{
			num = Mathf.Min(4 - _dataOffset, bufferLength);
			Array.Copy(buffer, bufferOffset, _dataBuffer, _dataOffset, num);
			_dataOffset += num;
			if (!IsHeaderDecoded)
			{
				return num;
			}
			try
			{
				DecodeHeader();
			}
			catch (Exception ex)
			{
				Logger.Error("MP3 Frame {0} - Header Decode Failed\n\n{1}\n{2}", _frameIndex, ex, this);
				_frameIndex++;
				Clear();
				return num;
			}
			if (_dataBuffer.Length < FrameLength)
			{
				Logger.Error("MP3 Frame {0} - Data Buffer Needs Increase\nNew Frame Length: {1}\nOld Frame Length: {2}\n{3}", _frameIndex, FrameLength, _dataBuffer.Length, this);
			}
			if (_sampleBuffer.Length < SampleCount)
			{
				Logger.Error("MP3 Frame {0} - Sample Buffer Needs Increase\nNew Sample Count: {1}\nOld Sample Count: {2}\n{3}", _frameIndex, SampleCount, _sampleBuffer.Length, this);
			}
		}
		int num2 = Mathf.Min(FrameLength - _dataOffset, bufferLength - num);
		Array.Copy(buffer, bufferOffset + num, _dataBuffer, _dataOffset, num2);
		_dataOffset += num2;
		num += num2;
		if (_dataOffset < FrameLength)
		{
			return num;
		}
		int length = _decoder.DecodeFrame(this, _sampleBuffer, 0);
		onSamplesDecoded?.Invoke(_sampleBuffer, 0, length);
		_frameIndex++;
		Clear();
		return num;
	}

	public static void Reverse<T>(T[] array, int start, int length)
	{
		for (int i = 0; i < length / 2; i++)
		{
			int num = start + i;
			int num2 = start + length - i - 1;
			int num3 = num;
			int num4 = num2;
			T val = array[num2];
			T val2 = array[num];
			array[num3] = val;
			array[num4] = val2;
		}
	}

	private void DecodeHeader()
	{
		Reverse(_dataBuffer, 0, 4);
		int num = BitConverter.ToInt32(_dataBuffer, 0);
		if ((BitRShift(num, 21) & 0x7FF) != 2047)
		{
			throw new Exception($"Invalid frame {_frameIndex} sync\nBits: {GetBitString(num)}");
		}
		Version = GetMpegVersion(num);
		Layer = GetMpegLayer(num);
		HasCrc = (BitRShift(num, 16) & 1) == 0;
		BitRateIndex = BitRShift(num, 12) & 0xF;
		if (BitRateIndex > 0)
		{
			BitRate = _bitRateTable[(int)Version / 10 - 1][(int)(Layer - 1)][BitRateIndex] * 1000;
			SampleRateIndex = BitRShift(num, 10) & 3;
			switch (SampleRateIndex)
			{
			case 0:
				SampleRate = 44100;
				break;
			case 1:
				SampleRate = 48000;
				break;
			case 2:
				SampleRate = 32000;
				break;
			default:
				SampleRate = 0;
				throw new Exception($"Invalid frame {_frameIndex} Mpeg sample rate index\nBits: {GetBitString(num)}");
			}
			if (Version == MpegVersion.Version2)
			{
				SampleRate /= 2;
			}
			else if (Version == MpegVersion.Version25)
			{
				SampleRate /= 4;
			}
			if (Layer == MpegLayer.LayerI)
			{
				SampleCount = 384;
			}
			else if (Layer == MpegLayer.LayerIII && Version > MpegVersion.Version1)
			{
				SampleCount = 576;
			}
			else
			{
				SampleCount = 1152;
			}
			int num2 = BitRShift(num, 9) & 1;
			ChannelMode = (MpegChannelMode)(BitRShift(num, 6) & 3);
			ChannelModeExtension = BitRShift(num, 4) & 3;
			IsCopyrighted = (BitRShift(num, 3) & 1) != 0;
			if (BitRateIndex > 0)
			{
				if (Layer == MpegLayer.LayerI)
				{
					FrameLength = 12 * BitRate / SampleRate + num2;
					FrameLength <<= 2;
				}
				else
				{
					FrameLength = 144 * BitRate / SampleRate;
					if (Version == MpegVersion.Version2 || Version == MpegVersion.Version25)
					{
						FrameLength >>= 1;
					}
					FrameLength += num2;
				}
			}
			else
			{
				FrameLength = _bitsRead + GetSideDataSize() + num2;
				BitRate = (FrameLength * 8 * SampleRate / SampleCount + 499 + 500) / 1000 * 1000;
			}
			IsCorrupted = false;
			return;
		}
		throw new Exception($"Invalid frame {_frameIndex} bitrate index\nBits: {GetBitString(num)}");
	}

	private static MpegVersion GetMpegVersion(int header)
	{
		return (BitRShift(header, 19) & 3) switch
		{
			1 => MpegVersion.Version1, 
			2 => MpegVersion.Version2, 
			0 => MpegVersion.Version25, 
			_ => throw new Exception("Invalid Mpeg Version\nBits: " + GetBitString(header)), 
		};
	}

	private static MpegLayer GetMpegLayer(int header)
	{
		int num = (4 - BitRShift(header, 17)) & 3;
		if (num == 0)
		{
			throw new Exception("Invalid frame Mpeg Layer\nBits: " + GetBitString(header));
		}
		return (MpegLayer)num;
	}

	internal static int BitRShift(int number, int bits)
	{
		if (number >= 0)
		{
			return number >> bits;
		}
		return (number >> bits) + (2 << ~bits);
	}

	internal int GetSideDataSize()
	{
		switch (Layer)
		{
		case MpegLayer.LayerI:
			if (ChannelMode == MpegChannelMode.Mono)
			{
				return 16;
			}
			if (ChannelMode == MpegChannelMode.Stereo || ChannelMode == MpegChannelMode.DualChannel)
			{
				return 32;
			}
			switch (ChannelModeExtension)
			{
			case 0:
				return 18;
			case 1:
				return 20;
			case 2:
				return 22;
			case 3:
				return 24;
			}
			break;
		case MpegLayer.LayerII:
			return 0;
		case MpegLayer.LayerIII:
			if (ChannelMode == MpegChannelMode.Mono && Version >= MpegVersion.Version2)
			{
				return 9;
			}
			if (ChannelMode != MpegChannelMode.Mono && Version < MpegVersion.Version2)
			{
				return 32;
			}
			return 17;
		}
		return 0;
	}

	public int ReadBits(int bitCount)
	{
		if (bitCount < 1 || bitCount > 32)
		{
			throw new ArgumentOutOfRangeException("bitCount");
		}
		if (IsCorrupted)
		{
			return 0;
		}
		while (_bitsRead < bitCount)
		{
			int num = ReadByte(_readOffset);
			if (num == -1)
			{
				throw new EndOfStreamException();
			}
			_readOffset++;
			_bitBucket <<= 8;
			_bitBucket |= (byte)(num & 0xFF);
			_bitsRead += 8;
		}
		int result = (int)((long)(_bitBucket >> _bitsRead - bitCount) & ((1L << bitCount) - 1));
		_bitsRead -= bitCount;
		return result;
	}

	private int ReadByte(int offset)
	{
		if (_dataBuffer == null || offset < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		return _dataBuffer[offset];
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("MP3 Frame Data");
		if (!IsHeaderDecoded)
		{
			stringBuilder.AppendLine("\tNot yet decoded");
		}
		else
		{
			int headerData = BitConverter.ToInt32(_dataBuffer, 0);
			stringBuilder.AppendLine("\tBits: " + GetBitString(headerData));
			stringBuilder.AppendLine("\tRaw: " + BitConverter.ToString(_dataBuffer));
			stringBuilder.AppendLine("\tVersion: " + Version);
			stringBuilder.AppendLine("\tLayer: " + Layer);
			stringBuilder.AppendLine("\tChannel Mode: " + ChannelMode);
			stringBuilder.AppendLine($"\tCrc: {HasCrc}");
			stringBuilder.AppendLine($"\tCopyright: {IsCopyrighted}");
			stringBuilder.AppendLine($"\tBit Rate[{BitRateIndex}]: {BitRate}");
			stringBuilder.AppendLine($"\tSample Rate[{SampleRateIndex}]: {SampleRate}");
			stringBuilder.AppendLine($"\tSample Count: {SampleCount}");
			stringBuilder.AppendLine($"\tFrame Length: {FrameLength}");
		}
		return stringBuilder.ToString();
	}

	internal static string GetBitString(int headerData)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int num = 31; num >= 0; num--)
		{
			stringBuilder.Append(BitRShift(headerData, num) & 1);
			if (num % 8 == 0 && num > 0)
			{
				stringBuilder.Append(" ");
			}
		}
		return stringBuilder.ToString();
	}
}
