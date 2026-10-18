#define DEBUG
using System;

namespace Fusion;

internal class Histogram
{
	public enum QuantileEstimator
	{
		HyndmanFanType1,
		HyndmanFanType2,
		HyndmanFanType3,
		HyndmanFanType4,
		HyndmanFanType5,
		HyndmanFanType6,
		HyndmanFanType7,
		HyndmanFanType8,
		HyndmanFanType9
	}

	private int resolution;

	private int minExp;

	private int maxExp;

	private double count;

	private double zeroCount;

	private readonly RingBuffer<double> bins;

	internal const int MaxResolution = 20;

	internal const int MaxCapacity = 2048;

	public double Count => count;

	public Histogram()
		: this(256)
	{
	}

	public Histogram(double min, double max, double error)
		: this(Contrast(min, max), Resolution(error))
	{
		Assert.Check(error > 0.0);
	}

	public Histogram(double contrast, int resolution)
		: this(Capacity(contrast, resolution))
	{
		Assert.Check(resolution <= 20);
	}

	public Histogram(int capacity)
	{
		if (capacity > 2048)
		{
			InternalLogStreams.LogWarn?.Log($"The requested histogram capacity was more than is allowed: {capacity} > {2048}. Limited capacity to the maximum.");
			capacity = 2048;
		}
		resolution = 20;
		minExp = 0;
		maxExp = 0;
		count = 0.0;
		zeroCount = 0.0;
		bins = new RingBuffer<double>(capacity);
	}

	public void Print()
	{
		InternalLogStreams.LogDebug?.Log($"resolution: {resolution} => base: {Base()}");
		InternalLogStreams.LogDebug?.Log($"worst-case estimation error: {MaxRelativeSampleError() * 100.0}%");
		InternalLogStreams.LogDebug?.Log($"worst-case quantile estimation error: {MaxRelativeQuantileError() * 100.0}%");
		for (int i = 0; i < bins.Count; i++)
		{
			InternalLogStreams.LogDebug?.Log($"bin[{BinExponent(i)}] = [{BinLowerBound(i)}, {BinLowerBound(i + 1)}) = {bins[i]}");
		}
	}

	public void Clear()
	{
		resolution = 20;
		minExp = 0;
		maxExp = 0;
		count = 0.0;
		zeroCount = 0.0;
		bins.Clear();
	}

	public void Record(double value)
	{
		Record(value, 1.0);
	}

	public void Record(double value, double count)
	{
		if (double.IsNaN(value) || double.IsInfinity(value))
		{
			return;
		}
		if (value == 0.0)
		{
			zeroCount += count;
			this.count += count;
			return;
		}
		int num = Exponent(Math.Abs(value));
		if (bins.IsEmpty)
		{
			minExp = num;
			maxExp = num;
			bins.PushBack(0.0);
		}
		if (num < minExp)
		{
			int num2 = bins.Capacity - bins.Count;
			if (num < minExp - num2)
			{
				num >>= Downsample(num, maxExp);
			}
			minExp = num;
			int num3 = maxExp - minExp + 1;
			for (int i = bins.Count; i < num3; i++)
			{
				bins.PushFront(0.0);
			}
		}
		if (num > maxExp)
		{
			int num4 = bins.Capacity - bins.Count;
			if (num > maxExp + num4)
			{
				num >>= Downsample(minExp, num);
			}
			maxExp = num;
			int num5 = maxExp - minExp + 1;
			for (int j = bins.Count; j < num5; j++)
			{
				bins.PushBack(0.0);
			}
		}
		int index = BinIndex(num);
		bins[index] += count;
		this.count += count;
	}

	public void Normalize()
	{
		Rescale(1.0 / count);
	}

	public void Rescale(double scaleFactor)
	{
		if (!double.IsNaN(scaleFactor) && !double.IsInfinity(scaleFactor))
		{
			count *= scaleFactor;
			zeroCount *= scaleFactor;
			for (int i = 0; i < bins.Count; i++)
			{
				bins[i] *= scaleFactor;
			}
		}
	}

	public double Mean()
	{
		try
		{
			double num = 0.0;
			for (int i = 0; i < bins.Count; i++)
			{
				num += bins[i] * BinMidpoint(i);
			}
			return num / count;
		}
		catch
		{
			return 0.0;
		}
	}

	public double MeanGeometric()
	{
		try
		{
			double num = 0.0;
			for (int i = 0; i < bins.Count; i++)
			{
				num += bins[i] * Math.Log(BinMidpoint(i));
			}
			return Math.Exp(num / count);
		}
		catch
		{
			return 0.0;
		}
	}

