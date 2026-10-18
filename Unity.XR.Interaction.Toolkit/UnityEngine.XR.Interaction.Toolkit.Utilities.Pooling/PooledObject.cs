using System;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities.Pooling;

internal readonly struct PooledObject<T> : IDisposable where T : class
{
	private readonly T m_ToReturn;

	private readonly LinkedPool<T> m_Pool;

	internal PooledObject(T value, LinkedPool<T> pool)
	{
		m_ToReturn = value;
		m_Pool = pool;
	}

	void IDisposable.Dispose()
	{
		m_Pool.Release(m_ToReturn);
	}
}
