using System;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class SegmentHashGrid2d<T>
{
	private Dictionary<Vector2i, List<T>> Hash;

	private ScaleGridIndexer2 Indexer;

	private double MaxExtent;

	private T invalidValue;

	private SpinLock spinlock;

	public SegmentHashGrid2d(double cellSize, T invalidValue)
	{
		Hash = new Dictionary<Vector2i, List<T>>();
		Indexer = new ScaleGridIndexer2
		{
			CellSize = cellSize
		};
		MaxExtent = 0.0;
		spinlock = default(SpinLock);
		this.invalidValue = invalidValue;
	}

	public void InsertSegment(T value, Vector2d center, double extent)
	{
		Vector2i idx = Indexer.ToGrid(center);
		if (extent > MaxExtent)
		{
			MaxExtent = extent;
		}
		insert_segment(value, idx);
	}

	public void InsertSegmentUnsafe(T value, Vector2d center, double extent)
	{
		Vector2i idx = Indexer.ToGrid(center);
		if (extent > MaxExtent)
		{
			MaxExtent = extent;
		}
		insert_segment(value, idx, threadsafe: false);
	}

	public bool RemoveSegment(T value, Vector2d center)
	{
		Vector2i idx = Indexer.ToGrid(center);
		return remove_segment(value, idx);
	}

	public bool RemoveSegmentUnsafe(T value, Vector2d center)
	{
		Vector2i idx = Indexer.ToGrid(center);
		return remove_segment(value, idx, threadsafe: false);
	}

	public void UpdateSegment(T value, Vector2d old_center, Vector2d new_center, double new_extent)
	{
		if (new_extent > MaxExtent)
		{
			MaxExtent = new_extent;
		}
		Vector2i vector2i = Indexer.ToGrid(old_center);
		Vector2i vector2i2 = Indexer.ToGrid(new_center);
		if (!(vector2i == vector2i2))
		{
			remove_segment(value, vector2i);
			insert_segment(value, vector2i2);
		}
	}

	public void UpdateSegmentUnsafe(T value, Vector2d old_center, Vector2d new_center, double new_extent)
	{
		if (new_extent > MaxExtent)
		{
			MaxExtent = new_extent;
		}
		Vector2i vector2i = Indexer.ToGrid(old_center);
		Vector2i vector2i2 = Indexer.ToGrid(new_center);
		if (!(vector2i == vector2i2))
		{
			remove_segment(value, vector2i, threadsafe: false);
			insert_segment(value, vector2i2, threadsafe: false);
		}
	}

	public KeyValuePair<T, double> FindNearestInRadius(Vector2d query_pt, double radius, Func<T, double> distF, Func<T, bool> ignoreF = null)
	{
		double num = radius + MaxExtent;
		Vector2i vector2i = Indexer.ToGrid(query_pt - num * Vector2d.One);
		Vector2i vector2i2 = Indexer.ToGrid(query_pt + num * Vector2d.One);
		double num2 = double.MaxValue;
		T key = invalidValue;
		if (ignoreF == null)
		{
			ignoreF = (T pt) => false;
		}
		for (int i = vector2i.y; i <= vector2i2.y; i++)
		{
			for (int j = vector2i.x; j <= vector2i2.x; j++)
			{
				Vector2i key2 = new Vector2i(j, i);
				if (!Hash.TryGetValue(key2, out var value))
				{
					continue;
				}
				foreach (T item in value)
				{
					if (!ignoreF(item))
					{
						double num3 = distF(item);
						if (num3 < radius && num3 < num2)
						{
							key = item;
							num2 = num3;
						}
					}
				}
			}
		}
		return new KeyValuePair<T, double>(key, num2);
	}

	public KeyValuePair<T, double> FindNearestInSquaredRadius(Vector2d query_pt, double radiusSqr, Func<T, double> distSqrF, Func<T, bool> ignoreF = null)
	{
		double num = Math.Sqrt(radiusSqr) + MaxExtent;
		Vector2i vector2i = Indexer.ToGrid(query_pt - num * Vector2d.One);
		Vector2i vector2i2 = Indexer.ToGrid(query_pt + num * Vector2d.One);
		double num2 = double.MaxValue;
		T key = invalidValue;
		if (ignoreF == null)
		{
			ignoreF = (T pt) => false;
		}
		for (int i = vector2i.y; i <= vector2i2.y; i++)
		{
			for (int j = vector2i.x; j <= vector2i2.x; j++)
			{
				Vector2i key2 = new Vector2i(j, i);
				if (!Hash.TryGetValue(key2, out var value))
				{
					continue;
				}
				foreach (T item in value)
				{
					if (!ignoreF(item))
					{
						double num3 = distSqrF(item);
						if (num3 < radiusSqr && num3 < num2)
						{
							key = item;
							num2 = num3;
						}
					}
				}
			}
		}
		return new KeyValuePair<T, double>(key, num2);
	}

	private void insert_segment(T value, Vector2i idx, bool threadsafe = true)
	{
		bool lockTaken = false;
		while (threadsafe && !lockTaken)
		{
			spinlock.Enter(ref lockTaken);
		}
		if (Hash.TryGetValue(idx, out var value2))
		{
			value2.Add(value);
		}
		else
		{
			Hash[idx] = new List<T> { value };
		}
		if (lockTaken)
		{
			spinlock.Exit();
		}
	}

	private bool remove_segment(T value, Vector2i idx, bool threadsafe = true)
	{
		bool lockTaken = false;
		while (threadsafe && !lockTaken)
		{
			spinlock.Enter(ref lockTaken);
		}
		bool result = false;
		if (Hash.TryGetValue(idx, out var value2))
		{
			result = value2.Remove(value);
		}
		if (lockTaken)
		{
			spinlock.Exit();
		}
		return result;
	}
}
