using System;
using System.Collections.Generic;
using System.Text;

namespace g3;

public class LocalProfiler : IDisposable
{
	private Dictionary<string, BlockTimer> Timers = new Dictionary<string, BlockTimer>();

	private List<string> Order = new List<string>();

	public BlockTimer Start(string label)
	{
		if (Timers.ContainsKey(label))
		{
			Timers[label].Reset();
		}
		else
		{
			Timers[label] = new BlockTimer(label, bStart: true);
			Order.Add(label);
		}
		return Timers[label];
	}

	public BlockTimer StopAllAndStartNew(string label)
	{
		StopAll();
		return Start(label);
	}

	public BlockTimer Get(string label)
	{
		return Timers[label];
	}

	public void Stop(string label)
	{
		Timers[label].Stop();
	}

	public void StopAll()
	{
		foreach (BlockTimer value in Timers.Values)
		{
			if (value.Running)
			{
				value.Stop();
			}
		}
	}

	public void StopAndAccumulate(string label, bool bReset = false)
	{
		Timers[label].Accumulate(bReset);
	}

	public void Reset(string label)
	{
		Timers[label].Reset();
	}

	public void ResetAccumulated(string label)
	{
		Timers[label].Accumulated = TimeSpan.Zero;
	}

	public void ResetAllAccumulated(string label)
	{
		foreach (BlockTimer value in Timers.Values)
		{
			value.Accumulated = TimeSpan.Zero;
		}
	}

	public void DivideAllAccumulated(int div)
	{
		foreach (BlockTimer value in Timers.Values)
		{
			value.Accumulated = new TimeSpan(value.Accumulated.Ticks / div);
		}
	}

	public string Elapsed(string label)
	{
		return Timers[label].ToString();
	}

	public string Accumulated(string label)
	{
		TimeSpan accumulated = Timers[label].Accumulated;
		return string.Format(BlockTimer.TimeFormatString(accumulated), accumulated);
	}

	public string AllTicks(string prefix = "Times:")
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(prefix + " ");
		foreach (string item in Order)
		{
			stringBuilder.Append(item + ": " + Timers[item].ToString() + " ");
		}
		return stringBuilder.ToString();
	}

	public string AllAccumulatedTicks(string prefix = "Times:")
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(prefix + " ");
		foreach (string item in Order)
		{
			stringBuilder.Append(item + ": " + Accumulated(item) + " ");
		}
		return stringBuilder.ToString();
	}

	public string AllTimes(string prefix = "Times:", string separator = " ")
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(prefix + " ");
		foreach (string item in Order)
		{
			TimeSpan elapsed = Timers[item].Watch.Elapsed;
			stringBuilder.Append(item + ": " + string.Format(BlockTimer.TimeFormatString(elapsed), elapsed) + separator);
		}
		return stringBuilder.ToString();
	}

	public string AllAccumulatedTimes(string prefix = "Times:", string separator = " ")
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(prefix + " ");
		foreach (string item in Order)
		{
			TimeSpan accumulated = Timers[item].Accumulated;
			stringBuilder.Append(item + ": " + string.Format(BlockTimer.TimeFormatString(accumulated), accumulated) + separator);
		}
		return stringBuilder.ToString();
	}

	public void Dispose()
	{
		foreach (BlockTimer value in Timers.Values)
		{
			value.Stop();
		}
		Timers.Clear();
	}
}
