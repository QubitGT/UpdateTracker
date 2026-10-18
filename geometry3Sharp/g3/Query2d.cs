using System.Collections.Generic;

namespace g3;

public class Query2d : QueryBase, Query2
{
	protected IList<Vector2d> mVertices;

	public Query2d(IList<Vector2d> Vertices)
	{
		mVertices = Vertices;
	}

	public int GetNumVertices()
	{
		return mVertices.Count;
	}

	public IList<Vector2d> GetVertices()
	{
		return mVertices;
	}

	public virtual int ToLine(int i, int v0, int v1)
	{
		Vector2d test = mVertices[i];
		return ToLine(ref test, v0, v1);
	}

	public virtual int ToLine(ref Vector2d test, int v0, int v1)
	{
		bool num = Sort(ref v0, ref v1);
		Vector2d vector2d = mVertices[v0];
		Vector2d vector2d2 = mVertices[v1];
		double x = test.x - vector2d.x;
		double y = test.y - vector2d.y;
		double x2 = vector2d2.x - vector2d.x;
		double y2 = vector2d2.y - vector2d.y;
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

	public virtual int ToTriangle(int i, int v0, int v1, int v2)
	{
		Vector2d test = mVertices[i];
		return ToTriangle(ref test, v0, v1, v2);
	}

	public virtual int ToTriangle(ref Vector2d test, int v0, int v1, int v2)
	{
		int num = ToLine(ref test, v1, v2);
		if (num > 0)
		{
			return 1;
		}
		int num2 = ToLine(ref test, v0, v2);
		if (num2 < 0)
		{
			return 1;
		}
		int num3 = ToLine(ref test, v0, v1);
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

	public virtual int ToCircumcircle(int i, int v0, int v1, int v2)
	{
		Vector2d test = mVertices[i];
		return ToCircumcircle(ref test, v0, v1, v2);
	}

	public virtual int ToCircumcircle(ref Vector2d test, int v0, int v1, int v2)
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

	public double Dot(double x0, double y0, double x1, double y1)
	{
		return x0 * x1 + y0 * y1;
	}

	private double Det2(double x0, double y0, double x1, double y1)
	{
		return x0 * y1 - x1 * y0;
	}

	public double Det3(double x0, double y0, double z0, double x1, double y1, double z1, double x2, double y2, double z2)
	{
		double num = y1 * z2 - y2 * z1;
		double num2 = y2 * z0 - y0 * z2;
		double num3 = y0 * z1 - y1 * z0;
		return x0 * num + x1 * num2 + x2 * num3;
	}
}
