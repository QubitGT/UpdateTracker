using System.Collections.Generic;

namespace g3;

public class LockingQueue<T>
{
	private Queue<T> queue;

	private object queue_lock;

	public int Count
	{
		get
		{
			lock (queue_lock)
			{
				return queue.Count;
			}
		}
	}

	public LockingQueue()
	{
		queue = new Queue<T>();
		queue_lock = new object();
	}

	public bool Remove(ref T val)
	{
		lock (queue_lock)
		{
			if (queue.Count > 0)
			{
				val = queue.Dequeue();
				return true;
			}
			return false;
		}
	}

	public void Add(T obj)
	{
		lock (queue_lock)
		{
			queue.Enqueue(obj);
		}
	}
}
