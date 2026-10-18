using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering.Universal;

internal static class NativeArrayExtensions
{
	public unsafe static ref T UnsafeElementAt<T>(this NativeArray<T> array, int index) where T : struct
	{
		return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafeReadOnlyPtr(), index);
	}

	public unsafe static ref T UnsafeElementAtMutable<T>(this NativeArray<T> array, int index) where T : struct
	{
		return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
	}
}
