using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Splines;

[Serializable]
internal class SplineDataDictionary<T> : IEnumerable<SplineDataKeyValuePair<T>>, IEnumerable
{
	[SerializeField]
	private List<SplineDataKeyValuePair<T>> m_Data = new List<SplineDataKeyValuePair<T>>();

	public IEnumerable<string> Keys => m_Data.Select((SplineDataKeyValuePair<T> x) => x.Key);

	public IEnumerable<SplineData<T>> Values => m_Data.Select((SplineDataKeyValuePair<T> x) => x.Value);

	public SplineData<T> this[string key]
	{
		get
		{
			if (!TryGetValue(key, out var value))
			{
				return null;
			}
			return value;
		}
		set
		{
			int num = FindIndex(key);
			SplineData<T> value2 = new SplineData<T>(value);
			if (num < 0)
			{
				m_Data.Add(new SplineDataKeyValuePair<T>
				{
					Key = key,
					Value = value2
				});
			}
			else
			{
				m_Data[num].Value = value2;
			}
		}
	}

	private int FindIndex(string key)
	{
		int i = 0;
		for (int count = m_Data.Count; i < count; i++)
		{
			if (m_Data[i].Key == key)
			{
				return i;
			}
		}
		return -1;
	}

	public bool TryGetValue(string key, out SplineData<T> value)
	{
		int num = FindIndex(key);
		value = ((num < 0) ? null : m_Data[num].Value);
		return num > -1;
	}

	public SplineData<T> GetOrCreate(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			throw new ArgumentNullException("key");
		}
		if (!TryGetValue(key, out var value))
		{
			List<SplineDataKeyValuePair<T>> data = m_Data;
			SplineDataKeyValuePair<T> obj = new SplineDataKeyValuePair<T>
			{
				Key = key
			};
			value = (obj.Value = new SplineData<T>());
			data.Add(obj);
		}
		return value;
	}

	public bool Contains(string key)
	{
		return FindIndex(key) > -1;
	}

	public IEnumerator<SplineDataKeyValuePair<T>> GetEnumerator()
	{
		return m_Data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)m_Data).GetEnumerator();
	}

	public bool Remove(string key)
	{
		int num = FindIndex(key);
		if (num < 0)
		{
			return false;
		}
		m_Data.RemoveAt(num);
		return true;
	}

	public void RemoveEmpty()
	{
		for (int num = m_Data.Count - 1; num > -1; num--)
		{
			if (!string.IsNullOrEmpty(m_Data[num].Key))
			{
				SplineData<T> value = m_Data[num].Value;
				if (value == null || value.Count >= 1)
				{
					continue;
				}
			}
			Debug.Log($"{typeof(T)} remove empty key \"{m_Data[num].Key}\"");
			m_Data.RemoveAt(num);
		}
	}
}
