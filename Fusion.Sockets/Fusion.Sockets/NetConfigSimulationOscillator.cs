using System;

namespace Fusion.Sockets;

public struct NetConfigSimulationOscillator
{
	public enum WaveShape
	{
		Noise,
		Sine,
		Square,
		Triangle,
		Saw,
		ReverseSaw
	}

	public WaveShape Shape;

	public double Min;

	public double Max;

	public double Period;

	public double Threshold;

	public double Additional;

	public double GetCurveValue(Random rng, double elapsedSecs)
	{
		double num;
		if (Period == 0.0 && Shape != 0)
		{
			num = Min + (Max - Min) * 0.5;
		}
		else if (Min == Max)
		{
			num = Min;
		}
		else
		{
			double num2 = Shape switch
			{
				WaveShape.Noise => rng.NextDouble(), 
				WaveShape.Sine => Math.Sin(elapsedSecs * 2.0 * Math.PI / Period) * 0.5 + 0.5, 
				WaveShape.Square => (elapsedSecs / Period % 1.0 > 0.5) ? 1 : 0, 
				WaveShape.Triangle => Math.Abs(elapsedSecs / Period % 1.0 * 2.0 - 1.0), 
				WaveShape.Saw => elapsedSecs / Period % 1.0, 
				WaveShape.ReverseSaw => 1.0 - elapsedSecs / Period % 1.0, 
				_ => 0.0, 
			};
			num2 = ((num2 > Threshold) ? num2 : 0.0);
			num = Min + (Max - Min) * num2;
		}
		double num3 = Additional * (rng.NextDouble() - 0.5);
		return num + num3;
	}
}
