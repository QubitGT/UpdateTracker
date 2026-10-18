using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Unity.XR.CoreUtils;

public static class MathUtility
{
	internal static readonly float EpsilonScaled = Mathf.Epsilon * 8f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Approximately(float a, float b)
	{
		float num = b - a;
		return ((num >= 0f) ? num : (0f - num)) < EpsilonScaled;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ApproximatelyZero(float a)
	{
		return ((a >= 0f) ? a : (0f - a)) < EpsilonScaled;
	}

	public static double Clamp(double input, double min, double max)
	{
		if (input > max)
		{
			return max;
		}
		if (!(input < min))
		{
			return input;
		}
		return min;
	}

	public static double ShortestAngleDistance(double start, double end, double halfMax, double max)
	{
		double value = end - start;
		int num = Math.Sign(value);
		value = Math.Abs(value) % max;
		if (value > halfMax)
		{
			value = 0.0 - (max - value);
		}
		return value * (double)num;
	}

	public static float ShortestAngleDistance(float start, float end, float halfMax, float max)
	{
		float num = end - start;
		float num2 = Mathf.Sign(num);
		num = Math.Abs(num) % max;
		if (num > halfMax)
		{
			num = 0f - (max - num);
		}
		return num * num2;
	}

	public static bool IsUndefined(this float value)
	{
		if (!float.IsInfinity(value))
		{
			return float.IsNaN(value);
		}
		return true;
	}

	public static bool IsAxisAligned(this Vector3 v)
	{
		if (ApproximatelyZero(v.x * v.y) && ApproximatelyZero(v.y * v.z))
		{
			return ApproximatelyZero(v.z * v.x);
		}
		return false;
	}

	public static bool IsPositivePowerOfTwo(int value)
	{
		if (value > 0)
		{
			return (value & (value - 1)) == 0;
		}
		return false;
	}

	public static int FirstActiveFlagIndex(int value)
	{
		if (value == 0)
		{
			return 0;
		}
		for (int i = 0; i < 32; i++)
		{
			if ((value & (1 << i)) != 0)
			{
				return i;
			}
		}
		return 0;
	}
}
