using System;
using UnityEngine;

namespace Pathfinding.Voxels;

public class VoxelArea
{
	public const uint MaxHeight = 65536u;

	public const int MaxHeightInt = 65536;

	public const uint InvalidSpanValue = uint.MaxValue;

	public const float AvgSpanLayerCountEstimate = 8f;

	public readonly int width;

	public readonly int depth;

	public CompactVoxelSpan[] compactSpans;

	public CompactVoxelCell[] compactCells;

	public int compactSpanCount;

	public ushort[] tmpUShortArr;

	public int[] areaTypes;

	public ushort[] dist;

	public ushort maxDistance;

	public int maxRegions;

	public int[] DirectionX;

	public int[] DirectionZ;

	public Vector3[] VectorDirection;

	private int linkedSpanCount;

	public LinkedVoxelSpan[] linkedSpans;

	private int[] removedStack = new int[128];

	private int removedStackCount;

	public void Reset()
	{
		ResetLinkedVoxelSpans();
		for (int i = 0; i < compactCells.Length; i++)
		{
			compactCells[i].count = 0u;
			compactCells[i].index = 0u;
		}
	}

	private void ResetLinkedVoxelSpans()
	{
		int num = linkedSpans.Length;
		linkedSpanCount = width * depth;
		LinkedVoxelSpan linkedVoxelSpan = new LinkedVoxelSpan(uint.MaxValue, uint.MaxValue, -1, -1);
		int num2;
		for (num2 = 0; num2 < num; num2++)
		{
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
			num2++;
			linkedSpans[num2] = linkedVoxelSpan;
		}
		removedStackCount = 0;
	}

	public VoxelArea(int width, int depth)
	{
		this.width = width;
		this.depth = depth;
		int num = width * depth;
		compactCells = new CompactVoxelCell[num];
		linkedSpans = new LinkedVoxelSpan[((int)((float)num * 8f) + 15) & -16];
		ResetLinkedVoxelSpans();
		DirectionX = new int[4] { -1, 0, 1, 0 };
		DirectionZ = new int[4]
		{
			0,
			width,
			0,
			-width
		};
		VectorDirection = new Vector3[4]
		{
			Vector3.left,
			Vector3.forward,
			Vector3.right,
			Vector3.back
		};
	}

	public int GetSpanCountAll()
	{
		int num = 0;
		int num2 = width * depth;
		for (int i = 0; i < num2; i++)
		{
			int num3 = i;
			while (num3 != -1 && linkedSpans[num3].bottom != uint.MaxValue)
			{
				num++;
				num3 = linkedSpans[num3].next;
			}
		}
		return num;
	}

	public int GetSpanCount()
	{
		int num = 0;
		int num2 = width * depth;
		for (int i = 0; i < num2; i++)
		{
			int num3 = i;
			while (num3 != -1 && linkedSpans[num3].bottom != uint.MaxValue)
			{
				if (linkedSpans[num3].area != 0)
				{
					num++;
				}
				num3 = linkedSpans[num3].next;
			}
		}
		return num;
	}

	private void PushToSpanRemovedStack(int index)
	{
		if (removedStackCount == removedStack.Length)
		{
			int[] dst = new int[removedStackCount * 4];
			Buffer.BlockCopy(removedStack, 0, dst, 0, removedStackCount * 4);
			removedStack = dst;
		}
		removedStack[removedStackCount] = index;
		removedStackCount++;
	}

	public void AddLinkedSpan(int index, uint bottom, uint top, int area, int voxelWalkableClimb)
	{
		LinkedVoxelSpan[] array = linkedSpans;
		if (array[index].bottom == uint.MaxValue)
		{
			array[index] = new LinkedVoxelSpan(bottom, top, area);
			return;
		}
		int num = -1;
		int num2 = index;
		while (index != -1)
		{
			LinkedVoxelSpan linkedVoxelSpan = array[index];
			if (linkedVoxelSpan.bottom > top)
			{
				break;
			}
			if (linkedVoxelSpan.top < bottom)
			{
				num = index;
				index = linkedVoxelSpan.next;
				continue;
			}
			bottom = Math.Min(linkedVoxelSpan.bottom, bottom);
			top = Math.Max(linkedVoxelSpan.top, top);
			if (Math.Abs((int)(top - linkedVoxelSpan.top)) <= voxelWalkableClimb)
			{
				area = Math.Max(area, linkedVoxelSpan.area);
			}
			int next = linkedVoxelSpan.next;
			if (num != -1)
			{
				array[num].next = next;
				PushToSpanRemovedStack(index);
				index = next;
				continue;
			}
			if (next != -1)
			{
				array[num2] = array[next];
				PushToSpanRemovedStack(next);
				continue;
			}
			array[num2] = new LinkedVoxelSpan(bottom, top, area);
			return;
		}
		if (linkedSpanCount >= array.Length)
		{
			LinkedVoxelSpan[] array2 = array;
			int num3 = linkedSpanCount;
			int num4 = removedStackCount;
			array = (linkedSpans = new LinkedVoxelSpan[array.Length * 2]);
			ResetLinkedVoxelSpans();
			linkedSpanCount = num3;
			removedStackCount = num4;
			for (int i = 0; i < linkedSpanCount; i++)
			{
				array[i] = array2[i];
			}
		}
		int num5;
		if (removedStackCount > 0)
		{
			removedStackCount--;
			num5 = removedStack[removedStackCount];
		}
		else
		{
			num5 = linkedSpanCount;
			linkedSpanCount++;
		}
		if (num != -1)
		{
			array[num5] = new LinkedVoxelSpan(bottom, top, area, array[num].next);
			array[num].next = num5;
		}
		else
		{
			array[num5] = array[num2];
			array[num2] = new LinkedVoxelSpan(bottom, top, area, num5);
		}
	}
}
