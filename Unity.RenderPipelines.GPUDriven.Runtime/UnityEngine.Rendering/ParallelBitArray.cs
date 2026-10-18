using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UnityEngine.Rendering;

internal struct ParallelBitArray
{
	private Allocator m_Allocator;

	private NativeArray<long> m_Bits;

	private int m_Length;

	public int Length => m_Length;

	public bool IsCreated => m_Bits.IsCreated;

	public ParallelBitArray(int length, Allocator allocator, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
	{
		m_Allocator = allocator;
		m_Bits = new NativeArray<long>((length + 63) / 64, allocator, options);
		m_Length = length;
	}

	public void Dispose()
	{
		m_Bits.Dispose();
		m_Length = 0;
	}

	public void Dispose(JobHandle inputDeps)
	{
		m_Bits.Dispose(inputDeps);
		m_Length = 0;
	}

	public void Resize(int newLength)
	{
		int length = m_Length;
		if (newLength == length)
		{
			return;
		}
		int length2 = m_Bits.Length;
		int num = (newLength + 63) / 64;
		if (num != length2)
		{
			NativeArray<long> nativeArray = new NativeArray<long>(num, m_Allocator, NativeArrayOptions.UninitializedMemory);
			if (m_Bits.IsCreated)
			{
				NativeArray<long>.Copy(m_Bits, nativeArray, m_Bits.Length);
				m_Bits.Dispose();
			}
			m_Bits = nativeArray;
		}
		int num2 = Math.Min(length, newLength);
		for (int i = Math.Min(length2, num); i < m_Bits.Length; i++)
		{
			int num3 = Math.Max(num2 - 64 * i, 0);
			if (num3 < 64)
			{
				ulong num4 = (ulong)((1L << num3) - 1);
				m_Bits[i] &= (long)num4;
			}
		}
		m_Length = newLength;
	}

	public unsafe void Set(int index, bool value)
	{
		int num = index >> 6;
		long* unsafePtr = (long*)m_Bits.GetUnsafePtr();
		ulong num2 = (ulong)(1L << (index & 0x3F));
		long num3 = (long)(~num2);
		long num4 = (long)(value ? num2 : 0);
		long num5;
		long value2;
		do
		{
			num5 = Interlocked.Read(ref unsafePtr[num]);
			value2 = (num5 & num3) | num4;
		}
		while (Interlocked.CompareExchange(ref unsafePtr[num], value2, num5) != num5);
	}

	public unsafe bool Get(int index)
	{
		int num = index >> 6;
		long* unsafeReadOnlyPtr = (long*)m_Bits.GetUnsafeReadOnlyPtr();
		long num2 = 1L << (index & 0x3F);
		return (unsafeReadOnlyPtr[num] & num2) != 0;
	}

	public ulong GetChunk(int chunk_index)
	{
		return (ulong)m_Bits[chunk_index];
	}

	public void SetChunk(int chunk_index, ulong chunk_bits)
	{
		m_Bits[chunk_index] = (long)chunk_bits;
	}

	public unsafe ulong InterlockedReadChunk(int chunk_index)
	{
		long* unsafeReadOnlyPtr = (long*)m_Bits.GetUnsafeReadOnlyPtr();
		return (ulong)Interlocked.Read(ref unsafeReadOnlyPtr[chunk_index]);
	}

	public unsafe void InterlockedOrChunk(int chunk_index, ulong chunk_bits)
	{
		long* unsafePtr = (long*)m_Bits.GetUnsafePtr();
		long num;
		long value;
		do
		{
			num = Interlocked.Read(ref unsafePtr[chunk_index]);
			value = num | (long)chunk_bits;
		}
		while (Interlocked.CompareExchange(ref unsafePtr[chunk_index], value, num) != num);
	}

	public int ChunkCount()
	{
		return m_Bits.Length;
	}

	public ParallelBitArray GetSubArray(int length)
	{
		ParallelBitArray result = default(ParallelBitArray);
		result.m_Bits = m_Bits.GetSubArray(0, (length + 63) / 64);
		result.m_Length = length;
		return result;
	}

	public NativeArray<long> GetBitsArray()
	{
		return m_Bits;
	}

	public void FillZeroes(int length)
	{
		length = Math.Min(length, m_Length);
		int num = length / 64;
		int num2 = length & 0x3F;
		ref NativeArray<long> bits = ref m_Bits;
		long value = 0L;
		bits.FillArray(in value, 0, num);
		if (num2 > 0)
		{
			long num3 = (1L << num2) - 1;
			m_Bits[num] &= ~num3;
		}
	}
}
