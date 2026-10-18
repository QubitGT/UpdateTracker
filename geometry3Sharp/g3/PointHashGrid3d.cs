using System;
using System.Collections.Generic;
using System.Threading;

namespace g3;

public class PointHashGrid3d<T>
{
	private Dictionary<Vector3i, List<T>> Hash;

	private ScaleGridIndexer3 Indexer;

	private T invalidValue;

	private SpinLock spinlock;

	public T InvalidValue => invalidValue;

	public PointHashGrid3d(double cellSize, T invalidValue)
	{
		Hash = new Dictionary<Vector3i, List<T>>();
		Indexer = new ScaleGridIndexer3
		{
			CellSize = cellSize
		};
		spinlock = default(SpinLock);
		this.invalidValue = invalidValue;
	}

	public void InsertPoint(T value, Vector3d pos)
	{
		Vector3i idx = Indexer.ToGrid(pos);
		insert_point(value, idx);
	}

	public void InsertPointUnsafe(T value, Vector3d pos)
	{
		Vector3i idx = Indexer.ToGrid(pos);
		insert_point(value, idx, threadsafe: false);
	}

	public bool RemovePoint(T value, Vector3d pos)
	{
		Vector3i idx = Indexer.ToGrid(pos);
		return remove_point(value, idx);
	}

	public bool RemovePointUnsafe(T value, Vector3d pos)
	{
		Vector3i idx = Indexer.ToGrid(pos);
		return remove_point(value, idx, threadsafe: false);
	}

	public void UpdatePoint(T value, Vector3d old_pos, Vector3d new_pos)
	{
		Vector3i vector3i = Indexer.ToGrid(old_pos);
		Vector3i vector3i2 = Indexer.ToGrid(new_pos);
		if (!(vector3i == vector3i2))
		{
			remove_point(value, vector3i);
			insert_point(value, vector3i2);
		}
	}

	public void UpdatePointUnsafe(T value, Vector3d old_pos, Vector3d new_pos)
	{
		Vector3i vector3i = Indexer.ToGrid(old_pos);
		Vector3i vector3i2 = Indexer.ToGrid(new_pos);
		if (!(vector3i == vector3i2))
		{
			remove_point(value, vector3i, threadsafe: false);
			insert_point(value, vector3i2, threadsafe: false);
		}
	}

	public KeyValuePair<T, double> FindNearestInRadius(Vector3d query_pt, double radius, Func<T, double> distF, Func<T, bool> ignoreF = null)
	{
		Vector3i vector3i = Indexer.ToGrid(query_pt - radius * Vector3d.One);
		Vector3i vector3i2 = Indexer.ToGrid(query_pt + radius * Vector3d.One);
		double num = double.MaxValue;
		T key = invalidValue;
		if (ignoreF == null)
		{
			ignoreF = (T pt) => false;
		}
		for (int i = vector3i.z; i <= vector3i2.z; i++)
		{
			for (int j = vector3i.y; j <= vector3i2.y; j++)
			{
				for (int k = vector3i.x; k <= vector3i2.x; k++)
				{
					Vector3i key2 = new Vector3i(k, j, i);
					if (!Hash.TryGetValue(key2, out var value))
					{
						continue;
					}
					foreach (T item in value)
					{
						if (!ignoreF(item))
						{
							double num2 = distF(item);
							if (num2 < radius && num2 < num)
							{
								key = item;
								num = num2;
							}
						}
					}
				}
			}
		}
		return new KeyValuePair<T, double>(key, num);
	}

	private void insert_point(T value, Vector3i idx, bool threadsafe = true)
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

	private bool remove_point(T value, Vector3i idx, bool threadsafe = true)
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

	public void print_large_buckets()
	{
		foreach (KeyValuePair<Vector3i, List<T>> item in Hash)
		{
			if (item.Value.Count > 512)
			{
				Console.WriteLine("{0} : {1}", item.Key, item.Value.Count);
			}
		}
	}
}
