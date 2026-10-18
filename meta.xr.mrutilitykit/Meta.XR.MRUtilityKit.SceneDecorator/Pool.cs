using System;
using System.Collections.Generic;
using Meta.XR.Util;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
public abstract class Pool<T> where T : class
{
	protected struct Entry
	{
		public bool active;

		public T t;
	}

	public struct Callbacks
	{
		public Func<T, T> Create;

		public Action<T> OnGet;

		public Action<T> OnRelease;
	}

	protected Entry[] pool;

	protected Dictionary<T, int> indices;

	protected int index;

	public Callbacks callbacks;

	protected abstract int CountAll { get; }

	protected abstract int CountActive { get; }

	public virtual int CountInactive => CountAll - CountActive;

	public abstract T Get();

	public abstract void Release(T t);

	protected void Swap(int i0, int i1)
	{
		indices[pool[i0].t] = i1;
		indices[pool[i1].t] = i0;
		ref Entry reference = ref pool[i0];
		ref Entry reference2 = ref pool[i1];
		Entry entry = pool[i1];
		Entry entry2 = pool[i0];
		reference = entry;
		reference2 = entry2;
	}
}
