using System;

namespace g3;

public class Snapping
{
	public static double SnapToIncrement(double fValue, double fIncrement, double offset = 0.0)
	{
		if (!MathUtil.IsFinite(fValue))
		{
			return 0.0;
		}
		fValue -= offset;
		double num = Math.Sign(fValue);
		fValue = Math.Abs(fValue);
		int num2 = (int)(fValue / fIncrement);
		if (fValue % fIncrement > fIncrement / 2.0)
		{
			num2++;
		}
		return num * (double)num2 * fIncrement + offset;
	}

	public static double SnapToNearbyIncrement(double fValue, double fIncrement, double fTolerance)
	{
		double num = SnapToIncrement(fValue, fIncrement);
		if (Math.Abs(num - fValue) < fTolerance)
		{
			return num;
		}
		return fValue;
	}

	private static double SnapToIncrementSigned(double fValue, double fIncrement, bool low)
	{
		if (!MathUtil.IsFinite(fValue))
		{
			return 0.0;
		}
		double num = Math.Sign(fValue);
		fValue = Math.Abs(fValue);
		int num2 = (int)(fValue / fIncrement);
		if (low && num < 0.0)
		{
			num2++;
		}
		else if (!low && num > 0.0)
		{
			num2++;
		}
		return num * (double)num2 * fIncrement;
	}

	public static double SnapToIncrementLow(double fValue, double fIncrement, double offset = 0.0)
	{
		return SnapToIncrementSigned(fValue - offset, fIncrement, low: true) + offset;
	}

	public static double SnapToIncrementHigh(double fValue, double fIncrement, double offset = 0.0)
	{
		return SnapToIncrementSigned(fValue - offset, fIncrement, low: false) + offset;
	}
}
