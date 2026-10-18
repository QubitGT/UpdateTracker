using Meta.XR.Util;
using UnityEngine;

namespace Meta.XR.MRUtilityKit.SceneDecorator;

[Feature(Feature.Scene)]
[SingletonMonoBehaviour.InstantiationSettings(dontDestroyOnLoad = false)]
public class PoolManagerSingleton : SingletonMonoBehaviour<PoolManagerSingleton>
{
	private PoolManagerComponent poolManagerComponent;

	public PoolManager<GameObject, Pool<GameObject>> poolManager => poolManagerComponent.poolManager;

	public GameObject Create(GameObject primitive, Vector3 position, Quaternion rotation, MRUKAnchor anchor, Transform parent = null)
	{
		if (poolManagerComponent == null)
		{
			poolManagerComponent = GetComponent<PoolManagerComponent>();
		}
		return poolManagerComponent.Create(primitive, position, rotation, anchor, parent);
	}

	public GameObject Create(GameObject primitive, MRUKAnchor anchor, Transform parent = null, bool instantiateInWorldSpace = false)
	{
		return poolManagerComponent.Create(primitive, anchor, parent, instantiateInWorldSpace);
	}

	public T Create<T>(T primitive, Vector3 position, Quaternion rotation, MRUKAnchor anchor, Transform parent = null) where T : Component
	{
		return poolManagerComponent.Create(primitive, position, rotation, anchor, parent);
	}

	public T Create<T>(T primitive, MRUKAnchor anchor, Transform parent = null, bool instantiateInWorldSpace = false) where T : Component
	{
		return poolManagerComponent.Create(primitive, anchor, parent, instantiateInWorldSpace);
	}

	public void Release(GameObject go)
	{
		if (SingletonMonoBehaviour<PoolManagerSingleton>.Instance == null)
		{
			Object.Destroy(go);
		}
		else if (!(go == null))
		{
			SingletonMonoBehaviour<PoolManagerSingleton>.Instance.poolManagerComponent.Release(go);
		}
	}

	private void Start()
	{
		poolManagerComponent = GetComponent<PoolManagerComponent>();
	}
}
