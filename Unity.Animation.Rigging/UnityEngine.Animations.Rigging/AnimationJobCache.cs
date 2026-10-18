using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Animations.Rigging;

public struct AnimationJobCache : IDisposable
{
	private NativeArray<float> m_Data;

	internal AnimationJobCache(float[] data)
	{
		m_Data = new NativeArray<float>(data, Allocator.Persistent);
	}

	public void Dispose()
	{
		m_Data.Dispose();
	}

	public float GetRaw(CacheIndex index, int offset = 0)
	{
		return m_Data[index.idx + offset];
	}

	public void SetRaw(float val, CacheIndex index, int offset = 0)
	{
		m_Data[index.idx + offset] = val;
	}

	public unsafe T Get<T>(CacheIndex index, int offset = 0) where T : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>();
		int num2 = num / UnsafeUtility.SizeOf<float>();
		T result = default(T);
		UnsafeUtility.MemCpy(&result, (byte*)m_Data.GetUnsafeReadOnlyPtr() + (nint)index.idx * (nint)4 + (nint)(offset * num2) * (nint)4, num);
		return result;
	}

	public unsafe void Set<T>(T val, CacheIndex index, int offset = 0) where T : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>();
		int num2 = num / UnsafeUtility.SizeOf<float>();
		UnsafeUtility.MemCpy((byte*)m_Data.GetUnsafePtr() + (nint)index.idx * (nint)4 + (nint)(offset * num2) * (nint)4, &val, num);
	}

	public unsafe void SetArray<T>(T[] v, CacheIndex index, int offset = 0) where T : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>();
		int num2 = num / UnsafeUtility.SizeOf<float>();
		fixed (T* ptr = v)
		{
			void* source = ptr;
			UnsafeUtility.MemCpy((byte*)m_Data.GetUnsafePtr() + (nint)index.idx * (nint)4 + (nint)(offset * num2) * (nint)4, source, num * v.Length);
		}
	}
}
