using System;
using System.IO;
using Meta.Voice.NLayer.Decoder;

namespace Meta.Voice.NLayer;

public class MpegFile : IDisposable
{
	private Stream _stream;

	private bool _closeStream;

	private bool _eofFound;

	private MpegStreamReader _reader;

	private MpegFrameDecoder _decoder;

	private object _seekLock = new object();

	private long _position;

	private float[] _readBuf = new float[2304];

	private int _readBufLen;

	private int _readBufOfs;

	public int SampleRate => _reader.SampleRate;

	public int Channels => _reader.Channels;

	public bool CanSeek => _reader.CanSeek;

	public long Length => _reader.SampleCount * _reader.Channels * 4;

	public TimeSpan Duration
	{
		get
		{
			long sampleCount = _reader.SampleCount;
			if (sampleCount == -1)
			{
				return TimeSpan.Zero;
			}
			return TimeSpan.FromSeconds((double)sampleCount / (double)_reader.SampleRate);
		}
	}

	public long Position
	{
		get
		{
			return _position;
		}
		set
		{
			if (!_reader.CanSeek)
			{
				throw new InvalidOperationException("Cannot Seek!");
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			long num = value / 4 / _reader.Channels;
			int num2 = 0;
			if (num >= _reader.FirstFrameSampleCount)
			{
				num2 = _reader.FirstFrameSampleCount;
				num -= num2;
			}
			lock (_seekLock)
			{
				long num3 = _reader.SeekTo(num);
				if (num3 == -1)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				_decoder.Reset();
				if (num2 != 0)
				{
					_decoder.DecodeFrame(_reader.NextFrame(), _readBuf, 0);
					num3 += num2;
				}
				_position = num3 * 4 * _reader.Channels;
				_eofFound = false;
				_readBufOfs = (_readBufLen = 0);
			}
		}
	}

	public TimeSpan Time
	{
		get
		{
			return TimeSpan.FromSeconds((double)_position / 4.0 / (double)_reader.Channels / (double)_reader.SampleRate);
		}
		set
		{
			Position = (long)(value.TotalSeconds * (double)_reader.SampleRate * (double)_reader.Channels * 4.0);
		}
	}

	public StereoMode StereoMode
	{
		get
		{
			return _decoder.StereoMode;
		}
		set
		{
			_decoder.StereoMode = value;
		}
	}

	public MpegFile(string fileName)
	{
		Init(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), closeStream: true);
	}

	public MpegFile(Stream stream)
	{
		Init(stream, closeStream: false);
	}

	private void Init(Stream stream, bool closeStream)
	{
		_stream = stream;
		_closeStream = closeStream;
		_reader = new MpegStreamReader(_stream);
		_decoder = new MpegFrameDecoder();
	}

	public void Dispose()
	{
		if (_closeStream)
		{
			_stream.Dispose();
			_closeStream = false;
		}
	}

	public void SetEQ(float[] eq)
	{
		_decoder.SetEQ(eq);
	}

	public int ReadSamples(byte[] buffer, int index, int count)
	{
		if (index < 0 || index + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		count -= count % 4;
		return ReadSamplesImpl(buffer, index, count, 32);
	}

	public int ReadSamples(float[] buffer, int index, int count)
	{
		if (index < 0 || index + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ReadSamplesImpl(buffer, index * 4, count * 4, 32) / 4;
	}

	public int ReadSamplesInt16(byte[] buffer, int index, int count)
	{
		if (index < 0 || index + count > buffer.Length * 2)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ReadSamplesImpl(buffer, index, count, 16) * 2 / 4;
	}

	public int ReadSamplesInt8(byte[] buffer, int index, int count)
	{
		if (index < 0 || index + count > buffer.Length * 4)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return ReadSamplesImpl(buffer, index, count, 8) / 4;
	}

	private int ReadSamplesImpl(Array buffer, int index, int count, int bitDepth)
	{
		int num = 0;
		lock (_seekLock)
		{
			while (count > 0)
			{
				if (_readBufLen > _readBufOfs)
				{
					int num2 = _readBufLen - _readBufOfs;
					if (num2 > count)
					{
						num2 = count;
					}
					if (bitDepth == 32)
					{
						Buffer.BlockCopy(_readBuf, _readBufOfs, buffer, index, num2);
					}
					else
					{
						for (int i = 0; i < num2 / 4; i++)
						{
							switch (bitDepth)
							{
							case 8:
								buffer.SetValue((byte)Math.Round(127.5f * _readBuf[_readBufOfs / 4 + i] + 127.5f), index / 4 + i);
								break;
							case 16:
							{
								int num3 = (int)Math.Round(32767.5f * _readBuf[_readBufOfs / 4 + i] - 0.5f);
								if (num3 < 0)
								{
									num3 += 65536;
								}
								buffer.SetValue((byte)(num3 % 256), 2 * (index / 4 + i));
								buffer.SetValue((byte)(num3 / 256), 2 * (index / 4 + i) + 1);
								break;
							}
							}
						}
					}
					num += num2;
					count -= num2;
					index += num2;
					_position += num2;
					_readBufOfs += num2;
					if (_readBufOfs == _readBufLen)
					{
						_readBufLen = 0;
					}
				}
				if (_readBufLen != 0)
				{
					continue;
				}
				if (_eofFound)
				{
					break;
				}
				MpegFrame mpegFrame = _reader.NextFrame();
				if (mpegFrame == null)
				{
					_eofFound = true;
					break;
				}
				try
				{
					_readBufLen = _decoder.DecodeFrame(mpegFrame, _readBuf, 0) * 4;
					_readBufOfs = 0;
				}
				catch (InvalidDataException)
				{
					_decoder.Reset();
					_readBufOfs = (_readBufLen = 0);
				}
				catch (EndOfStreamException)
				{
					_eofFound = true;
					break;
				}
				finally
				{
					mpegFrame.ClearBuffer();
				}
			}
		}
		return num;
	}
}