	public double MeanHarmonic()
	{
		try
		{
			double num = 0.0;
			for (int i = 0; i < bins.Count; i++)
			{
				num += bins[i] * (1.0 / BinMidpoint(i));
			}
			return count / num;
		}
		catch
		{
			return 0.0;
		}
	}

	public double Variance()
	{
		try
		{
			double num = Mean();
			double num2 = 0.0;
			for (int i = 0; i < bins.Count; i++)
			{
				double num3 = BinMidpoint(i) - num;
				num2 += bins[i] * (num3 * num3);
			}
			return num2 / (count - 1.0);
		}
		catch
		{
			return 0.0;
		}
	}

	public double Quantile(double fraction)
	{
		return QuantileWithEstimator(fraction, QuantileEstimator.HyndmanFanType7);
	}

	public double QuantileWithEstimator(double fraction, QuantileEstimator estimator)
	{
		Assert.Check(fraction >= 0.0 && fraction <= 1.0);
		try
		{
			double num;
			switch (estimator)
			{
			default:
				num = fraction * count;
				break;
			case QuantileEstimator.HyndmanFanType2:
			case QuantileEstimator.HyndmanFanType5:
				num = fraction * count + 0.5;
				break;
			case QuantileEstimator.HyndmanFanType3:
				num = fraction * count - 0.5;
				break;
			case QuantileEstimator.HyndmanFanType6:
				num = fraction * (count + 1.0);
				break;
			case QuantileEstimator.HyndmanFanType7:
				num = fraction * (count - 1.0) + 1.0;
				break;
			case QuantileEstimator.HyndmanFanType8:
				num = fraction * (count + 1.0 / 3.0) + 1.0 / 3.0;
				break;
			case QuantileEstimator.HyndmanFanType9:
				num = fraction * (count + 0.25) + 0.375;
				break;
			}
			double priorCount = zeroCount;
			if (num <= priorCount)
			{
				return 0.0;
			}
			int i;
			for (i = 0; i < bins.Count && !(num <= priorCount + bins[i]); i++)
			{
				priorCount += bins[i];
			}
			i = Math.Min(i, bins.Count - 1);
			double binCount = bins[i];
			double binLowerBound = BinLowerBound(i);
			double binUpperBound = BinLowerBound(i + 1);
			double result;
			switch (estimator)
			{
			default:
				result = Upsample(Math.Ceiling(num));
				break;
			case QuantileEstimator.HyndmanFanType2:
				result = 0.5 * (Upsample(Math.Ceiling(num - 0.5)) + Upsample(Math.Floor(num + 0.5)));
				break;
			case QuantileEstimator.HyndmanFanType3:
				result = Upsample(Math.Round(num, MidpointRounding.ToEven));
				break;
			case QuantileEstimator.HyndmanFanType4:
			case QuantileEstimator.HyndmanFanType5:
			case QuantileEstimator.HyndmanFanType6:
			case QuantileEstimator.HyndmanFanType7:
			case QuantileEstimator.HyndmanFanType8:
			case QuantileEstimator.HyndmanFanType9:
			{
				double num2 = num - Math.Floor(num);
				result = Upsample(Math.Floor(num)) + num2 * (Upsample(Math.Ceiling(num)) - Upsample(Math.Floor(num)));
				break;
			}
			}
			return result;
			double Upsample(double cutCount)
			{
				double num3 = (cutCount - priorCount) / (binCount + 1.0);
				return binLowerBound + num3 * (binUpperBound - binLowerBound);
			}
		}
		catch
		{
			return 0.0;
		}
	}

	public double MaxRelativeSampleError()
	{
		return MaxRelativeSampleError(resolution);
	}

	public double MaxRelativeQuantileError()
	{
		return MaxRelativeQuantileError(resolution);
	}

