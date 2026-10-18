using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Unity.Collections;

[BurstCompile]
public struct RewindableAllocator : AllocatorManager.IAllocator, IDisposable
{
	internal struct Union
	{
		internal long m_long;

		private const int currentBits = 40;

		private const int currentOffset = 0;

		private const long currentMask = 1099511627775L;

		private const int allocCountBits = 24;

		private const int allocCountOffset = 40;

		private const long allocCountMask = 16777215L;

		internal long m_current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return m_long & 0xFFFFFFFFFFL;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				m_long &= -1099511627776L;
				m_long |= value & 0xFFFFFFFFFFL;
			}
		}

		internal long m_allocCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				return (m_long >> 40) & 0xFFFFFF;
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				m_long &= 1099511627775L;
				m_long |= (value & 0xFFFFFF) << 40;
			}
		}
	}

	[GenerateTestsForBurstCompatibility]
	internal struct MemoryBlock : IDisposable
	{
		public const int kMaximumAlignment = 16384;

		public unsafe byte* m_pointer;

		public long m_bytes;

		public Union m_union;

		public unsafe MemoryBlock(long bytes)
		{
			m_pointer = (byte*)Memory.Unmanaged.Allocate(bytes, 16384, Allocator.Persistent);
			m_bytes = bytes;
			m_union = default(Union);
		}

		public void Rewind()
		{
			m_union = default(Union);
		}

		public unsafe void Dispose()
		{
			Memory.Unmanaged.Free(m_pointer, Allocator.Persistent);
			m_pointer = null;
			m_bytes = 0L;
			m_union = default(Union);
		}

		public unsafe bool Contains(IntPtr ptr)
		{
			void* ptr2 = (void*)ptr;
			if (ptr2 >= m_pointer)
			{
				return ptr2 < m_pointer + m_union.m_current;
			}
			return false;
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int Try_000009E0_0024PostfixBurstDelegate(IntPtr state, ref AllocatorManager.Block block);

	internal static class Try_000009E0_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<Try_000009E0_0024PostfixBurstDelegate>(Try).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static int Invoke(IntPtr state, ref AllocatorManager.Block block)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					return ((delegate* unmanaged[Cdecl]<IntPtr, ref AllocatorManager.Block, int>)functionPointer)(state, ref block);
				}
			}
			return Try_0024BurstManaged(state, ref block);
		}
	}

	private const int kLog2MaxMemoryBlockSize = 26;

	private const long kMaxMemoryBlockSize = 67108864L;

	private const long kMinMemoryBlockSize = 131072L;

	private const int kMaxNumBlocks = 64;

	private const int kBlockBusyRewindMask = int.MinValue;

	private const int kBlockBusyAllocateMask = int.MaxValue;

	private Spinner m_spinner;

	private AllocatorManager.AllocatorHandle m_handle;

	private UnmanagedArray<MemoryBlock> m_block;

	private int m_last;

	private int m_used;

	private byte m_enableBlockFree;

	private byte m_reachMaxBlockSize;

	public bool EnableBlockFree
	{
		get
		{
			return m_enableBlockFree != 0;
		}
		set
		{
			m_enableBlockFree = (byte)(value ? 1 : 0);
		}
	}

	public int BlocksAllocated => m_last + 1;

	public int InitialSizeInBytes => (int)m_block[0].m_bytes;

	internal long MaxMemoryBlockSize => 67108864L;

	internal long BytesAllocated
	{
		get
		{
			long num = 0L;
			for (int i = 0; i <= m_last; i++)
			{
				num += m_block[i].m_bytes;
			}
			return num;
		}
	}

	[ExcludeFromBurstCompatTesting("Uses managed delegate")]
	public AllocatorManager.TryFunction Function => Try;

	public AllocatorManager.AllocatorHandle Handle
	{
		get
		{
			return m_handle;
		}
		set
		{
			m_handle = value;
		}
	}

	public Allocator ToAllocator => m_handle.ToAllocator;

	public bool IsCustomAllocator => m_handle.IsCustomAllocator;

	public bool IsAutoDispose => true;

	public void Initialize(int initialSizeInBytes, bool enableBlockFree = false)
	{
		m_spinner = default(Spinner);
		m_block = new UnmanagedArray<MemoryBlock>(64, Allocator.Persistent);
		long bytes = (((long)initialSizeInBytes > 131072L) ? initialSizeInBytes : 131072);
		m_block[0] = new MemoryBlock(bytes);
		m_last = (m_used = 0);
		m_enableBlockFree = (byte)(enableBlockFree ? 1 : 0);
		m_reachMaxBlockSize = (byte)(((long)initialSizeInBytes >= 67108864L) ? 1 : 0);
	}

	public void Rewind()
	{
		if (JobsUtility.IsExecutingJob)
		{
			throw new InvalidOperationException("You cannot Rewind a RewindableAllocator from a Job.");
		}
		m_handle.Rewind();
		while (m_last > m_used)
		{
			m_block[m_last--].Dispose();
		}
		while (m_used > 0)
		{
			m_block[m_used--].Rewind();
		}
		m_block[0].Rewind();
	}

	public void Dispose()
	{
		if (JobsUtility.IsExecutingJob)
		{
			throw new InvalidOperationException("You cannot Dispose a RewindableAllocator from a Job.");
		}
		m_used = 0;
		Rewind();
		m_block[0].Dispose();
		m_block.Dispose();
		m_last = (m_used = 0);
	}

	private unsafe int TryAllocate(ref AllocatorManager.Block block, int startIndex, int lastIndex, long alignedSize, long alignmentMask)
	{
		for (int i = startIndex; i <= lastIndex; i++)
		{
			Union union = default(Union);
			long num = 0L;
			bool flag = false;
			union.m_long = Interlocked.Read(ref m_block[i].m_union.m_long);
			Union union2;
			do
			{
				num = (union.m_current + alignmentMask) & ~alignmentMask;
				if (num + block.Bytes > m_block[i].m_bytes)
				{
					flag = true;
					break;
				}
				union2 = union;
				Union union3 = default(Union);
				union3.m_current = ((num + alignedSize > m_block[i].m_bytes) ? m_block[i].m_bytes : (num + alignedSize));
				union3.m_allocCount = union.m_allocCount + 1;
				union.m_long = Interlocked.CompareExchange(ref m_block[i].m_union.m_long, union3.m_long, union2.m_long);
			}
			while (union.m_long != union2.m_long);
			if (!flag)
			{
				block.Range.Pointer = (IntPtr)(m_block[i].m_pointer + num);
				block.AllocatedItems = block.Range.Items;
				Interlocked.MemoryBarrier();
				int num2 = m_used;
				int num3;
				int num4;
				do
				{
					num3 = num2;
					num4 = ((i > num3) ? i : num3);
					num2 = Interlocked.CompareExchange(ref m_used, num4, num3);
				}
				while (num4 != num3);
				return 0;
			}
		}
		return -1;
	}

	public int Try(ref AllocatorManager.Block block)
	{
		if (block.Range.Pointer == IntPtr.Zero)
		{
			int num = math.max(64, block.Alignment);
			int num2 = ((num != 64) ? 1 : 0);
			int num3 = 63;
			if (num2 == 1)
			{
				num = (num + num3) & ~num3;
			}
			long num4 = (long)num - 1L;
			long num5 = (block.Bytes + num2 * num + num4) & ~num4;
			int last = m_last;
			int num6 = TryAllocate(ref block, 0, m_last, num5, num4);
			if (num6 == 0)
			{
				return num6;
			}
			m_spinner.Acquire();
			num6 = TryAllocate(ref block, last, m_last, num5, num4);
			if (num6 == 0)
			{
				m_spinner.Release();
				return num6;
			}
			long x = ((m_reachMaxBlockSize != 0) ? (m_block[m_last].m_bytes + 67108864) : (m_block[m_last].m_bytes << 1));
			x = math.max(x, num5);
			m_reachMaxBlockSize = (byte)((x >= 67108864) ? 1 : 0);
			m_block[m_last + 1] = new MemoryBlock(x);
			Interlocked.Increment(ref m_last);
			num6 = TryAllocate(ref block, m_last, m_last, num5, num4);
			m_spinner.Release();
			return num6;
		}
		if (block.Range.Items == 0)
		{
			if (m_enableBlockFree != 0)
			{
				for (int i = 0; i <= m_last; i++)
				{
					if (!m_block[i].Contains(block.Range.Pointer))
					{
						continue;
					}
					Union union = default(Union);
					union.m_long = Interlocked.Read(ref m_block[i].m_union.m_long);
					Union union2;
					do
					{
						union2 = union;
						Union union3 = union;
						union3.m_allocCount--;
						if (union3.m_allocCount == 0L)
						{
							union3.m_current = 0L;
						}
						union.m_long = Interlocked.CompareExchange(ref m_block[i].m_union.m_long, union3.m_long, union2.m_long);
					}
					while (union.m_long != union2.m_long);
				}
			}
			return 0;
		}
		return -1;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(AllocatorManager.TryFunction))]
	internal static int Try(IntPtr state, ref AllocatorManager.Block block)
	{
		return Try_000009E0_0024BurstDirectCall.Invoke(state, ref block);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe NativeArray<T> AllocateNativeArray<T>(int length) where T : unmanaged
	{
		NativeArray<T> result = default(NativeArray<T>);
		result.m_Buffer = AllocatorManager.AllocateStruct(ref this, default(T), length);
		result.m_Length = length;
		result.m_AllocatorLabel = Allocator.None;
		return result;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe NativeList<T> AllocateNativeList<T>(int capacity) where T : unmanaged
	{
		NativeList<T> result = default(NativeList<T>);
		result.m_ListData = AllocatorManager.Allocate(ref this, default(UnsafeList<T>), 1);
		result.m_ListData->Ptr = AllocatorManager.Allocate(ref this, default(T), capacity);
		result.m_ListData->m_length = 0;
		result.m_ListData->m_capacity = capacity;
		result.m_ListData->Allocator = Allocator.None;
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	[MonoPInvokeCallback(typeof(AllocatorManager.TryFunction))]
	internal unsafe static int Try_0024BurstManaged(IntPtr state, ref AllocatorManager.Block block)
	{
		return ((RewindableAllocator*)(void*)state)->Try(ref block);
	}
}
