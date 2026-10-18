namespace Unity.Cinemachine;

internal struct RectD
{
	public double left;

	public double top;

	public double right;

	public double bottom;

	public double Width
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

	public double Height
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

	public RectD(double l, double t, double r, double b)
	{
		left = l;
		top = t;
		right = r;
		bottom = b;
	}

	public RectD(RectD rec)
	{
		left = rec.left;
		top = rec.top;
		right = rec.right;
		bottom = rec.bottom;
	}

	public bool IsEmpty()
	{
		if (!(bottom <= top))
		{
			return right <= left;
		}
		return true;
	}

	public PointD MidPoint()
	{
		return new PointD((left + right) / 2.0, (top + bottom) / 2.0);
	}

	public bool PtIsInside(PointD pt)
	{
		if (pt.x > left && pt.x < right && pt.y > top)
		{
			return pt.y < bottom;
		}
		return false;
	}
}
