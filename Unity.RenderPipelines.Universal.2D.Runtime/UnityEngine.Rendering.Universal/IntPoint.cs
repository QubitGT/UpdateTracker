namespace UnityEngine.Rendering.Universal;

internal struct IntPoint
{
	public long N;

	public long X;

	public long Y;

	public long D;

	public double NX;

	public double NY;

	public IntPoint(long X, long Y)
	{
		this.X = X;
		this.Y = Y;
		NX = 0.0;
		NY = 0.0;
		N = -1L;
		D = 0L;
	}

	public IntPoint(double x, double y)
	{
		X = (long)x;
		Y = (long)y;
		NX = 0.0;
		NY = 0.0;
		N = -1L;
		D = 0L;
	}

	public IntPoint(IntPoint pt)
	{
		X = pt.X;
		Y = pt.Y;
		NX = pt.NX;
		NY = pt.NY;
		N = pt.N;
		D = pt.D;
	}

	public static bool operator ==(IntPoint a, IntPoint b)
	{
		if (a.X == b.X)
		{
			return a.Y == b.Y;
		}
		return false;
	}

	public static bool operator !=(IntPoint a, IntPoint b)
	{
		if (a.X == b.X)
		{
			return a.Y != b.Y;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is IntPoint intPoint)
		{
			if (X == intPoint.X)
			{
				return Y == intPoint.Y;
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
