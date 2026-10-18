#define DEBUG
using System;
using System.Collections.Generic;

namespace Fusion;

internal class TimeSeries
{
	private double _mean;

	private double _varSum;

	private readonly RingBuffer<double> _samples;

	public int Count => _samples.Count;

	public int Capacity => _samples.Capacity;

	public bool IsEmpty => _samples.IsEmpty;

	public bool IsFull => _samples.IsFull;

	public double Latest => IsEmpty ? 0.0 : _samples.Back();

	public double Avg => _mean;

	public double Var
	{
		get
		{
			if (Count > 1 && _varSum >= 0.0)
			{
				return _varSum / (double)(Count - 1);
			}
			return 0.0;
		}
	}

	public double Dev
	{
		get
		{
			double var = Var;
			return (var >= double.Epsilon) ? Math.Sqrt(var) : 0.0;
		}
	}

	public double Min
	{
		get
		{
			double num = 0.0;
			for (int i = 0; i < Count; i++)
			{
				num = Math.Min(num, _samples[i]);
			}
			return num;
		}
	}

	public double Max
	{
		get
		{
			double num = 0.0;
			for (int i = 0; i < Count; i++)
			{
				num = Math.Max(num, _samples[i]);
			}
			return num;
		}
	}

	public double Median => FindMedian(_samples);

	public double MeanAbsDev
	{
		get
		{
			double mean = _mean;
			double num = 0.0;
			for (int i = 0; i < Count; i++)
			{
				num += Math.Abs(_samples[i] - mean);
			}
			return 1.2533 * (num / (double)Count);
		}
	}

	public double MedianAbsDev
	{
		get
		{
			double median = Median;
			List<double> list = new List<double>(Count);
			for (int i = 0; i < Count; i++)
			{
				list.Add(Math.Abs(_samples[i] - median));
			}
			return 1.4826 * FindMedian(list);
		}
	}

	public double Smoothed(double alpha)
	{
		Assert.Check(alpha >= 0.0 && alpha <= 1.0, "the input range of this method is [0.0, 1.0], inclusive");
		if (Count > 0)
		{
			double num = _samples[0];
			for (int i = 1; i < Count; i++)
			{
				num = (1.0 - alpha) * num + alpha * _samples[i];
			}
			return num;
		}
		return 0.0;
	}

	public TimeSeries(int capacity)
	{
		_mean = 0.0;
		_varSum = 0.0;
		_samples = new RingBuffer<double>(Math.Max(2, capacity));
	}

	public void Add(double value)
	{
		Assert.Check(!double.IsNaN(value));
		Assert.Check(!double.IsInfinity(value));
		double mean = _mean;
		if (IsFull)
		{
			double num = _samples.PopFront();
			_samples.PushBack(value);
			double num2 = value - num;
			_mean += num2 / (double)Capacity;
			_varSum += num2 * (value - _mean + (num - mean));
		}
		else
		{
			_samples.PushBack(value);
			double num3 = value - mean;
			_mean += num3 / (double)Count;
			_varSum += num3 * (value - _mean);
		}
	}

	public void Fill(double value)
	{
		Clear();
		for (int i = 0; i < Capacity; i++)
		{
			Add(value);
		}
	}

	public double QuantileNormal(double p)
	{
		return Avg + InverseCdfNormal(p) * Dev;
	}

	public static double InverseCdfNormal(double p)
	{
		Assert.Check(p > 0.0 && p < 1.0, "the input range of this function is (0.0, 1.0), non-inclusive");
		if (p < 0.5)
		{
			return 0.0 - Polynomial(p);
		}
		return Polynomial(1.0 - p);
		static double Polynomial(double x)
		{
			double num = Math.Sqrt(-2.0 * Math.Log(x));
			double num2 = (0.06114673576519699 * num + 1.5615337002120804) * num + 2.6539620026016846;
			double num3 = ((0.009547745327068945 * num + 0.4540555364442335) * num + 1.9048751828364987) * num + 1.0;
			return num - num2 / num3;
		}
	}

	internal static double FindMedian(IEnumerable<double> values)
	{
		return FindMedian(new List<double>(values));
	}

	internal static double FindMedian(List<double> list)
	{
		int count = list.Count;
		if (count == 0)
		{
			return 0.0;
		}
		list.Sort();
		if (count % 2 == 1)
		{
			return list[count / 2];
		}
		return 0.5 * (list[count / 2 - 1] + list[count / 2]);
	}

	public void Clear()
	{
		_mean = 0.0;
		_varSum = 0.0;
		_samples.Clear();
	}
}
