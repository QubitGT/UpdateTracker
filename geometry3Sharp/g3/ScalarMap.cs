using System.Collections.Generic;

namespace g3;

public class ScalarMap
{
	private struct Sample
	{
		public double t;

		public double value;
	}

	private List<Sample> points = new List<Sample>();

	private Interval1d validRange;

	public ScalarMap()
	{
		validRange = Interval1d.Empty;
	}

	public void AddPoint(double t, double value)
	{
		Sample sample = default(Sample);
		sample.t = t;
		sample.value = value;
		Sample sample2 = sample;
		if (points.Count == 0)
		{
			points.Add(sample2);
			validRange.Contain(t);
			return;
		}
		if (t < points[0].t)
		{
			points.Insert(0, sample2);
			validRange.Contain(t);
			return;
		}
		for (int i = 0; i < points.Count; i++)
		{
			if (points[i].t == t)
			{
				points[i] = sample2;
				return;
			}
			if (points[i].t > t)
			{
				points.Insert(i, sample2);
				return;
			}
		}
		points.Add(sample2);
		validRange.Contain(t);
	}

	public double Linear(double t)
	{
		if (t <= points[0].t)
		{
			return points[0].value;
		}
		int count = points.Count;
		if (t >= points[count - 1].t)
		{
			return points[count - 1].value;
		}
		for (int i = 1; i < points.Count; i++)
		{
			if (points[i].t > t)
			{
				Sample sample = points[i - 1];
				Sample sample2 = points[i];
				double num = (t - sample.t) / (sample2.t - sample.t);
				return (1.0 - num) * sample.value + num * sample2.value;
			}
		}
		return points[count - 1].value;
	}
}
