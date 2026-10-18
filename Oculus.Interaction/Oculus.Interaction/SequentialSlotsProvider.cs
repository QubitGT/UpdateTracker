using System;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

public class SequentialSlotsProvider : MonoBehaviour, ISnapPoseDelegate
{
	[SerializeField]
	private List<Transform> _slots;

	private int[] _slotInteractors;

	protected bool _started;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_slotInteractors = new int[_slots.Count];
		this.EndStart(ref _started);
	}

	public void TrackElement(int id, Pose pose)
	{
		int num = FindBestSlotIndex(in pose.position);
		if (TryOccupySlot(num))
		{
			_slotInteractors[num] = id;
		}
	}

	public void UntrackElement(int id)
	{
		if (TryFindIndexForInteractor(id, out var index))
		{
			_slotInteractors[index] = 0;
		}
	}

	public void SnapElement(int id, Pose pose)
	{
	}

	public void UnsnapElement(int id)
	{
	}

	public void MoveTrackedElement(int id, Pose pose)
	{
		int num = FindBestSlotIndex(in pose.position);
		if (TryFindIndexForInteractor(id, out var index))
		{
			if (num != index)
			{
				_slotInteractors[index] = 0;
				if (TryOccupySlot(num))
				{
					_slotInteractors[num] = id;
				}
			}
		}
		else if (TryOccupySlot(num))
		{
			_slotInteractors[num] = id;
		}
	}

	private bool TryFindIndexForInteractor(int id, out int index)
	{
		index = Array.FindIndex(_slotInteractors, (int i) => i == id);
		return index >= 0;
	}

	public bool SnapPoseForElement(int id, Pose pose, out Pose result)
	{
		if (TryFindIndexForInteractor(id, out var index))
		{
			result = _slots[index].GetPose();
			return true;
		}
		int num = FindBestSlotIndex(in pose.position, freeOnly: true);
		if (num >= 0)
		{
			result = _slots[num].GetPose();
			return true;
		}
		result = Pose.identity;
		return false;
	}

	private bool TryOccupySlot(int index)
	{
		if (IsSlotFree(index))
		{
			return true;
		}
		Vector3 target = _slots[index].position;
		int num = FindBestSlotIndex(in target, freeOnly: true);
		if (num < 0)
		{
			return false;
		}
		PushSlots(index, num);
		return true;
	}

	private bool IsSlotFree(int index)
	{
		return _slotInteractors[index] == 0;
	}

	private int FindBestSlotIndex(in Vector3 target, bool freeOnly = false)
	{
		int result = -1;
		float num = float.PositiveInfinity;
		for (int i = 0; i < _slots.Count; i++)
		{
			if (!freeOnly || IsSlotFree(i))
			{
				float sqrMagnitude = (target - _slots[i].position).sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					result = i;
				}
			}
		}
		return result;
	}

	private void PushSlots(int index, int freeSlot)
	{
		bool forwardDirection = index > freeSlot;
		for (int num = freeSlot; num != index; num = Next(num))
		{
			int freeSlot2 = Next(num);
			SwapSlot(num, freeSlot2);
		}
		int Next(int value)
		{
			return value + (forwardDirection ? 1 : (-1));
		}
	}

	private void SwapSlot(int index, int freeSlot)
	{
		ref int reference = ref _slotInteractors[index];
		ref int reference2 = ref _slotInteractors[freeSlot];
		int num = _slotInteractors[freeSlot];
		int num2 = _slotInteractors[index];
		reference = num;
		reference2 = num2;
	}

	public void InjectAllSequentialSlotsProvider(List<Transform> slots)
	{
		InjectSlots(slots);
	}

	public void InjectSlots(List<Transform> slots)
	{
		_slots = slots;
	}
}
