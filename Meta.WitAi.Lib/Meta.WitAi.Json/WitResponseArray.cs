using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Meta.WitAi.Json;

public class WitResponseArray : WitResponseNode, IEnumerable
{
	private List<WitResponseNode> m_List = new List<WitResponseNode>();

	public override WitResponseNode this[int aIndex]
	{
		get
		{
			if (aIndex < 0 || aIndex >= m_List.Count)
			{
				return new WitResponseLazyCreator(this);
			}
			return m_List[aIndex];
		}
		set
		{
			if (aIndex < 0 || aIndex >= m_List.Count)
			{
				m_List.Add(value);
			}
			else
			{
				m_List[aIndex] = value;
			}
		}
	}

	public override WitResponseNode this[string aKey]
	{
		get
		{
			return new WitResponseLazyCreator(this);
		}
		set
		{
			m_List.Add(value);
		}
	}

	public override int Count => m_List.Count;

	public override IEnumerable<WitResponseNode> Childs
	{
		get
		{
			foreach (WitResponseNode item in m_List)
			{
				yield return item;
			}
		}
	}

	public override void Add(string aKey, WitResponseNode aItem)
	{
		if (!(aItem == null))
		{
			m_List.Add(aItem);
		}
	}

	public override WitResponseNode Remove(int aIndex)
	{
		if (aIndex < 0 || aIndex >= m_List.Count)
		{
			return null;
		}
		WitResponseNode result = m_List[aIndex];
		m_List.RemoveAt(aIndex);
		return result;
	}

	public override WitResponseNode Remove(WitResponseNode aNode)
	{
		m_List.Remove(aNode);
		return aNode;
	}

	public IEnumerator GetEnumerator()
	{
		foreach (WitResponseNode item in m_List)
		{
			yield return item;
		}
	}

	public override string ToString()
	{
		string text = "[";
		foreach (WitResponseNode item in m_List)
		{
			if (text.Length > 2)
			{
				text += ", ";
			}
			text += item.ToString();
		}
		return text + "]";
	}

	public override string ToString(string aPrefix)
	{
		string text = "[";
		foreach (WitResponseNode item in m_List)
		{
			if (text.Length > 3)
			{
				text += ", ";
			}
			text = text + "\n" + aPrefix + "   ";
			text += item.ToString(aPrefix + "   ");
		}
		return text + "\n" + aPrefix + "]";
	}

	public override void Serialize(BinaryWriter aWriter)
	{
		aWriter.Write((byte)1);
		aWriter.Write(m_List.Count);
		for (int i = 0; i < m_List.Count; i++)
		{
			m_List[i].Serialize(aWriter);
		}
	}
}
