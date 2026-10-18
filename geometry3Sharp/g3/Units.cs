using System;

namespace g3;

public static class Units
{
	public enum Angular
	{
		Degrees,
		Radians
	}

	public enum Linear
	{
		UnknownUnits = 0,
		Nanometers = 11,
		Microns = 14,
		Millimeters = 17,
		Centimeters = 18,
		Meters = 20,
		Kilometers = 23,
		Inches = 105,
		Feet = 109,
		Yards = 110,
		Miles = 115
	}

	public static bool IsMetric(Linear t)
	{
		if (t > Linear.UnknownUnits)
		{
			return t < (Linear)50;
		}
		return false;
	}

	public static double GetMetricPower(Linear t)
	{
		if (t > Linear.UnknownUnits && t < (Linear)50)
		{
			return (double)t - 20.0;
		}
		throw new Exception("Units.GetMetricPower: input unit is not metric!");
	}

	public static double ToMeters(Linear t)
	{
		if (t > Linear.UnknownUnits && t < (Linear)50)
		{
			double metricPower = GetMetricPower(t);
			return Math.Pow(10.0, metricPower);
		}
		return t switch
		{
			Linear.Inches => 0.0254, 
			Linear.Feet => 0.3048, 
			Linear.Yards => 0.9144, 
			Linear.Miles => 1609.34, 
			_ => throw new Exception("Units.ToMeters: input unit is not handled!"), 
		};
	}

	public static double MetersTo(Linear t)
	{
		if (t > Linear.UnknownUnits && t < (Linear)50)
		{
			double metricPower = GetMetricPower(t);
			return Math.Pow(10.0, 0.0 - metricPower);
		}
		return t switch
		{
			Linear.Inches => 39.37007874015748, 
			Linear.Feet => 3.280839895013123, 
			Linear.Yards => 1.0936132983377078, 
			Linear.Miles => 0.0006213727366498068, 
			_ => throw new Exception("Units.ToMeters: input unit is not handled!"), 
		};
	}

	public static double Convert(Linear from, Linear to)
	{
		if (from == to)
		{
			return 1.0;
		}
		if (IsMetric(from) && IsMetric(to))
		{
			double metricPower = GetMetricPower(from);
			double metricPower2 = GetMetricPower(to);
			double y = metricPower - metricPower2;
			return Math.Pow(10.0, y);
		}
		return ToMeters(from) * MetersTo(to);
	}

	public static Linear ParseLinear(string units)
	{
		return units switch
		{
			"nm" => Linear.Nanometers, 
			"um" => Linear.Microns, 
			"mm" => Linear.Millimeters, 
			"cm" => Linear.Centimeters, 
			"m" => Linear.Meters, 
			"km" => Linear.Kilometers, 
			"in" => Linear.Inches, 
			"ft" => Linear.Feet, 
			"yd" => Linear.Yards, 
			"mi" => Linear.Miles, 
			_ => Linear.UnknownUnits, 
		};
	}

	public static string GetShortString(Linear unit)
	{
		return unit switch
		{
			Linear.UnknownUnits => "??", 
			Linear.Nanometers => "nm", 
			Linear.Microns => "um", 
			Linear.Millimeters => "mm", 
			Linear.Centimeters => "cm", 
			Linear.Meters => "m", 
			Linear.Kilometers => "km", 
			Linear.Inches => "in", 
			Linear.Feet => "ft", 
			Linear.Yards => "yd", 
			Linear.Miles => "mi", 
			_ => throw new Exception("Units.GetShortString: unhandled unit type!"), 
		};
	}
}
