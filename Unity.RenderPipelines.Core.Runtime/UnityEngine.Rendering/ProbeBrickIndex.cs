using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;

namespace UnityEngine.Rendering;

internal class ProbeBrickIndex
{
	[Serializable]
	[DebuggerDisplay("Brick [{position}, {subdivisionLevel}]")]
	public struct Brick : IEquatable<Brick>
	{
		public Vector3Int position;

		public int subdivisionLevel;

		internal Brick(Vector3Int position, int subdivisionLevel)
		{
			this.position = position;
			this.subdivisionLevel = subdivisionLevel;
		}

		public bool Equals(Brick other)
		{
			if (position == other.position)
			{
				return subdivisionLevel == other.subdivisionLevel;
			}
			return false;
		}

		public bool IntersectArea(Bounds boundInBricksToCheck)
		{
			int num = ProbeReferenceVolume.CellSize(subdivisionLevel);
			Bounds bounds = default(Bounds);
			bounds.min = position;
			bounds.max = position + new Vector3Int(num, num, num);
			bounds.extents *= 0.99f;
			return boundInBricksToCheck.Intersects(bounds);
		}
	}

	public struct IndirectionEntryUpdateInfo
	{
		public int firstChunkIndex;

		public int numberOfChunks;

		public int minSubdivInCell;

		public Vector3Int minValidBrickIndexForCellAtMaxRes;

		public Vector3Int maxValidBrickIndexForCellAtMaxResPlusOne;

		public Vector3Int entryPositionInBricksAtMaxRes;

		public bool hasOnlyBiggerBricks;
	}

	public struct CellIndexUpdateInfo
	{
		public IndirectionEntryUpdateInfo[] entriesInfo;

		public int GetNumberOfChunks()
		{
			int num = 0;
			IndirectionEntryUpdateInfo[] array = entriesInfo;
			for (int i = 0; i < array.Length; i++)
			{
				IndirectionEntryUpdateInfo indirectionEntryUpdateInfo = array[i];
				num += indirectionEntryUpdateInfo.numberOfChunks;
			}
			return num;
		}
	}

	internal const int kMaxSubdivisionLevels = 7;

	internal const int kIndexChunkSize = 243;

	internal const int kFailChunkIndex = -1;

	internal const int kEmptyIndex = -2;

	private BitArray m_IndexChunks;

	private BitArray m_IndexChunksCopyForChecks;

	private int m_ChunksCount;

	private int m_AvailableChunkCount;

	private ComputeBuffer m_PhysicalIndexBuffer;

	private NativeArray<int> m_PhysicalIndexBufferData;

	private ComputeBuffer m_DebugFragmentationBuffer;

	private int[] m_DebugFragmentationData;

	private bool m_NeedUpdateIndexComputeBuffer;

	private int m_UpdateMinIndex = int.MaxValue;

	private int m_UpdateMaxIndex = int.MinValue;

	private Vector3Int m_CenterRS;

	internal int estimatedVMemCost { get; private set; }

	internal float fragmentationRate { get; private set; }

	internal ComputeBuffer GetDebugFragmentationBuffer()
	{
		return m_DebugFragmentationBuffer;
	}

	private int SizeOfPhysicalIndexFromBudget(ProbeVolumeTextureMemoryBudget memoryBudget)
	{
		return memoryBudget switch
		{
			ProbeVolumeTextureMemoryBudget.MemoryBudgetLow => 4000000, 
			ProbeVolumeTextureMemoryBudget.MemoryBudgetMedium => 8000000, 
			ProbeVolumeTextureMemoryBudget.MemoryBudgetHigh => 16000000, 
			_ => 32000000, 
		};
	}

