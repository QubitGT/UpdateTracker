using System;
using System.Diagnostics;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[DebuggerDisplay("Key = {Key}, Value = {Value}")]
[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
{
	typeof(int),
	typeof(int)
})]
public struct KVPair<TKey, TValue> where TKey : unmanaged, IEquatable<TKey> where TValue : unmanaged
{
	internal unsafe HashMapHelper<TKey>* m_Data;

	internal int m_Index;

	internal int m_Next;

	public static KVPair<TKey, TValue> Null
	{
		get
		{
			KVPair<TKey, TValue> result = default(KVPair<TKey, TValue>);
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
				return m_Data->Keys[m_Index];
			}
			return default(TKey);
		}
	}

	public unsafe ref TValue Value => ref UnsafeUtility.AsRef<TValue>(m_Data->Ptr + sizeof(TValue) * m_Index);

	public unsafe bool GetKeyValue(out TKey key, out TValue value)
	{
		if (m_Index != -1)
		{
			key = m_Data->Keys[m_Index];
			value = UnsafeUtility.ReadArrayElement<TValue>(m_Data->Ptr, m_Index);
			return true;
		}
		key = default(TKey);
		value = default(TValue);
		return false;
	}
}
