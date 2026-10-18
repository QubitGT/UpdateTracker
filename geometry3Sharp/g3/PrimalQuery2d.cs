using System;

namespace g3;

internal class PrimalQuery2d
{
	public enum OrderType
	{
		ORDER_Q0_EQUALS_Q1,
		ORDER_P_EQUALS_Q0,
		ORDER_P_EQUALS_Q1,
		ORDER_POSITIVE,
		ORDER_NEGATIVE,
		ORDER_COLLINEAR_LEFT,
		ORDER_COLLINEAR_RIGHT,
		ORDER_COLLINEAR_CONTAIN
	}

	private Func<int, Vector2d> PointF;

	public PrimalQuery2d(Func<int, Vector2d> PositionFunc)
	{
		PointF = PositionFunc;
	}

	public int ToLine(int i, int v0, int v1)
	{
		return ToLine(PointF(i), v0, v1);
	}

	public int ToLine(Vector2d test, int v0, int v1)
	{
		Vector2d vector2d = PointF(v0);
		Vector2d vector2d2 = PointF(v1);
		double num = test[0] - vector2d[0];
		double num2 = test[1] - vector2d[1];
		double num3 = vector2d2[0] - vector2d[0];
		double num4 = vector2d2[1] - vector2d[1];
		double num5 = num * num4;
		double num6 = num3 * num2;
		double num7 = num5 - num6;
		if (!(num7 > 0.0))
		{
			if (!(num7 < 0.0))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public int ToLine(int i, int v0, int v1, out int order)
	{
		return ToLine(PointF(i), v0, v1, out order);
	}

	public int ToLine(Vector2d test, int v0, int v1, out int order)
	{
		Vector2d vector2d = PointF(v0);
		Vector2d vector2d2 = PointF(v1);
		double num = test[0] - vector2d[0];
		double num2 = test[1] - vector2d[1];
		double num3 = vector2d2[0] - vector2d[0];
		double num4 = vector2d2[1] - vector2d[1];
		double num5 = num * num4;
		double num6 = num3 * num2;
		double num7 = num5 - num6;
		if (num7 > 0.0)
		{
			order = 3;
			return 1;
		}
		if (num7 < 0.0)
		{
			order = -3;
			return -1;
		}
		double num8 = num * num3;
		double num9 = num2 * num4;
		double num10 = num8 + num9;
		if (num10 == 0.0)
		{
			order = -1;
		}
		else if (num10 < 0.0)
		{
			order = -2;
		}
		else
		{
			double num11 = num * num;
			double num12 = num2 * num2;
			double num13 = num11 + num12;
			if (num10 == num13)
			{
				order = 1;
			}
			else if (num10 > num13)
			{
				order = 2;
			}
			else
			{
				order = 0;
			}
		}
		return 0;
	}

	public int ToTriangle(int i, int v0, int v1, int v2)
	{
		return ToTriangle(PointF(i), v0, v1, v2);
	}

	public int ToTriangle(Vector2d test, int v0, int v1, int v2)
	{
		int num = ToLine(test, v1, v2);
		if (num > 0)
		{
			return 1;
		}
		int num2 = ToLine(test, v0, v2);
		if (num2 < 0)
		{
			return 1;
		}
		int num3 = ToLine(test, v0, v1);
		if (num3 > 0)
		{
			return 1;
		}
		if (num == 0 || num2 == 0 || num3 == 0)
		{
			return 0;
		}
		return -1;
	}

	public int ToTriangleUnsigned(int i, int v0, int v1, int v2)
	{
		return ToTriangleUnsigned(PointF(i), v0, v1, v2);
	}

	public int ToTriangleUnsigned(Vector2d test, int v0, int v1, int v2)
	{
		int num = ToLine(test, v1, v2);
		int num2 = ToLine(test, v0, v2);
		int num3 = ToLine(test, v0, v1);
		if ((num <= 0 && num2 >= 0 && num3 <= 0) || (num >= 0 && num2 <= 0 && num3 >= 0))
		{
			if (num == 0 || num2 == 0 || num3 == 0)
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public int ToCircumcircle(int i, int v0, int v1, int v2)
	{
		return ToCircumcircle(PointF(i), v0, v1, v2);
	}

	public int ToCircumcircle(Vector2d test, int v0, int v1, int v2)
	{
		Vector2d vector2d = PointF(v0);
		Vector2d vector2d2 = PointF(v1);
		Vector2d vector2d3 = PointF(v2);
		double num = vector2d[0] - test[0];
		double num2 = vector2d[1] - test[1];
		double num3 = vector2d[0] + test[0];
		double num4 = vector2d[1] + test[1];
		double num5 = num3 * num;
		double num6 = num4 * num2;
		double num7 = num5 + num6;
		double num8 = vector2d2[0] - test[0];
		double num9 = vector2d2[1] - test[1];
		double num10 = vector2d2[0] + test[0];
		double num11 = vector2d2[1] + test[1];
		double num12 = num10 * num8;
		double num13 = num11 * num9;
		double num14 = num12 + num13;
		double num15 = vector2d3[0] - test[0];
		double num16 = vector2d3[1] - test[1];
		double num17 = vector2d3[0] + test[0];
		double num18 = vector2d3[1] + test[1];
		double num19 = num17 * num15;
		double num20 = num18 * num16;
		double num21 = num19 + num20;
		double num22 = num2 * num14;
		double num23 = num2 * num21;
		double num24 = num9 * num7;
		double num25 = num9 * num21;
		double num26 = num16 * num7;
		double num27 = num16 * num14;
		double num28 = num25 - num27;
		double num29 = num26 - num23;
		double num30 = num22 - num24;
		double num31 = num * num28;
		double num32 = num8 * num29;
		double num33 = num15 * num30;
		double num34 = num31 + num32 + num33;
		if (!(num34 < 0.0))
		{
			if (!(num34 > 0.0))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public OrderType ToLineExtended(Vector2d P, Vector2d Q0, Vector2d Q1)
	{
		double num = Q1[0] - Q0[0];
		double num2 = Q1[1] - Q0[1];
		if (num == 0.0 && num2 == 0.0)
		{
			return OrderType.ORDER_Q0_EQUALS_Q1;
		}
		double num3 = P[0] - Q0[0];
		double num4 = P[1] - Q0[1];
		if (num3 == 0.0 && num4 == 0.0)
		{
			return OrderType.ORDER_P_EQUALS_Q0;
		}
		double num5 = P[0] - Q1[0];
		double num6 = P[1] - Q1[1];
		if (num5 == 0.0 && num6 == 0.0)
		{
			return OrderType.ORDER_P_EQUALS_Q1;
		}
		double num7 = num * num4;
		double num8 = num3 * num2;
		double num9 = num7 - num8;
		if (num9 != 0.0)
		{
			if (num9 > 0.0)
			{
				return OrderType.ORDER_POSITIVE;
			}
			return OrderType.ORDER_NEGATIVE;
		}
		double num10 = num * num3;
		double num11 = num2 * num4;
		double num12 = num10 + num11;
		if (num12 < 0.0)
		{
			return OrderType.ORDER_COLLINEAR_LEFT;
		}
		double num13 = num * num;
		double num14 = num2 * num2;
		double num15 = num13 + num14;
		if (num12 > num15)
		{
			return OrderType.ORDER_COLLINEAR_RIGHT;
		}
		return OrderType.ORDER_COLLINEAR_CONTAIN;
	}
}
