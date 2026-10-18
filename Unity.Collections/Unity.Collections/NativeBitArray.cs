using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace Unity.Collections;

[NativeContainer]
[DebuggerDisplay("Length = {Length}, IsCreated = {IsCreated}")]
[GenerateTestsForBurstCompatibility]
public struct NativeBitArray : INativeDisposable, IDisposable
{
	[NativeContainer]
	[NativeContainerIsReadOnly]
	public struct ReadOnly
	{
		[NativeDisableUnsafePtrRestriction]
		internal UnsafeBitArray.ReadOnly m_BitArray;

		public readonly bool IsCreated => m_BitArray.IsCreated;

		public readonly bool IsEmpty => m_BitArray.IsEmpty;

		public readonly int Length => CollectionHelper.AssumePositive(m_BitArray.Length);

		internal unsafe ReadOnly(ref NativeBitArray data)
		{
			m_BitArray = data.m_BitArray->AsReadOnly();
		}

		public readonly ulong GetBits(int pos, int numBits = 1)
		{
			return m_BitArray.GetBits(pos, numBits);
		}

		public readonly bool IsSet(int pos)
		{
			return m_BitArray.IsSet(pos);
		}

		public readonly int Find(int pos, int numBits)
		{
			return m_BitArray.Find(pos, numBits);
		}

		public readonly int Find(int pos, int count, int numBits)
		{
			return m_BitArray.Find(pos, count, numBits);
		}

		public readonly bool TestNone(int pos, int numBits = 1)
		{
			return m_BitArray.TestNone(pos, numBits);
		}

		public readonly bool TestAny(int pos, int numBits = 1)
		{
			return m_BitArray.TestAny(pos, numBits);
		}

		public readonly bool TestAll(int pos, int numBits = 1)
		{
			return m_BitArray.TestAll(pos, numBits);
		}

		public readonly int CountBits(int pos, int numBits = 1)
		{
			return m_BitArray.CountBits(pos, numBits);
		}

		[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
		private readonly void CheckRead()
		{
		}
	}

	[NativeDisableUnsafePtrRestriction]
	internal unsafe UnsafeBitArray* m_BitArray;

	internal AllocatorManager.AllocatorHandle m_Allocator;

	public unsafe readonly bool IsCreated
	{
		get
		{
			if (m_BitArray != null)
			{
				return m_BitArray->IsCreated;
			}
			return false;
		}
	}

	public readonly bool IsEmpty
	{
		get
		{
			if (IsCreated)
			{
				return Length == 0;
			}
			return true;
		}
	}

	public unsafe readonly int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return CollectionHelper.AssumePositive(m_BitArray->Length);
		}
	}

	public unsafe readonly int Capacity
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return CollectionHelper.AssumePositive(m_BitArray->Capacity);
		}
	}

	public unsafe NativeBitArray(int numBits, AllocatorManager.AllocatorHandle allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
	{
		m_BitArray = UnsafeBitArray.Alloc(allocator);
		m_Allocator = allocator;
		*m_BitArray = new UnsafeBitArray(numBits, allocator, options);
	}

	public unsafe void Resize(int numBits, NativeArrayOptions options = NativeArrayOptions.UninitializedMemory)
	{
		m_BitArray->Resize(numBits, options);
	}

	public unsafe void SetCapacity(int capacityInBits)
	{
		m_BitArray->SetCapacity(capacityInBits);
	}

	public unsafe void TrimExcess()
	{
		m_BitArray->TrimExcess();
	}

	public unsafe void Dispose()
	{
		if (IsCreated)
		{
			UnsafeBitArray.Free(m_BitArray, m_Allocator);
			m_BitArray = null;
			m_Allocator = AllocatorManager.Invalid;
		}
	}

	public unsafe JobHandle Dispose(JobHandle inputDeps)
	{
		if (!IsCreated)
		{
			return inputDeps;
		}
		NativeBitArrayDisposeJob jobData = default(NativeBitArrayDisposeJob);
		jobData.Data = new NativeBitArrayDispose
		{
			m_BitArrayData = m_BitArray,
			m_Allocator = m_Allocator
		};
		JobHandle result = jobData.Schedule(inputDeps);
		m_BitArray = null;
		m_Allocator = AllocatorManager.Invalid;
		return result;
	}

	public unsafe void Clear()
	{
		m_BitArray->Clear();
	}

	[GenerateTestsForBurstCompatibility(GenericTypeArguments = new Type[] { typeof(int) })]
	public unsafe NativeArray<T> AsNativeArray<T>() where T : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>() * 8;
		int length = m_BitArray->Length / num;
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(m_BitArray->Ptr, length, Allocator.None);
	}

	public unsafe void Set(int pos, bool value)
	{
		m_BitArray->Set(pos, value);
	}

	public unsafe void SetBits(int pos, bool value, int numBits)
	{
		m_BitArray->SetBits(pos, value, numBits);
	}

	public unsafe void SetBits(int pos, ulong value, int numBits = 1)
	{
		m_BitArray->SetBits(pos, value, numBits);
	}

	public unsafe ulong GetBits(int pos, int numBits = 1)
	{
		return m_BitArray->GetBits(pos, numBits);
	}

	public unsafe bool IsSet(int pos)
	{
		return m_BitArray->IsSet(pos);
	}

	public unsafe void Copy(int dstPos, int srcPos, int numBits)
	{
		m_BitArray->Copy(dstPos, srcPos, numBits);
	}

	public unsafe void Copy(int dstPos, ref NativeBitArray srcBitArray, int srcPos, int numBits)
	{
		m_BitArray->Copy(dstPos, ref *srcBitArray.m_BitArray, srcPos, numBits);
	}

	public unsafe int Find(int pos, int numBits)
	{
		return m_BitArray->Find(pos, numBits);
	}

	public unsafe int Find(int pos, int count, int numBits)
	{
		return m_BitArray->Find(pos, count, numBits);
	}

	public unsafe bool TestNone(int pos, int numBits = 1)
	{
		return m_BitArray->TestNone(pos, numBits);
	}

	public unsafe bool TestAny(int pos, int numBits = 1)
	{
		return m_BitArray->TestAny(pos, numBits);
	}

	public unsafe bool TestAll(int pos, int numBits = 1)
	{
		return m_BitArray->TestAll(pos, numBits);
	}

	public unsafe int CountBits(int pos, int numBits = 1)
	{
		return m_BitArray->CountBits(pos, numBits);
	}

	public ReadOnly AsReadOnly()
	{
		return new ReadOnly(ref this);
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private readonly void CheckRead()
	{
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	[Conditional("UNITY_DOTS_DEBUG")]
	private unsafe void CheckReadBounds<T>() where T : unmanaged
	{
		int num = UnsafeUtility.SizeOf<T>() * 8;
		int num2 = m_BitArray->Length / num;
		if (num2 == 0)
		{
			throw new InvalidOperationException($"Number of bits in the NativeBitArray {m_BitArray->Length} is not sufficient to cast to NativeArray<T> {UnsafeUtility.SizeOf<T>() * 8}.");
		}
		if (m_BitArray->Length != num * num2)
		{
			throw new InvalidOperationException($"Number of bits in the NativeBitArray {m_BitArray->Length} couldn't hold multiple of T {UnsafeUtility.SizeOf<T>()}. Output array would be truncated.");
		}
	}

	[Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
	private void CheckWrite()
	{
	}
}
