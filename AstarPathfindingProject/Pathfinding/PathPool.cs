using System;
using System.Collections.Generic;

namespace Pathfinding;

public static class PathPool
{
	private static readonly Dictionary<Type, Stack<Path>> pool = new Dictionary<Type, Stack<Path>>();

	private static readonly Dictionary<Type, int> totalCreated = new Dictionary<Type, int>();

	public static void Pool(Path path)
	{
		lock (pool)
		{
			if (((IPathInternals)path).Pooled)
			{
				throw new ArgumentException("The path is already pooled.");
			}
			if (!pool.TryGetValue(path.GetType(), out var value))
			{
				value = new Stack<Path>();
				pool[path.GetType()] = value;
			}
			((IPathInternals)path).Pooled = true;
			((IPathInternals)path).OnEnterPool();
			value.Push(path);
		}
	}

	public static int GetTotalCreated(Type type)
	{
		if (totalCreated.TryGetValue(type, out var value))
		{
			return value;
		}
		return 0;
	}

	public static int GetSize(Type type)
	{
		if (pool.TryGetValue(type, out var value))
		{
			return value.Count;
		}
		return 0;
	}

	public static T GetPath<T>() where T : Path, new()
	{
		lock (pool)
		{
			T val;
			if (pool.TryGetValue(typeof(T), out var value) && value.Count > 0)
			{
				val = value.Pop() as T;
			}
			else
			{
				val = new T();
				if (!totalCreated.ContainsKey(typeof(T)))
				{
					totalCreated[typeof(T)] = 0;
				}
				totalCreated[typeof(T)]++;
			}
			((IPathInternals)val).Pooled = false;
			((IPathInternals)val).Reset();
			return val;
		}
	}
}
