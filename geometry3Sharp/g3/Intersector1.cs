namespace g3;

public class Intersector1
{
	public Interval1d U;

	public Interval1d V;

	public int NumIntersections;

	private Interval1d Intersections = Interval1d.Zero;

	public bool Test
	{
		get
		{
			if (U.a <= V.b)
			{
				return U.b >= V.a;
			}
			return false;
		}
	}

	public Intersector1(double u0, double u1, double v0, double v1)
	{
		U = new Interval1d(u0, u1);
		V = new Interval1d(v0, v1);
	}

	public Intersector1(Interval1d u, Interval1d v)
	{
		U = u;
		V = v;
	}

	public double GetIntersection(int i)
	{
		return Intersections[i];
	}

	public bool Find()
	{
		if (U.b < V.a || U.a > V.b)
		{
			NumIntersections = 0;
		}
		else if (U.b > V.a)
		{
			if (U.a < V.b)
			{
				NumIntersections = 2;
				Intersections.a = ((U.a < V.a) ? V.a : U.a);
				Intersections.b = ((U.b > V.b) ? V.b : U.b);
				if (Intersections.a == Intersections.b)
				{
					NumIntersections = 1;
				}
			}
			else
			{
				NumIntersections = 1;
				Intersections.a = U.a;
			}
		}
		else
		{
			NumIntersections = 1;
			Intersections.a = U.b;
		}
		return NumIntersections > 0;
	}
}
