using System;
using System.Collections.Generic;

namespace UnityEngine.Recorder;

[Serializable]
internal class SerializedDictionary<TKey, TValue> : ISerializationCallbackReceiver
{
	[SerializeField]
	private List<TKey> m_Keys = new List<TKey>();

	[SerializeField]
	private List<TValue> m_Values = new List<TValue>();

	private readonly Dictionary<TKey, TValue> m_Dictionary = new Dictionary<TKey, TValue>();

	public Dictionary<TKey, TValue> dictionary => m_Dictionary;

	public void OnBeforeSerialize()
	{
		m_Keys.Clear();
		m_Values.Clear();
		foreach (KeyValuePair<TKey, TValue> item in m_Dictionary)
		{
			m_Keys.Add(item.Key);
			m_Values.Add(item.Value);
		}
	}

	public void OnAfterDeserialize()
	{
		m_Dictionary.Clear();
		for (int i = 0; i < m_Keys.Count; i++)
		{
			m_Dictionary.Add(m_Keys[i], m_Values[i]);
		}
	}
}
