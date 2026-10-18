using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Fusion;

public class FusionAddressablePrefabsPreloader : MonoBehaviour
{
	private List<AsyncOperationHandle<GameObject>> _handles = new List<AsyncOperationHandle<GameObject>>();

	private async void Start()
	{
		NetworkProjectConfig global = NetworkProjectConfig.Global;
		foreach (var entry in global.PrefabTable.GetEntries())
		{
			if (entry.Item2 is NetworkPrefabSourceAddressable { RuntimeKey: var runtimeKey })
			{
				AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(runtimeKey);
				await handle.Task;
				_handles.Add(handle);
			}
		}
	}

	private void OnDestroy()
	{
		foreach (AsyncOperationHandle<GameObject> handle in _handles)
		{
			Addressables.Release(handle);
		}
	}
}