	internal ProbeBrickIndex(ProbeVolumeTextureMemoryBudget memoryBudget)
	{
		m_CenterRS = new Vector3Int(0, 0, 0);
		m_NeedUpdateIndexComputeBuffer = false;
		m_ChunksCount = Mathf.Max(1, Mathf.CeilToInt((float)SizeOfPhysicalIndexFromBudget(memoryBudget) / 243f));
		m_AvailableChunkCount = m_ChunksCount;
		m_IndexChunks = new BitArray(m_ChunksCount);
		m_IndexChunksCopyForChecks = new BitArray(m_ChunksCount);
		int num = m_ChunksCount * 243;
		m_PhysicalIndexBufferData = new NativeArray<int>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_PhysicalIndexBuffer = new ComputeBuffer(num, 4, ComputeBufferType.Structured);
		estimatedVMemCost = num * 4;
		Clear();
	}

	public int GetRemainingChunkCount()
	{
		return m_AvailableChunkCount;
	}

	internal void UploadIndexData()
	{
		int count = m_UpdateMaxIndex - m_UpdateMinIndex + 1;
		m_PhysicalIndexBuffer.SetData(m_PhysicalIndexBufferData, m_UpdateMinIndex, m_UpdateMinIndex, count);
		m_NeedUpdateIndexComputeBuffer = false;
		m_UpdateMaxIndex = int.MinValue;
		m_UpdateMinIndex = int.MaxValue;
	}

	private void UpdateDebugData()
	{
		if (m_DebugFragmentationData == null || m_DebugFragmentationData.Length != m_IndexChunks.Length)
		{
			m_DebugFragmentationData = new int[m_IndexChunks.Length];
			CoreUtils.SafeRelease(m_DebugFragmentationBuffer);
			m_DebugFragmentationBuffer = new ComputeBuffer(m_IndexChunks.Length, 4);
		}
		for (int i = 0; i < m_IndexChunks.Length; i++)
		{
			m_DebugFragmentationData[i] = (m_IndexChunks[i] ? 1 : (-1));
		}
		m_DebugFragmentationBuffer.SetData(m_DebugFragmentationData);
	}

	internal unsafe void Clear()
	{
		m_IndexChunks.SetAll(value: false);
		m_AvailableChunkCount = m_ChunksCount;
		uint* unsafePtr = (uint*)m_PhysicalIndexBufferData.GetUnsafePtr();
		UnsafeUtility.MemSet(unsafePtr, byte.MaxValue, m_PhysicalIndexBufferData.Length * 4);
		m_NeedUpdateIndexComputeBuffer = true;
		m_UpdateMinIndex = 0;
		m_UpdateMaxIndex = m_PhysicalIndexBufferData.Length - 1;
	}

	internal void GetRuntimeResources(ref ProbeReferenceVolume.RuntimeResources rr)
	{
		bool displayIndexFragmentation = ProbeReferenceVolume.instance.probeVolumeDebug.displayIndexFragmentation;
		if (m_NeedUpdateIndexComputeBuffer)
		{
			UploadIndexData();
			if (displayIndexFragmentation)
			{
				UpdateDebugData();
			}
		}
		if (displayIndexFragmentation && m_DebugFragmentationBuffer == null)
		{
			UpdateDebugData();
		}
		rr.index = m_PhysicalIndexBuffer;
	}

	internal void Cleanup()
	{
		m_PhysicalIndexBufferData.Dispose();
		CoreUtils.SafeRelease(m_PhysicalIndexBuffer);
		m_PhysicalIndexBuffer = null;
		CoreUtils.SafeRelease(m_DebugFragmentationBuffer);
		m_DebugFragmentationBuffer = null;
	}

	internal void ComputeFragmentationRate()
	{
		int num = 0;
		for (int num2 = m_ChunksCount - 1; num2 >= 0; num2--)
		{
			if (m_IndexChunks[num2])
			{
				num = num2 + 1;
				break;
			}
		}
		int num3 = m_ChunksCount - num;
		int num4 = m_AvailableChunkCount - num3;
		fragmentationRate = (float)num4 / (float)num;
	}

	private int MergeIndex(int index, int size)
	{
		return (index & -1879048193) | ((size & 7) << 28);
	}

	internal int GetNumberOfChunks(int brickCount)
	{
		return Mathf.CeilToInt((float)brickCount / 243f);
	}

