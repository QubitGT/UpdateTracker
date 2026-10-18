using System;
using System.Collections.Generic;

namespace ExitGames.Client.Photon.StructWrapping;

public static class StructWrapperUtility
{
	public static Type GetWrappedType(this object obj)
	{
		if (!(obj is StructWrapper { ttype: var ttype }))
		{
			return obj.GetType();
		}
		return ttype;
	}

	public static StructWrapper<T> Wrap<T>(this T value, bool persistant)
	{
		StructWrapper<T> structWrapper = StructWrapper<T>.staticPool.Acquire(value);
		if (persistant)
		{
			structWrapper.DisconnectFromPool();
		}
		return structWrapper;
	}

	public static StructWrapper<T> Wrap<T>(this T value)
	{
		return StructWrapper<T>.staticPool.Acquire(value);
	}

	public static StructWrapper<byte> Wrap(this byte value)
	{
		return StructWrapperPools.mappedByteWrappers[value];
	}

	public static StructWrapper<bool> Wrap(this bool value)
	{
		return StructWrapperPools.mappedBoolWrappers[value ? 1 : 0];
	}

	public static bool IsType<T>(this object obj)
	{
		if (obj is T)
		{
			return true;
		}
		if (obj is StructWrapper<T>)
		{
			return true;
		}
		return false;
	}

	public static T DisconnectPooling<T>(this T table) where T : IEnumerable<object>
	{
		foreach (object item in table)
		{
			if (item is StructWrapper structWrapper)
			{
				structWrapper.DisconnectFromPool();
			}
		}
		return table;
	}

	public static List<object> ReleaseAllWrappers(this List<object> collection)
	{
		foreach (object item in collection)
		{
			if (item is StructWrapper structWrapper)
			{
				structWrapper.Dispose();
			}
		}
		return collection;
	}

	public static object[] ReleaseAllWrappers(this object[] collection)
	{
		foreach (object obj in collection)
		{
			if (obj is StructWrapper structWrapper)
			{
				structWrapper.Dispose();
			}
		}
		return collection;
	}

	public static Hashtable ReleaseAllWrappers(this Hashtable table)
	{
		foreach (object value in table.Values)
		{
			if (value is StructWrapper structWrapper)
			{
				structWrapper.Dispose();
			}
		}
		return table;
	}

	public static void BoxAll(this Hashtable table, bool recursive = false)
	{
		foreach (object value in table.Values)
		{
			if (recursive && value is Hashtable table2)
			{
				table2.BoxAll();
			}
			if (value is StructWrapper structWrapper)
			{
				structWrapper.Box();
			}
		}
	}

	public static T Unwrap<T>(this object obj)
	{
		if (!(obj is StructWrapper<T> { value: var value } structWrapper))
		{
			return (T)obj;
		}
		if ((structWrapper.pooling & Pooling.ReleaseOnUnwrap) == Pooling.ReleaseOnUnwrap)
		{
			structWrapper.Dispose();
		}
		return structWrapper.value;
	}

	public static T Get<T>(this object obj)
	{
		if (!(obj is StructWrapper<T> { value: var value }))
		{
			return (T)obj;
		}
		return value;
	}

	public static T Unwrap<T>(this Hashtable table, object key)
	{
		object obj = table[key];
		return obj.Unwrap<T>();
	}

	public static bool TryUnwrapValue<T>(this Hashtable table, byte key, out T value) where T : new()
	{
		if (!table.TryGetValue(key, out var value2))
		{
			value = default(T);
			return false;
		}
		value = value2.Unwrap<T>();
		return true;
	}

	public static bool TryGetValue<T>(this Hashtable table, byte key, out T value) where T : new()
	{
		if (!table.TryGetValue(key, out var value2))
		{
			value = default(T);
			return false;
		}
		value = value2.Get<T>();
		return true;
	}

	public static bool TryGetValue<T>(this Hashtable table, object key, out T value) where T : new()
	{
		if (!table.TryGetValue(key, out var value2))
		{
			value = default(T);
			return false;
		}
		value = value2.Get<T>();
		return true;
	}

	public static bool TryUnwrapValue<T>(this Hashtable table, object key, out T value) where T : new()
	{
		if (!table.TryGetValue(key, out var value2))
		{
			value = default(T);
			return false;
		}
		value = value2.Unwrap<T>();
		return true;
	}

	public static T Unwrap<T>(this Hashtable table, byte key)
	{
		object obj = table[key];
		return obj.Unwrap<T>();
	}

	public static T Get<T>(this Hashtable table, byte key)
	{
		object obj = table[key];
		return obj.Get<T>();
	}
}
