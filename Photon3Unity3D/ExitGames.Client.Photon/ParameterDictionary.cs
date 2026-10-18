using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;

namespace ExitGames.Client.Photon;

public class ParameterDictionary : IEnumerable<KeyValuePair<byte, object>>, IEnumerable
{
	public readonly NonAllocDictionary<byte, object> paramDict;

	public readonly StructWrapperPools wrapperPools = new StructWrapperPools();

	public object this[byte key]
	{
		get
		{
			object obj = paramDict[key];
			if (!(obj is StructWrapper<object> result))
			{
				return obj;
			}
			return result;
		}
		set
		{
			paramDict[key] = value;
		}
	}

	public int Count => paramDict.Count;

	public ParameterDictionary()
	{
		paramDict = new NonAllocDictionary<byte, object>();
	}

	public ParameterDictionary(int capacity)
	{
		paramDict = new NonAllocDictionary<byte, object>((uint)capacity);
	}

	public static implicit operator NonAllocDictionary<byte, object>(ParameterDictionary value)
	{
		return value.paramDict;
	}

	IEnumerator<KeyValuePair<byte, object>> IEnumerable<KeyValuePair<byte, object>>.GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<byte, object>>)paramDict).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<KeyValuePair<byte, object>>)paramDict).GetEnumerator();
	}

	public NonAllocDictionary<byte, object>.PairIterator GetEnumerator()
	{
		return paramDict.GetEnumerator();
	}

	public void Clear()
	{
		wrapperPools.Clear();
		paramDict.Clear();
	}

	public void Add(byte code, string value)
	{
		if (paramDict.ContainsKey(code))
		{
			Debug.LogWarning(code + " already exists as key in ParameterDictionary");
		}
		paramDict[code] = value;
	}

	public void Add(byte code, Hashtable value)
	{
		if (paramDict.ContainsKey(code))
		{
			Debug.LogWarning(code + " already exists as key in ParameterDictionary");
		}
		paramDict[code] = value;
	}

	public void Add(byte code, byte value)
	{
		if (paramDict.ContainsKey(code))
		{
			Debug.LogError(code + " already exists as key in ParameterDictionary");
		}
		StructWrapper<byte> value2 = StructWrapperPools.mappedByteWrappers[value];
		paramDict[code] = value2;
	}

	public void Add(byte code, bool value)
	{
		if (paramDict.ContainsKey(code))
		{
			Debug.LogError(code + " already exists as key in ParameterDictionary");
		}
		StructWrapper<bool> value2 = StructWrapperPools.mappedBoolWrappers[value ? 1 : 0];
		paramDict[code] = value2;
	}

	public void Add(byte code, short value)
	{
		if (paramDict.ContainsKey(code))
		{
			Debug.LogWarning(code + " already exists as key in ParameterDictionary");
		}
		paramDict[code] = value;
	}

	public void Add(byte code, int value)
	{
		if (paramDict.ContainsKey(code))
		{
			Debug.LogWarning(code + " already exists as key in ParameterDictionary");
		}
		paramDict[code] = value;
	}

	public void Add(byte code, long value)
	{
		if (paramDict.ContainsKey(code))
		{
			Debug.LogWarning(code + " already exists as key in ParameterDictionary");
		}
		paramDict[code] = value;
	}

	public void Add(byte code, object value)
	{
		if (paramDict.ContainsKey(code))
		{
			Debug.LogWarning(code + " already exists as key in ParameterDictionary");
		}
		paramDict[code] = value;
	}

	public T Unwrap<T>(byte key)
	{
		object obj = paramDict[key];
		return obj.Unwrap<T>();
	}

	public T Get<T>(byte key)
	{
		object obj = paramDict[key];
		return obj.Get<T>();
	}

	public bool ContainsKey(byte key)
	{
		return paramDict.ContainsKey(key);
	}

	public object TryGetObject(byte key)
	{
		if (paramDict.TryGetValue(key, out var val))
		{
			return val;
		}
		return null;
	}

	public bool TryGetValue(byte key, out object value)
	{
		return paramDict.TryGetValue(key, out value);
	}

	public bool TryGetValue<T>(byte key, out T value) where T : struct
	{
		object val;
		bool flag = paramDict.TryGetValue(key, out val);
		if (!flag)
		{
			value = default(T);
			return false;
		}
		if (val is StructWrapper<T> structWrapper)
		{
			value = structWrapper.value;
		}
		else if (val is StructWrapper<object> structWrapper2)
		{
			value = (T)structWrapper2.value;
		}
		else
		{
			value = (T)val;
		}
		return flag;
	}

	public string ToStringFull(bool includeTypes = true)
	{
		if (includeTypes)
		{
			return $"(ParameterDictionary){SupportClass.DictionaryToString(paramDict, includeTypes)}";
		}
		return SupportClass.DictionaryToString(paramDict, includeTypes);
	}
}
