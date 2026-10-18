using System.Collections.Generic;

namespace Unity.XR.CoreUtils;

public class ObjectPool<T> where T : class, new()
{
	protected readonly Queue<T> PooledQueue = new Queue<T>();

	public virtual T Get()
	{
		if (PooledQueue.Count != 0)
		{
			return PooledQueue.Dequeue();
		}
		return new T();
	}

	public void Recycle(T instance)
	{
		ClearInstance(instance);
		PooledQueue.Enqueue(instance);
	}

	protected virtual void ClearInstance(T instance)
	{
	}
}
