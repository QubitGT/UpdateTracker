using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

public static class HashSetExtensions
{
	public static void ExceptWith<T>(this ref NativeHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count, Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		UnsafeList<T> other2 = new UnsafeList<T>(container.Count(), Allocator.Temp);
		foreach (T item in other)
		{
			T value = item;
			if (container.Contains(value))
			{
				other2.Add(in value);
			}
		}
		container.Clear();
		container.UnionWith(other2);
		other2.Dispose();
	}

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}
}
