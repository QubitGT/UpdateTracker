namespace Photon.Voice;

public class PrimitiveArrayPool<T> : ObjectPool<T[], int>
{
	public PrimitiveArrayPool(int capacity, string name)
		: base(capacity, name)
	{
	}

	public PrimitiveArrayPool(int capacity, string name, int info)
		: base(capacity, name, info)
	{
	}

	protected override T[] createObject(int info)
	{
		return new T[info];
	}

	protected override void destroyObject(T[] obj)
	{
	}

	protected override bool infosMatch(int i0, int i1)
	{
		return i0 == i1;
	}
}
