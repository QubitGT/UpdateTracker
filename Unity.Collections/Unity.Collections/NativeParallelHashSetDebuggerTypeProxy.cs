using System;
using System.Collections.Generic;

namespace Unity.Collections;

internal sealed class NativeParallelHashSetDebuggerTypeProxy<T> where T : unmanaged, IEquatable<T>
{
	private NativeParallelHashSet<T> Data;

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

	public NativeParallelHashSetDebuggerTypeProxy(NativeParallelHashSet<T> data)
	{
		Data = data;
	}
}
