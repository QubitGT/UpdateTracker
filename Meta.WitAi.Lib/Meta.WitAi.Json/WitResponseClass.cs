using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Meta.WitAi.Json;

public class WitResponseClass : WitResponseNode, IEnumerable
{
	private ConcurrentDictionary<string, WitResponseNode> m_Dict = new ConcurrentDictionary<string, WitResponseNode>();

	public override string[] ChildNodeNames => m_Dict.Keys.ToArray();

	public override WitResponseNode this[string aKey]
	{
		get
		{
			if (m_Dict.ContainsKey(aKey))
			{
				return m_Dict[aKey];
			}
			return new WitResponseLazyCreator(this, aKey);
		}
		set
		{
			if (!string.IsNullOrEmpty(aKey))
			{
				if (m_Dict.ContainsKey(aKey))
				{
					m_Dict[aKey] = value;
				}
				else
				{
					m_Dict.TryAdd(aKey, value);
				}
			}
		}
	}

	public override WitResponseNode this[int aIndex]
	{
		get
		{
			if (aIndex < 0 || aIndex >= m_Dict.Count)
			{
				return null;
			}
			return m_Dict.ElementAt(aIndex).Value;
		}
		set
		{
			if (aIndex >= 0 && aIndex < m_Dict.Count)
			{
				string key = m_Dict.ElementAt(aIndex).Key;
				m_Dict[key] = value;
			}
		}
	}

	public override int Count => m_Dict.Count;

	public override IEnumerable<WitResponseNode> Childs
	{
		get
		{
			foreach (KeyValuePair<string, WitResponseNode> item in m_Dict)
			{
				yield return item.Value;
			}
		}
	}

	public bool HasChild(string child)
	{
		return m_Dict.ContainsKey(child);
	}

	public override void Add(string aKey, WitResponseNode aItem)
	{
		if (!string.IsNullOrEmpty(aKey))
		{
			if (m_Dict.ContainsKey(aKey))
			{
				m_Dict[aKey] = aItem;
			}
			else
			{
				m_Dict.TryAdd(aKey, aItem);
			}
		}
		else
		{
			m_Dict.TryAdd(Guid.NewGuid().ToString(), aItem);
		}
	}

	public override WitResponseNode Remove(string aKey)
	{
		if (!m_Dict.ContainsKey(aKey))
		{
			return null;
		}
		m_Dict.TryRemove(aKey, out var value);
		return value;
	}

	public override WitResponseNode Remove(int aIndex)
	{
		if (aIndex < 0 || aIndex >= m_Dict.Count)
		{
			return null;
		}
		KeyValuePair<string, WitResponseNode> keyValuePair = m_Dict.ElementAt(aIndex);
		m_Dict.TryRemove(keyValuePair.Key, out var value);
		return value;
	}

	public override WitResponseNode Remove(WitResponseNode aNode)
	{
		try
		{
			KeyValuePair<string, WitResponseNode> keyValuePair = m_Dict.Where((KeyValuePair<string, WitResponseNode> k) => k.Value == aNode).First();
			m_Dict.TryRemove(keyValuePair.Key, out var _);
			return aNode;
		}
		catch
		{
			return null;
		}
	}

	public IEnumerator GetEnumerator()
	{
		foreach (KeyValuePair<string, WitResponseNode> item in m_Dict)
		{
			yield return item;
		}
	}

	public T GetChild<T>(string aKey, T defaultValue = default(T))
	{
		if (!HasChild(aKey))
		{
			return defaultValue;
		}
		WitResponseNode witResponseNode = this[aKey];
		if (!(witResponseNode == null))
		{
			return witResponseNode.Cast(defaultValue);
		}
		return defaultValue;
	}

	public override string ToString()
	{
		return ToFilteredString();
	}

	public string ToString(bool ignoreEmptyFields)
	{
		return ToFilteredString(ignoreEmptyFields);
	}

	private string ToFilteredString(bool ignoreEmptyFields = false)
	{
		string text = "{";
		KeyValuePair<string, WitResponseNode>[] array = m_Dict.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, WitResponseNode> keyValuePair = array[i];
			if (!ignoreEmptyFields || !string.IsNullOrEmpty(keyValuePair.Value))
			{
				if (text.Length > 2)
				{
					text += ", ";
				}
				text = text + "\"" + WitResponseNode.Escape(keyValuePair.Key) + "\": " + (keyValuePair.Value?.ToString() ?? "\"\"");
			}
		}
		return text + "}";
	}

	public override string ToString(string aPrefix)
	{
		string text = "{ ";
		foreach (KeyValuePair<string, WitResponseNode> item in m_Dict)
		{
			if (text.Length > 3)
			{
				text += ", ";
			}
			text = text + "\n" + aPrefix + "   ";
			text = text + "\"" + WitResponseNode.Escape(item.Key) + "\": " + (item.Value?.ToString(aPrefix) ?? "\"\"");
		}
		return text + "\n" + aPrefix + "}";
	}

	public override void Serialize(BinaryWriter aWriter)
	{
		aWriter.Write((byte)2);
		aWriter.Write(m_Dict.Count);
		foreach (string key in m_Dict.Keys)
		{
			aWriter.Write(key);
			m_Dict[key].Serialize(aWriter);
		}
	}
}
