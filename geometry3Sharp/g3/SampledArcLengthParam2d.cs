using System;
using System.Collections.Generic;
using System.Linq;

namespace g3;

public class SampledArcLengthParam2d : IArcLengthParam2d
{
	private double[] arc_len;

	private Vector2d[] positions;

	public double ArcLength => arc_len[arc_len.Length - 1];

	public SampledArcLengthParam2d(IEnumerable<Vector2d> samples, int nCountHint = -1)
	{
		int num = ((nCountHint == -1) ? samples.Count() : nCountHint);
		arc_len = new double[num];
		arc_len[0] = 0.0;
		positions = new Vector2d[num];
		int num2 = 0;
		Vector2d vector2d = Vector2d.Zero;
		foreach (Vector2d sample in samples)
		{
			positions[num2] = sample;
			if (num2 > 0)
			{
				double length = (sample - vector2d).Length;
				arc_len[num2] = arc_len[num2 - 1] + length;
			}
			num2++;
			vector2d = sample;
		}
	}

	public CurveSample2d Sample(double f)
	{
		if (f <= 0.0)
		{
			return new CurveSample2d(new Vector2d(positions[0]), tangent(0));
		}
		int num = arc_len.Length;
		if (f >= arc_len[num - 1])
		{
			return new CurveSample2d(new Vector2d(positions[num - 1]), tangent(num - 1));
		}
		for (int i = 0; i < num; i++)
		{
			if (f < arc_len[i])
			{
				int num2 = i - 1;
				int num3 = i;
				if (arc_len[num2] == arc_len[num3])
				{
					return new CurveSample2d(new Vector2d(positions[num2]), tangent(num2));
				}
				double t = (f - arc_len[num2]) / (arc_len[num3] - arc_len[num2]);
				return new CurveSample2d(Vector2d.Lerp(positions[num2], positions[num3], t), Vector2d.Lerp(tangent(num2), tangent(num3), t));
			}
		}
		throw new ArgumentException("SampledArcLengthParam2d.Sample: somehow arc len is outside any possible range");
	}

	protected Vector2d tangent(int i)
	{
		int num = arc_len.Length;
		if (i == 0)
		{
			return (positions[1] - positions[0]).Normalized;
		}
		if (i == num - 1)
		{
			return (positions[num - 1] - positions[num - 2]).Normalized;
		}
		return (positions[i + 1] - positions[i - 1]).Normalized;
	}
}
