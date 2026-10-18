using System;

namespace g3;

public class Hexagon2d
{
	public enum TopModes
	{
		Flat,
		Tip
	}

	public Vector2d Center;

	public double Radius;

	public TopModes TopMode;

	public bool IsClosed => true;

	public double InnerRadius
	{
		get
		{
			return 1.7320508075688772 * Radius / 2.0;
		}
		set
		{
			Radius = 2.0 * value / 1.7320508075688772;
		}
	}

	public double Width
	{
		get
		{
			if (TopMode != 0)
			{
				return 0.8660254037844386 * Height;
			}
			return Radius * 2.0;
		}
	}

	public double Height
	{
		get
		{
			if (TopMode != 0)
			{
				return Radius * 2.0;
			}
			return 0.8660254037844386 * Width;
		}
	}

	public double VertSpacing
	{
		get
		{
			if (TopMode != 0)
			{
				return Height * 3.0 / 4.0;
			}
			return Height;
		}
	}

	public double HorzSpacing
	{
		get
		{
			if (TopMode != 0)
			{
				return Width;
			}
			return Width * 3.0 / 4.0;
		}
	}

	public AxisAlignedBox2d Bounds => new AxisAlignedBox2d(Center, Width / 2.0, Height / 2.0);

	public Hexagon2d(Vector2d center, double radius, TopModes mode = TopModes.Flat)
	{
		Center = center;
		Radius = radius;
		TopMode = mode;
	}

	public Hexagon2d Clone()
	{
		return new Hexagon2d(Center, Radius, TopMode);
	}

	public Vector2d Corner(int i)
	{
		double num = 60.0 * (double)i;
		if (TopMode == TopModes.Tip)
		{
			num += 30.0;
		}
		double num2 = num * (Math.PI / 180.0);
		return new Vector2d(Center.x + Radius * Math.Cos(num2), Center.y + Radius * Math.Sin(num2));
	}
}
