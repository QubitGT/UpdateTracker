using System.Collections.Generic;

namespace Unity.XR.CoreUtils.Bindings;

public class BindingsGroup
{
	private readonly List<IEventBinding> m_Bindings = new List<IEventBinding>();

	public void AddBinding(IEventBinding binding)
	{
		m_Bindings.Add(binding);
	}

	public void ClearBinding(IEventBinding binding)
	{
		m_Bindings.Remove(binding);
		binding?.ClearBinding();
	}

	public void Bind()
	{
		for (int i = 0; i < m_Bindings.Count; i++)
		{
			m_Bindings[i]?.Bind();
		}
	}

	public void Unbind()
	{
		for (int i = 0; i < m_Bindings.Count; i++)
		{
			m_Bindings[i]?.Unbind();
		}
	}

	public void Clear()
	{
		for (int i = 0; i < m_Bindings.Count; i++)
		{
			m_Bindings[i]?.ClearBinding();
		}
		m_Bindings.Clear();
	}
}
