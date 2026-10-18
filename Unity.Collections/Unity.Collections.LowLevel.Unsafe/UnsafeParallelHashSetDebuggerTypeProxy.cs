using System;
using System.Collections.Generic;

namespace Unity.Collections.LowLevel.Unsafe;

internal sealed class UnsafeParallelHashSetDebuggerTypeProxy<T> where T : unmanaged, IEquatable<T>
{
	private UnsafeParallelHashSet<T> Data;

	public List<T> Items
	{
		get
		{
			List<T> list = new List<T>();
			using NativeArray<T> nativeArray = Data.ToNativeArray(Allocator.Temp);
			for (int i = 0; i < nativeArray.Length; i++)
			{
				list.Add(nativeArray[i]);
			}
			return list;
		}
	}

	public UnsafeParallelHashSetDebuggerTypeProxy(UnsafeParallelHashSet<T> data)
	{
		Data = data;
	}
}
