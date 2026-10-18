using System;

namespace g3;

public abstract class BaseCurve2
{
	protected double mTMin;

	protected double mTMax;

	public BaseCurve2(double tmin, double tmax)
	{
		mTMin = tmin;
		mTMax = tmax;
	}

	public double GetMinTime()
	{
		return mTMax;
	}

	public double GetMaxTime()
	{
		return mTMax;
	}

	public void SetTimeInterval(double tmin, double tmax)
	{
		if (tmin >= tmax)
		{
			throw new Exception("Curve2.SetTimeInterval: invalid min/max");
		}
		mTMin = tmin;
		mTMax = tmax;
	}

	public abstract Vector2d GetPosition(double t);

	public abstract Vector2d GetFirstDerivative(double t);

	public abstract Vector2d GetSecondDerivative(double t);

	public abstract Vector2d GetThirdDerivative(double t);

	public double GetSpeed(double t)
	{
		return GetFirstDerivative(t).Length;
	}

	private double GetSpeedWithData(double t, object data)
	{
		return (data as BaseCurve2).GetSpeed(t);
	}

	public virtual double GetLength(double t0, double t1)
	{
		if (t0 < mTMin || t0 > mTMax)
		{
			throw new Exception("BaseCurve2.GetLength: min t out of bounds: " + t0);
		}
		if (t1 < mTMin || t1 > mTMax)
		{
			throw new Exception("BaseCurve2.GetLength: max t out of bounds: " + t1);
		}
		if (t0 > t1)
		{
			throw new Exception("BaseCurve2.GetLength: inverted t-range\n " + t0 + " " + t1);
		}
		return Integrate1d.RombergIntegral(8, t0, t1, GetSpeedWithData, this);
	}

	public double GetTotalLength()
	{
		return GetLength(mTMin, mTMax);
	}

	public Vector2d GetTangent(double t)
	{
		return GetFirstDerivative(t).Normalized;
	}

	public Vector2d GetNormal(double t)
	{
		return GetFirstDerivative(t).Normalized.Perp;
	}

	public void GetFrame(double t, ref Vector2d position, ref Vector2d tangent, ref Vector2d normal)
	{
		position = GetPosition(t);
		tangent = GetFirstDerivative(t).Normalized;
		normal = tangent.Perp;
	}

	public double GetCurvature(double t)
	{
		Vector2d firstDerivative = GetFirstDerivative(t);
		Vector2d secondDerivative = GetSecondDerivative(t);
		double lengthSquared = firstDerivative.LengthSquared;
		if (lengthSquared >= 1E-08)
		{
			double num = firstDerivative.DotPerp(secondDerivative);
			double num2 = Math.Pow(lengthSquared, 1.5);
			return num / num2;
		}
		return 0.0;
	}

	public virtual double GetTime(double length, int iterations = 32, double tolerance = 1E-06)
	{
		if (length <= 0.0)
		{
			return mTMin;
		}
		if (length >= GetTotalLength())
		{
			return mTMax;
		}
		double num = length / GetTotalLength();
		double num2 = (1.0 - num) * mTMin + num * mTMax;
		double num3 = mTMin;
		double num4 = mTMax;
		for (int i = 0; i < iterations; i++)
		{
			double num5 = GetLength(mTMin, num2) - length;
			if (Math.Abs(num5) < tolerance)
			{
				return num2;
			}
			double num6 = num2 - num5 / GetSpeed(num2);
			if (num5 > 0.0)
			{
				num4 = num2;
				num2 = ((!(num6 <= num3)) ? num6 : (0.5 * (num4 + num3)));
			}
			else
			{
				num3 = num2;
				num2 = ((!(num6 >= num4)) ? num6 : (0.5 * (num4 + num3)));
			}
		}
		return num2;
	}

	private Vector2d[] SubdivideByTime(int numPoints)
	{
		if (numPoints < 2)
		{
			throw new Exception("BaseCurve2.SubdivideByTime: Subdivision requires at least two points, requested " + numPoints);
		}
		Vector2d[] array = new Vector2d[numPoints];
		double num = (mTMax - mTMin) / (double)(numPoints - 1);
		for (int i = 0; i < numPoints; i++)
		{
			double t = mTMin + num * (double)i;
			array[i] = GetPosition(t);
		}
		return array;
	}

	private Vector2d[] SubdivieByLength(int numPoints)
	{
		if (numPoints < 2)
		{
			throw new Exception("BaseCurve2.SubdivideByTime: Subdivision requires at least two points, requested " + numPoints);
		}
		Vector2d[] array = new Vector2d[numPoints];
		double num = GetTotalLength() / (double)(numPoints - 1);
		for (int i = 0; i < numPoints; i++)
		{
			double length = num * (double)i;
			double time = GetTime(length);
			array[i] = GetPosition(time);
		}
		return array;
	}
}
