using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Fusion;

public struct Timer
{
	private long _start;

	private long _elapsed;

	private byte _running;

	public long ElapsedInTicks
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (_running == 1) ? (_elapsed + GetDelta()) : _elapsed;
		}
	}

	public double ElapsedInMilliseconds
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return ElapsedInSeconds * 1000.0;
		}
	}

	public double ElapsedInSeconds
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return (double)ElapsedInTicks / (double)Stopwatch.Frequency;
		}
	}

	public bool IsRunning
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return _running == 1;
		}
	}

	public static Timer StartNew()
	{
		Timer result = default(Timer);
		result.Start();
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Start()
	{
		if (_running == 0)
		{
			_start = Stopwatch.GetTimestamp();
			_running = 1;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Stop()
	{
		long delta = GetDelta();
		if (_running == 1)
		{
			_elapsed += delta;
			_running = 0;
			if (_elapsed < 0)
			{
				_elapsed = 0L;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Reset()
	{
		_elapsed = 0L;
		_running = 0;
		_start = 0L;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Restart()
	{
		_elapsed = 0L;
		_running = 1;
		_start = Stopwatch.GetTimestamp();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private long GetDelta()
	{
		return Stopwatch.GetTimestamp() - _start;
	}
}
