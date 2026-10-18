using System;
using System.Diagnostics;
using Unity.Burst.LowLevel;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Unity.Burst;

public readonly struct SharedStatic<T> where T : struct
{
	private unsafe readonly void* _buffer;

	private const uint DefaultAlignment = 16u;

	public unsafe ref T Data => ref Unsafe.AsRef<T>(_buffer);

	public unsafe void* UnsafeDataPointer => _buffer;

	private unsafe SharedStatic(void* buffer)
	{
		_buffer = buffer;
	}

	public static SharedStatic<T> GetOrCreate<TContext>(uint alignment = 0u)
	{
		return GetOrCreateUnsafe(alignment, BurstRuntime.GetHashCode64<TContext>(), 0L);
	}

	public static SharedStatic<T> GetOrCreate<TContext, TSubContext>(uint alignment = 0u)
	{
		return GetOrCreateUnsafe(alignment, BurstRuntime.GetHashCode64<TContext>(), BurstRuntime.GetHashCode64<TSubContext>());
	}

	public unsafe static SharedStatic<T> GetOrCreateUnsafe(uint alignment, long hashCode, long subHashCode)
	{
		return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(hashCode, subHashCode, (uint)UnsafeUtility.SizeOf<T>(), (alignment == 0) ? 16u : alignment));
	}

	public unsafe static SharedStatic<T> GetOrCreatePartiallyUnsafeWithHashCode<TSubContext>(uint alignment, long hashCode)
	{
		return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(hashCode, BurstRuntime.GetHashCode64<TSubContext>(), (uint)UnsafeUtility.SizeOf<T>(), (alignment == 0) ? 16u : alignment));
	}

	public unsafe static SharedStatic<T> GetOrCreatePartiallyUnsafeWithSubHashCode<TContext>(uint alignment, long subHashCode)
	{
		return new SharedStatic<T>(SharedStatic.GetOrCreateSharedStaticInternal(BurstRuntime.GetHashCode64<TContext>(), subHashCode, (uint)UnsafeUtility.SizeOf<T>(), (alignment == 0) ? 16u : alignment));
	}

	public static SharedStatic<T> GetOrCreate(Type contextType, uint alignment = 0u)
	{
		return GetOrCreateUnsafe(alignment, BurstRuntime.GetHashCode64(contextType), 0L);
	}

	public static SharedStatic<T> GetOrCreate(Type contextType, Type subContextType, uint alignment = 0u)
	{
		return GetOrCreateUnsafe(alignment, BurstRuntime.GetHashCode64(contextType), BurstRuntime.GetHashCode64(subContextType));
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckIf_T_IsUnmanagedOrThrow()
	{
		if (!UnsafeUtility.IsUnmanaged<T>())
		{
			throw new InvalidOperationException($"The type {typeof(T)} used in SharedStatic<{typeof(T)}> must be unmanaged (contain no managed types).");
		}
	}
}
internal static class SharedStatic
{
	internal class PreserveAttribute : Attribute
	{
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private static void CheckSizeOf(uint sizeOf)
	{
		if (sizeOf == 0)
		{
			throw new ArgumentException("sizeOf must be > 0", "sizeOf");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private unsafe static void CheckResult(void* result)
	{
		if (result == null)
		{
			throw new InvalidOperationException("Unable to create a SharedStatic for this key. This is most likely due to the size of the struct inside of the SharedStatic having changed or the same key being reused for differently sized values. To fix this the editor needs to be restarted.");
		}
	}

	[Preserve]
	public unsafe static void* GetOrCreateSharedStaticInternal(long getHashCode64, long getSubHashCode64, uint sizeOf, uint alignment)
	{
		Hash128 key = new Hash128((ulong)getHashCode64, (ulong)getSubHashCode64);
		return BurstCompilerService.GetOrCreateSharedMemory(ref key, sizeOf, alignment);
	}
}
