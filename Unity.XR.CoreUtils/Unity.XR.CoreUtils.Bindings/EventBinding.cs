using System;

namespace Unity.XR.CoreUtils.Bindings;

public struct EventBinding : IEventBinding
{
	private bool m_IsBound;

	public Action BindAction { get; set; }

	public Action UnbindAction { get; set; }

	public bool IsBound => m_IsBound;

	public EventBinding(Action bindAction, Action unBindAction)
	{
		BindAction = bindAction;
		UnbindAction = unBindAction;
		m_IsBound = false;
	}

	public void Bind()
	{
		if (!m_IsBound)
		{
			BindAction?.Invoke();
		}
		m_IsBound = true;
	}

	public void Unbind()
	{
		if (m_IsBound)
		{
			UnbindAction?.Invoke();
		}
		m_IsBound = false;
	}

	public void ClearBinding()
	{
		Unbind();
		BindAction = null;
		UnbindAction = null;
	}
}
