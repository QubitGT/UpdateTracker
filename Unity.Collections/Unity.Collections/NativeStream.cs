using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[GenerateTestsForBurstCompatibility]
public struct NativeStream : INativeDisposable, IDisposable
{
	[BurstCompile]
	private struct ConstructJobList : IJob
	{
		public NativeStream Container;

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
		public NativeStream Container;

		[ReadOnly]
		public NativeArray<int> Length;

		public void Execute()
		{
			Container.AllocateForEach(Length[0]);
		}
	}

	[NativeContainer]
	[NativeContainerSupportsMinMaxWriteRestriction]
	[GenerateTestsForBurstCompatibility]
	public struct Writer
	{
		private UnsafeStream.Writer m_Writer;

		public int ForEachCount => m_Writer.ForEachCount;

		internal Writer(ref NativeStream stream)
		{
			m_Writer = stream.m_Stream.AsWriter();
		}

		public void PatchMinMaxRange(int foreEachIndex)
		{
		}

		public void BeginForEachIndex(int foreachIndex)
		{
			m_Writer.BeginForEachIndex(foreachIndex);
		}

		public void EndForEachIndex()
		{
			m_Writer.EndForEachIndex();
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
			return m_Writer.Allocate(size);
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckBeginForEachIndex(int foreachIndex)
		{
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckEndForEachIndex()
		{
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckAllocateSize(int size)
		{
		}
	}

	[NativeContainer]
	[NativeContainerIsReadOnly]
	[GenerateTestsForBurstCompatibility]
	public struct Reader
	{
		private UnsafeStream.Reader m_Reader;

		public int ForEachCount => m_Reader.ForEachCount;

		public int RemainingItemCount => m_Reader.RemainingItemCount;

		internal Reader(ref NativeStream stream)
		{
			m_Reader = stream.m_Stream.AsReader();
		}

		public int BeginForEachIndex(int foreachIndex)
		{
			return m_Reader.BeginForEachIndex(foreachIndex);
		}

		public void EndForEachIndex()
		{
			m_Reader.EndForEachIndex();
		}

		public unsafe byte* ReadUnsafePtr(int size)
		{
			m_Reader.m_RemainingItemCount--;
			byte* currentPtr = m_Reader.m_CurrentPtr;
			m_Reader.m_CurrentPtr += size;
			if (m_Reader.m_CurrentPtr > m_Reader.m_CurrentBlockEnd)
			{
				m_Reader.m_CurrentBlock = m_Reader.m_CurrentBlock->Next;
				m_Reader.m_CurrentPtr = m_Reader.m_CurrentBlock->Data;
				m_Reader.m_CurrentBlockEnd = (byte*)m_Reader.m_CurrentBlock + 4096;
				currentPtr = m_Reader.m_CurrentPtr;
				m_Reader.m_CurrentPtr += size;
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
		public ref T Peek<T>() where T : unmanaged
		{
			UnsafeUtility.SizeOf<T>();
			return ref m_Reader.Peek<T>();
		}

		public int Count()
		{
			return m_Reader.Count();
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckNotReadingOutOfBounds(int size)
		{
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private void CheckRead()
		{
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckReadSize(int size)
		{
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private void CheckBeginForEachIndex(int forEachIndex)
		{
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		[Conditional("UNITY_DOTS_DEBUG")]
		private unsafe void CheckEndForEachIndex()
		{
			if (m_Reader.m_RemainingItemCount != 0)
			{
				throw new ArgumentException("Not all elements (Count) have been read. If this is intentional, simply skip calling EndForEachIndex();");
			}
			if (m_Reader.m_CurrentBlockEnd != m_Reader.m_CurrentPtr)
			{
				throw new ArgumentException("Not all data (Data Size) has been read. If this is intentional, simply skip calling EndForEachIndex();");
			}
		}
	}

	private UnsafeStream m_Stream;

	public readonly bool IsCreated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Stream.IsCreated;
		}
	}

	public readonly int ForEachCount => m_Stream.ForEachCount;

	public NativeStream(int bufferCount, AllocatorManager.AllocatorHandle allocator)
	{
		AllocateBlock(out this, allocator);
		m_Stream.AllocateForEach(bufferCount);
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe static JobHandle ScheduleConstruct<T>(out NativeStream stream, NativeList<T> bufferCount, JobHandle dependency, AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		AllocateBlock(out stream, allocator);
		ConstructJobList jobData = default(ConstructJobList);
		jobData.List = (UntypedUnsafeList*)bufferCount.GetUnsafeList();
		jobData.Container = stream;
		return jobData.Schedule(dependency);
	}

	public static JobHandle ScheduleConstruct(out NativeStream stream, NativeArray<int> bufferCount, JobHandle dependency, AllocatorManager.AllocatorHandle allocator)
	{
		AllocateBlock(out stream, allocator);
		ConstructJob jobData = default(ConstructJob);
		jobData.Length = bufferCount;
		jobData.Container = stream;
		return jobData.Schedule(dependency);
	}

	public readonly bool IsEmpty()
	{
		return m_Stream.IsEmpty();
	}

	public Reader AsReader()
	{
		return new Reader(ref this);
	}

	public Writer AsWriter()
	{
		return new Writer(ref this);
	}

	public int Count()
	{
		return m_Stream.Count();
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public NativeArray<T> ToNativeArray<T>(AllocatorManager.AllocatorHandle allocator) where T : unmanaged
	{
		return m_Stream.ToNativeArray<T>(allocator);
	}

	public void Dispose()
	{
		if (IsCreated)
		{
			m_Stream.Dispose();
		}
	}

	public JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		NativeStreamDisposeJob jobData = default(NativeStreamDisposeJob);
		jobData.Data = new NativeStreamDispose
		{
			m_StreamData = m_Stream
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_Stream = default(UnsafeStream);
		return result;
	}

	private static void AllocateBlock(out NativeStream stream, AllocatorManager.AllocatorHandle allocator)
	{
		UnsafeStream.AllocateBlock(out stream.m_Stream, allocator);
	}

	private void AllocateForEach(int forEachCount)
	{
		m_Stream.AllocateForEach(forEachCount);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private static void CheckForEachCountGreaterThanZero(int forEachCount)
	{
		if (forEachCount <= 0)
		{
			throw new ArgumentException("foreachCount must be > 0", "foreachCount");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private readonly void CheckRead()
	{
	}
}
