namespace g3;

public class QueryTuple2d
{
	private Vector2dTuple3 mVertices;

	public QueryTuple2d(Vector2d v0, Vector2d v1, Vector2d v2)
	{
		mVertices = new Vector2dTuple3(v0, v1, v2);
	}

	public QueryTuple2d(Vector2dTuple3 tuple)
	{
		mVertices = tuple;
	}

	public int ToLine(int i, int v0, int v1)
	{
		return ToLine(mVertices[i], v0, v1);
	}

	public int ToLine(Vector2d test, int v0, int v1)
	{
		bool num = Sort(ref v0, ref v1);
		Vector2d vector2d = mVertices[v0];
		Vector2d vector2d2 = mVertices[v1];
		double x = test[0] - vector2d[0];
		double y = test[1] - vector2d[1];
		double x2 = vector2d2[0] - vector2d[0];
		double y2 = vector2d2[1] - vector2d[1];
		double num2 = Det2(x, y, x2, y2);
		if (!num)
		{
			num2 = 0.0 - num2;
		}
		if (!(num2 > 0.0))
		{
			if (!(num2 < 0.0))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public int ToTriangle(int i, int v0, int v1, int v2)
	{
		return ToTriangle(mVertices[i], v0, v1, v2);
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

	public int ToCircumcircle(int i, int v0, int v1, int v2)
	{
		return ToCircumcircle(mVertices[i], v0, v1, v2);
	}

	public int ToCircumcircle(Vector2d test, int v0, int v1, int v2)
	{
		bool num = Sort(ref v0, ref v1, ref v2);
		Vector2d vector2d = mVertices[v0];
		Vector2d vector2d2 = mVertices[v1];
		Vector2d vector2d3 = mVertices[v2];
		double num2 = vector2d.x + test.x;
		double num3 = vector2d.x - test.x;
		double num4 = vector2d.y + test.y;
		double num5 = vector2d.y - test.y;
		double num6 = vector2d2.x + test.x;
		double num7 = vector2d2.x - test.x;
		double num8 = vector2d2.y + test.y;
		double num9 = vector2d2.y - test.y;
		double num10 = vector2d3.x + test.x;
		double num11 = vector2d3.x - test.x;
		double num12 = vector2d3.y + test.y;
		double num13 = vector2d3.y - test.y;
		double z = num2 * num3 + num4 * num5;
		double z2 = num6 * num7 + num8 * num9;
		double z3 = num10 * num11 + num12 * num13;
		double num14 = Det3(num3, num5, z, num7, num9, z2, num11, num13, z3);
		if (!num)
		{
			num14 = 0.0 - num14;
		}
		if (!(num14 < 0.0))
		{
			if (!(num14 > 0.0))
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public static double Dot(double x0, double y0, double x1, double y1)
	{
		return x0 * x1 + y0 * y1;
	}

	public static double Det2(double x0, double y0, double x1, double y1)
	{
		return x0 * y1 - x1 * y0;
	}

	public static double Det3(double x0, double y0, double z0, double x1, double y1, double z1, double x2, double y2, double z2)
	{
		double num = y1 * z2 - y2 * z1;
		double num2 = y2 * z0 - y0 * z2;
		double num3 = y0 * z1 - y1 * z0;
		return x0 * num + x1 * num2 + x2 * num3;
	}

	public static bool Sort(ref int v0, ref int v1)
	{
		int key;
		int key2;
		bool result;
		if (v0 < v1)
		{
			key = 0;
			key2 = 1;
			result = true;
		}
		else
		{
			key = 1;
			key2 = 0;
			result = false;
		}
		Index2i index2i = new Index2i(v0, v1);
		v0 = index2i[key];
		v1 = index2i[key2];
		return result;
	}

	public static bool Sort(ref int v0, ref int v1, ref int v2)
	{
		int key;
		int key2;
		int key3;
		bool result;
		if (v0 < v1)
		{
			if (v2 < v0)
			{
				key = 2;
				key2 = 0;
				key3 = 1;
				result = true;
			}
			else if (v2 < v1)
			{
				key = 0;
				key2 = 2;
				key3 = 1;
				result = false;
			}
			else
			{
				key = 0;
				key2 = 1;
				key3 = 2;
				result = true;
			}
		}
		else if (v2 < v1)
		{
			key = 2;
			key2 = 1;
			key3 = 0;
			result = false;
		}
		else if (v2 < v0)
		{
			key = 1;
			key2 = 2;
			key3 = 0;
			result = true;
		}
		else
		{
			key = 1;
			key2 = 0;
			key3 = 2;
			result = false;
		}
		Index3i index3i = new Index3i(v0, v1, v2);
		v0 = index3i[key];
		v1 = index3i[key2];
		v2 = index3i[key3];
		return result;
	}

	public static bool Sort(ref int v0, ref int v1, ref int v2, ref int v3)
	{
		int key;
		int key2;
		int key3;
		int key4;
		bool result;
		if (v0 < v1)
		{
			if (v2 < v3)
			{
				if (v1 < v2)
				{
					key = 0;
					key2 = 1;
					key3 = 2;
					key4 = 3;
					result = true;
				}
				else if (v3 < v0)
				{
					key = 2;
					key2 = 3;
					key3 = 0;
					key4 = 1;
					result = true;
				}
				else if (v2 < v0)
				{
					if (v3 < v1)
					{
						key = 2;
						key2 = 0;
						key3 = 3;
						key4 = 1;
						result = false;
					}
					else
					{
						key = 2;
						key2 = 0;
						key3 = 1;
						key4 = 3;
						result = true;
					}
				}
				else if (v3 < v1)
				{
					key = 0;
					key2 = 2;
					key3 = 3;
					key4 = 1;
					result = true;
				}
				else
				{
					key = 0;
					key2 = 2;
					key3 = 1;
					key4 = 3;
					result = false;
				}
			}
			else if (v1 < v3)
			{
				key = 0;
				key2 = 1;
				key3 = 3;
				key4 = 2;
				result = false;
			}
			else if (v2 < v0)
			{
				key = 3;
				key2 = 2;
				key3 = 0;
				key4 = 1;
				result = false;
			}
			else if (v3 < v0)
			{
				if (v2 < v1)
				{
					key = 3;
					key2 = 0;
					key3 = 2;
					key4 = 1;
					result = true;
				}
				else
				{
					key = 3;
					key2 = 0;
					key3 = 1;
					key4 = 2;
					result = false;
				}
			}
			else if (v2 < v1)
			{
				key = 0;
				key2 = 3;
				key3 = 2;
				key4 = 1;
				result = false;
			}
			else
			{
				key = 0;
				key2 = 3;
				key3 = 1;
				key4 = 2;
				result = true;
			}
		}
		else if (v2 < v3)
		{
			if (v0 < v2)
			{
				key = 1;
				key2 = 0;
				key3 = 2;
				key4 = 3;
				result = false;
			}
			else if (v3 < v1)
			{
				key = 2;
				key2 = 3;
				key3 = 1;
				key4 = 0;
				result = false;
			}
			else if (v2 < v1)
			{
				if (v3 < v0)
				{
					key = 2;
					key2 = 1;
					key3 = 3;
					key4 = 0;
					result = true;
				}
				else
				{
					key = 2;
					key2 = 1;
					key3 = 0;
					key4 = 3;
					result = false;
				}
			}
			else if (v3 < v0)
			{
				key = 1;
				key2 = 2;
				key3 = 3;
				key4 = 0;
				result = false;
			}
			else
			{
				key = 1;
				key2 = 2;
				key3 = 0;
				key4 = 3;
				result = true;
			}
		}
		else if (v0 < v3)
		{
			key = 1;
			key2 = 0;
			key3 = 3;
			key4 = 2;
			result = true;
		}
		else if (v2 < v1)
		{
			key = 3;
			key2 = 2;
			key3 = 1;
			key4 = 0;
			result = true;
		}
		else if (v3 < v1)
		{
			if (v2 < v0)
			{
				key = 3;
				key2 = 1;
				key3 = 2;
				key4 = 0;
				result = false;
			}
			else
			{
				key = 3;
				key2 = 1;
				key3 = 0;
				key4 = 2;
				result = true;
			}
		}
		else if (v2 < v0)
		{
			key = 1;
			key2 = 3;
			key3 = 2;
			key4 = 0;
			result = true;
		}
		else
		{
			key = 1;
			key2 = 3;
			key3 = 0;
			key4 = 2;
			result = false;
		}
		Index4i index4i = new Index4i(v0, v1, v2, v3);
		v0 = index4i[key];
		v1 = index4i[key2];
		v2 = index4i[key3];
		v3 = index4i[key4];
		return result;
	}
}
