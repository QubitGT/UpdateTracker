using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Liv.Lck;

internal static class LckUpdateManager
{
	private static ILckEarlyUpdate _earlyUpdateSystem;

	private static ILckLateUpdate _lateUpdateSystem;

	public static void RegisterSingleEarlyUpdate(ILckEarlyUpdate earlyUpdateSystem)
	{
		if (_earlyUpdateSystem != null)
		{
			LckLog.LogWarning($"LCK EarlyUpdateSystem already has a reference ({_earlyUpdateSystem}). Note only one system is supported.", "RegisterSingleEarlyUpdate", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckUpdateManager.cs", 25);
		}
		_earlyUpdateSystem = earlyUpdateSystem;
	}

	public static void UnregisterSingleEarlyUpdate(ILckEarlyUpdate earlyUpdateSystem)
	{
		if (_earlyUpdateSystem == earlyUpdateSystem)
		{
			_earlyUpdateSystem = null;
		}
	}

	public static void RegisterSingleLateUpdate(ILckLateUpdate lateUpdateSystem)
	{
		if (_lateUpdateSystem != null)
		{
			LckLog.LogWarning($"LCK LateUpdateSystem already has a reference ({_lateUpdateSystem}). Note only one system is supported.", "RegisterSingleLateUpdate", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckUpdateManager.cs", 48);
		}
		_lateUpdateSystem = lateUpdateSystem;
	}

	public static void UnregisterSingleLateUpdate(ILckLateUpdate lateUpdateSystem)
	{
		if (_lateUpdateSystem == lateUpdateSystem)
		{
			_lateUpdateSystem = null;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		PlayerLoopSystem loopSystem = PlayerLoop.GetCurrentPlayerLoop();
		loopSystem = RemoveSystem<LckEarlyUpdate>(in loopSystem);
		loopSystem = RemoveSystem<LckLateUpdate>(in loopSystem);
		PlayerLoopSystem playerLoopSystem = default(PlayerLoopSystem);
		playerLoopSystem.subSystemList = null;
		playerLoopSystem.updateDelegate = OnEarlyUpdate;
		playerLoopSystem.type = typeof(LckEarlyUpdate);
		PlayerLoopSystem systemToAdd = playerLoopSystem;
		playerLoopSystem = default(PlayerLoopSystem);
		playerLoopSystem.subSystemList = null;
		playerLoopSystem.updateDelegate = OnLateUpdate;
		playerLoopSystem.type = typeof(LckLateUpdate);
		PlayerLoopSystem systemToAdd2 = playerLoopSystem;
		PlayerLoopSystem loopSystem2 = AddSystem<EarlyUpdate>(in loopSystem, systemToAdd);
		PlayerLoop.SetPlayerLoop(AddSystem<PostLateUpdate>(in loopSystem2, systemToAdd2));
	}

	private static PlayerLoopSystem AddSystem<T>(in PlayerLoopSystem loopSystem, PlayerLoopSystem systemToAdd) where T : struct
	{
		PlayerLoopSystem playerLoopSystem = default(PlayerLoopSystem);
		playerLoopSystem.loopConditionFunction = loopSystem.loopConditionFunction;
		playerLoopSystem.type = loopSystem.type;
		playerLoopSystem.updateDelegate = loopSystem.updateDelegate;
		playerLoopSystem.updateFunction = loopSystem.updateFunction;
		PlayerLoopSystem result = playerLoopSystem;
		Type typeFromHandle = typeof(T);
		PlayerLoopSystem[] subSystemList = loopSystem.subSystemList;
		List<PlayerLoopSystem> list = new List<PlayerLoopSystem>((subSystemList != null) ? subSystemList.Length : 0);
		PlayerLoopSystem[] subSystemList2 = loopSystem.subSystemList;
		for (int i = 0; i < subSystemList2.Length; i++)
		{
			PlayerLoopSystem item = subSystemList2[i];
			list.Add(item);
			if (item.type == typeFromHandle)
			{
				list.Add(systemToAdd);
			}
		}
		result.subSystemList = list.ToArray();
		return result;
	}

	private static PlayerLoopSystem RemoveSystem<T>(in PlayerLoopSystem loopSystem)
	{
		PlayerLoopSystem playerLoopSystem = default(PlayerLoopSystem);
		playerLoopSystem.loopConditionFunction = loopSystem.loopConditionFunction;
		playerLoopSystem.type = loopSystem.type;
		playerLoopSystem.updateDelegate = loopSystem.updateDelegate;
		playerLoopSystem.updateFunction = loopSystem.updateFunction;
		PlayerLoopSystem result = playerLoopSystem;
		if (loopSystem.subSystemList == null)
		{
			return result;
		}
		Type typeFromHandle = typeof(T);
		List<PlayerLoopSystem> list = new List<PlayerLoopSystem>();
		PlayerLoopSystem[] subSystemList = loopSystem.subSystemList;
		for (int i = 0; i < subSystemList.Length; i++)
		{
			PlayerLoopSystem item = subSystemList[i];
			if (item.type != typeFromHandle)
			{
				list.Add(item);
			}
		}
		result.subSystemList = list.ToArray();
		return result;
	}

	private static void OnEarlyUpdate()
	{
		_earlyUpdateSystem?.EarlyUpdate();
	}

	private static void OnLateUpdate()
	{
		_lateUpdateSystem?.LateUpdate();
	}
}
