using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

internal sealed class NativeHashSetDebuggerTypeProxy<T> where T : unmanaged, IEquatable<T>
{
	private unsafe HashMapHelper<T>* Data;

	public unsafe List<T> Items
	{
		get
		{
			if (Data == null)
			{
				return null;
			}
			List<T> list = new List<T>();
			using NativeArray<T> nativeArray = Data->GetKeyArray(Allocator.Temp);
			for (int i = 0; i < nativeArray.Length; i++)
			{
				list.Add(nativeArray[i]);
			}
			return list;
		}
	}

	public unsafe NativeHashSetDebuggerTypeProxy(NativeHashSet<T> data)
	{
		Data = data.m_Data;
	}
}
