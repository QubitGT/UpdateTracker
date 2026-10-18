#define DEBUG
namespace Fusion;

public struct TickAccumulator
{
	private double _time;

	private double _scale;

	private int _ticks;

	private bool _running;

	public int Pending => _ticks;

	public double Remainder => _time;

	public bool Running => _running;

	public double TimeScale
	{
		get
		{
			return _scale;
		}
		set
		{
			Assert.Check(value > 0.0);
			_scale = value;
		}
	}

	public float Alpha(double step)
	{
		return (float)Maths.Clamp01((double)_ticks / step);
	}

	public void AddTicks(int ticks)
	{
		_ticks += ticks;
	}

	public void AddTime(double dt, double step, int? maxTicks = null)
	{
		Assert.Check(dt >= 0.0);
		Assert.Check(step > 0.0);
		if (!_running)
		{
			return;
		}
		Assert.Check(_scale > 0.0);
		Assert.Check(!double.IsInfinity(dt));
		_time += dt * _scale;
		while (_time >= step)
		{
			_time -= step;
			_ticks++;
			if (maxTicks.HasValue && _ticks >= maxTicks.Value)
			{
				_time = 0.0;
				_ticks = maxTicks.Value;
				break;
			}
		}
	}

	public void Stop()
	{
		_running = false;
	}

	public void Start()
	{
		_running = true;
	}

	public bool ConsumeTick(out bool last)
	{
		Assert.Check(_ticks >= 0);
		if (_ticks > 0)
		{
			_ticks--;
			last = _ticks == 0;
			return true;
		}
		last = false;
		return false;
	}

	public static TickAccumulator StartNew()
	{
		TickAccumulator result = default(TickAccumulator);
		result.TimeScale = 1.0;
		result.Start();
		return result;
	}
}
