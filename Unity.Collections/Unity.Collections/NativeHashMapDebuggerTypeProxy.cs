using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

internal sealed class NativeHashMapDebuggerTypeProxy<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	private unsafe HashMapHelper<TKey>* Data;

	public unsafe List<Pair<TKey, TValue>> Items
	{
		get
		{
			if (Data == null)
			{
				return null;
			}
			List<Pair<TKey, TValue>> list = new List<Pair<TKey, TValue>>();
			NativeKeyValueArrays<TKey, TValue> keyValueArrays = Data->GetKeyValueArrays<TValue>(Allocator.Temp);
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

	public unsafe NativeHashMapDebuggerTypeProxy(NativeHashMap<TKey, TValue> target)
	{
		Data = target.m_Data;
	}

	public unsafe NativeHashMapDebuggerTypeProxy(NativeHashMap<TKey, TValue>.ReadOnly target)
	{
		Data = target.m_Data;
	}
}
