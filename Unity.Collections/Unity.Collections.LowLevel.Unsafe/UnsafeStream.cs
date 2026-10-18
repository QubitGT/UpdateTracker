using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Collections.LowLevel.Unsafe;

[GenerateTestsForBurstCompatibility]
public struct UnsafeStream : INativeDisposable, IDisposable
{
	[BurstCompile]
	private struct DisposeJob : IJob
	{
		public UnsafeStream Container;

		public void Execute()
		{
			Container.Deallocate();
		}
	}

	[BurstCompile]
	private struct ConstructJobList : IJob
	{
		public UnsafeStream Container;

		[ReadOnly]
		[NativeDisableUnsafePtrRestriction]
		public unsafe UntypedUnsafeList* List;

		public unsafe void Execute()
		{
			Container.AllocateForEach(List->m_length);
		}
	}

	[BurstCompile]
	private struct ConstructJob : IJob
	{
		public UnsafeStream Container;

		[ReadOnly]
		public NativeArray<int> Length;

		public void Execute()
		{
			Container.AllocateForEach(Length[0]);
		}
	}

	[GenerateTestsForBurstCompatibility]
	public struct Writer
	{
		[NativeDisableUnsafePtrRestriction]
		internal AllocatorManager.Block m_BlockData;

		[NativeDisableUnsafePtrRestriction]
		private unsafe UnsafeStreamBlock* m_CurrentBlock;

		[NativeDisableUnsafePtrRestriction]
		private unsafe byte* m_CurrentPtr;

		[NativeDisableUnsafePtrRestriction]
		private unsafe byte* m_CurrentBlockEnd;

		internal int m_ForeachIndex;

		private int m_ElementCount;

		[NativeDisableUnsafePtrRestriction]
		private unsafe UnsafeStreamBlock* m_FirstBlock;

		private int m_FirstOffset;

		private int m_NumberOfBlocks;

		[NativeSetThreadIndex]
		private int m_ThreadIndex;

