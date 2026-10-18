using System.Collections;
using System.Collections.Generic;

namespace g3;

public struct Interval1i : IEnumerable<int>, IEnumerable
{
	public int a;

	public int b;

	public static readonly Interval1i Zero = new Interval1i(0, 0);

	public static readonly Interval1i Empty = new Interval1i(int.MaxValue, -2147483647);

	public static readonly Interval1i Infinite = new Interval1i(-2147483647, int.MaxValue);

	public int this[int key]
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

	public int LengthSquared => (a - b) * (a - b);

	public int Length => b - a;

	public int Center => (b + a) / 2;

	public Interval1i(int f)
	{
		a = (b = f);
	}

	public Interval1i(int x, int y)
	{
		a = x;
		b = y;
	}

	public Interval1i(int[] v2)
	{
		a = v2[0];
		b = v2[1];
	}

	public Interval1i(Interval1i copy)
	{
		a = copy.a;
		b = copy.b;
	}

	public static Interval1i Range(int N)
	{
		return new Interval1i(0, N - 1);
	}

	public static Interval1i RangeInclusive(int N)
	{
		return new Interval1i(0, N);
	}

	public static Interval1i Range(int start, int N)
	{
		return new Interval1i(start, start + N - 1);
	}

	public static Interval1i FromToInclusive(int a, int b)
	{
		return new Interval1i(a, b);
	}

	public void Contain(int d)
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

	public bool Contains(int d)
	{
		if (d >= a)
		{
			return d <= b;
		}
		return false;
	}

	public bool Overlaps(Interval1i o)
	{
		if (o.a <= b)
		{
			return o.b >= a;
		}
		return false;
	}

	public int SquaredDist(Interval1i o)
	{
		if (b < o.a)
		{
			return (o.a - b) * (o.a - b);
		}
		if (a > o.b)
		{
			return (a - o.b) * (a - o.b);
		}
		return 0;
	}

	public int Dist(Interval1i o)
	{
		if (b < o.a)
		{
			return o.a - b;
		}
		if (a > o.b)
		{
			return a - o.b;
		}
		return 0;
	}

	public void Set(Interval1i o)
	{
		a = o.a;
		b = o.b;
	}

	public void Set(int fA, int fB)
	{
		a = fA;
		b = fB;
	}

	public static Interval1i operator -(Interval1i v)
	{
		return new Interval1i(-v.a, -v.b);
	}

	public static Interval1i operator +(Interval1i a, int f)
	{
		return new Interval1i(a.a + f, a.b + f);
	}

	public static Interval1i operator -(Interval1i a, int f)
	{
		return new Interval1i(a.a - f, a.b - f);
	}

	public static Interval1i operator *(Interval1i a, int f)
	{
		return new Interval1i(a.a * f, a.b * f);
	}

	public IEnumerator<int> GetEnumerator()
	{
		int i = a;
		while (i <= b)
		{
			yield return i;
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override string ToString()
	{
		return $"[{a},{b}]";
	}
}
