using System;
using System.Collections.Concurrent;

namespace Meta.WitAi;

public class ObjectPool<T> : IDisposable
{
	private readonly Func<T> _generator;

	private readonly ConcurrentBag<T> _available;

	public ObjectPool(Func<T> generator, int preload = 0)
	{
		_generator = generator ?? throw new ArgumentNullException("generator");
		_available = new ConcurrentBag<T>();
		Preload(preload);
	}

	~ObjectPool()
	{
		Dispose();
	}

	public T Get()
	{
		if (_available.TryTake(out var result))
		{
			return result;
		}
		return _generator();
	}

	public void Return(T item)
	{
		_available.Add(item);
	}

	public void Preload(int total)
	{
		if (total > 0)
		{
			for (int i = 0; i < total; i++)
			{
				Return(_generator());
			}
		}
	}

	public void Dispose()
	{
		try
		{
			_available?.Clear();
		}
		catch (ObjectDisposedException)
		{
		}
	}
}
