using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public struct UnsafeAppendBuffer : INativeDisposable, IDisposable
{
	[GenerateTestsForBurstCompatibility]
	public struct Reader
	{
		public unsafe readonly byte* Ptr;

		public readonly int Size;

		public int Offset;

		public bool EndOfBuffer => Offset == Size;

		public unsafe Reader(ref UnsafeAppendBuffer buffer)
		{
			Ptr = buffer.Ptr;
			Size = buffer.Length;
			Offset = 0;
		}

		public unsafe Reader(void* ptr, int length)
		{
			Ptr = (byte*)ptr;
			Size = length;
			Offset = 0;
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe void ReadNext<T>(out T value) where T : unmanaged
		{
			int num = UnsafeUtility.SizeOf<T>();
			void* ptr = Ptr + Offset;
			if (CollectionHelper.IsAligned((ulong)ptr, UnsafeUtility.AlignOf<T>()))
			{
				UnsafeUtility.CopyPtrToStructure<T>(ptr, out value);
			}
			else
			{
				fixed (T* ptr2 = &value)
				{
					void* destination = ptr2;
					UnsafeUtility.MemCpy(destination, ptr, num);
				}
			}
			Offset += num;
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe T ReadNext<T>() where T : unmanaged
		{
			int num = UnsafeUtility.SizeOf<T>();
			void* ptr = Ptr + Offset;
			T result = default(T);
			if (CollectionHelper.IsAligned((ulong)ptr, UnsafeUtility.AlignOf<T>()))
			{
				result = UnsafeUtility.ReadArrayElement<T>(ptr, 0);
			}
			else
			{
				UnsafeUtility.MemCpy(&result, ptr, num);
			}
			Offset += num;
			return result;
		}

		public unsafe void* ReadNext(int structSize)
		{
			void* result = (void*)((IntPtr)Ptr + Offset);
			Offset += structSize;
			return result;
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe void ReadNext<T>(out NativeArray<T> value, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
		{
			int num = ReadNext<int>();
			value = CollectionHelper.CreateNativeArray<T>(num, allocator, NativeArrayOptions.UninitializedMemory);
			int num2 = num * UnsafeUtility.SizeOf<T>();
			if (num2 > 0)
			{
				void* source = ReadNext(num2);
				UnsafeUtility.MemCpy(value.GetUnsafePtr(), source, num2);
			}
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe void* ReadNextArray<T>(out int length) where T : unmanaged
		{
			length = ReadNext<int>();
			if (length != 0)
			{
				return ReadNext(length * UnsafeUtility.SizeOf<T>());
			}
			return null;
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckBounds(int structSize)
		{
			if (Offset + structSize > Size)
			{
				throw new ArgumentException($"Requested value outside bounds of UnsafeAppendOnlyBuffer. Remaining bytes: {Size - Offset} Requested: {structSize}");
			}
		}
	}

	[NativeDisableUnsafePtrRestriction]
	public unsafe byte* Ptr;

	public int Length;

	public int Capacity;

	public AllocatorManager.AllocatorHandle Allocator;

	public readonly int Alignment;

	public readonly bool IsEmpty
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Length == 0;
		}
	}

	public unsafe readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return Ptr != null;
		}
	}

	public unsafe UnsafeAppendBuffer(int initialCapacity, int alignment, AllocatorManager.AllocatorHandle allocator)
	{
		Alignment = alignment;
		Allocator = allocator;
		Ptr = null;
		Length = 0;
		Capacity = 0;
		SetCapacity(math.max(initialCapacity, 1));
	}

	public unsafe UnsafeAppendBuffer(void* ptr, int length)
	{
		Alignment = 0;
		Allocator = AllocatorManager.None;
		Ptr = (byte*)ptr;
		Length = 0;
		Capacity = length;
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			if (CollectionHelper.ShouldDeallocate(Allocator))
			{
				Memory.Unmanaged.Free(Ptr, Allocator);
				Allocator = AllocatorManager.Invalid;
			}
			Ptr = null;
			Length = 0;
			Capacity = 0;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		if (CollectionHelper.ShouldDeallocate(Allocator))
		{
			UnsafeDisposeJob jobData = default(UnsafeDisposeJob);
			jobData.Ptr = Ptr;
			jobData.Allocator = Allocator;
			JobHandle result = jobData.Schedule(inputDeps);
			Ptr = null;
			Allocator = AllocatorManager.Invalid;
			return result;
		}
		Ptr = null;
		return inputDeps;
	}

	public void Reset()
	{
		Length = 0;
	}

	public unsafe void SetCapacity(int capacity)
	{
		if (capacity > Capacity)
		{
			capacity = math.max(64, math.ceilpow2(capacity));
			byte* ptr = (byte*)Memory.Unmanaged.Allocate(capacity, Alignment, Allocator);
			if (Ptr != null)
			{
				UnsafeUtility.MemCpy(ptr, Ptr, Length);
				Memory.Unmanaged.Free(Ptr, Allocator);
			}
			Ptr = ptr;
			Capacity = capacity;
		}
	}

	public void ResizeUninitialized(int length)
	{
		SetCapacity(length);
		Length = length;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe void Add<T>(T value) where T : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>();
		SetCapacity(Length + num);
		void* ptr = Ptr + Length;
		if (CollectionHelper.IsAligned((ulong)ptr, UnsafeUtility.AlignOf<T>()))
		{
			UnsafeUtility.CopyStructureToPtr(ref value, ptr);
		}
		else
		{
			UnsafeUtility.MemCpy(ptr, &value, num);
		}
		Length += num;
	}

	public unsafe void Add(void* ptr, int structSize)
	{
		SetCapacity(Length + structSize);
		UnsafeUtility.MemCpy(Ptr + Length, ptr, structSize);
		Length += structSize;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe void AddArray<T>(void* ptr, int length) where T : unmanaged
	{
		Add(length);
		if (length != 0)
		{
			Add(ptr, length * UnsafeUtility.SizeOf<T>());
		}
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe void Add<T>(NativeArray<T> value) where T : unmanaged
	{
		Add(value.Length);
		Add(value.GetUnsafeReadOnlyPtr(), UnsafeUtility.SizeOf<T>() * value.Length);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe T Pop<T>() where T : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>();
		long num2 = (long)Ptr;
		long num3 = Length;
		long num4 = num2 + num3 - num;
		T result = default(T);
		if (CollectionHelper.IsAligned((ulong)num4, UnsafeUtility.AlignOf<T>()))
		{
			result = UnsafeUtility.ReadArrayElement<T>((void*)num4, 0);
		}
		else
		{
			UnsafeUtility.MemCpy(&result, (void*)num4, num);
		}
		Length -= num;
		return result;
	}

	public unsafe void Pop(void* ptr, int structSize)
	{
		long num = (long)Ptr;
		long num2 = Length;
		long num3 = num + num2 - structSize;
		UnsafeUtility.MemCpy(ptr, (void*)num3, structSize);
		Length -= structSize;
	}

	public Reader AsReader()
	{
		return new Reader(ref this);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckAlignment(int alignment)
	{
		bool num = alignment == 0;
		bool flag = ((alignment - 1) & alignment) == 0;
		if (!(!num && flag))
		{
			throw new ArgumentException($"Specified alignment must be non-zero positive power of two. Requested: {alignment}");
		}
	}
}
