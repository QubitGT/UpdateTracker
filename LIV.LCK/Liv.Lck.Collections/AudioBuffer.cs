using System;
using System.Runtime.InteropServices;

namespace Liv.Lck.Collections;

public class AudioBuffer
{
	private float[] _buffer;

	private int _logicalCount;

	public int Count => _logicalCount;

	public int Capacity => _buffer.Length;

	public float this[int index] => _buffer[index];

	public float[] Buffer => _buffer;

	public AudioBuffer(int maxCapacity)
	{
		_buffer = new float[maxCapacity];
		_logicalCount = 0;
	}

	public void Clear()
	{
		_logicalCount = 0;
	}

	public bool TryAdd(float value)
	{
		if (_logicalCount >= _buffer.Length)
		{
			return false;
		}
		_buffer[_logicalCount] = value;
		_logicalCount++;
		return true;
	}

	public bool TryCopyFrom(float[] source, int sourceIndex, int count)
	{
		if (count > _buffer.Length)
		{
			return false;
		}
		Array.Copy(source, sourceIndex, _buffer, 0, count);
		_logicalCount = count;
		return true;
	}

	public bool TryCopyFrom(IntPtr source, int count)
	{
		if (count > _buffer.Length)
		{
			return false;
		}
		Marshal.Copy(source, _buffer, 0, count);
		_logicalCount = count;
		return true;
	}

	public bool TryCopyFrom(AudioBuffer source)
	{
		if (source._logicalCount > _buffer.Length)
		{
			return false;
		}
		Array.Copy(source._buffer, 0, _buffer, 0, source._logicalCount);
		_logicalCount = source._logicalCount;
		return true;
	}

	public bool TryExtendFrom(float[] sourceArray, int sourceIndex, int length)
	{
		if (_logicalCount + length > _buffer.Length)
		{
			return false;
		}
		Array.Copy(sourceArray, sourceIndex, _buffer, _logicalCount, length);
		_logicalCount += length;
		return true;
	}

	public bool TryExtendFrom(float[] source)
	{
		return TryExtendFrom(source, 0, source.Length);
	}

	public bool TryExtendFrom(AudioBuffer source)
	{
		if (_logicalCount + source._logicalCount > _buffer.Length)
		{
			return false;
		}
		Array.Copy(source._buffer, 0, _buffer, _logicalCount, source._logicalCount);
		_logicalCount += source._logicalCount;
		return true;
	}

	public void OverrideCount(int newCount)
	{
		_logicalCount = newCount;
	}

	public void PadAudioBuffer(int samplesToPad)
	{
		for (int i = 0; i < samplesToPad; i++)
		{
			TryAdd(0f);
		}
	}

	public void SkipAudioSamples(int samplesToSkip)
	{
		if (samplesToSkip >= Count)
		{
			Clear();
			return;
		}
		int num = Count - samplesToSkip;
		Array.Copy(Buffer, samplesToSkip, Buffer, 0, num);
		TryCopyFrom(Buffer, 0, num);
	}
}
