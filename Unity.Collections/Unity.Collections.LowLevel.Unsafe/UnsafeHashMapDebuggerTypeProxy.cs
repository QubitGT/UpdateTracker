using System;
using System.Collections.Generic;

namespace Unity.Collections.LowLevel.Unsafe;

internal sealed class UnsafeHashMapDebuggerTypeProxy<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	private HashMapHelper<TKey> Data;

	public List<Pair<TKey, TValue>> Items
	{
		get
		{
			List<Pair<TKey, TValue>> list = new List<Pair<TKey, TValue>>();
			NativeKeyValueArrays<TKey, TValue> keyValueArrays = Data.GetKeyValueArrays<TValue>(Allocator.Temp);
			try
			{
				for (int i = 0; i < keyValueArrays.Length; i++)
				{
					NativeArray<TKey> keys = keyValueArrays.Keys;
					TKey k = keys[i];
					NativeArray<TValue> values = keyValueArrays.Values;
					list.Add(new Pair<TKey, TValue>(k, values[i]));
				}
				return list;
			}
			finally
			{
				((IDisposable)keyValueArrays).Dispose();
			}
		}
	}

	public UnsafeHashMapDebuggerTypeProxy(UnsafeHashMap<TKey, TValue> target)
	{
		Data = target.m_Data;
	}

	public UnsafeHashMapDebuggerTypeProxy(UnsafeHashMap<TKey, TValue>.ReadOnly target)
	{
		Data = target.m_Data;
	}
}
