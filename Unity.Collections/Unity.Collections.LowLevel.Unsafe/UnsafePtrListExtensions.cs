using System;
using System.Runtime.CompilerServices;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
internal static class UnsafePtrListExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static ref UnsafeList<IntPtr> ListData<T>(this ref UnsafePtrList<T> from) where T : unmanaged
	{
		return ref UnsafeUtility.As<UnsafePtrList<T>, UnsafeList<IntPtr>>(ref from);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static UnsafeList<IntPtr> ListDataRO<T>(this UnsafePtrList<T> from) where T : unmanaged
	{
		return UnsafeUtility.As<UnsafePtrList<T>, UnsafeList<IntPtr>>(ref from);
	}
}
