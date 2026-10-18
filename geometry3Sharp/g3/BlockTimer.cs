using System;
using System.Diagnostics;

namespace g3;

public class BlockTimer
{
	public Stopwatch Watch;

	public string Label;

	public TimeSpan Accumulated;

	private const string minute_format = "{0:mm}:{0:ss}.{0:fffffff}";

	private const string second_format = "{0:ss}.{0:fffffff}";

	public bool Running => Watch.IsRunning;

	public string AccumulatedString => string.Format(TimeFormatString(Accumulated), Accumulated);

	public BlockTimer(string label, bool bStart)
	{
		Label = label;
		Watch = new Stopwatch();
		if (bStart)
		{
			Watch.Start();
		}
		Accumulated = TimeSpan.Zero;
	}

	public void Start()
	{
		Watch.Start();
	}

	public void Stop()
	{
		Watch.Stop();
	}

	public void Accumulate(bool bReset = false)
	{
		Watch.Stop();
		Accumulated += Watch.Elapsed;
		if (bReset)
		{
			Watch.Reset();
		}
	}

	public void Reset()
	{
		Watch.Stop();
		Watch.Reset();
		Watch.Start();
	}

	public override string ToString()
	{
		_ = Watch.Elapsed;
		return string.Format(TimeFormatString(Accumulated), Watch.Elapsed);
	}

	public static string TimeFormatString(TimeSpan span)
	{
		if (span.Minutes > 0)
		{
			return "{0:mm}:{0:ss}.{0:fffffff}";
		}
		return "{0:ss}.{0:fffffff}";
	}
}
