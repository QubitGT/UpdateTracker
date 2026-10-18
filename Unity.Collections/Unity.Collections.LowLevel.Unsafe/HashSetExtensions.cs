using System;

namespace Unity.Collections.LowLevel.Unsafe;

public static class HashSetExtensions
{
	public static void ExceptWith<T>(this ref NativeHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref NativeParallelHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref NativeParallelHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref NativeParallelHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList128Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList32Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList4096Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList512Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, FixedList64Bytes<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, NativeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, NativeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, NativeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeParallelHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeParallelHashSet<T>.ReadOnly other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}

	public static void ExceptWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Remove(item);
		}
	}

	public static void IntersectWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
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

	public static void UnionWith<T>(this ref UnsafeParallelHashSet<T> container, UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		foreach (T item in other)
		{
			container.Add(item);
		}
	}
}
