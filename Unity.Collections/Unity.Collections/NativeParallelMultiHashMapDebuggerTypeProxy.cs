using System;
using System.Collections.Generic;

namespace Unity.Collections;

internal sealed class NativeParallelMultiHashMapDebuggerTypeProxy<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	private NativeParallelMultiHashMap<TKey, TValue> m_Target;

	public List<ListPair<TKey, List<TValue>>> Items
	{
		get
		{
			List<ListPair<TKey, List<TValue>>> list = new List<ListPair<TKey, List<TValue>>>();
			(NativeArray<TKey>, int) tuple = default((NativeArray<TKey>, int));
			using (NativeParallelHashMap<TKey, TValue> nativeParallelHashMap = new NativeParallelHashMap<TKey, TValue>(m_Target.Count(), Allocator.Temp))
			{
				NativeParallelMultiHashMap<TKey, TValue>.KeyValueEnumerator enumerator = m_Target.GetEnumerator();
				while (enumerator.MoveNext())
				{
					nativeParallelHashMap.TryAdd(enumerator.Current.Key, default(TValue));
				}
				tuple.Item1 = nativeParallelHashMap.GetKeyArray(Allocator.Temp);
				tuple.Item2 = tuple.Item1.Length;
			}
			using (tuple.Item1)
			{
				for (int i = 0; i < tuple.Item2; i++)
				{
					List<TValue> list2 = new List<TValue>();
					if (m_Target.TryGetFirstValue(tuple.Item1[i], out var item, out var it))
					{
						do
						{
							list2.Add(item);
						}
						while (m_Target.TryGetNextValue(out item, ref it));
					}
					list.Add(new ListPair<TKey, List<TValue>>(tuple.Item1[i], list2));
				}
				return list;
			}
		}
	}

	public NativeParallelMultiHashMapDebuggerTypeProxy(NativeParallelMultiHashMap<TKey, TValue> target)
	{
		m_Target = target;
	}
}
