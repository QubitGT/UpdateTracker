using System.Collections.Generic;

namespace g3;

public class Query2Int64 : Query2d
{
	public Query2Int64(IList<Vector2d> Vertices)
		: base(Vertices)
	{
	}

	public override int ToLine(ref Vector2d test, int v0, int v1)
	{
		Vector2d vector2d = mVertices[v0];
		Vector2d vector2d2 = mVertices[v1];
		long x = (long)test.x - (long)vector2d.x;
		long y = (long)test.y - (long)vector2d.y;
		long x2 = (long)vector2d2.x - (long)vector2d.x;
		long y2 = (long)vector2d2.y - (long)vector2d.y;
		long num = Det2(x, y, x2, y2);
		if (num <= 0)
		{
			if (num >= 0)
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	public override int ToCircumcircle(ref Vector2d test, int v0, int v1, int v2)
	{
		Vector2d vector2d = mVertices[v0];
		Vector2d vector2d2 = mVertices[v1];
		Vector2d vector2d3 = mVertices[v2];
		Vector2l vector2l = new Vector2l((long)test.x, (long)test.y);
		Vector2l vector2l2 = new Vector2l((long)vector2d.x, (long)vector2d.y);
		Vector2l vector2l3 = new Vector2l((long)vector2d2.x, (long)vector2d2.y);
		Vector2l vector2l4 = new Vector2l((long)vector2d3.x, (long)vector2d3.y);
		long num = vector2l2.x + vector2l.x;
		long num2 = vector2l2.x - vector2l.x;
		long num3 = vector2l2.y + vector2l.y;
		long num4 = vector2l2.y - vector2l.y;
		long num5 = vector2l3.x + vector2l.x;
		long num6 = vector2l3.x - vector2l.x;
		long num7 = vector2l3.y + vector2l.y;
		long num8 = vector2l3.y - vector2l.y;
		long num9 = vector2l4.x + vector2l.x;
		long num10 = vector2l4.x - vector2l.x;
		long num11 = vector2l4.y + vector2l.y;
		long num12 = vector2l4.y - vector2l.y;
		long z = num * num2 + num3 * num4;
		long z2 = num5 * num6 + num7 * num8;
		long z3 = num9 * num10 + num11 * num12;
		long num13 = Det3(num2, num4, z, num6, num8, z2, num10, num12, z3);
		if (num13 >= 0)
		{
			if (num13 <= 0)
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}

	private long Dot(long x0, long y0, long x1, long y1)
	{
		return x0 * x1 + y0 * y1;
	}

	private long Det2(long x0, long y0, long x1, long y1)
	{
		return x0 * y1 - x1 * y0;
	}

	private long Det3(long x0, long y0, long z0, long x1, long y1, long z1, long x2, long y2, long z2)
	{
		long num = y1 * z2 - y2 * z1;
		long num2 = y2 * z0 - y0 * z2;
		long num3 = y0 * z1 - y1 * z0;
		return x0 * num + x1 * num2 + x2 * num3;
	}
}
