using System;

namespace Fusion;

internal struct TimerDelta
{
	private Timer _timer;

	private double _timerLast;

	public bool IsRunning => _timer.IsRunning;

	public double Consume()
	{
		double elapsedInSeconds = _timer.ElapsedInSeconds;
		double result = Math.Max(elapsedInSeconds - _timerLast, 0.0);
		_timerLast = elapsedInSeconds;
		return result;
	}

	public double Peek()
	{
		return Math.Max(_timer.ElapsedInSeconds - _timerLast, 0.0);
	}

	public static TimerDelta StartNew()
	{
		TimerDelta result = default(TimerDelta);
		result._timer = Timer.StartNew();
		return result;
	}
}
