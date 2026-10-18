#define DEBUG
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Fusion;

[StructLayout(LayoutKind.Explicit)]
[NetworkStructWeaved(1)]
public struct Angle : INetworkStruct, IEquatable<Angle>
{
	public const int SIZE = 4;

	private const int ACCURACY = 10000;

	private const int DECIMALS = 4;

	private const int _360 = 3600000;

	[FieldOffset(0)]
	private int _value;

	public void Clamp(Angle min, Angle max)
	{
		Assert.Check(max._value >= min._value);
		if (_value < min._value)
		{
			_value = min._value;
		}
		if (_value > max._value)
		{
			_value = max._value;
		}
	}

	public static Angle Min(Angle a, Angle b)
	{
		return (a._value < b._value) ? a : b;
	}

	public static Angle Max(Angle a, Angle b)
	{
		return (a._value > b._value) ? a : b;
	}

	public static Angle Lerp(Angle a, Angle b, float t)
	{
		if (a._value == b._value)
		{
			return a;
		}
		return Mathf.LerpAngle((float)a, (float)b, t);
	}

	public static Angle Clamp(Angle value, Angle min, Angle max)
	{
		if (max._value < min._value)
		{
			Angle angle = max;
			max = min;
			min = angle;
		}
		if (value._value < min._value)
		{
			return min;
		}
		if (value._value > max._value)
		{
			return max;
		}
		return value;
	}

	public static bool operator <(Angle a, Angle b)
	{
		return a._value < b._value;
	}

	public static bool operator <=(Angle a, Angle b)
	{
		return a._value <= b._value;
	}

	public static bool operator >(Angle a, Angle b)
	{
		return a._value > b._value;
	}

	public static bool operator >=(Angle a, Angle b)
	{
		return a._value >= b._value;
	}

	public static bool operator ==(Angle a, Angle b)
	{
		return a._value == b._value;
	}

	public static bool operator !=(Angle a, Angle b)
	{
		return a._value != b._value;
	}

	public bool Equals(Angle other)
	{
		return _value == other._value;
	}

	public override bool Equals(object obj)
	{
		return obj is Angle other && Equals(other);
	}

	public override int GetHashCode()
	{
		return _value;
	}

	public static Angle operator +(Angle a, Angle b)
	{
		Assert.Check(a._value >= 0 && a._value <= 3600000);
		Assert.Check(b._value >= 0 && b._value <= 3600000);
		a._value += b._value;
		if (a._value > 3600000)
		{
			a._value %= 3600000;
		}
		return a;
	}

	public static Angle operator -(Angle a, Angle b)
	{
		Assert.Check(a._value >= 0 && a._value <= 3600000);
		Assert.Check(b._value >= 0 && b._value <= 3600000);
		a._value -= b._value;
		if (a._value < 0)
		{
			Assert.Check(a._value >= -3600000);
			a._value = 3600000 + a._value;
		}
		return a;
	}

	public static explicit operator float(Angle value)
	{
		return (float)((double)value._value / 10000.0);
	}

	public static explicit operator double(Angle value)
	{
		return (double)value._value / 10000.0;
	}

	public static implicit operator Angle(double value)
	{
		if (value > 360.0)
		{
			value %= 360.0;
		}
		else if (value < 0.0)
		{
			value = ((!(value < -360.0)) ? (360.0 + value) : (360.0 + value % -360.0));
		}
		Angle result = default(Angle);
		result._value = (int)(value * 10000.0 + 0.5);
		return result;
	}

	public static implicit operator Angle(float value)
	{
		return (double)value;
	}

	public static implicit operator Angle(int value)
	{
		if (value > 360)
		{
			value %= 360;
		}
		else if (value < 0)
		{
			value = ((value >= -360) ? (360 + value) : (360 + value % -360));
		}
		Angle result = default(Angle);
		result._value = value * 10000;
		return result;
	}

	public override string ToString()
	{
		string text = (_value % 10000).ToString();
		if (text.Length < 4)
		{
			text = new string('0', 4 - text.Length) + text;
		}
		return $"[Angle:{_value / 10000}.{text}]";
	}
}
