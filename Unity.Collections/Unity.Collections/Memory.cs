using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[GenerateTestsForBurstCompatibility]
internal struct Memory
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[GenerateTestsForBurstCompatibility]
	internal struct Unmanaged
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		[GenerateTestsForBurstCompatibility]
		internal struct Array
		{
			private static bool IsCustom(AllocatorManager.AllocatorHandle allocator)
			{
				return allocator.Index >= 64;
			}

			private unsafe static void* CustomResize(void* oldPointer, long oldCount, long newCount, AllocatorManager.AllocatorHandle allocator, long size, int align)
			{
				AllocatorManager.Block block = default(AllocatorManager.Block);
				block.Range.Allocator = allocator;
				block.Range.Items = (int)newCount;
				block.Range.Pointer = (IntPtr)oldPointer;
				block.BytesPerItem = (int)size;
				block.Alignment = align;
				block.AllocatedItems = (int)oldCount;
				AllocatorManager.Try(ref block);
				return (void*)block.Range.Pointer;
			}

			internal unsafe static void* Resize(void* oldPointer, long oldCount, long newCount, AllocatorManager.AllocatorHandle allocator, long size, int align)
			{
				int num = math.max(64, align);
				if (IsCustom(allocator))
				{
					return CustomResize(oldPointer, oldCount, newCount, allocator, size, num);
				}
				void* ptr = default(void*);
				if (newCount > 0)
				{
					ptr = UnsafeUtility.MallocTracked(newCount * size, num, allocator.ToAllocator, 0);
					if (oldCount > 0)
					{
						long size2 = math.min(oldCount, newCount) * size;
						UnsafeUtility.MemCpy(ptr, oldPointer, size2);
					}
				}
				if (oldCount > 0)
				{
					UnsafeUtility.FreeTracked(oldPointer, allocator.ToAllocator);
				}
				return ptr;
			}

			[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
			internal unsafe static T* Resize<T>(T* oldPointer, long oldCount, long newCount, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
			{
				return (T*)Resize(oldPointer, oldCount, newCount, allocator, UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>());
			}

			[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
			internal unsafe static T* Allocate<T>(long count, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
			{
				return Resize<T>(null, 0L, count, allocator);
			}

			[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
			internal unsafe static void Free<T>(T* pointer, long count, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
			{
				if (pointer != null)
				{
					Resize(pointer, count, 0L, allocator);
				}
			}
		}

		internal unsafe static void* Allocate(long size, int align, AllocatorManager.AllocatorHandle allocator)
		{
			return Array.Resize(null, 0L, 1L, allocator, size, align);
		}

		internal unsafe static void Free(void* pointer, AllocatorManager.AllocatorHandle allocator)
		{
			if (pointer != null)
			{
				Array.Resize(pointer, 1L, 0L, allocator, 1L, 1);
			}
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		internal unsafe static T* Allocate<T>(AllocatorManager.AllocatorHandle allocator) where T : unmanaged
		{
			return Array.Resize<T>(null, 0L, 1L, allocator);
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		internal unsafe static void Free<T>(T* pointer, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
		{
			if (pointer != null)
			{
				Array.Resize(pointer, 1L, 0L, allocator);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[GenerateTestsForBurstCompatibility]
	internal struct Array
	{
		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		internal unsafe static void Set<T>(T* pointer, long count, T t = default(T)) where T : unmanaged
		{
			UnsafeUtility.SizeOf<T>();
			for (int i = 0; i < count; i++)
			{
				pointer[i] = t;
			}
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		internal unsafe static void Clear<T>(T* pointer, long count) where T : unmanaged
		{
			long size = count * UnsafeUtility.SizeOf<T>();
			UnsafeUtility.MemClear(pointer, size);
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		internal unsafe static void Copy<T>(T* dest, T* src, long count) where T : unmanaged
		{
			long size = count * UnsafeUtility.SizeOf<T>();
			UnsafeUtility.MemCpy(dest, src, size);
		}
	}

	internal const long k_MaximumRamSizeInBytes = 1099511627776L;

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	internal static void CheckByteCountIsReasonable(long size)
	{
		if (size < 0)
		{
			throw new InvalidOperationException($"Attempted to operate on {size} bytes of memory: negative size");
		}
		if (size > 1099511627776L)
		{
			throw new InvalidOperationException($"Attempted to operate on {size} bytes of memory: size too big");
		}
	}
}
