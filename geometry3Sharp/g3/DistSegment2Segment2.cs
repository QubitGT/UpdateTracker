using System;

namespace g3;

public class DistSegment2Segment2
{
	private Segment2d segment0;

	private Segment2d segment1;

	public double DistanceSquared = -1.0;

	public Vector2d Segment1Closest;

	public Vector2d Segment2Closest;

	public double Segment1Parameter;

	public double Segment2Parameter;

	public Segment2d Segment1
	{
		get
		{
			return segment0;
		}
		set
		{
			segment0 = value;
			DistanceSquared = -1.0;
		}
	}

	public Segment2d Segment2
	{
		get
		{
			return segment1;
		}
		set
		{
			segment1 = value;
			DistanceSquared = -1.0;
		}
	}

	public DistSegment2Segment2(Segment2d Segment1, Segment2d Segment2)
	{
		segment1 = Segment2;
		segment0 = Segment1;
	}

	public static double MinDistance(Segment2d Segment1, Segment2d Segment2)
	{
		return new DistSegment2Segment2(Segment1, Segment2).Get();
	}

	public DistSegment2Segment2 Compute()
	{
		GetSquared();
		return this;
	}

	public double Get()
	{
		return Math.Sqrt(GetSquared());
	}

	public double GetSquared()
	{
		if (DistanceSquared >= 0.0)
		{
			return DistanceSquared;
		}
		Vector2d vector2d = segment0.Center - segment1.Center;
		double num = 0.0 - segment0.Direction.Dot(segment1.Direction);
		double num2 = vector2d.Dot(segment0.Direction);
		double num3 = 0.0 - vector2d.Dot(segment1.Direction);
		double lengthSquared = vector2d.LengthSquared;
		double num4 = Math.Abs(1.0 - num * num);
		double num5;
		double num6;
		double num10;
		if (num4 >= 1E-08)
		{
			num5 = num * num3 - num2;
			num6 = num * num2 - num3;
			double num7 = segment0.Extent * num4;
			double num8 = segment1.Extent * num4;
			if (num5 >= 0.0 - num7)
			{
				if (num5 <= num7)
				{
					if (num6 >= 0.0 - num8)
					{
						if (num6 <= num8)
						{
							double num9 = 1.0 / num4;
							num5 *= num9;
							num6 *= num9;
							num10 = 0.0;
						}
						else
						{
							num6 = segment1.Extent;
							double num11 = 0.0 - (num * num6 + num2);
							if (num11 < 0.0 - segment0.Extent)
							{
								num5 = 0.0 - segment0.Extent;
								num10 = num5 * (num5 - 2.0 * num11) + num6 * (num6 + 2.0 * num3) + lengthSquared;
							}
							else if (num11 <= segment0.Extent)
							{
								num5 = num11;
								num10 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
							}
							else
							{
								num5 = segment0.Extent;
								num10 = num5 * (num5 - 2.0 * num11) + num6 * (num6 + 2.0 * num3) + lengthSquared;
							}
						}
					}
					else
					{
						num6 = 0.0 - segment1.Extent;
						double num11 = 0.0 - (num * num6 + num2);
						if (num11 < 0.0 - segment0.Extent)
						{
							num5 = 0.0 - segment0.Extent;
							num10 = num5 * (num5 - 2.0 * num11) + num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
						else if (num11 <= segment0.Extent)
						{
							num5 = num11;
							num10 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
						else
						{
							num5 = segment0.Extent;
							num10 = num5 * (num5 - 2.0 * num11) + num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
					}
				}
				else if (num6 >= 0.0 - num8)
				{
					if (num6 <= num8)
					{
						num5 = segment0.Extent;
						double num12 = 0.0 - (num * num5 + num3);
						if (num12 < 0.0 - segment1.Extent)
						{
							num6 = 0.0 - segment1.Extent;
							num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
						else if (num12 <= segment1.Extent)
						{
							num6 = num12;
							num10 = (0.0 - num6) * num6 + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
						else
						{
							num6 = segment1.Extent;
							num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
					}
					else
					{
						num6 = segment1.Extent;
						double num11 = 0.0 - (num * num6 + num2);
						if (num11 < 0.0 - segment0.Extent)
						{
							num5 = 0.0 - segment0.Extent;
							num10 = num5 * (num5 - 2.0 * num11) + num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
						else if (num11 <= segment0.Extent)
						{
							num5 = num11;
							num10 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
						}
						else
						{
							num5 = segment0.Extent;
							double num12 = 0.0 - (num * num5 + num3);
							if (num12 < 0.0 - segment1.Extent)
							{
								num6 = 0.0 - segment1.Extent;
								num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
							}
							else if (num12 <= segment1.Extent)
							{
								num6 = num12;
								num10 = (0.0 - num6) * num6 + num5 * (num5 + 2.0 * num2) + lengthSquared;
							}
							else
							{
								num6 = segment1.Extent;
								num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
							}
						}
					}
				}
				else
				{
					num6 = 0.0 - segment1.Extent;
					double num11 = 0.0 - (num * num6 + num2);
					if (num11 < 0.0 - segment0.Extent)
					{
						num5 = 0.0 - segment0.Extent;
						num10 = num5 * (num5 - 2.0 * num11) + num6 * (num6 + 2.0 * num3) + lengthSquared;
					}
					else if (num11 <= segment0.Extent)
					{
						num5 = num11;
						num10 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
					}
					else
					{
						num5 = segment0.Extent;
						double num12 = 0.0 - (num * num5 + num3);
						if (num12 > segment1.Extent)
						{
							num6 = segment1.Extent;
							num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
						else if (num12 >= 0.0 - segment1.Extent)
						{
							num6 = num12;
							num10 = (0.0 - num6) * num6 + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
						else
						{
							num6 = 0.0 - segment1.Extent;
							num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
					}
				}
			}
			else if (num6 >= 0.0 - num8)
			{
				if (num6 <= num8)
				{
					num5 = 0.0 - segment0.Extent;
					double num12 = 0.0 - (num * num5 + num3);
					if (num12 < 0.0 - segment1.Extent)
					{
						num6 = 0.0 - segment1.Extent;
						num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
					}
					else if (num12 <= segment1.Extent)
					{
						num6 = num12;
						num10 = (0.0 - num6) * num6 + num5 * (num5 + 2.0 * num2) + lengthSquared;
					}
					else
					{
						num6 = segment1.Extent;
						num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
					}
				}
				else
				{
					num6 = segment1.Extent;
					double num11 = 0.0 - (num * num6 + num2);
					if (num11 > segment0.Extent)
					{
						num5 = segment0.Extent;
						num10 = num5 * (num5 - 2.0 * num11) + num6 * (num6 + 2.0 * num3) + lengthSquared;
					}
					else if (num11 >= 0.0 - segment0.Extent)
					{
						num5 = num11;
						num10 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
					}
					else
					{
						num5 = 0.0 - segment0.Extent;
						double num12 = 0.0 - (num * num5 + num3);
						if (num12 < 0.0 - segment1.Extent)
						{
							num6 = 0.0 - segment1.Extent;
							num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
						else if (num12 <= segment1.Extent)
						{
							num6 = num12;
							num10 = (0.0 - num6) * num6 + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
						else
						{
							num6 = segment1.Extent;
							num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
						}
					}
				}
			}
			else
			{
				num6 = 0.0 - segment1.Extent;
				double num11 = 0.0 - (num * num6 + num2);
				if (num11 > segment0.Extent)
				{
					num5 = segment0.Extent;
					num10 = num5 * (num5 - 2.0 * num11) + num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
				else if (num11 >= 0.0 - segment0.Extent)
				{
					num5 = num11;
					num10 = (0.0 - num5) * num5 + num6 * (num6 + 2.0 * num3) + lengthSquared;
				}
				else
				{
					num5 = 0.0 - segment0.Extent;
					double num12 = 0.0 - (num * num5 + num3);
					if (num12 < 0.0 - segment1.Extent)
					{
						num6 = 0.0 - segment1.Extent;
						num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
					}
					else if (num12 <= segment1.Extent)
					{
						num6 = num12;
						num10 = (0.0 - num6) * num6 + num5 * (num5 + 2.0 * num2) + lengthSquared;
					}
					else
					{
						num6 = segment1.Extent;
						num10 = num6 * (num6 - 2.0 * num12) + num5 * (num5 + 2.0 * num2) + lengthSquared;
					}
				}
			}
		}
		else
		{
			double num13 = segment0.Extent + segment1.Extent;
			double num14 = ((num > 0.0) ? (-1.0) : 1.0);
			double num15 = 0.5 * (num2 - num14 * num3);
			double num16 = 0.0 - num15;
			if (num16 < 0.0 - num13)
			{
				num16 = 0.0 - num13;
			}
			else if (num16 > num13)
			{
				num16 = num13;
			}
			num6 = (0.0 - num14) * num16 * segment1.Extent / num13;
			num5 = num16 + num14 * num6;
			num10 = num16 * (num16 + 2.0 * num15) + lengthSquared;
		}
		if (num10 < 0.0)
		{
			num10 = 0.0;
		}
		Segment1Parameter = num5;
		Segment1Closest = segment0.Center + num5 * segment0.Direction;
		Segment2Parameter = num6;
		Segment2Closest = segment1.Center + num6 * segment1.Direction;
		DistanceSquared = num10;
		return num10;
	}
}
