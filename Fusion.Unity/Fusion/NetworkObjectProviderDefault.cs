#define FUSION_LOGLEVEL_TRACE
using System;
using UnityEngine;

namespace Fusion;

public class NetworkObjectProviderDefault : Behaviour, INetworkObjectProvider
{
	[InlineHelp]
	public bool DelayIfSceneManagerIsBusy = true;

	public virtual NetworkObjectAcquireResult AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject instance)
	{
		instance = null;
		if (DelayIfSceneManagerIsBusy && runner.SceneManager.IsBusy)
		{
			return NetworkObjectAcquireResult.Retry;
		}
		NetworkObject networkObject;
		try
		{
			networkObject = runner.Prefabs.Load(context.PrefabId, context.IsSynchronous);
		}
		catch (Exception arg)
		{
			Log.Error($"Failed to load prefab: {arg}");
			return NetworkObjectAcquireResult.Failed;
		}
		if (!networkObject)
		{
			return NetworkObjectAcquireResult.Retry;
		}
		instance = InstantiatePrefab(runner, networkObject);
		if (context.DontDestroyOnLoad)
		{
			runner.MakeDontDestroyOnLoad(instance.gameObject);
		}
		else
		{
			runner.MoveToRunnerScene(instance.gameObject);
		}
		runner.Prefabs.AddInstance(context.PrefabId);
		return NetworkObjectAcquireResult.Success;
	}

	public virtual void ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
	{
		NetworkObject @object = context.Object;
		if (!context.IsBeingDestroyed)
		{
			if (context.TypeId.IsPrefab)
			{
				DestroyPrefabInstance(runner, context.TypeId.AsPrefabId, @object);
			}
			else if (context.TypeId.IsSceneObject)
			{
				DestroySceneObject(runner, context.TypeId.AsSceneObjectId, @object);
			}
			else
			{
				if (!context.IsNestedObject)
				{
					throw new NotImplementedException($"Unknown type id {context.TypeId}");
				}
				DestroyPrefabNestedObject(runner, @object);
			}
		}
		if (context.TypeId.IsPrefab)
		{
			runner.Prefabs.RemoveInstance(context.TypeId.AsPrefabId);
		}
	}

	public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid)
	{
		return runner.Prefabs.GetId(prefabGuid);
	}

	protected virtual NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
	{
		return UnityEngine.Object.Instantiate(prefab);
	}

	protected virtual void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
	{
		UnityEngine.Object.Destroy(instance.gameObject);
	}

	protected virtual void DestroyPrefabNestedObject(NetworkRunner runner, NetworkObject instance)
	{
		UnityEngine.Object.Destroy(instance.gameObject);
	}

	protected virtual void DestroySceneObject(NetworkRunner runner, NetworkSceneObjectId sceneObjectId, NetworkObject instance)
	{
		UnityEngine.Object.Destroy(instance.gameObject);
	}
}
