using System;
using System.Runtime.CompilerServices;

namespace Unity.Collections.LowLevel.Unsafe;

internal struct UnsafeParallelHashMapDataEnumerator
{
	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeParallelHashMapData* m_Buffer;

	internal int m_Index;

	internal int m_BucketIndex;

	internal int m_NextIndex;

	internal unsafe UnsafeParallelHashMapDataEnumerator(UnsafeParallelHashMapData* data)
	{
		m_Buffer = data;
		m_Index = -1;
		m_BucketIndex = 0;
		m_NextIndex = -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe bool MoveNext()
	{
		return UnsafeParallelHashMapData.MoveNext(m_Buffer, ref m_BucketIndex, ref m_NextIndex, out m_Index);
	}

	internal void Reset()
	{
		m_Index = -1;
		m_BucketIndex = 0;
		m_NextIndex = -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe KeyValue<TKey, TValue> GetCurrent<TKey, TValue>() where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
	{
		KeyValue<TKey, TValue> result = default(KeyValue<TKey, TValue>);
		result.m_Buffer = m_Buffer;
		result.m_Index = m_Index;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe TKey GetCurrentKey<TKey>() where TKey : unmanaged, IEquatable<TKey>
	{
		if (m_Index != -1)
		{
			return UnsafeUtility.ReadArrayElement<TKey>(m_Buffer->keys, m_Index);
		}
		return default(TKey);
	}
}
