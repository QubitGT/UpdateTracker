using System;

namespace Unity.Collections.NotBurstCompatible;

public static class Extensions
{
	[ExcludeFromBurstCompatTesting("Returns managed array")]
	public static T[] ToArray<T>(this NativeHashSet<T> set) where T : unmanaged, IEquatable<T>
	{
		NativeArray<T> nativeArray = set.ToNativeArray(Allocator.TempJob);
		T[] result = nativeArray.ToArray();
		nativeArray.Dispose();
		return result;
	}

	[ExcludeFromBurstCompatTesting("Returns managed array")]
	public static T[] ToArray<T>(this NativeParallelHashSet<T> set) where T : unmanaged, IEquatable<T>
	{
		NativeArray<T> nativeArray = set.ToNativeArray(Allocator.TempJob);
		T[] result = nativeArray.ToArray();
		nativeArray.Dispose();
		return result;
	}

	[ExcludeFromBurstCompatTesting("Returns managed array")]
	public static T[] ToArrayNBC<T>(this NativeList<T> list) where T : unmanaged
	{
		return list.AsArray().ToArray();
	}

	[ExcludeFromBurstCompatTesting("Takes managed array")]
	public static void CopyFromNBC<T>(this NativeList<T> list, T[] array) where T : unmanaged
	{
		list.Clear();
		list.Resize(array.Length, NativeArrayOptions.UninitializedMemory);
		list.AsArray().CopyFrom(array);
	}
}
