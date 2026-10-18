using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UnityEngine.Rendering.Universal;

internal struct BuddyAllocator : IDisposable
{
	private struct Header
	{
		public int branchingOrder;

		public int levelCount;

		public int allocationCount;

		public int freeAllocationIdsCount;
	}

	private unsafe void* m_Data;

	private (int, int) m_ActiveFreeMaskCounts;

	private (int, int) m_FreeMasksStorage;

	private (int, int) m_FreeMaskIndicesStorage;

	private Allocator m_Allocator;

	private unsafe ref Header header => ref UnsafeUtility.AsRef<Header>(m_Data);

	private NativeArray<int> freeMaskCounts => GetNativeArray<int>(m_ActiveFreeMaskCounts.Item1, m_ActiveFreeMaskCounts.Item2);

	private NativeArray<ulong> freeMasksStorage => GetNativeArray<ulong>(m_FreeMasksStorage.Item1, m_FreeMasksStorage.Item2);

	private NativeArray<int> freeMaskIndicesStorage => GetNativeArray<int>(m_FreeMaskIndicesStorage.Item1, m_FreeMaskIndicesStorage.Item2);

	public int levelCount => header.levelCount;

	private NativeArray<ulong> FreeMasks(int level)
	{
		return freeMasksStorage.GetSubArray(LevelOffset64(level, header.branchingOrder), LevelLength64(level, header.branchingOrder));
	}

	private NativeArray<int> FreeMaskIndices(int level)
	{
		return freeMaskIndicesStorage.GetSubArray(LevelOffset64(level, header.branchingOrder), LevelLength64(level, header.branchingOrder));
	}

	public unsafe BuddyAllocator(int levelCount, int branchingOrder, Allocator allocator = Allocator.Persistent)
	{
		int dataSize = sizeof(Header);
		m_ActiveFreeMaskCounts = AllocateRange<int>(levelCount, ref dataSize);
		m_FreeMasksStorage = AllocateRange<ulong>(LevelOffset64(levelCount, branchingOrder), ref dataSize);
		m_FreeMaskIndicesStorage = AllocateRange<int>(LevelOffset64(levelCount, branchingOrder), ref dataSize);
		m_Data = UnsafeUtility.Malloc(dataSize, 64, allocator);
		UnsafeUtility.MemClear(m_Data, dataSize);
		m_Allocator = allocator;
		header = new Header
		{
			branchingOrder = branchingOrder,
			levelCount = levelCount
		};
		NativeArray<ulong> nativeArray = FreeMasks(0);
		nativeArray[0] = 15uL;
		NativeArray<int> nativeArray2 = freeMaskCounts;
		nativeArray2[0] = 1;
	}

	public bool TryAllocate(int requestedLevel, out BuddyAllocation allocation)
	{
		allocation = default(BuddyAllocation);
		int num = requestedLevel;
		NativeArray<int> nativeArray = freeMaskCounts;
		while (num >= 0 && nativeArray[num] <= 0)
		{
			num--;
		}
		if (num < 0)
		{
			return false;
		}
		NativeArray<int> nativeArray2 = FreeMaskIndices(num);
		int num2 = nativeArray2[--nativeArray[num]];
		NativeArray<ulong> nativeArray3 = FreeMasks(num);
		ulong num3 = nativeArray3[num2];
		int num4 = math.tzcnt(num3);
		num3 = (nativeArray3[num2] = num3 ^ (ulong)(1L << num4));
		if (num3 != 0L)
		{
			nativeArray2[nativeArray[num]++] = num2;
		}
		int num6 = num2 * 64 + num4;
		while (num < requestedLevel)
		{
			num++;
			num6 <<= header.branchingOrder;
			int num7 = num6 >> 6;
			int num8 = num6 & 0x3F;
			NativeArray<ulong> nativeArray4 = FreeMasks(num);
			ulong num9 = nativeArray4[num7];
			if (num9 == 0L)
			{
				NativeArray<int> nativeArray5 = FreeMaskIndices(num);
				nativeArray5[nativeArray[num]++] = num7;
			}
			num9 |= (ulong)((1L << Pow2(header.branchingOrder)) - 2 << num8);
			nativeArray4[num7] = num9;
		}
		allocation.level = num;
		allocation.index = num6;
		return true;
	}

	public void Free(BuddyAllocation allocation)
	{
		int num = allocation.level;
		int num2 = allocation.index;
		while (num >= 0)
		{
			int num3 = num2 >> 6;
			int num4 = num2 & 0x3F;
			NativeArray<ulong> nativeArray = FreeMasks(num);
			ulong num5 = nativeArray[num3];
			bool flag = num5 == 0;
			num5 |= (ulong)(1L << num4);
			NativeArray<int> nativeArray2 = FreeMaskIndices(num);
			NativeArray<int> nativeArray3 = freeMaskCounts;
			ulong num6 = (ulong)((1L << Pow2(header.branchingOrder)) - 1 << (num4 >> header.branchingOrder) * Pow2(header.branchingOrder));
			if (num == 0 || (~num5 & num6) != 0L)
			{
				nativeArray[num3] = num5;
				if (flag)
				{
					nativeArray2[nativeArray3[num]++] = num3;
				}
				break;
			}
			num5 = (nativeArray[num3] = num5 & ~num6);
			if (!flag && num5 == 0L)
			{
				for (int i = 0; i < nativeArray2.Length; i++)
				{
					if (nativeArray2[i] == num3)
					{
						nativeArray2[i] = nativeArray2[--nativeArray3[num]];
						break;
					}
				}
			}
			num--;
			num2 >>= header.branchingOrder;
		}
	}

	public unsafe void Dispose()
	{
		UnsafeUtility.Free(m_Data, m_Allocator);
		m_Data = default(void*);
		m_Allocator = Allocator.Invalid;
	}

	private unsafe NativeArray<T> GetNativeArray<T>(int offset, int length) where T : struct
	{
		return NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(PtrAdd(m_Data, offset), length, m_Allocator);
	}

	private static int LevelOffset(int level, int branchingOrder)
	{
		return Pow2(branchingOrder) * (Pow2(branchingOrder * (level - 1) + branchingOrder) - 1) / (Pow2(branchingOrder) - 1);
	}

	private static int LevelLength(int level, int branchingOrder)
	{
		return Pow2N(branchingOrder, level + 1);
	}

	private static int LevelOffset64(int level, int branchingOrder)
	{
		return math.min(level, 6 / branchingOrder) + LevelOffset(math.max(0, level - 6 / branchingOrder), branchingOrder);
	}

	private static int LevelLength64(int level, int branchingOrder)
	{
		return Pow2N(branchingOrder, math.max(0, level - 6 / branchingOrder + 1));
	}

	private static (int, int) AllocateRange<T>(int length, ref int dataSize) where T : struct
	{
		dataSize = AlignForward(dataSize, UnsafeUtility.AlignOf<T>());
		(int, int) result = (dataSize, length);
		dataSize += length * UnsafeUtility.SizeOf<T>();
		return result;
	}

	private static int AlignForward(int offset, int alignment)
	{
		int num = offset % alignment;
		if (num != 0)
		{
			offset += alignment - num;
		}
		return offset;
	}

	private unsafe static void* PtrAdd(void* ptr, int bytes)
	{
		return (void*)((IntPtr)ptr + bytes);
	}

	private static int Pow2(int n)
	{
		return 1 << n;
	}

	private static int Pow2N(int x, int n)
	{
		return 1 << x * n;
	}
}
