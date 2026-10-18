using System;

namespace Photon.Voice;

public class FactoryPrimitiveArrayPool<T> : ObjectFactory<T[], int>, IDisposable
{
	private PrimitiveArrayPool<T> pool;

	public int Info => pool.Info;

	public FactoryPrimitiveArrayPool(int capacity, string name)
	{
		pool = new PrimitiveArrayPool<T>(capacity, name);
	}

	public FactoryPrimitiveArrayPool(int capacity, string name, int info)
	{
		pool = new PrimitiveArrayPool<T>(capacity, name, info);
	}

	public T[] New()
	{
		return pool.AcquireOrCreate();
	}

	public T[] New(int size)
	{
		return pool.AcquireOrCreate(size);
	}

	public void Free(T[] obj)
	{
		pool.Release(obj);
	}

	public void Free(T[] obj, int info)
	{
		pool.Release(obj, info);
	}

	public void Dispose()
	{
		pool.Dispose();
	}
}
