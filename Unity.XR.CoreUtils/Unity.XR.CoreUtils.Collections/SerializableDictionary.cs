using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.XR.CoreUtils.Collections;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
	[Serializable]
	public struct Item
	{
		public TKey Key;

		public TValue Value;
	}

	[SerializeField]
	private List<Item> m_Items = new List<Item>();

	public List<Item> SerializedItems => m_Items;

	public SerializableDictionary()
	{
	}

	public SerializableDictionary(IDictionary<TKey, TValue> input)
		: base(input)
	{
	}

	public virtual void OnBeforeSerialize()
	{
		m_Items.Clear();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<TKey, TValue> current = enumerator.Current;
			m_Items.Add(new Item
			{
				Key = current.Key,
				Value = current.Value
			});
		}
	}

	public virtual void OnAfterDeserialize()
	{
		Clear();
		foreach (Item item in m_Items)
		{
			if (ContainsKey(item.Key))
			{
				Debug.LogWarning(string.Format("The key \"{0}\" is duplicated in the {1}.{2} and will be ignored.", item.Key, GetType().Name, "SerializedItems"));
			}
			else
			{
				Add(item.Key, item.Value);
			}
		}
	}
}
