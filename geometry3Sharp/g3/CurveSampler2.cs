using System;
using System.Collections.Generic;

namespace g3;

public static class CurveSampler2
{
	public static VectorArray2d AutoSample(IParametricCurve2d curve, double fSpacingLength, double fSpacingT)
	{
		if (curve is ParametricCurveSequence2)
		{
			return AutoSample(curve as ParametricCurveSequence2, fSpacingLength, fSpacingT);
		}
		if (curve.HasArcLength)
		{
			if (curve is NURBSCurve2)
			{
				return SampleNURBSHybrid(curve as NURBSCurve2, fSpacingLength);
			}
			return SampleArcLen(curve, fSpacingLength);
		}
		return SampleT(curve, fSpacingT);
	}

	public static VectorArray2d SampleT(IParametricCurve2d curve, int N)
	{
		double paramLength = curve.ParamLength;
		VectorArray2d vectorArray2d = new VectorArray2d(N);
		double num = (curve.IsClosed ? ((double)N) : ((double)(N - 1)));
		for (int i = 0; i < N; i++)
		{
			double num2 = (double)i / num;
			vectorArray2d[i] = curve.SampleT(num2 * paramLength);
		}
		return vectorArray2d;
	}

	public static VectorArray2d SampleTRange(IParametricCurve2d curve, int N, double t0, double t1)
	{
		VectorArray2d vectorArray2d = new VectorArray2d(N);
		for (int i = 0; i < N; i++)
		{
			double num = (double)i / (double)(N - 1);
			double t2 = (1.0 - num) * t0 + num * t1;
			vectorArray2d[i] = curve.SampleT(t2);
		}
		return vectorArray2d;
	}

	public static VectorArray2d SampleT(IParametricCurve2d curve, double fSpacing)
	{
		double paramLength = curve.ParamLength;
		int num = Math.Max((int)(paramLength / fSpacing) + 1, 2);
		VectorArray2d vectorArray2d = new VectorArray2d(num);
		for (int i = 0; i < num; i++)
		{
			double num2 = (double)i / (double)(num - 1);
			vectorArray2d[i] = curve.SampleT(num2 * paramLength);
		}
		return vectorArray2d;
	}

	public static VectorArray2d SampleArcLen(IParametricCurve2d curve, double fSpacing)
	{
		if (!curve.HasArcLength)
		{
			throw new InvalidOperationException("CurveSampler2.SampleArcLen: curve does not support arc length sampling!");
		}
		double arcLength = curve.ArcLength;
		if (arcLength < 1E-08)
		{
			return new VectorArray2d(2)
			{
				[0] = curve.SampleArcLength(0.0),
				[1] = curve.SampleArcLength(1.0)
			};
		}
		int num = Math.Max((int)(arcLength / fSpacing) + 1, 2);
		VectorArray2d vectorArray2d = new VectorArray2d(num);
		for (int i = 0; i < num; i++)
		{
			double num2 = (double)i / (double)(num - 1);
			vectorArray2d[i] = curve.SampleArcLength(num2 * arcLength);
		}
		return vectorArray2d;
	}

	public static VectorArray2d SampleNURBSHybrid(NURBSCurve2 curve, double fSpacing)
	{
		List<double> paramIntervals = curve.GetParamIntervals();
		int num = paramIntervals.Count - 1;
		VectorArray2d[] array = new VectorArray2d[num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			double num3 = paramIntervals[i];
			double num4 = paramIntervals[i + 1];
			int num5 = Math.Max((int)(curve.GetLength(num3, num4) / fSpacing) + 1, 2);
			double num6 = 1.0 / (double)num5;
			if (!curve.IsClosed && i == num - 1)
			{
				num5++;
				num6 = 1.0 / (double)(num5 - 1);
			}
			VectorArray2d vectorArray2d = new VectorArray2d(num5);
			for (int j = 0; j < num5; j++)
			{
				double num7 = (double)j * num6;
				double t = (1.0 - num7) * num3 + num7 * num4;
				vectorArray2d[j] = curve.SampleT(t);
			}
			array[i] = vectorArray2d;
			num2 += num5;
		}
		VectorArray2d vectorArray2d2 = new VectorArray2d(num2);
		int num8 = 0;
		for (int k = 0; k < num; k++)
		{
			vectorArray2d2.Set(num8, array[k].Count, array[k]);
			num8 += array[k].Count;
		}
		return vectorArray2d2;
	}

	public static VectorArray2d AutoSample(ParametricCurveSequence2 curves, double fSpacingLength, double fSpacingT)
	{
		int count = curves.Count;
		bool isClosed = curves.IsClosed;
		VectorArray2d[] array = new VectorArray2d[count];
		int num = 0;
		int num2 = 0;
		foreach (IParametricCurve2d curf in curves.Curves)
		{
			array[num] = AutoSample(curf, fSpacingLength, fSpacingT);
			num2 += array[num].Count;
			num++;
		}
		int num3 = (isClosed ? count : (count - 1));
		num2 -= num3;
		VectorArray2d vectorArray2d = new VectorArray2d(num2);
		int num4 = 0;
		for (int i = 0; i < count; i++)
		{
			VectorArray2d vectorArray2d2 = array[i];
			int num5 = ((isClosed || i < count - 1) ? (vectorArray2d2.Count - 1) : vectorArray2d2.Count);
			vectorArray2d.Set(num4, num5, vectorArray2d2);
			num4 += num5;
		}
		return vectorArray2d;
	}
}
