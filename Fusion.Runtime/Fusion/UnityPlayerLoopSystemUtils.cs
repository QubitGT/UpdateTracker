using System;
using UnityEngine.LowLevel;

namespace Fusion;

internal static class UnityPlayerLoopSystemUtils
{
	public static bool AddToPlayerLoop(ref PlayerLoopSystem parentSystem, Type referenceSystemType, UnityPlayerLoopSystemAddMode addMode, Type ownerType, PlayerLoopSystem.UpdateFunction updateDelegate)
	{
		ref PlayerLoopSystem[] subSystemList = ref parentSystem.subSystemList;
		PlayerLoopSystem[] obj = subSystemList;
		int num = ((obj != null) ? obj.Length : 0);
		if (parentSystem.type == referenceSystemType)
		{
			switch (addMode)
			{
			case UnityPlayerLoopSystemAddMode.FirstChild:
				InsertSystem(ref subSystemList, 0, ownerType, updateDelegate);
				break;
			case UnityPlayerLoopSystemAddMode.LastChild:
				InsertSystem(ref subSystemList, num, ownerType, updateDelegate);
				break;
			default:
				throw new InvalidOperationException($"Unable to add with a mode {addMode} once a system has been entered");
			}
			return true;
		}
		for (int i = 0; i < num; i++)
		{
			PlayerLoopSystem playerLoopSystem = subSystemList[i];
			if (playerLoopSystem.type == referenceSystemType)
			{
				switch (addMode)
				{
				case UnityPlayerLoopSystemAddMode.Before:
					InsertSystem(ref subSystemList, i, ownerType, updateDelegate);
					return true;
				case UnityPlayerLoopSystemAddMode.After:
					InsertSystem(ref subSystemList, i + 1, ownerType, updateDelegate);
					return true;
				}
			}
			if (AddToPlayerLoop(ref subSystemList[i], referenceSystemType, addMode, ownerType, updateDelegate))
			{
				return true;
			}
		}
		return false;
	}

	public static bool RemoveFromPlayerLoop(ref PlayerLoopSystem parentSystem, Type type)
	{
		ref PlayerLoopSystem[] subSystemList = ref parentSystem.subSystemList;
		if (subSystemList == null)
		{
			return false;
		}
		for (int i = 0; i < subSystemList.Length; i++)
		{
			PlayerLoopSystem playerLoopSystem = subSystemList[i];
			if (playerLoopSystem.type == type)
			{
				for (int j = i + 1; j < subSystemList.Length; j++)
				{
					subSystemList[j - 1] = subSystemList[j];
				}
				Array.Resize(ref subSystemList, subSystemList.Length - 1);
				return true;
			}
			if (RemoveFromPlayerLoop(ref subSystemList[i], type))
			{
				return true;
			}
		}
		return false;
	}

	private static void InsertSystem(ref PlayerLoopSystem[] systems, int position, Type ownerType, PlayerLoopSystem.UpdateFunction updateDelegate)
	{
		PlayerLoopSystem[] obj = systems;
		int num = ((obj != null) ? obj.Length : 0);
		if (position < 0 || position > num)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		PlayerLoopSystem playerLoopSystem = default(PlayerLoopSystem);
		playerLoopSystem.type = ownerType;
		playerLoopSystem.updateDelegate = updateDelegate;
		PlayerLoopSystem playerLoopSystem2 = playerLoopSystem;
		Array.Resize(ref systems, num + 1);
		if (position < num)
		{
			Array.Copy(systems, position, systems, position + 1, systems.Length - position - 1);
		}
		systems[position] = playerLoopSystem2;
	}
}