	internal bool FindSlotsForEntries(ref IndirectionEntryUpdateInfo[] entriesInfo)
	{
		using (new ProfilerMarker("FindSlotsForEntries").Auto())
		{
			m_IndexChunksCopyForChecks.SetAll(value: false);
			m_IndexChunksCopyForChecks.Or(m_IndexChunks);
			int num = entriesInfo.Length;
			for (int i = 0; i < num; i++)
			{
				entriesInfo[i].firstChunkIndex = -2;
				int numberOfChunks = entriesInfo[i].numberOfChunks;
				if (numberOfChunks == 0)
				{
					continue;
				}
				for (int j = 0; j < m_ChunksCount - numberOfChunks; j++)
				{
					if (!m_IndexChunksCopyForChecks[j])
					{
						int firstChunkIndex = j;
						int num2 = j + numberOfChunks;
						while (j + 1 < num2 && !m_IndexChunksCopyForChecks[++j])
						{
						}
						if (!m_IndexChunksCopyForChecks[j])
						{
							entriesInfo[i].firstChunkIndex = firstChunkIndex;
							break;
						}
					}
				}
				if (entriesInfo[i].firstChunkIndex < 0)
				{
					for (int k = 0; k < num; k++)
					{
						entriesInfo[k].firstChunkIndex = -1;
					}
					return false;
				}
				for (int l = entriesInfo[i].firstChunkIndex; l < entriesInfo[i].firstChunkIndex + numberOfChunks; l++)
				{
					m_IndexChunksCopyForChecks[l] = true;
				}
			}
			return true;
		}
	}

	internal bool ReserveChunks(IndirectionEntryUpdateInfo[] entriesInfo, bool ignoreErrorLog)
	{
		int num = entriesInfo.Length;
		for (int i = 0; i < num; i++)
		{
			int firstChunkIndex = entriesInfo[i].firstChunkIndex;
			int numberOfChunks = entriesInfo[i].numberOfChunks;
			if (numberOfChunks == 0)
			{
				continue;
			}
			if (firstChunkIndex < 0)
			{
				if (!ignoreErrorLog)
				{
					Debug.LogError("APV Index Allocation failed.");
				}
				return false;
			}
			for (int j = firstChunkIndex; j < firstChunkIndex + numberOfChunks; j++)
			{
				m_IndexChunks[j] = true;
			}
			m_AvailableChunkCount -= numberOfChunks;
		}
		return true;
	}

	internal static bool BrickOverlapEntry(Vector3Int brickMin, Vector3Int brickMax, Vector3Int entryMin, Vector3Int entryMax)
	{
		if (brickMax.x > entryMin.x && entryMax.x > brickMin.x && brickMax.y > entryMin.y && entryMax.y > brickMin.y && brickMax.z > entryMin.z)
		{
			return entryMax.z > brickMin.z;
		}
		return false;
	}

	private static int LocationToIndex(int x, int y, int z, Vector3Int sizeOfValid)
	{
		return z * (sizeOfValid.x * sizeOfValid.y) + x * sizeOfValid.y + y;
	}

