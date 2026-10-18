using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Meta.XR.MultiplayerBlocks.Fusion;

public class CustomNetworkObjectProvider : NetworkObjectProviderDefault
{
	private static NetworkObjectBaker _baker;

	private static readonly Dictionary<uint, Func<GameObject>> CustomSpawnDict = new Dictionary<uint, Func<GameObject>>();

	private static NetworkObjectBaker Baker => _baker ?? (_baker = new NetworkObjectBaker());

	public static void RegisterCustomNetworkObject(uint customPrefabID, Func<GameObject> func)
	{
		if (CustomSpawnDict.ContainsKey(customPrefabID))
		{
			Debug.LogError($"The requested customPrefabID {customPrefabID} already existed, aborting registration");
		}
		CustomSpawnDict[customPrefabID] = func;
	}

	public override NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
	{
		if (CustomSpawnDict.TryGetValue(context.PrefabId.RawValue, out var value))
		{
			GameObject gameObject = value();
			NetworkObject networkObject = gameObject.GetComponent<NetworkObject>();
			if (networkObject == null)
			{
				networkObject = gameObject.AddComponent<NetworkObject>();
			}
			Baker.Bake(gameObject);
			if (context.DontDestroyOnLoad)
			{
				runner.MakeDontDestroyOnLoad(gameObject);
			}
			else
			{
				runner.MoveToRunnerScene(gameObject);
			}
			result = networkObject;
			return NetworkObjectAcquireResult.Success;
		}
		return base.AcquirePrefabInstance(runner, in context, out result);
	}
}
