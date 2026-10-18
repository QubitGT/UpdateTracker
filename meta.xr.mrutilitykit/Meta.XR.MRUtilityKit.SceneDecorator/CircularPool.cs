using System.Collections.Generic;
using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public class CircularPool<T> : Pool<T> where T : class
{
	private int active;

	protected override int CountAll => pool.Length;

	protected override int CountActive => active;

	public CircularPool(T primitive, int size, Callbacks callbacks)
	{
		pool = new Entry[size];
		indices = new Dictionary<T, int>(size);
		index = 0;
		active = 0;
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
			index = 0;
		}
		Entry entry = pool[index];
		if (entry.active && callbacks.OnRelease != null)
		{
			callbacks.OnRelease(entry.t);
		}
		else
		{
			pool[index].active = true;
			active++;
		}
		if (callbacks.OnGet != null)
		{
			callbacks.OnGet(entry.t);
		}
		index++;
		return entry.t;
	}

	public override void Release(T t)
	{
		int num = indices[t];
		if (pool[num].active)
		{
			pool[num].active = false;
			active--;
			index--;
			if (index < 0)
			{
				index = pool.Length - 1;
			}
			Swap(num, index);
			if (callbacks.OnRelease != null)
			{
				callbacks.OnRelease(t);
			}
		}
	}
}
