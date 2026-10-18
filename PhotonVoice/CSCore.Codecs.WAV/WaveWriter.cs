using System;
using System.IO;
using System.Text;

namespace CSCore.Codecs.WAV;

public class WaveWriter : IDisposable, IWriteable
{
	private readonly WaveFormat _waveFormat;

	private readonly long _waveStartPosition;

	private int _dataLength;

	private bool _isDisposed;

	private Stream _stream;

	private BinaryWriter _writer;

	private bool _isDisposing;

	private readonly bool _closeStream;

	public bool IsDisposed => _isDisposed;

	public bool IsDisposing => _isDisposing;

	public WaveWriter(string fileName, WaveFormat waveFormat)
		: this(File.OpenWrite(fileName), waveFormat)
	{
		_closeStream = true;
	}

	public WaveWriter(Stream stream, WaveFormat waveFormat)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		if (!stream.CanWrite)
		{
			throw new ArgumentException("Stream not writeable.", "stream");
		}
		if (!stream.CanSeek)
		{
			throw new ArgumentException("Stream not seekable.", "stream");
		}
		_isDisposing = false;
		_isDisposed = false;
		_stream = stream;
		_waveStartPosition = stream.Position;
		_writer = new BinaryWriter(stream);
		for (int i = 0; i < 44; i++)
		{
			_writer.Write((byte)0);
		}
		_waveFormat = waveFormat;
		WriteHeader();
		_closeStream = false;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[Obsolete("Use the Extensions.WriteToWaveStream extension instead.")]
	public static void WriteToFile(string filename, IWaveSource source, bool deleteFileIfAlreadyExists, int maxlength = -1)
	{
		if (deleteFileIfAlreadyExists && File.Exists(filename))
		{
			File.Delete(filename);
		}
		int num = 0;
		byte[] array = new byte[source.WaveFormat.BytesPerSecond];
		using WaveWriter waveWriter = new WaveWriter(filename, source.WaveFormat);
		int num2;
		while ((num2 = source.Read(array, 0, array.Length)) > 0)
		{
			waveWriter.Write(array, 0, num2);
			num += num2;
			if (maxlength != -1 && num > maxlength)
			{
				break;
			}
		}
	}

	public void WriteSample(float sample)
	{
		CheckObjectDisposed();
		if (sample < -1f || sample > 1f)
		{
			sample = Math.Max(-1f, Math.Min(1f, sample));
		}
		if (_waveFormat.IsPCM())
		{
			switch (_waveFormat.BitsPerSample)
			{
			case 8:
				Write((byte)(255f * sample));
				break;
			case 16:
				Write((short)(32767f * sample));
				break;
			case 24:
			{
				byte[] bytes = BitConverter.GetBytes((int)(8388607f * sample));
				Write(new byte[3]
				{
					bytes[0],
					bytes[1],
					bytes[2]
				}, 0, 3);
				break;
			}
			case 32:
				Write((int)(2.1474836E+09f * sample));
				break;
			default:
				throw new InvalidOperationException("Invalid Waveformat", new InvalidOperationException("Invalid BitsPerSample while using PCM encoding."));
			}
		}
		else if (_waveFormat.IsIeeeFloat())
		{
			Write(sample);
		}
		else
		{
			if (_waveFormat.WaveFormatTag != AudioEncoding.Extensible || _waveFormat.BitsPerSample != 32)
			{
				throw new InvalidOperationException("Invalid Waveformat: Waveformat has to be PCM[8, 16, 24, 32] or IeeeFloat[32]");
			}
			Write(65535 * (int)sample);
		}
	}

	public void WriteSamples(float[] samples, int offset, int count)
	{
		CheckObjectDisposed();
		for (int i = offset; i < offset + count; i++)
		{
			WriteSample(samples[i]);
		}
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		CheckObjectDisposed();
		_stream.Write(buffer, offset, count);
		_dataLength += count;
	}

	public void Write(byte value)
	{
		CheckObjectDisposed();
		_writer.Write(value);
		_dataLength++;
	}

	public void Write(short value)
	{
		CheckObjectDisposed();
		_writer.Write(value);
		_dataLength += 2;
	}

	public void Write(int value)
	{
		CheckObjectDisposed();
		_writer.Write(value);
		_dataLength += 4;
	}

	public void Write(float value)
	{
		CheckObjectDisposed();
		_writer.Write(value);
		_dataLength += 4;
	}

	private void WriteHeader()
	{
		_writer.Flush();
		long position = _stream.Position;
		_stream.Position = _waveStartPosition;
		WriteRiffHeader();
		WriteFmtChunk();
		WriteDataChunk();
		_writer.Flush();
		_stream.Position = position;
	}

	private void WriteRiffHeader()
	{
		_writer.Write(Encoding.UTF8.GetBytes("RIFF"));
		_writer.Write((int)(_stream.Length - 8));
		_writer.Write(Encoding.UTF8.GetBytes("WAVE"));
	}

	private void WriteFmtChunk()
	{
		AudioEncoding audioEncoding = _waveFormat.WaveFormatTag;
		if (audioEncoding == AudioEncoding.Extensible && _waveFormat is WaveFormatExtensible)
		{
			audioEncoding = AudioSubTypes.EncodingFromSubType((_waveFormat as WaveFormatExtensible).SubFormat);
		}
		_writer.Write(Encoding.UTF8.GetBytes("fmt "));
		_writer.Write(16);
		_writer.Write((short)audioEncoding);
		_writer.Write((short)_waveFormat.Channels);
		_writer.Write(_waveFormat.SampleRate);
		_writer.Write(_waveFormat.BytesPerSecond);
		_writer.Write((short)_waveFormat.BlockAlign);
		_writer.Write((short)_waveFormat.BitsPerSample);
	}

	private void WriteDataChunk()
	{
		_writer.Write(Encoding.UTF8.GetBytes("data"));
		_writer.Write(_dataLength);
	}

	private void CheckObjectDisposed()
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException("WaveWriter");
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_isDisposed || !disposing)
		{
			return;
		}
		try
		{
			_isDisposing = true;
			WriteHeader();
		}
		catch (Exception)
		{
		}
		finally
		{
			if (_closeStream)
			{
				if (_writer != null)
				{
					_writer.Close();
					_writer = null;
				}
				if (_stream != null)
				{
					_stream.Dispose();
					_stream = null;
				}
			}
			_isDisposing = false;
		}
		_isDisposed = true;
	}

	~WaveWriter()
	{
		Dispose(disposing: false);
	}
}
