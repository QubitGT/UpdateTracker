using System;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class TriangleBinsGrid2d
{
	private ShiftGridIndexer2 indexer;

	private AxisAlignedBox2d bounds;

	private SmallListSet bins_list;

	private int bins_x;

	private int bins_y;

	private AxisAlignedBox2i grid_bounds;

	private SpinLock spinlock;

	public AxisAlignedBox2d Bounds => bounds;

	public TriangleBinsGrid2d(AxisAlignedBox2d bounds, int numCells)
	{
		this.bounds = bounds;
		double num = bounds.MaxDim / (double)numCells;
		Vector2d origin = bounds.Min - num * 0.5 * Vector2d.One;
		indexer = new ShiftGridIndexer2(origin, num);
		bins_x = (int)(bounds.Width / num) + 2;
		bins_y = (int)(bounds.Height / num) + 2;
		grid_bounds = new AxisAlignedBox2i(0, 0, bins_x - 1, bins_y - 1);
		bins_list = new SmallListSet();
		bins_list.Resize(bins_x * bins_y);
	}

	public void InsertTriangle(int triangle_id, ref Vector2d a, ref Vector2d b, ref Vector2d c)
	{
		insert_triangle(triangle_id, ref a, ref b, ref c);
	}

	public void InsertTriangleUnsafe(int triangle_id, ref Vector2d a, ref Vector2d b, ref Vector2d c)
	{
		insert_triangle(triangle_id, ref a, ref b, ref c, threadsafe: false);
	}

	public void RemoveTriangle(int triangle_id, ref Vector2d a, ref Vector2d b, ref Vector2d c)
	{
		remove_triangle(triangle_id, ref a, ref b, ref c);
	}

	public void RemoveTriangleUnsafe(int triangle_id, ref Vector2d a, ref Vector2d b, ref Vector2d c)
	{
		remove_triangle(triangle_id, ref a, ref b, ref c, threadsafe: false);
	}

	public int FindContainingTriangle(Vector2d query_pt, Func<int, Vector2d, bool> containsF, Func<int, bool> ignoreF = null)
	{
		Vector2i v = indexer.ToGrid(query_pt);
		if (!grid_bounds.Contains(v))
		{
			return -1;
		}
		int list_index = v.y * bins_x + v.x;
		if (ignoreF == null)
		{
			foreach (int item in bins_list.ValueItr(list_index))
			{
				if (containsF(item, query_pt))
				{
					return item;
				}
			}
		}
		else
		{
			foreach (int item2 in bins_list.ValueItr(list_index))
			{
				if (!ignoreF(item2) && containsF(item2, query_pt))
				{
					return item2;
				}
			}
		}
		return -1;
	}

	public void FindTrianglesInRange(AxisAlignedBox2d range, HashSet<int> triangles)
	{
		Vector2i v = indexer.ToGrid(range.Min);
		if (!grid_bounds.Contains(v))
		{
			throw new Exception("TriangleBinsGrid2d.FindTrianglesInRange: range.Min is out of bounds");
		}
		Vector2i v2 = indexer.ToGrid(range.Max);
		if (!grid_bounds.Contains(v2))
		{
			throw new Exception("TriangleBinsGrid2d.FindTrianglesInRange: range.Max is out of bounds");
		}
		for (int i = v.y; i <= v2.y; i++)
		{
			for (int j = v.x; j <= v2.x; j++)
			{
				int list_index = i * bins_x + j;
				foreach (int item in bins_list.ValueItr(list_index))
				{
					triangles.Add(item);
				}
			}
		}
	}

	private void insert_triangle(int triangle_id, ref Vector2d a, ref Vector2d b, ref Vector2d c, bool threadsafe = true)
	{
		bool lockTaken = false;
		while (threadsafe && !lockTaken)
		{
			spinlock.Enter(ref lockTaken);
		}
		AxisAlignedBox2d axisAlignedBox2d = BoundsUtil.Bounds(ref a, ref b, ref c);
		Vector2i vector2i = indexer.ToGrid(axisAlignedBox2d.Min);
		Vector2i vector2i2 = indexer.ToGrid(axisAlignedBox2d.Max);
		for (int i = vector2i.y; i <= vector2i2.y; i++)
		{
			for (int j = vector2i.x; j <= vector2i2.x; j++)
			{
				int list_index = i * bins_x + j;
				bins_list.Insert(list_index, triangle_id);
			}
		}
		if (lockTaken)
		{
			spinlock.Exit();
		}
	}

	private void remove_triangle(int triangle_id, ref Vector2d a, ref Vector2d b, ref Vector2d c, bool threadsafe = true)
	{
		bool lockTaken = false;
		while (threadsafe && !lockTaken)
		{
			spinlock.Enter(ref lockTaken);
		}
		AxisAlignedBox2d axisAlignedBox2d = BoundsUtil.Bounds(ref a, ref b, ref c);
		Vector2i vector2i = indexer.ToGrid(axisAlignedBox2d.Min);
		Vector2i vector2i2 = indexer.ToGrid(axisAlignedBox2d.Max);
		for (int i = vector2i.y; i <= vector2i2.y; i++)
		{
			for (int j = vector2i.x; j <= vector2i2.x; j++)
			{
				int list_index = i * bins_x + j;
				bins_list.Remove(list_index, triangle_id);
			}
		}
		if (lockTaken)
		{
			spinlock.Exit();
		}
	}
}
