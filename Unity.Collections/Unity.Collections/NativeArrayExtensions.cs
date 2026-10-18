using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

namespace Unity.Collections;

[GenerateTestsForBurstCompatibility]
public static class NativeArrayExtensions
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct NativeArrayStaticId<T> where T : unmanaged
	{
		internal static readonly SharedStatic<int> s_staticSafetyId = SharedStatic<int>.GetOrCreate<NativeArray<T>>();
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static bool Contains<T, U>(this NativeArray<T> array, U value) where T : unmanaged, IEquatable<U>
	{
		return IndexOf<T, U>(array.GetUnsafeReadOnlyPtr(), array.Length, value) != -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static int IndexOf<T, U>(this NativeArray<T> array, U value) where T : unmanaged, IEquatable<U>
	{
		return IndexOf<T, U>(array.GetUnsafeReadOnlyPtr(), array.Length, value);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static bool Contains<T, U>(this NativeArray<T>.ReadOnly array, U value) where T : unmanaged, IEquatable<U>
	{
		return IndexOf<T, U>(array.GetUnsafeReadOnlyPtr(), array.m_Length, value) != -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static int IndexOf<T, U>(this NativeArray<T>.ReadOnly array, U value) where T : unmanaged, IEquatable<U>
	{
		return IndexOf<T, U>(array.GetUnsafeReadOnlyPtr(), array.m_Length, value);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static bool Contains<T, U>(void* ptr, int length, U value) where T : unmanaged, IEquatable<U>
	{
		return IndexOf<T, U>(ptr, length, value) != -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static int IndexOf<T, U>(void* ptr, int length, U value) where T : unmanaged, IEquatable<U>
	{
		for (int i = 0; i != length; i++)
		{
			if (UnsafeUtility.ReadArrayElement<T>(ptr, i).Equals(value))
			{
				return i;
			}
		}
		return -1;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static void CopyFrom<T>(this ref NativeArray<T> container, NativeList<T> other) where T : unmanaged, IEquatable<T>
	{
		container.CopyFrom(other.AsArray());
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static void CopyFrom<T>(this ref NativeArray<T> container, in NativeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		using NativeArray<T> array = other.ToNativeArray(Allocator.TempJob);
		container.CopyFrom(array);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static void CopyFrom<T>(this ref NativeArray<T> container, in UnsafeHashSet<T> other) where T : unmanaged, IEquatable<T>
	{
		using NativeArray<T> array = other.ToNativeArray(Allocator.TempJob);
		container.CopyFrom(array);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(int)
	})]
	public unsafe static NativeArray<U> Reinterpret<T, U>(this NativeArray<T> array) where T : unmanaged where U : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>();
		int num2 = UnsafeUtility.SizeOf<U>();
		long num3 = (long)array.Length * (long)num / num2;
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<U>(NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), (int)num3, Allocator.None);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public static bool ArraysEqual<T>(this NativeArray<T> container, NativeArray<T> other) where T : unmanaged, IEquatable<T>
	{
		if (container.Length != other.Length)
		{
			return false;
		}
		for (int i = 0; i != container.Length; i++)
		{
			if (!container[i].Equals(other[i]))
			{
				return false;
			}
		}
		return true;
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckReinterpretSize<T, U>(ref NativeArray<T> array) where T : unmanaged where U : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>();
		int num2 = UnsafeUtility.SizeOf<U>();
		long num3 = (long)array.Length * (long)num;
		if (num3 / num2 * num2 != num3)
		{
			throw new InvalidOperationException($"Types {typeof(T)} (array length {array.Length}) and {typeof(U)} cannot be aliased due to size constraints. The size of the types and lengths involved must line up.");
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal unsafe static void Initialize<T>(this ref NativeArray<T> array, int length, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : unmanaged
	{
		AllocatorManager.AllocatorHandle t = allocator;
		array = default(NativeArray<T>);
		array.m_Buffer = AllocatorManager.AllocateStruct(ref t, default(T), length);
		array.m_Length = length;
		array.m_AllocatorLabel = (allocator.IsAutoDispose ? Allocator.None : allocator.ToAllocator);
		if (options == NativeArrayOptions.ClearMemory)
		{
			UnsafeUtility.MemClear(array.m_Buffer, array.m_Length * UnsafeUtility.SizeOf<T>());
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[]
	{
		typeof(int),
		typeof(AllocatorManager.AllocatorHandle)
	})]
	internal unsafe static void Initialize<T, U>(this ref NativeArray<T> array, int length, ref U allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory) where T : unmanaged where U : unmanaged, AllocatorManager.IAllocator
	{
		array = default(NativeArray<T>);
		array.m_Buffer = AllocatorManager.AllocateStruct(ref allocator, default(T), length);
		array.m_Length = length;
		array.m_AllocatorLabel = (allocator.IsAutoDispose ? Allocator.None : allocator.ToAllocator);
		if (options == NativeArrayOptions.ClearMemory)
		{
			UnsafeUtility.MemClear(array.m_Buffer, array.m_Length * UnsafeUtility.SizeOf<T>());
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	internal unsafe static void DisposeCheckAllocator<T>(this ref NativeArray<T> array) where T : unmanaged
	{
		if (array.m_Buffer == null)
		{
			throw new ObjectDisposedException("The NativeArray is already disposed.");
		}
		if (!AllocatorManager.IsCustomAllocator(array.m_AllocatorLabel))
		{
			array.Dispose();
			return;
		}
		AllocatorManager.Free(array.m_AllocatorLabel, array.m_Buffer);
		array.m_AllocatorLabel = Allocator.Invalid;
		array.m_Buffer = null;
	}
}
