using System;
using System.Collections.Generic;

namespace Unity.Collections.LowLevel.Unsafe;

internal sealed class UnsafeHashSetDebuggerTypeProxy<T> where T : unmanaged, IEquatable<T>
{
	private HashMapHelper<T> Data;

	public List<T> Items
	{
		get
		{
			List<T> list = new List<T>();
			using NativeArray<T> nativeArray = Data.GetKeyArray(Allocator.Temp);
			for (int i = 0; i < nativeArray.Length; i++)
			{
				list.Add(nativeArray[i]);
			}
			return list;
		}
	}

	public UnsafeHashSetDebuggerTypeProxy(UnsafeHashSet<T> data)
	{
		Data = data.m_Data;
	}
}
