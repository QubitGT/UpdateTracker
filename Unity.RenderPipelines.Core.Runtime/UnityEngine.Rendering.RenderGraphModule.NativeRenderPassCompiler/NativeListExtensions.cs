using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

internal static class NativeListExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal unsafe static ReadOnlySpan<T> MakeReadOnlySpan<T>(this ref NativeList<T> list, int first, int numElements) where T : unmanaged
	{
		return new ReadOnlySpan<T>(list.GetUnsafeReadOnlyPtr() + first, numElements);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int LastIndex<T>(this ref NativeList<T> list) where T : unmanaged
	{
		return list.Length - 1;
	}
}
