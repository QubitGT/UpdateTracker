using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering.Universal;

internal struct PinnedArray<T> : IDisposable where T : struct
{
	public T[] managedArray;

	public GCHandle handle;

	public NativeArray<T> nativeArray;

	public int length
	{
		get
		{
			if (managedArray == null)
			{
				return 0;
			}
			return managedArray.Length;
		}
	}

	public unsafe PinnedArray(int length)
	{
		managedArray = new T[length];
		handle = GCHandle.Alloc(managedArray, GCHandleType.Pinned);
		nativeArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>((void*)handle.AddrOfPinnedObject(), length, Allocator.None);
	}

	public void Dispose()
	{
		if (managedArray != null)
		{
			handle.Free();
			this = default(PinnedArray<T>);
		}
	}
}
