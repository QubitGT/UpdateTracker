using System;
using System.Collections.Generic;

namespace Unity.Collections.LowLevel.Unsafe;

internal sealed class UnsafeParallelHashMapDebuggerTypeProxy<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	private UnsafeParallelHashMap<TKey, TValue> m_Target;

	public List<Pair<TKey, TValue>> Items
	{
		get
		{
			List<Pair<TKey, TValue>> list = new List<Pair<TKey, TValue>>();
			NativeKeyValueArrays<TKey, TValue> keyValueArrays = m_Target.GetKeyValueArrays(Allocator.Temp);
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

	public UnsafeParallelHashMapDebuggerTypeProxy(UnsafeParallelHashMap<TKey, TValue> target)
	{
		m_Target = target;
	}
}
