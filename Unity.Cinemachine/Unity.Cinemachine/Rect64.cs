namespace Unity.Cinemachine;

internal struct Rect64
{
	public long left;

	public long top;

	public long right;

	public long bottom;

	public long Width
	{
		get
		{
			return right - left;
		}
		set
		{
			right = left + value;
		}
	}

	public long Height
	{
		get
		{
			return bottom - top;
		}
		set
		{
			bottom = top + value;
		}
	}

	public Rect64(long l, long t, long r, long b)
	{
		left = l;
		top = t;
		right = r;
		bottom = b;
	}

	public Rect64(Rect64 rec)
	{
		left = rec.left;
		top = rec.top;
		right = rec.right;
		bottom = rec.bottom;
	}

	public bool IsEmpty()
	{
		if (bottom > top)
		{
			return right <= left;
		}
		return true;
	}

	public Point64 MidPoint()
	{
		return new Point64((left + right) / 2, (top + bottom) / 2);
	}

	public bool Contains(Point64 pt)
	{
		if (pt.X > left && pt.X < right && pt.Y > top)
		{
			return pt.Y < bottom;
		}
		return false;
	}

	public bool Contains(Rect64 rec)
	{
		if (rec.left >= left && rec.right <= right && rec.top >= top)
		{
			return rec.bottom <= bottom;
		}
		return false;
	}
}
