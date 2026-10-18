using System;

namespace UnityEngine.VFX;

[Serializable]
internal abstract class EventAttributeValue<T> : EventAttribute
{
	private readonly Func<VFXEventAttribute, int, bool> m_HasFunc;

	private readonly Action<VFXEventAttribute, int, T> m_ApplyFunc;

	public T value;

	protected EventAttributeValue(Func<VFXEventAttribute, int, bool> hasFunc, Action<VFXEventAttribute, int, T> applyFunc)
	{
		m_HasFunc = hasFunc;
		m_ApplyFunc = applyFunc;
	}

	public sealed override bool ApplyToVFX(VFXEventAttribute eventAttribute)
	{
		if (!m_HasFunc(eventAttribute, id))
		{
			return false;
		}
		m_ApplyFunc(eventAttribute, id, value);
		return true;
	}
}
