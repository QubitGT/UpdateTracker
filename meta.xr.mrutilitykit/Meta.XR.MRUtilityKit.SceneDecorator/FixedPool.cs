using System.Collections.Generic;
using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class FixedPool<T> : Pool<T> where T : class
{
	protected override int CountAll => pool.Length;

	protected override int CountActive => index;

	public FixedPool(T primitive, int size, Callbacks callbacks)
	{
		pool = new Entry[size];
		indices = new Dictionary<T, int>(size);
		index = 0;
		base.callbacks = callbacks;
		for (int i = 0; i < size; i++)
		{
			T val = callbacks.Create(primitive);
			pool[i].t = val;
			indices[val] = i;
		}
	}

	public override T Get()
	{
		if (index >= pool.Length)
		{
			return null;
		}
		T t = pool[index].t;
		pool[index].active = true;
		if (callbacks.OnGet != null)
		{
			callbacks.OnGet(t);
		}
		index++;
		return t;
	}

	public override void Release(T t)
	{
		int num = indices[t];
		if (pool[num].active)
		{
			pool[num].active = false;
			index--;
			Swap(num, index);
			if (callbacks.OnRelease != null)
			{
				callbacks.OnRelease(t);
			}
		}
	}
}