	private void MarkBrickInPhysicalBuffer(in IndirectionEntryUpdateInfo entry, Vector3Int brickMin, Vector3Int brickMax, int brickSubdivLevel, int entrySubdivLevel, int idx)
	{
		m_NeedUpdateIndexComputeBuffer = true;
		if (entry.hasOnlyBiggerBricks)
		{
			int num = entry.firstChunkIndex * 243;
			m_UpdateMinIndex = Math.Min(m_UpdateMinIndex, num);
			m_UpdateMaxIndex = Math.Max(m_UpdateMaxIndex, num);
			m_PhysicalIndexBufferData[num] = idx;
			return;
		}
		int num2 = ProbeReferenceVolume.CellSize(entry.minSubdivInCell);
		Vector3Int vector3Int = entry.minValidBrickIndexForCellAtMaxRes / num2;
		Vector3Int vector3Int2 = entry.maxValidBrickIndexForCellAtMaxResPlusOne / num2 - vector3Int;
		if (brickSubdivLevel >= entrySubdivLevel)
		{
			brickMin = Vector3Int.zero;
			brickMax = vector3Int2;
		}
		else
		{
			brickMin -= entry.entryPositionInBricksAtMaxRes;
			brickMax -= entry.entryPositionInBricksAtMaxRes;
			brickMin /= num2;
			brickMax /= num2;
			ProbeReferenceVolume.CellSize(entrySubdivLevel - entry.minSubdivInCell);
			brickMin -= vector3Int;
			brickMax -= vector3Int;
		}
		int num3 = entry.firstChunkIndex * 243;
		int val = num3 + LocationToIndex(brickMin.x, brickMin.y, brickMin.z, vector3Int2);
		int val2 = num3 + LocationToIndex(brickMax.x - 1, brickMax.y - 1, brickMax.z - 1, vector3Int2);
		m_UpdateMinIndex = Math.Min(m_UpdateMinIndex, val);
		m_UpdateMaxIndex = Math.Max(m_UpdateMaxIndex, val2);
		for (int i = brickMin.x; i < brickMax.x; i++)
		{
			for (int j = brickMin.z; j < brickMax.z; j++)
			{
				for (int k = brickMin.y; k < brickMax.y; k++)
				{
					int num4 = LocationToIndex(i, k, j, vector3Int2);
					m_PhysicalIndexBufferData[num3 + num4] = idx;
				}
			}
		}
	}

	public void AddBricks(ProbeReferenceVolume.CellIndexInfo cellInfo, NativeArray<Brick> bricks, List<ProbeBrickPool.BrickChunkAlloc> allocations, int allocationSize, int poolWidth, int poolHeight)
	{
		int entrySubdivLevel = ProbeReferenceVolume.instance.GetEntrySubdivLevel();
		int num = 0;
		for (int i = 0; i < allocations.Count; i++)
		{
			ProbeBrickPool.BrickChunkAlloc brickChunkAlloc = allocations[i];
			int num2 = num + Mathf.Min(allocationSize, bricks.Length - num);
			while (num != num2)
			{
				Brick brick = bricks[num++];
				int idx = MergeIndex(brickChunkAlloc.flattenIndex(poolWidth, poolHeight), brick.subdivisionLevel);
				brickChunkAlloc.x += 4;
				int num3 = ProbeReferenceVolume.CellSize(brick.subdivisionLevel);
				Vector3Int position = brick.position;
				Vector3Int brickMax = brick.position + new Vector3Int(num3, num3, num3);
				IndirectionEntryUpdateInfo[] entriesInfo = cellInfo.updateInfo.entriesInfo;
				for (int j = 0; j < entriesInfo.Length; j++)
				{
					IndirectionEntryUpdateInfo entry = entriesInfo[j];
					Vector3Int entryMin = entry.entryPositionInBricksAtMaxRes + entry.minValidBrickIndexForCellAtMaxRes;
					Vector3Int entryMax = entry.entryPositionInBricksAtMaxRes + entry.maxValidBrickIndexForCellAtMaxResPlusOne - Vector3Int.one;
					if (BrickOverlapEntry(position, brickMax, entryMin, entryMax))
					{
						MarkBrickInPhysicalBuffer(in entry, position, brickMax, brick.subdivisionLevel, entrySubdivLevel, idx);
					}
				}
			}
		}
	}

	public void RemoveBricks(ProbeReferenceVolume.CellIndexInfo cellInfo)
	{
		for (int i = 0; i < cellInfo.updateInfo.entriesInfo.Length; i++)
		{
			ref IndirectionEntryUpdateInfo reference = ref cellInfo.updateInfo.entriesInfo[i];
			if (reference.firstChunkIndex >= 0)
			{
				for (int j = reference.firstChunkIndex; j < reference.firstChunkIndex + reference.numberOfChunks; j++)
				{
					m_IndexChunks[j] = false;
				}
				m_AvailableChunkCount += reference.numberOfChunks;
				reference.numberOfChunks = 0;
			}
		}
	}
}
