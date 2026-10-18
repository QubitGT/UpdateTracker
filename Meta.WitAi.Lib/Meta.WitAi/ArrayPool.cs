using System;

namespace Meta.WitAi;

public class ArrayPool<TElementType> : ObjectPool<TElementType[]>
{
	public int Capacity { get; }

	public ArrayPool(int capacity, int preload = 0)
		: base((Func<TElementType[]>)(() => new TElementType[capacity]), preload)
	{
		Capacity = capacity;
	}
}
