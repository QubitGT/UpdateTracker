using System.Collections.Generic;

namespace Unity.XR.CoreUtils;

public static class DictionaryExtensions
{
	public static KeyValuePair<TKey, TValue> First<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
	{
		KeyValuePair<TKey, TValue> result = default(KeyValuePair<TKey, TValue>);
		Dictionary<TKey, TValue>.Enumerator enumerator = dictionary.GetEnumerator();
		if (enumerator.MoveNext())
		{
			result = enumerator.Current;
		}
		enumerator.Dispose();
		return result;
	}
}