	private int Downsample(int desiredMinExp, int desiredMaxExp)
	{
		Assert.Check(desiredMaxExp >= desiredMinExp);
		int num = 0;
		while (desiredMinExp + bins.Capacity <= desiredMaxExp)
		{
			desiredMinExp >>= 1;
			desiredMaxExp >>= 1;
			num++;
		}
		Assert.Check(num >= 0);
		if (num > 0)
		{
			int val = minExp;
			int val2 = maxExp;
			int val3 = minExp >> num;
			int val4 = maxExp >> num;
			int num2 = Math.Min(val, -1);
			int num3 = Math.Min(val2, -1);
			int num4 = Math.Max(val, 1);
			int num5 = Math.Max(val2, 1);
			int num6 = Math.Min(val3, -1);
			int num7 = Math.Min(val4, -1);
			int num8 = Math.Max(val3, 1);
			int num9 = Math.Max(val4, 1);
			int num10 = num3 - num2;
			int num11 = num7 - num6;
			int num12 = num5 - num4;
			int num13 = num9 - num8;
			if (num10 > 0)
			{
				for (int num14 = num3; num14 >= num2; num14--)
				{
					int num15 = num14 >> num;
					int num16 = BinIndex(num3);
					int num17 = num3 - num14;
					int num18 = num7 - num15;
					int index = num16 - num17;
					int index2 = num16 - num18;
					double num19 = bins[index];
					bins[index] = 0.0;
					bins[index2] += num19;
				}
			}
			if (num12 > 0)
			{
				for (int i = num4; i <= num5; i++)
				{
					int num20 = i >> num;
					int num21 = BinIndex(num4);
					int num22 = i - num4;
					int num23 = num20 - num8;
					int index3 = num21 + num22;
					int index4 = num21 + num23;
					double num24 = bins[index3];
					bins[index3] = 0.0;
					bins[index4] += num24;
				}
			}
			for (int j = 0; j < num10 - num11; j++)
			{
				bins.PopFront();
			}
			for (int k = 0; k < num12 - num13; k++)
			{
				bins.PopBack();
			}
		}
		resolution -= num;
		minExp >>= num;
		maxExp >>= num;
		return num;
	}

	private int Exponent(double value)
	{
		return Exponent(resolution, value);
	}

	private int BinExponent(int index)
	{
		return index + minExp;
	}

	public int BinIndex(int exponent)
	{
		int num = exponent - minExp;
		Assert.Check(num >= 0);
		return num;
	}

	private double BinLowerBound(int index)
	{
		return LowerBound(resolution, BinExponent(index));
	}

	private double BinMidpoint(int index)
	{
		double num = BinLowerBound(index);
		double num2 = BinLowerBound(index + 1);
		return (num + num2) * 0.5;
	}

	private static int Resolution(double error)
	{
		double num = Math.Log(2.0);
		double num2 = 1.0 / num;
		double d = (1.0 + error) / (1.0 - error);
		double d2 = Math.Log(d);
		double a = (Math.Log(num) - Math.Log(d2)) * num2;
		return (int)Math.Ceiling(a);
	}

	private static double Contrast(double min, double max)
	{
		Assert.Check(max >= min);
		return Math.Log(max) - Math.Log(min);
	}

	private static int Capacity(double contrast, int resolution)
	{
		double num = Math.Floor(contrast / Math.Log(Base(resolution)) + 1.0);
		Assert.Check(num >= 1.0);
		return (int)num;
	}

	private static double MaxRelativeSampleError(int resolution)
	{
		double num = Base(resolution);
		return (num - 1.0) / (num + 1.0);
	}

	private static double MaxRelativeQuantileError(int resolution)
	{
		double num = Base(resolution);
		return num - 1.0;
	}

	public double Base()
	{
		return Base(resolution);
	}

	private static double Base(int resolution)
	{
		double num = Math.Log(2.0);
		return Math.Exp(num * Math.Exp(num * (double)(-resolution)));
	}

	private static double CopySign(double x, double s)
	{
		long num = BitConverter.DoubleToInt64Bits(x);
		long num2 = BitConverter.DoubleToInt64Bits(s);
		num &= 0x7FFFFFFFFFFFFFFFL;
		num2 &= long.MinValue;
		return BitConverter.Int64BitsToDouble(num | num2);
	}

	private static (double, int) FrExp(double value)
	{
		if (value == 0.0 || double.IsInfinity(value) || double.IsNaN(value))
		{
			return (value, 0);
		}
		double d = Math.Log(Math.Abs(value), 2.0);
		int num = (int)Math.Floor(d) + 1;
		double x = value * (1.0 / Math.Pow(2.0, num));
		return (CopySign(x, value), num);
	}

	private static int Exponent(int resolution, double value)
	{
		Assert.Check(value != 0.0);
		double num = Math.Log(2.0);
		double num2 = 1.0 / num;
		double value2 = 9.313225746154785E-10;
		if (double.IsSubnormal(value))
		{
			return Exponent(resolution, value2);
		}
		(double, int) tuple = FrExp(value);
		double item = tuple.Item1;
		int item2 = tuple.Item2;
		double num3 = (double)item2 * Math.Pow(2.0, resolution);
		double num4 = Math.Log(item) * num2 * Math.Pow(2.0, resolution);
		return (int)Math.Floor(num3 + num4);
	}

	private static double LowerBound(int resolution, int exp)
	{
		double num = Math.Log(2.0);
		double num2 = num * Math.Exp(num * (double)(-resolution));
		return Math.Exp(num2 * (double)exp);
	}
}
