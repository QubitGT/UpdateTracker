using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime;

public static class Extensions
{
	private static readonly List<object> keysWithNullValue = new List<object>();

	public static void Merge(this IDictionary target, IDictionary addHash)
	{
		if (addHash == null || target.Equals(addHash))
		{
			return;
		}
		foreach (object key in addHash.Keys)
		{
			target[key] = addHash[key];
		}
	}

	public static void MergeStringKeys(this IDictionary target, IDictionary addHash)
	{
		if (addHash == null || target.Equals(addHash))
		{
			return;
		}
		foreach (object key in addHash.Keys)
		{
			if (key is string)
			{
				target[key] = addHash[key];
			}
		}
	}

	public static string ToStringFull(this IDictionary origin)
	{
		return SupportClass.DictionaryToString(origin, includeTypes: false);
	}

	public static string ToStringFull<T>(this List<T> data)
	{
		if (data == null)
		{
			return "null";
		}
		string[] array = new string[data.Count];
		for (int i = 0; i < data.Count; i++)
		{
			object obj = data[i];
			array[i] = ((obj != null) ? obj.ToString() : "null");
		}
		return string.Join(", ", array);
	}

	public static string ToStringFull(this object[] data)
	{
		if (data == null)
		{
			return "null";
		}
		string[] array = new string[data.Length];
		for (int i = 0; i < data.Length; i++)
		{
			object obj = data[i];
			array[i] = ((obj != null) ? obj.ToString() : "null");
		}
		return string.Join(", ", array);
	}

	public static ExitGames.Client.Photon.Hashtable StripToStringKeys(this IDictionary original)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		if (original != null)
		{
			foreach (object key in original.Keys)
			{
				if (key is string)
				{
					hashtable[key] = original[key];
				}
			}
		}
		return hashtable;
	}

	public static ExitGames.Client.Photon.Hashtable StripToStringKeys(this ExitGames.Client.Photon.Hashtable original)
	{
		ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
		if (original != null)
		{
			foreach (DictionaryEntry item in original)
			{
				if (item.Key is string)
				{
					hashtable[item.Key] = original[item.Key];
				}
			}
		}
		return hashtable;
	}

	public static void StripKeysWithNullValues(this IDictionary original)
	{
		lock (keysWithNullValue)
		{
			keysWithNullValue.Clear();
			foreach (DictionaryEntry item in original)
			{
				if (item.Value == null)
				{
					keysWithNullValue.Add(item.Key);
				}
			}
			for (int i = 0; i < keysWithNullValue.Count; i++)
			{
				object key = keysWithNullValue[i];
				original.Remove(key);
			}
		}
	}

	public static void StripKeysWithNullValues(this ExitGames.Client.Photon.Hashtable original)
	{
		lock (keysWithNullValue)
		{
			keysWithNullValue.Clear();
			foreach (DictionaryEntry item in original)
			{
				if (item.Value == null)
				{
					keysWithNullValue.Add(item.Key);
				}
			}
			for (int i = 0; i < keysWithNullValue.Count; i++)
			{
				object key = keysWithNullValue[i];
				original.Remove(key);
			}
		}
	}

	public static bool Contains(this int[] target, int nr)
	{
		if (target == null)
		{
			return false;
		}
		for (int i = 0; i < target.Length; i++)
		{
			if (target[i] == nr)
			{
				return true;
			}
		}
		return false;
	}
}
