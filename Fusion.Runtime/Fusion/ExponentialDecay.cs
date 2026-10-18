#define DEBUG
using System;

namespace Fusion;

internal class ExponentialDecay
{
	public double Fraction { get; internal set; }

	public double Time { get; internal set; }

	public double TimeScale { get; internal set; }

	public double Rate => Math.Log(Fraction) / (Time * TimeScale);

	public double Calculate(double elapsed)
	{
		return Math.Exp(Rate * elapsed);
	}

	public double CalculateLimit(double period)
	{
		double num = Calculate(period);
		return 1.0 / (1.0 - num);
	}

	public ExponentialDecay(double fraction, double time)
	{
		Assert.Check(fraction > 0.0, fraction <= 1.0);
		Assert.Check(time > 0.0);
		Fraction = fraction;
		Time = time;
		TimeScale = 1.0;
	}
}
