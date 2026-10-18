using System;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public static class NativeListExtensions
{
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static bool Contains<T, U>(this NativeList<T> list, U value) where T : unmanaged, IEquatable<U>
	{
		return NativeArrayExtensions.IndexOf<T, U>(list.GetUnsafeReadOnlyPtr(), list.Length, value) != -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static int IndexOf<T, U>(this NativeList<T> list, U value) where T : unmanaged, IEquatable<U>
	{
		return NativeArrayExtensions.IndexOf<T, U>(list.GetUnsafeReadOnlyPtr(), list.Length, value);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static bool ArraysEqual<T>(this NativeArray<T> container, in NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		return container.ArraysEqual(other.AsArray());
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static bool ArraysEqual<T>(this NativeList<T> container, in NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		return other.ArraysEqual(in container);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static bool ArraysEqual<T>(this NativeList<T> container, in NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		return container.AsArray().ArraysEqual(other.AsArray());
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static bool ArraysEqual<T>(this NativeList<T> container, in UnsafeList<T> other) where T : unmanaged, IEquatable<T>
	{
		return (*container.m_ListData).ArraysEqual(in other);
	}
}
