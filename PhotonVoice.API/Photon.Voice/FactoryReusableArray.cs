using System;

namespace Photon.Voice;

public class FactoryReusableArray<T> : ObjectFactory<T[], int>, IDisposable
{
	private T[] arr;

	public int Info => arr.Length;

	public FactoryReusableArray(int size)
	{
		arr = new T[size];
	}

	public T[] New()
	{
		return arr;
	}

	public T[] New(int size)
	{
		if (arr.Length != size)
		{
			arr = new T[size];
		}
		return arr;
	}

	public void Free(T[] obj)
	{
	}

	public void Free(T[] obj, int info)
	{
	}

	public void Dispose()
	{
	}
}
