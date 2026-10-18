using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityEngine.Rendering;

internal sealed class SerializedDictionaryDebugView<K, V>
{
	private IDictionary<K, V> dict;

	[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
	public KeyValuePair<K, V>[] Items
	{
		get
		{
			KeyValuePair<K, V>[] array = new KeyValuePair<K, V>[dict.Count];
			dict.CopyTo(array, 0);
			return array;
		}
	}

	public SerializedDictionaryDebugView(IDictionary<K, V> dictionary)
	{
		if (dictionary == null)
		{
			throw new ArgumentNullException(dictionary.ToString());
		}
		dict = dictionary;
	}
}
