using System;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder;

internal sealed class ObjectPool<T> : IDisposable
{
	private bool m_IsDisposed;

	private Queue<T> m_Pool = new Queue<T>();

	public int desiredSize;

	public Func<T> constructor;

	public Action<T> destructor;

	public ObjectPool(int initialSize, int desiredSize, Func<T> constructor, Action<T> destructor, bool lazyInitialization = false)
	{
		if (constructor == null)
		{
			throw new ArgumentNullException("constructor");
		}
		if (destructor == null)
		{
			throw new ArgumentNullException("destructor");
		}
		this.constructor = constructor;
		this.destructor = destructor;
		this.desiredSize = desiredSize;
		for (int i = 0; i < initialSize && i < desiredSize; i++)
		{
			if (lazyInitialization)
			{
				break;
			}
			m_Pool.Enqueue(constructor());
		}
	}

	public T Dequeue()
	{
		if (m_Pool.Count > 0)
		{
			return m_Pool.Dequeue();
		}
		return constructor();
	}

	public void Enqueue(T obj)
	{
		if (m_Pool.Count < desiredSize)
		{
			m_Pool.Enqueue(obj);
		}
		else
		{
			destructor(obj);
		}
	}

	public void Empty()
	{
		int count = m_Pool.Count;
		for (int i = 0; i < count; i++)
		{
			destructor(m_Pool.Dequeue());
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && !m_IsDisposed)
		{
			Empty();
			m_IsDisposed = true;
		}
	}
}
