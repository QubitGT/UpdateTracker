using System;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityEngine.Rendering.RenderGraphModule;

public sealed class RenderGraphObjectPool
{
	private class SharedObjectPoolBase
	{
		public virtual void Clear()
		{
		}
	}

	private class SharedObjectPool<T> : SharedObjectPoolBase where T : class, new()
	{
		private static readonly UnityEngine.Pool.ObjectPool<T> s_Pool = AllocatePool();

		private static UnityEngine.Pool.ObjectPool<T> AllocatePool()
		{
			UnityEngine.Pool.ObjectPool<T> result = new UnityEngine.Pool.ObjectPool<T>(() => new T());
			DynamicArray<SharedObjectPoolBase> s_AllocatedPools = RenderGraphObjectPool.s_AllocatedPools;
			SharedObjectPoolBase value = new SharedObjectPool<T>();
			s_AllocatedPools.Add(in value);
			return result;
		}

		public override void Clear()
		{
			s_Pool.Clear();
		}

		public static T Get()
		{
			return s_Pool.Get();
		}

		public static void Release(T toRelease)
		{
			s_Pool.Release(toRelease);
		}
	}

	private static DynamicArray<SharedObjectPoolBase> s_AllocatedPools = new DynamicArray<SharedObjectPoolBase>();

	private Dictionary<(Type, int), Stack<object>> m_ArrayPool = new Dictionary<(Type, int), Stack<object>>();

	private List<(object, (Type, int))> m_AllocatedArrays = new List<(object, (Type, int))>();

	private List<MaterialPropertyBlock> m_AllocatedMaterialPropertyBlocks = new List<MaterialPropertyBlock>();

	internal RenderGraphObjectPool()
	{
	}

	public T[] GetTempArray<T>(int size)
	{
		if (!m_ArrayPool.TryGetValue((typeof(T), size), out var value))
		{
			value = new Stack<object>();
			m_ArrayPool.Add((typeof(T), size), value);
		}
		T[] array = ((value.Count > 0) ? ((T[])value.Pop()) : new T[size]);
		m_AllocatedArrays.Add((array, (typeof(T), size)));
		return array;
	}

	public MaterialPropertyBlock GetTempMaterialPropertyBlock()
	{
		MaterialPropertyBlock materialPropertyBlock = SharedObjectPool<MaterialPropertyBlock>.Get();
		materialPropertyBlock.Clear();
		m_AllocatedMaterialPropertyBlocks.Add(materialPropertyBlock);
		return materialPropertyBlock;
	}

	internal void ReleaseAllTempAlloc()
	{
		foreach (var allocatedArray in m_AllocatedArrays)
		{
			m_ArrayPool.TryGetValue(allocatedArray.Item2, out var value);
			value.Push(allocatedArray.Item1);
		}
		m_AllocatedArrays.Clear();
		foreach (MaterialPropertyBlock allocatedMaterialPropertyBlock in m_AllocatedMaterialPropertyBlocks)
		{
			SharedObjectPool<MaterialPropertyBlock>.Release(allocatedMaterialPropertyBlock);
		}
		m_AllocatedMaterialPropertyBlocks.Clear();
	}

	internal bool IsEmpty()
	{
		if (m_AllocatedArrays.Count == 0)
		{
			return m_AllocatedMaterialPropertyBlocks.Count == 0;
		}
		return false;
	}

	internal T Get<T>() where T : class, new()
	{
		return SharedObjectPool<T>.Get();
	}

	internal void Release<T>(T value) where T : class, new()
	{
		SharedObjectPool<T>.Release(value);
	}

	internal void Cleanup()
	{
		m_AllocatedArrays.Clear();
		m_AllocatedMaterialPropertyBlocks.Clear();
		m_ArrayPool.Clear();
		DynamicArray<SharedObjectPoolBase>.Iterator enumerator = s_AllocatedPools.GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.Clear();
		}
	}
}
