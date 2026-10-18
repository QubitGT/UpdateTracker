using System;

namespace g3;

public struct Interval1d
{
	public double a;

	public double b;

	public static readonly Interval1d Zero = new Interval1d(0f, 0f);

	public static readonly Interval1d Empty = new Interval1d(double.MaxValue, double.MinValue);

	public static readonly Interval1d Infinite = new Interval1d(double.MinValue, double.MaxValue);

	public double this[int key]
	{
		get
		{
			if (key != 0)
			{
				return b;
			}
			return a;
		}
		set
		{
			if (key == 0)
			{
				a = value;
			}
			else
			{
				b = value;
			}
		}
	}

	public double LengthSquared => (a - b) * (a - b);

	public double Length => b - a;

	public bool IsConstant => b == a;

	public double Center => (b + a) * 0.5;

	public Interval1d(double f)
	{
		a = (b = f);
	}

	public Interval1d(double x, double y)
	{
		a = x;
		b = y;
	}

	public Interval1d(double[] v2)
	{
		a = v2[0];
		b = v2[1];
	}

	public Interval1d(float f)
	{
		a = (b = f);
	}

	public Interval1d(float x, float y)
	{
		a = x;
		b = y;
	}

	public Interval1d(float[] v2)
	{
		a = v2[0];
		b = v2[1];
	}

	public Interval1d(Interval1d copy)
	{
		a = copy.a;
		b = copy.b;
	}

	public static Interval1d Unsorted(double x, double y)
	{
		if (!(x < y))
		{
			return new Interval1d(y, x);
		}
		return new Interval1d(x, y);
	}

	public void Contain(double d)
	{
		if (d < a)
		{
			a = d;
		}
		if (d > b)
		{
			b = d;
		}
	}

	public bool Contains(double d)
	{
		if (d >= a)
		{
			return d <= b;
		}
		return false;
	}

	public bool Overlaps(Interval1d o)
	{
		if (!(o.a > b))
		{
			return !(o.b < a);
		}
		return false;
	}

	public double SquaredDist(Interval1d o)
	{
		if (b < o.a)
		{
			return (o.a - b) * (o.a - b);
		}
		if (a > o.b)
		{
			return (a - o.b) * (a - o.b);
		}
		return 0.0;
	}

	public double Dist(Interval1d o)
	{
		if (b < o.a)
		{
			return o.a - b;
		}
		if (a > o.b)
		{
			return a - o.b;
		}
		return 0.0;
	}

	public Interval1d IntersectionWith(ref Interval1d o)
	{
		if (o.a > b || o.b < a)
		{
			return Empty;
		}
		return new Interval1d(Math.Max(a, o.a), Math.Min(b, o.b));
	}

	public double Clamp(double f)
	{
		if (!(f < a))
		{
			if (!(f > b))
			{
				return f;
			}
			return b;
		}
		return a;
	}

	public double Interpolate(double t)
	{
		return (1.0 - t) * a + t * b;
	}

	public double GetT(double value)
	{
		if (value <= a)
		{
			return 0.0;
		}
		if (value >= b)
		{
			return 1.0;
		}
		if (a == b)
		{
			return 0.5;
		}
		return (value - a) / (b - a);
	}

	public void Set(Interval1d o)
	{
		a = o.a;
		b = o.b;
	}

	public void Set(double fA, double fB)
	{
		a = fA;
		b = fB;
	}

	public static Interval1d operator -(Interval1d v)
	{
		return new Interval1d(0.0 - v.a, 0.0 - v.b);
	}

	public static Interval1d operator +(Interval1d a, double f)
	{
		return new Interval1d(a.a + f, a.b + f);
	}

	public static Interval1d operator -(Interval1d a, double f)
	{
		return new Interval1d(a.a - f, a.b - f);
	}

	public static Interval1d operator *(Interval1d a, double f)
	{
		return new Interval1d(a.a * f, a.b * f);
	}

	public override string ToString()
	{
		return $"[{a:F8},{b:F8}]";
	}
}
