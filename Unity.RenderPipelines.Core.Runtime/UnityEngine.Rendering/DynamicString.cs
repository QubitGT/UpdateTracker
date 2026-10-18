using System.Diagnostics;

namespace UnityEngine.Rendering;

[DebuggerDisplay("Size = {size} Capacity = {capacity}")]
public class DynamicString : DynamicArray<char>
{
	public DynamicString()
	{
	}

	public DynamicString(string s)
		: base(s.Length, resize: true)
	{
		for (int i = 0; i < s.Length; i++)
		{
			m_Array[i] = s[i];
		}
	}

	public DynamicString(int capacity)
		: base(capacity, resize: false)
	{
	}

	public void Append(string s)
	{
		int num = base.size;
		Reserve(base.size + s.Length, keepContent: true);
		for (int i = 0; i < s.Length; i++)
		{
			m_Array[num + i] = s[i];
		}
		base.size += s.Length;
		BumpVersion();
	}

	public void Append(DynamicString s)
	{
		AddRange(s);
	}

	public override string ToString()
	{
		return new string(m_Array, 0, base.size);
	}
}
