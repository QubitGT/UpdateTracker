using System;
using System.Diagnostics;

namespace Unity.Collections.LowLevel.Unsafe;

[DebuggerDisplay("Key = {Key}, Value = {Value}")]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
public struct KeyValue<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	internal unsafe UnsafeParallelHashMapData* m_Buffer;

	internal int m_Index;

	internal int m_Next;

	public static KeyValue<TKey, TValue> Null
	{
		get
		{
			KeyValue<TKey, TValue> result = default(KeyValue<TKey, TValue>);
			result.m_Index = -1;
			return result;
		}
	}

	public unsafe TKey Key
	{
		get
		{
			if (m_Index != -1)
			{
				return UnsafeUtility.ReadArrayElement<TKey>(m_Buffer->keys, m_Index);
			}
			return default(TKey);
		}
	}

	public unsafe ref TValue Value => ref UnsafeUtility.AsRef<TValue>(m_Buffer->values + UnsafeUtility.SizeOf<TValue>() * m_Index);

	public unsafe bool GetKeyValue(out TKey key, out TValue value)
	{
		if (m_Index != -1)
		{
			key = UnsafeUtility.ReadArrayElement<TKey>(m_Buffer->keys, m_Index);
			value = UnsafeUtility.ReadArrayElement<TValue>(m_Buffer->values, m_Index);
			return true;
		}
		key = default(TKey);
		value = default(TValue);
		return false;
	}
}
