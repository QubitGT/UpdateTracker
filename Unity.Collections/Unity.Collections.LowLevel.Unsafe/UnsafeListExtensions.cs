using System;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public static class UnsafeListExtensions
{
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static int IndexOf<T, U>(this UnsafeList<T> list, U value) where T : unmanaged, IEquatable<U>
	{
		return NativeArrayExtensions.IndexOf<T, U>(list.Ptr, list.Length, value);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public static bool Contains<T, U>(this UnsafeList<T> list, U value) where T : unmanaged, IEquatable<U>
	{
		return list.IndexOf(value) != -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static int IndexOf<T, U>(this UnsafeList<T>.ReadOnly list, U value) where T : unmanaged, IEquatable<U>
	{
		return NativeArrayExtensions.IndexOf<T, U>(list.Ptr, list.Length, value);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public static bool Contains<T, U>(this UnsafeList<T>.ReadOnly list, U value) where T : unmanaged, IEquatable<U>
	{
		return list.IndexOf(value) != -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static int IndexOf<T, U>(this UnsafeList<T>.ParallelReader list, U value) where T : unmanaged, IEquatable<U>
	{
		return NativeArrayExtensions.IndexOf<T, U>(list.Ptr, list.Length, value);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public static bool Contains<T, U>(this UnsafeList<T>.ParallelReader list, U value) where T : unmanaged, IEquatable<U>
	{
		return list.IndexOf(value) != -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static bool ArraysEqual<T>(this UnsafeList<T> container, in UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		if (container.Length != other.Length)
		{
			return false;
		}
		for (int i = 0; i != container.Length; i++)
		{
			if (!container[i].Equals(other[i]))
			{
				return false;
			}
		}
		return true;
	}
}
