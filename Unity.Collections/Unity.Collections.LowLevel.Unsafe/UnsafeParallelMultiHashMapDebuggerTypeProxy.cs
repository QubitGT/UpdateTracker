using System;
using System.Collections.Generic;

namespace Unity.Collections.LowLevel.Unsafe;

internal sealed class UnsafeParallelMultiHashMapDebuggerTypeProxy<TKey, TValue> where TKey : unmanaged, IEquatable<TKey>, IComparable<TKey> where TValue : unmanaged
{
	private UnsafeParallelMultiHashMap<TKey, TValue> m_Target;

	public List<ListPair<TKey, List<TValue>>> Items
	{
		get
		{
			List<ListPair<TKey, List<TValue>>> list = new List<ListPair<TKey, List<TValue>>>();
			(NativeArray<TKey>, int) uniqueKeyArray = GetUniqueKeyArray(ref m_Target, Allocator.Temp);
			using (uniqueKeyArray.Item1)
			{
				for (int i = 0; i < uniqueKeyArray.Item2; i++)
				{
					List<TValue> list2 = new List<TValue>();
					if (m_Target.TryGetFirstValue(uniqueKeyArray.Item1[i], out var item, out var it))
					{
						do
						{
							list2.Add(item);
						}
						while (m_Target.TryGetNextValue(out item, ref it));
					}
					list.Add(new ListPair<TKey, List<TValue>>(uniqueKeyArray.Item1[i], list2));
				}
				return list;
			}
		}
	}

	public UnsafeParallelMultiHashMapDebuggerTypeProxy(UnsafeParallelMultiHashMap<TKey, TValue> target)
	{
		m_Target = target;
	}

	public static (NativeArray<TKey>, int) GetUniqueKeyArray(ref UnsafeParallelMultiHashMap<TKey, TValue> hashMap, AllocatorManager.AllocatorHandle allocator)
	{
		NativeArray<TKey> keyArray = hashMap.GetKeyArray(allocator);
		keyArray.Sort();
		int item = keyArray.Unique();
		return (keyArray, item);
	}
}
