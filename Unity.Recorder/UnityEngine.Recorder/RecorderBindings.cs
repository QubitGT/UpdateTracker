using System;
using System.Linq;

namespace UnityEngine.Recorder;

[ExecuteInEditMode]
public class RecorderBindings : MonoBehaviour
{
	[Serializable]
	private class PropertyObjects : SerializedDictionary<string, Object>
	{
	}

	[SerializeField]
	private PropertyObjects m_References = new PropertyObjects();

	public void SetBindingValue(string id, Object value)
	{
		m_References.dictionary[id] = value;
	}

	public Object GetBindingValue(string id)
	{
		if (!m_References.dictionary.TryGetValue(id, out var value))
		{
			return null;
		}
		return value;
	}

	public bool HasBindingValue(string id)
	{
		return m_References.dictionary.ContainsKey(id);
	}

	public void RemoveBinding(string id)
	{
		if (m_References.dictionary.ContainsKey(id))
		{
			m_References.dictionary.Remove(id);
			MarkSceneDirty();
		}
	}

	public bool IsEmpty()
	{
		if (m_References != null)
		{
			return !m_References.dictionary.Keys.Any();
		}
		return true;
	}

	public void DuplicateBinding(string src, string dst)
	{
		if (m_References.dictionary.ContainsKey(src))
		{
			m_References.dictionary[dst] = m_References.dictionary[src];
			MarkSceneDirty();
		}
	}

	private void MarkSceneDirty()
	{
	}
}