		public unsafe int ForEachCount => ((UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer)->RangeCount;

		internal unsafe Writer(ref UnsafeStream stream)
		{
			m_BlockData = stream.m_BlockData;
			m_ForeachIndex = int.MinValue;
			m_ElementCount = -1;
			m_CurrentBlock = null;
			m_CurrentBlockEnd = null;
			m_CurrentPtr = null;
			m_FirstBlock = null;
			m_NumberOfBlocks = 0;
			m_FirstOffset = 0;
			m_ThreadIndex = 0;
		}

		public unsafe void BeginForEachIndex(int foreachIndex)
		{
			m_ForeachIndex = foreachIndex;
			m_ElementCount = 0;
			m_NumberOfBlocks = 0;
			m_FirstBlock = m_CurrentBlock;
			m_FirstOffset = (int)(m_CurrentPtr - (byte*)m_CurrentBlock);
		}

		public unsafe void EndForEachIndex()
		{
			UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer;
			UnsafeStreamRange* ptr2 = (UnsafeStreamRange*)(void*)ptr->Ranges.Range.Pointer;
			ptr2[m_ForeachIndex].ElementCount = m_ElementCount;
			ptr2[m_ForeachIndex].OffsetInFirstBlock = m_FirstOffset;
			ptr2[m_ForeachIndex].Block = m_FirstBlock;
			ptr2[m_ForeachIndex].LastOffset = (int)(m_CurrentPtr - (byte*)m_CurrentBlock);
			ptr2[m_ForeachIndex].NumberOfBlocks = m_NumberOfBlocks;
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public void Write<T>(T value) where T : unmanaged
		{
			Allocate<T>() = value;
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe ref T Allocate<T>() where T : unmanaged
		{
			int size = UnsafeUtility.SizeOf<T>();
			return ref UnsafeUtility.AsRef<T>(Allocate(size));
		}

		public unsafe byte* Allocate(int size)
		{
			byte* currentPtr = m_CurrentPtr;
			m_CurrentPtr += size;
			if (m_CurrentPtr > m_CurrentBlockEnd)
			{
				UnsafeStreamBlock* currentBlock = m_CurrentBlock;
				UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer;
				m_CurrentBlock = ptr->Allocate(currentBlock, m_ThreadIndex);
				m_CurrentPtr = m_CurrentBlock->Data;
				if (m_FirstBlock == null)
				{
					m_FirstOffset = (int)(m_CurrentPtr - (byte*)m_CurrentBlock);
					m_FirstBlock = m_CurrentBlock;
				}
				else
				{
					m_NumberOfBlocks++;
				}
				m_CurrentBlockEnd = (byte*)m_CurrentBlock + 4096;
				currentPtr = m_CurrentPtr;
				m_CurrentPtr += size;
			}
			m_ElementCount++;
			return currentPtr;
		}
	}

	[GenerateTestsForBurstCompatibility]
	public struct Reader
	{
		[NativeDisableUnsafePtrRestriction]
		internal AllocatorManager.Block m_BlockData;

		[NativeDisableUnsafePtrRestriction]
		internal unsafe UnsafeStreamBlock* m_CurrentBlock;

		[NativeDisableUnsafePtrRestriction]
		internal unsafe byte* m_CurrentPtr;

		[NativeDisableUnsafePtrRestriction]
		internal unsafe byte* m_CurrentBlockEnd;

		internal int m_RemainingItemCount;

		internal int m_LastBlockSize;

		public unsafe int ForEachCount => ((UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer)->RangeCount;

		public int RemainingItemCount => m_RemainingItemCount;

		internal unsafe Reader(ref UnsafeStream stream)
		{
			m_BlockData = stream.m_BlockData;
			m_CurrentBlock = null;
			m_CurrentPtr = null;
			m_CurrentBlockEnd = null;
			m_RemainingItemCount = 0;
			m_LastBlockSize = 0;
		}

		public unsafe int BeginForEachIndex(int foreachIndex)
		{
			UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer;
			UnsafeStreamRange* ptr2 = (UnsafeStreamRange*)(void*)ptr->Ranges.Range.Pointer;
			m_RemainingItemCount = ptr2[foreachIndex].ElementCount;
			m_LastBlockSize = ptr2[foreachIndex].LastOffset;
			m_CurrentBlock = ptr2[foreachIndex].Block;
			m_CurrentPtr = (byte*)m_CurrentBlock + ptr2[foreachIndex].OffsetInFirstBlock;
			m_CurrentBlockEnd = (byte*)m_CurrentBlock + 4096;
			return m_RemainingItemCount;
		}

		public void EndForEachIndex()
		{
		}

		public unsafe byte* ReadUnsafePtr(int size)
		{
			m_RemainingItemCount--;
			byte* currentPtr = m_CurrentPtr;
			m_CurrentPtr += size;
			if (m_CurrentPtr > m_CurrentBlockEnd)
			{
				m_CurrentBlock = m_CurrentBlock->Next;
				m_CurrentPtr = m_CurrentBlock->Data;
				m_CurrentBlockEnd = (byte*)m_CurrentBlock + 4096;
				currentPtr = m_CurrentPtr;
				m_CurrentPtr += size;
			}
			return currentPtr;
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe ref T Read<T>() where T : unmanaged
		{
			int size = UnsafeUtility.SizeOf<T>();
			return ref UnsafeUtility.AsRef<T>(ReadUnsafePtr(size));
		}

		[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
		public unsafe ref T Peek<T>() where T : unmanaged
		{
			int num = UnsafeUtility.SizeOf<T>();
			byte* ptr = m_CurrentPtr;
			if (ptr + num > m_CurrentBlockEnd)
			{
				ptr = m_CurrentBlock->Next->Data;
			}
			return ref UnsafeUtility.AsRef<T>(ptr);
		}

		public unsafe int Count()
		{
			UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer;
			UnsafeStreamRange* ptr2 = (UnsafeStreamRange*)(void*)ptr->Ranges.Range.Pointer;
			int num = 0;
			for (int i = 0; i != ptr->RangeCount; i++)
			{
				num += ptr2[i].ElementCount;
			}
			return num;
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal AllocatorManager.Block m_BlockData;

	public readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_BlockData.Range.Pointer != IntPtr.Zero;
		}
	}

	public unsafe readonly int ForEachCount => ((UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer)->RangeCount;

	public UnsafeStream(int bufferCount, AllocatorManager.AllocatorHandle allocator)
	{
		AllocateBlock(out this, allocator);
		AllocateForEach(bufferCount);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static JobHandle ScheduleConstruct<T>(out UnsafeStream stream, NativeList<T> bufferCount, JobHandle dependency, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		AllocateBlock(out stream, allocator);
		ConstructJobList jobData = default(ConstructJobList);
		jobData.List = (UntypedUnsafeList*)bufferCount.GetUnsafeList();
		jobData.Container = stream;
		return jobData.Schedule(dependency);
	}

	public static JobHandle ScheduleConstruct(out UnsafeStream stream, NativeArray<int> bufferCount, JobHandle dependency, AllocatorManager.AllocatorHandle allocator)
	{
		AllocateBlock(out stream, allocator);
		ConstructJob jobData = default(ConstructJob);
		jobData.Length = bufferCount;
		jobData.Container = stream;
		return jobData.Schedule(dependency);
	}

	internal unsafe static void AllocateBlock(out UnsafeStream stream, AllocatorManager.AllocatorHandle allocator)
	{
		int threadIndexCount = JobsUtility.ThreadIndexCount;
		int sizeOf = sizeof(UnsafeStreamBlockData) + sizeof(UnsafeStreamBlock*) * threadIndexCount;
		AllocatorManager.Block blockData = AllocatorManager.AllocateBlock(ref allocator, sizeOf, 16, 1);
		UnsafeUtility.MemClear((void*)blockData.Range.Pointer, blockData.AllocatedBytes);
		stream.m_BlockData = blockData;
		UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)blockData.Range.Pointer;
		ptr->Allocator = allocator;
		ptr->BlockCount = threadIndexCount;
		ptr->Blocks = (UnsafeStreamBlock**)(void*)(blockData.Range.Pointer + sizeof(UnsafeStreamBlockData));
		ptr->Ranges = default(AllocatorManager.Block);
		ptr->RangeCount = 0;
	}

	internal unsafe void AllocateForEach(int forEachCount)
	{
		UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer;
		ptr->Ranges = AllocatorManager.AllocateBlock(ref m_BlockData.Range.Allocator, sizeof(UnsafeStreamRange), 16, forEachCount);
		ptr->RangeCount = forEachCount;
		UnsafeUtility.MemClear((void*)ptr->Ranges.Range.Pointer, ptr->Ranges.AllocatedBytes);
	}

	public unsafe readonly bool IsEmpty()
	{
		if (!IsCreated)
		{
			return true;
		}
		UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer;
		UnsafeStreamRange* ptr2 = (UnsafeStreamRange*)(void*)ptr->Ranges.Range.Pointer;
		for (int i = 0; i != ptr->RangeCount; i++)
		{
			if (ptr2[i].ElementCount > 0)
			{
				return false;
			}
		}
		return true;
	}

	public Reader AsReader()
	{
		return new Reader(ref this);
	}

	public Writer AsWriter()
	{
		return new Writer(ref this);
	}

	public unsafe int Count()
	{
		int num = 0;
		UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer;
		UnsafeStreamRange* ptr2 = (UnsafeStreamRange*)(void*)ptr->Ranges.Range.Pointer;
		for (int i = 0; i != ptr->RangeCount; i++)
		{
			num += ptr2[i].ElementCount;
		}
		return num;
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public NativeArray<T> ToNativeArray<T>(AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		NativeArray<T> result = CollectionHelper.CreateNativeArray<T>(Count(), allocator, NativeArrayOptions.UninitializedMemory);
		Reader reader = AsReader();
		int num = 0;
		for (int i = 0; i != reader.ForEachCount; i++)
		{
			reader.BeginForEachIndex(i);
			int remainingItemCount = reader.RemainingItemCount;
			for (int j = 0; j < remainingItemCount; j++)
			{
				result[num] = reader.Read<T>();
				num++;
			}
			reader.EndForEachIndex();
		}
		return result;
	}

	private unsafe void Deallocate()
	{
		if (!IsCreated)
		{
			return;
		}
		UnsafeStreamBlockData* ptr = (UnsafeStreamBlockData*)(void*)m_BlockData.Range.Pointer;
		for (int i = 0; i != ptr->BlockCount; i++)
		{
			UnsafeStreamBlock* ptr2 = ptr->Blocks[i];
			while (ptr2 != null)
			{
				UnsafeStreamBlock* next = ptr2->Next;
				ptr->Free(ptr2);
				ptr2 = next;
			}
		}
		ptr->Ranges.Dispose();
		m_BlockData.Dispose();
		m_BlockData = default(AllocatorManager.Block);
	}

	public void Dispose()
	{
		if (IsCreated)
		{
			Deallocate();
		}
	}

	public JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		DisposeJob jobData = default(DisposeJob);
		jobData.Container = this;
		JobHandle result = jobData.Schedule(inputDeps);
		m_BlockData = default(AllocatorManager.Block);
		return result;
	}
}
